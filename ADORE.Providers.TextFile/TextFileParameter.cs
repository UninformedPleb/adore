using System.Data;
using System.Data.Common;

namespace ADORE.Providers.TextFile
{
	public class TextFileParameter : DbParameter
	{
		#region DbParameter
		public override string ParameterName { get; set; }
		public override object Value { get; set; }
		public override DbType DbType { get; set; } = DbType.Object;
		public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
		public override bool IsNullable { get; set; }
		public override int Size { get; set; }
		public override string SourceColumn { get; set; }
		public override bool SourceColumnNullMapping { get; set; }

		public override void ResetDbType() => throw new NotImplementedException();
		#endregion

		public string Name { get => ParameterName; set => ParameterName = value; }
	}

	public class TextFileParameter<T> : TextFileParameter
	{
		public new T Value { get => (T)base.Value; set => base.Value = value; }
	}
}
