// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;
using System.IO;
using System.Collections.Generic;
using static PrgBak.Log;
using System.Windows.Forms;
using System.Drawing;

namespace PrgBak
{
	internal class App : Form
	{
		private static string homePath;
		private static App appForm;

		private IList<Backup> backups;

		// Form
		private TextBox backupName;
		private TextBox sourceFolder;
		private TextBox destFolder;
		private TextBox extensions;
		private TextBox lastBackup;

		static void Main()
		{
			try
			{
				App.homePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + ".prgbak" + Path.DirectorySeparatorChar;
				Directory.CreateDirectory(App.homePath);
				Print("PrgBak started at " + App.HomePath);

				App.appForm = new App();
				appForm.Text = "Programmer's Backup";
				appForm.Width = 640;
				appForm.Height = 540;
				appForm.CenterToScreen();
				App.appForm.ShowDialog();
			}
			catch (Exception e)
			{
				Print(e);
			}
			finally
			{
				Print("PrgBak ended");
			}
		}
		
		App()
		{
			var tab = new TabControl();
			tab.Dock = DockStyle.Fill;
			this.Controls.Add(tab);

			this.backups = new List<Backup>();

			AddBackupTab(tab);
			AddLogTab(tab);
		}

		internal static string HomePath
		{
			get
			{
				return App.homePath;				
			}
		}

		void AddBackupTab(TabControl tab)
		{
			var page = new TabPage("Backup");
			tab.TabPages.Add(page);

			TableLayoutPanel table = new TableLayoutPanel();
			table.Dock = DockStyle.Fill;
			table.ColumnCount = 1;
			table.RowCount = 2;

			var list = new ListBox();
			list.Dock = DockStyle.Fill;
			table.Controls.Add(list);
			table.SetRow(list, 0);
			table.SetColumn(list, 0);

			table.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
			table.RowStyles.Add(new RowStyle(SizeType.Percent, 70));

			page.Controls.Add(table);

			var panel = new FlowLayoutPanel();
			panel.FlowDirection = FlowDirection.TopDown;
			panel.Dock = DockStyle.Fill;
			table.Controls.Add(panel);
			table.SetRow(panel, 1);
			table.SetColumn(panel, 0);

			Label label;

			label = new Label();
			label.Text = "Name of the zip file";
			label.Width = 200;
			label.TextAlign = ContentAlignment.BottomLeft;
			panel.Controls.Add(label);

			this.backupName = new TextBox();
			this.backupName.Width = 300;
			panel.Controls.Add(this.backupName);

			label = new Label();
			label.Text = "Source Folder";
			label.TextAlign = ContentAlignment.BottomLeft;
			panel.Controls.Add(label);
			this.sourceFolder = new TextBox();
			this.sourceFolder.Width = 300;
			panel.Controls.Add(this.sourceFolder);

			label = new Label();
			label.Text = "Dest Folder";
			label.TextAlign = ContentAlignment.BottomLeft;
			panel.Controls.Add(label);
			this.destFolder = new TextBox();
			this.destFolder.Width = 300;
			panel.Controls.Add(this.destFolder);

			label = new Label();
			label.Text = "Extensions (comma separated)";
			label.Width = 200;
			label.TextAlign = ContentAlignment.BottomLeft;
			panel.Controls.Add(label);
			this.extensions = new TextBox();
			this.extensions.Width = 300;
			panel.Controls.Add(this.extensions);

			Button button = new Button();
			button.Text = "Do Full Backup";
			button.Width = 150;
			button.Click += (sender, e) => DoBackup(true);
			panel.Controls.Add(button);

			label = new Label();
			label.Text = "Last Backup Date";
			label.Width = 100;
			label.TextAlign = ContentAlignment.BottomLeft;
			panel.Controls.Add(label);
			this.lastBackup = new TextBox();
			this.lastBackup.Width = 200;
			panel.Controls.Add(this.lastBackup);

			button = new Button();
			button.Text = "Backup Diff";
			button.Width = 150;
			button.Click += (sender, e) => DoBackup(false);
			panel.Controls.Add(button);
		}

		private void DoBackup(bool full)
		{
			MessageBox.Show("Hello");
		}
	}
}

