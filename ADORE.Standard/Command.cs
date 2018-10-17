using System;
using System.Data;
using System.Data.Common;

namespace ADORE.Standard
{
	public class Command : IDisposable
	{
		/// <summary>
		/// <para>The connection manager that handles all connection and provider-factory details for this command</para>
		/// </summary>
		internal ConnectionManager ConnectionManager { get; set; }
		/// <summary>
		/// <para>The underlying DbCommand used by this command. (This command object is a wrapper.)</para>
		/// </summary>
		internal DbCommand DbCommand { get; set; }
		/// <summary>
		/// <para>The parameters to be sent with this command.</para>
		/// </summary>
		public CommandParameterCollection Parameters { get; private set; }
		/// <summary>
		/// <para>The command timeout, in seconds, used by this command.</para>
		/// </summary>
		public int Timeout
		{
			get { return this.DbCommand.CommandTimeout; }
			set { this.DbCommand.CommandTimeout = value; }
		}

		internal Command()
		{
			this.Parameters = new CommandParameterCollection();
		}

		#region IDisposable
		~Command()
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
				this.DbCommand?.Dispose();
			}
		}
		#endregion

		#region param mappings
		/// <summary>
		/// <para>Maps values from pseudo-parameters (CommandParameters) into native ADO DbParameters.</para>
		/// <para>This is done because DbParameter is abstract and cannot be instantiated without a factory instance, while CommandParameters must be done without a known factory instance.</para>
		/// </summary>
		private void MapToDBP()
		{
			this.DbCommand.Parameters.Clear();
			DbParameter param;
			for(int x = 0; x < this.Parameters.Count; x++)
			{
				param = this.DbCommand.CreateParameter();
				param.ParameterName = this.Parameters[x].Name;
				param.Value = this.Parameters[x].Value;
				param.DbType = this.Parameters[x].Type;
				param.Direction = this.Parameters[x].Direction;
				this.DbCommand.Parameters.Add(param);
			}
		}
		/// <summary>
		/// <para>Maps values from output, input-output, and return directional native ADO DbParameters back into the pseudo-parameter collection.</para>
		/// </summary>
		private void MapFromDBP()
		{
			for(int x = 0; x < this.DbCommand.Parameters.Count; x++)
			{
				if(this.Parameters[this.DbCommand.Parameters[x].ParameterName] != null && this.Parameters[this.DbCommand.Parameters[x].ParameterName].Direction != ParameterDirection.Input)
				{
					this.Parameters[this.DbCommand.Parameters[x].ParameterName].Value = this.DbCommand.Parameters[x].Value;
				}
			}
		}
		#endregion

		/// <summary>
		/// <para>Executes the command's query and fills a DataSet with the results.</para>
		/// </summary>
		/// <returns>All resultsets returned by the query, contained in a DataSet.</returns>
		public DataSet RetrieveDataSet()
		{
			DataSet ds = null;
			try
			{
				this.MapToDBP();
				this.DbCommand.Connection = this.ConnectionManager.Connect();
				ds = new DataSet();
				using(DbDataAdapter dda = this.ConnectionManager.CreateDataAdapter())
				{
					dda.SelectCommand = this.DbCommand;
					dda.Fill(ds);
				}
				this.MapFromDBP();
			}
			finally
			{
				this.ConnectionManager.Disconnect(this.DbCommand);
			}
			return ds;
		}
		/// <summary>
		/// <para>Executes the command's query and fills a DataTable with the results in the first resultset returned by the query.</para>
		/// </summary>
		/// <returns>The first resultset returned by the query, contained in a DataTable.</returns>
		public DataTable RetrieveDataTable()
		{
			DataTable dt = null;
			try
			{
				this.MapToDBP();
				this.DbCommand.Connection = this.ConnectionManager.Connect();
				dt = new DataTable();
				using(DbDataAdapter dda = this.ConnectionManager.CreateDataAdapter())
				{
					dda.SelectCommand = this.DbCommand;
					dda.Fill(dt);
				}
				this.MapFromDBP();
			}
			finally
			{
				this.ConnectionManager.Disconnect(this.DbCommand);
			}
			return dt;
		}
		/// <summary>
		/// <para>Executes the command's query and returns the first resultset's first row's first column value as a scalar value.</para>
		/// </summary>
		/// <returns>The first field value returned by the query.</returns>
		public object ExecuteScalar()
		{
			object o = null;
			try
			{
				this.MapToDBP();
				this.DbCommand.Connection = this.ConnectionManager.Connect();
				o = this.DbCommand.ExecuteScalar();
				this.MapFromDBP();
			}
			finally
			{
				this.ConnectionManager.Disconnect(this.DbCommand);
			}
			return o;
		}
		/// <summary>
		/// <para>Executes the command's query and returns no results.</para>
		/// </summary>
		public void Execute()
		{
			try
			{
				this.MapToDBP();
				this.DbCommand.Connection = this.ConnectionManager.Connect();
				this.DbCommand.ExecuteNonQuery();
				this.MapFromDBP();
			}
			finally
			{
				this.ConnectionManager.Disconnect(this.DbCommand);
			}
		}
	}
}
