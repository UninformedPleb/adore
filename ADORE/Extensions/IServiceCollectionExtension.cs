using ADORE.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ADORE.Extensions
{
	public static class IServiceCollectionExtension
	{
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
			var factory = ConnectionLoader.GetFactory(key);
			var connStr = ConnectionLoader.ConnectionStrings[key].ConnectionString;
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
			ConnectionLoader.Config = config;

			services.ConfigureOptions<AdoreConfigSetup>();
			ConnectionLoader.RegisterProviders(config.ProviderFactories);
			ConnectionLoader.RegisterDatabaseConnections(config.ConnectionStrings);

			return services;
		}
	}
}
