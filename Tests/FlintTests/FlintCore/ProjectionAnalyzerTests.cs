using Flint.Analyzers;

namespace FlintTests.FlintCore
{
	[TestClass]
	public class ProjectionAnalyzerTests
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
		public void ReadWholeObject()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadWholeObject));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void ReadAllProperties()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadAllProperties));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void ReadSomeProperties()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadSomeProperties));

			ctx.Output.AssertSame([
				"consider using projection { Id, FirstName } in method Samples.ProjectionSamples.ReadSomeProperties line 36"
			]);
		}

		[TestMethod]
		public void MultipleQueries()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.MultipleQueries));

			ctx.Output.AssertSame([
				"consider using projection { Id, FirstName } in method Samples.ProjectionSamples.MultipleQueries line 49",
				"consider using projection { Id, Email } in method Samples.ProjectionSamples.MultipleQueries line 55",
			]);
		}

		[TestMethod]
		public void ComplexProjection()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ComplexProjection));

			ctx.Output.AssertSame([
				"consider using projection { Number, Items = { Product.Name } } in method Samples.ProjectionSamples.ComplexProjection line 68",
			]);
		}

		[TestMethod]
		public void ReadAllChainedProperties()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadAllChainedProperties));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void SimpleCRUD()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.SimpleCRUD));

			ctx.Output.AssertSame([
				"consider using projection { Name, Price } in method Samples.ProjectionSamples.SimpleCRUD line 111",
			]);
		}

		[TestMethod]
		public void ToListAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToListAsync));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.ToListAsync line 142"
			]);
		}

		[TestMethod]
		public void ToArrayAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToArrayAsync));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.ToArrayAsync line 154"
			]);
		}

		[TestMethod]
		public void ToHashSetAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToHashSetAsync));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.ToHashSetAsync line 166"
			]);
		}

		[TestMethod]
		public void ToDictionaryAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ToDictionaryAsync));

			ctx.Output.AssertSame([
				"consider using projection { Id, FirstName } in method Samples.ProjectionSamples.ToDictionaryAsync line 178"
			]);
		}

		[TestMethod]
		public void FirstAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.FirstAsync));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.FirstAsync line 190"
			]);
		}

		[TestMethod]
		public void FirstOrDefaultAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.FirstOrDefaultAsync));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.FirstOrDefaultAsync line 199"
			]);
		}

		[TestMethod]
		public void LastAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LastAsync));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.LastAsync line 209"
			]);
		}

		[TestMethod]
		public void LastOrDefaultAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LastOrDefaultAsync));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.LastOrDefaultAsync line 218"
			]);
		}

		[TestMethod]
		public void SingleAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.SingleAsync));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.SingleAsync line 227"
			]);
		}

		[TestMethod]
		public void SingleOrDefaultAsync()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.SingleOrDefaultAsync));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.SingleOrDefaultAsync line 236"
			]);
		}

		[TestMethod]
		public void AsAsyncEnumerable()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.AsAsyncEnumerable));

			ctx.Output.AssertSame([
				"consider using projection { FirstName } in method Samples.ProjectionSamples.AsAsyncEnumerable line 246"
			]);
		}

		[TestMethod]
		public void ReadForUpdate_ChangeProperty()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_ChangeProperty));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_ChangeNestedProperty()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_ChangeNestedProperty));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionAdd()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_CollectionAdd));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionRemove()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_CollectionRemove));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void ReadForUpdate_CollectionIterate()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.ReadForUpdate_CollectionIterate));

			ctx.Output.AssertEmpty();
		}

		[TestMethod]
		public void LambdaRead()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LambdaRead));

			ctx.Output.AssertSame([
				"consider using projection { Id, Name, User.FirstName, User.LastName } in method Samples.ProjectionSamples.LambdaRead line 330"
			]);
		}

		[TestMethod]
		public void LambdaWrite()
		{
			var ctx = new AnalyzerContextMock();

			ProjectionAnalyzer.Run(ctx, ASM, nameof(Samples.ProjectionSamples), nameof(Samples.ProjectionSamples.LambdaWrite));

			ctx.Output.AssertEmpty();
		}
	}
}
