using System.Data;
using System.Data.Common;

namespace ADORE.Providers.JsonFile
{
	public class JsonDataAdapter : DbDataAdapter
	{
		public JsonDataAdapter() : base()
		{
		}
		public JsonDataAdapter(JsonCommand selectCommand) : this()
		{
			SelectCommand = selectCommand;
		}
		public JsonDataAdapter(string selectCommandText, string selectConnectionString) : this()
		{
			SelectCommand = new JsonCommand() {
				CommandText = selectCommandText,
				Connection = new JsonConnection() {
					ConnectionString = selectConnectionString,
				},
			};
		}
		public JsonDataAdapter(string selectCommandText, JsonConnection selectConnection) : this()
		{
			SelectCommand = new JsonCommand() {
				CommandText = selectCommandText,
				Connection = selectConnection,
			};
		}
	}
}
