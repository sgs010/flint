using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace FlintVSIX
{
	#region ErrorListEntry
	internal sealed class ErrorListEntry : ITableEntry
	{
		public object Identity { get; }
		public Guid ProjectId { get; }
		public string Project { get; }
		public string Code { get; }
		public string Message { get; }
		public string File { get; }
		public int Line { get; }

		public static readonly string[] Columns =
		[
			StandardTableColumnDefinitions.ErrorSeverity,
			StandardTableColumnDefinitions.ErrorCode,
			StandardTableColumnDefinitions.Text,
			StandardTableColumnDefinitions.ProjectName,
			StandardTableColumnDefinitions.DocumentName,
			StandardTableColumnDefinitions.Line
		];

		public ErrorListEntry(Guid projectId, string project, string code, string message, string file, int line)
		{
			Identity = Guid.NewGuid();
			ProjectId = projectId;
			Project = project;
			Code = code;
			Message = message;
			File = file;
			Line = line;
		}

		public bool TryGetValue(string keyName, out object content)
		{
			switch (keyName)
			{
				case StandardTableColumnDefinitions.ErrorSeverity:
					content = __VSERRORCATEGORY.EC_WARNING;
					return true;
				case StandardTableColumnDefinitions.BuildTool:
					content = "Flint";
					return true;
				case StandardTableColumnDefinitions.ErrorCategory:
					content = "Linter";
					return true;
				case StandardTableColumnDefinitions.ErrorSource:
					content = "Build";
					return true;
				case StandardTableColumnDefinitions.ProjectName:
					content = Project;
					return true;
				case StandardTableColumnDefinitions.ErrorCode:
					content = Code;
					return true;
				case StandardTableColumnDefinitions.Text:
					content = Message;
					return true;
				case StandardTableColumnDefinitions.DocumentName:
					content = File;
					return true;
				case StandardTableColumnDefinitions.Line:
					content = Line - 1;
					return true;
				case StandardTableColumnDefinitions.Column:
					content = 0;
					return true;
				default:
					content = null;
					return false;
			}
		}

		bool ITableEntry.CanSetValue(string keyName) => false;
		bool ITableEntry.TrySetValue(string keyName, object content) => false;
	}
	#endregion

	#region ErrorListDataSource
	internal sealed class ErrorListDataSource : ITableDataSource
	{
		private readonly List<ITableDataSink> _sinks = [];
		private readonly List<ErrorListEntry> _entries = [];

		public string Identifier => "Flint";
		public string DisplayName => "Flint";
		public string SourceTypeIdentifier => StandardTableDataSources.ErrorTableDataSource;

		public ErrorListDataSource(IComponentModel componentModel)
		{
			var provider = componentModel.GetService<ITableManagerProvider>();
			var table = provider.GetTableManager(StandardTables.ErrorsTable);
			table.AddSource(this, ErrorListEntry.Columns);
		}

		public IDisposable Subscribe(ITableDataSink sink)
		{
			_sinks.Add(sink);
			sink.AddEntries(_entries, removeAllEntries: false);
			return new Subscription(this, sink);
		}

		public void UpdateProjectEntries(Guid projectId, IReadOnlyCollection<ErrorListEntry> entries)
		{
			_entries.RemoveAll(e => e.ProjectId == projectId);
			_entries.AddRange(entries);

			foreach (var sink in _sinks)
			{
				sink.AddEntries([], removeAllEntries: true);
				sink.AddEntries(_entries, removeAllEntries: false);
			}
		}

		private class Subscription : IDisposable
		{
			private readonly ErrorListDataSource _source;
			private readonly ITableDataSink _sink;

			public Subscription(ErrorListDataSource source, ITableDataSink sink)
			{
				_source = source;
				_sink = sink;
			}

			public void Dispose()
			{
				_source._sinks.Remove(_sink);
			}
		}
	}
	#endregion
}
