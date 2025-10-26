using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Flint.Analyzers;

[assembly: InternalsVisibleTo("FlintTests")]

namespace Flint
{
	public static class Api
	{
		#region Interface
		public static ImmutableArray<string> Analyze(string dllPath, bool trace = false)
		{
			var ctx = new AnalyzerContext { Trace = trace };
			var tt = ctx.BeginTrace($"analyze {dllPath}");

			using var asm = AssemblyAnalyzer.Load(ctx, dllPath);
			var result = Analyze(ctx, asm);

			ctx.EndTrace(tt);
			return result;
		}

		public static ImmutableArray<string> Analyze(Stream dllStream, Stream pdbStream, bool trace = false)
		{
			var ctx = new AnalyzerContext { Trace = trace };
			var tt = ctx.BeginTrace($"analyze");

			using var asm = AssemblyAnalyzer.Load(ctx, dllStream, pdbStream);
			var result = Analyze(ctx, asm);

			ctx.EndTrace(tt);
			return result;
		}
		#endregion

		#region Implementation
		private static ImmutableArray<string> Analyze(AnalyzerContext ctx, AssemblyInfo asm)
		{
			Parallel.Invoke(
				() => ProjectionAnalyzer.Run(ctx, asm),
				() => IncludeAnalyzer.Run(ctx, asm),
				() => AsNoTrackingAnalyzer.Run(ctx, asm),
				() => AsSplitQueryAnalyzer.Run(ctx, asm),
				() => OutboxAnalyzer.Run(ctx, asm));

			return [.. ctx.Output];
		}
		#endregion
	}
}
