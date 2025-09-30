using Flint.Analyzers;
using FluentAssertions;

namespace FlintTests
{
	[TestClass]
	public class IncludeAnalyzerTests
	{
		[TestMethod]
		public void Lambda_NestedEntity()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			IncludeAnalyzer.Run(ctx, asm, nameof(Samples.IncludeSamples), nameof(Samples.IncludeSamples.Lambda_NestedEntity));

			ctx.Output.Should().BeEquivalentTo([
				"add Include(t => t.User) in method Samples.IncludeSamples.Lambda_NestedEntity line 16"
			]);
		}

		[TestMethod]
		public void Lambda_NoNestedEntities()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			IncludeAnalyzer.Run(ctx, asm, nameof(Samples.IncludeSamples), nameof(Samples.IncludeSamples.Lambda_NoNestedEntities));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ChainedEntities_NoInclude()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			IncludeAnalyzer.Run(ctx, asm, nameof(Samples.IncludeSamples), nameof(Samples.IncludeSamples.ChainedEntities_NoInclude));

			ctx.Output.Should().BeEquivalentTo([
				"add Include(o => o.Items).ThenInclude(oi => oi.Product) in method Samples.IncludeSamples.ChainedEntities_NoInclude line 47"
			]);
		}

		[TestMethod]
		public void ChainedEntities_FullInclude()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			IncludeAnalyzer.Run(ctx, asm, nameof(Samples.IncludeSamples), nameof(Samples.IncludeSamples.ChainedEntities_FullInclude));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ChainedEntities_PartialInclude()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			IncludeAnalyzer.Run(ctx, asm, nameof(Samples.IncludeSamples), nameof(Samples.IncludeSamples.ChainedEntities_PartialInclude));

			ctx.Output.Should().BeEquivalentTo([
				"add ThenInclude(oi => oi.Product) in method Samples.IncludeSamples.ChainedEntities_PartialInclude line 77"
			]);
		}

		[TestMethod]
		public void MultipleChains()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			IncludeAnalyzer.Run(ctx, asm, nameof(Samples.IncludeSamples), nameof(Samples.IncludeSamples.MultipleChains));

			ctx.Output.Should().BeEquivalentTo([
				"add Include(b => b.Posts).ThenInclude(p => p.Author) in method Samples.IncludeSamples.MultipleChains line 93",
				"add Include(b => b.Tags) in method Samples.IncludeSamples.MultipleChains line 93"
			]);
		}
	}
}
