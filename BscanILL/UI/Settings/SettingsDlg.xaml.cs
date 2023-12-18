using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace BscanILL.UI.Settings
{
	/// <summary>
	/// Interaction logic for SettingsDlg.xaml
	/// </summary>
	public partial class SettingsDlg : Window, Scanners.PingDeviceReceiver
	{
		static BscanILL.SETTINGS.Settings settingsClone;
		bool settingsModified = false;
		BscanILL.UI.Frames.Start.FrameStart dlgFrameStart;
		bool licenseFileJustInstalled = false;

		#region constructor
		public SettingsDlg(BscanILL.UI.Frames.Start.FrameStart frameStart, BscanILL.Hierarchy.SessionBatch batch)
		{
			SettingsDlg.settingsClone = BscanILL.SETTINGS.Settings.Instance.Clone();
			dlgFrameStart = frameStart;

			InitializeComponent();

			settingsModified = false;
			licenseFileJustInstalled = false;

			this.panelScanner.CheckLicenseFile_Request += delegate (object sender, EventArgs e)
			{
				CheckLicenseFile_Click();
			};

			this.panelScanner.DownloadLicenseFile_Request += delegate (object sender, EventArgs e)
			{
				DownloadLicenseFile_Click();
			};

			this.panelPrinter.PrintTestSheet += delegate (BscanILL.Export.Printing.PrinterProfile printerProfile)
			{
				PrintTestSheet_Click(printerProfile);
			};

			panelEmail.SendTestEmailRequest += delegate (bool sendDataAttachment)
			{
				SendTestEmail_Click(sendDataAttachment);
			};

			panelEmail.ValidateTestEmailAddressRequest += delegate ()
				{
					ValidateTestEmailAddress_Click();
				};

			panelScanner.S2N_TestConnection += delegate (object sender, EventArgs e)
			{
				S2N_TestConnection_Click();
			};

			panelScanner.S2N_TestTouchscreen += delegate (object sender, EventArgs e)
				  {
					  S2N_TestTouchscreen_Click();
				  };

			panelScanner.Twain_ScanClick += delegate (object sender, EventArgs e)
			{
				TwainTestScan_Click();
			};

			panelScanner.Twain_SettingsClick += delegate (object sender, EventArgs e)
			{
				TwainSettings_Click();
			};

			panelScanner.Rebel_OpenSettingsClick += delegate (object sender, EventArgs e)
			{
				OpenClickSettings();
			};

			panelScanner.ClickMini_OpenSettingsClick += delegate (object sender, EventArgs e)
			{
				OpenClickMiniSettings();
			};

			panelScanner.AddOnScanner_ScanClick += delegate (object sender, EventArgs e)
			{
				AddOnScannerTestScan_Click();
			};

			panelScanner.AddOnScanner_SettingsClick += delegate (object sender, EventArgs e)
			{
				AddOnScannerSettings_Click();
			};

			/*panelCloudStorage.TestStartSmartDock_Request += delegate(object sender, EventArgs e)
			{
				if (TestStartSmartDockRequest != null)
					TestStartSmartDockRequest(sender, e);
				else
					throw new Exception("SettingsDlg, TestStartSmartDock_Click event is not hooked up!");
			};*/

			this.panelCleanUp.Batch = batch;

			if ((batch != null) && (batch.Articles.Count > 1))
			{
				this.panelGeneral.checkMultipleArticles.IsEnabled = false;
			}
			else
			{
				this.panelGeneral.checkMultipleArticles.IsEnabled = true;
			}

			Init();
		}
		#endregion


		public enum Stage
		{
			Scanner
		}


		//PUBLIC PROPERTIES
		#region public properties

		public IntPtr Handle { get { return new System.Windows.Interop.WindowInteropHelper(this).Handle; } }

		public bool SettingsModified { get { return settingsModified; } }
		public bool LicenseFileJustInstalled { get { return licenseFileJustInstalled; } }

		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		internal static BscanILL.SETTINGS.Settings _settings { get { return SettingsDlg.settingsClone; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region ShowDialog()
		public bool? ShowDialog(Stage stage)
		{
			switch (stage)
			{
				case Stage.Scanner: tabControl.SelectedIndex = 1; break;
			}

			return this.ShowDialog();
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region SaveSettings()
		private bool SaveSettings()
		{
			try
			{
				Lock();

				Validate();
				ApplySettings();
				//CheckScannerLicense();

				SaveClickMiniSettings();  //need to update ClickMini.settings file
				BscanILL.SETTINGS.Settings.Instance.Save();
				settingsModified = true;
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL Settings", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region Ok_Click()
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			if (SaveSettings())
				this.DialogResult = true;
		}
		#endregion

		#region Apply_Click()
		private void Apply_Click(object sender, RoutedEventArgs e)
		{
			SaveSettings();
		}
		#endregion

		#region OnSourceInitialized()
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
		}
		#endregion

		#region Lock()
		public void Lock()
		{
			this.IsEnabled = false;
			this.Cursor = Cursors.Wait;
		}
		#endregion

		#region UnLock()
		public void UnLock()
		{
			this.IsEnabled = true;
			this.Cursor = null;
		}
		#endregion

		#region Validate()
		private void Validate()
		{
			CheckScannerLicense();
		}
		#endregion

		#region LoadSettings()
		private void LoadSettings()
		{
			_settings.ApplySettings(BscanILL.SETTINGS.Settings.Instance, false);
		}
		#endregion

		#region ApplySettings()
		private void ApplySettings()
		{
			BscanILL.SETTINGS.Settings.Instance.ApplySettings(_settings, true);
		}
        #endregion

		# region SaveClickMiniSettings()
        private void SaveClickMiniSettings()
		{
			//just for ClickMini as Click scanner saves into Click.settings file automaticaly when closing SettingsWizard (coded in Jirka's dll)
			BscanILL.SETTINGS.Settings.Instance.Scanner.ClickMini.Save();
		}
		#endregion

		#region CheckScannerLicense()
		private void CheckScannerLicense()
		{
			string		sn = GetScannerSn();
			FileInfo	licenseFile = BscanILL.Misc.Licensing.GetLicenseFile(sn);
            
			if (licenseFile.Exists)
			{
                if (BscanILL.Misc.Licensing.CheckLicenseFile(sn))
                {                   
                   licenseFileJustInstalled |= BscanILL.Misc.Licensing.InstallNecessaryComponentsBasedOnLicensing(sn);
                }
                else
                {
                    throw new Exception("The license file associated with scanner '" + sn + "' is invalid!");
                }
			}
			else
				throw new Exception("The license file associated with scanner '" + sn + "' is not installed on this Bscan ILL Site!");
		}
		#endregion

		#region PingDeviceProgressChanged()
		public void PingDeviceProgressChanged(string description)
		{
			throw new NotImplementedException();
		}
		#endregion

		#endregion

	}
}
