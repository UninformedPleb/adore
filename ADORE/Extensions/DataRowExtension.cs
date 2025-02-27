using System.Data;

namespace ADORE.Extensions
{
	internal static class DataRowExtension
	{
		internal static T MapTo<T>(this DataRow dr)
		{
			Type t = typeof(T);
			T thing = Activator.CreateInstance<T>();

			var fields =
				from f in t.GetFields()
				where !f.IsInitOnly && !f.IsStatic && !f.IsLiteral
					&& dr.Table.Columns.Contains(f.Name)
					&& (
						dr.Table.Columns[f.Name].DataType.Equals(f.FieldType)
						|| dr.Table.Columns[f.Name].DataType.IsSubclassOf(f.FieldType)
					)
				select f;
			foreach(var field in fields) { field.SetValue(thing, dr[field.Name]); }

			var props =
				from p in t.GetProperties()
				where p.CanWrite
					&& dr.Table.Columns.Contains(p.Name)
					&& (
						dr.Table.Columns[p.Name].DataType.Equals(p.PropertyType)
						|| dr.Table.Columns[p.Name].DataType.IsSubclassOf(p.PropertyType)
					)
				select p;
			foreach(var prop in props) { prop.SetValue(thing, dr[prop.Name]); }

			return thing;
		}
	}
}
