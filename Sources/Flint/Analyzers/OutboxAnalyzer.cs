using System.Collections.Frozen;
using System.Text;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;

namespace Flint.Analyzers
{
	internal class OutboxAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyDefinition asm, string className = null, string methodName = null)
		{
			foreach (var method in MethodAnalyzer.GetMethods(asm, className, methodName))
			{
				Analyze(ctx, asm, method);
			}
		}
		#endregion

		#region Implementation
		private static void Analyze(IAnalyzerContext ctx, AssemblyDefinition asm, MethodDefinition method)
		{
			// general idea:
			// 1. look for direct or nested call of Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync
			// 2. same for Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync
			// 3. look for adding an entity with a word "Outbox" in the name
			// if 1,2 are true and 3 is false - suggest Outbox pattern

			var saveChangesAsync = MethodAnalyzer.GetCalls(asm, method, "Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync");
			if (saveChangesAsync.Count == 0)
				return; // SaveChangesAsync is not found

			var sendMessageAsync = MethodAnalyzer.GetCalls(asm, method, "Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync");
			if (sendMessageAsync.Count == 0)
				return; // SendMessageAsync is not found


			//var outboxes = asm.EntityTypes.Where(x => x.Name.Contains("Outbox")).ToFrozenSet();
			//foreach (var add in expressions.OfCall("Add"))
			//{
			//	if (add.Method.DeclaringType.IsDbSet(out var _, outboxes))
			//		return; // message added to outbox
			//}

			//// report issue
			//var sb = new StringBuilder();
			//sb.Append("consider using Outbox pattern in method ");
			//MethodAnalyzer.PrettyPrintMethod(sb, method, null);
			//ctx.Log(sb.ToString());
		}

		//private static void Analyze(IAnalyzerContext ctx, AssemblyDefinition asm, MethodDefinition method)
		//{
		//	// general idea:
		//	// 1. look for direct or nested call of Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync
		//	// 2. same for Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync
		//	// 3. look for adding an entity with a word "Outbox" in the name
		//	// if 1,2 are true and 3 is false - suggest Outbox pattern

		//	var expressions = MethodAnalyzer.EvalRecursive(asm, method);

		//	var hasSaveChangesAsync = expressions.OfCall("Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync").Any();
		//	if (hasSaveChangesAsync == false)
		//		return; // SaveChangesAsync is not found

		//	var hasSendMessageAsync = expressions.OfCall("Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync").Any();
		//	if (hasSendMessageAsync == false)
		//		return; // SendMessageAsync is not found

		//	var outboxes = asm.EntityTypes.Where(x => x.Name.Contains("Outbox")).ToFrozenSet();
		//	foreach (var add in expressions.OfCall("Add"))
		//	{
		//		if (add.Method.DeclaringType.IsDbSet(out var _, outboxes))
		//			return; // message added to outbox
		//	}

		//	// report issue
		//	var sb = new StringBuilder();
		//	sb.Append("consider using Outbox pattern in method ");
		//	MethodAnalyzer.PrettyPrintMethod(sb, method, null);
		//	ctx.Log(sb.ToString());
		//}
		#endregion
	}
}
