using System.Collections.Generic;

namespace ADORE.Configuration
{
	/// <summary>
	/// Represents a configuration structure to initialize ADORE.
	/// </summary>
    public class AdoreConfig
	{
		/// <summary>
		/// <para>A list of provider factories</para>
		/// </summary>
		public List<ProviderFactoryConfig> ProviderFactories { get; set; }
		/// <summary>
		/// <para>A list of connection strings</para>
		/// </summary>
		public List<ConnectionStringConfig> ConnectionStrings { get; set; }
	}
}
