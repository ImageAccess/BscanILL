using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;


namespace BscanILLTester
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{


		}

		private void Form_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			try
			{
				Exception exception = e.Exception;
				string errorMessage = e.Exception.Message;

				while ((exception = exception.InnerException) != null)
					errorMessage += Environment.NewLine + exception.Message;

				MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				Application.Current.Shutdown();
				System.Diagnostics.Process.GetCurrentProcess().Kill();
				e.Handled = true;
			}
		}
		
		private void Application_Exit(object sender, ExitEventArgs e)
		{

		}
	}
}
