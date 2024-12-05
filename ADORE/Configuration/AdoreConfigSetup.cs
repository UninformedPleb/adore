using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ADORE.Configuration
{
	public class AdoreConfigSetup : IConfigureOptions<AdoreConfig>
	{
		private static readonly string _sectionName = "ADORE";

		private readonly IConfiguration _configuration;

		public AdoreConfigSetup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void Configure(AdoreConfig adoreConfig)
		{
			_configuration.GetSection(_sectionName).Bind(adoreConfig);
		}
	}
}
