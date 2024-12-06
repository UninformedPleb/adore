using ADORE.Configuration;

namespace ADORETests.Configuration
{
	public class ConnectionStringConfigTest
	{
		private readonly ConnectionStringConfig config = new (){
			ConnectionName = "test1",
			ProviderName = "testprovider",
			ConnectionStringValues = new Dictionary<string, string>() {
				{"Database", "foo"},
				{"User ID", "bar"},
				{"Password", "baz"},
				{"Encryption", "Optional"},
			}
		};
		private readonly string connectionStringMatch = "Database=foo;User ID=bar;Password=baz;Encryption=Optional";
		private readonly string catalogNameMatch = "foo";

		[Fact]
		public void ConnectionString_ShouldMatch() => Assert.Equal(connectionStringMatch, config.ConnectionString);
		[Fact]
		public void CatalogName_ShouldMatch() => Assert.Equal(catalogNameMatch, config.CatalogName);
		[Fact]
		public void ConnectionString_ShouldParse()
		{
			var parsed = ConnectionStringConfig.Parse(connectionStringMatch);
			foreach(var kvp in config.ConnectionStringValues)
			{
				Assert.Equal(kvp.Value, parsed.ConnectionStringValues[kvp.Key]);
			}
		}
	}
}
