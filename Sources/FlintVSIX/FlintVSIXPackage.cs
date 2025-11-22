using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FlintVSIX
{
	[Guid("ddeffbba-ecba-4a0e-b282-b953ae175015")]
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
	public sealed class FlintVSIXPackage : AsyncPackage
	{
		private DTE2 _dte;

		protected override async Task InitializeAsync(CancellationToken ct, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(ct);

			if (await GetServiceAsync(typeof(DTE)) is DTE2 dteObj)
			{
				_dte = dteObj;
				_dte.Events.BuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;
			}
			else
			{
				Debug.WriteLine("[Flint] ERROR DTE service not available.");
			}
		}

		private void WriteLine(string message)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var pane = _dte.ToolWindows.OutputWindow.OutputWindowPanes.Item("Build");
			pane.OutputString("[Flint] ");
			pane.OutputString(message);
			pane.OutputString(Environment.NewLine);
		}

		private void OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (success == false)
				return;

			try
			{
				string outputPath = GetOutputDllPath(project);
				if (File.Exists(outputPath))
				{
					string hash = ComputeHash(outputPath);
					WriteLine($"{Path.GetFileName(outputPath)} hash: {hash}");
				}
			}
			catch (Exception ex)
			{
				WriteLine($"ERROR: {ex.Message}");
			}
		}

		private string GetOutputDllPath(string projectName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			foreach (Project proj in _dte.Solution.Projects)
			{
				if (proj.UniqueName == projectName)
				{
					string outputPath = proj.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
					string fullPath = Path.Combine(Path.GetDirectoryName(proj.FullName), outputPath, proj.Properties.Item("OutputFileName").Value.ToString());
					return fullPath;
				}
			}
			return string.Empty;
		}

		private string ComputeHash(string filePath)
		{
			using var sha256 = SHA256.Create();
			using var stream = File.OpenRead(filePath);
			byte[] hashBytes = sha256.ComputeHash(stream);
			return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
		}
	}
}
