using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace ADORE.Standard
{
	public static class DbProviderFactories
	{
		private static Dictionary<string,Type> _providers = new Dictionary<string, Type>();

		public static void RegisterFactory(string providerName, Type dbProviderFactoryType)
		{
			_providers[providerName] = dbProviderFactoryType ?? throw new ArgumentNullException("dbProviderFactoryType");
		}
		internal static DbProviderFactory GetFactory(string providerName)
		{
			if(!_providers.ContainsKey(providerName)) { throw new InvalidOperationException(string.Format("Factory for provider {0} is not registered.", providerName)); }
			if(_providers[providerName] == null) { throw new InvalidOperationException(string.Format("Factory for provider {0} is not valid.", providerName)); }
			if(!_providers[providerName].IsSubclassOf(typeof(DbProviderFactories))) { throw new InvalidOperationException(string.Format("Factory registered for provider {0} is not a System.Data.Common.DbProviderFactory.", providerName)); }
			return (DbProviderFactory)Activator.CreateInstance(_providers[providerName]);
		}
	}
}
