namespace FlintTests.FlintCore
{
	[TestClass]
	public class ApiTests
	{
		[TestMethod]
		public void TestWebApp()
		{
			var result = Flint.Api.AnalyzeCLI("WebApp.dll");

			result.AssertSame([
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

		[TestMethod]
		[Timeout(60 * 1000)]
		public void TestAnyDll()
		{
			var options = new Flint.ApiOptions { Trace = true };
			foreach (var path in Directory.GetFiles(".", "*.dll"))
			{
				Flint.Api.Analyze(path, options);
			}
		}
	}
}
