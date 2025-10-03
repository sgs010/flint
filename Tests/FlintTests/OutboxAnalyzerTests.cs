using Flint.Analyzers;
using FluentAssertions;

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

			ctx.Output.Should().BeEquivalentTo([
				"consider using Outbox pattern in method Samples.OutboxSamples.NoOutbox line 16"
			]);
		}

		[TestMethod]
		public void DelayedOutbox()
		{
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.DelayedOutbox));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ImmediateOutbox()
		{
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.ImmediateOutbox));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void Services()
		{
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, ASM, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.Services));

			ctx.Output.Should().BeEquivalentTo([
				"consider using Outbox pattern in method Samples.OutboxSamples.Services line 64"
			]);
		}
	}
}
