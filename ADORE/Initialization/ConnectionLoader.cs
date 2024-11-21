using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

using ADORE.Configuration;

namespace ADORE.Initialization
{
	public static class ConnectionLoader
	{
		private static AdoreConfig _config;
		private static Dictionary<string, ConnectionStringConfig> _connectionStrings = new Dictionary<string, ConnectionStringConfig>();

		/// <summary>
		/// <para>Registers a typed database object with the DI service collection.</para>
		/// <para>Typed database objects are a factory, so this method registers it as a singleton.</para>
		/// <para>This method requires the connection to already be configured before it is called. Be sure to use ConfigureAdore or manually use RegisterDatabaseConnection for the given connection string first.</para>
		/// </summary>
		/// <typeparam name="T">The database type, derived from ADORE.Database</typeparam>
		/// <param name="services">The DI service collection</param>
		/// <param name="key">The name of the database connection</param>
		/// <returns></returns>
		public static IServiceCollection RegisterDatabase<T>(this IServiceCollection services, string key) where T : Database
		{
			var factory = GetFactory(key);
			var connStr = _connectionStrings[key].ConnectionString;
			var db = (T)Activator.CreateInstance(typeof(T), factory, connStr);
			return services.AddSingleton<T>(db);
		}
		/// <summary>
		/// <para>Configures ADORE using the provided AdoreConfig</para>
		/// </summary>
		/// <param name="services">The DI service collection</param>
		/// <param name="config">The AdoreConfig to use</param>
		/// <returns></returns>
		public static IServiceCollection ConfigureAdore(this IServiceCollection services, AdoreConfig config)
		{
			_config = config;
			RegisterProviders(config.ProviderFactories);
			RegisterDatabaseConnections(config.ConnectionStrings);

			return services;
		}

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
			if(!_connectionStrings.ContainsKey(key)) { throw new ArgumentOutOfRangeException("key", $"Key: {key}"); }
			var pfs = _config.ProviderFactories.Where(pf => pf.ProviderName == _connectionStrings[key].ProviderName);
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
				_connectionStrings[connection.ConnectionName] = connection;
			}
		}
		/// <summary>
		/// <para>Registers a single connection from a ConnectionStringConfig</para>
		/// </summary>
		/// <param name="connection">The ConnectionStringConfig to register</param>
		public static void RegisterDatabaseConnection(ConnectionStringConfig connection)
		{
			_connectionStrings[connection.ConnectionName] = connection;
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
			_connectionStrings[key] = csc;
		}
		/// <summary>
		/// <para>Gets a named connection</para>
		/// </summary>
		/// <param name="key">The name of the connection</param>
		/// <returns>The connection corresponding to the name</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the connection name is not registered</exception>
		public static ConnectionStringConfig GetDatabaseConnection(string key)
		{
			if(!_connectionStrings.ContainsKey(key)) { throw new ArgumentOutOfRangeException("key", $"key: {key}"); }
			return _connectionStrings[key];
		}
	}
}
