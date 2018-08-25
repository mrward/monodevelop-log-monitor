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

using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Core.Logging;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using Xwt.Drawing;

namespace MonoDevelop.LogMonitor.Gui
{
	partial class LogMonitorWidget
	{
		public LogMonitorWidget ()
		{
			Build ();

			LogMonitorMessages.MessageLogged += LogMessageLogged;
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
	}
}
