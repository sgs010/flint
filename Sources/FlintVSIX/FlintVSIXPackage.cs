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
using Microsoft.VisualStudio.TextManager.Interop;

namespace FlintVSIX
{
	[Guid("DDEFFBBA-ECBA-4A0E-B282-B953AE175015")]
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
	public sealed class FlintVSIXPackage : AsyncPackage
	{
		private DTE2 _dte;
		private ErrorListProvider _errorList;
		private int _buildSessionId;

		protected override async Task InitializeAsync(CancellationToken ct, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(ct);

			if (await GetServiceAsync(typeof(DTE)) is not DTE2 dteObj)
			{
				Debug.WriteLine("[Flint] ERROR: DTE service is not available.");
				return;
			}

			_errorList = new ErrorListProvider(this)
			{
				ProviderName = "Flint",
				ProviderGuid = new Guid("45208DF8-290C-4DAA-BDBC-42550DD3F704")
			};

			_dte = dteObj;
			_dte.Events.BuildEvents.OnBuildBegin += OnBuildBegin;
			_dte.Events.BuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;
		}

		private void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
		{
			Interlocked.Increment(ref _buildSessionId);

			ThreadHelper.ThrowIfNotOnUIThread();
			_errorList.Tasks.Clear();
		}

		private void OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
		{
			if (success == false)
				return;

			ThreadHelper.ThrowIfNotOnUIThread();
			var sessionId = _buildSessionId;
			var outputPath = GetOutputDllPath(project);
			_ = JoinableTaskFactory.RunAsync(() => AnalyzeAsync(sessionId, outputPath));
		}

		private async Task AnalyzeAsync(int sessionId, string outputPath)
		{
			try
			{
				var result = Flint.Api.Analyze(outputPath);
				if (sessionId == _buildSessionId)
				{
					await JoinableTaskFactory.SwitchToMainThreadAsync();
					foreach (var x in result)
					{
						AddErrorListMessage(x.Code, x.Message, x.File, x.Line, x.Column);
					}
				}
			}
			catch (Exception ex)
			{
				if (sessionId == _buildSessionId)
				{
					await JoinableTaskFactory.SwitchToMainThreadAsync();
					WriteToBuildOutput($"ERROR: {ex}");
				}
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

		private void WriteToBuildOutput(string message)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var pane = _dte.ToolWindows.OutputWindow.OutputWindowPanes.Item("Build");
			pane.OutputString("[Flint] ");
			pane.OutputString(message);
			pane.OutputString(Environment.NewLine);
		}

		private void AddErrorListMessage(string code, string message, string file, int line, int column)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var task = new ErrorTask
			{
				Category = TaskCategory.BuildCompile,
				ErrorCategory = TaskErrorCategory.Warning,
				Priority = TaskPriority.Low,
				Text = message,
				Document = file,
				Line = line - 1,
				Column = column - 1,
			};
			task.Navigate += OnNavidate;

			_errorList.Tasks.Add(task);
			_errorList.Show();
		}

		private void OnNavidate(object sender, EventArgs e)
		{
			if (sender is not ErrorTask task)
				return;

			ThreadHelper.ThrowIfNotOnUIThread();

			VsShellUtilities.OpenDocument(this, task.Document, Guid.Empty, out var _, out var _, out var _, out var view);
			if (view == null)
				return;

			view.SetCaretPos(task.Line, task.Column);
			view.CenterLines(task.Line, 1);
			view.EnsureSpanVisible(new TextSpan { iStartLine = task.Line, iStartIndex = task.Column });
		}
	}
}
