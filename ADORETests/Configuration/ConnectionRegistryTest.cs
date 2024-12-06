using System.Data.Common;

using ADORE.Configuration;

using ADORETests.Mocks;

namespace ADORETests.Configuration
{
	public class ConnectionRegistryTest
	{
		private readonly AdoreConfig config = new AdoreConfig() {
			ProviderFactories = new List<ProviderFactoryConfig>() {
				new ProviderFactoryConfig() {
					ProviderName = "mockprovider",
					FactoryTypeName = typeof(MockDbProviderFactory).AssemblyQualifiedName
				},
			},
			ConnectionStrings = new List<ConnectionStringConfig>() {
				new ConnectionStringConfig() {
					ConnectionName = "mockconnection",
					ProviderName = "mockprovider",
					ConnectionStringValues = new Dictionary<string, string>() {
						{"Database", "foo"},
						{"User ID", "bar"},
						{"Password", "baz"},
						{"Encryption", "Optional"},
					}
				},
			},
		};
		private readonly string connectionStringMatch = "Database=foo;User ID=bar;Password=baz;Encryption=Optional";

		[Fact]
		public void Providers_RegisteredCorrectly()
		{
			var cr = new ConnectionRegistry() { Config = config };
			cr.RegisterProviders();
			var factory = DbProviderFactories.GetFactory("mockprovider");
			Assert.Equal(factory.GetType().AssemblyQualifiedName, typeof(MockDbProviderFactory).AssemblyQualifiedName);
		}
		[Fact]
		public void Connections_RegisteredCorrectly()
		{
			var cr = new ConnectionRegistry() { Config = config };
			cr.RegisterProviders();
			cr.RegisterDatabaseConnections();
			var csc = cr.GetDatabaseConnection("mockconnection");
			Assert.Equal(csc.ConnectionString, connectionStringMatch);
		}
	}
}
