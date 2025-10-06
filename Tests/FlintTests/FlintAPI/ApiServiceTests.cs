using System.Text;
using Flint.Common;
using Flint.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;

namespace FlintTests.FlintAPI
{
	[TestClass]
	public sealed class ApiServiceTests
	{
		[TestMethod]
		public async Task CheckAsync_AsmIsRequired()
		{
			var blob = new BlobServiceMock();
			var flint = new FlintServiceMock();
			var api = new ApiService(blob, flint);
			var pdb = new FormFileMock("Samples.pdb");

			await Assert.ThrowsAsync<ArgumentNullException>(() => api.CheckAsync(null, pdb));
		}

		[TestMethod]
		public async Task CheckAsync_PdbIsOptional()
		{
			var blob = new BlobServiceMock();
			var flint = new FlintServiceMock();
			var api = new ApiService(blob, flint);
			var asm = new FormFileMock("Samples.dll");

			await api.CheckAsync(asm, null);
		}

		[TestMethod]
		public async Task CheckAsync()
		{
			// HASH is asm file SHA256 hash
			// save asm file to blob as HASH.bin
			// (optional) save pdb file to blob as HASH.pdb
			// analyze asm and get results from flint core
			// save results to blob as HASH.txt
			// return results

			var hash = FileUtils.GetSHA256Hash("Samples.dll");
			var blob = new BlobServiceMock();
			var flint = new FlintServiceMock { AnalyzeResult = "foo bar baz" };
			var api = new ApiService(blob, flint);
			var asm = new FormFileMock("Samples.dll");
			var pdb = new FormFileMock("Samples.pdb");

			var result = await api.CheckAsync(asm, pdb);

			result.AssertEquals("foo bar baz");
			blob.Files.AssertContains(key: hash + ".bin", value: File.ReadAllBytes("Samples.dll"));
			blob.Files.AssertContains(key: hash + ".txt", value: Encoding.Default.GetBytes("foo bar baz"));
		}

		[TestMethod]
		public async Task CheckAsync_SaveDataOnError()
		{
			// if flint core fails with error, save asm to blob for further checks

			var error = new Exception("fffuuu");
			var hash = FileUtils.GetSHA256Hash("Samples.dll");
			var blob = new BlobServiceMock();
			var flint = new FlintServiceMock { AnalyzeError = error };
			var api = new ApiService(blob, flint);
			var asm = new FormFileMock("Samples.dll");
			var pdb = new FormFileMock("Samples.pdb");

			await Assert.ThrowsAsync<Exception>(() => api.CheckAsync(asm, pdb));

			blob.Files.AssertContains(key: hash + ".bin", value: File.ReadAllBytes("Samples.dll"));
		}
	}
}
