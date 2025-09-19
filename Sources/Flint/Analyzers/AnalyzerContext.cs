namespace Flint.Analyzers
{
	class AnalyzerContext : IAnalyzerContext
	{
		public void Log(string message)
		{
			Console.WriteLine(message);
		}
	}
}
