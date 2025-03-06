using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ADORE.Configuration.Metadata
{
	/// <summary>
	/// <para>Represents a mapping between SQL, C#, and System.Data.DbType data types for the purposes of conversion.</para>
	/// </summary>
	public class TypeMapSpec
	{
		/// <summary>
		/// <para>A list of all TypeMapSpec values configured within this library</para>
		/// </summary>
		public static List<TypeMapSpec> TypeMapData { get; private set; }

		#region static init
		static TypeMapSpec()
		{
			LoadTypeMapData();
		}
		private static void LoadTypeMapData()
		{
			using(var s = typeof(TypeMapSpec).Assembly.GetManifestResourceStream("ADORE.Configuration.Metadata.TypeMapData.json"))
			using(var sr = new StreamReader(s))
			{
				TypeMapData = JsonSerializer.Deserialize<List<TypeMapSpec>>(sr.ReadToEnd(), new JsonSerializerOptions() { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
			}
		}
		#endregion

		#region static data lookups
		/// <summary>
		/// <para>Finds the TypeMapSpec for a given SQL data type name</para>
		/// </summary>
		/// <param name="sqlType">The name of the SQL data type</param>
		/// <returns>A TypeMapSpec corresponding to the SQL data type</returns>
		public static IEnumerable<TypeMapSpec> FindBySqlType(string sqlType) => TypeMapData.Where(tmd => tmd.SqlType == sqlType);
		/// <summary>
		/// <para>Finds the TypeMapSpec for a given C# data type name</para>
		/// </summary>
		/// <param name="csType">The C# type name</param>
		/// <returns>A TypeMapSpec corresponding to the C# type name</returns>
		public static IEnumerable<TypeMapSpec> FindByCSharpType(string csType) => TypeMapData.Where(tmd => tmd.CSharpType == csType);
		/// <summary>
		/// <para>Finds the TypeMapSpec for a given C# Type object</para>
		/// </summary>
		/// <param name="csType">
		/// <para>The C# Type object.</para>
		/// <para>Note that only the Name property will be used for this lookup, not the FullName or the AssemblyQualifiedName.</para>
		/// </param>
		/// <returns>A TypeMapSpec corresponding to the C# Type object</returns>
		public static IEnumerable<TypeMapSpec> FindByCSharpType(Type csType) => TypeMapData.Where(tmd => tmd.CSharpType == csType.Name);
		/// <summary>
		/// <para>Finds the TypeMapSpec for a given DbType enumerated constant</para>
		/// </summary>
		/// <param name="dbType">The DbType enumerated constant</param>
		/// <returns>A TypeMapSpec corresponding to the DbType enumerated constant</returns>
		public static IEnumerable<TypeMapSpec> FindByDbType(DbType dbType) => TypeMapData.Where(tmd => tmd.DbType == dbType);
		#endregion

		#region static mappings
		/// <summary>
		/// <para>Gets the DbType for an object's concrete type</para>
		/// </summary>
		/// <param name="o">The object to get the type from</param>
		/// <returns>The DbType mapped to the object's concrete type</returns>
		public static DbType GetDbTypeMappingFrom<T>(T o)
		{
			var t = typeof(T);
			if(t.IsGenericType && t.Equals(typeof(Nullable))) { t = t.GenericTypeArguments[0]; }
			return GetDbTypeMapping(t);
		}
		/// <summary>
		/// <para>Gets the DbType for a given Type</para>
		/// </summary>
		/// <param name="t">The Type</param>
		/// <returns>The DbType mapped to the Type</returns>
		public static DbType GetDbTypeMapping(Type t) => FindByCSharpType(t).FirstOrDefault()?.DbType ?? DbType.Object;
		#endregion

		/// <summary>
		/// <para>The SQL data type</para>
		/// </summary>
		public string SqlType { get; set; }
		/// <summary>
		/// <para>The C# type name</para>
		/// </summary>
		public string CSharpType { get; set; }
		/// <summary>
		/// <para>A flag indicating if the C# type defaults to null. This is true for reference types and false for value types.</para>
		/// <para>This does not account for compile-time nullability checking in your project settings.</para>
		/// </summary>
		public bool CSharpNullDefault { get; set; }
		/// <summary>
		/// <para>The DbType enumerated constant</para>
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DbType DbType { get; set; }
	}
}
