using System;
using System.Data;
using System.Data.Common;

namespace ADORE.Standard
{
	public class ConnectionManager : IDisposable
	{
		private DbProviderFactory _factory = null;
		private ConnectionStringSettings _connectionString = null;
		private DbTransaction _transaction = null;

		public ConnectionManager(ConnectionStringSettings settings)
		{
			this._connectionString = settings;
			this._factory = DbProviderFactories.GetFactory(this._connectionString.ProviderName);
		}

		#region IDisposable
		~ConnectionManager()
		{
			this.Dispose(false);
		}
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
				this.Rollback();
			}
		}
		#endregion

		#region command factory
		public Command CreateQuery(string queryString)
		{
			Command c = new Command();
			c.DbCommand = this._factory.CreateCommand();
			c.DbCommand.CommandText = queryString;
			c.DbCommand.CommandType = CommandType.Text;
			c.ConnectionManager = this;

			return c;
		}
		public Command CreateStoredProcedure(string storedProcName)
		{
			Command c = new Command();
			c.DbCommand = this._factory.CreateCommand();
			c.DbCommand.CommandText = storedProcName;
			c.DbCommand.CommandType = CommandType.StoredProcedure;
			c.ConnectionManager = this;

			return c;
		}
		internal DbDataAdapter CreateDataAdapter()
		{
			return this._factory.CreateDataAdapter();
		}
		#endregion

		#region transaction stuff
		public void BeginTransaction()
		{
			if(this._transaction != null) { this.Rollback(); }
			DbConnection cxn = this.Connect();
			this._transaction = cxn.BeginTransaction();
		}
		public void Commit()
		{
			if(this._transaction != null)
			{
				this._transaction.Commit();
				this._transaction.Dispose();
				this._transaction = null;
			}
		}
		public void Rollback()
		{
			if(this._transaction != null)
			{
				this._transaction.Rollback();
				this._transaction.Dispose();
				this._transaction = null;
			}
		}
		#endregion

		#region connection management
		public DbConnection Connect()
		{
			if(this._transaction == null)
			{
				DbConnection cxn = this._factory.CreateConnection();
				cxn.ConnectionString = this._connectionString.ConnectionString;
				cxn.Open();

				return cxn;
			}
			else
			{
				return this._transaction.Connection;
			}
		}
		public void Disconnect(DbCommand cmd)
		{
			if(this._transaction == null)
			{
				cmd.Connection.Close();
			}
			cmd.Connection = null;
		}
		#endregion
	}
}
