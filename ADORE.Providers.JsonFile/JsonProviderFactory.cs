using System.Data;
using System.Data.Common;

namespace ADORE.Providers.JsonFile
{
	public class JsonProviderFactory : DbProviderFactory
	{
		public static readonly JsonProviderFactory Instance = new JsonProviderFactory();

		private JsonProviderFactory() { }

		public override DbConnection CreateConnection() => new JsonConnection();
		public override DbConnectionStringBuilder CreateConnectionStringBuilder() => throw new NotImplementedException();
		public override DbDataAdapter CreateDataAdapter() => new JsonDataAdapter();
		public override DbParameter CreateParameter() => new JsonParameter();
		public override DbCommand CreateCommand() => new JsonCommand();
	}
}
