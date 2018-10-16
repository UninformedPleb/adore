using System.Data;

namespace ADORE.Standard
{
	public class CommandParameter
	{
		public string Name { get; set; }
		public object Value { get; set; }
		public DbType Type { get; set; }
		public ParameterDirection Direction { get; set; }
	}
}
