using Flint.Analyzers;

namespace FlintTests.FlintCore
{
	[TestClass]
	public class ProjectionAnalyzerTests
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
		public void ReadWholeObject()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadWholeObject));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void ReadAllProperties()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadAllProperties));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void ReadSomeProperties()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadSomeProperties));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { Id, FirstName } in method Samples.ProjectionSamples.ReadSomeProperties line 36"
			]);
		}

		[TestMethod]
		public void MultipleQueries()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.MultipleQueries));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { Id, FirstName } in method Samples.ProjectionSamples.MultipleQueries line 49",
				"consider using projection { Id, Email } in method Samples.ProjectionSamples.MultipleQueries line 55",
			]);
		}

		[TestMethod]
		public void ComplexProjection()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ComplexProjection));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { Number, Items = { Product.Name } } in method Samples.ProjectionSamples.ComplexProjection line 68",
			]);
		}

		[TestMethod]
		public void ReadAllChainedProperties()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadAllChainedProperties));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void SimpleCRUD()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.SimpleCRUD));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { Name, Price } in method Samples.ProjectionSamples.SimpleCRUD line 111",
			]);
		}

		[TestMethod]
		public void ToListAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToListAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.ToListAsync line 142"
			]);
		}

		[TestMethod]
		public void ToArrayAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToArrayAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.ToArrayAsync line 154"
			]);
		}

		[TestMethod]
		public void ToHashSetAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToHashSetAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.ToHashSetAsync line 166"
			]);
		}

		[TestMethod]
		public void ToDictionaryAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToDictionaryAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { Id, FirstName } in method Samples.ProjectionSamples.ToDictionaryAsync line 178"
			]);
		}

		[TestMethod]
		public void FirstAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.FirstAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.FirstAsync line 190"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.FirstOrDefaultAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.FirstOrDefaultAsync line 199"
			]);
		}

		[TestMethod]
		public void LastAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LastAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.LastAsync line 209"
			]);
		}

		[TestMethod]
		public void LastOrDefaultAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LastOrDefaultAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.LastOrDefaultAsync line 218"
			]);
		}

		[TestMethod]
		public void SingleAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.SingleAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.SingleAsync line 227"
			]);
		}

		[TestMethod]
		public void SingleOrDefaultAsync()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.SingleOrDefaultAsync));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.SingleOrDefaultAsync line 236"
			]);
		}

		[TestMethod]
		public void AsAsyncEnumerable()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.AsAsyncEnumerable));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.AsAsyncEnumerable line 246"
			]);
		}

		[TestMethod]
		public void ReadForUpdate_ChangeProperty()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_ChangeProperty));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_ChangeNestedProperty()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_ChangeNestedProperty));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionAdd()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_CollectionAdd));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionRemove()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_CollectionRemove));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionIterate()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_CollectionIterate));

			ctx.Result.AssertEmpty();
		}

		[TestMethod]
		public void LambdaRead()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LambdaRead));

			Flint.Api.PrettyPrint(ctx.Result).AssertSame([
				"consider using projection { Id, Name, User.FirstName, User.LastName } in method Samples.ProjectionSamples.LambdaRead line 330"
			]);
		}

		[TestMethod]
		public void LambdaWrite()
		{
			var ctx = new AnalyzerContext();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LambdaWrite));

			ctx.Result.AssertEmpty();
		}
	}
}
