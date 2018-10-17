using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace ADORE.Standard
{
	public static class DbProviderFactories
	{
		private static Dictionary<string,Type> _providers = new Dictionary<string, Type>();

		/// <summary>
		/// <para>Registers a DbProviderFactory implementation with a given provider name.</para>
		/// </summary>
		/// <param name="providerName">The name to register this provider with</param>
		/// <param name="dbProviderFactoryType">The DbProviderFactory class name being registered</param>
		/// <exception cref="ArgumentNullException">Thrown if dbProviderFactoryType is null</exception>
		public static void RegisterFactory(string providerName, Type dbProviderFactoryType)
		{
			_providers[providerName] = dbProviderFactoryType ?? throw new ArgumentNullException("dbProviderFactoryType");
		}
		/// <summary>
		/// <para>Gets a registered factory for a given provider name.</para>
		/// </summary>
		/// <param name="providerName">The provider name registered and mapped to a DbProviderFactory type</param>
		/// <returns>The DbProviderFactory type registered and mapped to the provider name</returns>
		/// <exception cref="InvalidOperationException">Thrown if provider name is not registered, if registered DbProviderFactory is null, or if registered DbProviderFactory is not derived from System.Data.Common.DbProviderFactory</exception>
		internal static DbProviderFactory GetFactory(string providerName)
		{
			if(!_providers.ContainsKey(providerName)) { throw new InvalidOperationException(string.Format("Factory for provider {0} is not registered.", providerName)); }
			if(_providers[providerName] == null) { throw new InvalidOperationException(string.Format("Factory for provider {0} is not valid.", providerName)); }
			if(!_providers[providerName].IsSubclassOf(typeof(DbProviderFactories))) { throw new InvalidOperationException(string.Format("Factory registered for provider {0} is not a System.Data.Common.DbProviderFactory.", providerName)); }
			return (DbProviderFactory)Activator.CreateInstance(_providers[providerName]);
		}
	}
}
