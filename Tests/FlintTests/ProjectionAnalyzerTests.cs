using FluentAssertions;
using Mono.Cecil;

namespace FlintTests
{
	[TestClass]
	public class ProjectionAnalyzerTests
	{
		private static ModuleDefinition LoadSamples()
		{
			return ModuleDefinition.ReadModule("Samples.dll", new ReaderParameters { ReadSymbols = true });
		}

		[TestMethod]
		public void ReadWholeObject()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadWholeObject));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadAllProperties()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadAllProperties));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadSomeProperties()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadSomeProperties));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.ReadSomeProperties line 36"
			]);
		}

		[TestMethod]
		public void MultipleQueries()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.MultipleQueries));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.MultipleQueries line 49",
				"consider using projection { Id, Email } in method Samples.Projections.MultipleQueries line 55",
			]);
		}

		[TestMethod]
		public void ComplexProjection()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ComplexProjection));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Number, Items = { Product.Name } } in method Samples.Projections.ComplexProjection line 68",
			]);
		}

		[TestMethod]
		public void ReadAllChainedProperties()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadAllChainedProperties));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void SimpleCRUD()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.SimpleCRUD));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name, Price } in method Samples.Projections.SimpleCRUD line 111",
			]);
		}

		[TestMethod]
		public void ToListAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ToListAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToListAsync line 142"
			]);
		}

		[TestMethod]
		public void ToArrayAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ToArrayAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToArrayAsync line 154"
			]);
		}

		[TestMethod]
		public void ToHashSetAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ToHashSetAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToHashSetAsync line 166"
			]);
		}

		[TestMethod]
		public void ToDictionaryAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ToDictionaryAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.ToDictionaryAsync line 178"
			]);
		}

		[TestMethod]
		public void FirstAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.FirstAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.FirstAsync line 190"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.FirstOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.FirstOrDefaultAsync line 199"
			]);
		}

		[TestMethod]
		public void LastAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.LastAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.LastAsync line 209"
			]);
		}

		[TestMethod]
		public void LastOrDefaultAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.LastOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.LastOrDefaultAsync line 218"
			]);
		}

		[TestMethod]
		public void SingleAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.SingleAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.SingleAsync line 227"
			]);
		}

		[TestMethod]
		public void SingleOrDefaultAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.SingleOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.SingleOrDefaultAsync line 236"
			]);
		}

		[TestMethod]
		public void AsAsyncEnumerable()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.AsAsyncEnumerable));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.AsAsyncEnumerable line 246"
			]);
		}

		[TestMethod]
		public void ReadForUpdate_ChangeProperty()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_ChangeProperty));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_ChangeNestedProperty()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_ChangeNestedProperty));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionAdd()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_CollectionAdd));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionRemove()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_CollectionRemove));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionIterate()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_CollectionIterate));

			ctx.Output.Should().BeEmpty();
		}
	}
}
