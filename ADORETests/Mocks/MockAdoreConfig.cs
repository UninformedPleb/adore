using ADORE.Configuration;

namespace ADORETests.Mocks
{
	public class MockAdoreConfig
	{
		public static readonly AdoreConfig BasicConfig = new AdoreConfig()
		{
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
	}
}
