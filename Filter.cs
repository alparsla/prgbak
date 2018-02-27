// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;
using System.IO;
using System.Xml;

namespace PrgBak
{
	public abstract class Filter
	{
		public virtual bool Include(string filename)
		{
			return false;
		}

		public virtual bool MustInclude(string filename)
		{
			return false;
		}

		public virtual bool Exclude(string filename)
		{
			return false;
		}

		public virtual bool MustExclude(string filename)
		{
			return false;
		}

		public abstract void ToXml(XmlWriter xw);

		internal static Filter FromXml(XmlCursor xr)
		{
			if (xr.IsElement("extension"))
			{
				return new Extension(xr);
			}
			else
			{
				xr.UnexpectedElement();
				return null;
			}
		}


		internal class Extension : Filter
		{
			private string extension;

			internal Extension(string extension)
			{
				this.extension = extension.ToLowerInvariant();

				// Tolerate known styles
				if (this.extension.StartsWith("."))
				{
					this.extension = this.extension.Substring(1);
				}
				else if (this.extension.StartsWith("*."))
				{
					this.extension = this.extension.Substring(2);
				}
			}

			internal string Ext
			{
				get
				{
					return this.extension;
				}
			}

			internal Extension(XmlCursor xr) : this(xr.Text)
			{
			}

			public override void ToXml(XmlWriter xw)
			{
				xw.WriteStartElement("extension");
				xw.WriteCData(this.extension);
				xw.WriteEndElement();
			}

			public override bool Include(string filename)
			{
				return filename.ToLowerInvariant().EndsWith("." + this.extension);
			}
		}

		internal class IncludeSubFolders : Filter
		{
			public override bool Include(string filename)
			{
				return Directory.Exists(filename);
			}

			public override void ToXml(XmlWriter xw)
			{
				throw new NotImplementedException();
			}
		}

		internal class ExcludeFolder : Filter
		{
			private string folder;

			public override void ToXml(XmlWriter xw)
			{
				throw new NotImplementedException();
			}

			internal ExcludeFolder(string folder)
			{
				this.folder = folder;
			}

			public override bool MustExclude(string filename)
			{
				if (!Directory.Exists(filename))
				{
					return false;
				}

				return Path.GetFileName(filename).Equals(this.folder);
			}	
		}
	}

}
