using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

using ADORE.Configuration;

namespace ADORE.Initialization
{
	public static class ConnectionLoader
	{
		private static AdoreConfig _config;
		private static Dictionary<string, ConnectionStringConfig> _connectionStrings = new Dictionary<string, ConnectionStringConfig>();

		public static IServiceCollection RegisterDatabase<T>(this IServiceCollection services, string key) where T : Database
		{
			var factory = GetFactory(key);
			var connStr = _connectionStrings[key].ConnectionString;
			var db = (T)Activator.CreateInstance(typeof(T), factory, connStr);
			return services.AddSingleton<T>(db);
		}
		public static IServiceCollection ConfigureAdore(this IServiceCollection services, AdoreConfig config)
		{
			_config = config;
			RegisterProviders(config.ProviderFactories);

			return services;
		}

		private static void RegisterProviders(IEnumerable<ProviderFactoryConfig> providers)
		{
			foreach(var provider in providers)
			{
				DbProviderFactories.RegisterFactory(provider.ProviderName, provider.FactoryTypeName);
			}
		}
		public static DbProviderFactory GetFactory(string key)
		{
			if(!_connectionStrings.ContainsKey(key)) { throw new ArgumentOutOfRangeException("key", $"Key: {key}"); }
			var pfs = _config.ProviderFactories.Where(pf => pf.ProviderName == _connectionStrings[key].ProviderName);
			if(!pfs.Any()) { throw new ArgumentException(); }
			return DbProviderFactories.GetFactory(pfs.First().ProviderName);
		}
	}
}
