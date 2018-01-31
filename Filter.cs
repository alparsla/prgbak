// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;

namespace PrgBak
{
	public abstract class Filter
	{
		public abstract bool Include(string filename);

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
	}

}
