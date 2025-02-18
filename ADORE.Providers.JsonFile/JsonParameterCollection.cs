using System.Collections;
using System.Data.Common;

namespace ADORE.Providers.JsonFile
{
	public class JsonParameterCollection : DbParameterCollection
	{
		#region DbParameterCollection implementation
		public override int Count => _jsonDbParameters.Count;
		public override object SyncRoot => _syncRoot;

		public override int Add(object value)
		{
			// validate
			if(value is null) { throw new ArgumentNullException(); }
			if(value is not JsonParameter && !value.GetType().IsSubclassOf(typeof(JsonParameter))) { throw new InvalidCastException(); }
			// give it a name if it doesn't have one
			if(((JsonParameter)value).ParameterName.Length == 0)
			{
				((JsonParameter)value).ParameterName = _jsonDbParameters.Count.ToString();
			}
			// add it to the collection
			_jsonDbParameters.Add((JsonParameter)value);
			// return the index of the added parameter
			return _jsonDbParameters.Count - 1;
		}
		public override void AddRange(Array values)
		{
			// validate
			if(values is null) { throw new ArgumentNullException() }
			// add each item
			foreach(object value in values)
			{
				Add(value);
			}
		}
		public override void Clear() => _jsonDbParameters.Clear();
		public override bool Contains(object value) => _jsonDbParameters.Contains(value);
		public override bool Contains(string value) => _jsonDbParameters.Any(p => p.ParameterName == value);
		public override void CopyTo(Array array, int index) => ((ICollection)_jsonDbParameters).CopyTo(array, index);
		public override IEnumerator GetEnumerator() => _jsonDbParameters.GetEnumerator();
		public override int IndexOf(object value) => _jsonDbParameters.IndexOf((JsonParameter)value);
		public override int IndexOf(string parameterName)
		{
			for(int x = 0; x < _jsonDbParameters.Count; x++)
			{
				if(_jsonDbParameters[x].ParameterName == parameterName) { return x; }
			}
			return -1;
		}
		public override void Insert(int index, object value)
		{
			// validate
			if(value is null) { throw new ArgumentNullException(); }
			if(value is not JsonParameter && !value.GetType().IsSubclassOf(typeof(JsonParameter))) { throw new InvalidCastException(); }
			// insert
			_jsonDbParameters.Insert(index, (JsonParameter)value);
		}
		public override void Remove(object value) => _jsonDbParameters.Remove((JsonParameter)value);
		public override void RemoveAt(int index) => _jsonDbParameters.RemoveAt(index);
		public override void RemoveAt(string parameterName) => _jsonDbParameters.RemoveAt(IndexOf(parameterName));
		protected override DbParameter GetParameter(int index) => _jsonDbParameters[index];
		protected override DbParameter GetParameter(string parameterName) => _jsonDbParameters[IndexOf(parameterName)];
		protected override void SetParameter(int index, DbParameter value) => _jsonDbParameters[index] = (JsonParameter)value;
		protected override void SetParameter(string parameterName, DbParameter value) => _jsonDbParameters[IndexOf(parameterName)] = (JsonParameter)value;
		#endregion

		private List<JsonParameter> _jsonDbParameters = new();
		private object _syncRoot = new();

		public new JsonParameter this[int index] { get => _jsonDbParameters[index]; set => _jsonDbParameters[index] = value; }
		public new JsonParameter this[string name] { get => (JsonParameter)GetParameter(name); set => SetParameter(name, value); }

		internal JsonParameterCollection() : base() { }
		internal JsonParameterCollection(int capacity) : this()
		{
			_jsonDbParameters = new List<JsonParameter>(Math.Max(capacity, 1));
		}
	}
}
