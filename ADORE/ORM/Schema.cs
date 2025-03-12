using System.Data;

namespace ADORE.ORM
{
	/// <summary>
	/// <para>Represents a database schema.</para>
	/// <para>Use of this class requires inheriting it and implementing it to mimic a database schema's structure.</para>
	/// <para>Some database providers, such as SQLite, have no concept of a schema and make little sense to implement this way.</para>
	/// <para>Implementing this class is entirely optional. It merely provides some syntactic sugar for your code.</para>
	/// </summary>
	public abstract class Schema : IQueryCreator
	{
		private Database _parent;

		protected abstract string Name { get; }

		protected Schema(Database parent)
		{
			_parent = parent;
		}

		public virtual Query CreateQuery(string sql, params object[] parameterMap)
		{
			return _parent.CreateQuery(sql, parameterMap);
		}
		public virtual Query CreateQuery(string sql, CommandType commandType = CommandType.Text, params object[] parameterMap)
		{
			return _parent.CreateQuery(sql, commandType, parameterMap);
		}
		public virtual Query CreateStoredProcedure(string procName, params object[] parameterMap)
		{
			string[] parts = procName.Split('.');
			if(parts.Length == 1) { procName = $"{Name}.{procName}"; }
			return _parent.CreateStoredProcedure(procName, parameterMap);
		}
	}
}
