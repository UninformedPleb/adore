using System.Data;

namespace ADORE.Extensions
{
	internal static class DataRowExtension
	{
		internal static T MapTo<T>(this DataRow dr)
		{
			Type t = typeof(T);
			T thing = Activator.CreateInstance<T>();
			
			foreach(var field in t.GetFields())
			{
				if(dr.Table.Columns.Contains(field.Name) && dr.Table.Columns[field.Name].DataType.Equals(field.FieldType))
				{
					field.SetValue(thing, dr[field.Name]);
				}
			}
			foreach(var prop in t.GetProperties().Where(p => p.CanWrite))
			{
				if(dr.Table.Columns.Contains(prop.Name) && dr.Table.Columns[prop.Name].DataType.Equals(prop.PropertyType))
				{
					prop.SetValue(thing, dr[prop.Name]);
				}
			}

			return thing;
		}
	}
}
