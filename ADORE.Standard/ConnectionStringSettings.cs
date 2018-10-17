using System;
using System.Net;

namespace ADORE.Standard
{
	public class ConnectionStringSettings
	{
		/// <summary>
		/// <para>The name of the connection string.</para>
		/// <para>Not used. This is a hold-over from XML connection string configurations.</para>
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// <para>The connection string used to define the database connection and parameters.</para>
		/// </summary>
		public string ConnectionString { get; set; }
		/// <summary>
		/// <para>The provider name that the connection manager uses to find a DbProviderFactory object to handle object instantiation for this connection string.</para>
		/// <para>Must be valid and registered with the DbProviderFactories.RegisterFactory method prior to instantiating any ConnectionManagers that use this connection string.</para>
		/// </summary>
		public string ProviderName { get; set; }
		/// <summary>
		/// <para>Network credentials used by some connection providers.</para>
		/// <para>If this value is not null, the credentials will be embedded into the connection string automatically prior to any connections being established.</para>
		/// </summary>
		public NetworkCredential Credential { get; set; }
	}
}
