using System.Collections.Generic;

namespace ADORE.Configuration
{
    public class AdoreConfig
	{
		public List<ProviderFactoryConfig> ProviderFactories { get; set; }
		public List<ConnectionStringConfig> ConnectionStrings { get; set; }
	}
}
