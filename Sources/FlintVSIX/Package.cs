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
	[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(UIContextGuids80.EmptySolution, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(UIContextGuids80.SolutionHasSingleProject, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(UIContextGuids80.SolutionHasMultipleProjects, PackageAutoLoadFlags.BackgroundLoad)]
	public sealed class Package : AsyncPackage
	{
		// TODO: write log to %AppData%\Flint\log.txt using NLOG https://github.com/NLog/NLog/wiki/Tutorial
		private IVsActivityLog _log;
		private DTE2 _dte;
		private int _buildSessionId;
		private ErrorListDataSource _errorList;

		protected override async Task InitializeAsync(CancellationToken ct, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(ct);

			if (await GetServiceAsync(typeof(SVsActivityLog)) is not IVsActivityLog logObj)
			{
				LogError("SVsActivityLog service is not available");
				return;
			}
			_log = logObj;

			LogInfo("initialization started");

			if (await GetServiceAsync(typeof(DTE)) is not DTE2 dteObj)
			{
				LogError("DTE service is not available");
				return;
			}
			_dte = dteObj;

			if (await GetServiceAsync(typeof(SComponentModel)) is not IComponentModel componentModel)
			{
				LogError("SComponentModel service is not available");
				return;
			}
			_errorList = new ErrorListDataSource(componentModel);

			_dte.Events.SolutionEvents.Opened += SolutionEvents_Opened;
			_dte.Events.SolutionEvents.BeforeClosing += SolutionEvents_BeforeClosing;
			_dte.Events.SolutionEvents.ProjectRenamed += SolutionEvents_ProjectRenamed;
			_dte.Events.SolutionEvents.ProjectRemoved += SolutionEvents_ProjectRemoved;
			_dte.Events.BuildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
			_dte.Events.BuildEvents.OnBuildProjConfigBegin += BuildEvents_OnBuildProjConfigBegin;
			_dte.Events.BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;

			LogInfo("initialization complete");
		}

		private void LogMessage(__ACTIVITYLOG_ENTRYTYPE type, string message)
		{
			string typeStr = type switch
			{
				__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR => "ERROR",
				__ACTIVITYLOG_ENTRYTYPE.ALE_WARNING => "WARNING",
				_ => "INFO",
			};
			var fullMessage = $"[Flint] {typeStr}: {message}";

			Trace.WriteLine(fullMessage);

			if (_log != null)
			{
				var hr = _log.LogEntry((uint)type, "Flint", fullMessage);
				if (hr != Microsoft.VisualStudio.VSConstants.S_OK)
					Trace.WriteLine("Failed to write to activity log.");
			}
		}

		private void LogError(string message)
		{
			LogMessage(__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, message);
		}

		private void LogInfo(string message)
		{
			LogMessage(__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION, message);
		}

		private void OnSolutionChanged()
		{
			Interlocked.Increment(ref _buildSessionId);
			_errorList.ClearAll();
		}

		private void SolutionEvents_Opened()
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			OnSolutionChanged();
		}

		private void SolutionEvents_BeforeClosing()
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			OnSolutionChanged();
		}

		private void SolutionEvents_ProjectRenamed(Project project, string oldName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			OnSolutionChanged();
		}

		private void SolutionEvents_ProjectRemoved(Project Project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			OnSolutionChanged();
		}

		private void BuildEvents_OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
		{
			Interlocked.Increment(ref _buildSessionId);
		}

		private void BuildEvents_OnBuildProjConfigBegin(string project, string projectConfig, string platform, string solutionConfig)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (TryGetProjectParameters(project, out var projectId, out var _, out var _) == false)
				return;

			_errorList.ClearProjectEntries(projectId);
		}

		private void BuildEvents_OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
		{
			if (success == false)
				return;

			ThreadHelper.ThrowIfNotOnUIThread();

			LogInfo($"processing output from project {project}");
			if (TryGetProjectParameters(project, out var projectId, out var projectName, out var projectOutputPath) == false)
			{
				LogError($"failed to get parameters for project {project}");
				return;
			}

			try
			{
				Flint.Api.CheckValidImage(projectOutputPath);
			}
			catch (Exception)
			{
				LogInfo($"output from project {project} seems to be invalid for analysis");
				return;
			}

			var sessionId = _buildSessionId;
			_ = JoinableTaskFactory.RunAsync(() => AnalyzeAsync(sessionId, projectId, projectName, projectOutputPath));
			LogInfo($"output from project {project} is scheduled for analysis");
		}

		private async Task AnalyzeAsync(int sessionId, Guid projectId, string projectName, string outputPath)
		{
			try
			{
				LogInfo($"analysis started for project {projectName}");
				var st = Stopwatch.StartNew();
				var result = Flint.Api.Analyze(outputPath);
				st.Stop();
				LogInfo($"analysis finished for project {projectName}");

				if (sessionId == _buildSessionId)
				{
					await JoinableTaskFactory.SwitchToMainThreadAsync();
					var entries = result.ToList(x => new ErrorListEntry(projectId, projectName, x.Code, x.Message, x.File, x.Line));
					WriteToBuildOutput(projectName, st.ElapsedMilliseconds, entries);
					_errorList.UpdateProjectEntries(projectId, entries);
				}
				else
				{
					LogInfo($"analysis session for project {projectName} has expired, ignore results");
				}
			}
			catch (Exception ex)
			{
				LogError(ex.ToString());
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
					projectId = GetProjectId(proj);
					projectName = proj.Name;
					projectOutputPath = GetProjectOutputPath(proj);
					return true;
				}
			}
			return false;
		}

		private Guid GetProjectId(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var solution = (IVsSolution)GetGlobalService(typeof(SVsSolution));
			solution.GetProjectOfUniqueName(project.FileName, out var hierarchy);
			solution.GetGuidOfProject(hierarchy, out var projectId);

			return projectId;
		}

		private string GetProjectOutputPath(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var outputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
			var outputFileName = project.Properties.Item("OutputFileName").Value.ToString();
			var projectOutputPath = Path.Combine(Path.GetDirectoryName(project.FullName), outputPath, outputFileName);

			return projectOutputPath;
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
