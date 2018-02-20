// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;
using System.IO;
using System.Xml;

namespace PrgBak
{
	public abstract class Target
	{
		public abstract void send(string zippath);

		public abstract void ToXml(XmlWriter xw);

		internal static Target FromXml(XmlCursor xr)
		{
			if (xr.IsElement("folder"))
			{
				return new Folder(xr);
			}
			else
			{
				xr.UnexpectedElement();
				return null;
			}
		}

		internal class Folder : Target
		{
			private string folder;
			private bool dayFolder;

			internal Folder(string folder, bool dayFolder)
			{
				this.folder = folder;
				this.dayFolder = dayFolder;
			}

			internal Folder(XmlCursor xr)
			{
				this.dayFolder = true;
				this.folder = xr.Text;
			}

			public override void ToXml(XmlWriter xw)
			{
				xw.WriteStartElement("folder");
				xw.WriteCData(this.folder);
				xw.WriteEndElement();
			}

			internal string FolderPath
			{
				get
				{
					return this.folder;
				}
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
