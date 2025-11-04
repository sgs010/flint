using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Cryptography;
using FlintWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlintWeb.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;
		private readonly IFlintService _flint;
		private readonly IStorageService _storage;
		public IndexModel(ILogger<IndexModel> logger, IFlintService flint, IStorageService storage)
		{
			_logger = logger;
			_flint = flint;
			_storage = storage;
		}

		[BindProperty]
		[Required(ErrorMessage = "required")]
		public IFormFile DllFile { get; set; }

		[BindProperty]
		public IFormFile PdbFile { get; set; }

		public bool ShowResult { get; set; }
		public IReadOnlyList<string> Result { get; set; }
		public long ElapsedMilliseconds { get; set; }
		public string FileName { get; set; }
		public string Error { get; set; }

		public async Task<IActionResult> OnPostAsync(CancellationToken ct)
		{
			if (ModelState.IsValid == false)
				return Page();
			if (DllFile == null)
				return Page();

			FileName = DllFile.FileName;
			Result = [];
			ElapsedMilliseconds = 0;
			Error = null;

			var traceId = Guid.NewGuid();
			_logger.LogInformation($"[{traceId}] processing file {FileName}");
			var watch = Stopwatch.StartNew();
			try
			{
				Result = await ProcessFileAsync(DllFile, PdbFile, _flint, _storage, ct);
			}
			catch (Exception ex)
			{
				_logger.LogError($"[{traceId}] ERROR {ex}");
				Error = ex.Message;
			}
			watch.Stop();
			ElapsedMilliseconds = watch.ElapsedMilliseconds;
			if (Error == null)
				_logger.LogInformation($"[{traceId}] finished in {ElapsedMilliseconds} ms");

			ShowResult = true;
			return Page();
		}

		private static async Task<IReadOnlyList<string>> ProcessFileAsync(IFormFile dllFile, IFormFile pdbFile, IFlintService flint, IStorageService storage, CancellationToken ct)
		{
			using var dllStream = new MemoryStream();
			await dllFile.CopyToAsync(dllStream, ct);

			using var pdbStream = new MemoryStream();
			if (pdbFile != null)
				await pdbFile.CopyToAsync(pdbStream, ct);

			using var sha256 = SHA256.Create();
			dllStream.Position = 0;
			var dllHashBytes = await sha256.ComputeHashAsync(dllStream, ct);
			var dllHash = Convert.ToHexStringLower(dllHashBytes);
			var blobFile = dllHash + ".blob";
			var resultFile = dllHash + ".result";
			var errorFile = dllHash + ".error";

			IReadOnlyList<string> result = null;
			if (await storage.ExistsAsync(resultFile, ct))
			{
				result = await storage.ReadAllLinesAsync(resultFile, ct);
			}
			else
			{
				dllStream.Position = 0;
				await storage.UploadAsync(blobFile, dllStream, ct);

				try
				{
					dllStream.Position = 0;
					pdbStream.Position = 0;
					result = flint.Analyze(dllStream, pdbStream);
				}
				catch (Exception ex)
				{
					await storage.UploadAsync(errorFile, ex.ToString(), ct);
					throw;
				}
				ct.ThrowIfCancellationRequested();

				await storage.UploadAsync(resultFile, result, ct);
			}
			return result;
		}
	}
}
