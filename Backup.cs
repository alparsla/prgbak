using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using static PrgBak.Log;

namespace PrgBak
{
	internal class Backup
	{
		private string name;
		private string zipname;
		private IList<Filter> filters;
		private string folder;
		private IList<Target> targets;

		internal Backup()
		{
			this.filters = new List<Filter>();
			this.targets = new List<Target>();
		}

		internal bool Do(long time)
		{
			if (this.zipname == null)
			{
				Print("Backup " + this.name + " has no zipname");
				return false;
			}

			Print("Start backup " + this.name + " to zip " + this.zipname);

			if (this.folder == null)
			{
				Print("No folders specified");
				return false;
			}

			string filename = App.HomePath + "temp" + Path.DirectorySeparatorChar + this.zipname + ".zip";
			Directory.CreateDirectory(App.HomePath + "temp");
			FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
			ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create, true, Encoding.UTF8);
			ZipFolder(zip, this.folder, time);
			zip.Dispose();
			fs.Flush();
			fs.Close();
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
				if (File.GetLastWriteTimeUtc(file).ToFileTimeUtc() <= time)
				{
					continue;
				}

				bool no = false;
				foreach (var filter in this.filters)
				{
					if (!filter.filter(file))
					{
						no = true;
						break;
					}
				}

				if (no)
				{
					continue;	
				}

				var bytes = File.ReadAllBytes(file);
				var ze = zip.CreateEntry(subdir + Path.GetFileName(file), CompressionLevel.Optimal);
				var stream = ze.Open();
				stream.Write(bytes, 0, bytes.Length);
				stream.Close();
			}

			var dirs = Directory.GetDirectories(folder);
			foreach (var dir in dirs)
			{
				ZipFolder(zip, dir, time);
			}
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
