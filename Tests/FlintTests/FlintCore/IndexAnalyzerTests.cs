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
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.OrderBy line 12"
			]);
		}

		[TestMethod]
		public void Where()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.Where));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.Where line 26"
			]);
		}

		[TestMethod]
		public void NestedWhere()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.NestedWhere));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (FirstName,Email) on entity Samples.User for the query in method Samples.IndexSamples.NestedWhere line 40"
			]);
		}

		[TestMethod]
		public void FirstAsync()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.FirstAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.FirstAsync line 55"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.FirstOrDefaultAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.FirstOrDefaultAsync line 66"
			]);
		}

		[TestMethod]
		public void LastAsync()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.LastAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.LastAsync line 77"
			]);
		}

		[TestMethod]
		public void LastOrDefaultAsync()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.LastOrDefaultAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.LastOrDefaultAsync line 88"
			]);
		}

		[TestMethod]
		public void SingleAsync()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.SingleAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.SingleAsync line 99"
			]);
		}

		[TestMethod]
		public void SingleOrDefaultAsync()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.SingleOrDefaultAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.SingleOrDefaultAsync line 110"
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
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.Mixed line 166",
				"consider adding index (Name) on entity Samples.Product for the query in method Samples.IndexSamples.Mixed line 168"
			]);
		}

		[TestMethod]
		public void NestedChain()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.NestedChain));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Price) on entity Samples.Product for the query in method Samples.IndexSamples.NestedChain line 177"
			]);
		}

		[TestMethod]
		public void NestedAny()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.NestedAny));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Total) on entity Samples.OrderItem for the query in method Samples.IndexSamples.NestedAny line 186"
			]);
		}

		[TestMethod]
		public void MixedWhere()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.MixedWhere));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (IsCompleted) on entity Samples.Todo for the query in method Samples.IndexSamples.MixedWhere line 196",
				"consider adding index (FirstName) on entity Samples.User for the query in method Samples.IndexSamples.MixedWhere line 196"
			]);
		}

		[TestMethod]
		public void OrderByDescending()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.OrderByDescending));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (Email) on entity Samples.User for the query in method Samples.IndexSamples.OrderByDescending line 210"
			]);
		}

		[TestMethod]
		public void FilteredInclude()
		{
			var ctx = new AnalyzerContext();

			IndexAnalyzer.Run(ctx, ASM, nameof(Samples.IndexSamples), nameof(Samples.IndexSamples.FilteredInclude));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider adding index (CreatedDate,TotalAmount) on entity Samples.Order2 for the query in method Samples.IndexSamples.FilteredInclude line 224"
			]);
		}
	}
}
