using System.Data;
using System.Data.Common;

namespace ADORETests.Mocks
{
	internal class MockDbDataAdapter : DbDataAdapter
	{
		public override int Fill(DataSet dataSet)
		{
			DataTable dt = new DataTable("Foo");
			dt.Columns.Add("FooID", typeof(int));
			dt.Columns.Add("FooName", typeof(string));
			dt.Rows.Add(1, "first");
			dt.Rows.Add(2, "second");
			dt.Rows.Add(3, "third");
			dt.AcceptChanges();
			dataSet.Tables.Add(dt);

			dt = new DataTable("Bar");
			dt.Columns.Add("BarID", typeof(int));
			dt.Columns.Add("FooID", typeof(int));
			dt.Columns.Add("BarName", typeof(string));
			dt.Rows.Add(1, 2, "first");
			dt.Rows.Add(2, 1, "second");
			dt.Rows.Add(3, 3, "third");
			dt.Rows.Add(4, 2, "fourth");
			dt.AcceptChanges();
			dataSet.Tables.Add(dt);

			return default(int);
		}
	}
}
