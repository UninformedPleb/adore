using ADORE.Configuration;
using ADORETests.Mocks;

using System.Data;
using System.Data.Common;

namespace ADORETests
{
	public class SchemaTest
	{
		private MockDatabase db;

		public SchemaTest()
		{
			// set up providerfactory
			var cr = new ConnectionRegistry() { Config = MockAdoreConfig.BasicConfig };
			cr.RegisterProviders();
			var factory = DbProviderFactories.GetFactory("mockprovider");
			db = new MockDatabase(factory, MockAdoreConfig.BasicConfig.ConnectionStrings[0].ConnectionString);
		}

		[Fact]
		public void Query_CanCreateWithoutCommandTypeWithParameters()
		{
			var query = db.CreateQuery(string.Empty, new { });
			Assert.NotNull(query);
		}
		[Fact]
		public void Query_CanCreateWithCommandTypeWithoutParameters()
		{
			var query = db.CreateQuery(string.Empty, CommandType.Text);
			Assert.NotNull(query);
		}
		[Fact]
		public void Query_CanCreateWithCommandTypeAndParameters()
		{
			var query = db.CreateQuery(string.Empty, CommandType.Text, new { });
			Assert.NotNull(query);
		}
		[Fact]
		public void StoredProcedure_CanCreate()
		{
			var sproc = db.CreateStoredProcedure(string.Empty, new { });
			Assert.NotNull(sproc);
		}

	}
}
