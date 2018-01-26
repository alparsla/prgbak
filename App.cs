using System;
using System.IO;
using static PrgBak.Log;
using System.Windows.Forms;

namespace PrgBak
{
	internal class App : Form
	{
		private static string homePath;
		private static App appForm;
		
		static void Main()
		{
			try
			{
				App.homePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + ".prgbak" + Path.DirectorySeparatorChar;
				Directory.CreateDirectory(App.homePath);
				Print("PrgBak started at " + App.HomePath);
				
				App.appForm = new App();
				App.appForm.ShowDialog();
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
		
		App()
		{
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
