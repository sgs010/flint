using FluentAssertions;
using Mono.Cecil;

namespace FlintTests
{
	[TestClass]
	public class ProjectionAnalyzerTests
	{
		[TestMethod]
		public void ReadWholeObject()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ReadWholeObject");

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadAllProperties()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ReadAllProperties");

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadSomeProperties()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ReadSomeProperties");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.ReadSomeProperties()"
			]);
		}

		[TestMethod]
		public void MultipleQueries()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "MultipleQueries");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.MultipleQueries()",
				"consider using projection { Id, Email } in method Samples.Projections.MultipleQueries()",
			]);
		}

		[TestMethod]
		public void ComplexProjection()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ComplexProjection");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Number, Items = { Product.Name } } in method Samples.Projections.ComplextProjection()",
			]);
		}

		[TestMethod]
		public void ReadAllChainedProperties()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ReadAllChainedProperties");

			ctx.Output.Should().BeEmpty();
		}
	}
}
