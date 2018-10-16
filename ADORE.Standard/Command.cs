using System;
using System.Data;
using System.Data.Common;

namespace ADORE.Standard
{
	public class Command : IDisposable
	{
		internal ConnectionManager ConnectionManager { get; set; }
		internal DbCommand DbCommand { get; set; }
		public CommandParameterCollection Parameters { get; private set; }
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
