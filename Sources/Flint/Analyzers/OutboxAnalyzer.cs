using System.Text;
using Flint.Vm;
using Mono.Cecil;
using Cil = Flint.Vm.Cil;
using Match = Flint.Vm.Match;

namespace Flint.Analyzers
{
	internal class OutboxAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, ModuleDefinition asm, HashSet<TypeReference> entityTypes, string className = null, string methodName = null)
		{
			foreach (var method in MethodAnalyzer.GetMethods(asm, className, methodName))
			{
				Analyze(ctx, method, entityTypes);
			}
		}
		#endregion

		#region Implementation
		private static void Analyze(IAnalyzerContext ctx, MethodDefinition mtd, HashSet<TypeReference> entityTypes)
		{
			// general idea:
			// 1. look for direct or nested call of Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync
			// 2. same for Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync
			// 3. look for writing an entity with a word "Outbox" in the name
			// if 1,2 are true and 3 is false - suggest Outbox pattern

			var expressions = MethodAnalyzer.EvalRecursive(mtd);
		}
		#endregion
	}
}
