using System.Text;
using Flint.Vm;

namespace Flint.Analyzers
{
	internal class AsNoTrackingAnalyzer
	{
		#region Properties
		public const int Code = 3;
		#endregion

		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyInfo asm, string className = null, string methodName = null)
		{
			var queries = QueryAnalyzer.Analyze(asm, className, methodName);
			foreach (var query in queries)
			{
				if (QueryAnalyzer.SomePropertiesAreChanged(query.Entity))
					continue; // entity is changed, do not advise to add AsNoTracking

				var hasAsNoTracking = query.Roots.OfCall("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking").Any();
				if (hasAsNoTracking)
					continue; // AsNoTracking is already present

				// report issue
				ctx.AddResult(Code, "add AsNoTracking()", query.Method, query.CilPoint);
			}
		}
		#endregion
	}
}
