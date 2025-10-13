using Flint.Analyzers;

namespace FlintTests.FlintCore
{
	[TestClass]
	public class AsNoTrackingAnalyzerTests
	{
		private static AssemblyInfo ASM;

		[ClassInitialize]
		public static void Setup(TestContext ctx)
		{
			ASM = AssemblyAnalyzer.Load(new AnalyzerContext(), "Samples.dll");
		}

		[ClassCleanup(ClassCleanupBehavior.EndOfClass)]
		public static void Cleanup()
		{
			ASM.Dispose();
			ASM = null;
		}

		[TestMethod]
		public void Read_NoAsNoTracking()
		{
			var ctx = new AnalyzerContext();

			AsNoTrackingAnalyzer.Run(ctx, ASM, nameof(Samples.AsNoTrackingSamples), nameof(Samples.AsNoTrackingSamples.Read_NoAsNoTracking));

			ctx.Output.AssertSame([
				"add AsNoTracking() in method Samples.AsNoTrackingSamples.Read_NoAsNoTracking line 14"
			]);
		}

		[TestMethod]
		public void Read_HasAsNoTracking()
		{
			var ctx = new AnalyzerContext();

			AsNoTrackingAnalyzer.Run(ctx, ASM, nameof(Samples.AsNoTrackingSamples), nameof(Samples.AsNoTrackingSamples.Read_HasAsNoTracking));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void Update()
		{
			var ctx = new AnalyzerContext();

			AsNoTrackingAnalyzer.Run(ctx, ASM, nameof(Samples.AsNoTrackingSamples), nameof(Samples.AsNoTrackingSamples.Update));

			ctx.Output.AssertEmpty();
		}
	}
}
