using System.Data;
using System.Data.Common;

namespace ADORE
{
	/// <summary>
	/// <para>Represents a database connection and all of the programmatic means of using it.</para>
	/// <para>Use of this class requires inheriting it and implementing it with the components of the database it is representing.</para>
	/// <para>To use a ready-made database-agnostic implementation of this class, use AdHocDatabase.</para>
	/// </summary>
	public abstract class Database
	{
		private string _connectionString;

		/// <summary>
		/// <para>The database's provider factory</para>
		/// </summary>
		internal DbProviderFactory Factory { get; private set; }

		// ctor protected so only implementors can call it with base()
		protected Database(DbProviderFactory factory, string connectionString)
		{
			Factory = factory;
			_connectionString = connectionString;
		}

		internal virtual DbConnection CreateConnection()
		{
			var conn = Factory.CreateConnection();
			conn.ConnectionString = _connectionString;
			return conn;
		}

		/// <summary>
		/// <para>Gets a Query object initialized to use this Database.</para>
		/// </summary>
		/// <param name="sql">The query string</param>
		/// <param name="parameterMap">The parameters, as an array of objects</param>
		/// <returns></returns>
		public Query CreateQuery(string sql, params object[] parameterMap)
		{
			return CreateQuery(sql, CommandType.Text, parameterMap);
		}
		/// <summary>
		/// <para>Gets a Query object initialized to use this Database.</para>
		/// </summary>
		/// <param name="sql">The query string.</param>
		/// <param name="commandType">The type of query.</param>
		/// <param name="parameterMap">The parameters, as an array of objects</param>
		/// <returns></returns>
		public Query CreateQuery(string sql, CommandType commandType = CommandType.Text, params object[] parameterMap)
		{
			var q = new Query(this);
			q.Text = sql;
			q.CommandType = commandType;
			if(parameterMap is not null)
			{
				for(int x = 0; x < parameterMap.Length; x++)
				{
					if(parameterMap[x] is null) { continue; }
					q.Parameters.MapObject(parameterMap[x]);
				}
			}
			return q;
		}
		/// <summary>
		/// <para>Gets a Query object initialized to use this Database, and with a CommandType of StoredProcedure.</para>
		/// </summary>
		/// <param name="procName">The query string.</param>
		/// <param name="parameterMap">The parameters, as an array of objects</param>
		/// <returns></returns>
		public Query CreateStoredProcedure(string procName, params object[] parameterMap)
		{
			return CreateQuery(procName, CommandType.StoredProcedure, parameterMap);
		}
	}
}
