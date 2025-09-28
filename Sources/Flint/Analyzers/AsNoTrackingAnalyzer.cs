using System.Text;
using Flint.Vm;
using Mono.Cecil;

namespace Flint.Analyzers
{
	internal class AsNoTrackingAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, ModuleDefinition asm, HashSet<TypeReference> entityTypes, string className = null, string methodName = null)
		{
			var entities = EntityAnalyzer.Analyze(asm, entityTypes, className, methodName);
			foreach (var entity in entities)
			{
				if (EntityAnalyzer.SomePropertiesAreChanged(entity))
					continue; // entity is changed, do not advise to add AsNoTracking

				var hasAsNoTracking = entity.Root.OfCall("Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking").Any();
				if (hasAsNoTracking)
					continue; // AsNoTracking is already present

				// report issue
				var sb = new StringBuilder();
				sb.Append("add AsNoTracking() in method ");
				EntityAnalyzer.PrettyPrintMethod(sb, entity.Method, entity.Root.SequencePoint);
				ctx.Log(sb.ToString());
			}
		}
		#endregion
	}
}
