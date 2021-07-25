//
// LogMonitorWidget.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2018 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;
using AppKit;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Core.Logging;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components.LogView;
using Xwt;
using Xwt.Drawing;

namespace MonoDevelop.LogMonitor.Gui
{
	partial class LogMonitorWidget
	{
		LogViewProgressMonitor progressMonitor;

		public LogMonitorWidget ()
		{
			Build ();

			// Need to create a progress monitor to avoid a null reference exception
			// when LogViewController.WriteText is called.
			progressMonitor = (LogViewProgressMonitor)logViewController.GetProgressMonitor();

			listView.SelectionChanged += ListViewSelectionChanged;
			listView.RowActivated += ListViewRowActivated;
			listView.ButtonPressed += ListViewButtonPressed;

			LogMonitorMessages.MessageLogged += LogMessageLogged;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				listView.SelectionChanged -= ListViewSelectionChanged;
				listView.RowActivated -= ListViewRowActivated;
				listView.ButtonPressed -= ListViewButtonPressed;

				LogMonitorMessages.MessageLogged -= LogMessageLogged;
			}
			base.Dispose (disposing);
		}

		void LogMessageLogged (object sender, LogMessageEventArgs e)
		{
			Runtime.RunInMainThread (() => AddLogMessage (e));
		}

		void AddLogMessage (LogMessageEventArgs e)
		{
			int row = 0;
			if (listStore.RowCount == 0) {
				row = listStore.AddRow ();
			} else {
				row = listStore.InsertRowBefore (0);
			}

			listStore.SetValues (
				row,
				iconField,
				GetIcon (e.Level),
				logMessageTypeField,
				GetTypeName (e.Level),
				logMessageTextField,
				GetListMessage (e.Message),
				logMessageField,
				e);
		}

		static string GetTypeName (LogLevel level)
		{
			switch (level) {
				case LogLevel.Error:
					return GettextCatalog.GetString ("Error");
				case LogLevel.Fatal:
					return GettextCatalog.GetString ("Fatal");
				case LogLevel.Warn:
					return GettextCatalog.GetString ("Warning");
				case LogLevel.Info:
					return GettextCatalog.GetString ("Info");
				case LogLevel.Debug:
					return GettextCatalog.GetString ("Debug");
				default:
					return string.Empty;
			}
		}

		static Image GetIcon (LogLevel level)
		{
			switch (level) {
				case LogLevel.Error:
				case LogLevel.Fatal:
					return ImageService.GetIcon (Stock.Error, Gtk.IconSize.Menu);
				case LogLevel.Warn:
					return ImageService.GetIcon (Stock.Warning, Gtk.IconSize.Menu);
				case LogLevel.Info:
					return ImageService.GetIcon (Stock.Information, Gtk.IconSize.Menu);
				case LogLevel.Debug:
					return ImageService.GetIcon (Stock.Console, Gtk.IconSize.Menu);
				default:
					return ImageService.GetIcon (Stock.Error, Gtk.IconSize.Menu);
			}
		}

		static string GetListMessage (string message)
		{
			return message.Split ('\n').FirstOrDefault () ?? string.Empty;
		}

		void ListViewSelectionChanged (object sender, EventArgs e)
		{
			logViewController.Clear ();

			int row = listView.SelectedRow;
			if (row < 0) {
				return;
			}

			LogMessageEventArgs logMessage = listStore.GetValue (row, logMessageField);
			if (logMessage != null) {
				logViewController.WriteText (progressMonitor, logMessage.Message);
			}
		}

		void ListViewRowActivated (object sender, ListViewRowEventArgs e)
		{
			CurrentIdeLogFile.Open ();
		}

		void ListViewButtonPressed (object sender, ButtonEventArgs e)
		{
			if (!e.IsContextMenuTrigger) {
				return;
			}

			var commands = IdeApp.CommandService.CreateCommandEntrySet ("/MonoDevelop/LogMonitorPad/ContextMenu");
			var view = listView.Surface.NativeWidget as NSView;
			IdeApp.CommandService.ShowContextMenu (view, (int)e.X, (int)e.Y, commands, this);
		}

		[CommandHandler (LogMonitorCommands.OpenLogFile)]
		void OpenLogFile ()
		{
			CurrentIdeLogFile.Open ();
		}
	}
}
