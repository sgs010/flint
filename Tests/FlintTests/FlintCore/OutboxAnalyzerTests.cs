using Flint.Analyzers;

namespace FlintTests.FlintCore
{
	[TestClass]
	public class OutboxAnalyzerTests
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
		public void NoOutbox()
		{
			var ctx = new AnalyzerContext();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.NoOutbox));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using Outbox pattern in method Samples.OutboxSamples.NoOutbox line 18"
			]);
		}

		[TestMethod]
		public void DelayedOutbox()
		{
			var ctx = new AnalyzerContext();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.DelayedOutbox));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void ImmediateOutbox()
		{
			var ctx = new AnalyzerContext();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.ImmediateOutbox));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void Services()
		{
			var ctx = new AnalyzerContext();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.Services));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using Outbox pattern in method Samples.OutboxSamples.Services line 63"
			]);
		}

		[TestMethod]
		public void ProcessOutbox()
		{
			var ctx = new AnalyzerContext();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.ProcessOutbox));

			ctx.Result.AssertEmpty();
		}
	}
}
