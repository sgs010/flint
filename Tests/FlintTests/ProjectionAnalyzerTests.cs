using FluentAssertions;
using Mono.Cecil;

namespace FlintTests
{
	[TestClass]
	public class ProjectionAnalyzerTests
	{
		[TestMethod]
		public void Test001()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "Test001");

			ctx.Output.Should().BeEquivalentTo([
				"Consider using projection { Id, FirstName, LastName } in method Samples.Projections.Test001()."
			]);
		}
	}
}
