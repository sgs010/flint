using Flint.Analyzers;

namespace FlintTests.FlintCore
{
	[TestClass]
	public class IndexAnalyzerTests
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
		public void OrderBy()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.OrderBy));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on table Users for query in method Samples.IndexSamples.OrderBy line 12"
			]);
		}

		[TestMethod]
		public void Where()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.Where));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on table Users for query in method Samples.IndexSamples.Where line 26"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.FirstOrDefaultAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on table Users for query in method Samples.IndexSamples.FirstOrDefaultAsync line 40"
			]);
		}

		[TestMethod]
		public void PrimaryKeyDefault()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.PrimaryKeyDefault));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void PrimaryKeyFromKeyAttribute()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.PrimaryKeyFromKeyAttribute));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void PrimaryKeyFromPrimaryKeyAttribute()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.PrimaryKeyFromPrimaryKeyAttribute));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void PrimaryKeyFromFluent()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.PrimaryKeyFromFluent));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void Mixed()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.Mixed));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on table Users for query in method Samples.IndexSamples.Mixed line 96",
				"consider adding index (Name) on table Products for query in method Samples.IndexSamples.Mixed line 98"
			]);
		}

		[TestMethod]
		public void NestedChain()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.NestedChain));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Price) on table Products for query in method Samples.IndexSamples.NestedChain line 107"
			]);
		}

		[TestMethod]
		public void NestedAny()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.NestedAny));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Total) on table OrderItem for query in method Samples.IndexSamples.NestedAny line 116"
			]);
		}
	}
}
