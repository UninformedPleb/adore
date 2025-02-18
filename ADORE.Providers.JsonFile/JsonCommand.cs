using System.Data;
using System.Data.Common;

namespace ADORE.Providers.JsonFile
{
	public class JsonCommand : DbCommand, IDbCommand, IDisposable
	{
		#region DbCommand
		public override string CommandText { get; set; }
		public override int CommandTimeout { get; set; }
		public override CommandType CommandType { get; set; } = CommandType.Text;
		protected override DbConnection DbConnection { get; set; }
		protected override DbParameterCollection DbParameterCollection { get => _jsonParameterCollection; }
		protected override DbTransaction DbTransaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public override bool DesignTimeVisible { get => false; set => throw new NotImplementedException(); }
		public override UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public override void Cancel() => throw new NotImplementedException();
		protected override DbParameter CreateDbParameter() => new JsonParameter();
		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			// TODO: parse the command
			// TODO: create a LINQ expression
			// TODO: open the file and run the expression
			// TODO: pass the expression to this data reader:
			var reader = new JsonDataReader();
			return reader;
		}
		public override int ExecuteNonQuery()
		{
			using var reader = ExecuteReader();
			return reader.RecordsAffected;
		}
		public override object ExecuteScalar()
		{
			using var reader = ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow);
			if(reader.FieldCount > 0) { return reader.GetValue(0); }
			return null;
		}
		public override void Prepare() => throw new NotImplementedException();
		#endregion

		#region IDisposable
		~JsonCommand() => Dispose(false);
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				// TODO: dispose managed resources
			}
			// TODO: dispose unmanaged resources
		}
		#endregion

		public JsonCommandType JsonCommandType { get; set; } = JsonCommandType.Unknown;
		private JsonParameterCollection _jsonParameterCollection = new();

		public JsonCommand() : base()
		{
		}
		public JsonCommand(string text) : this()
		{
			CommandText = text;
		}
		public JsonCommand(string text, JsonConnection connection) : this(text)
		{
			Connection = connection;
		}
	}
}
