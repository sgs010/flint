using System.Runtime.CompilerServices;
using Flint.Analyzers;

[assembly: InternalsVisibleTo("FlintTests")]

namespace Flint
{
	public static class Api
	{
		public static void Run(string path)
		{
			using var asm = AssemblyAnalyzer.Load(path);
			var ctx = new AnalyzerContext();
			Parallel.Invoke(
				() => ProjectionAnalyzer.Run(ctx, asm),
				() => IncludeAnalyzer.Run(ctx, asm),
				() => AsNoTrackingAnalyzer.Run(ctx, asm),
				() => AsSplitQueryAnalyzer.Run(ctx, asm),
				() => OutboxAnalyzer.Run(ctx, asm));
		}
	}
}
