using System.Collections.Generic;
using System.Text;

namespace ADORE.Configuration
{
	/// <summary>
	/// <para>Represents a connection string and how to use it.</para>
	/// </summary>
	public class ConnectionStringConfig
	{
		/// <summary>
		/// <para>The name of the connection.</para>
		/// <para>This has no meaning beyond being an identifier for this set of connection settings. It should be unique within your software.</para>
		/// </summary>
		public string ConnectionName { get; set; }
		/// <summary>
		/// <para>The name of the provider configuration used by this connection.</para>
		/// <para>This must correspond to one of the items in the list of provider factory configurations in your software.</para>
		/// </summary>
		public string ProviderName { get; set; }
		/// <summary>
		/// <para>The list of connection string values used to connect to this database.</para>
		/// </summary>
		public Dictionary<string,string> ConnectionStringValues { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// <para>A connection string built from the ConnectionStringValues dictionary.</para>
		/// </summary>
		public string ConnectionString
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				foreach(var kvp in ConnectionStringValues)
				{
					sb.Append($";{kvp.Key}={kvp.Value}");
				}
				return sb.ToString().Substring(1); // substring to lop off the semicolon at the front
			}
		}
		/// <summary>
		/// <para>The database catalog name specified in the ConnectionStringValues, if available.</para>
		/// <para>If no catalog name can be found, an empty string will be returned.</para>
		/// </summary>
		public string CatalogName
		{
			get
			{
				string name = string.Empty;
				if(!ConnectionStringValues.TryGetValue("Initial Catalog", out name))
				{
					ConnectionStringValues.TryGetValue("Database", out name);
				}
				return name;
			}
		}

		/// <summary>
		/// <para>Parses a connection string into a ConnectionStringConfig object.</para>
		/// <para>This only populates the ConnectionStringValues dictionary. You will still need to set the ConnectionName and ProviderName separately.</para>
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static ConnectionStringConfig Parse(string s)
		{
			var csc = new ConnectionStringConfig();

			string[] pairs = s.Split(';');
			foreach(var pair in pairs)
			{
				string[] kv = pair.Split('=');
				if(kv.Length < 1) { continue; }
				string key = kv[0];
				string value = kv.Length > 1 ? kv[1] : string.Empty;
				csc.ConnectionStringValues[key] = value;
			}

			return csc;
		}
	}
}
