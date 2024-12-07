using System.Data.Common;

namespace ADORE
{
	/// <summary>
	/// <para>Represents any database, but with no special configurations applied or possible beyond what the connection string allows.</para>
	/// </summary>
	public sealed class AdHocDatabase : Database
	{
		/// <summary>
		/// <para>Creates an ad-hoc database connection using the provider factory and connection string supplied.</para>
		/// </summary>
		/// <param name="factory">The provider factory for the DBMS this connection expects</param>
		/// <param name="connectionString">The connection string containing the parameters of this connection</param>
		public AdHocDatabase(DbProviderFactory factory, string connectionString) : base(factory, connectionString) { }
	}
}
