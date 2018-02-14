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
		private Backup current;

		// Form
		private ListView listView;
		private FlowLayoutPanel editPanel;
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
			table.ColumnCount = 2;
			table.RowCount = 2;

			table.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
			table.RowStyles.Add(new RowStyle(SizeType.Percent, 70));

			table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
			table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

			this.listView = new ListView();
			this.listView .Dock = DockStyle.Fill;
			table.Controls.Add(this.listView );
			table.SetRow(this.listView , 0);
			table.SetColumn(this.listView , 0);
			table.SetColumnSpan(this.listView , 2);

			this.listView .HeaderStyle = ColumnHeaderStyle.Clickable;
			this.listView .FullRowSelect = true;
			this.listView .View = View.Details;
			this.listView .Columns.Add("name", "Name", 140);
			this.listView .Columns.Add("source", "Source Folder", 320);
			this.listView .Columns.Add("date", "Last Backup", 140);


			page.Controls.Add(table);

			this.editPanel = new FlowLayoutPanel();
			this.editPanel.FlowDirection = FlowDirection.TopDown;
			this.editPanel.Dock = DockStyle.Fill;
			table.Controls.Add(this.editPanel);
			table.SetRow(this.editPanel, 1);
			table.SetColumn(this.editPanel, 0);

			Label label;

			label = new Label();
			label.Text = "Name of the zip file";
			label.Width = 200;
			label.TextAlign = ContentAlignment.BottomLeft;
			this.editPanel.Controls.Add(label);

			this.backupName = new TextBox();
			this.backupName.Width = 300;
			this.editPanel.Controls.Add(this.backupName);

			label = new Label();
			label.Text = "Source Folder";
			label.TextAlign = ContentAlignment.BottomLeft;
			this.editPanel.Controls.Add(label);
			this.sourceFolder = new TextBox();
			this.sourceFolder.Width = 300;
			this.editPanel.Controls.Add(this.sourceFolder);

			label = new Label();
			label.Text = "Dest Folder";
			label.TextAlign = ContentAlignment.BottomLeft;
			this.editPanel.Controls.Add(label);
			this.destFolder = new TextBox();
			this.destFolder.Width = 300;
			this.editPanel.Controls.Add(this.destFolder);

			label = new Label();
			label.Text = "Extensions (comma separated)";
			label.Width = 200;
			label.TextAlign = ContentAlignment.BottomLeft;
			this.editPanel.Controls.Add(label);
			this.extensions = new TextBox();
			this.extensions.Width = 300;
			this.editPanel.Controls.Add(this.extensions);

			Button button = new Button();
			button.Text = "Do Full Backup";
			button.Width = 150;
			button.Click += (sender, e) => DoBackup(true);
			this.editPanel.Controls.Add(button);

			label = new Label();
			label.Text = "Last Backup Date";
			label.Width = 100;
			label.TextAlign = ContentAlignment.BottomLeft;
			this.editPanel.Controls.Add(label);
			this.lastBackup = new TextBox();
			this.lastBackup.Width = 200;
			this.editPanel.Controls.Add(this.lastBackup);

			button = new Button();
			button.Text = "Backup Diff";
			button.Width = 150;
			button.Click += (sender, e) => DoBackup(false);
			this.editPanel.Controls.Add(button);

			if (this.backups.Count == 0)
			{
				foreach (Control ctrl in this.editPanel.Controls)
				{
					ctrl.Enabled = false;
				}
			}
			else
			{
				this.listView.FocusedItem = this.listView.Items[0];
			}

			var panel = new FlowLayoutPanel();
			panel.FlowDirection = FlowDirection.TopDown;
			panel.Dock = DockStyle.Fill;
			table.Controls.Add(panel);
			table.SetRow(panel, 1);
			table.SetColumn(panel, 1);

			button = new Button();
			button.Text = "New";
			button.Width = 100;
			button.Click += (sender, e) => New();
			panel.Controls.Add(button);

			button = new Button();
			button.Text = "Delete";
			button.Width = 100;
			button.Click += (sender, e) => Delete();
			panel.Controls.Add(button);

		}

		private void DoBackup(bool full)
		{
			MessageBox.Show("Hello");
		}

		private void New()
		{
			this.current = new Backup();
			this.backups.Add(this.current);
			AddToList(this.current);
		}

		private void Delete()
		{
			MessageBox.Show("Delete");
		}

		private void AddToList(Backup backup)
		{
			this.listView.Items.Add(BackupToListViewItem(backup));
		}

		private ListViewItem BackupToListViewItem(Backup backup)
		{
			var lvi = new ListViewItem();
			lvi.Tag = backup;
			lvi.Text = backup.Name;

			if (backup.Targets.Count > 0)
			{
				if (backup.Targets.Count > 1)
				{
					throw new ApplicationException("Backup " + backup.Name + " has " + 
					                               backup.Targets.Count + " targets and it is not supported yet");
				}
				lvi.SubItems.Add((backup.Targets[0] as Target.Folder).FolderPath);
			}
			else
			{
				lvi.SubItems.Add("<empty path>");
			}

			if (backup.LastBackup != 0)
			{
				lvi.SubItems.Add(DateTime.FromBinary(backup.LastBackup).ToLongDateString());
			}
			else
			{
				lvi.SubItems.Add("<not backed up yet>");
			}

			return lvi;
		}
	}
}

