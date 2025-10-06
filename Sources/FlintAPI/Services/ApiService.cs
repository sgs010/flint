using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Flint.Common;

namespace Flint.Services
{
	interface IApiService
	{
		Task<string> CheckAsync(IFormFile asm, IFormFile pdb);
	}

	class ApiService : IApiService
	{
		private readonly IBlobService _blob;
		private readonly IFlintService _flint;
		public ApiService(IBlobService blob, IFlintService flint)
		{
			_blob = blob;
			_flint = flint;
		}

		public async Task<string> CheckAsync(IFormFile asm, IFormFile pdb)
		{
			ArgumentNullException.ThrowIfNull(asm);

			using var asmStream = asm.OpenReadStream();
			using var pdbStream = pdb?.OpenReadStream();

			var hash = FileUtils.GetSHA256Hash(asmStream);
			var asmName = hash + ".bin";
			var resultName = hash + ".txt";

			asmStream.Position = 0;
			await _blob.UploadAsync(asmName, asmStream);

			var result = _flint.Analyze(asmStream, pdbStream);

			using var resultStream = new MemoryStream(Encoding.Default.GetBytes(result ?? ""));
			await _blob.UploadAsync(resultName, resultStream);

			return await Task.FromResult(result);
		}
	}
}
