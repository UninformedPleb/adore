using System.Data;
using System.Data.Common;

namespace ADORETests.Mocks
{
	internal class MockDbConnection : DbConnection
	{
		public override string ConnectionString { get; set; }

		public override string Database => throw new NotImplementedException();
		public override string DataSource => throw new NotImplementedException();
		public override string ServerVersion => throw new NotImplementedException();
		private ConnectionState _state = ConnectionState.Closed;
		public override ConnectionState State => _state;

		public override void ChangeDatabase(string databaseName) { }
		public override void Close() { _state = ConnectionState.Closed; }
		public override void Open() { _state = ConnectionState.Open; }
		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotImplementedException();
		protected override DbCommand CreateDbCommand() => new MockDbCommand() { Connection = this };
	}
}
