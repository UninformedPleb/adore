using System.Data;
using System.Data.Common;

namespace ADORETests.Mocks
{
	internal class MockDbCommand : DbCommand
	{
		public override string CommandText { get; set; }
		public override int CommandTimeout { get; set; }
		public override CommandType CommandType { get; set; }
		public override bool DesignTimeVisible { get; set; }
		public override UpdateRowSource UpdatedRowSource { get; set; }
		protected override DbConnection DbConnection { get; set; }

		protected override DbParameterCollection DbParameterCollection => throw new NotImplementedException();

		protected override DbTransaction DbTransaction { get; set; }

		public override void Cancel() { }
		public override int ExecuteNonQuery() => default(int);
		public override object ExecuteScalar() => null;
		public override void Prepare() { }
		protected override DbParameter CreateDbParameter() => new MockDbParameter();
		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => throw new NotImplementedException();
	}
}
