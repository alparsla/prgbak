﻿// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;
using System.IO;
using System.Collections.Generic;
using static PrgBak.Log;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;

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
		private bool dirty;
		private bool loading;
		private int selectedIndex;

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

				if (appForm.dirty)
				{
					App.appForm.ReflectChanges();
					App.appForm.WritePrgBakXml();
				}
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
			ReadPrgBakXml();

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

		private void ReadPrgBakXml()
		{
			string path = homePath + "prgbak.xml";
			if (!File.Exists(path))
			{
				return;
			}

			var xml = File.ReadAllText(path);
			var xr = new XmlCursor(xml);
			if (!xr.MoveNext())
			{
				throw new XmlException(path + " is not a valid xml file");
			}

			if (!xr.IsElement("prgbak"))
			{
				xr.UnexpectedElement();
			}

			if (xr.MoveIn())
			{
				while (xr.MoveNext())
				{
					if (xr.IsElement("backups"))
					{
						if (xr.MoveIn())
						{
							while (xr.MoveNext())
							{
								if (xr.IsElement("backup"))
								{
									this.backups.Add(new Backup(xr));
								}
							}

							xr.MoveOut();
						}
					}
					else
					{
						xr.UnexpectedElement();
					}
				}

				xr.MoveOut();
			} // Move into <prgbak>
		}

		private void WritePrgBakXml()
		{
			var ms = new MemoryStream();
			XmlTextWriter xw = new XmlTextWriter(new StreamWriter(ms));
			xw.Formatting = Formatting.Indented;

			xw.WriteStartDocument(true);
			xw.WriteStartElement("prgbak");

			xw.WriteStartElement("backups");
			foreach (var backup in this.backups)
			{
				backup.ToXml(xw);
			}
			xw.WriteEndElement();

			xw.WriteEndElement(); // <prgbak>
			xw.WriteEndDocument();
			xw.Flush();

			string path = homePath + "prgbak.xml";
			Print("Writing " + path);
			File.WriteAllBytes(path, ms.GetBuffer());

			this.dirty = false;
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
			this.listView.SelectedIndexChanged += (sender, e) => HandleSelectedIndexChanged();
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
			this.backupName.TextChanged += (sender, e) => HandleEditChange(this.backupName.Text, -1);
			this.backupName.TextChanged += (sender, e) => SetDirty();
			this.backupName.Width = 300;
			this.editPanel.Controls.Add(this.backupName);

			label = new Label();
			label.Text = "Source Folder";
			label.TextAlign = ContentAlignment.BottomLeft;
			this.editPanel.Controls.Add(label);
			this.sourceFolder = new TextBox();
			this.sourceFolder.TextChanged += (sender, e) => HandleEditChange(this.sourceFolder.Text, 1);
			this.sourceFolder.TextChanged += (sender, e) => SetDirty();
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
			this.extensions.TextChanged += (sender, e) => SetDirty();
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
			this.lastBackup.TextChanged += (sender, e) => SetDirty();
			this.lastBackup.Width = 200;
			this.editPanel.Controls.Add(this.lastBackup);

			button = new Button();
			button.Text = "Backup Diff";
			button.Width = 150;
			button.Click += (sender, e) => DoBackup(false);
			this.editPanel.Controls.Add(button);

			if (this.backups.Count == 0)
			{
				this.selectedIndex = -1;
				foreach (Control ctrl in this.editPanel.Controls)
				{
					ctrl.Enabled = false;
				}
			}
			else
			{
				foreach (var backup in this.backups)
				{
					AddToList(backup);
				}

				LoadBackup(this.backups[0]);
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

		private void HandleEditChange(string text, int index)
		{
			if (this.loading)
			{
				return;
			}

			if (index == -1)
			{
				this.listView.FocusedItem.Text = text;
			}
			else
			{
				this.listView.FocusedItem.SubItems[index].Text = text;
				this.listView.Refresh();
			}
		}

		private void SetDirty()
		{
			if (this.loading)
			{
				return;
			}

			this.dirty = true;
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

			this.selectedIndex = this.listView.FocusedItem.Index;
			LoadBackup(this.backups[this.selectedIndex]);

			foreach (Control ctrl in this.editPanel.Controls)
			{
				ctrl.Enabled = true;
			}
		}

		private void Delete()
		{
			MessageBox.Show("Delete");
		}

		private void AddToList(Backup backup)
		{
			var item = BackupToListViewItem(backup);
			this.listView.Items.Add(item);
			this.listView.FocusedItem = item;
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

		private void HandleSelectedIndexChanged()
		{
			if (this.dirty)
			{
				ReflectChanges();
				WritePrgBakXml(); // Save it instantly
			}

			this.selectedIndex = this.listView.FocusedItem.Index;
			LoadBackup(this.backups[this.selectedIndex]);
		}

		private void LoadBackup(Backup backup)
		{
			try
			{
				this.loading = true;
				this.backupName.Text = backup.Name;
			}
			finally
			{
				this.loading = false;
			}

		}

		private void ReflectChanges()
		{
			this.backups[this.selectedIndex].Name = this.backupName.Text;
		}
	}
}

