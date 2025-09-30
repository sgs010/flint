using Flint.Analyzers;
using FluentAssertions;

namespace FlintTests
{
	[TestClass]
	public class AsSplitQueryAnalyzerTests
	{
		[TestMethod]
		public void MultipleChains()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			AsSplitQueryAnalyzer.Run(ctx, asm, nameof(Samples.AsSplitQuerySamples), nameof(Samples.AsSplitQuerySamples.MultipleChains));

			ctx.Output.Should().BeEquivalentTo([
				"consider adding AsSplitQuery() in method Samples.AsSplitQuerySamples.MultipleChains line 12"
			]);
		}

		[TestMethod]
		public void MultipleChainsWithSplit()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			AsSplitQueryAnalyzer.Run(ctx, asm, nameof(Samples.AsSplitQuerySamples), nameof(Samples.AsSplitQuerySamples.MultipleChainsWithSplit));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void SingleChain()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			AsSplitQueryAnalyzer.Run(ctx, asm, nameof(Samples.AsSplitQuerySamples), nameof(Samples.AsSplitQuerySamples.SingleChain));

			ctx.Output.Should().BeEmpty();
		}
	}
}
