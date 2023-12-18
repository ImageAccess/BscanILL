using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BscanILL
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		BscanILL.MainWindow form;
		
		//application thread
		[DllImport("user32.dll")]
		static private extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		static private extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
		[DllImport("user32.dll")]
		static private extern bool IsIconic(IntPtr hWnd);

		private const int SW_RESTORE = 9;
		
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			try
			{				
				if (e.Args.Length > 0 && e.Args[0] == "/u")
				{
					Process.Start(new ProcessStartInfo(Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\msiexec.exe", "/x " + e.Args[1]));
					Application.Current.Shutdown();
					Process.GetCurrentProcess().Kill();
					return;
				}
				else
				{
					/*try
					{
						BscanILL.SETTINGS.Settings.Instance.Licensing.OcrEnabled = true;

						BscanILLData.Models.DbArticle dbArticle = BscanILL.DB.Database.Instance.GetArticle(17);
						BscanILL.Hierarchy.Article article = new BscanILL.Hierarchy.Article(dbArticle);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
					}*/
								
					// get the name of our process
					string proc = Process.GetCurrentProcess().ProcessName;

					// get the list of all processes by that name
					Process[] processes = Process.GetProcessesByName(proc);

					//MessageBox.Show(Process.GetCurrentProcess().ProcessName + "   Count: " + processes.Length); 
					
					// if there is more than one process...
					if (processes.Length > 1)
					{						
						// get our process
						Process p = Process.GetCurrentProcess();
						int n = 0;        // assume the other process is at index 0

						// if this process id is OUR process ID...
						if (processes[0].Id == p.Id)
							n = 1;	// then the other process is at index 1

						// get the window handle
						IntPtr hWnd = processes[n].MainWindowHandle;

						// if iconic, we need to restore the window
						if (IsIconic(hWnd))
							ShowWindowAsync(hWnd, SW_RESTORE);

						// bring it to the foreground
						SetForegroundWindow(hWnd);
						// exit our process

						Process.GetCurrentProcess().Kill();
						return;
					}
					else
					{
						//at MTU I've seen access error to 'C:\\Abbyy logs' folder so this is precaustion to try to set Write and Modify to 'Everyone' group to this folder
						SetFullControl(new DirectoryInfo(@"C:\ABBYY Logs"));

						SetFullControl(new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"DLSG\BscanILL")));
						SetFullControlClickSettings(new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"DLSG\Clicks")));

						BscanILL.SETTINGS.Settings settings = BscanILL.SETTINGS.Settings.Instance;						

						form = new MainWindow();
						form.ShowDialog();
					}
				}
			}
			catch (Exception ex)
			{
				string errorMessage = BscanILL.Misc.Misc.GetErrorMessage(ex);

				if (ex is BscanILL.Misc.IllException)
					MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Warning);
				else
					MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);

				try { Application.Current.Shutdown(); }
				catch { }
				try { Process.GetCurrentProcess().Kill(); }
				catch { }
			}
		}

		private static void SetFullControlClickSettings(DirectoryInfo dir)
		{
			try
			{
				bool dirCreated = false;

				dir.Refresh();
				if ( ! dir.Exists)
                {
					dir.Create();
					dirCreated = true;
					dir.Refresh();
				}					
				
				if (dir.Exists)
				{
					System.Security.AccessControl.DirectorySecurity security = dir.GetAccessControl();
					System.Security.Principal.SecurityIdentifier everyone = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
					//security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(everyone, System.Security.AccessControl.FileSystemRights.FullControl,
					//					
					//security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule( @"\Users", System.Security.AccessControl.FileSystemRights.Read | System.Security.AccessControl.FileSystemRights.ReadAndExecute | System.Security.AccessControl.FileSystemRights.Write | System.Security.AccessControl.FileSystemRights.Modify,
					//																				System.Security.AccessControl.InheritanceFlags.ContainerInherit | System.Security.AccessControl.InheritanceFlags.ObjectInherit,
					//																				System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));

					security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(everyone, System.Security.AccessControl.FileSystemRights.Read | System.Security.AccessControl.FileSystemRights.ReadAndExecute | System.Security.AccessControl.FileSystemRights.Write | System.Security.AccessControl.FileSystemRights.Modify,
																									System.Security.AccessControl.InheritanceFlags.ContainerInherit | System.Security.AccessControl.InheritanceFlags.ObjectInherit,
																									System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));

					dir.SetAccessControl(security);
					if( ! dirCreated )
                    {
						string settingsDir = Path.Combine(dir.FullName, @"Settings");
						//in case '..\DLSG\Clicks' was already there, make sure 'Settings' subfolder and settings files have write and modify permissions
						//in case '..\DLSG\Clicks' was just created, we do not needto look for subfolders and set their priorities..
						SetFullControl(new DirectoryInfo(settingsDir));
						
						SetFullControl(new FileInfo(Path.Combine(settingsDir, @"Click.settings")));
						SetFullControl(new FileInfo(Path.Combine(settingsDir, @"ClickMini.settings")));
					}
				}
			}

			//catch (Exception ex)
			catch (Exception)
			{
				//throw new Exception(string.Format("Can't set full access permissions to the directory '{0}'!", dir.FullName) + " " + ex.Message);
				//MessageBox.Show(ex.Message, string.Format("Can't set full access permissions to the directory '{0}'!", dir.FullName));
			}
		}

		private static void SetFullControl(DirectoryInfo dir)
		{
			try
			{
				dir.Create();

				dir.Refresh();
				if (dir.Exists)
				{
					System.Security.AccessControl.DirectorySecurity security = dir.GetAccessControl();
					System.Security.Principal.SecurityIdentifier everyone = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
					//security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(everyone, System.Security.AccessControl.FileSystemRights.FullControl,
					//					
					//security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule( @"\Users", System.Security.AccessControl.FileSystemRights.Read | System.Security.AccessControl.FileSystemRights.ReadAndExecute | System.Security.AccessControl.FileSystemRights.Write | System.Security.AccessControl.FileSystemRights.Modify,
					//																				System.Security.AccessControl.InheritanceFlags.ContainerInherit | System.Security.AccessControl.InheritanceFlags.ObjectInherit,
					//																				System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));

					security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(everyone, System.Security.AccessControl.FileSystemRights.Read | System.Security.AccessControl.FileSystemRights.ReadAndExecute | System.Security.AccessControl.FileSystemRights.Write | System.Security.AccessControl.FileSystemRights.Modify,
																									System.Security.AccessControl.InheritanceFlags.ContainerInherit | System.Security.AccessControl.InheritanceFlags.ObjectInherit,
																									System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));

					dir.SetAccessControl(security);


				}
			}

			//catch (Exception ex)
			catch (Exception)
			{
				//throw new Exception(string.Format("Can't set full access permissions to the directory '{0}'!", dir.FullName) + " " + ex.Message);
				//MessageBox.Show(ex.Message, string.Format("Can't set full access permissions to the directory '{0}'!", dir.FullName));
			}
		}

		private static void SetFullControl(FileInfo file)
		{
			try
			{
				file.Refresh();
				if (file.Exists)
				{
					System.Security.AccessControl.FileSecurity fileSecurity = file.GetAccessControl();
					System.Security.Principal.SecurityIdentifier everyone = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
					//fileSecurity.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(everyone, System.Security.AccessControl.FileSystemRights.FullControl,					
					fileSecurity.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(everyone, System.Security.AccessControl.FileSystemRights.Read | System.Security.AccessControl.FileSystemRights.ReadAndExecute | System.Security.AccessControl.FileSystemRights.Write | System.Security.AccessControl.FileSystemRights.Modify ,
																										System.Security.AccessControl.InheritanceFlags.None, 
																										System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));
					file.SetAccessControl(fileSecurity);
					file.Refresh();
				}
			}

			//catch (Exception ex)
			catch (Exception)
			{
				//throw new Exception(string.Format("Can't set full access permissions to the file '{0}'!", file.FullName) + " " + ex.Message);
				//MessageBox.Show(ex.Message, string.Format("Can't set full access permissions to the file '{0}'!", file.FullName));
			}
		}

		private void Form_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			try
			{
				if (this.form != null)
				{
					this.form.Close();
					//this.form = null;
				} 
				
				string errorMessage = BscanILL.Misc.Misc.GetErrorMessage(e.Exception);
				MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);

				if (e.Exception is BscanILL.Misc.IllException)
					BscanILL.Misc.Notifications.Instance.Notify(null, BscanILL.Misc.Notifications.Type.Error, errorMessage, null);
				else
					BscanILL.Misc.Notifications.Instance.Notify(null, BscanILL.Misc.Notifications.Type.Warning, errorMessage, e.Exception);
			}
			catch
			{
				string errorMessage = BscanILL.Misc.Misc.GetErrorMessage(e.Exception);
				MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				try { Application.Current.Shutdown(); }	
				catch { }
				try { Process.GetCurrentProcess().Kill(); }
				catch { }
				e.Handled = true;
			}
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
		}

	}
}
