using System.Data.Common;

namespace ADORETests.Mocks
{
	internal class MockDbProviderFactory : DbProviderFactory
	{
		public static MockDbProviderFactory Instance = new MockDbProviderFactory();

		public override DbConnection CreateConnection()
		{
			return new MockDbConnection();
		}
		public override DbCommand CreateCommand()
		{
			return new MockDbCommand();
		}
		public override DbDataAdapter CreateDataAdapter()
		{
			return new MockDbDataAdapter();
		}
		public override DbParameter CreateParameter()
		{
			return new MockDbParameter();
		}
	}
}
