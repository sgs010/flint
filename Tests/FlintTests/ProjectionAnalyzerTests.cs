using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Rocks;

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

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadWholeObject));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadAllProperties()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadAllProperties));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadSomeProperties()
		{
			//var p = new ReaderParameters();
			//p.ReadSymbols = true;

			//using var x = AssemblyDefinition.ReadAssembly(GetType().Assembly.Location, p);
			//foreach (var m in x.MainModule.GetAllTypes().SelectMany(t => t.Methods).Where(m => m.DebugInformation != null && m.DebugInformation.HasSequencePoints))
			//{
			//	foreach (var d in m.DebugInformation.GetSequencePointMapping())
			//	{
			//		System.Console.WriteLine($"{d.Key} => {d.Value.Document.Url} ({d.Value.StartLine},{d.Value.StartColumn})");
			//	}
			//}



			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadSomeProperties));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.ReadSomeProperties()"
			]);
		}

		[TestMethod]
		public void MultipleQueries()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.MultipleQueries));

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

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ComplexProjection));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Number, Items = { Product.Name } } in method Samples.Projections.ComplexProjection()",
			]);
		}

		[TestMethod]
		public void ReadAllChainedProperties()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadAllChainedProperties));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void SimpleCRUD()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.SimpleCRUD));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name, Price } in method Samples.Projections.SimpleCRUD()",
			]);
		}

		[TestMethod]
		public void ToListAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ToListAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToListAsync()"
			]);
		}

		[TestMethod]
		public void ToArrayAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ToArrayAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToArrayAsync()"
			]);
		}

		[TestMethod]
		public void ToHashSetAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ToHashSetAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.ToHashSetAsync()"
			]);
		}

		[TestMethod]
		public void ToDictionaryAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ToDictionaryAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Id, Name } in method Samples.Projections.ToDictionaryAsync()"
			]);
		}

		[TestMethod]
		public void FirstAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.FirstAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.FirstAsync()"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.FirstOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.FirstOrDefaultAsync()"
			]);
		}

		[TestMethod]
		public void LastAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.LastAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.LastAsync()"
			]);
		}

		[TestMethod]
		public void LastOrDefaultAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.LastOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.LastOrDefaultAsync()"
			]);
		}

		[TestMethod]
		public void SingleAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.SingleAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.SingleAsync()"
			]);
		}

		[TestMethod]
		public void SingleOrDefaultAsync()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.SingleOrDefaultAsync));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.SingleOrDefaultAsync()"
			]);
		}

		[TestMethod]
		public void AsAsyncEnumerable()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.AsAsyncEnumerable));

			ctx.Output.Should().BeEquivalentTo([
				"consider using projection { Name } in method Samples.Projections.AsAsyncEnumerable()"
			]);
		}

		[TestMethod]
		public void ReadForUpdate_ChangeProperty()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_ChangeProperty));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_ChangeNestedProperty()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_ChangeNestedProperty));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionAdd()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_CollectionAdd));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionRemove()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_CollectionRemove));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionIterate()
		{
			using var asm = ModuleDefinition.ReadModule("Samples.dll");
			var ctx = new AnalyzerContextMock();

			Flint.Analyzers.ProjectionAnalyzer.Run(ctx, asm, nameof(Samples.Projections), nameof(Samples.Projections.ReadForUpdate_CollectionIterate));

			ctx.Output.Should().BeEmpty();
		}
	}
}
