using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using FlintWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FlintWeb.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;
		private readonly IFlintService _flint;
		public IndexModel(ILogger<IndexModel> logger, IFlintService flint)
		{
			_logger = logger;
			_flint = flint;
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

			_logger.LogInformation("processing file {fileName}", FileName);
			var watch = Stopwatch.StartNew();
			try
			{
				Result = await ProcessFileAsync(DllFile, PdbFile, _flint, _logger, ct);
			}
			catch (Exception ex)
			{
				_logger.LogError("ERROR: {error}", ex);
				Error = PrettyPrint(ex);
			}
			watch.Stop();
			ElapsedMilliseconds = watch.ElapsedMilliseconds;
			if (Error == null)
				_logger.LogInformation("finished in {ms} ms", ElapsedMilliseconds);

			ShowResult = true;
			return Page();
		}

		private static string PrettyPrint(Exception ex)
		{
			if (ex is BadImageFormatException)
			{
				return ex.Message;
			}
			return "Internal error";
		}

		private static async Task<IReadOnlyList<string>> ProcessFileAsync(IFormFile dllFile, IFormFile pdbFile, IFlintService flint, ILogger log, CancellationToken ct)
		{
			using var dllStream = new MemoryStream();
			await dllFile.CopyToAsync(dllStream, ct);

			flint.CheckValidImage(dllStream);
			ct.ThrowIfCancellationRequested();

			using var pdbStream = new MemoryStream();
			if (pdbFile != null)
				await pdbFile.CopyToAsync(pdbStream, ct);

			dllStream.Position = 0;
			pdbStream.Position = 0;
			var result = flint.Analyze(dllStream, pdbStream);
			ct.ThrowIfCancellationRequested();

			return result;
		}
	}
}
