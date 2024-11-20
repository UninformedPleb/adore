using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ADORE.Configuration.Metadata
{
	public class TypeMapSpec
	{
		public static List<TypeMapSpec> TypeMapData { get; private set; }

		#region static init
		static TypeMapSpec()
		{
			LoadTypeMapData();
		}
		private static void LoadTypeMapData()
		{
			using(var s = typeof(TypeMapSpec).Assembly.GetManifestResourceStream("ADORE.Metadata.TypeMapData.json"))
			using(var sr = new StreamReader(s))
			{
				TypeMapData = JsonSerializer.Deserialize<List<TypeMapSpec>>(sr.ReadToEnd());
			}
		}
		#endregion

		#region static data lookups
		public static IEnumerable<TypeMapSpec> FindBySqlType(string sqlType)
		{
			return TypeMapData.Where(tmd => tmd.SqlType == sqlType);
		}
		public static IEnumerable<TypeMapSpec> FindByCSharpType(string csType)
		{
			return TypeMapData.Where(tmd => tmd.CSharpType == csType);
		}
		public static IEnumerable<TypeMapSpec> FindByCSharpType(Type csType)
		{
			return TypeMapData.Where(tmd => tmd.CSharpType == csType.Name);
		}
		public static IEnumerable<TypeMapSpec> FindByDbType(DbType dbType)
		{
			return TypeMapData.Where(tmd => tmd.DbType == dbType);
		}
		#endregion

		public string SqlType { get; set; }
		public string CSharpType { get; set; }
		public bool CSharpNullDefault { get; set; }
		public DbType DbType { get; set; }
	}
}
