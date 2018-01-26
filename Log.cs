using System;
using System.IO;

namespace PrgBak
{
	internal class Log
	{
		private static string path;
		
		static Log()
		{
			Log.path = App.HomePath + "prgbak.log";
		}
		
		internal static void Print(object o)
		{
			File.AppendAllText(Log.path, DateTime.UtcNow + " " + o + "\n");
		}
	}
}