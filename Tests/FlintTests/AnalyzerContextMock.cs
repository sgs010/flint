using Flint.Analyzers;

namespace FlintTests
{
	class AnalyzerContextMock : IAnalyzerContext
	{
		public readonly List<string> Output = [];

		public void Log(string message)
		{
			Output.Add(message);
		}
	}
}
