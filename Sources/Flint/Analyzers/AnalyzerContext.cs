namespace Flint.Analyzers
{
	#region IAnalyzerContext
	internal interface IAnalyzerContext
	{
		void Log(string message);
	}
	#endregion

	#region AnalyzerContext
	internal class AnalyzerContext : IAnalyzerContext
	{
		private List<string> _output = [];

		public IReadOnlyCollection<string> Output => _output;

		public void Log(string message)
		{
			_output.Add(message);
		}
	}
	#endregion
}
