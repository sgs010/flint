using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
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
		public ImmutableArray<string> Result { get; set; }
		public long ElapsedMilliseconds { get; set; }
		public string FileName { get; set; }
		public string Error { get; set; }

		public IActionResult OnPost()
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
			using (var dllStream = DllFile.OpenReadStream())
			using (var pdbStream = PdbFile?.OpenReadStream())
			{
				var watch = Stopwatch.StartNew();
				try
				{
					Result = _flint.Analyze(dllStream, pdbStream);
				}
				catch (Exception ex)
				{
					_logger.LogError($"[{traceId}] ERROR {ex}");
					Error = ex.Message;
				}
				watch.Stop();
				ElapsedMilliseconds = watch.ElapsedMilliseconds;
			}
			if (Error == null)
				_logger.LogInformation($"[{traceId}] finished in {ElapsedMilliseconds} ms");

			ShowResult = true;
			return Page();
		}
	}
}
