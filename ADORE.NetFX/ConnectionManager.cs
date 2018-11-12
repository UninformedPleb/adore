using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace ADORE.NetFX
{
	public class ConnectionManager : IDisposable
	{
		private DbProviderFactory _factory = null;
		private ConnectionStringSettings _connectionString = null;
		private DbTransaction _transaction = null;

		public ConnectionManager(ConnectionStringSettings settings)
		{
			this._connectionString = settings;
			if(string.IsNullOrEmpty(this._connectionString.ProviderName)) { this._connectionString.ProviderName = "System.Data.SqlClient"; }
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
		/// <summary>
		/// <para>Creates an ad-hoc query to be run against the connection managed by this ConnectionManager.</para>
		/// </summary>
		/// <param name="queryString">The query string to execute</param>
		/// <returns>A query object</returns>
		public Command CreateQuery(string queryString)
		{
			Command c = new Command();
			c.DbCommand = this._factory.CreateCommand();
			c.DbCommand.CommandText = queryString;
			c.DbCommand.CommandType = CommandType.Text;
			c.ConnectionManager = this;

			return c;
		}
		/// <summary>
		/// <para>Creates a stored procedure query to be run against the connection managed by this ConnectionManager.</para>
		/// <para>This is equivalent to using the SQL 'EXEC' command.</para>
		/// </summary>
		/// <param name="storedProcName">The name of a stored procedure</param>
		/// <returns>A query object</returns>
		public Command CreateStoredProcedure(string storedProcName)
		{
			Command c = new Command();
			c.DbCommand = this._factory.CreateCommand();
			c.DbCommand.CommandText = storedProcName;
			c.DbCommand.CommandType = CommandType.StoredProcedure;
			c.ConnectionManager = this;

			return c;
		}
		/// <summary>
		/// <para>Creates a data adapter on this connection. Used internally to populate DataSet and DataTable objects.</para>
		/// </summary>
		/// <returns>A data adapter from the DbProviderFactory</returns>
		internal DbDataAdapter CreateDataAdapter()
		{
			return this._factory.CreateDataAdapter();
		}
		#endregion

		#region transaction stuff
		/// <summary>
		/// <para>Opens and holds a connection and begins a transaction on it.</para>
		/// <para>Connection will stay open until either Commit() or Rollback() are called.</para>
		/// </summary>
		public void BeginTransaction()
		{
			if(this._transaction != null) { this.Rollback(); }
			DbConnection cxn = this.Connect();
			this._transaction = cxn.BeginTransaction();
		}
		/// <summary>
		/// <para>If a transaction is active, this commits the changes and ends the transaction.</para>
		/// <para>If no transaction is active, no action is taken.</para>
		/// </summary>
		public void Commit()
		{
			if(this._transaction != null)
			{
				this._transaction.Commit();
				this._transaction.Dispose();
				this._transaction = null;
			}
		}
		/// <summary>
		/// <para>If a transaction is active, this rolls back the changes and ends the transaction.</para>
		/// <para>If no transaction is active, no action is taken.</para>
		/// </summary>
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
		/// <summary>
		/// <para>Creates a connection from the DbProviderFactory, then opens it.</para>
		/// <para>If a transaction is active, the already-open connection belonging to the transaction is returned instead.</para>
		/// </summary>
		/// <returns>An open database connection, ready for use</returns>
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
		/// <summary>
		/// <para>Closes the active connection on a given command object.</para>
		/// <para>If a transaction is active, the connection itself is not closed, but the command object's reference to the active transaction's connection is severed to prevent it from closing when the DbCommand is garbage-collected.</para>
		/// </summary>
		/// <param name="cmd"></param>
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
