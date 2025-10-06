namespace Flint.Services
{
	interface IFlintService
	{
		string Analyze(Stream asm, Stream pdb);
	}

	class FlintService
	{
		public string Analyze(Stream asm, Stream pdb)
		{
			throw new NotImplementedException();
		}
	}
}
