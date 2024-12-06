using Microsoft.Extensions.DependencyInjection;

namespace ADORE.Configuration
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
			// build the servicecollection into a serviceprovider to get the connection registry back out of it...
			// yes, this is non-standard, but it makes the resulting usage so much smoother, while still keeping the connection registry testable
			var sp = services.BuildServiceProvider();
			var cr = sp.GetService<ConnectionRegistry>();

			// now use the connectionloader to add the DB instance
			var factory = cr.GetFactory(key);
			var connStr = cr.ConnectionStrings[key].ConnectionString;
			var db = (T)Activator.CreateInstance(typeof(T), factory, connStr);
			return services.AddSingleton(db);
		}
		/// <summary>
		/// <para>Configures ADORE using the provided AdoreConfig</para>
		/// </summary>
		/// <param name="services">The DI service collection</param>
		/// <param name="config">The AdoreConfig to use</param>
		/// <returns></returns>
		public static IServiceCollection ConfigureAdore(this IServiceCollection services, AdoreConfig config)
		{
			// add the AdoreConfig options
			services.ConfigureOptions<AdoreConfigOptions>();

			// configure and add the connection registry
			ConnectionRegistry cr = new ConnectionRegistry() { Config = config };
			cr.RegisterProviders();
			cr.RegisterDatabaseConnections();
			services.AddSingleton(cr);

			return services;
		}
	}
}
