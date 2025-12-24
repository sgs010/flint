using Mono.Cecil;

namespace Flint.Analyzers
{
	internal class OutboxAnalyzer
	{
		#region Properties
		public const int Code = 5;
		#endregion

		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyInfo asm, string className = null, string methodName = null)
		{
			foreach (var method in MethodAnalyzer.GetMethods(asm, className, methodName))
			{
				Analyze(ctx, asm, method);
			}
		}
		#endregion

		#region Implementation
		private static void Analyze(IAnalyzerContext ctx, AssemblyInfo asm, MethodDefinition method)
		{
			// general idea:
			// 1. look for direct or nested call of Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync
			// 2. same for Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync
			// 3. look for adding an entity with a word "Outbox" in the name
			// if 1,2 are true and 3 is false - suggest Outbox pattern

			var saveChangesAsync = MethodAnalyzer.GetCallChains(asm, method, "Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync");
			if (saveChangesAsync.Length == 0)
				return; // SaveChangesAsync is not found

			var sendMessageAsync = MethodAnalyzer.GetCallChains(asm, method, "Azure.Messaging.ServiceBus.ServiceBusSender.SendMessageAsync");
			if (sendMessageAsync.Length == 0)
				return; // SendMessageAsync is not found

			// check if message is added to outbox
			foreach (var outbox in asm.EntityTypes.Where(x => x.Name.Contains("Outbox")))
			{
				var outboxName = AssemblyAnalyzer.GetTypeFullName(asm, outbox);
				var add = MethodAnalyzer.GetCallChains(asm, method, $"Microsoft.EntityFrameworkCore.DbSet`1<{outboxName}>.Add");
				if (add.Length > 0)
					return; // message added to outbox
			}

			// check if only outbox is accessed
			var getEntitiesExceptOutbox = asm.EntityCollections
				.Where(x => x.Name.Contains("Outbox") == false)
				.Select(x => MethodAnalyzer.GetCallChains(asm, method, x.GetMethod))
				.Any(x => x.Length > 0);
			if (getEntitiesExceptOutbox == false)
				return; // only outbox is accessed

			// report issue
			var pt = sendMessageAsync.SelectMany(x => x).FirstOrDefault().CilPoint;
			ctx.AddResult(Code, "consider using Outbox pattern", method, pt);
		}
		#endregion
	}
}
