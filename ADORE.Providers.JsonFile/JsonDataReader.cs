using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text.Json.Nodes;

namespace ADORE.Providers.JsonFile
{
	public class JsonDataReader : DbDataReader, IDataReader, IDataRecord, IEnumerable
	{
		#region DbDataReader
		public override bool HasRows => (_data?.Count() ?? 0) > 0;
		#endregion

		#region IDataReader
		public override int Depth { get => throw new NotImplementedException(); }
		public override bool IsClosed { get => throw new NotImplementedException(); }
		public override int RecordsAffected { get => throw new NotImplementedException(); }

		public override void Close() { }
		public override DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}
		public override bool NextResult()
		{
			throw new NotImplementedException();
		}
		public override bool Read()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region IDataRecord
		public override object this[int i] { get => throw new NotImplementedException(); }
		public override object this[string name] { get => throw new NotImplementedException(); }
		public override int FieldCount { get => throw new NotImplementedException(); }

		public override bool GetBoolean(int i) => GetValue<bool>(i);
		public override byte GetByte(int i) => GetValue<byte>(i);
		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length) => throw new NotImplementedException();
		public override char GetChar(int i) => GetValue<char>(i);
		public override long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length) => throw new NotImplementedException();
		public new JsonDataReader GetData(int i) => throw new NotImplementedException();
		public override string GetDataTypeName(int i) => throw new NotImplementedException();
		public override DateTime GetDateTime(int i) => GetValue<DateTime>(i);
		public override decimal GetDecimal(int i) => GetValue<decimal>(i);
		public override double GetDouble(int i) => GetValue<double>(i);
		public override Type GetFieldType(int i) => throw new NotImplementedException();
		public override float GetFloat(int i) => GetValue<float>(i);
		public override Guid GetGuid(int i) => GetValue<Guid>(i);
		public override short GetInt16(int i) => GetValue<short>(i);
		public override int GetInt32(int i) => GetValue<int>(i);
		public override long GetInt64(int i) => GetValue<long>(i);
		public override string GetName(int i) => throw new NotImplementedException();
		public override int GetOrdinal(string name) => throw new NotImplementedException();
		public override string GetString(int i) => GetValue<string>(i);
		public override object GetValue(int i) => GetValue<object>(i);
		public override int GetValues(object[] values) => throw new NotImplementedException();
		public override bool IsDBNull(int i) => this[i] is DBNull;
		#endregion

		#region IEnumerable
		public override IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion

		private IEnumerable<JsonNode> _data = null;

		public T GetValue<T>(int i)
		{
			if(this[i] is T t) { return t; }
			throw new InvalidCastException();
		}
	}
}
