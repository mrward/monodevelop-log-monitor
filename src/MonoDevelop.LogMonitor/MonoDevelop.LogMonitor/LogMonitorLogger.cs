//
// LogMonitorLogger.cs
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
using MonoDevelop.Core;
using MonoDevelop.Core.Logging;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.LogMonitor.Gui;

namespace MonoDevelop.LogMonitor
{
	public class LogMonitorLogger : ILogger
	{
		StatusBarIcon statusBarIcon;
		int errorsCount;

		public EnabledLoggingLevel EnabledLevel => EnabledLoggingLevel.UpToWarn;

		public string Name => nameof (LogMonitorLogger);

		public void Log (LogLevel level, string message)
		{
			LogMonitorMessages.ReportLogMessage (level, message);

			switch (level) {
				case LogLevel.Error:
					OnLogError (message);
					break;
				case LogLevel.Fatal:
					OnLogFatal (message);
					break;
				case LogLevel.Warn:
					OnLogWarn (message);
					break;
			}
		}

		void OnLogWarn (string message)
		{

		}

		void OnLogFatal (string message)
		{
			errorsCount++;
		}

		void OnLogError (string message)
		{
			errorsCount++;
			ShowStatusIcon ();
		}

		void ShowStatusIcon ()
		{
			Runtime.RunInMainThread (() => {
				if (statusBarIcon == null) {
					var icon = ImageService.GetIcon (Stock.TextFileIcon, Gtk.IconSize.Menu);
					statusBarIcon = IdeApp.Workbench.StatusBar.ShowStatusIcon (icon);
					statusBarIcon.Clicked += StatusBarIconClicked;
				}
				statusBarIcon.Title = GettextCatalog.GetString ("IDE log errors");
				statusBarIcon.ToolTip = GettextCatalog.GetPluralString ("{0} IDE log error", "{0} IDE log errors", errorsCount, errorsCount);
				statusBarIcon.SetAlertMode (1);
			}).Ignore ();
		}

		void StatusBarIconClicked (object sender, StatusBarIconClickedEventArgs e)
		{
			Pad pad = IdeApp.Workbench.GetPad<LogMonitorPad> ();
			pad.BringToFront (true);
		}
	}
}
