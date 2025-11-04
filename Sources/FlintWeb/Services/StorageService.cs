using System.Text;
using Azure.Storage.Blobs;

namespace FlintWeb.Services
{
	#region IStorageService
	public interface IStorageService
	{
		Task<bool> ExistsAsync(string fileName, CancellationToken ct);
		Task UploadAsync(string fileName, Stream data, CancellationToken ct);
		Task DownloadAsync(string fileName, Stream result, CancellationToken ct);
	}
	#endregion

	#region StorageServiceExtensions
	public static class StorageServiceExtensions
	{
		public static async Task<List<string>> ReadAllLinesAsync(this IStorageService storage, string fileName, CancellationToken ct)
		{
			using var ms = new MemoryStream();
			await storage.DownloadAsync(fileName, ms, ct);

			var result = new List<string>();
			ms.Position = 0;
			using var sr = new StreamReader(ms);
			while (true)
			{
				var line = sr.ReadLine();
				if (line == null)
					break;
				result.Add(line);
			}
			return result;
		}

		public static async Task UploadAsync(this IStorageService storage, string fileName, string data, CancellationToken ct)
		{
			using var ms = new MemoryStream(Encoding.Default.GetBytes(data));
			await storage.UploadAsync(fileName, ms, ct);
		}

		public static async Task UploadAsync(this IStorageService storage, string fileName, IEnumerable<string> data, CancellationToken ct)
		{
			var eol = Encoding.Default.GetBytes(Environment.NewLine);
			using var ms = new MemoryStream();
			var needSeparator = false;
			foreach (var line in data)
			{
				if (needSeparator)
					ms.Write(eol);
				needSeparator = true;
				ms.Write(Encoding.Default.GetBytes(line));
			}
			ms.Flush();
			ms.Position = 0;
			await storage.UploadAsync(fileName, ms, ct);
		}
	}
	#endregion

	#region FileSystemStorageService
	sealed class FileSystemStorageService : IStorageService
	{
		private readonly string _rootPath;
		public FileSystemStorageService(string connectionString, string containeerName)
		{
			_rootPath = Path.Combine(AppContext.BaseDirectory, connectionString, containeerName);
			if (Directory.Exists(_rootPath) == false)
				Directory.CreateDirectory(_rootPath);
		}

		public Task<bool> ExistsAsync(string fileName, CancellationToken ct)
		{
			var filePath = Path.Combine(_rootPath, fileName);
			var result = File.Exists(filePath);
			return Task.FromResult(result);
		}

		public async Task UploadAsync(string fileName, Stream data, CancellationToken ct)
		{
			var filePath = Path.Combine(_rootPath, fileName);
			using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			await data.CopyToAsync(fs, ct);
		}

		public async Task DownloadAsync(string fileName, Stream result, CancellationToken ct)
		{
			var filePath = Path.Combine(_rootPath, fileName);
			using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			await fs.CopyToAsync(result, ct);
		}
	}
	#endregion

	#region AzureStorageService
	sealed class AzureStorageService : IStorageService
	{
		private readonly string _connectionString;
		private readonly string _containeerName;
		public AzureStorageService(string connectionString, string containeerName)
		{
			_connectionString = connectionString;
			_containeerName = containeerName;
		}

		public async Task<bool> ExistsAsync(string fileName, CancellationToken ct)
		{
			var blob = await GetBlobClient(fileName, ct);
			var result = await blob.ExistsAsync(ct);
			return result;
		}

		public async Task UploadAsync(string fileName, Stream data, CancellationToken ct)
		{
			var blob = await GetBlobClient(fileName, ct);
			await blob.UploadAsync(data, overwrite: true, ct);
		}

		public async Task DownloadAsync(string fileName, Stream result, CancellationToken ct)
		{
			var blob = await GetBlobClient(fileName, ct);
			await blob.DownloadToAsync(result, ct);
		}

		private async Task<BlobClient> GetBlobClient(string fileName, CancellationToken ct)
		{
			var client = new BlobContainerClient(_connectionString, _containeerName);
			await client.CreateIfNotExistsAsync(cancellationToken: ct);

			var blob = client.GetBlobClient(fileName);
			return blob;
		}
	}
	#endregion
}
