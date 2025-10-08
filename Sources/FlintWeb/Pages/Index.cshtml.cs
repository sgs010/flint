using System.Collections.Immutable;
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
		[Required]
		public IFormFile DllFile { get; set; }

		[BindProperty]
		public IFormFile PdbFile { get; set; }

		public bool ShowResult { get; set; }
		public ImmutableArray<string> Result { get; set; }
		public long ElapsedMilliseconds { get; set; }

		public IActionResult OnPost()
		{
			if (ModelState.IsValid == false)
				return Page();
			if (DllFile == null)
				return Page();

			Result = [];
			ElapsedMilliseconds = 0;

			using (var dllStream = DllFile.OpenReadStream())
			using (var pdbStream = PdbFile?.OpenReadStream())
			{
				var watch = Stopwatch.StartNew();
				Result = _flint.Analyze(dllStream, pdbStream);
				watch.Stop();
				ElapsedMilliseconds = watch.ElapsedMilliseconds;
			}

			ShowResult = true;
			return Page();
		}
	}
}
