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
using System.Xml;

namespace PrgBak
{
	internal class App : Form
	{
		private static string homePath;
		private static App appForm;

		private IList<Backup> backups;

		// Form
		private ListView listView;
		private int selectedIndex;
		private BackupEditPanel editPanel;

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

				if (appForm.editPanel.Dirty)
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

			this.editPanel.Undirty();
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

			this.editPanel = new BackupEditPanel(this);
			this.editPanel.FlowDirection = FlowDirection.TopDown;
			this.editPanel.Dock = DockStyle.Fill;
			table.Controls.Add(this.editPanel);
			table.SetRow(this.editPanel, 1);
			table.SetColumn(this.editPanel, 0);

			if (this.backups.Count == 0)
			{
				this.selectedIndex = -1;
				this.editPanel.Enable(false);
			}
			else
			{
				foreach (var backup in this.backups)
				{
					AddToList(backup);
				}

				this.editPanel.LoadBackup(this.backups[0]);
				this.listView.FocusedItem = this.listView.Items[0];
			}


			var panel = new FlowLayoutPanel();
			panel.FlowDirection = FlowDirection.TopDown;
			panel.Dock = DockStyle.Fill;
			table.Controls.Add(panel);
			table.SetRow(panel, 1);
			table.SetColumn(panel, 1);

			Button button = new Button();
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
			if (this.editPanel.IsLoading)
			{
				return;
			}

			ListViewItem item = this.listView.FocusedItem;
			if (item == null)
			{
				item = this.listView.Items[this.selectedIndex];
			}

			if (index == -1)
			{
				item.Text = text;
			}
			else
			{
				item.SubItems[index].Text = text;
				this.listView.Refresh();
			}
		}

		private void DoBackup(bool full)
		{
			if (this.selectedIndex == -1)
			{
				return;
			}

			if (this.editPanel.Dirty)
			{
				ReflectChanges();
			}

			Backup backup = this.backups[this.selectedIndex];
			try
			{
				Log.SwitchToTabPage();
				Cursor.Current = Cursors.WaitCursor;
				Application.DoEvents();
				backup.Do(full ? long.MinValue : backup.LastBackup);
			}
			catch (Exception e)
			{
				Print("Exception occured while backing up " + backup.Name + ": " + e);
				MessageBox.Show(e.Message, "Error while backing up " + backup.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}

			// The following are for the updated lastBackup value
			this.editPanel.LastBackup = backup.LastBackup;
			HandleEditChange(backup.LastBackupText, 2);
			WritePrgBakXml();
		}

		private void New()
		{
			var backup = new Backup();
			this.backups.Add(backup);
			AddToList(backup);

			foreach (var ext in Filter.Extension.defaultExtensions)
			{
				backup.AddFilter(new Filter.Extension(ext));
			}

			this.selectedIndex = this.listView.FocusedItem.Index;
			this.editPanel.LoadBackup(this.backups[this.selectedIndex]);

			foreach (Control ctrl in this.editPanel.Controls)
			{
				ctrl.Enabled = true;
			}
		}

		private void Delete()
		{
			if (this.selectedIndex == -1)
			{
				return;
			}

			this.editPanel.SetDirty();

			this.editPanel.StartLoading();
			this.backups.RemoveAt(this.selectedIndex);
			this.listView.Items.RemoveAt(this.selectedIndex);

			if (this.selectedIndex == 0)
			{
				if (this.backups.Count == 0)
				{
					this.selectedIndex = -1;
				}
			}
			else if (this.selectedIndex > this.backups.Count - 2)
			{
				this.selectedIndex--;
			}

			this.editPanel.EndLoading();

			WritePrgBakXml();

			if (this.selectedIndex != -1)
			{
				this.listView.FocusedItem = this.listView.Items[this.selectedIndex];
				ReflectChanges();
			}
			else
			{
				foreach (Control ctrl in this.editPanel.Controls)
				{
					ctrl.Enabled = false;
				}
			}
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

			if (!string.IsNullOrWhiteSpace(backup.Folder))
			{
				lvi.SubItems.Add(backup.Folder);
			}
			else
			{
				lvi.SubItems.Add("<empty path>");
			}

			if (backup.LastBackup != long.MinValue)
			{
				lvi.SubItems.Add(DateTime.FromBinary(backup.LastBackup).ToString());
			}
			else
			{
				lvi.SubItems.Add("<not backed up yet>");
			}

			return lvi;
		}

		private void HandleSelectedIndexChanged()
		{
			if (this.editPanel.IsLoading)
			{
				return;
			}

			if (this.editPanel.Dirty)
			{
				ReflectChanges();
				WritePrgBakXml(); // Save it instantly
			}

			this.selectedIndex = this.listView.FocusedItem.Index;
			this.editPanel.LoadBackup(this.backups[this.selectedIndex]);
		}


		private void ReflectChanges()
		{
			Backup backup = this.backups[this.selectedIndex];

			backup.Name = this.editPanel.BackupName;
			backup.Folder = this.editPanel.SourceFolder;

			backup.LastBackup = long.MinValue;
			if (this.editPanel.LastBackup != 0)
			{
				backup.LastBackup = this.editPanel.LastBackup;
			}

			backup.ClearFilters();
			char[] seps = {','};
			string[] exts = this.editPanel.Extensions.Split(seps);
			foreach (var ext in exts)
			{
				backup.AddFilter(new Filter.Extension(ext.Trim()));
			}

			backup.ClearTargets();
			backup.AddTarget(new Target.Folder(this.editPanel.DestFolder, true));
		}

		protected internal class BackupEditPanel : FlowLayoutPanel
		{
			private TextBox backupName;
			private TextBox sourceFolder;
			private TextBox destFolder;
			private TextBox extensions;
			private TextBox lastBackup;
			private bool dirty;
			private bool loading;

			protected internal BackupEditPanel(App app)
			{
				Label label;

				label = new Label();
				label.Text = "Name of the zip file";
				label.Width = 200;
				label.TextAlign = ContentAlignment.BottomLeft;
				Controls.Add(label);

				this.backupName = new TextBox();
				this.backupName.TextChanged += (sender, e) => app.HandleEditChange(this.backupName.Text, -1);
				this.backupName.TextChanged += (sender, e) => SetDirty();
				this.backupName.Width = 300;
				Controls.Add(this.backupName);

				label = new Label();
				label.Text = "Source Folder";
				label.TextAlign = ContentAlignment.BottomLeft;
				Controls.Add(label);
				this.sourceFolder = new TextBox();
				this.sourceFolder.TextChanged += (sender, e) => app.HandleEditChange(this.sourceFolder.Text, 1);
				this.sourceFolder.TextChanged += (sender, e) => SetDirty();
				this.sourceFolder.Width = 300;
				Controls.Add(this.sourceFolder);

				label = new Label();
				label.Text = "Dest Folder";
				label.TextAlign = ContentAlignment.BottomLeft;
				Controls.Add(label);
				this.destFolder = new TextBox();
				this.destFolder.TextChanged += (sender, e) => SetDirty();
				this.destFolder.Width = 300;
				Controls.Add(this.destFolder);

				label = new Label();
				label.Text = "Extensions (comma separated)";
				label.Width = 200;
				label.TextAlign = ContentAlignment.BottomLeft;
				Controls.Add(label);
				this.extensions = new TextBox();
				this.extensions.TextChanged += (sender, e) => SetDirty();
				this.extensions.Width = 300;
				Controls.Add(this.extensions);

				Button button = new Button();
				button.Text = "Do Full Backup";
				button.Width = 150;
				button.Click += (sender, e) => app.DoBackup(true);
				Controls.Add(button);

				label = new Label();
				label.Text = "Last Backup Date";
				label.Width = 100;
				label.TextAlign = ContentAlignment.BottomLeft;
				Controls.Add(label);
				this.lastBackup = new TextBox();
				this.lastBackup.TextChanged += (sender, e) => SetDirty();
				this.lastBackup.Width = 200;
				Controls.Add(this.lastBackup);

				button = new Button();
				button.Text = "Backup Diff";
				button.Width = 150;
				button.Click += (sender, e) => app.DoBackup(false);
				Controls.Add(button);
			}

			protected internal string BackupName
			{
				get
				{
					return this.backupName.Text;
				}
			}

			protected internal string SourceFolder
			{
				get
				{
					return this.sourceFolder.Text;
				}
			}

			protected internal long LastBackup
			{
				get
				{
					if (string.IsNullOrWhiteSpace(this.lastBackup.Text))
					{
						return 0;
					}

					return DateTime.Parse(this.lastBackup.Text).ToBinary();
				}

				set
				{
					if (value == long.MinValue)
					{
						this.lastBackup.Text = "";
					}
					else
					{
						this.lastBackup.Text = DateTime.FromBinary(value).ToString();
					}
				}
			}

			protected internal string DestFolder
			{
				get
				{
					return this.destFolder.Text;
				}
			}

			protected internal string Extensions
			{
				get
				{
					return this.extensions.Text;
				}
			}

			protected internal void Enable(bool enable)
			{
				foreach (Control ctrl in Controls)
				{
					ctrl.Enabled = enable;
				}
			}

			protected internal void SetDirty()
			{
				if (this.loading)
				{
					return;
				}

				this.dirty = true;
			}

			protected internal void Undirty()
			{
				this.dirty = false;
			}

			protected internal bool Dirty
			{
				get
				{
					return this.dirty;
				}
			}

			protected internal bool IsLoading
			{
				get
				{
					return this.loading;
				}
			}

			protected internal void StartLoading()
			{
				this.loading = true;
			}

			protected internal void EndLoading()
			{
				this.loading = false;
			}

			protected internal void LoadBackup(Backup backup)
			{
				try
				{
					StartLoading();
					this.backupName.Text = backup.Name;
					this.sourceFolder.Text = backup.Folder;
					if (backup.Targets.Count > 0)
					{
						this.destFolder.Text = (backup.Targets[0] as Target.Folder).FolderPath;
					}
					else
					{
						this.destFolder.Text = "";
					}

					string extensions = "";
					bool first = true;
					foreach (var filter in backup.Filters)
					{
						if (first)
						{
							first = false;
						}
						else
						{
							extensions += ", ";
						}
						extensions += (filter as Filter.Extension).Ext;
					}
					this.extensions.Text = extensions;

					this.LastBackup = backup.LastBackup;
				}
				finally
				{
					EndLoading();
				}

			}

		}
	}
}

