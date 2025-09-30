using Flint.Analyzers;
using FluentAssertions;

namespace FlintTests
{
	[TestClass]
	public class AsNoTrackingAnalyzerTests
	{
		[TestMethod]
		public void Read_NoAsNoTracking()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			AsNoTrackingAnalyzer.Run(ctx, asm, nameof(Samples.AsNoTrackingSamples), nameof(Samples.AsNoTrackingSamples.Read_NoAsNoTracking));

			ctx.Output.Should().BeEquivalentTo([
				"add AsNoTracking() in method Samples.AsNoTrackingSamples.Read_NoAsNoTracking line 14"
			]);
		}

		[TestMethod]
		public void Read_HasAsNoTracking()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			AsNoTrackingAnalyzer.Run(ctx, asm, nameof(Samples.AsNoTrackingSamples), nameof(Samples.AsNoTrackingSamples.Read_HasAsNoTracking));

			ctx.Output.Should().BeEmpty();
		}

		[TestMethod]
		public void Update()
		{
			using var asm = AssemblyAnalyzer.Load("Samples.dll");
			var ctx = new AnalyzerContextMock();

			AsNoTrackingAnalyzer.Run(ctx, asm, nameof(Samples.AsNoTrackingSamples), nameof(Samples.AsNoTrackingSamples.Update));

			ctx.Output.Should().BeEmpty();
		}
	}
}
