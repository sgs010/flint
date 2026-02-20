using Flint.Common;
using Flint.Vm;
using Mono.Cecil;

namespace Flint.Analyzers
{
	internal class IndexAnalyzer
	{
		#region Properties
		public const int Code = 7;
		#endregion

		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyInfo asm, string className = null, string methodName = null)
		{
			var queries = QueryAnalyzer.Analyze(asm, className, methodName);
			foreach (var query in queries)
			{
				// collect all properties referenced in linq expressions
				var filterProps = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
				foreach (var root in query.Roots)
					foreach (var filter in root.OfCall(asm.LinqFilters))
						foreach (var expr in filter.OfCall(asm.LinqExpressions))
							foreach (var mtd in expr.OfMethodof(asm.EntityGetSetMethods))
								filterProps.Add(mtd.Method);
			}
		}
		#endregion
	}
}
