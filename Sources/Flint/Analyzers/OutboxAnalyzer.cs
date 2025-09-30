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
		public static void Run(IAnalyzerContext ctx, AssemblyDefinition asm, string className = null, string methodName = null)
		{
			foreach (var method in MethodAnalyzer.GetMethods(asm.Module, className, methodName))
			{
				Analyze(ctx, asm, method);
			}
		}
		#endregion

		#region Implementation
		private static void Analyze(IAnalyzerContext ctx, AssemblyDefinition asm, MethodDefinition mtd)
		{
			// general idea:
			// 1. look for direct or nested call of Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync
			// 2. same for Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync
			// 3. look for writing an entity with a word "Outbox" in the name
			// if 1,2 are true and 3 is false - suggest Outbox pattern

			var expressions = MethodAnalyzer.EvalRecursive(asm, mtd);

			var hasSaveChangesAsync = expressions.OfCall("Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync").Any();
			if (hasSaveChangesAsync == false)
				return;

			var hasSendMessageAsync = expressions.OfCall("Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync").Any();
			if (hasSendMessageAsync == false)
				return;

			//var entities = EntityAnalyzer.Analyze(asm, expressions);
		}
		#endregion
	}
}
