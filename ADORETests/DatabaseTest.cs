using ADORE.Configuration;

using ADORETests.Mocks;

using System.Data;
using System.Data.Common;

namespace ADORETests
{
	public class DatabaseTest
	{
		private DbProviderFactory factory;

		public DatabaseTest()
		{
			// set up providerfactory
			var cr = new ConnectionRegistry() { Config = MockAdoreConfig.BasicConfig };
			cr.RegisterProviders();
			factory = DbProviderFactories.GetFactory("mockprovider");
		}

		[Fact]
		public void Query_CanCreateWithoutCommandTypeWithParameters()
		{
			var db = new MockDatabase(factory, MockAdoreConfig.BasicConfig.ConnectionStrings[0].ConnectionString);
			var query = db.CreateQuery(string.Empty, new {});
			Assert.NotNull(query);
		}
		[Fact]
		public void Query_CanCreateWithCommandTypeWithoutParameters()
		{
			var db = new MockDatabase(factory, MockAdoreConfig.BasicConfig.ConnectionStrings[0].ConnectionString);
			var query = db.CreateQuery(string.Empty, CommandType.Text);
			Assert.NotNull(query);
		}
		[Fact]
		public void Query_CanCreateWithCommandTypeAndParameters()
		{
			var db = new MockDatabase(factory, MockAdoreConfig.BasicConfig.ConnectionStrings[0].ConnectionString);
			var query = db.CreateQuery(string.Empty, CommandType.Text, new {});
			Assert.NotNull(query);
		}
		[Fact]
		public void StoredProcedure_CanCreate()
		{
			var db = new MockDatabase(factory, MockAdoreConfig.BasicConfig.ConnectionStrings[0].ConnectionString);
			var sproc = db.CreateStoredProcedure(string.Empty, new {});
			Assert.NotNull(sproc);
		}
	}
}
