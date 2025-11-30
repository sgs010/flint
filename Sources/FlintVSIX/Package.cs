using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FlintVSIX
{
	[Guid("DDEFFBBA-ECBA-4A0E-B282-B953AE175015")]
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
	public sealed class Package : AsyncPackage
	{
		private DTE2 _dte;
		private int _buildSessionId;
		private ErrorListDataSource _errorList;

		protected override async Task InitializeAsync(CancellationToken ct, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(ct);

			if (await GetServiceAsync(typeof(DTE)) is not DTE2 dteObj)
			{
				Debug.WriteLine("[Flint] ERROR: DTE service is not available.");
				return;
			}

			if (await GetServiceAsync(typeof(SComponentModel)) is not IComponentModel componentModel)
			{
				Debug.WriteLine("[Flint] ERROR: SComponentModel service is not available.");
				return;
			}

			_errorList = new ErrorListDataSource(componentModel);

			_dte = dteObj;
			_dte.Events.BuildEvents.OnBuildBegin += OnBuildBegin;
			_dte.Events.BuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;
		}

		private void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
		{
			Interlocked.Increment(ref _buildSessionId);
		}

		private void OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
		{
			if (success == false)
				return;

			ThreadHelper.ThrowIfNotOnUIThread();

			if (TryGetProjectParameters(project, out var projectId, out var projectName, out var projectOutputPath) == false)
				return;

			try
			{
				Flint.Api.CheckValidImage(projectOutputPath);
			}
			catch (Exception)
			{
				return;
			}

			var sessionId = _buildSessionId;
			_ = JoinableTaskFactory.RunAsync(() => AnalyzeAsync(sessionId, projectId, projectName, projectOutputPath));
		}

		private async Task AnalyzeAsync(int sessionId, Guid projectId, string projectName, string outputPath)
		{
			try
			{
				var st = Stopwatch.StartNew();
				var result = Flint.Api.Analyze(outputPath);
				st.Stop();

				if (sessionId == _buildSessionId)
				{
					await JoinableTaskFactory.SwitchToMainThreadAsync();
					var entries = result.ToList(x => new ErrorListEntry(projectId, projectName, x.Code, x.Message, x.File, x.Line));
					WriteToBuildOutput(projectName, st.ElapsedMilliseconds, entries);
					_errorList.UpdateProjectEntries(projectId, entries);
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

		private bool TryGetProjectParameters(string project, out Guid projectId, out string projectName, out string projectOutputPath)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			projectId = default;
			projectName = default;
			projectOutputPath = default;

			foreach (Project proj in _dte.Solution.Projects)
			{
				if (proj.UniqueName == project)
				{
					// project id
					var solution = (IVsSolution)GetGlobalService(typeof(SVsSolution));
					solution.GetProjectOfUniqueName(proj.FileName, out var hierarchy);
					solution.GetGuidOfProject(hierarchy, out projectId);

					// project name
					projectName = proj.Name;

					// project output path
					var outputPath = proj.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
					var outputFileName = proj.Properties.Item("OutputFileName").Value.ToString();
					projectOutputPath = Path.Combine(Path.GetDirectoryName(proj.FullName), outputPath, outputFileName);

					return true;
				}
			}
			return false;
		}

		private void WriteToBuildOutput(string message)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var pane = _dte.ToolWindows.OutputWindow.OutputWindowPanes.Item("Build");
			pane.OutputString("[Flint] ");
			pane.OutputString(message);
			pane.OutputString(Environment.NewLine);
		}

		private void WriteToBuildOutput(string projectName, long elapsedMilliseconds, IReadOnlyCollection<ErrorListEntry> entries)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var pane = _dte.ToolWindows.OutputWindow.OutputWindowPanes.Item("Build");

			pane.OutputString($"========== [Flint] analysis started for project {projectName} ==========");
			pane.OutputString(Environment.NewLine);

			foreach (var x in entries)
			{
				pane.OutputString($"{x.File}({x.Line},1): {x.Message}");
				pane.OutputString(Environment.NewLine);
			}

			pane.OutputString($"========== [Flint] found {entries.Count} issues in project {projectName}, took {elapsedMilliseconds} ms ==========");
			pane.OutputString(Environment.NewLine);
		}
	}
}
