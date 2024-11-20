using System.Collections.Generic;

namespace ADORE.Generator.Models
{
	internal class RoutineSpec
	{
		public string SchemaName { get; set; }
		public string RoutineName { get; set; }
		public List<ParameterSpec> Parameters { get; set; } = new List<ParameterSpec>();
		public TableSpec Resultset { get; set; }
	}
}
