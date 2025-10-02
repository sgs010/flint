using Flint.Analyzers;
using FluentAssertions;

namespace FlintTests
{
	[TestClass]
	public class AsSplitQueryAnalyzerTests
	{
		private static AssemblyDefinition ASM;

		[ClassInitialize]
		public static void Setup(TestContext ctx)
		{
			ASM = AssemblyAnalyzer.Load("Samples.dll");
		}

		[ClassCleanup(ClassCleanupBehavior.EndOfClass)]
		public static void Cleanup()
		{
			ASM.Dispose();
			ASM = null;
		}

		[TestMethod]
		public void MultipleChains()
		{
			var ctx = new AnalyzerContextMock();

			AsSplitQueryAnalyzer.Run(ctx, ASM, nameof(Samples.AsSplitQuerySamples), nameof(Samples.AsSplitQuerySamples.MultipleChains));

			ctx.Output.Should().BeEquivalentTo([
				"consider adding AsSplitQuery() in method Samples.AsSplitQuerySamples.MultipleChains line 12"
			]);
		}

		[TestMethod]
		public void MultipleChainsWithSplit()
		{
			var ctx = new AnalyzerContextMock();

			AsSplitQueryAnalyzer.Run(ctx, ASM, nameof(Samples.AsSplitQuerySamples), nameof(Samples.AsSplitQuerySamples.MultipleChainsWithSplit));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void SingleChain()
		{
			var ctx = new AnalyzerContextMock();

			AsSplitQueryAnalyzer.Run(ctx, ASM, nameof(Samples.AsSplitQuerySamples), nameof(Samples.AsSplitQuerySamples.SingleChain));

			ctx.Output.Should().BeEmpty();
		}
	}
}
