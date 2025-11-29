using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace FlintVSIX
{
	#region ErrorListEntry
	internal sealed class ErrorListEntry : ITableEntry
	{
		private readonly string _code;
		private readonly string _message;
		private readonly string _file;
		private readonly int _line;
		private readonly int _column;

		public object Identity { get; }
		public string Project { get; }

		public ErrorListEntry(string code, string message, string project, string file, int line, int column)
		{
			_code = code;
			_message = message;
			_file = file;
			_line = line;
			_column = column;
			Project = project;
			Identity = $"{project}|{file}|{line}|{code}";
		}

		public bool TryGetValue(string keyName, out object content)
		{
			switch (keyName)
			{
				case StandardTableColumnDefinitions.ErrorSeverity:
					content = __VSERRORCATEGORY.EC_WARNING;
					return true;
				case StandardTableColumnDefinitions.ErrorCode:
					content = _code;
					return true;
				case StandardTableColumnDefinitions.Text:
					content = _message;
					return true;
				case StandardTableColumnDefinitions.ProjectName:
					content = Project;
					return true;
				case StandardTableColumnDefinitions.DocumentName:
					content = _file;
					return true;
				case StandardTableColumnDefinitions.Line:
					content = _line;
					return true;
				case StandardTableColumnDefinitions.Column:
					content = _column;
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
		public static readonly string[] Columns =
		[
			StandardTableColumnDefinitions.ErrorSeverity,
			StandardTableColumnDefinitions.ErrorCode,
			StandardTableColumnDefinitions.Text,
			StandardTableColumnDefinitions.ProjectName,
			StandardTableColumnDefinitions.DocumentName,
			StandardTableColumnDefinitions.Line
		];

		public IDisposable Subscribe(ITableDataSink sink)
		{
			_sinks.Add(sink);
			sink.AddEntries(_entries, removeAllEntries: false);
			return new Subscription(this, sink);
		}

		//
		// TODO: update all entries in a batch
		//
		//public void UpdateEntries(IEnumerable<MyErrorListEntry> newEntries, Guid projectGuid)
		//{
		//	// Remove old entries for this project
		//	_entries.RemoveAll(e => e.ProjectGuid == projectGuid);

		//	// Add new ones
		//	var list = new List<MyErrorListEntry>(newEntries);
		//	_entries.AddRange(list);

		//	// Notify sinks: clear project’s rows, then re-add
		//	foreach (var sink in _sinks)
		//	{
		//		// Clear everything, then re-add all entries (simplest approach)
		//		sink.AddEntries(Array.Empty<ITableEntry>(), removeAllEntries: true);
		//		sink.AddEntries(_entries, removeAllEntries: false);
		//	}
		//}

		public void AddEntry(ErrorListEntry entry)
		{
			_entries.Add(entry);
			foreach (var sink in _sinks)
			{
				sink.AddEntries([entry], removeAllEntries: false);
			}
		}

		public void ClearEntriesForProject(string project)
		{
			_entries.RemoveAll(x => x.Project == project);
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
