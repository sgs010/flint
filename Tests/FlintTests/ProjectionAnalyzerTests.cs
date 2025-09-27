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

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadWholeObject));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadAllProperties()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadAllProperties));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadSomeProperties()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadSomeProperties));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, FirstName } in method Samples.ProjectionSamples.ReadSomeProperties line 36"
			]);
		}

		[TestMethod]
		public void MultipleQueries()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.MultipleQueries));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, FirstName } in method Samples.ProjectionSamples.MultipleQueries line 49",
				"consider using projection { Id, Email } in method Samples.ProjectionSamples.MultipleQueries line 55",
			]);
		}

		[TestMethod]
		public void ComplexProjection()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ComplexProjection));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Number, Items = { Product.Name } } in method Samples.ProjectionSamples.ComplexProjection line 68",
			]);
		}

		[TestMethod]
		public void ReadAllChainedProperties()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadAllChainedProperties));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void SimpleCRUD()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.SimpleCRUD));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name, Price } in method Samples.ProjectionSamples.SimpleCRUD line 111",
			]);
		}

		[TestMethod]
		public void ToListAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToListAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.ToListAsync line 142"
			]);
		}

		[TestMethod]
		public void ToArrayAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToArrayAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.ToArrayAsync line 154"
			]);
		}

		[TestMethod]
		public void ToHashSetAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToHashSetAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.ToHashSetAsync line 166"
			]);
		}

		[TestMethod]
		public void ToDictionaryAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToDictionaryAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, FirstName } in method Samples.ProjectionSamples.ToDictionaryAsync line 178"
			]);
		}

		[TestMethod]
		public void FirstAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.FirstAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.FirstAsync line 190"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.FirstOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.FirstOrDefaultAsync line 199"
			]);
		}

		[TestMethod]
		public void LastAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LastAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.LastAsync line 209"
			]);
		}

		[TestMethod]
		public void LastOrDefaultAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LastOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.LastOrDefaultAsync line 218"
			]);
		}

		[TestMethod]
		public void SingleAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.SingleAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.SingleAsync line 227"
			]);
		}

		[TestMethod]
		public void SingleOrDefaultAsync()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.SingleOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.SingleOrDefaultAsync line 236"
			]);
		}

		[TestMethod]
		public void AsAsyncEnumerable()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.AsAsyncEnumerable));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.AsAsyncEnumerable line 246"
			]);
		}

		[TestMethod]
		public void ReadForUpdate_ChangeProperty()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_ChangeProperty));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_ChangeNestedProperty()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_ChangeNestedProperty));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionAdd()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_CollectionAdd));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionRemove()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_CollectionRemove));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionIterate()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_CollectionIterate));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void LambdaRead()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LambdaRead));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name, User.FirstName, User.LastName } in method Samples.ProjectionSamples.LambdaRead line 330"
			]);
		}

		[TestMethod]
		public void LambdaWrite()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			ProjectionAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LambdaWrite));

			ctx.Output.Should().BeEmpty();
		}
	}
}
