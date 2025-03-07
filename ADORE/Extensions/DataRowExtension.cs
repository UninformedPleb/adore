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
					&& CheckTypeFit(dr.Table.Columns[f.Name].DataType, f.FieldType)
				select f;
			foreach(var field in fields)
			{
				if(dr[field.Name] is DBNull) { dr[field.Name] = null; }
				field.SetValue(thing, dr[field.Name]);
			}

			var props =
				from p in t.GetProperties()
				where p.CanWrite
					&& dr.Table.Columns.Contains(p.Name)
					&& CheckTypeFit(dr.Table.Columns[p.Name].DataType, p.PropertyType)
				select p;
			foreach(var prop in props)
			{
				if(dr[prop.Name] is DBNull) { dr[prop.Name] = null; }
				prop.SetValue(thing, dr[prop.Name]);
			}

			return thing;
		}

		private static bool CheckTypeFit(Type src, Type dest)
		{
			if(src.Equals(dest)) { return true; }
			if(src.IsSubclassOf(dest)) { return true; }
			if(dest.IsGenericType && dest.Equals(typeof(Nullable)))
			{
				if(src.Equals(dest.GenericTypeArguments[0])) { return true; }
				if(src.IsSubclassOf(dest.GenericTypeArguments[0])) { return true; }
			}
			return false;
		}
	}
}
