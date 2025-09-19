using System.Runtime.CompilerServices;
using Flint.Analyzers;
using Flint.Common;
using Mono.Cecil;

[assembly: InternalsVisibleTo("FlintTests")]

namespace Flint
{
	internal static class Program
	{
		class InputParameters
		{
			public string Input { get; set; }
		}

		internal static int Main(string[] args)
		{
			if (args.IsNullOrEmpty())
			{
				ShowHelpMessage();
				return 0;
			}

			try
			{
				var p = ParseInputParameters(args);
				Run(p);
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return 1;
			}
		}

		private static void ShowHelpMessage()
		{
			Console.WriteLine("Flint - linter tool to analyze and suggest improvements for EF Core code.");
			Console.WriteLine("Usage: flint --input=<path_to_assembly>");
			Console.WriteLine("Options:");
			Console.WriteLine("  --input - Path to the assembly (DLL or EXE) to analyze.");
			Console.WriteLine("Example:");
			Console.WriteLine("  flint --input=MyApp.dll");
		}

		private static InputParameters ParseInputParameters(string[] args)
		{
			var parameters = new InputParameters();
			foreach (var arg in args)
			{
				if (arg.StartsWith("--input="))
				{
					parameters.Input = arg.Substring("--input=".Length);
				}
				else
				{
					throw new ArgumentException($"Unknown argument: {arg}");
				}
			}
			if (string.IsNullOrWhiteSpace(parameters.Input))
			{
				throw new ArgumentException("Input parameter is required.");
			}
			return parameters;
		}

		private static void Run(InputParameters parameters)
		{
			using var asm = ModuleDefinition.ReadModule(parameters.Input, new ReaderParameters { ReadSymbols = true });
			var ctx = new AnalyzerContext();
			ProjectionAnalyzer.Run(ctx, asm);
		}
	}
}
