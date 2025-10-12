using System.Text;
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

			var saveChangesAsync = MethodAnalyzer.GetCallChains(asm, method, "Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync");
			if (saveChangesAsync.Count == 0)
				return; // SaveChangesAsync is not found

			var sendMessageAsync = MethodAnalyzer.GetCallChains(asm, method, "Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync");
			if (sendMessageAsync.Count == 0)
				return; // SendMessageAsync is not found

			// check if message is added to outbox
			foreach (var outbox in asm.EntityTypes.Where(x => x.Name.Contains("Outbox")))
			{
				var add = MethodAnalyzer.GetCallChains(asm, method, $"Microsoft.EntityFrameworkCore.DbSet`1<{outbox.FullName}>.Add");
				if (add.Count > 0)
					return; // message added to outbox
			}

			// check if only outbox is accessed
			var getEntitiesExceptOutbox = asm.EntityCollections
				.Where(x => x.Name.Contains("Outbox") == false)
				.Select(x => MethodAnalyzer.GetCallChains(asm, method, x.GetMethod))
				.Any(x => x.Count > 0);
			if (getEntitiesExceptOutbox == false)
				return; // only outbox is accessed

			// report issue
			var debug = sendMessageAsync.SelectMany(x => x).FirstOrDefault()?.CilPoint;
			var sb = new StringBuilder();
			sb.Append("consider using Outbox pattern in method ");
			MethodAnalyzer.PrettyPrintMethod(sb, method, debug);
			ctx.Log(sb.ToString());
		}
		#endregion
	}
}
