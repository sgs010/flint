using Flint.Analyzers;

namespace FlintTests
{
	[TestClass]
	public class OutboxAnalyzerTests
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
		public void NoOutbox()
		{
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.NoOutbox));

			ctx.Output.AssertSame([
				"consider using Outbox pattern in method Samples.OutboxSamples.NoOutbox line 18"
			]);
		}

		[TestMethod]
		public void DelayedOutbox()
		{
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.DelayedOutbox));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void ImmediateOutbox()
		{
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.ImmediateOutbox));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void Services()
		{
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.Services));

			ctx.Output.AssertSame([
				"consider using Outbox pattern in method Samples.OutboxSamples.Services line 63"
			]);
		}

		[TestMethod]
		public void ProcessOutbox()
		{
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.ProcessOutbox));

			ctx.Output.AssertEmpty();
		}
	}
}
