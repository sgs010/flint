using Flint.Analyzers;

namespace FlintTests.FlintCore
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
