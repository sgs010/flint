using System.Text;
using Flint.Vm;
using Match = Flint.Vm.Match;

namespace Flint.Analyzers
{
	internal class AsSplitQueryAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyInfo asm, string className = null, string methodName = null)
		{
			var queries = QueryAnalyzer.Analyze(asm, className, methodName);
			foreach (var query in queries)
			{
				if (query.Entity.Properties.Length == 0)
					continue; // no properties accessed

				// determine how many child entities are accessed
				var childCount = query.Entity.Properties.Count(x => x.Entity != null);
				if (childCount < 2)
					continue; // advise AsSplitQuery for 2 or more child entities

				// check if AsSplitQuery is already present
				var ok = query.Roots.AnyMatch(
					new Match.Call(null, "Microsoft.EntityFrameworkCore.RelationalQueryableExtensions.AsSplitQuery", Match.Any.Args),
					true);
				if (ok)
					continue; // AsSplitQuery is present

				// report issue
				var sb = new StringBuilder();
				sb.Append("consider adding AsSplitQuery() in method ");
				MethodAnalyzer.PrettyPrintMethod(sb, query.Method, query.CilPoint);
				ctx.Log(sb.ToString());
			}
		}
		#endregion
	}
}
