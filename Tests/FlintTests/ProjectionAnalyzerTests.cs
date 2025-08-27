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
				"consider using projection { Number, Items = { Product.Name } } in method Samples.Projections.ComplexProjection()",
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

		[TestMethod]
		public void SimpleCRUD()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "SimpleCRUD");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name, Price } in method Samples.Projections.SimpleCRUD()",
			]);
		}

		[TestMethod]
		public void ToListAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ToListAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToListAsync()"
			]);
		}

		[TestMethod]
		public void ToArrayAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ToArrayAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToArrayAsync()"
			]);
		}

		[TestMethod]
		public void ToHashSetAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ToHashSetAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToHashSetAsync()"
			]);
		}

		[TestMethod]
		public void ToDictionaryAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ToDictionaryAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.ToDictionaryAsync()"
			]);
		}

		[TestMethod]
		public void FirstAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "FirstAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.FirstAsync()"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "FirstOrDefaultAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.FirstOrDefaultAsync()"
			]);
		}

		[TestMethod]
		public void LastAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "LastAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.LastAsync()"
			]);
		}

		[TestMethod]
		public void LastOrDefaultAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "LastOrDefaultAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.LastOrDefaultAsync()"
			]);
		}

		[TestMethod]
		public void SingleAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "SingleAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.SingleAsync()"
			]);
		}

		[TestMethod]
		public void SingleOrDefaultAsync()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "SingleOrDefaultAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.SingleOrDefaultAsync()"
			]);
		}

		[TestMethod]
		public void AsAsyncEnumerable()
		{
			var ctx = new AnalyzerContextMock();
			var asm = ModuleDefinition.ReadModule("Samples.dll");

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "AsAsyncEnumerable");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.AsAsyncEnumerable()"
			]);
		}
	}
}
