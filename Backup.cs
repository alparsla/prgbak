using System;
using System.Collections.Generic;
using static PrgBak.Log;

namespace PrgBak
{
	internal class Backup
	{
		private string name;
		private string zipname;
		private IList<Filter> filters;
		private IList<string> folders;
		private IList<Target> targets;

		internal Backup()
		{
			this.filters = new List<Filter>();
			this.folders = new List<string>();
			this.targets = new List<Target>();
		}

		internal bool Do()
		{
			if (this.zipname == null)
			{
				Print("Backup " + this.name + " has no zipname");
				return false;
			}

			Print("Start backup " + this.name + " to zip " + this.zipname);

			if (this.folders.Count == 0)
			{
				Print("No folders specified");
				return false;
			}

			return true;
		}

		internal void AddFilter(Filter filter)
		{
			this.filters.Add(filter);
		}

		internal void AddFolder(string folder)
		{
			this.folders.Add(folder);
		}

		internal void AddTarget(Target target)
		{
			this.targets.Add(target);
		}
	}
}
