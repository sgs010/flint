using System.Collections.Immutable;
using Flint;

namespace FlintWeb.Services
{
	#region IFlintService
	public interface IFlintService
	{
		ImmutableArray<string> Analyze(Stream dllFile, Stream pdbFile);
	}
	#endregion

	#region FlintService
	class FlintService : IFlintService
	{
		public ImmutableArray<string> Analyze(Stream dllFile, Stream pdbFile)
		{
			return Api.Analyze(dllFile, pdbFile);
		}
	}
	#endregion
}
