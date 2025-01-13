using System.Collections;
using System.Data.Common;

namespace ADORE.Providers.TextFile
{
	public class TextFileParameterCollection : DbParameterCollection
	{
		#region DbParameterCollection
		public override int Count => _textFileParameters.Count;
		public override object SyncRoot => _syncRoot;

		public override int Add(object value)
		{
			// validate
			if(value is null) { throw new ArgumentNullException(); }
			if(value is not TextFileParameter && !value.GetType().IsSubclassOf(typeof(TextFileParameter))) { throw new InvalidCastException(); }
			// give it a name if it doesn't have one
			if(((TextFileParameter)value).ParameterName.Length == 0)
			{
				((TextFileParameter)value).ParameterName = _textFileParameters.Count.ToString();
			}
			// add it to the collection
			_textFileParameters.Add((TextFileParameter)value);
			// return the index of the added parameter
			return _textFileParameters.Count - 1;
		}
		public override void AddRange(Array values)
		{
			// validate
			if(values is null) { throw new ArgumentNullException(); }
			// add each item
			foreach(object value in values)
			{
				Add(value);
			}
		}
		public override void Clear() => _textFileParameters.Clear();
		public override bool Contains(object value) => _textFileParameters.Contains(value);
		public override bool Contains(string value) => _textFileParameters.Any(p => p.ParameterName == value);
		public override void CopyTo(Array array, int index) => ((ICollection)_textFileParameters).CopyTo(array, index);
		public override IEnumerator GetEnumerator() => _textFileParameters.GetEnumerator();
		public override int IndexOf(object value) => _textFileParameters.IndexOf((TextFileParameter)value);
		public override int IndexOf(string parameterName)
		{
			for(int x = 0; x < _textFileParameters.Count; x++)
			{
				if(_textFileParameters[x].ParameterName == parameterName) { return x; }
			}
			return -1;
		}
		public override void Insert(int index, object value)
		{
			// validate
			if(value is null) { throw new ArgumentNullException(); }
			if(value is not TextFileParameter && !value.GetType().IsSubclassOf(typeof(TextFileParameter))) { throw new InvalidCastException(); }
			// insert
			_textFileParameters.Insert(index, (TextFileParameter)value);
		}
		public override void Remove(object value) => _textFileParameters.Remove((TextFileParameter)value);
		public override void RemoveAt(int index) => _textFileParameters.RemoveAt(index);
		public override void RemoveAt(string parameterName) => _textFileParameters.RemoveAt(IndexOf(parameterName));
		protected override DbParameter GetParameter(int index) => _textFileParameters[index];
		protected override DbParameter GetParameter(string parameterName) => _textFileParameters[IndexOf(parameterName)];
		protected override void SetParameter(int index, DbParameter value) => _textFileParameters[index] = (TextFileParameter)value;
		protected override void SetParameter(string parameterName, DbParameter value) => _textFileParameters[IndexOf(parameterName)] = (TextFileParameter)value;
		#endregion

		private List<TextFileParameter> _textFileParameters = new();
		private object _syncRoot = new();

		public new TextFileParameter this[int index] { get => _textFileParameters[index]; set => _textFileParameters[index] = value; }
		public new TextFileParameter this[string name] { get => (TextFileParameter)GetParameter(name); set => SetParameter(name, value); }
	}
}
