//
// LogMonitorWidget.UI.cs
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

using Xwt;
using Xwt.Drawing;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Components;

namespace MonoDevelop.LogMonitor.Gui
{
	partial class LogMonitorWidget : Widget
	{
		HPaned paned;
		ListView listView;
		ListStore listStore;
		DataField<Image> iconField = new DataField<Image> ();
		DataField<string> logMessageTypeField = new DataField<string> ();
		DataField<string> logMessageTextField = new DataField<string> ();
		DataField<LogMessageEventArgs> logMessageField = new DataField<LogMessageEventArgs> ();
		LogView logView;

		void Build ()
		{
			paned = new HPaned ();

			var mainVBox = new VBox ();

			listView = new ListView ();
			listView.BorderVisible = false;
			listView.HeadersVisible = true;
			listStore = new ListStore (iconField, logMessageTypeField, logMessageTextField, logMessageField);
			listView.DataSource = listStore;

			paned.Panel1.Content = listView;

			var column = new ListViewColumn ();
			column.Views.Add (new ImageCellView (iconField));
			column.CanResize = false;
			listView.Columns.Add (column);

			column = new ListViewColumn ();
			column.Title = GettextCatalog.GetString ("Type");
			column.Views.Add (new TextCellView (logMessageTypeField));
			column.CanResize = true;
			listView.Columns.Add (column);

			column = new ListViewColumn ();
			column.Title = GettextCatalog.GetString ("Message");
			column.Views.Add (new TextCellView (logMessageTextField));
			column.CanResize = true;
			listView.Columns.Add (column);

			logView = new LogView ();
			logView.ShowAll ();
			paned.Panel2.Content = Toolkit.CurrentEngine.WrapWidget (logView, NativeWidgetSizing.DefaultPreferredSize);

			Content = paned;
		}
	}
}
