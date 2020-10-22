using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace ADORE.Standard
{
	public static class DbProviderFactories
	{
		private static Dictionary<string, DbProviderFactory> _providers = new Dictionary<string, DbProviderFactory>();

		/// <summary>
		/// <para>Registers a DbProviderFactory implementation with a given provider name. Assumes that the factory is NOT implemented as a singleton instance.</para>
		/// </summary>
		/// <param name="providerName">The name to register this provider with</param>
		/// <param name="dbProviderFactoryType">The DbProviderFactory class name being registered</param>
		/// <exception cref="ArgumentNullException">Thrown if dbProviderFactoryType is null</exception>
		public static void RegisterFactory(string providerName, Type dbProviderFactoryType)
		{
			if(_providers.ContainsKey(providerName)) { throw new InvalidOperationException(string.Format("Factory for provider {0} is already registered.", providerName)); }
			if(string.IsNullOrEmpty(providerName)) { throw new InvalidOperationException(string.Format("Factory for provider {0} is not valid.", providerName)); }
			if(!dbProviderFactoryType.IsSubclassOf(typeof(DbProviderFactories))) { throw new InvalidOperationException(string.Format("{0} is not a System.Data.Common.DbProviderFactory.", dbProviderFactoryType.FullName)); }

			_providers[providerName] = (DbProviderFactory)Activator.CreateInstance(dbProviderFactoryType);
		}
		/// <summary>
		/// <para>Registers a DbProviderFactory implementation with a given provider name. Assumes that the factory is NOT implemented as a singleton instance.</para>
		/// </summary>
		/// <param name="providerName">The name to register thisprovider with</param>
		/// <param name="singletonInstance">The instance of a DbProviderFactory class being registered</param>
		/// <exception cref="ArgumentNullException">Thrown if dbProviderFactoryType is null</exception>
		public static void RegisterFactory(string providerName, DbProviderFactory singletonInstance)
		{
			if(_providers.ContainsKey(providerName)) { throw new InvalidOperationException(string.Format("Factory for provider {0} is already registered.", providerName)); }
			if(string.IsNullOrEmpty(providerName)) { throw new InvalidOperationException(string.Format("Factory for provider {0} is not valid.", providerName)); }
			if(singletonInstance == null) { throw new InvalidOperationException(string.Format("A null instance was provided and cannot be registered.")); }
			_providers[providerName] = singletonInstance;
		}
		/// <summary>
		/// <para>Removes a registered factory provider.</para>
		/// </summary>
		/// <param name="providerName"></param>
		public static void UnregisterFactory(string providerName)
		{
			if(_providers.ContainsKey(providerName))
			{
				_providers.Remove(providerName);
			}
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
			return _providers[providerName];
		}
	}
}
