namespace Flint.Analyzers
{
	internal class LoopAnalyzer
	{
		#region Properties
		public const int Code = 6;
		#endregion

		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyInfo asm, string className = null, string methodName = null)
		{
			// Idea:
			// Loop can be recognized by BR to a target with lesser offset (go back).
			// If a root (call to ToListAsync and so on) is located between target and such BR, report an issue.

			var queries = QueryAnalyzer.Analyze(asm, className, methodName);
			foreach (var query in queries)
			{
				foreach (var (target, pt) in MethodAnalyzer.GetBrs(query.Method))
				{
					if (target.Offset > pt.Offset)
						continue; // not a loop

					foreach (var root in query.Roots)
					{
						if (root.CilPoint.Offset < target.Offset)
							continue; // root is before loop
						if (root.CilPoint.Offset > pt.Offset)
							continue; // root is after loop

						// report issue
						ctx.AddResult(Code, "avoid making queries in a loop", query.Method, query.CilPoint);
					}
				}
			}
		}
		#endregion
	}
}
