using System.Data.Common;

namespace ADORE.Configuration
{
	public static class ConnectionLoader
	{
		internal static AdoreConfig Config { get; set; }
		internal static Dictionary<string, ConnectionStringConfig> ConnectionStrings { get; set; } = new Dictionary<string, ConnectionStringConfig>();

		/// <summary>
		/// <para>Registers multiple providers from ProviderFactoryConfigs</para>
		/// </summary>
		/// <param name="providers">The list of ProviderFactoryConfigs to register</param>
		public static void RegisterProviders(IEnumerable<ProviderFactoryConfig> providers)
		{
			foreach(var provider in providers)
			{
				DbProviderFactories.RegisterFactory(provider.ProviderName, provider.FactoryTypeName);
			}
		}
		/// <summary>
		/// <para>Gets a provider factory for a given connection.</para>
		/// </summary>
		/// <param name="key">The connection name</param>
		/// <returns>A DbProviderFactory object corresponding to the provider named in the specified connection</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the connection name is not registered</exception>
		/// <exception cref="ArgumentException">Thrown if the connection is found, but the provider configured on that connection is not registered.</exception>
		public static DbProviderFactory GetFactory(string key)
		{
			if(!ConnectionStrings.ContainsKey(key)) { throw new ArgumentOutOfRangeException("key", $"Key: {key}"); }
			var pfs = Config.ProviderFactories.Where(pf => pf.ProviderName == ConnectionStrings[key].ProviderName);
			if(!pfs.Any()) { throw new ArgumentException(); }
			return DbProviderFactories.GetFactory(pfs.First().ProviderName);
		}

		/// <summary>
		/// <para>Registers multiple connections from ConnectionStringConfigs</para>
		/// </summary>
		/// <param name="connections">The list of ConnectionStringConfigs to register</param>
		public static void RegisterDatabaseConnections(IEnumerable<ConnectionStringConfig> connections)
		{
			foreach(var connection in connections)
			{
				ConnectionStrings[connection.ConnectionName] = connection;
			}
		}
		/// <summary>
		/// <para>Registers a single connection from a ConnectionStringConfig</para>
		/// </summary>
		/// <param name="connection">The ConnectionStringConfig to register</param>
		public static void RegisterDatabaseConnection(ConnectionStringConfig connection)
		{
			ConnectionStrings[connection.ConnectionName] = connection;
		}
		/// <summary>
		/// <para>Registers a connection directly, without the need for an AdoreConfig</para>
		/// </summary>
		/// <param name="key">The name of the connection</param>
		/// <param name="providerName">The name of the provider</param>
		/// <param name="connectionString">The connection string</param>
		public static void RegisterDatabaseConnection(string key, string providerName, string connectionString)
		{
			var csc = ConnectionStringConfig.Parse(connectionString);
			csc.ConnectionName = key;
			csc.ProviderName = providerName;
			ConnectionStrings[key] = csc;
		}
		/// <summary>
		/// <para>Gets a named connection</para>
		/// </summary>
		/// <param name="key">The name of the connection</param>
		/// <returns>The connection corresponding to the name</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the connection name is not registered</exception>
		public static ConnectionStringConfig GetDatabaseConnection(string key)
		{
			if(!ConnectionStrings.ContainsKey(key)) { throw new ArgumentOutOfRangeException("key", $"key: {key}"); }
			return ConnectionStrings[key];
		}
	}
}
