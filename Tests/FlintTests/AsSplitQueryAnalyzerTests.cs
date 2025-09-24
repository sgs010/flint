using Flint.Analyzers;
using FluentAssertions;
using Mono.Cecil;

namespace FlintTests
{
	[TestClass]
	public class AsSplitQueryAnalyzerTests
	{
		private static ModuleDefinition LoadSamples()
		{
			return ModuleDefinition.ReadModule("Samples.dll", new ReaderParameters { ReadSymbols = true });
		}

		[TestMethod]
		public void MultipleChains()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			AsSplitQueryAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.AsSplitQuery), nameof(Samples.AsSplitQuery.MultipleChains));

			ctx.Output.Should().BeEquivalentTo([
				"consider adding AsSplitQuery() in method Samples.AsSplitQuery.MultipleChains line 12"
			]);
		}

		[TestMethod]
		public void MultipleChainsWithSplit()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			AsSplitQueryAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.AsSplitQuery), nameof(Samples.AsSplitQuery.MultipleChainsWithSplit));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void SingleChain()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			AsSplitQueryAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.AsSplitQuery), nameof(Samples.AsSplitQuery.SingleChain));

			ctx.Output.Should().BeEmpty();
		}
	}
}
