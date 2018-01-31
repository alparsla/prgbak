// Copyright 2018 Savas Alparslan
// Distributed under the MIT License.
// (See accompanying file license.txt file or copy at http://opensource.org/licenses/MIT)
//
using System;

namespace PrgBak
{
	internal abstract class Filter
	{
		internal abstract bool Include(string filename);

		internal virtual bool MustInclude(string filename)
		{
			return false;
		}

		internal virtual bool Exclude(string filename)
		{
			return false;
		}

		internal virtual bool MustExclude(string filename)
		{
			return false;
		}
	}

}
