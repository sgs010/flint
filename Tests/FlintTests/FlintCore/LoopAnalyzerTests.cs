using Flint.Analyzers;

namespace FlintTests.FlintCore
{
	[TestClass]
	public class LoopAnalyzerTests
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
		public void Loop()
		{
			var ctx = new AnalyzerContext();

			LoopAnalyzer.Run(ctx, ASM, nameof(Samples.LoopSamples), nameof(Samples.LoopSamples.Loop));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"avoid making queries in a loop in method Samples.LoopSamples.Loop line 14"
			]);
		}

		[TestMethod]
		public void NoLoop()
		{
			var ctx = new AnalyzerContext();

			LoopAnalyzer.Run(ctx, ASM, nameof(Samples.LoopSamples), nameof(Samples.LoopSamples.NoLoop));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void NotInLoop()
		{
			var ctx = new AnalyzerContext();

			LoopAnalyzer.Run(ctx, ASM, nameof(Samples.LoopSamples), nameof(Samples.LoopSamples.NotInLoop));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void Mixed()
		{
			var ctx = new AnalyzerContext();

			LoopAnalyzer.Run(ctx, ASM, nameof(Samples.LoopSamples), nameof(Samples.LoopSamples.Mixed));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"avoid making queries in a loop in method Samples.LoopSamples.Mixed line 51",
				"avoid making queries in a loop in method Samples.LoopSamples.Mixed line 60"
			]);
		}
	}
}
