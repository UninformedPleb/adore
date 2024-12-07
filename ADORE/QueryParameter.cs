using System.Data;

using ADORE.Configuration.Metadata;

namespace ADORE
{
	/// <summary>
	/// <para>Represents a database query parameter.</para>
	/// </summary>
	public class QueryParameter
	{
		/// <summary>
		/// <para>The parameter name.</para>
		/// <para>If the parameter is named, this will be used. Positional parameters will only use this if it parses to an int (after trimming an "@" prefix).</para>
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// <para>The value of the parameter.</para>
		/// </summary>
		public object Value { get; set; }
		/// <summary>
		/// <para>The data type of the parameter, expressed as a DbType enumeration value.</para>
		/// </summary>
		public DbType Type { get; set; }
		/// <summary>
		/// <para>The direction of the parameter.</para>
		/// </summary>
		public ParameterDirection Direction { get; set; } = ParameterDirection.Input;

		/// <summary>
		/// <para>The name of the parameter, with an "@" prefix added if it didn't already have one.</para>
		/// <para>This is non-destructive. The Name remains unchanged.</para>
		/// </summary>
		public string ParameterizedName => Name.StartsWith("@") ? Name : "@" + Name;
		/// <summary>
		/// <para>The name of the parameter, with the "@" prefix stripped off if it has one.</para>
		/// <para>This is non-destructive. The Name remains unchanged.</para>
		/// </summary>
		public string PropertyizedName => Name.StartsWith("@") ? Name.Substring(1) : Name;
	}
}
