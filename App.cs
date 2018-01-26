using System;
using System.IO;
using static PrgBak.Log;

namespace PrgBak
{
	internal class App
	{
		private static string homePath;
		
		static void Main()
		{
			try
			{
				App.homePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + ".prgbak" + Path.DirectorySeparatorChar;
				Directory.CreateDirectory(App.homePath);
				Print("PrgBak started at " + App.HomePath);
			}
			catch (Exception e)
			{
				Print(e);
			}
			finally
			{
				Print("PrgBak ended");
			}
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
