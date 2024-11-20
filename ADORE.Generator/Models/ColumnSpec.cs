using System.Linq;

using ADORE.Configuration.Metadata;

namespace ADORE.Generator.Models
{
	internal class ColumnSpec
	{
		public string Name { get; set; }
		public int Order { get; set; }
		public int MaxLength { get; set; }
		public int Precision { get; set; }
		public int Scale { get; set; }
		public bool IsNullable { get; set; }
		public string TypeName { get; set; }

		public TypeMapSpec TypeMap
		{
			get => TypeMapSpec.TypeMapData.Where(tm => tm.SqlType == TypeName).First();
		}
		public string CSharpTypeNameString
		{
			get => TypeMap.CSharpType;
		}
		public string CSharpNullableTypeNameString
		{
			get => TypeMap.CSharpNullDefault ? TypeMap.CSharpType : TypeMap.CSharpType + "?";
		}

		public virtual string Generate(bool isPrototype = false)
		{
			return $"public {(IsNullable ? CSharpNullableTypeNameString : CSharpTypeNameString)} {DbGenerator.FixCase(Name)} {{ get; set; }}";
		}
	}
}
