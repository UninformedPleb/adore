using System.Data.Common;

namespace ADORE.Configuration
{
	/// <summary>
	/// <para>Represents a registry of provider and connection configuration data, indexed by name, ready to use on demand.</para>
	/// <para>This is initialized with values from an AdoreConfig object.</para>
	/// <para>Barring that, this object can also be initialized manually with RegisterProviders and RegisterConnection(s) methods if the values are obtainable via some other means. This is good for old XML configuration files, CLI-driven configurations, or other sources of data connection information.</para>
	/// </summary>
	public class ConnectionRegistry
	{
		/// <summary>
		/// <para>The configuration object containing all of the provider and connection settings</para>
		/// </summary>
		public AdoreConfig Config { internal get; init; }
		internal Dictionary<string, ConnectionStringConfig> ConnectionStrings { get; set; } = new Dictionary<string, ConnectionStringConfig>();

		/// <summary>
		/// <para>Registers multiple providers from ProviderFactoryConfigs</para>
		/// </summary>
		/// <param name="providers">The list of ProviderFactoryConfigs to register</param>
		public void RegisterProviders(IEnumerable<ProviderFactoryConfig> providers = null)
		{
			foreach(var provider in providers ?? Config.ProviderFactories)
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
		public DbProviderFactory GetFactory(string key)
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
		public void RegisterDatabaseConnections(IEnumerable<ConnectionStringConfig> connections = null)
		{
			foreach(var connection in connections ?? Config.ConnectionStrings)
			{
				ConnectionStrings[connection.ConnectionName] = connection;
			}
		}
		/// <summary>
		/// <para>Registers a single connection from a ConnectionStringConfig</para>
		/// </summary>
		/// <param name="connection">The ConnectionStringConfig to register</param>
		public void RegisterDatabaseConnection(ConnectionStringConfig connection)
		{
			ConnectionStrings[connection.ConnectionName] = connection;
		}
		/// <summary>
		/// <para>Registers a connection directly, without the need for an AdoreConfig</para>
		/// </summary>
		/// <param name="key">The name of the connection</param>
		/// <param name="providerName">The name of the provider</param>
		/// <param name="connectionString">The connection string</param>
		public void RegisterDatabaseConnection(string key, string providerName, string connectionString)
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
		public ConnectionStringConfig GetDatabaseConnection(string key)
		{
			if(!ConnectionStrings.ContainsKey(key)) { throw new ArgumentOutOfRangeException("key", $"key: {key}"); }
			return ConnectionStrings[key];
		}
	}
}
