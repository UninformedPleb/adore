using System.Data;

namespace ADORE.NetFX
{
	public class CommandParameter
	{
		/// <summary>
		/// <para>The parameter name. Should include the "@" if the database expects it.</para>
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// <para>The parameter value.</para>
		/// </summary>
		public object Value { get; set; }
		/// <summary>
		/// <para>The parameter data type.</para>
		/// </summary>
		public DbType Type { get; set; }
		/// <summary>
		/// <para>The parameter direction.</para>
		/// </summary>
		public ParameterDirection Direction { get; set; }
	}
}
