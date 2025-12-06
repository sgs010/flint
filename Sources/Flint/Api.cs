using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Flint.Analyzers;
using Flint.Common;
using Flint.Vm.Cil;

[assembly: InternalsVisibleTo("FlintTests")]

namespace Flint
{
	public class ApiOptions
	{
		public bool Trace { get; set; }
	}

	public readonly struct ApiResult
	{
		public string Code { get; init; }
		public string Message { get; init; }
		public string Method { get; init; }
		public string File { get; init; }
		public int Line { get; init; }
		public int Column { get; init; }
	}

	public static class Api
	{
		#region Interface
		public static void CheckValidImage(string path)
		{
			using var fs = File.OpenRead(path);
			CheckValidImage(fs);
		}

		public static void CheckValidImage(Stream dllStream)
		{
			dllStream.Position = 0;
			Mono.Cecil.ModuleDefinition.ReadModule(dllStream).Dispose();
		}

		public static ImmutableArray<ApiResult> Analyze(string dllPath, ApiOptions options = null)
		{
			var opt = options ?? new ApiOptions();
			var ctx = new AnalyzerContext { Trace = opt.Trace };
			var tt = ctx.BeginTrace($"analyze {dllPath}");

			using var asm = AssemblyAnalyzer.Load(dllPath);
			var result = Analyze(ctx, asm);

			ctx.EndTrace(tt);
			return result;
		}

		public static ImmutableArray<ApiResult> Analyze(Stream dllStream, Stream pdbStream, ApiOptions options = null)
		{
			var opt = options ?? new ApiOptions();
			var ctx = new AnalyzerContext { Trace = opt.Trace };
			var tt = ctx.BeginTrace($"analyze");

			using var asm = AssemblyAnalyzer.Load(dllStream, pdbStream);
			var result = Analyze(ctx, asm);

			ctx.EndTrace(tt);
			return result;
		}

		public static ImmutableArray<string> AnalyzeCLI(string dllPath, ApiOptions options = null)
		{
			return PrettyPrint(Analyze(dllPath, options));
		}

		public static ImmutableArray<string> AnalyzeCLI(Stream dllStream, Stream pdbStream, ApiOptions options = null)
		{
			return PrettyPrint(Analyze(dllStream, pdbStream, options));
		}
		#endregion

		#region Implementation
		private static ImmutableArray<ApiResult> Analyze(AnalyzerContext ctx, AssemblyInfo asm)
		{
			ProjectionAnalyzer.Run(ctx, asm);
			IncludeAnalyzer.Run(ctx, asm);
			AsNoTrackingAnalyzer.Run(ctx, asm);
			AsSplitQueryAnalyzer.Run(ctx, asm);
			OutboxAnalyzer.Run(ctx, asm);

			return ctx.Result.ToImmutableArray(x => new ApiResult
			{
				Code = string.Intern($"FLINT{x.Code:00}"),
				Message = x.Message,
				Method = string.Intern(MethodAnalyzer.PrettyPrintMethod(x.Method)),
				File = x.CilPoint?.SequencePoint.Document.Url,
				Line = x.CilPoint?.SequencePoint.StartLine ?? 0,
				Column = x.CilPoint?.SequencePoint.StartColumn ?? 0
			});
		}

		internal static ImmutableArray<string> PrettyPrint(IReadOnlyCollection<AnalyzerResult> result)
		{
			return result.ToImmutableArray(x =>
			{
				var sb = new StringBuilder(x.Message);
				sb.Append(" in method ").Append(string.Intern(MethodAnalyzer.PrettyPrintMethod(x.Method)));
				if (x.CilPoint != null)
					sb.Append(" line ").Append(x.CilPoint.SequencePoint.StartLine);
				return sb.ToString();
			});
		}

		internal static ImmutableArray<string> PrettyPrint(IReadOnlyCollection<ApiResult> result)
		{
			return result.ToImmutableArray(x =>
			{
				var sb = new StringBuilder(x.Message);
				if (x.Method != null)
					sb.Append(" in method ").Append(x.Method);
				if (x.Line > 0)
					sb.Append(" line ").Append(x.Line);
				return sb.ToString();
			});
		}
		#endregion
	}
}
