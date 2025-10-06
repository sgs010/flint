using Flint.Services;

namespace FlintTests.FlintAPI
{
	class FlintServiceMock : IFlintService
	{
		public string AnalyzeResult { get; set; }
		public Exception AnalyzeError { get; set; }

		public string Analyze(Stream asm, Stream pdb)
		{
			if (AnalyzeError != null)
				throw AnalyzeError;
			return AnalyzeResult;
		}
	}
}
