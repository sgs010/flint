using System.Collections.Immutable;
using System.IO;
using System.Net.Security;
using System.Runtime.CompilerServices;
using Flint.Analyzers;

[assembly: InternalsVisibleTo("FlintTests")]

namespace Flint
{
	public static class Api
	{
		#region Interface
		public static ImmutableArray<string> Analyze(string dllPath)
		{
			using var asm = AssemblyAnalyzer.Load(dllPath);
			return Analyze(asm);
		}

		public static ImmutableArray<string> Analyze(Stream dllStream, Stream pdbStream)
		{
			using var asm = AssemblyAnalyzer.Load(dllStream, pdbStream);
			return Analyze(asm);
		}
		#endregion

		#region Implementation
		private static ImmutableArray<string> Analyze(AssemblyDefinition asm)
		{
			var ctx = new AnalyzerContext();

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
