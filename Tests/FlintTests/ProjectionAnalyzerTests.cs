using Flint.Analyzers;
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
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ReadWholeObject));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadAllProperties()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ReadAllProperties));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadSomeProperties()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ReadSomeProperties));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, FirstName } in method Samples.Projection.ReadSomeProperties line 36"
			]);
		}

		[TestMethod]
		public void MultipleQueries()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.MultipleQueries));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, FirstName } in method Samples.Projection.MultipleQueries line 49",
				"consider using projection { Id, Email } in method Samples.Projection.MultipleQueries line 55",
			]);
		}

		[TestMethod]
		public void ComplexProjection()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ComplexProjection));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Number, Items = { Product.Name } } in method Samples.Projection.ComplexProjection line 68",
			]);
		}

		[TestMethod]
		public void ReadAllChainedProperties()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ReadAllChainedProperties));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void SimpleCRUD()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.SimpleCRUD));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name, Price } in method Samples.Projection.SimpleCRUD line 111",
			]);
		}

		[TestMethod]
		public void ToListAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ToListAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.ToListAsync line 142"
			]);
		}

		[TestMethod]
		public void ToArrayAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ToArrayAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.ToArrayAsync line 154"
			]);
		}

		[TestMethod]
		public void ToHashSetAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ToHashSetAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.ToHashSetAsync line 166"
			]);
		}

		[TestMethod]
		public void ToDictionaryAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ToDictionaryAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, FirstName } in method Samples.Projection.ToDictionaryAsync line 178"
			]);
		}

		[TestMethod]
		public void FirstAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.FirstAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.FirstAsync line 190"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.FirstOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.FirstOrDefaultAsync line 199"
			]);
		}

		[TestMethod]
		public void LastAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.LastAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.LastAsync line 209"
			]);
		}

		[TestMethod]
		public void LastOrDefaultAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.LastOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.LastOrDefaultAsync line 218"
			]);
		}

		[TestMethod]
		public void SingleAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.SingleAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.SingleAsync line 227"
			]);
		}

		[TestMethod]
		public void SingleOrDefaultAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.SingleOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.SingleOrDefaultAsync line 236"
			]);
		}

		[TestMethod]
		public void AsAsyncEnumerable()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.AsAsyncEnumerable));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.Projection.AsAsyncEnumerable line 246"
			]);
		}

		[TestMethod]
		public void ReadForUpdate_ChangeProperty()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ReadForUpdate_ChangeProperty));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_ChangeNestedProperty()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ReadForUpdate_ChangeNestedProperty));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionAdd()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ReadForUpdate_CollectionAdd));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionRemove()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ReadForUpdate_CollectionRemove));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionIterate()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.ReadForUpdate_CollectionIterate));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void LambdaRead()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.LambdaRead));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name, User.FirstName, User.LastName } in method Samples.Projection.LambdaRead line 330"
			]);
		}

		[TestMethod]
		public void LambdaWrite()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Projection), nameof(Samples.Projection.LambdaWrite));

			ctx.Output.Should().BeEmpty();
		}
	}
}
