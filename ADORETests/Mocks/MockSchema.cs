using ADORE;

namespace ADORETests.Mocks
{
	internal class MockSchema : Schema
	{
		protected override string Name => "mock";

		public MockSchema(Database parent) : base(parent) { }
	}
}
