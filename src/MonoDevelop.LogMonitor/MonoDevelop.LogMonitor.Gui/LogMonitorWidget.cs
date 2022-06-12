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
using CoreGraphics;
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
					return ImageService.GetIcon (Stock.Error, IconSize.Small);
				case LogLevel.Warn:
					return ImageService.GetIcon (Stock.Warning, IconSize.Small);
				case LogLevel.Info:
					return ImageService.GetIcon (Stock.Information, IconSize.Small);
				case LogLevel.Debug:
					return ImageService.GetIcon (Stock.Console, IconSize.Small);
				default:
					return ImageService.GetIcon (Stock.Error, IconSize.Small);
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
			var menu = IdeApp.CommandService.CreateNSMenu (commands, this);
			ShowContextMenu (view, (int)e.X, (int)e.Y, menu);
		}

		/// <summary>
		/// Take from main/src/core/MonoDevelop.Ide/MonoDevelop.Components/ContextMenuExtensionsMac.cs
		/// </summary>
		static void ShowContextMenu (NSView parent, int x, int y, NSMenu menu, bool selectFirstItem = false, bool convertToViewCoordinates = true)
		{
			if (parent == null)
				throw new ArgumentNullException ("parent");
			if (menu == null)
				throw new ArgumentNullException ("menu");

			var pt = convertToViewCoordinates ? parent.ConvertPointToView (new CGPoint (x, y), null) : new CGPoint (x, y);
			if (selectFirstItem) {
				menu.PopUpMenu (menu.ItemAt (0), pt, parent);
			} else {
				var tmp_event = NSEvent.MouseEvent (NSEventType.LeftMouseDown,
												pt,
												0, 0,
												parent.Window.WindowNumber,
												null, 0, 0, 0);

				// the following lines are here to dianose & fix VSTS 1026106 - we were getting
				// a SigSegv from here and it is likely caused by NSEvent being null, however
				// it's worth leaving Debug checks in just to be on the safe side
				if (tmp_event == null) {
					// since this is often called outside of a try/catch loop, we'll just
					// log an error and not throw the exception
					LoggingService.LogInternalError (new ArgumentNullException (nameof (tmp_event)));
					return;
				}

				System.Diagnostics.Debug.Assert (parent != null, "Parent was modified (set to null) during execution.");
				System.Diagnostics.Debug.Assert (menu != null, "Menu was modified (set to null) during execution.");

				NSMenu.PopUpContextMenu (menu, tmp_event, parent);
			}
		}

		[CommandUpdateHandler (LogMonitorCommands.OpenLogFile)]
		void OnOpenLogFile (CommandInfo info)
		{
			info.Enabled = CurrentIdeLogFile.CanOpen;
		}

		[CommandHandler (LogMonitorCommands.OpenLogFile)]
		void OpenLogFile ()
		{
			CurrentIdeLogFile.Open ();
		}
	}
}
