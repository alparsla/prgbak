// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using static PrgBak.Log;

namespace PrgBak
{
	internal class Backup
	{
		private string name;
		private string zipname;
		private List<Filter> filters;
		private string folder;
		private List<Target> targets;
		private long lastBackup;

		internal Backup()
		{
			this.filters = new List<Filter>();
			this.targets = new List<Target>();
			this.lastBackup = long.MinValue;
		}

		internal Backup(XmlCursor xr) : this()
		{
			if (!xr.MoveIn())
			{
				throw new XmlException("<backup> should have sub items");
			}

			while (xr.MoveNext())
			{
				if (xr.IsElement("name"))
				{
					this.name = xr.Text;
				}
				else if (xr.IsElement("zipname"))
				{
					this.zipname = xr.Text;
				}
				else if (xr.IsElement("folder"))
				{
					this.folder = xr.Text;
				}
				else if (xr.IsElement("lastBackup"))
				{
					this.lastBackup = long.Parse(xr.Text);
				}
				else if (xr.IsElement("filters"))
				{
					if (xr.MoveIn())
					{
						while (xr.MoveNext())
						{
							AddFilter(Filter.FromXml(xr));
						}
						xr.MoveOut();	
					}
				}
				else if (xr.IsElement("targets"))
				{
					if (xr.MoveIn())
					{
						while (xr.MoveNext())
						{
							AddTarget(Target.FromXml(xr));
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
		}

		internal void ToXml(XmlWriter xw)
		{
			xw.WriteStartElement("backup");

			xw.WriteStartElement("name");
			xw.WriteCData(this.name);
			xw.WriteEndElement();

			xw.WriteStartElement("zipname");
			xw.WriteCData(this.zipname);
			xw.WriteEndElement();

			xw.WriteStartElement("folder");
			xw.WriteCData(this.folder);
			xw.WriteEndElement();

			xw.WriteStartElement("lastBackup");
			xw.WriteCData(this.lastBackup.ToString());
			xw.WriteEndElement();

			xw.WriteStartElement("filters");
			foreach (var filter in this.filters)
			{
				filter.ToXml(xw);
			}
			xw.WriteEndElement();

			xw.WriteStartElement("targets");
			foreach (var target in this.targets)
			{
				target.ToXml(xw);
			}
			xw.WriteEndElement();

			xw.WriteEndElement(); // <backup>
		}

		internal string Name
		{
			get
			{
				return this.name;
			}

			set
			{
				this.name = value;
				if (this.zipname == null)
				{
					this.zipname = name;
				}
			}
		}

		internal long LastBackup
		{
			get
			{
				return this.lastBackup;
			}

			set
			{
				this.lastBackup = value;
			}
		}

		internal string LastBackupText
		{
			get
			{
				if (this.lastBackup == long.MinValue)
				{
					return "";
				}

				return DateTime.FromBinary(this.lastBackup).ToString();
			}
		}

		internal string Folder
		{
			get
			{
				return this.folder;
			}

			set
			{
				this.folder = value;
			}
		}

		internal IList<Target> Targets
		{
			get
			{
				return this.targets.AsReadOnly();
			}
		}

		internal IList<Filter> Filters
		{
			get
			{
				return this.filters.AsReadOnly();
			}
		}

		internal void ClearFilters()
		{
			this.filters.Clear();
		}

		internal void ClearTargets()
		{
			this.targets.Clear();
		}

		internal bool Do(long time)
		{
			if (this.zipname == null)
			{
				Print("Backup " + this.name + " has no zipname");
				return false;
			}

			Print("Start backing up " + this.name + " to zip " + this.zipname);

			if (this.folder == null)
			{
				Print("No folders specified");
				return false;
			}

			string zipname = this.zipname;
			if (time != long.MinValue)
			{
				var now = DateTime.FromBinary(time);
				string sub = now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00");
				var timepostfix = sub + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
				zipname += timepostfix + "-";
			}

			string filename = App.HomePath + "temp" + Path.DirectorySeparatorChar + zipname + ".zip";
			Directory.CreateDirectory(App.HomePath + "temp");
			FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
			ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create, true, Encoding.UTF8);
			Print("Zipping " + this.folder + " to " + filename);
			ZipFolder(zip, this.folder, time);
			zip.Dispose();
			fs.Flush();
			fs.Close();

			foreach (var target in this.targets)
			{
				target.send(filename);
			}

			this.lastBackup = DateTime.Now.ToBinary();

			Print("Backup ended successfully");
			return true;
		}

		private void ZipFolder(ZipArchive zip, string folder, long time)
		{
			string[] files = Directory.GetFiles(folder);
			string subdir = folder.Substring(this.folder.Length);
			if (subdir.StartsWith(Path.DirectorySeparatorChar.ToString()))
			{
				subdir = subdir.Substring(1);
			}
			if (subdir.Length > 0)
			{
				subdir += Path.DirectorySeparatorChar;
			}

			foreach (var file in files)
			{
				var lastWriteTime = File.GetLastWriteTime(file).ToBinary();
				if (lastWriteTime <= time)
				{
					continue;
				}

				if (Eliminate(file, false))
				{
					continue;
				}

				var bytes = File.ReadAllBytes(file);
				var ze = zip.CreateEntry(subdir + Path.GetFileName(file), CompressionLevel.Optimal);
				ze.LastWriteTime = DateTime.FromBinary(lastWriteTime);
				var stream = ze.Open();
				stream.Write(bytes, 0, bytes.Length);
				stream.Close();
			}

			var dirs = Directory.GetDirectories(folder);
			foreach (var dir in dirs)
			{
				if (Eliminate(dir, true))
				{
					continue;
				}

				ZipFolder(zip, dir, time);
			}
		}

		private bool Eliminate(string filename, bool dir)
		{
			bool eliminate = true;
			foreach (var filter in this.filters)
			{
				if (dir && !filter.CheckDirectory)
				{
					return false;	
				}

				if (filter.MustExclude(filename))
				{
					return true;
				}
				else if (filter.MustInclude(filename))
				{
					return false;
				}
				else if (filter.Include(filename))
				{
					eliminate = false;
				}
				else if (filter.Exclude(filename))
				{
					eliminate = true;
				}
			}

			return eliminate;
		}

		internal void AddFilter(Filter filter)
		{
			this.filters.Add(filter);
		}

		internal void SetFolder(string folder)
		{
			this.folder = folder;
		}

		internal void AddTarget(Target target)
		{
			this.targets.Add(target);
		}

		internal void SetNames(string name, string zipname)
		{
			this.name = name;
			this.zipname = zipname;
		}
	}
}
