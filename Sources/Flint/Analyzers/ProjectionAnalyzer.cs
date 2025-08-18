using Flint.Vm;
using Mono.Cecil;

namespace Flint.Analyzers
{
	internal class ProjectionAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, ModuleDefinition asm, string className = null, string methodName = null)
		{
			foreach (var mtd in GetMethods(asm, className, methodName))
			{
				Analyze(mtd);
			}
		}
		#endregion

		#region Implementation
		private static IEnumerable<MethodDefinition> GetMethods(ModuleDefinition asm, string className = null, string methodName = null)
		{
			foreach (var type in asm.Types)
			{
				if (className != null && type.Name != className)
					continue;

				foreach (var mtd in type.Methods)
				{
					if (methodName != null && mtd.Name != methodName)
						continue;

					yield return mtd;
				}
			}
		}

		private static void Analyze(MethodDefinition mtd)
		{
			var ast = EvalMachine.Run(mtd);

			// find roots (methods that unwrap IQueryable and make an query to db)
		}
		#endregion
	}
}
