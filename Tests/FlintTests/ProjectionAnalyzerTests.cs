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
				"consider using projection { Id, FirstName, LastName } in method Samples.Projections.Test001()"
			]);
		}

		[TestMethod]
		public void Test002()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "Test002");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, FirstName } in method Samples.Projections.Test002()",
				"consider using projection { Id, Address } in method Samples.Projections.Test002()"
			]);
		}

		[TestMethod]
		public void Test003()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "Test003");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, FirstName, LastName } in method Samples.Projections.Test003()"
			]);
		}

	}
}
