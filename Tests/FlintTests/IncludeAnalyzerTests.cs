using Flint.Analyzers;
using FluentAssertions;
using Mono.Cecil;

namespace FlintTests
{
	[TestClass]
	public class IncludeAnalyzerTests
	{
		private static ModuleDefinition LoadSamples()
		{
			return ModuleDefinition.ReadModule("Samples.dll", new ReaderParameters { ReadSymbols = true });
		}

		[TestMethod]
		public void Lambda_NestedEntity()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			IncludeAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Include), nameof(Samples.Include.Lambda_NestedEntity));

			ctx.Output.Should().BeEquivalentTo([
				"add Include(t => t.User) in method Samples.Include.Lambda_NestedEntity line 16"
			]);
		}

		[TestMethod]
		public void Lambda_NoNestedEntities()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			IncludeAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Include), nameof(Samples.Include.Lambda_NoNestedEntities));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ChainedEntities_NoInclude()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			IncludeAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Include), nameof(Samples.Include.ChainedEntities_NoInclude));

			ctx.Output.Should().BeEquivalentTo([
				"add Include(o => o.Items).ThenInclude(oi => oi.Product) in method Samples.Include.ChainedEntities_NoInclude line 47"
			]);
		}

		[TestMethod]
		public void ChainedEntities_FullInclude()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			IncludeAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Include), nameof(Samples.Include.ChainedEntities_FullInclude));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ChainedEntities_PartialInclude()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			IncludeAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Include), nameof(Samples.Include.ChainedEntities_PartialInclude));

			ctx.Output.Should().BeEquivalentTo([
				"add ThenInclude(oi => oi.Product) in method Samples.Include.ChainedEntities_PartialInclude line 77"
			]);
		}

		[TestMethod]
		public void MultipleChains()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			IncludeAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Include), nameof(Samples.Include.MultipleChains));

			ctx.Output.Should().BeEquivalentTo([
				"add Include(b => b.Posts).ThenInclude(p => p.Author) in method Samples.Include.MultipleChains line 93",
				"add Include(b => b.Tags) in method Samples.Include.MultipleChains line 93"
			]);
		}
	}
}
