// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;
using System.IO;

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

		internal class Extension : Filter
		{
			private string extension;

			internal Extension(string extension)
			{
				this.extension = extension.ToLowerInvariant();
			}

			public override bool Include(string filename)
			{
				return filename.ToLowerInvariant().EndsWith(this.extension);
			}
		}

		internal class SubDir : Filter
		{
			public override bool Include(string filename)
			{
				return Directory.Exists(filename);
			}
		}

		internal class ExcludeDir : Filter
		{
			private string dir;

			internal ExcludeDir(string dir)
			{
				this.dir = dir;
			}

			public override bool MustExclude(string filename)
			{
				if (!Directory.Exists(filename))
				{
					return false;
				}

				return Path.GetFileName(filename).Equals(this.dir);
			}	
		}
	}

}
