using System.Collections.Generic;
using System.Text;

namespace ADORE.Generator.Models
{
	internal class TableSpec
	{
		public string SchemaName { get; set; }
		public string TableName { get; set; }
		public List<ColumnSpec> Columns { get; set; } = new List<ColumnSpec>();

		public virtual string Generate(string ns, string classname, bool isPrototype = false)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("#nullable disable");
			sb.AppendLine($"namespace {ns};");
			sb.AppendLine("{");
			sb.AppendLine($"\tpublic partial class {classname}");
			sb.AppendLine("\t{");
			foreach(var col in Columns)
			{
				sb.AppendLine($"\t\t{col.Generate(isPrototype)}");
			}
			sb.AppendLine("\t}");
			sb.AppendLine("}");
			sb.AppendLine("#nullable restore");
			return sb.ToString();
		}
	}
}
