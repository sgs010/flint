using Flint.Analyzers;
using FluentAssertions;
using Mono.Cecil;

namespace FlintTests
{
	[TestClass]
	public class OutboxAnalyzerTests
	{
		private static ModuleDefinition LoadSamples()
		{
			return ModuleDefinition.ReadModule("Samples.dll", new ReaderParameters { ReadSymbols = true });
		}

		[TestMethod]
		public void NoOutbox()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			OutboxAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.NoOutbox));

			ctx.Output.Should().BeEquivalentTo([
				"consider using Outbox pattern in method Samples.OutboxSamples.NoOutbox line 16"
			]);
		}

		[TestMethod]
		public void DelayedOutbox()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			OutboxAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.DelayedOutbox));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void ImmediateOutbox()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			OutboxAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.OutboxSamples), nameof(Samples.OutboxSamples.ImmediateOutbox));

			ctx.Output.Should().BeEmpty();
		}
	}
}
