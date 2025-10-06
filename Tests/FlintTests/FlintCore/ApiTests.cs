namespace FlintTests.FlintCore
{
	[TestClass]
	public class ApiTests
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
			Flint.Api.Run("WebApp.dll");

			var output = _mockOut.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

			output.AssertSame([
				"consider using projection { Id, Name, User.FirstName, User.LastName } in method WebApp.Program.Main line 19",
				"add Include(t => t.User) in method WebApp.Program.Main line 19",
				"add AsNoTracking() in method WebApp.Program.Main line 19",

				"consider using projection { Posts = { Author.FirstName, Author.LastName }, Tags = { Name } } in method WebApp.Program.Main line 44",
				"add Include(b => b.Posts).ThenInclude(p => p.Author) in method WebApp.Program.Main line 44",
				"add Include(b => b.Tags) in method WebApp.Program.Main line 44",
				"add AsNoTracking() in method WebApp.Program.Main line 44",
				"consider adding AsSplitQuery() in method WebApp.Program.Main line 44",

				"consider using Outbox pattern in method WebApp.Program.Main line 58",
			]);
		}
	}
}
