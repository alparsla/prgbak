// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;
using System.IO;

namespace PrgBak
{
	public abstract class Target
	{
		public abstract void send(string zippath);
	
		internal class Folder : Target
		{
			private string folder;
			private bool dayFolder;

			internal Folder(string folder, bool dayFolder)
			{
				this.folder = folder;
				this.dayFolder = dayFolder;
			}

			public override void send(string zippath)
			{
				var now = DateTime.Now;
				string sub = now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00");

				string dir = this.folder;
				if (this.dayFolder)
				{
					dir += Path.DirectorySeparatorChar + sub;
				}
				Directory.CreateDirectory(dir);

				var timepostfix = sub + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
				File.Copy(zippath, dir + Path.DirectorySeparatorChar + 
				          Path.GetFileNameWithoutExtension(zippath) + timepostfix + Path.GetExtension(zippath));
			}
		}
	}
}
