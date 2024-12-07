namespace ADORE.Configuration
{
	/// <summary>
	/// <para>Represents a provider factory that can be registered during ADORE initialization.</para>
	/// </summary>
	public class ProviderFactoryConfig
	{
		/// <summary>
		/// <para>The name of the provider.</para>
		/// <para>This serves no purpose beyond being an identifier for this provider within the context of your software. It should be unique within your software's set of provider names.</para>
		/// <para>It does not have to follow any special conventions or match any pre-determined values.</para>
		/// </summary>
		public string ProviderName { get; set; }
		/// <summary>
		/// <para>The assembly-qualified name of the DbProviderFactory impelementation for this database provider's ADO.NET library.</para>
		/// <para>Examples: "Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient" for Microsoft SQL Server or "Npgsql.NpgsqlFactory, Npgsql" for PostgreSQL</para>
		/// </summary>
		public string FactoryTypeName { get; set; }
	}
}
