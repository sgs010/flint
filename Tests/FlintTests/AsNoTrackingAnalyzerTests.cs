using Flint.Analyzers;
using FluentAssertions;
using Mono.Cecil;

namespace FlintTests
{
	[TestClass]
	public class AsNoTrackingAnalyzerTests
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

			AsNoTrackingAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.AsNoTrackingSamples), nameof(Samples.AsNoTrackingSamples.Read));

			ctx.Output.Should().BeEquivalentTo([
				"add AsNoTracking() in method Samples.AsNoTrackingSamples.Read line 14"
			]);
		}

		[TestMethod]
		public void Update()
		{
			using var asm = LoadSamples();
			var ctx = new AnalyzerContextMock();
			var entityTypes = EntityAnalyzer.GetEntityTypes(asm);

			AsNoTrackingAnalyzer.Run(ctx, asm, entityTypes, nameof(Samples.AsNoTrackingSamples), nameof(Samples.AsNoTrackingSamples.Update));

			ctx.Output.Should().BeEmpty();
		}
	}
}
