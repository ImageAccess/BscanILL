using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Security.AccessControl;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Speech.Synthesis;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Management;
using System.Drawing.Printing;
using System.Security.Cryptography;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using BscanILL.Misc;


namespace BscanILL.UI.Settings
{
	public partial class SettingsDlg : Window
	{
		Scanners.Twain.TwainBase			twainScanner = null;
		Scanners.Twain.AdfScanner			addOnScanner = null;

		delegate SerialNumberRequest				GetTwainSnHnd();
		GetTwainSnHnd								dlgGetTwainSn;

		delegate void LicenseFileSuccessfullHnd( string info );
        delegate void LicenseFileDownloadSuccessfullHnd();
		delegate void LicenseFileErrorHnd(Exception ex, string scannerSn);

		Scanners.OperationSuccessfullHnd	dlgIpScannerConnectionSuccessfull;
		Scanners.OperationErrorHnd			dlgIpScannerConnectionError;

        Scanners.OperationSuccessfullHnd    dlgTouchScreenVersionSuccessfull;
        Scanners.OperationErrorHnd          dlgTouchScreenVersionError;
        
		LicenseFileSuccessfullHnd	dlgScannerLicenseCheckSuccessfull;
		LicenseFileErrorHnd			dlgScannerLicenseCheckError ;
        LicenseFileDownloadSuccessfullHnd dlgScannerLicenseDownloadSuccessfull;
		LicenseFileErrorHnd			dlgScannerLicenseDownloadError;
		

		#region constructor
		public void Init()
		{
			this.Closing += new CancelEventHandler(Form_Closing);

			/*dlgRebelImagesScanned = delegate(Bitmap image1, Bitmap image2)
			{
				try
				{
					string tempDir = _settings.General.TempDir;

					Directory.CreateDirectory(tempDir);
					
					image1.Save(Path.Combine(tempDir, @"Left sample.jpg"), ImageFormat.Jpeg);
					image2.Save(Path.Combine(tempDir, @"Right sample.jpg"), ImageFormat.Jpeg);

					MessageBox.Show(string.Format("Scan was successfull. Open up '{0}' to check images.", tempDir), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Information);
					UnLock();
				}
				catch (Exception ex)
				{
					MessageBox.Show("Scanning was successfull but exception occured while saving images:" + " " + ex.Message, "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				finally
				{
					UnLock();
				}
			};*/

			/*dlgRebelScanError = delegate(Exception ex)
			{
				MessageBox.Show(ex.Message, "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			};*/

			this.dlgIpScannerConnectionSuccessfull = delegate(int operationId)
			{
				MessageBox.Show("Connection to the scanner is OK.", "", MessageBoxButton.OK, MessageBoxImage.Information);
				UnLock();
			};

			this.dlgIpScannerConnectionError = delegate(int operationId, Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			};

            this.dlgTouchScreenVersionSuccessfull = delegate(int touchscreenVersion)
            {
                if( touchscreenVersion > 0 )
                {
                    float temp = (float) touchscreenVersion / 10;
                    MessageBox.Show("Touchscreen version: " + temp.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Touchscreen version: N/A", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }                
                UnLock();
            };

            this.dlgTouchScreenVersionError = delegate(int operationId, Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                UnLock();
            };
            
			this.dlgScannerLicenseCheckSuccessfull = delegate( string licenseInfo )
			{
                MessageBox.Show("BscanILL licensing is set well." + "\r\n" + "\r\n" + licenseInfo, "", MessageBoxButton.OK, MessageBoxImage.Information);
				UnLock();
			};

			this.dlgScannerLicenseCheckError = delegate(Exception ex, string scannerSn)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);

				UnLock();
			};

			this.dlgScannerLicenseDownloadSuccessfull = delegate()
			{
				MessageBox.Show("BscanILL licensing was downloaded successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
				UnLock();
			};

			this.dlgScannerLicenseDownloadError = delegate(Exception ex, string scannerSn)
			{
				//if (ex is IllException && scannerSn != null)
				{
                    //hardcode to use Keith's http delivery method for license requests...
                    //BscanILL.UI.Settings.Dialogs.LicenseRequestDlg dlg = new BscanILL.UI.Settings.Dialogs.LicenseRequestDlg(ex.Message, scannerSn, _settings.Export.Email.EmailDeliveryType);
                    BscanILL.UI.Settings.Dialogs.LicenseRequestDlg dlg = new BscanILL.UI.Settings.Dialogs.LicenseRequestDlg(ex.Message, scannerSn, SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.HTTP);

					if (dlg.ShowDialog() == true)
					{
					}
					else
					{
					}
				}
				//else
				//	MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);

				UnLock();
			};

			this.dlgGetTwainSn = delegate()
			{
				try
				{
					DestroyTwainScanner();

					string sn;
					Scanners.MODELS.Model model = new Scanners.MODELS.Model(_settings.Scanner.General.ScannerType);

					if (model.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
					{
						sn = Scanners.Twain.AdfScanner.GetSerialNumber(this, model);
					}
					else
					{
						sn = Scanners.Twain.TwainScanner.GetSerialNumber(this, model);
					}

					return new SerialNumberRequest(sn, null);
				}
				catch (Exception ex)
				{
					return new SerialNumberRequest(null, ex);
				}
			};
		}
		#endregion


		#region class SerialNumberRequest
		public class SerialNumberRequest
		{
			public readonly string SerialNumber;
			public readonly Exception Exception;

			public SerialNumberRequest(string sn, Exception ex)
			{
				this.SerialNumber = sn;
				this.Exception = ex;
			}
		}
		#endregion
	
		
		//PUBLIC METHODS
		#region public methods

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region SendTestEmail_Click()
		private void SendTestEmail_Click(bool sendDataAttachment)
		{
			try
			{
           string result = "";
				
				Lock();

				     if (_settings.Export.Email.EmailDeliveryType != SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP)
                {
                    result = BscanILL.Export.Email.Email.SendTestEmailHttp(_settings, sendDataAttachment);
                    if( String.Compare(result , "Message Sent", false) != 0)
                    {
                        if (_settings.Export.Email.EmailDeliveryType == SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both)
                      { 
                        //try also SMTP validation
                          BscanILL.Export.Email.Email.SendTestEmail(_settings, sendDataAttachment);
                      }
                      else
                      {
                          throw new Exception("Test e-mail failed.");
                      }
                    }
                }
                else
                {
                    BscanILL.Export.Email.Email.SendTestEmail(_settings, sendDataAttachment);
                }

				MessageBox.Show("Test e-mail sent successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

        #region ValidateTestEmailAddress_Click()
        private void ValidateTestEmailAddress_Click()
        {
            try
            {
                Lock();

                if (_settings.Export.Email.From.Length > 0)
                {
                  if (_settings.Export.Email.EmailDeliveryType != SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP)        //merging Validation and delivery type control in settings GUI
                  {
                    if (!BscanILL.Misc.EmailValidatorHttp.ValidateEmailHttp(_settings.Export.Email.From))        //Http validation -> where we send the email address to our web server where the validation gets perform to overcome firewall issues with SMTP blocking at customer's network
                    {
                      if (_settings.Export.Email.EmailDeliveryType == SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both)
                      { 
                        //try also SMTP validation
                        if (!BscanILL.Misc.EmailValidator.ValidateEmail(_settings.Export.Email.From))
                        {
                            //validation failed
                            throw new Exception("Validation of Default Email Address Failed with Both HTTP and SMTP Methods!");
                        }
                      }
                      else
                      {
                        //validation failed
                        throw new Exception("Validation of Default Email Address Failed with HTTP Method!");
                      }
                    }
                  }
                  else
                  if (!BscanILL.Misc.EmailValidator.ValidateEmail(_settings.Export.Email.From))       //SMTP validation -> might be failing at customers because of the firewall
                  {
                    //validation failed
                    throw new Exception("Validation of Default Email Address Failed with SMTP Method!");
                  }

                  MessageBox.Show("Default Email Address Passed Validation Successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Default Email Address Field is Empty!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.IsEnabled = true;
                this.Cursor = null;
            }
        }
        #endregion
        
		#region S2N_TestConnection_Click()
		private void S2N_TestConnection_Click()
		{
			try
			{
				Lock();

				System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(TestIpScannerConnection));
				thread.Name = "ThreadSettingsDlg_TestIpScannerConncetion";
				thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
				thread.SetApartmentState(System.Threading.ApartmentState.STA);
				thread.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}
		}
		#endregion

        #region S2N_TestTouchscreen_Click()
        private void S2N_TestTouchscreen_Click()
		{
			try
			{
				Lock();

				System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(TestTouchScreenVersion));
				thread.Name = "ThreadSettingsDlg_TestTouchscreenVersion";
				thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
				thread.SetApartmentState(System.Threading.ApartmentState.STA);
				thread.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}
		}
		#endregion
		
		#region TestIpScannerConnection()
		private void TestIpScannerConnection()
		{
			Scanners.S2N.ScannerS2N scanner = null;
			
			try
			{
				Scanners.Scanner.PingDevice(null, settingsClone.Scanner.S2NScanner.Ip, false);
				/*scanner = Scanners.S2N.ScannerS2N.GetInstance();
				scanner.Dispose();*/

				this.Dispatcher.Invoke(this.dlgIpScannerConnectionSuccessfull, -1);
			}
			catch (Exception ex)
			{
				if (scanner != null)
					scanner.Dispose();
				
				this.Dispatcher.Invoke(this.dlgIpScannerConnectionError, -1, ex);
			}
		}
		#endregion

        #region TestTouchScreenVersion()
        private void TestTouchScreenVersion()
		{						
			try
			{                
                int touchVersion = dlgFrameStart.FrameStartScanner.TouchApp_Request();
                                             
                this.Dispatcher.Invoke(this.dlgTouchScreenVersionSuccessfull, touchVersion);
			}
			catch (Exception ex)
			{			
                this.Dispatcher.Invoke(this.dlgTouchScreenVersionError, -1, ex);  
			}
		}
		#endregion 
		
		#region CheckLicenseFile_Click()
		private void CheckLicenseFile_Click()
		{
			try
			{
				Lock();

				System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(CheckLicenseFileThread));
				thread.Name = "ThreadSettingsDlg_CheckLicenseFile";
				thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
				thread.SetApartmentState(System.Threading.ApartmentState.STA);
				thread.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}
		}
		#endregion

		#region CheckLicenseFileThread()
		private void CheckLicenseFileThread()
		{
			string sn = "N/A";
			
			try
			{
                //debug: just to encrypt IRIS bin/resources files for installation purposes (go to 'settings'->'scanner' tab->hit 'Check License File' button')
                //BscanILL.Misc.Licensing.EncryptOCRBin();

				sn = GetScannerSn();

				FileInfo licenseFile = Licensing.GetLicenseFile(sn);
                
				if (licenseFile.Exists == false)
					throw new IllException("The license file associated with scanner '" + sn + "' is not installed on this Bscan ILL Site!");

                string licenseInfo = BscanILL.Misc.Licensing.ExtractLicenseFileInfo(sn);

                if (BscanILL.Misc.Licensing.CheckLicenseFile(sn))
                {                    
                    licenseFileJustInstalled |= BscanILL.Misc.Licensing.InstallNecessaryComponentsBasedOnLicensing(sn);
                }
                else
                {
                    throw new IllException("The license file associated with scanner '" + sn + "' is not valid!");
                }

                this.Dispatcher.Invoke(this.dlgScannerLicenseCheckSuccessfull ,licenseInfo);
			}
			catch (IllException ex)
			{
				this.Dispatcher.Invoke(this.dlgScannerLicenseCheckError, ex, sn);
			}
			catch (Exception ex)
			{
				if (sn != null)
					this.Dispatcher.Invoke(this.dlgScannerLicenseCheckError, new IllException("The license file associated with scanner '" + sn + "' is invalid! Please contact DLSG at Image Access."), sn);
				else
					this.Dispatcher.Invoke(this.dlgScannerLicenseCheckError, ex);
			}
		}
		#endregion

		#region DownloadLicenseFile_Click()
		private void DownloadLicenseFile_Click()
		{
			try
			{
				Lock();

				System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(DownloadLicenseFileTU));
				thread.Name = "ThreadSettingsDlg_DownloadLicenseFile";
				thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
				thread.SetApartmentState(System.Threading.ApartmentState.STA);
				thread.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}
		}
		#endregion

		#region DownloadLicenseFileTU()
		private void DownloadLicenseFileTU()
		{
			string sn = "N/A";

			try
			{
				sn = GetScannerSn();
				BscanILL.Misc.Licensing.DownloadLicenseFile(sn);

				this.Dispatcher.Invoke(this.dlgScannerLicenseDownloadSuccessfull);
			}
			catch (Exception ex)
			{
				if (sn != null && ex.Message.Contains("(404) Not Found"))
					this.Dispatcher.Invoke(this.dlgScannerLicenseDownloadError, new IllException("There is no license file associated with scanner '" + sn + "'! Please contact DLSG at Image Access."), sn);
				else
					this.Dispatcher.Invoke(this.dlgScannerLicenseDownloadError, ex, sn);
			}
		}
		#endregion

		//database 
		#region DatabaseConnectionTest_Click()
		private void DatabaseConnectionTest_Click(object sender, EventArgs e)
		{
			/*try
			{
				this.Enabled = false;
				Lock();

					BscanILL.DB.Database.Instance..SQLiteDatabase.TestConnection();
				
				MessageBox.Show("Database connection is OK.", "", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.Cursor = null;
				this.Enabled = true;
			}*/
		}
		#endregion

		#region PrintTestSheet_Click()
		private void PrintTestSheet_Click(BscanILL.Export.Printing.PrinterProfile printerProfile)
		{
			try
			{
				BscanILL.Export.Printing.PrintOption printOption = new BscanILL.Export.Printing.PrintOption(printerProfile);
				printOption.PrintTestSheet();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		//rebel
		#region OpenClickSettings()
		private void OpenClickSettings()
		{
			try
			{
				Lock();

				using (Scanners.Click.ClickWrapper click = new Scanners.Click.ClickWrapper())
				{
					click.OpenClickSettings();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion


		//click mini
		#region OpenClickMiniSettings()
		private void OpenClickMiniSettings()
		{
			try
			{
				Lock();

				using (Scanners.Click.ClickMiniWrapper click = new Scanners.Click.ClickMiniWrapper())
				{
					click.OpenClickSettings();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion


		//twain
		#region CreateTwainScanner()
		private void CreateTwainScanner()
		{
			if (this.twainScanner == null)
			{
				Scanners.MODELS.Model model = new Scanners.MODELS.Model(_settings.Scanner.General.ScannerType);

				if (model.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
					this.twainScanner = Scanners.Twain.AdfScanner.GetInstance(this, model);
				else
					this.twainScanner = Scanners.Twain.TwainScanner.GetInstance(this, model);

				this.twainScanner.ImageScanned += new Scanners.Twain.TwainScanner.ImageScannedHnd(Twain_ImageScanned);
				this.twainScanner.ScanError += new Scanners.Twain.TwainScanner.ScanErrorHnd(Twain_ScanError);
				this.twainScanner.ProgressChanged += new Scanners.ProgressChangedHnd(Twain_ProgressChanged);
			}

			if (_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.iVinaFB6080E || _settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.iVinaFB6280E)
			{
				this.twainScanner.Brightness = _settings.Scanner.TwainScanner.BrightnessDelta;
				this.twainScanner.Contrast = _settings.Scanner.TwainScanner.ContrastDelta;
			}
		}

		void twainScanner_ImageScanned(int operationId, TwainApp.TwainImage twainImage, Scanners.Twain.BookedgePage bookedgePage, bool moreImagesToTransfer)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region DestroyTwainScanner()
		private void DestroyTwainScanner()
		{
			if (this.twainScanner != null)
			{
				this.twainScanner.ImageScanned -= new Scanners.Twain.TwainScanner.ImageScannedHnd(Twain_ImageScanned);
				this.twainScanner.ScanError -= new Scanners.Twain.TwainScanner.ScanErrorHnd(Twain_ScanError);
				this.twainScanner.ProgressChanged -= new Scanners.ProgressChangedHnd(Twain_ProgressChanged);

				if (this.twainScanner is Scanners.Twain.AdfScanner)
					((Scanners.Twain.AdfScanner)this.twainScanner).Dispose(this);
				else
					((Scanners.Twain.TwainScanner)this.twainScanner).Dispose(this);

				this.twainScanner = null;
			}
		}
		#endregion

		#region TwainTestScan_Click()
		private void TwainTestScan_Click()
		{
			try
			{
				Lock();

				DestroyTwainScanner();
				CreateTwainScanner();

				if (this.twainScanner is Scanners.Twain.AdfScanner)
				{
					Scanners.Twain.AdfSettings scanSettings = new Scanners.Twain.AdfSettings();

					scanSettings.Brightness.Value = 0;
					scanSettings.ColorMode.Value = Scanners.Twain.ColorMode.Color;
					scanSettings.Contrast.Value = 0;
					scanSettings.DocSize.Value = Scanners.Twain.DocSize.Auto;
					scanSettings.Dpi.Value = 200;
					scanSettings.FileFormat.Value = Scanners.Twain.FileFormat.Tiff;
					scanSettings.Duplex.Value = false;
                    scanSettings.TransferCount.Value = 1;

					((Scanners.Twain.AdfScanner)this.twainScanner).Scan(-1, scanSettings);
				}
				else
				{
					Scanners.Twain.BookedgeSettings scanSettings = new Scanners.Twain.BookedgeSettings();

					scanSettings.Brightness.Value = 0;
					scanSettings.ColorMode.Value = Scanners.Twain.ColorMode.Color;
					scanSettings.Contrast.Value = 0;
					scanSettings.DocSize.Value = Scanners.Twain.DocSize.Auto;
					scanSettings.Dpi.Value = 200;
					scanSettings.FileFormat.Value = Scanners.Twain.FileFormat.Tiff;
					scanSettings.ScanPage.Value = Scanners.Twain.BookedgePage.FlatMode;

					((Scanners.Twain.TwainScanner)this.twainScanner).Scan(-1, scanSettings);
				}
			}
			catch (Exception ex)
			{
				DestroyTwainScanner();			
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}
		}
		#endregion

		#region AddOnScannerTestScan_Click()
		private void AddOnScannerTestScan_Click()
		{
			try
			{
				Lock();

				if (this.addOnScanner == null)
				{
					this.addOnScanner = Scanners.Twain.AdfScanner.GetInstance(this, new Scanners.MODELS.Model(_settings.Scanner.AdfAddOnScanner.ScannerType));

					this.addOnScanner.ImageScanned += new Scanners.Twain.TwainScanner.ImageScannedHnd(Twain_ImageScanned);
					this.addOnScanner.ScanError += new Scanners.Twain.TwainScanner.ScanErrorHnd(Twain_ScanError);
					this.addOnScanner.ProgressChanged += new Scanners.ProgressChangedHnd(Twain_ProgressChanged);
				}

				Scanners.Twain.AdfSettings scanSettings = new Scanners.Twain.AdfSettings();

				scanSettings.Brightness.Value = 0;
				scanSettings.ColorMode.Value = Scanners.Twain.ColorMode.Color;
				scanSettings.Contrast.Value = 0;
				scanSettings.DocSize.Value = Scanners.Twain.DocSize.Auto;
				scanSettings.Dpi.Value = 200;
				scanSettings.FileFormat.Value = Scanners.Twain.FileFormat.Tiff;
				scanSettings.Duplex.Value = false;
                
                scanSettings.TransferCount.Value = 1;  //just one scan when testing ADF                

				this.addOnScanner.Scan(-1, scanSettings);
			}
			catch (Exception ex)
			{
				if (this.addOnScanner != null)
				{
					this.addOnScanner.Dispose(this);
					this.addOnScanner = null;
				}

				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}
		}
		#endregion

		#region Twain_ImageScanned()
		void Twain_ImageScanned(int operationId, TwainApp.TwainImage twainImage, bool moreImagesToTransfer)
		{
			this.Dispatcher.BeginInvoke((Action)delegate { Twain_ImageScannedTU(operationId, twainImage, moreImagesToTransfer); });
			//this.dlg.Dispatcher.Invoke(this.dlgTwainImageScanned, operationId, bitmap, scanPage, moreImagesToTransfer);
		}
		#endregion

		#region Twain_ScanError()
		void Twain_ScanError(int operationId, Exception ex)
		{
			this.Dispatcher.Invoke((Action)delegate { Twain_ScanErrorTU(operationId, ex); });
			//this.dlg.Dispatcher.Invoke(this.dlgTwainScanError, operationId, ex);
		}
		#endregion

		#region Twain_ImageScannedTU()
		void Twain_ImageScannedTU(int operationId, TwainApp.TwainImage twainImage, bool moreImagesToTransfer)
		{
			try
			{
				if (MessageBox.Show(string.Format("Image scanned successfully. Width: {0}, Height: {1}, Dpi: {2}. Save Image to Disk?", twainImage.Bitmap.Width, twainImage.Bitmap.Height,
					(int)twainImage.Bitmap.HorizontalResolution), "BscanILL Setup", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					Microsoft.Win32.SaveFileDialog dlg = new SaveFileDialog();
					//System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();

					dlg.Filter = "JPEG files (*.jpg)|*.jpg";
					dlg.FilterIndex = 1;
					dlg.RestoreDirectory = true;
					dlg.AddExtension = true;
					dlg.DefaultExt = "jpg";
					dlg.FileName = @"c:\temp\Bscan ILL Test Image.jpg";

					if (dlg.ShowDialog().Value == true)
					{
						FileInfo file = new FileInfo(dlg.FileName);

						file.Directory.Create();
						twainImage.Bitmap.Save(dlg.FileName, ImageFormat.Jpeg);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				twainImage.Dispose();

				if (moreImagesToTransfer == false)
				{
                    DestroyTwainScanner();
					UnLock();
				}
			}
		}
		#endregion

		#region Twain_ScanErrorTU()
		void Twain_ScanErrorTU(int operationId, Exception ex)
		{
			try
			{
				MessageBox.Show("Error while scanning:" + Environment.NewLine + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception exception)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(exception), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
                DestroyTwainScanner();
				UnLock();
			}
		}
		#endregion

		#region Twain_ProgressChanged()
		void Twain_ProgressChanged(string description, float progress)
		{
			//throw new NotImplementedException();
		}
		#endregion

		#region TwainSettings_Click()
		private void TwainSettings_Click()
		{
			try
			{
				Lock();

                DestroyTwainScanner();
				CreateTwainScanner();

				this.twainScanner.OpenSetupWindow();
			}
			catch (Exception ex)
			{
				DestroyTwainScanner();
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region AddOnScannerSettings_Click()
		private void AddOnScannerSettings_Click()
		{
			try
			{
				Lock();

				if (this.addOnScanner == null)
				{
					this.addOnScanner = Scanners.Twain.AdfScanner.GetInstance(this, new Scanners.MODELS.Model(_settings.Scanner.AdfAddOnScanner.ScannerType));

					this.addOnScanner.ImageScanned += new Scanners.Twain.TwainScanner.ImageScannedHnd(Twain_ImageScanned);
					this.addOnScanner.ScanError += new Scanners.Twain.TwainScanner.ScanErrorHnd(Twain_ScanError);
					this.addOnScanner.ProgressChanged += new Scanners.ProgressChangedHnd(Twain_ProgressChanged);
				}

				this.addOnScanner.OpenSetupWindow();
			}
			catch (Exception ex)
			{
				if (this.addOnScanner != null)
				{
					this.addOnScanner.Dispose(this);
					this.addOnScanner = null;
				}

				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL Setup", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region Form_Closing()
		private void Form_Closing(object sender, CancelEventArgs e)
		{
			DestroyTwainScanner();

			if (this.addOnScanner != null)
			{
				this.addOnScanner.ImageScanned -= new Scanners.Twain.TwainScanner.ImageScannedHnd(Twain_ImageScanned);
				this.addOnScanner.ScanError -= new Scanners.Twain.TwainScanner.ScanErrorHnd(Twain_ScanError);
				this.addOnScanner.ProgressChanged -= new Scanners.ProgressChangedHnd(Twain_ProgressChanged);

				this.addOnScanner.Dispose(this);
				this.addOnScanner = null;
			}
		}
		#endregion

		#region GetScannerSn()
		private string GetScannerSn()
		{
			string sn = null;

			switch (_settings.Scanner.General.ScannerType)
			{
				case Scanners.SettingsScannerType.S2N:
					{
						Scanners.Scanner.PingDevice(this, _settings.Scanner.S2NScanner.Ip, true);
						Scanners.DeviceInfo s2nInfo = Scanners.S2N.ScannerS2N.GetDeviceInfo(_settings.Scanner.S2NScanner.Ip);
						sn = s2nInfo.SerialNumber;
					} break;

				case Scanners.SettingsScannerType.iVinaFB6080E:
				case Scanners.SettingsScannerType.iVinaFB6280E:
					{
						sn = Scanners.Twain.TwainScanner.GetSerialNumber(this, new Scanners.MODELS.Model(_settings.Scanner.General.ScannerType));
					} break;
				case Scanners.SettingsScannerType.KodakI1405:
				case Scanners.SettingsScannerType.KodakI1120:
				case Scanners.SettingsScannerType.KodakI1150:
				case Scanners.SettingsScannerType.KodakI1150New:
				case Scanners.SettingsScannerType.KodakE1035:
				case Scanners.SettingsScannerType.KodakE1040:
					{
						sn = Scanners.Twain.AdfScanner.GetSerialNumber(this, new Scanners.MODELS.Model(_settings.Scanner.General.ScannerType));
					}; break;
				case Scanners.SettingsScannerType.Click:
					{
						this.Dispatcher.Invoke((Action)delegate()
						{
							using (Scanners.Click.ClickWrapper clickWrapper = new Scanners.Click.ClickWrapper())
							{
								sn = clickWrapper.DeviceInfo.SerialNumber;
							}
						});
					}; break;
				case Scanners.SettingsScannerType.ClickMini:
					{
						this.Dispatcher.Invoke((Action)delegate()
						{
							using (Scanners.Click.ClickMiniWrapper clickWrapper = new Scanners.Click.ClickMiniWrapper())
							{
								sn = clickWrapper.DeviceInfo.SerialNumber;
							}
						});
					}; break;
				default:
					{
						throw new Exception("Unsupported scanner type!");
					}
			}

			return sn;
		}
		#endregion

		#endregion
	
	}
}
