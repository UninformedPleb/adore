using System.Data.Common;

namespace ADORE
{
	public sealed class AdHocDatabase : Database
	{
		public AdHocDatabase(DbProviderFactory factory, string connectionString) : base(factory, connectionString) { }
	}
}
