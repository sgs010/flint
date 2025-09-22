using Flint.Analyzers;
using FluentAssertions;
using Mono.Cecil;

namespace FlintTests
{
	[TestClass]
	public class TrackingAnalyzerTests
	{
		private static ModuleDefinition LoadSamples()
		{
			return ModuleDefinition.ReadModule("Samples.dll", new ReaderParameters { ReadSymbols = true });
		}

		[TestMethod]
		public void Read()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			TrackingAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Tracking), nameof(Samples.Tracking.Read));

			ctx.Output.Should().BeEquivalentTo([
				"add AsNoTracking() in method Samples.Tracking.Read line 14"
			]);
		}

		[TestMethod]
		public void Update()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			TrackingAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.Tracking), nameof(Samples.Tracking.Update));

			ctx.Output.Should().BeEmpty();
		}
	}
}
