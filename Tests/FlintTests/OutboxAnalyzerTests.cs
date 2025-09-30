using Flint.Analyzers;
using FluentAssertions;

namespace FlintTests
{
	[TestClass]
	public class OutboxAnalyzerTests
	{
		[TestMethod]
		public void NoOutbox()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, asm, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.NoOutbox));

			ctx.Output.Should().BeEquivalentTo([
				"consider using Outbox pattern in method Samples.OutboxSamples.NoOutbox line 16"
			]);
		}

		[TestMethod]
		public void DelayedOutbox()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, asm, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.DelayedOutbox));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ImmediateOutbox()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			OutboxAnalyzer.Run(ctx, asm, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.ImmediateOutbox));

			ctx.Output.Should().BeEmpty();
		}
	}
}
