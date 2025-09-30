using System.Text;
using Match = Flint.Vm.Match;

namespace Flint.Analyzers
{
	internal class AsSplitQueryAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyDefinition asm, string className = null, string methodName = null)
		{
			var entities = EntityAnalyzer.Analyze(asm, className, methodName);
			foreach (var entity in entities)
			{
				if (entity.Properties.Length == 0)
					continue; // no properties accessed

				// determine how many child entities are accessed
				var childCount = entity.Properties.Count(x => x.Entity != null);
				if (childCount < 2)
					continue; // advise AsSplitQuery for 2 or more child entities

				// check if AsSplitQuery is already present
				var (_, ok) = entity.Root.Match(
					new Match.Call(null, "Microsoft.EntityFrameworkCore.RelationalQueryableExtensions.AsSplitQuery", Match.Any.Args),
					true);
				if (ok)
					continue; // AsSplitQuery is present

				// report issue
				var sb = new StringBuilder();
				sb.Append("consider adding AsSplitQuery() in method ");
				MethodAnalyzer.PrettyPrintMethod(sb, entity.Method, entity.Root.SequencePoint);
				ctx.Log(sb.ToString());
			}
		}
		#endregion
	}
}
