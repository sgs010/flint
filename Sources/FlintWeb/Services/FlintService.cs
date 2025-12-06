using System.Collections.Immutable;
using Flint;

namespace FlintWeb.Services
{
	#region IFlintService
	public interface IFlintService
	{
		void CheckValidImage(Stream dllFile);
		ImmutableArray<string> Analyze(Stream dllFile, Stream pdbFile);
	}
	#endregion

	#region FlintService
	class FlintService : IFlintService
	{
		public void CheckValidImage(Stream dllFile)
		{
			Api.CheckValidImage(dllFile);
		}

		public ImmutableArray<string> Analyze(Stream dllFile, Stream pdbFile)
		{
			return Api.AnalyzeCLI(dllFile, pdbFile);
		}
	}
	#endregion
}
