using System.Diagnostics;
using Flint.Vm;
using Mono.Cecil;

namespace Flint.Analyzers
{
	#region AnalyzerResult
	internal readonly struct AnalyzerResult
	{
		public int Code { get; init; }
		public string Message { get; init; }
		public MethodReference Method { get; init; }
		public CilPoint CilPoint { get; init; }
	}
	#endregion

	#region ITraceToken
	internal interface ITraceToken { }
	#endregion

	#region IAnalyzerContext
	internal interface IAnalyzerContext
	{
		void AddResult(int code, string message, MethodReference method, CilPoint pt);
		ITraceToken BeginTrace(string message);
		void EndTrace(ITraceToken token);
	}
	#endregion

	#region AnalyzerContext
	internal class AnalyzerContext : IAnalyzerContext
	{
		#region Data
		private int _traceOffset;
		private readonly List<AnalyzerResult> _result = [];
		#endregion

		#region Properties
		public bool Trace { get; init; }
		public IReadOnlyCollection<AnalyzerResult> Result { get { return _result; } }
		#endregion

		#region Interface
		public void AddResult(int code, string message, MethodReference method, CilPoint pt)
		{
			_result.Add(new AnalyzerResult { Code = code, Message = message, Method = method, CilPoint = pt });
		}

		public ITraceToken BeginTrace(string message)
		{
			if (Trace == false)
				return null;

			for (var i = 0; i < _traceOffset; ++i)
				Console.Write('\t');
			Console.WriteLine($"BEGIN {message}");

			++_traceOffset;

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
				--_traceOffset;

				for (var i = 0; i < _traceOffset; ++i)
					Console.Write('\t');
				Console.WriteLine($"END {tt.Message} [{tt.Watch.ElapsedMilliseconds} ms]");
			}
		}
		#endregion

		#region Implementation
		record struct TraceToken(string Message, Stopwatch Watch) : ITraceToken;
		#endregion
	}
	#endregion
}
