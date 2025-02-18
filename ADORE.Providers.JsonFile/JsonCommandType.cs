namespace ADORE.Providers.JsonFile
{
	public enum JsonCommandType
	{
		Unknown = 0, // auto-detect?

		Path = 1, // xpath, jpath/jsonpath
		Pointer = 2, // xpointer, jpointer
		GraphQL = 3,
	}
}
