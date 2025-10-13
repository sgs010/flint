using System.Diagnostics;

namespace Flint.Analyzers
{
	#region ITraceToken
	internal interface ITraceToken { }
	#endregion

	#region IAnalyzerContext
	internal interface IAnalyzerContext
	{
		void Log(string message);
		ITraceToken BeginTrace(string message);
		void EndTrace(ITraceToken token);
	}
	#endregion

	#region AnalyzerContext
	internal class AnalyzerContext : IAnalyzerContext
	{
		#region Data
		private List<string> _output = [];
		#endregion

		#region Properties
		public bool Trace { get; init; }
		public IReadOnlyCollection<string> Output { get { return _output; } }
		#endregion

		#region Interface
		public void Log(string message)
		{
			_output.Add(message);
		}

		public ITraceToken BeginTrace(string message)
		{
			if (Trace == false)
				return null;

			Console.WriteLine($"BEGIN {message}");
			return new TraceToken(message, Stopwatch.StartNew());
		}

		public void EndTrace(ITraceToken token)
		{
			if (token == null)
				return;
			if (Trace == false)
				return;

			if (token is TraceToken tt)
			{
				tt.Watch.Stop();
				Console.WriteLine($"END {tt.Message} - elapsed {tt.Watch.ElapsedMilliseconds} ms");
			}
		}
		#endregion

		#region Implementation
		record struct TraceToken(string Message, Stopwatch Watch) : ITraceToken;
		#endregion
	}
	#endregion
}
