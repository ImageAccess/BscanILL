#define TransNumber_LONG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using System.IO;
using System.Drawing.Printing;
using System.Windows.Input;
using System.Windows;
using System.Runtime.InteropServices;
using BscanILL.Misc;
using System.Collections.ObjectModel;
using BscanILL.FileSystem;

namespace BscanILL.UI.Frames.Start
{
	public class FrameStart : FrameBase
	{
		#region variables

		private BscanILL.UI.Frames.Start.FrameStartUi		frameStartUi;
		IFrameStartScanner									frameStartScanner = null;

		public event KeyEventHandler						KeyDown;

		int lastOperationId = 0;
		//Scanners.Twain.AdfSettings secondaryScannerPullslipSettings = new Scanners.Twain.AdfSettings();

		#endregion


		#region constructor
		public FrameStart(BscanILL.MainWindow mainWindow)
			: base(mainWindow)
		{
			this.frameStartUi = this.MainWindow.FrameStartUi;
			this.frameStartUi.ScanClick += new VoidHnd(Scan_Click);
			this.frameStartUi.KicImportClick += new VoidHnd(KicImport_Click);
			this.frameStartUi.DiskImportClick += new VoidHnd(DiskImport_Click);
			this.frameStartUi.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(Preview_KeyDown);
/*
			secondaryScannerPullslipSettings.Brightness.Value = 0;
			secondaryScannerPullslipSettings.ColorMode.Value = Scanners.Twain.ColorMode.Bitonal;
			secondaryScannerPullslipSettings.Contrast.Value = 0;
			secondaryScannerPullslipSettings.DocSize.Value = Scanners.Twain.DocSize.Auto;
			secondaryScannerPullslipSettings.Dpi.Value = 300;
			secondaryScannerPullslipSettings.FileFormat.Value = Scanners.Twain.FileFormat.Tiff;
			secondaryScannerPullslipSettings.Duplex.Value = false;
			secondaryScannerPullslipSettings.TransferCount.Value = 1;
*/
            //brightness is loaded from BscanILL.temporary_pullslips.settings file
            //  _scanSettingsPullSlips.Adf.Brightness.Value = 0;
            //  _scanSettingsPullSlips.Adf.Contrast.Value = 0;            
            secondaryScannerPullslipSettings.Adf.ColorMode.Value = Scanners.Twain.ColorMode.Bitonal;
            secondaryScannerPullslipSettings.Adf.DocSize.Value = Scanners.Twain.DocSize.Auto;
            secondaryScannerPullslipSettings.Adf.Dpi.Value = 300;
            secondaryScannerPullslipSettings.Adf.FileFormat.Value = Scanners.Twain.FileFormat.Tiff;
            secondaryScannerPullslipSettings.Adf.Duplex.Value = false;
            secondaryScannerPullslipSettings.Adf.TransferCount.Value = 1;

			this.MainWindow.ArticleChanged += delegate() 
			{ 
				if(this.IsActivated)
					this.frameStartUi.ArticleChanged(this.Article); 
			};

			BscanILL.Scan.ScannersManager.Instance.PrimaryScannerChanged += delegate() { this.frameStartUi.Dispatcher.BeginInvoke((Action)delegate() { PrimaryScanner_Changed(); }); };
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public System.Windows.Controls.UserControl	UserControl { get { return (System.Windows.Controls.UserControl)this.frameStartUi; } }
		public BscanILL.Hierarchy.Article			Article		{ get { return this.MainWindow.Article; } }
        public BscanILL.Hierarchy.SessionBatch      Batch { get { return this.MainWindow.Batch; } }
    public IFrameStartScanner FrameStartScanner  { get { return frameStartScanner; } }                 

		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		BscanILL.Scan.ScannersManager		sm { get { return BscanILL.Scan.ScannersManager.Instance; } }
		BscanILL.Misc.Notifications			notifications { get { return BscanILL.Misc.Notifications.Instance; } }
        BscanILL.SETTINGS.ScanSettingsPullSlips secondaryScannerPullslipSettings { get { return BscanILL.SETTINGS.ScanSettingsPullSlips.Instance; } }

		protected bool PrimaryScannerSelected
		{
			get { return frameStartUi.PrimaryScannerSelected; }
			set { frameStartUi.PrimaryScannerSelected = value; }
		}

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public override void Dispose()
		{
			Deactivate();
			base.Dispose();
		}
		#endregion

		#region Activate()
		public void Activate()
		{
			this.frameStartUi.Open(this.Article);

            ActivateScanner();

			this.IsActivated = true;
		}
		#endregion

        public void ActivateScanner()
        {
            if (this.frameStartScanner != null)
                this.frameStartScanner.Activate();

            if (sm.SecondaryScanner != null)
            {
                ActivateSecondaryScanner();
            }
        }

        public void ActivateSecondaryScanner()
        {
            if (sm.SecondaryScanner != null)
            {
                if (sm.SecondaryScanner is Scanners.Twain.AdfScanner)
                {
                    Scanners.Twain.AdfScanner adf = (Scanners.Twain.AdfScanner)sm.SecondaryScanner;

                    adf.ImageScanned += new Scanners.Twain.TwainBase.ImageScannedHnd(Adf_ImageScanned);
                    adf.ProgressChanged += new Scanners.ProgressChangedHnd(Adf_ProgressChanged);
                    adf.ScanError += new Scanners.Twain.TwainBase.ScanErrorHnd(Adf_ScanError);
                }
            }
        }

		#region ReRegisterScanner()
		public void ReRegisterScanner()
        {
			if (this.frameStartScanner != null)
				this.frameStartScanner.ReRegisterScanner();
		}
		#endregion

		#region UnRegisterScanner()
		public void UnRegisterScanner()
		{
			if (this.frameStartScanner != null)
				this.frameStartScanner.UnRegisterScanner();
		}
		#endregion

		#region Deactivate()
		public void Deactivate()
		{
			Reset();
			this.frameStartUi.Visibility = Visibility.Hidden;

            DeactivateScanner();

			this.IsActivated = false;
		}
		#endregion

        public void DeactivateScanner()
        {
            if (this.frameStartScanner != null)
                this.frameStartScanner.Deactivate();

            if (sm.SecondaryScanner != null)
            {
                if (sm.SecondaryScanner is Scanners.Twain.AdfScanner)
                {
                    Scanners.Twain.AdfScanner adf = (Scanners.Twain.AdfScanner)sm.SecondaryScanner;

                    adf.ImageScanned -= new Scanners.Twain.TwainBase.ImageScannedHnd(Adf_ImageScanned);
                    adf.ProgressChanged -= new Scanners.ProgressChangedHnd(Adf_ProgressChanged);
                    adf.ScanError -= new Scanners.Twain.TwainBase.ScanErrorHnd(Adf_ScanError);
                }
            }
        }

		#region Reset()
		public void Reset()
		{
			try
			{
				this.frameStartUi.Reset();

				ReleaseMemory();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
			finally
			{
			}
		}
		#endregion

		#region AddImage()
		public void AddImage(string file)
		{
			BscanILL.Hierarchy.Article article = CreateArticle(file);

			if (article != null)
			{
				this.MainWindow.ArticleCreated(article);
			}
			else
			{
				Reset();
			}
		}

		public void AddImage(Bitmap bitmap)
		{
			FileInfo file = new FileInfo(Path.Combine(_settings.General.TempDir, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".tif"));
			file.Directory.Create();
			file.Refresh();
			BscanILL.Misc.Io.SaveTiffG4(file.FullName, bitmap);
			//bitmap.Save(file.FullName, ImageFormat.Tiff);
			//bitmap.Dispose();
			AddImage(file.FullName);		
		}
		#endregion

		#region ReleaseMemory()
		private void ReleaseMemory()
		{
			BscanILL.Misc.MemoryManagement.ReleaseUnusedMemory();
		}
		#endregion

		#region Scanner_ImageScanned()
		public void Scanner_ImageScanned(Bitmap bitmap)
		{
			this.MainWindow.Dispatcher.BeginInvoke((Action)delegate() { Scanner_ImageScannedTU(bitmap); });
		}

		public void Scanner_ImageScanned(TwainApp.TwainImage twainImage)
		{
			this.MainWindow.Dispatcher.BeginInvoke((Action)delegate() { Scanner_ImageScannedTU(twainImage); });
		}

		public void Scanner_ImageScanned(string file)
		{
			this.MainWindow.Dispatcher.BeginInvoke((Action)delegate() { Scanner_ImageScannedTU(file); });
		}
		#endregion

		#region Scanner_ScanError()
		public void Scanner_ScanError(Exception ex)
		{
			this.MainWindow.Dispatcher.Invoke((Action)delegate() { Scanner_OperationErrorTU(ex); });
		}
		#endregion

		#region Scanner_ProgressChanged()
		public void Scanner_ProgressChanged(string description, float progress)
		{
			this.MainWindow.Dispatcher.BeginInvoke((Action)delegate() { Scanner_ProgressChangedTU(description, progress); });
		}
		#endregion

		#region DiskImport_Click()
		public void DiskImport_Click()
		{
			try
			{
				//LockWithProgressBar(true, "Importing images...");
				Lock();
				BscanILL.UI.Dialogs.ImportFromDiskDlg dlg = new Dialogs.ImportFromDiskDlg();
				ObservableCollection<FileInfo> list = dlg.Open();

				if (list != null && list.Count > 0)
				{
					Thread thread = new Thread(new ParameterizedThreadStart(ImportArticleThread));
					thread.SetApartmentState(ApartmentState.STA);
					thread.Name = "FrameStart, DiskImport_Click()";
					thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
					thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
					thread.Start(list);
				}
				else
					UnLock();
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}
		}
		#endregion

		#region KicImport_Click()
		public void KicImport_Click()
		{
			/*try
			{
				//LockWithProgressBar(true, "Importing images...");
				Lock();
				BscanILL.UI.Dialogs.ImportFromDiskDlg dlg = new Dialogs.ImportFromDiskDlg();
				ObservableCollection<FileInfo> list = dlg.Open();

				if (list != null && list.Count > 0)
				{
					Thread thread = new Thread(new ParameterizedThreadStart(ImportArticleThread));
					thread.SetApartmentState(ApartmentState.STA);
					thread.Name = "FrameStart, DiskImport_Click()";
					thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
					thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
					thread.Start(list);
				}
				else
					UnLock();
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}*/
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Scan()
		internal void Scan()
		{
			if (this.frameStartScanner != null && this.IsLocked == false)
			{
				try
				{
					Lock();

					LockWithProgressBar(false, "Scanning...");

					if (this.PrimaryScannerSelected) 
						this.frameStartScanner.ScanPullslip();
					else
						((Scanners.Twain.AdfScanner)sm.SecondaryScanner).Scan(++this.lastOperationId, secondaryScannerPullslipSettings.Adf);

					this.MainWindow.Activate();
				}
				catch (IllException ex)
				{
					ShowWarningMessage(ex.Message);
					UnLock();
				}
				catch (Exception ex)
				{
					ShowErrorMessage("The scanning process was interrupted! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
					Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
					UnLock();
				}
			}
		}
		#endregion

		#region ShowDefaultImage()
		/*void ShowDefaultImage()
		{
			if (frameStartUi != null)
				frameStartUi.ShowDefaultImage();
		}*/
		#endregion

		#region ShowIllImage()
		/*void ShowIllImage(BscanILL.Hierarchy.IllImage illImage)
		{
			if (frameStartUi != null)
				this.frameStartUi.ShowImage(illImage);
		}*/
		#endregion

		#region Scan_Click()
		void Scan_Click()
		{
			Scan();
		}
		#endregion

		#region Done_Click()
		private void Done_Click(object sender, System.EventArgs e)
		{
			/*try
			{
				Lock();

				if (this.session == null || this.session.IllImages.Count == 0 || BscanILL.UI.Dialogs.AlertDlg.Show(this.MainWindow, BscanILL.Languages.BscanILLStrings.Frames_AreYouSureToEndSessionImagesDeleted_STR, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Question) == true)
					Reset();
			}
			finally
			{
				UnLock();
			}*/
		}
		#endregion

		#region Operation_Successfull()
		void Operation_Successfull()
		{
			UnLock();
		}
		#endregion

		#region Operation_Error()
		void Operation_Error(IllException ex)
		{
			ShowErrorMessage(ex.Message);
			UnLock();
		}
		#endregion

		#region UserInteracted()
		void UserInteracted()
		{
		}
		#endregion

		#region Preview_KeyDown()
		void Preview_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			bool shift = ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Shift) > 0);

			//if (this.Visible && this.Enabled && e.Key == System.Windows.Input.Key.Space)
			if (this.IsActivated && this.IsEnabled && shift && e.Key == System.Windows.Input.Key.D7)
					Scan();
				else
			{
				if (KeyDown != null)
					KeyDown(this, e);
				//this.OnKeyDown(args);
			}
		}
		#endregion

		#region PrimaryScanner_Changed()
		void PrimaryScanner_Changed()
		{
			if (this.frameStartScanner != null)
				this.frameStartScanner.Dispose();

			this.frameStartScanner = null;

			Scanners.IScanner scanner = BscanILL.Scan.ScannersManager.Instance.PrimaryScanner;

			if (scanner is Scanners.Twain.TwainScanner)
				this.frameStartScanner = new FrameStartTwain(this, (Scanners.Twain.TwainScanner)scanner, this.IsActivated);   //bookedge
			else if (scanner is Scanners.Click.ClickWrapper)
				this.frameStartScanner = new FrameStartClick(this, (Scanners.Click.ClickWrapper)scanner, this.IsActivated);
			else if (scanner is Scanners.Click.ClickMiniWrapper)
				this.frameStartScanner = new FrameStartClickMini(this, (Scanners.Click.ClickMiniWrapper)scanner, this.IsActivated);
			else if (scanner is Scanners.S2N.Bookeye4Wrapper)
				this.frameStartScanner = new FrameStartS2N(this, (Scanners.S2N.Bookeye4Wrapper)scanner, this.IsActivated);
			else if (scanner is Scanners.S2N.ScannerS2NWrapper)
				this.frameStartScanner = new FrameStartS2N(this, (Scanners.S2N.ScannerS2NWrapper)scanner, this.IsActivated);            
            else if (scanner is Scanners.Twain.AdfScanner)
                this.frameStartScanner = new FrameStartAdf(this, (Scanners.Twain.AdfScanner)scanner, this.IsActivated);

			this.frameStartUi.ScannerChanged(this.frameStartScanner != null);
		}
		#endregion

		#region Scanner_ImageScannedTU()
		void Scanner_ImageScannedTU(Bitmap bitmap)
		{
			try
			{
				this.AddImage(bitmap);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(ex.Message);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("The scanning process was interrupted! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
			finally
			{
				try { bitmap.Dispose(); }
				catch { }

				UnLock();
			}
		}

		void Scanner_ImageScannedTU(TwainApp.TwainImage twainImage)
		{
			try
			{
				this.AddImage(twainImage.Bitmap);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(ex.Message);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("The scanning process was interrupted! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
			finally
			{
				try { twainImage.Dispose(); }
				catch {}
				
				UnLock();
			}
		}

		void Scanner_ImageScannedTU(string file)
		{
			try
			{
				this.AddImage(file);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(ex);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("The scanning process was interrupted! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region Scanner_OperationErrorTU()
		void Scanner_OperationErrorTU(Exception ex)
		{
			try
			{				
				this.MainWindow.Activate();

				if (ex != null && ex is IllException)
				{
					ShowWarningMessage(ex.Message);
				}
				else if (ex != null)
				{

					ShowErrorMessage("The scanning process was interrupted! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
					Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				}
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region Scanner_ProgressChangedTU()
		void Scanner_ProgressChangedTU(string description, float progress)
		{
			try
			{
				if (this.IsActivated)
				{
					LockProgressChanged(progress);
					LockDescriptionChanged(description);
				}
			}
			catch (IllException)
			{
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
		}
		#endregion

		#region DoFormsProcessing()
		BscanILL.FP.FormsProcessingResult DoFormsProcessing(FileInfo imageFile)
		{
			BscanILL.FP.FormsProcessingResult result = BscanILL.FP.FormsProcessing.Go
				( this.MainWindow, imageFile, _settings.FormsProcessing.BsaFile, _settings.FormsProcessing.ScriptFile, _settings.FormsProcessing.TrainingName);
            
/*
            if ((result != null) && (result.Address.Length > 0) && ( string.Compare( result.Address , "N/A" ) != 0 ) )
            if (_settings.Export.Email.EmailValidation)            
            {
              if (_settings.Export.Email.EmailValidationType != SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP)
              {
                if (!BscanILL.Misc.EmailValidatorHttp.ValidateEmailHttp(result.Address))        //Http validation -> where we send the email address to our web server where the validation gets perform to overcome firewall issues with SMTP blocking at customer's network
                {
                    if (_settings.Export.Email.EmailValidationType == SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both)
                    { 
                        //try also SMTP validation
                        if (!BscanILL.Misc.EmailValidator.ValidateEmail(result.Address))
                        {
                            // both validation failed -> clear the OCR'ed value
                            //result.Address = "";

                            //set error flag
                            result.AddressFlag = true;
                        }
                    }
                    else
                    {
                        //validation failed -> clear the OCR'ed value
                        //result.Address = "";

                        //set error flag
                        result.AddressFlag = true;
                    }
                }
              }
              else
              if (! BscanILL.Misc.EmailValidator.ValidateEmail(result.Address))       //SMTP validation -> might be failing at customers because of the firewall
              {
                  //validation failed -> clear the OCR'ed value
                  //result.Address = "";

                  //set error flag
                  result.AddressFlag = true;
              }              
            }

            if (result != null)
            if(      ( result.DeliveryMethod == BscanILL.Export.ExportType.Email )
                  || ( result.DeliveryMethod == BscanILL.Export.ExportType.ArticleExchange )
                  || ( result.DeliveryMethod == BscanILL.Export.ExportType.Ariel ) 
                  || ( (result.DeliveryMethod == BscanILL.Export.ExportType.FtpDir) &&  _settings.Export.FtpDirectory.SendConfirmationEmail  )    
                  || ( (result.DeliveryMethod == BscanILL.Export.ExportType.Ftp) &&  _settings.Export.FtpServer.SendConfirmationEmail  )    )
            {
                if((result.Address.Length == 0) || ( string.Compare( result.Address , "N/A" ) == 0 ) )
                {
                    result.AddressFlag = true;  //set flag when email address empty when email, AE, Ariel of FtpDir delivery when address is mandatory field
                }
            }
*/
            if(result != null)
            {
                bool addressCorrect = BscanILL.Misc.EmailValidator.IsAddressValid(result.Address, result.DeliveryMethod, _settings.Export, _settings.Export.FtpServer.SendConfirmationEmail, _settings.Export.FtpDirectory.SendConfirmationEmail);
                //result.AddressFlag = !addressCorrect;
                result.AddressFlag |= !addressCorrect;          //if upper/lower case address mismatch, do not overwrite error flag in case address passes other validation (if the other http validation is turned off in settup then it would get automatically reset-updated which we do not want
                                                                // becasue if upper/lower case mismatch and http/smtp validation turned off - we would not get red flag next to email address field)
            }

			return result;
		}
		#endregion

		#region AskForArticleDetails()
		bool AskForArticleDetails(BscanILL.FP.FormsProcessingResult fpResult)
		{
			if (this.frameStartUi.Dispatcher.CheckAccess())
			{
				//if (this.MessageWindow != null)
					//this.MessageWindow.Hide();
				HideLockUi();

				BscanILL.UI.Dialogs.ArticleDlg dlg = new Dialogs.ArticleDlg();
				return dlg.Open(fpResult);
			}
			else
			{
				return (bool)this.frameStartUi.Dispatcher.Invoke((Func<bool>)delegate()
				{
					//if (this.MessageWindow != null)
					//	this.MessageWindow.Hide();
					HideLockUi();
				
					BscanILL.UI.Dialogs.ArticleDlg dlg = new Dialogs.ArticleDlg();
					return dlg.Open(fpResult);
				});
			}
		}
		#endregion

		#region CreateArticleThread()
		private void CreateArticleThread(object obj)
		{
			try
			{
				string pullslipFile = (string)obj;

				BscanILL.Hierarchy.Article article = CreateArticle(pullslipFile);
				
				this.frameStartUi.Dispatcher.Invoke((Action)delegate()
				{
					if (article != null)
						ArticleCreatedSuccessfully(article);
					else
						ArticleCreationCanceled();
				});
			}
			catch (Exception ex)
			{
				this.frameStartUi.Dispatcher.Invoke((Action)delegate()
				{
					ArticleCreationError(ex);
				});
			}
		}
		#endregion

		#region ImportArticleThread()
		private void ImportArticleThread(object obj)
		{
			try
			{
				LockWithProgressBar(true, "Importing images...");

				ImageImport imageImport = new ImageImport();
				imageImport.ProgressChanged += delegate(double progress) { LockProgressChanged(progress); };
				List<ImportedImage> importImages = imageImport.GetImportImages((ObservableCollection<FileInfo>)obj);

				if (importImages.Count > 0)
				{
					string pullsheet = importImages[0].File;

					if (importImages[0].DeleteAfterImport == false)
					{
						string filePath = ImageImport.GetUniqueFile(new DirectoryInfo(BscanILL.SETTINGS.Settings.Instance.General.TempDir), ImageFormat.Tiff);
						
						File.Copy(pullsheet, filePath);
						pullsheet = filePath;
					}

					BscanILL.Hierarchy.Article article = CreateArticle(pullsheet);

					if (article != null)
					{
						for (int i = 1; i < importImages.Count; i++)
						{
							ImportedImage import = importImages[i];
							string newPath = article.GetIdenticalScanPath(import.FileFormat);

							if (import.DeleteAfterImport)
								File.Move(import.File, newPath);
							else
								File.Copy(import.File, newPath);

							article.Scans.Add(article, new FileInfo(newPath), import.ColorMode, import.FileFormat, import.Dpi, 0, 0);
							LockProgressChanged((i + 1.0) / importImages.Count);
						}
					}

                    this.frameStartUi.Dispatcher.Invoke((Action)delegate()
                    {
                        if (article != null)
                            ArticleImportedSuccessfully(article);
                        else
                            ArticleCreationCanceled();
                    });
				}
				else
				{
					this.frameStartUi.Dispatcher.Invoke((Action)delegate()
					{
						ArticleCreationError(new Exception("There are no images to import!"));
					});
				}
			}
			catch (Exception ex)
			{
				this.frameStartUi.Dispatcher.Invoke((Action)delegate()
				{
					ArticleCreationError(ex);
				});
			}
		}
		#endregion

		#region CreateArticle()
		private BscanILL.Hierarchy.Article CreateArticle(string file)
		{
			BscanILL.FP.FormsProcessingResult fpResult;

			try
			{
				fpResult = DoFormsProcessing(new FileInfo(file));
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "AddImage", "Forms processing error: " + ex.Message, ex);
				fpResult = new FP.FormsProcessingResult("", null, "", "", BscanILL.Export.ExportType.SaveOnDisk, false);
			}

			bool articleDetailsConfirmed = false;

			if (this.frameStartUi.Dispatcher.CheckAccess())
			{
				this.frameStartUi.ShowImage(file);
				articleDetailsConfirmed = AskForArticleDetails(fpResult);
			}
			else
			{
				articleDetailsConfirmed = (bool)this.frameStartUi.Dispatcher.Invoke((Func<bool>)delegate()
				{
					this.frameStartUi.ShowImage(file);
					return AskForArticleDetails(fpResult);
				});
			}

			if (articleDetailsConfirmed)
			{
				BscanILLData.Models.Helpers.NewDbArticle newArticleDb = new BscanILLData.Models.Helpers.NewDbArticle()
				{
#if TransNumber_LONG
                    TransactionNumberBig = fpResult.TN,                    
#else
                    TransactionNumber = fpResult.TN,
#endif
					IllNumber = fpResult.IllNumber,
					Patron = fpResult.PatronName,
					Address = fpResult.Address,
					ExportType = (BscanILLData.Models.ExportType)fpResult.DeliveryMethod,
					FolderName = DateTime.Now.GetHashCode().ToString(),
					Status = BscanILLData.Models.ArticleStatus.Creating
				};

				BscanILLData.Models.DbArticle articleDb = BscanILL.DB.Database.Instance.InsertArticle(newArticleDb);
				articleDb.FolderName = articleDb.Id.ToString("0000000000");
				BscanILL.Hierarchy.Article article = new BscanILL.Hierarchy.Article(articleDb);

				string filePath = article.GetIdenticalScanPath(Scanners.FileFormat.Tiff);
				File.Move(file, filePath);

				article.AddScanPullslip(filePath, Scanners.ColorMode.Bitonal, Scanners.FileFormat.Tiff, 300);
				article.Status = Hierarchy.ArticleStatus.Active;

				return article;
			}
            else
            {
                File.Delete(file);
            }

			return null;
		}
		#endregion

		#region ArticleCreatedSuccessfully()
		private void ArticleCreatedSuccessfully(BscanILL.Hierarchy.Article article)
		{
			try
			{
				this.MainWindow.ArticleCreated(article);
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "ArticleCreatedSuccessfully", ex.Message, ex);
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region ArticleCreationCanceled()
		private void ArticleCreationCanceled()
		{
			Reset();
			UnLock();
		}
		#endregion

		#region ArticleCreationError()
		private void ArticleCreationError(Exception ex)
		{
			try
			{
				Notify(this, Notifications.Type.Error, "ArticleCreatedSuccessfully", ex.Message, ex);
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region ArticleImportedSuccessfully()
		private void ArticleImportedSuccessfully(BscanILL.Hierarchy.Article article)
		{
			try
			{
				this.MainWindow.ArticleCreated(article);
				//this.MainWindow.ActivateStage(BscanILL.MainWindow.Stage.Scan);
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "ArticleCreatedSuccessfully", ex.Message, ex);
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region FrameScanBookeye4_ScanRequest()
		internal void FrameScanBookeye4_ScanRequest(Scanners.S2N.ScannerScanAreaSelection scanArea)
		{
			this.frameStartUi.Dispatcher.Invoke((Action)delegate() { Scan_Click(); });
		}
		#endregion

		#region Adf_OpenAdditionalSettings()
		void Adf_OpenAdditionalSettings()
		{
			try
			{
				Lock();

				if (this.sm.PrimaryScanner != null && this.sm.PrimaryScanner is Scanners.Twain.AdfScanner)
					((Scanners.Twain.AdfScanner)this.sm.PrimaryScanner).OpenSetupWindow();
				if (this.sm.SecondaryScanner != null && this.sm.SecondaryScanner is Scanners.Twain.AdfScanner)
					((Scanners.Twain.AdfScanner)this.sm.SecondaryScanner).OpenSetupWindow();
			}
			catch (Exception ex)
			{
				notifications.Notify(this, Notifications.Type.Error, "FrameScan, Adf_OpenAdditionalSettings(): " + ex.Message, ex);
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region Adf_ImageScanned()
		void Adf_ImageScanned(int operationId, TwainApp.TwainImage twainImage, bool moreImagesToTransfer)
		{
			Scanner_ImageScanned(twainImage);
		}
		#endregion

		#region Adf_ProgressChanged()
		void Adf_ProgressChanged(string description, float progress)
		{
			Scanner_ProgressChanged(description, progress);
		}
		#endregion

		#region Adf_ScanError()
		void Adf_ScanError(int operationId, Exception ex)
		{
			Scanner_ScanError(ex);
		}
		#endregion

		#endregion

	}
}
