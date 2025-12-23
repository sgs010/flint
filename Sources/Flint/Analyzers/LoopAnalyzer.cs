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
			var queries = QueryAnalyzer.Analyze(asm, className, methodName);
			foreach (var query in queries)
			{
				
			}
		}
		#endregion
	}
}
