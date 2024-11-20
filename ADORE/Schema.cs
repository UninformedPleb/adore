namespace ADORE
{
	public abstract class Schema
	{
		protected Database _parent;

		protected abstract string Name { get; }

		protected Schema(Database parent)
		{
			_parent = parent;
		}

		protected virtual Query CreateStoredProcedure(string procName, params object[] parameterMap)
		{
			string[] parts = procName.Split('.');
			if(parts.Length == 1) { procName = $"{Name}.{procName}"; }
			return _parent.CreateStoredProcedure(procName, parameterMap);
		}
	}
}
