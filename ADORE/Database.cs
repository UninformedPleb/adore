using System.Data;
using System.Data.Common;

namespace ADORE
{
	public abstract class Database
	{
		private string _connectionString;

		/// <summary>
		/// <para>The database's provider factory</para>
		/// </summary>
		internal DbProviderFactory Factory { get; set; }

		// ctor protected so only implementors can call it with base()
		protected Database(DbProviderFactory factory, string connectionString)
		{
			Factory = factory;
			_connectionString = connectionString;
		}

		/// <summary>
		/// <para>Gets a Query object initialized to use this Database.</para>
		/// </summary>
		/// <param name="sql">The query string.</param>
		/// <param name="parameterMap">The parameters, as an object.</param>
		/// <param name="commandType">The type of query.</param>
		/// <returns></returns>
		public Query CreateQuery(string sql, object parameterMap = null, CommandType commandType = CommandType.Text)
		{
			var q = new Query(this);
			q.Text = sql;
			q.CommandType = commandType;
			if(parameterMap is not null) { q.Parameters.MapObject(parameterMap); }
			return q;
		}
		/// <summary>
		/// <para>Gets a Query object initialized to use this Database, and with a CommandType of StoredProcedure.</para>
		/// </summary>
		/// <param name="procName">The query string.</param>
		/// <param name="parameterMap">The parameters, as an object.</param>
		/// <returns></returns>
		public Query CreateStoredProcedure(string procName, object parameterMap = null)
		{
			return CreateQuery(procName, parameterMap, CommandType.StoredProcedure);
		}
	}
}
