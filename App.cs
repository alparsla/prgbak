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
				appForm.Text = "Programmer's Backup";
				appForm.Width = 640;
				appForm.Height = 480;
				appForm.CenterToScreen();
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
			var tab = new TabControl();
			tab.Dock = DockStyle.Fill;
			this.Controls.Add(tab);
		
			AddLogTab(tab);
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
