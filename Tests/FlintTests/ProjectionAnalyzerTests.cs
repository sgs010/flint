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
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ReadWholeObject");

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadAllProperties()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ReadAllProperties");

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadSomeProperties()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ReadSomeProperties");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.ReadSomeProperties()"
			]);
		}

		[TestMethod]
		public void MultipleQueries()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "MultipleQueries");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.MultipleQueries()",
				"consider using projection { Id, Email } in method Samples.Projections.MultipleQueries()",
			]);
		}

		[TestMethod]
		public void ComplexProjection()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ComplexProjection");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Number, Items = { Product.Name } } in method Samples.Projections.ComplexProjection()",
			]);
		}

		[TestMethod]
		public void ReadAllChainedProperties()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ReadAllChainedProperties");

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void SimpleCRUD()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "SimpleCRUD");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name, Price } in method Samples.Projections.SimpleCRUD()",
			]);
		}

		[TestMethod]
		public void ToListAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ToListAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToListAsync()"
			]);
		}

		[TestMethod]
		public void ToArrayAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ToArrayAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToArrayAsync()"
			]);
		}

		[TestMethod]
		public void ToHashSetAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ToHashSetAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToHashSetAsync()"
			]);
		}

		[TestMethod]
		public void ToDictionaryAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ToDictionaryAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.ToDictionaryAsync()"
			]);
		}

		[TestMethod]
		public void FirstAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "FirstAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.FirstAsync()"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "FirstOrDefaultAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.FirstOrDefaultAsync()"
			]);
		}

		[TestMethod]
		public void LastAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "LastAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.LastAsync()"
			]);
		}

		[TestMethod]
		public void LastOrDefaultAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "LastOrDefaultAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.LastOrDefaultAsync()"
			]);
		}

		[TestMethod]
		public void SingleAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "SingleAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.SingleAsync()"
			]);
		}

		[TestMethod]
		public void SingleOrDefaultAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "SingleOrDefaultAsync");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.SingleOrDefaultAsync()"
			]);
		}

		[TestMethod]
		public void AsAsyncEnumerable()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "AsAsyncEnumerable");

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.AsAsyncEnumerable()"
			]);
		}

		//[TestMethod]
		//public void ReadForUpdate()
		//{
		//	using var asm = ModuleDefinition.ReadModule("Samples.dll");
		//	var ctx = new AnalyzerContextMock();

		//	Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, "Projections", "ReadForUpdate");

		//	ctx.Output.Should().BeEmpty();
		//}
	}
}
