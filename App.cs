using System;
using System.IO;

namespace PrgBak
{
	internal class App
	{
		private static string homePath;
		
		static void Main()
		{
			App.homePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + ".prgbak" + Path.DirectorySeparatorChar;	
		}

		internal static string HomePath
		{
			get
			{
				return App.homePath;				
			}
		}
		
	}
}
