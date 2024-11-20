using System.Collections.Generic;
using System.Text;

namespace ADORE.Configuration
{
	public class ConnectionStringConfig
	{
		public string ConnectionName { get; set; }
		public string ProviderName { get; set; }
		public Dictionary<string,string> ConnectionStringValues { get; set; } = new Dictionary<string, string>();

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
		public string CatalogName
		{
			get
			{
				foreach(var kvp in ConnectionStringValues)
				{
					if(kvp.Key == "Initial Catalog" || kvp.Key == "Database") { return kvp.Value; }
				}
				return string.Empty;
			}
		}

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
