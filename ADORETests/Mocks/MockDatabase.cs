using ADORE;
using System.Data.Common;

namespace ADORETests.Mocks
{
	internal class MockDatabase : Database
	{
		public MockSchema mock;

		public MockDatabase(DbProviderFactory factory, string connectionString) : base(factory, connectionString)
		{
			mock = new MockSchema(this);
		}
	}
}
