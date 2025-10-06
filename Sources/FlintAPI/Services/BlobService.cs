namespace Flint.Services
{
	interface IBlobService
	{
		Task UploadAsync(string name, Stream data);
	}

	class BlobService : IBlobService
	{
		public Task UploadAsync(string name, Stream data)
		{
			throw new NotImplementedException();
		}
	}
}
