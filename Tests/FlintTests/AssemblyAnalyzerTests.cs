using Flint.Analyzers;
using FluentAssertions;

namespace FlintTests
{
	[TestClass]
	public class AssemblyAnalyzerTests
	{
		[TestMethod]
		public void Analyze_EntityTypes()
		{
			var asm = AssemblyAnalyzer.Analyze("Samples.dll");

			asm.EntityTypes.Should().Contain(x => x.FullName == "Samples.User");
			asm.EntityTypes.Should().Contain(x => x.FullName == "Samples.Order");
			asm.EntityTypes.Should().Contain(x => x.FullName == "Samples.Product");
		}

		[TestMethod]
		public void Analyze_InterfaceImplementations()
		{
			var asm = AssemblyAnalyzer.Analyze("Samples.dll");

			asm.InterfaceImplementations
				.Where(x => x.Key.FullName == "Samples.IRepository")
				.SelectMany(x => x.Value)
				.Where(x => x.FullName == "Samples.Repository")
				.Should().NotBeEmpty();
		}
	}
}
