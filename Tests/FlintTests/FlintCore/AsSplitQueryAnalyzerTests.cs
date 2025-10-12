using Flint.Analyzers;

namespace FlintTests.FlintCore
{
	[TestClass]
	public class AsSplitQueryAnalyzerTests
	{
		private static AssemblyInfo ASM;

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
			var ctx = new AnalyzerContext();

			AsSplitQueryAnalyzer.Run(ctx, ASM, nameof(Samples.AsSplitQuerySamples), nameof(Samples.AsSplitQuerySamples.MultipleChains));

			ctx.Output.AssertSame([
				"consider adding AsSplitQuery() in method Samples.AsSplitQuerySamples.MultipleChains line 12"
			]);
		}

		[TestMethod]
		public void MultipleChainsWithSplit()
		{
			var ctx = new AnalyzerContext();

			AsSplitQueryAnalyzer.Run(ctx, ASM, nameof(Samples.AsSplitQuerySamples), nameof(Samples.AsSplitQuerySamples.MultipleChainsWithSplit));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void SingleChain()
		{
			var ctx = new AnalyzerContext();

			AsSplitQueryAnalyzer.Run(ctx, ASM, nameof(Samples.AsSplitQuerySamples), nameof(Samples.AsSplitQuerySamples.SingleChain));

			ctx.Output.AssertEmpty();
		}
	}
}
