using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Flint.Analyzers;

[assembly: InternalsVisibleTo("FlintTests")]

namespace Flint
{
	public class ApiOptions
	{
		public bool Trace { get; set; }
	}

	public static class Api
	{
		#region Interface
		public static void CheckValidImage(Stream dllStream)
		{
			dllStream.Position = 0;
			Mono.Cecil.ModuleDefinition.ReadModule(dllStream).Dispose();
		}

		public static ImmutableArray<string> Analyze(string dllPath, ApiOptions options = null)
		{
			var opt = options ?? new ApiOptions();
			var ctx = new AnalyzerContext { Trace = opt.Trace };
			var tt = ctx.BeginTrace($"analyze {dllPath}");

			using var asm = AssemblyAnalyzer.Load(dllPath);
			var result = Analyze(ctx, asm);

			ctx.EndTrace(tt);
			return result;
		}

		public static ImmutableArray<string> Analyze(Stream dllStream, Stream pdbStream, ApiOptions options = null)
		{
			var opt = options ?? new ApiOptions();
			var ctx = new AnalyzerContext { Trace = opt.Trace };
			var tt = ctx.BeginTrace($"analyze");

			using var asm = AssemblyAnalyzer.Load(dllStream, pdbStream);
			var result = Analyze(ctx, asm);

			ctx.EndTrace(tt);
			return result;
		}
		#endregion

		#region Implementation
		private static ImmutableArray<string> Analyze(AnalyzerContext ctx, AssemblyInfo asm)
		{
			ProjectionAnalyzer.Run(ctx, asm);
			IncludeAnalyzer.Run(ctx, asm);
			AsNoTrackingAnalyzer.Run(ctx, asm);
			AsSplitQueryAnalyzer.Run(ctx, asm);
			OutboxAnalyzer.Run(ctx, asm);

			return [.. ctx.Output];
		}
		#endregion
	}
}
