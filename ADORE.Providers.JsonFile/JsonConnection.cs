using System.Data;
using System.Data.Common;

namespace ADORE.Providers.JsonFile
{
	public class JsonConnection : DbConnection
	{
		private string _connectionString;
		private string _filename;
		private ConnectionState _state = ConnectionState.Closed;
		internal Stream _fs;

		public override string ConnectionString
		{
			get => _connectionString;
			set
			{
				_connectionString = value;
				string[] pairs = _connectionString.Split(';');
				for(int x = 0; x < pairs.Length; x++)
				{
					string[] pair = pairs[x].Split('=');
					if(pair.Length != 2) { continue; }
					switch(pair[0].ToLowerInvariant().Replace(" ", string.Empty))
					{
						case "database":
						case "datasource":
						case "catalog":
						case "initialcatalog":
						case "filename":
							_filename = pair[1];
							break;
					}
				}
			}
		}
		public override string Database => _filename;
		public override string DataSource => _filename;
		public override string ServerVersion => "1";
		public override ConnectionState State => _state;
		internal Stream FS => _fs;

		public JsonConnection()
		{
		}
		public JsonConnection(string connectionString) : this()
		{
			ConnectionString = connectionString;
		}

		public override void ChangeDatabase(string databaseName)
		{
			_filename = databaseName;
		}

		public override void Close()
		{
			_fs.Flush();
			_fs.Dispose();
			_state = ConnectionState.Closed;
		}
		public override void Open()
		{
			_fs = File.Open(_filename, FileMode.Open, FileAccess.ReadWrite);
			_state = ConnectionState.Open;
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			throw new NotImplementedException();
		}

		protected override DbCommand CreateDbCommand()
		{
			return new JsonCommand() { Connection = this };
		}
	}
}
