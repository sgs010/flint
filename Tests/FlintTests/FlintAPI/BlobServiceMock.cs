using Flint.Services;

namespace FlintTests.FlintAPI
{
	class BlobServiceMock : IBlobService
	{
		public Dictionary<string, byte[]> Files { get; } = [];

		public async Task UploadAsync(string name, Stream data)
		{
			using var ms = new MemoryStream();
			await data.CopyToAsync(ms);
			var bytes = ms.ToArray();
			Files.Add(name, bytes);
		}
	}
}
