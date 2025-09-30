using System.Text;
using Flint.Vm;

namespace Flint.Analyzers
{
	internal class AsNoTrackingAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyDefinition asm, string className = null, string methodName = null)
		{
			var entities = EntityAnalyzer.Analyze(asm, className, methodName);
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
				MethodAnalyzer.PrettyPrintMethod(sb, entity.Method, entity.Root.SequencePoint);
				ctx.Log(sb.ToString());
			}
		}
		#endregion
	}
}
