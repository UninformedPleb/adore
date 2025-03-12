using System.Data;

namespace ADORE.ORM
{
	public interface IQueryCreator
	{
		Query CreateQuery(string querytext, params object[] parameterMap);
		Query CreateQuery(string querytext, CommandType commandType, params object[] parameterMap);
		Query CreateStoredProcedure(string procName, params object[] parameterMap);
	}
}
