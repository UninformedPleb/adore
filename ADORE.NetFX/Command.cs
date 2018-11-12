using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;

namespace ADORE.NetFX
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

		#region beyond here there be dragons... and reflection... and dynamic objects. BEWARE.
		/// <summary>
		/// <para>EXPERIMENTAL.</para>
		/// <para>Retrieves the query data into a list of an object of a given type (T).</para>
		/// <para>Tries to load both fields and properties matching the column names.</para>
		/// </summary>
		/// <typeparam name="T">The type to attempt to fill with data</typeparam>
		/// <returns>A list of T's, hopefully populated with the data from the results.</returns>
		public List<T> RetrieveList<T>()
		where T : new()
		{
			DataTable dt = this.RetrieveDataTable();

			Type tt = typeof(T);

			List<T> ts = new List<T>();
			T t;
			foreach(DataRow dr in dt.Rows)
			{
				t = new T();

				for(int x = 0; x < dt.Columns.Count; x++)
				{
					tt.GetField(dt.Columns[x].ColumnName)?.SetValue(t, dr[dt.Columns[x].ColumnName]);
					tt.GetProperty(dt.Columns[x].ColumnName)?.SetValue(t, dr[dt.Columns[x].ColumnName], null);
				}

				ts.Add(t);
			}
			return ts;
		}
		/// <summary>
		/// <para>EXPERIMENTAL.</para>
		/// <para>Retrieves the query data into a list of dynamic objects.</para>
		/// <para>Abandon all hope ye who enter here. REPENT! THE END IS NIGH!</para>
		/// </summary>
		/// <returns>A list of dynamic objects (specifically, ExpandoObjects).</returns>
		public List<dynamic> RetrieveDynamic()
		{
			DataTable dt = this.RetrieveDataTable();

			List<dynamic> ld = new List<dynamic>();
			dynamic d;
			foreach(DataRow dr in dt.Rows)
			{
				d = new ExpandoObject();
				var dd = d as IDictionary<string,object>;
				for(int x = 0; x< dt.Columns.Count; x++)
				{
					dd[dt.Columns[x].ColumnName] = dr[dt.Columns[x].ColumnName];
				}
				ld.Add(d);
			}
			return ld;
		}
		#endregion
	}
}
