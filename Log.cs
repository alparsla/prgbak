﻿// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace PrgBak
{
	public class Log
	{
		private static FileStream stream;
		private static TabPage tabPage;
		private static ListBox listbox;

		static Log()
		{
			stream = new FileStream(App.HomePath + "prgbak.log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
		}
		
		public static void Print(object o)
		{
			var line = DateTime.UtcNow + " " + o + "\n";
			var bytes = Encoding.UTF8.GetBytes(line);
			int offset;
			lock (stream)
			{
				offset = (int)stream.Seek(0, SeekOrigin.End);
				stream.Write(bytes, 0, bytes.Length);
				stream.Flush();
			}

			if (listbox != null)
			{
				listbox.Items.Add(new Line(offset, bytes.Length));
				listbox.SelectedIndex = listbox.Items.Count - 1;
			}
		}
		
		internal static void AddLogTab(TabControl tab)
		{
			tabPage = new TabPage("Log");
			tab.TabPages.Add(tabPage);
			
			listbox = new ListBox();
			listbox.Dock = DockStyle.Fill;
			tabPage.Controls.Add(listbox);

			FillLog();
		}

		internal static void SwitchToTabPage()
		{
			(tabPage.Parent as TabControl).SelectedTab = tabPage;
		}
		
		static void FillLog()
		{
			stream.Seek(0, SeekOrigin.Begin);
			var wholeFile = new byte[stream.Length];
			stream.Read(wholeFile, 0, wholeFile.Length);

			int start = 0;
			for (int i = 0; i < wholeFile.Length; ++i)
			{
				var b = wholeFile[i];
				switch (b)
				{
					case 13:
					case 10:
						if (start != i)
						{
							listbox.Items.Add(new Line(start, i - start));
							start = i;		
						}
						break;
				}
			}

			listbox.SelectedIndex = listbox.Items.Count - 1;
			listbox.TopIndex = listbox.Items.Count - 1;
		}

		class Line
		{
			private int offset;
			private int length;

			internal Line(int offset, int length)
			{
				this.offset = offset;
				this.length = length;
			}

			internal int Offset
			{
				get
				{
					return offset;
				}
			}

			internal int Length
			{
				get
				{
					return length;
				}
			}

			public override string ToString()
			{
				byte[] bytes = new byte[this.length];
				lock (Log.stream)
				{
					Log.stream.Seek(this.offset, SeekOrigin.Begin);
					Log.stream.Read(bytes, 0, this.length);
				}
				return Encoding.UTF8.GetString(bytes);
			}
		}
	}
}