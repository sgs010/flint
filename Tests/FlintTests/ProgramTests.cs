using FluentAssertions;

namespace FlintTests
{
	[TestClass]
	public class ProgramTests
	{
		private StringWriter _mockOut;
		private TextWriter _originalOut;

		[TestInitialize]
		public void Setup()
		{
			_mockOut = new();
			_originalOut = Console.Out;
			Console.SetOut(_mockOut);
		}

		[TestCleanup]
		public void Cleanup()
		{
			Console.SetOut(_originalOut);
			_mockOut.Dispose();
		}

		[TestMethod]
		public void TestWebApp()
		{
			Flint.Program.Main(["--input=WebApp.dll"]);

			var output = _mockOut.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
			output.Should().BeEquivalentTo([
				"consider using projection { Id, Name, User.FirstName, User.LastName } in method WebApp.Program.Main line 18"
			]);
		}
	}
}
