using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace WpfTester
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void OnStartup(object sender, StartupEventArgs e)
		{
			//System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-ES");
			//System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es");

			try
			{
				//if (e.Args.Length > 0 && e.Args[0] == "/s")
				{
					this.StartupUri = new Uri("/WpfTester;component/Window1.xaml", UriKind.Relative);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
			}
		}
	}
}
