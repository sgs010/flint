using Flint.Analyzers;

namespace FlintTests.FlintCore
{
	[TestClass]
	public class AssemblyAnalyzerTests
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
		public void EntityTypes()
		{
			ASM.EntityTypes.AssertContains(x => x.FullName == "Samples.User");
			ASM.EntityTypes.AssertContains(x => x.FullName == "Samples.Order");
			ASM.EntityTypes.AssertContains(x => x.FullName == "Samples.Product");
		}

		[TestMethod]
		public void EntityCollections()
		{
			ASM.EntityCollections.AssertContains(x => x.Name == "Users");
			ASM.EntityCollections.AssertContains(x => x.Name == "Orders");
			ASM.EntityCollections.AssertContains(x => x.Name == "Products");
		}

		[TestMethod]
		public void InterfaceImplementations()
		{
			ASM.InterfaceImplementations
				.Where(x => x.Key.FullName == "Samples.IRepository")
				.SelectMany(x => x.Value)
				.Where(x => x.FullName == "Samples.Repository")
				.AssertNotEmpty();
		}
	}
}
