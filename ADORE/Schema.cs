namespace ADORE
{
	public abstract class Schema
	{
		protected Database _parent;

		protected Schema(Database parent)
		{
			_parent = parent;
		}
	}
}
