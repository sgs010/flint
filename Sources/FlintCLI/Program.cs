namespace Flint
{
	static class Program
	{
		class InputParameters
		{
			public string Input { get; set; }
			public bool Trace { get; set; }
		}

		internal static int Main(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				ShowHelpMessage();
				return 0;
			}

			try
			{
				var parameters = ParseInputParameters(args);
				var options = new ApiOptions
				{
					Trace = parameters.Trace
				};
				var result = Api.Analyze(parameters.Input, options);
				foreach (var item in result)
				{
					Console.WriteLine(item);
				}
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
			Console.WriteLine("FlintCLI - linter tool to analyze and suggest improvements for EF Core code.");
			Console.WriteLine("Usage: FlintCLI --input=<path_to_assembly>");
			Console.WriteLine("Options:");
			Console.WriteLine("  --input - Path to the assembly (DLL or EXE) to analyze.");
			Console.WriteLine("  --trace - Show trace.");
			Console.WriteLine("Example:");
			Console.WriteLine("  FlintCLI --input=MyApp.dll");
		}

		private static InputParameters ParseInputParameters(string[] args)
		{
			var parameters = new InputParameters();
			foreach (var arg in args)
			{
				if (arg.StartsWith("--input=", StringComparison.InvariantCultureIgnoreCase))
				{
					parameters.Input = arg.Substring("--input=".Length);
				}
				else if (arg.Equals("--trace", StringComparison.InvariantCultureIgnoreCase))
				{
					parameters.Trace = true;
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
	}
}
