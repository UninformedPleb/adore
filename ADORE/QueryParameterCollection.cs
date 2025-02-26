using System.Data;
using System.Data.Common;

using ADORE.Configuration.Metadata;

namespace ADORE
{
	/// <summary>
	/// <para>Manages a collection of QueryParameter objects and provides bulk mappings as a method of intake.</para>
	/// </summary>
	public class QueryParameterCollection
	{
		private Dictionary<string,QueryParameter> _parameters = new Dictionary<string, QueryParameter>();

		/// <summary>
		/// <para>Gets the parameter in the collection indexed by the given name.</para>
		/// </summary>
		/// <param name="name">The name to search for.</param>
		/// <returns>The named QueryParameter.</returns>
		public QueryParameter this[string name] { get => _parameters[name]; }

		// internal, so this should only be created by Query
		internal QueryParameterCollection() { }

		#region command parameter mapping - internal only
		/// <summary>
		/// <para>Maps this parameter collection to a command's parameter collection.</para>
		/// <para>NOTE: This is destructive to the existing command parameters.</para>
		/// </summary>
		/// <param name="cmd">The command to map to.</param>
		internal void MapToCommand(DbCommand cmd)
		{
			cmd.Parameters.Clear();
			DbParameter param;
			foreach(var kvp in _parameters)
			{
				param = cmd.CreateParameter();
				param.ParameterName = kvp.Value.ParameterizedName;
				param.Value = kvp.Value.Value ?? DBNull.Value;
				param.DbType = kvp.Value.Type;
				param.Direction = kvp.Value.Direction;
				cmd.Parameters.Add(param);
			}
		}
		/// <summary>
		/// <para>Maps the command's output, in-out, and return parameters to this parameter collection.</para>
		/// <para>Input-only parameters are left as-is, since their values should not have changed in the database script.</para>
		/// </summary>
		/// <param name="cmd">The command to map from.</param>
		internal void MapFromCommand(DbCommand cmd)
		{
			foreach(var kvp in _parameters.Where(kvp => kvp.Value.Direction != ParameterDirection.Input))
			{
				kvp.Value.Value = cmd.Parameters[kvp.Value.ParameterizedName].Value is DBNull ? null : cmd.Parameters[kvp.Value.ParameterizedName].Value;
			}
		}
		#endregion

		/// <summary>
		/// <para>Adds a parameter to the collection</para>
		/// </summary>
		/// <param name="param">The parameter to add</param>
		public void Add(QueryParameter param)
		{
			if(string.IsNullOrEmpty(param.Name)) { param.Name = _parameters.Count.ToString(); }
			_parameters[param.ParameterizedName] = param;
		}

		/// <summary>
		/// <para>Maps an object's fields and properties into parameters and adds them to the collection</para>
		/// </summary>
		/// <param name="param"></param>
		public void MapObject(object param)
		{
			Type t = param.GetType();
			if(t.IsPrimitive || t == typeof(string) || t == typeof(DateTime) || t == typeof(TimeSpan))
			{
				Add(MapParameter(null, param));
			}
			else
			{
				foreach(var field in param.GetType().GetFields())
				{
					Add(MapParameter(field.Name, field.GetValue(param)));
				}
				foreach(var prop in param.GetType().GetProperties().Where(p => p.CanRead))
				{
					Add(MapParameter(prop.Name, prop.GetValue(param)));
				}
			}
		}
		private static QueryParameter MapParameter<T>(string name, T value)
		{
			if(value is TimeSpan ts)
			{
				return new QueryParameter() {
					Name = name ?? string.Empty,
					Value = ts.Ticks,
					Type = DbType.Int64,
					Direction = ParameterDirection.InputOutput
				};
			}
			else
			{
				return new QueryParameter()
				{
					Name = name ?? string.Empty,
					Value = value,
					Type = TypeMapSpec.GetDbTypeMappingFrom(value),
					Direction = ParameterDirection.InputOutput
				};
			}
		}
	}
}
