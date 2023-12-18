//#define IRIS_ENGINE   //must add \Shared\IdrsWrapper3.dll into References when using Iris

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using BscanILL.Misc;
using System.IO;
using SmartAssembly.Attributes;

namespace BscanILL
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, Scanners.PingDeviceReceiver
	{
		bool disposed = false;
		
		//BscanILL.Hierarchy.Article			article = null;
        BscanILL.Hierarchy.SessionBatch     batch = null;

		//Scanners.IScanner					scanner = null;

		BscanILL.UI.Windows.SecondWindow	secondWindow;

		BscanILL.UI.Frames.Start.FrameStart frameStart;
		BscanILL.UI.Frames.Scan.FrameScan frameScan;
		BscanILL.UI.Frames.Edit.FrameEdit frameEdit;
		BscanILL.UI.Frames.Export.FrameExport frameExport;
		BscanILL.UI.Frames.Resend.FrameResend frameResend;
		BscanILL.UI.Frames.Help.FrameHelp frameHelp;

		public event BscanILL.Misc.VoidHnd ArticleChanged;

        string wakeOnLANLastIP = "";


		#region constructor
		public MainWindow()
		{
			BscanILL.SETTINGS.ScanSettings.Load();
            BscanILL.SETTINGS.ScanSettingsPullSlips.Load();

			InitializeComponent();

            batch = new Hierarchy.SessionBatch();
 
			frameStart = new BscanILL.UI.Frames.Start.FrameStart(this);
			frameScan = new BscanILL.UI.Frames.Scan.FrameScan(this);
			frameEdit = new UI.Frames.Edit.FrameEdit(this);
			frameExport = new UI.Frames.Export.FrameExport(this);
			frameResend = new UI.Frames.Resend.FrameResend(this);
			frameHelp = new UI.Frames.Help.FrameHelp(this);

			//frameStartUi.GoToScanClick += delegate() { ActivateStage(Stage.Scan); };
            frameStartUi.GoToScanClick += delegate() {
                if( Batch.Articles.Count > 0 )
                  ActivateStage(Stage.Scan); 
            };
			frameStartUi.ResendClick += delegate() { ActivateStage(Stage.Resend); };
			frameStartUi.HelpClick += delegate() { ActivateStage(Stage.Help); };
			frameStartUi.OpenSettingsClick += delegate() { OpenSettings(); };

            frameScanUi.ScanPullslipClick += new VoidHnd(ScanPullSlip_Click);
			frameScanUi.GoToStartClick += delegate() { ActivateStage(Stage.Start); };
			frameScanUi.GoToItClick += delegate() { ActivateStage(Stage.Edit); };
			frameScanUi.ResendClick += delegate() { ActivateStage(Stage.Resend); };
            frameScanUi.ResetClick += delegate() { ResetSession(); };
            frameScanUi.ScanSessionReset += delegate() { ResetSession_NoMessage(); };
			frameScanUi.HelpClick += delegate() { ActivateStage(Stage.Help); };

			frameEditUi.GoToStartClick += delegate() { ActivateStage(Stage.Start); };
			frameEditUi.GoToScanClick += delegate() { ActivateStage(Stage.Scan); };
			frameEditUi.GoToExportClick += delegate() { ActivateStage(Stage.Export); };
			frameEditUi.ResendClick += delegate() { ActivateStage(Stage.Resend); };
			frameEditUi.HelpClick += delegate() { ActivateStage(Stage.Help); };

            frameEditUi.CreatePageDerivFilesEvent += delegate() { CreatePageDerivFilesInExport(); };

			frameExportUi.GoToStartClick += delegate() { ActivateStage(Stage.Start); };
			frameExportUi.GoToScanClick += delegate() { ActivateStage(Stage.Scan); };
			frameExportUi.GoToItClick += delegate() { ActivateStage(Stage.Edit); };
			frameExportUi.ResendClick += delegate() { ActivateStage(Stage.Resend); };
			frameExportUi.HelpClick += delegate() { ActivateStage(Stage.Help); };

			frameResendUi.GoToStartClick += delegate() { ActivateStage(Stage.Start); };
			frameResendUi.GoToScanClick += delegate() { if(this.Article != null) ActivateStage(Stage.Scan); };
			frameResendUi.GoToItClick += delegate() { if (this.Article != null) ActivateStage(Stage.Edit); };
			frameResendUi.HelpClick += delegate() { ActivateStage(Stage.Help); };
			frameResendUi.OpenDbArticleInScan += new UI.Frames.Resend.FrameResendUi.DbArticleHnd(FrameResendUi_OpenDbArticleInScan);
			frameResendUi.OpenDbArticleInCleanUp += new UI.Frames.Resend.FrameResendUi.DbArticleHnd(FrameResendUi_OpenDbArticleInCleanUp);
			frameResendUi.OpenDbArticleInSend += new UI.Frames.Resend.FrameResendUi.DbArticleHnd(FrameResendUi_OpenDbArticleInSend);
			frameResendUi.ResendDbArticle += new UI.Frames.Resend.FrameResendUi.DbArticleHnd(FrameResendUi_ResendDbArticle);

			frameHelpUi.GoToStartClick += delegate() { ActivateStage(Stage.Start); };
			frameHelpUi.GoToScanClick += delegate() { ActivateStage(Stage.Scan); };
			frameHelpUi.GoToItClick += delegate() { ActivateStage(Stage.Edit); };
			frameHelpUi.ResendClick += delegate() { ActivateStage(Stage.Resend); };

			if ( (BscanILL.UI.Misc.ScreensCount > 1) && (_settings.General.PreviewWindowEnabled == true) )
			{
				this.secondWindow = new UI.Windows.SecondWindow();
				this.SecondWindow.Visible = false;
			}

			Thread startUpThread = new Thread(new ThreadStart(StartUp));
			startUpThread.Name = "StartUp Thread";
			startUpThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			startUpThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			startUpThread.SetApartmentState(ApartmentState.STA);
			startUpThread.Start();
		}
		#endregion


		#region enum Stage
		public enum Stage
		{
			Start,
			Scan,
			Edit,
			Export,
			Resend,
			Help
		}
		#endregion

		// PUBLIC PROPERTIES
		#region public properties

		public BscanILL.UI.Frames.Start.FrameStartUi	FrameStartUi { get { return this.frameStartUi; } }
		public BscanILL.UI.Frames.Scan.FrameScanUi		FrameScanUi { get { return this.frameScanUi; } }
		public BscanILL.UI.Frames.Edit.FrameEditUi		FrameEditUi { get { return this.frameEditUi; } }
		public BscanILL.UI.Frames.Export.FrameExportUi	FrameExportUi { get { return frameExportUi; } }
		public BscanILL.UI.Frames.Resend.FrameResendUi	FrameResendUi { get { return this.frameResendUi; } }
		public BscanILL.UI.Frames.Help.FrameHelpUi		FrameHelpUi { get { return this.frameHelpUi; } }

		public BscanILL.UI.Windows.SecondWindow			SecondWindow { get { return this.secondWindow; } }

        public BscanILL.UI.Frames.Scan.FrameScan FrameScan { get { return this.frameScan; } } 

        #region Batch
        public BscanILL.Hierarchy.SessionBatch Batch
        {
            get { return this.batch; }
        }
        #endregion

		#region Article
		public BscanILL.Hierarchy.Article		Article 
		{ 
			//get { return this.article; } 
            get { return this.Batch.CurrentArticle; } 
			set 
			{
				//if (this.article != value)
                if (this.Article != value)
				{
					//this.article = value;
                    this.Batch.CurrentArticle = value;

					if (ArticleChanged != null)
						ArticleChanged();
				}
			} 
		}
		#endregion

		#region Scanner
		/*public Scanners.IScanner Scanner
		{
			get { return BscanILL.Scan.ScannersControl.PrimaryScanner; }
			set
			{
				if (this.Dispatcher.CheckAccess())
				{
					if (BscanILL.Scan.ScannersControl.PrimaryScanner != value)
					{
						BscanILL.Scan.ScannersControl.PrimaryScanner = value;
					}
				}
				else
				{
					this.Dispatcher.Invoke((Action)delegate()
					{
						if (BscanILL.Scan.ScannersControl.PrimaryScanner != value)
						{
							BscanILL.Scan.ScannersControl.PrimaryScanner = value;
						}
					});
				}
			}
		}*/
		#endregion

        #endregion


		// PRIVATE PROPERTIES
		#region private properties

		BscanILL.SETTINGS.Settings		_settings { get { return BscanILL.SETTINGS.Settings.Instance; } }
		BscanILL.Scan.ScannersManager	sm { get { return BscanILL.Scan.ScannersManager.Instance; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

        #region ResetSession
        public void ResetSession()
        {
            if (MessageBox.Show("Are you sure you want to reset current session? (Articles will not be deleted from BscanILL database...)", "Reset session", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ResetSession_NoMessage();
            }
        }
        #endregion

        #region ResetSession_NoMessage
        public void ResetSession_NoMessage()
        {
                this.Batch.Reset();
                frameScan.Reset();
                ActivateStage(Stage.Start);
        }
        #endregion

		#region Lock()
		internal void Lock()
		{
			if (this.Dispatcher.CheckAccess())
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.IsEnabled = false;
					this.Cursor = Cursors.Wait;
				});
			}
		}
		#endregion

		#region LockWithProgressBar()
		internal virtual void LockWithProgressBar(bool isProgressVisible, string description)
		{
			if (this.Dispatcher.CheckAccess())
			{
				this.IsEnabled = false;
				this.Cursor = System.Windows.Input.Cursors.Wait;

				frameStartUpUi.Visibility = System.Windows.Visibility.Visible;
				frameStartUpUi.Description = description;
				frameStartUpUi.IsProgressVisible = isProgressVisible;
				frameStartUpUi.Progress = 0;
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.IsEnabled = false;
					this.Cursor = System.Windows.Input.Cursors.Wait;

					frameStartUpUi.Visibility = System.Windows.Visibility.Visible;
					frameStartUpUi.Description = description;
					frameStartUpUi.IsProgressVisible = isProgressVisible;
					frameStartUpUi.Progress = 0;
				});
			}
		}
		#endregion

		#region UnLock()
		internal void UnLock()
		{
			if (this.Dispatcher.CheckAccess())
			{
				this.IsEnabled = true;
				this.Cursor = null;

				frameStartUpUi.Visibility = System.Windows.Visibility.Hidden;
				Focus();
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.IsEnabled = true;
					this.Cursor = null;

					frameStartUpUi.Visibility = System.Windows.Visibility.Hidden;

					Focus();
				});
			}
		}
		#endregion

		#region LockProgressChanged()
		internal virtual void LockProgressChanged(double progress)
		{
			if (this.Dispatcher.CheckAccess())
			{
				this.frameStartUpUi.Progress = progress;
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.frameStartUpUi.Progress = progress;
				});
			}
		}
		#endregion

		#region LockDescriptionChanged()
		internal virtual void LockDescriptionChanged(string description)
		{
			if (this.Dispatcher.CheckAccess())
			{
				this.frameStartUpUi.Description = description;
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.frameStartUpUi.Description = description;
				});
			}
		}
		#endregion

		#region HideLockUi()
		internal void HideLockUi()
		{
			if (this.Dispatcher.CheckAccess())
			{
				frameStartUpUi.Visibility = System.Windows.Visibility.Hidden;
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					frameStartUpUi.Visibility = System.Windows.Visibility.Hidden;
				});
			}
		}
		#endregion

        #region ActivateStage()
        public void ActivateStage(Stage stage)
		{
			if (stage != Stage.Start)
				frameStart.Deactivate();
			if (stage != Stage.Scan)
				frameScan.Deactivate();
			if (stage != Stage.Edit)
				frameEdit.Deactivate();
			if (stage != Stage.Export)
				frameExport.Deactivate();
			if (stage != Stage.Resend)
				frameResend.Deactivate();
			if (stage != Stage.Help)
				frameHelp.Deactivate();

			if (stage == Stage.Start)
				frameStart.Activate();
			else if (stage == Stage.Scan)
				frameScan.Activate();
			else if (stage == Stage.Edit)
				frameEdit.Activate();
			else if (stage == Stage.Export)
				frameExport.Activate();
			else if (stage == Stage.Resend)
				frameResend.Activate();
			else if (stage == Stage.Help)
				frameHelp.Activate();
		}
		#endregion

        #region CreatePageDerivFilesInExport()
        public void CreatePageDerivFilesInExport()
        {
            frameExportUi.ProgressChanged += new UI.Frames.Export.FrameExportUi.UpdateEditProgressHnd(EditPregoressChanged);

            frameExportUi.CreatePageDerivFiles(this.Batch);

            frameExportUi.ProgressChanged -= new UI.Frames.Export.FrameExportUi.UpdateEditProgressHnd(EditPregoressChanged);
        }
        #endregion

        public void EditPregoressChanged(double progress)
        {
            frameEditUi.UpdateProgress(progress);
        }

        #region ScanPullSlip_Click()
        public void ScanPullSlip_Click()
        {
            if (this.FrameStartUi != null)
            {
              ActivateStage(Stage.Start);            
              this.FrameStartUi.ScanNextPullslip();
            }
        }
        #endregion


        #region DeleteArticle()

        public void DeleteArticle(BscanILLData.Models.DbArticle articleDb)  //soft deletion - sets just Deleted flag in database - image files not erased from disk
        {
            if (Batch.Articles.Count > 0)
            {
                foreach (Hierarchy.Article article in Batch.Articles)
                {
                    if (article.Id == articleDb.Id)
                    {
                        article.StopAllAutomatedProcessing();
                        Batch.DeleteArticle(article);
                        break;
                    }
                }
            }

            BscanILL.DB.Database.Instance.SetArticleStatus(articleDb, Hierarchy.ArticleStatus.Deleted);
        }
        #endregion

        #endregion


        // PRIVATE METHODS
		#region private methods

        #region ArticleAdd
        private void ArticleAdd(BscanILL.Hierarchy.Article article)
        {
            if (article != null)
            {
                this.Batch.AddCurrentArticle(article, _settings.General.MultiArticleSupportEnabled);
                this.Article = this.Batch.CurrentArticle;  
            }
        }
        #endregion

        #region Form_Loaded()
        private void Form_Loaded(object sender, RoutedEventArgs e)
		{
			foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
			{
				System.Drawing.Rectangle propertiesRect = new System.Drawing.Rectangle(Properties.Settings.Default.MainWindowLocation, Properties.Settings.Default.MainWindowSize);
				screen.Bounds.Inflate(10, 10);

				if (screen.Bounds.Contains(propertiesRect))
				{
					this.Left = Properties.Settings.Default.MainWindowLocation.X;
					this.Top = Properties.Settings.Default.MainWindowLocation.Y;
					this.Width = Properties.Settings.Default.MainWindowSize.Width;
					this.Height = Properties.Settings.Default.MainWindowSize.Height;
					this.WindowState = Properties.Settings.Default.MainWindowState;
					break;
				}
			}
		}
		#endregion

		#region Form_Closing()
		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (disposed == false)
			{
				Properties.Settings.Default.MainWindowLocation = new System.Drawing.Point((int)this.Left, (int)this.Top);
				Properties.Settings.Default.MainWindowSize = new System.Drawing.Size((int)this.Width, (int)this.Height);
				Properties.Settings.Default.MainWindowState = this.WindowState;
				Properties.Settings.Default.Save();

				BscanILL.Misc.MemoryManagement.Dispose();

				if (sm.PrimaryScanner != null)
				{
					if (sm.PrimaryScanner is Scanners.Twain.TwainScanner)
						((Scanners.Twain.TwainScanner)sm.PrimaryScanner).Dispose(this);
					else if (sm.PrimaryScanner is Scanners.Twain.AdfScanner)
						((Scanners.Twain.AdfScanner)sm.PrimaryScanner).Dispose(this);
					else
						sm.PrimaryScanner.Dispose();

					sm.PrimaryScanner = null;
				}

				if (sm.SecondaryScanner != null)
				{
					if (sm.SecondaryScanner is Scanners.Twain.TwainScanner)
						((Scanners.Twain.TwainScanner)sm.SecondaryScanner).Dispose(this);
					else if (sm.SecondaryScanner is Scanners.Twain.AdfScanner)
						((Scanners.Twain.AdfScanner)sm.SecondaryScanner).Dispose(this);
					else
						sm.SecondaryScanner.Dispose();

					sm.SecondaryScanner = null;
				}

				frameStart.Dispose();
				frameScan.Dispose();
				frameEdit.Dispose();
				frameExport.Dispose();
				frameResend.Dispose();
				frameHelp.Dispose();

                if (this.frameStartUi != null)
                { 
				  this.frameStartUi.Dispose();
                  this.frameStartUi = null;
                }

                if (this.frameScanUi != null)
                {
				  this.frameScanUi.Dispose();
				  this.frameScanUi = null;
                }

                if (this.frameEditUi != null )
                {
				  this.frameEditUi.Dispose();
				  this.frameEditUi = null;
                }

                if (this.frameExportUi != null )
                {
				  this.frameExportUi.Dispose();
				  this.frameExportUi = null;
                }

                if (this.frameResendUi != null)
                {
				  this.frameResendUi.Dispose();
				  this.frameResendUi = null;
                }

                if (this.frameHelpUi != null)
                {
				  this.frameHelpUi.Dispose();
				  this.frameHelpUi = null;
                }

				if (this.SecondWindow != null)
					this.SecondWindow.Dispose();

				BscanILL.IP.PreviewCreator.Instance.Dispose();
				BscanILL.Export.ExportFiles.PdfsBuilder.DisposeIfNecessary();

				try
				{
					BscanILL.SETTINGS.ScanSettings.Instance.Save();
                    BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.Save();
				}
#if DEBUG
				catch(Exception ex)
				{
					Console.WriteLine("ERROR in MainWindow, BscanILL.SETTINGS.ScanSettings.Instance.Save(): " + ex.Message);
				}
#else
				catch { }
#endif
				this.disposed = true;
				CleanUp();
				e.Cancel = true;
			}
		}
		#endregion

		#region LoadScanner()
		private void LoadScanner()
		{
			Scanners.IScanner scannerTemp = null;
			Scanners.SettingsScannerType scannerType = _settings.Scanner.General.ScannerType;

            if (sm.PrimaryScanner != null)      //try just always dispose scanners
            {
                DisposeScanner(sm.PrimaryScanner);
                sm.PrimaryScanner = null;
            }
            if (sm.SecondaryScanner != null)  //try just always dispose scanners
            {
                DisposeScanner(sm.SecondaryScanner);
                sm.SecondaryScanner = null;
            }

			if (scannerType == Scanners.SettingsScannerType.Click)
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					Scanners.Click.ClickWrapper clickWrapper = new Scanners.Click.ClickWrapper();
					scannerTemp = clickWrapper;
				});
			}
			else if (scannerType == Scanners.SettingsScannerType.ClickMini)
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					Scanners.Click.ClickMiniWrapper clickWrapper = new Scanners.Click.ClickMiniWrapper();
					scannerTemp = clickWrapper;
				});
			}
			else if (scannerType == Scanners.SettingsScannerType.iVinaFB6280E || scannerType == Scanners.SettingsScannerType.iVinaFB6080E)
				scannerTemp = GetTwainScannerFromMainThread(true);
			else if (    scannerType == Scanners.SettingsScannerType.KodakI1120 || scannerType == Scanners.SettingsScannerType.KodakI1150 || scannerType == Scanners.SettingsScannerType.KodakI1150New 
				      || scannerType == Scanners.SettingsScannerType.KodakI1405 || scannerType == Scanners.SettingsScannerType.KodakE1035 || scannerType == Scanners.SettingsScannerType.KodakE1040 )
				scannerTemp = GetAdfScannerFromMainThread(true);
			else
			{
                //delete first S2N scanner before loading new so we close properly old session first before opening new one with new scanner
                if (sm.PrimaryScanner != null)
                {
                    DisposeScanner(sm.PrimaryScanner);
                    sm.PrimaryScanner = null;
                }

                wakeOnLANLastIP = "" ;
				Scanners.Scanner.PingDevice(this, _settings.Scanner.S2NScanner.Ip, true);
				Scanners.DeviceInfo s2nInfo = Scanners.S2N.ScannerS2N.GetDeviceInfo(_settings.Scanner.S2NScanner.Ip);

				//if ( (s2nInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE4) || (s2nInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE5) )
                if (s2nInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE4)
				{
                    bool wakeOnLANPerformed = false;
                    if (wakeOnLANLastIP.Length > 0)
                    if( ((Scanners.DeviceInfoS2N)s2nInfo).Ip.Length > 0)
                    if( string.Compare( wakeOnLANLastIP ,((Scanners.DeviceInfoS2N)s2nInfo).Ip )  == 0)
                    {
                        wakeOnLANPerformed = true;
                    }
                    scannerTemp = new Scanners.S2N.Bookeye4Wrapper(((Scanners.DeviceInfoS2N)s2nInfo).Ip, wakeOnLANPerformed);
					((Scanners.S2N.Bookeye4Wrapper)scannerTemp).LockScannerUi( true );
				}
				else
					scannerTemp = new Scanners.S2N.ScannerS2NWrapper(((Scanners.DeviceInfoS2N)s2nInfo).Ip);
			}
	
			//check licensing
			//Licensing.LicenseResult result = Licensing.CheckScannerLicensing(this.Dispatcher, scannerTemp.DeviceInfo.SerialNumber);
			try
			{
				CheckScannerLicensing(scannerTemp.DeviceInfo.SerialNumber);
			}
			catch (Exception ex)
			{
				if (scannerTemp != null && sm.PrimaryScanner != scannerTemp)
					scannerTemp.Dispose();

				throw ex;
			}

			//assigning valid scanner
			if (sm.PrimaryScanner != null && sm.PrimaryScanner != scannerTemp)
				DisposeScanner(sm.PrimaryScanner);

			sm.PrimaryScanner = scannerTemp;

			//secondary scanner
			if (_settings.Scanner.AdfAddOnScanner.IsEnabled)
            {
                Scanners.Twain.AdfScanner adf = GetAdfScannerFromMainThread(false);

                if (sm.SecondaryScanner != null)  //bob
                {
                    if (sm.SecondaryScanner != adf)
                        DisposeScanner(sm.SecondaryScanner);
                }

                sm.SecondaryScanner = adf;
            }
			else
			{
				if (sm.MultiScannerMode)
				{
					DisposeScanner(sm.SecondaryScanner);
					sm.SecondaryScanner = null;
				}
			}

            if (sm.MultiScannerMode == false)
            {
                //force this because when secondary scanner was selected in Scan stage and we moved back to Start stage and turned off secondary scanner in Settings dialog
                //this flag stayed set to false and it caused using secondary scanner in Scan stage even there was not secondary scanner
                sm.IsPrimaryScannerSelected = true;
            }
		}
		#endregion

		#region DisposeScanner()
		private void DisposeScanner(Scanners.IScanner scanner)
		{
			if (scanner != null)
			{
				if (scanner is Scanners.Twain.TwainScanner)
					((Scanners.Twain.TwainScanner)scanner).Dispose(this);
				else if (scanner is Scanners.Twain.AdfScanner)
					((Scanners.Twain.AdfScanner)scanner).Dispose(this);
				else
					scanner.Dispose();
			}

			scanner = null;
		}
		#endregion

		#region PingDeviceProgressChanged()
		public void PingDeviceProgressChanged(string description)
		{
            // extract S2N scanner's IP string from description which is in format: "Scanner IP:#.#.#.# Wake On LAN Attempt #"
            int i = description.IndexOf("IP:");
            if( i >= 0 )
            {
                i = i + 3;
                if (i < description.Length)
                {
                    int j = description.IndexOf(" ", i);
                    if (j >= 0)
                    if(( j - i ) > 0 )
                    {
                        wakeOnLANLastIP = description.Substring(i, j - i);
                    }
                }                    
            }
		}
		#endregion

		#region ArticleCreated()
		public void ArticleCreated(BscanILL.Hierarchy.Article article)
		{
            if (this._settings.General.MultiArticleSupportEnabled == false)
            {
                this.Batch.Reset();
                frameScan.Reset();
            }
            ArticleAdd( article );
			//this.Article = article;
			ActivateStage(Stage.Scan);
		}
		#endregion

		#region FrameResendUi_OpenDbArticleInScan()
		void FrameResendUi_OpenDbArticleInScan(BscanILLData.Models.DbArticle article)
		{
			//this.Article = new Hierarchy.Article(article);
            this.Batch.Reset();
            frameScan.Reset();
            ArticleAdd(new Hierarchy.Article(article));
			ActivateStage(Stage.Scan);
		}
		#endregion

		#region FrameResendUi_OpenDbArticleInCleanUp()
		void FrameResendUi_OpenDbArticleInCleanUp(BscanILLData.Models.DbArticle article)
		{
			//this.Article = new Hierarchy.Article(article);
            this.Batch.Reset();
            frameScan.Reset();
            ArticleAdd(new Hierarchy.Article(article));
			ActivateStage(Stage.Edit);
		}
		#endregion

		#region FrameResendUi_OpenDbArticleInSend()
		void FrameResendUi_OpenDbArticleInSend(BscanILLData.Models.DbArticle article)
		{
			//this.Article = new Hierarchy.Article(article);
            this.Batch.Reset();
            frameScan.Reset();
            ArticleAdd(new Hierarchy.Article(article));
            ActivateStage(Stage.Export);            
		}
		#endregion

		#region FrameResendUi_ResendDbArticle()
		void FrameResendUi_ResendDbArticle(BscanILLData.Models.DbArticle article)
		{
			
		}
		#endregion

		#region StartUp()
		private void StartUp()
		{
			try
			{
				frameStartUpUi.Description = "Connecting to scanner...";
				bool connectionSuccessfull = false;

				frameStartUpUi.Progress = 0.1;

				while (connectionSuccessfull == false)
				{
					try
					{
						LoadScanner();
						connectionSuccessfull = true;
					}
					catch (Exception ex)
					{
						if (this.Dispatcher.CheckAccess())
						{
							MessageBox.Show("Can't load scanner! " + BscanILL.Misc.Misc.GetErrorMessage(ex));

							BscanILL.UI.Settings.SettingsDlg dlg = new UI.Settings.SettingsDlg( this.frameStart, this.Batch );

							if (dlg.ShowDialog(UI.Settings.SettingsDlg.Stage.Scanner) == true)
							{
								//BscanILL.Export.Email.Email.Instance.Refresh();
							}
							else
								throw new IllException("Bscan ILL will be closed.");
						}
						else
						{
							Exception except = (Exception)this.Dispatcher.Invoke((Func<Exception>)delegate()
							{
								try
								{
									MessageBox.Show("Can't load scanner! " + BscanILL.Misc.Misc.GetErrorMessage(ex));

									BscanILL.UI.Settings.SettingsDlg dlg = new UI.Settings.SettingsDlg( this.frameStart,  this.Batch );

									if (dlg.ShowDialog(UI.Settings.SettingsDlg.Stage.Scanner) == true)
										return null;
									else
										return new IllException("Bscan ILL will be closed.");
								}
								catch (Exception exception)
								{
									return exception;
								}
							});

							if (except != null)
								throw ex;
						}
					}
				}

				for (int i = 1; i < 5; i++)
				{
					frameStartUpUi.Progress = i / 10.0;
					Thread.Sleep(100);
				}

				if (_settings.Licensing.OcrEnabled || _settings.Licensing.AudioEnabled)
				{
					try
					{
//IRIS
#if IRIS_ENGINE
						BscanILL.Export.ExportFiles.Iris.LoadIris();
#else
                        BscanILL.Export.ExportFiles.Abbyy.LoadAbbyy();
#endif
					}
					catch (Exception exx)
					{
						Exception except = (Exception)this.Dispatcher.Invoke((Func<Exception>)delegate()
						{
							try
							{
								MessageBox.Show("Can't load OCR engine! " + BscanILL.Misc.Misc.GetErrorMessage(exx));

								BscanILL.UI.Settings.SettingsDlg dlg = new UI.Settings.SettingsDlg( this.frameStart , this.Batch );
                                
								//if (dlg.ShowDialog(UI.Settings.SettingsDlg.Stage.Scanner) == true)
                                dlg.ShowDialog(UI.Settings.SettingsDlg.Stage.Scanner) ;
                                if( dlg.LicenseFileJustInstalled )
                                {
									return null;
                                }
								else
                                {
									return new IllException("Bscan ILL will be closed.");
                                }
							}
							catch (Exception exception)
							{
								return exception;
							}
						});

						if (except != null)
							throw exx;
					}
				}

				BscanILL.DB.Database db = DB.Database.Instance;
				//db.CreateArticles();

				try
				{
					BscanILL.FP.FormsProcessing.Init();
				}
				catch (Exception ex)
				{
					string error = Misc.Misc.GetErrorMessage(ex);
					MessageBox.Show("Can't init Forms Processing! " + error);
				}

				for (int i = 5; i < 10; i++)
				{
					frameStartUpUi.Progress = i / 10.0;
					Thread.Sleep(100);
				}

				this.Dispatcher.BeginInvoke((Action)delegate() { StartUpSuccessfull(); });
			}
			catch (Exception ex)
			{
				this.Dispatcher.BeginInvoke((Action)delegate() { StartUpError(ex); });
			}
		}
		#endregion
        
        #region SecondaryWindowCheck()
		private void SecondaryWindowCheck()
        {
            if ((BscanILL.UI.Misc.ScreensCount > 1) && (_settings.General.PreviewWindowEnabled == true) && ( this.secondWindow == null ) )
            {
                this.secondWindow = new UI.Windows.SecondWindow();
                this.SecondWindow.Visible = false;
            }


            if ((BscanILL.UI.Misc.ScreensCount > 1) && (_settings.General.PreviewWindowEnabled == true) &&
                         //       (_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.S2N) && ( this.SecondWindow != null))          //allow secondary preview window for Click and Click Mini as well
                  ((_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.S2N) || (_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.ClickMini) || (_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.Click))
                  && (this.SecondWindow != null))
            {
                this.SecondWindow.Visible = true;
            }
            else
            {
                if (this.SecondWindow != null)
                    this.SecondWindow.Visible = false;
            }

        }            
		#endregion

		#region StartUpSuccessfull()
		private void StartUpSuccessfull()
		{
            if ((BscanILL.UI.Misc.ScreensCount > 1) && (_settings.General.PreviewWindowEnabled == true) &&
                //	(_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.S2N) && this.SecondWindow != null)           //allow secondary preview window for Click and Click Mini as well
                 ((_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.S2N) || (_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.ClickMini) || (_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.Click)) 
                 && (this.SecondWindow != null))
				this.SecondWindow.Visible = true;

			frameStartUpUi.SetVisibility(false);
			ActivateStage(Stage.Start);
			UnLock();
		}
		#endregion

		#region StartUpError()
		private void StartUpError(Exception ex)
		{
			if(ex != null)
				MessageBox.Show(Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
			
			ExitApplication();
		}
		#endregion

		#region ExitApplication()
		private void ExitApplication()
		{
			this.Close();
			
			/*if (Application.Current != null)
				Application.Current.Shutdown();

			/*if (Thread.CurrentThread != null)
				Thread.CurrentThread.Abort();*/
		}
		#endregion

		#region CheckScannerLicensing()
		/// <summary>
		/// Checks if license file exists
		///		1) Yes - checks if it is valid
		///			Yes - Good to go
		///			NO - throw exception about that license file is not good. 
		///		2) No - throw exception that license file doesn't exist
		/// </summary>
		/// <param name="dispatcher"></param>
		/// <param name="scanner"></param>
		public static void CheckScannerLicensing(string sn)
		{
			FileInfo licenseFile = Licensing.GetLicenseFile(sn);

			if (licenseFile.Exists)
			{
				if (BscanILL.Misc.Licensing.CheckLicensing(sn))
				{
					//BscanILL.Misc.Licensing.InstallNecessaryComponentsBasedOnLicensing(sn);
				}
				else
					throw new Exception("The license file associated with scanner '" + sn + "' is invalid!");
			}
			else
				throw new Exception("The license file associated with scanner '" + sn + "' is not installed on this Bscan ILL Site!");
		}
		#endregion

		#region class TwainConnectResult
		class TwainConnectResult
		{
			public readonly Exception Exception;
			public readonly object Scanner;

			public TwainConnectResult(object scanner, Exception ex)
			{
				this.Exception = ex;
				this.Scanner = scanner;
			}
		}
		#endregion

		#region GetTwainScannerFromMainThread()
		public Scanners.Twain.TwainScanner GetTwainScannerFromMainThread(bool primaryScanner)
		{
			TwainConnectResult result = (TwainConnectResult)this.Dispatcher.Invoke((Func<TwainConnectResult>)delegate()
			{
				try
				{
					if(primaryScanner)					
						return new TwainConnectResult(Scanners.Twain.TwainScanner.GetInstance(this, new Scanners.MODELS.Model(_settings.Scanner.General.ScannerType)), null);
					else
						return new TwainConnectResult(Scanners.Twain.TwainScanner.GetInstance(this, new Scanners.MODELS.Model(_settings.Scanner.AdfAddOnScanner.ScannerType)), null);
				}
				catch (Exception ex)
				{
					return new TwainConnectResult(null, ex);
				}
			});

			if (result.Exception != null)
				throw result.Exception;
			else
				return (Scanners.Twain.TwainScanner)result.Scanner;
		}
		#endregion

		#region GetAdfScannerFromMainThread()
		public Scanners.Twain.AdfScanner GetAdfScannerFromMainThread(bool primaryScanner)
		{
			TwainConnectResult result = (TwainConnectResult)this.Dispatcher.Invoke((Func<TwainConnectResult>)delegate()
			{
				try
				{
					if (primaryScanner)
						return new TwainConnectResult(Scanners.Twain.AdfScanner.GetInstance(this, new Scanners.MODELS.Model(_settings.Scanner.General.ScannerType)), null);
					else
						return new TwainConnectResult(Scanners.Twain.AdfScanner.GetInstance(this, new Scanners.MODELS.Model(_settings.Scanner.AdfAddOnScanner.ScannerType)), null);
				}
				catch (Exception ex)
				{
					return new TwainConnectResult(null, ex);
				}
			});

			if (result.Exception != null)
				throw result.Exception;
			else
				return (Scanners.Twain.AdfScanner)result.Scanner;
		}
		#endregion

		#region Window_PreviewKeyDown()
		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			bool ctrl = ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) > 0);
			bool alt = ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Alt) > 0);
			bool shift = ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Shift) > 0);

			switch (e.Key)
			{

                case Key.Right:
                case Key.Up:                
                    {
                        if (frameScan.IsActivated)
                        {
                            e.Handled = true;
                            frameScan.InitiatePageDown();
                        }
                        else if (frameEdit.IsActivated)
                        {
                            e.Handled = true;
                            frameEdit.InitiatePageDown();
                        }
                        else if (frameExport.IsActivated)
                        {
                            e.Handled = true;
                            frameExport.InitiatePageDown();
                        }
                    } break;

                case Key.PageDown:
                    {                        
                        if (frameScan.IsActivated)
                        {
                            e.Handled = true;
                            frameScan.InitiateArticleDown();
                        }
                        else if (frameEdit.IsActivated)
                        {
                            e.Handled = true;
                            frameEdit.InitiateArticleDown();
                        }
                        else if (frameExport.IsActivated)
                        {
                            e.Handled = true;
                            frameExport.InitiateArticleDown();
                        }
                    } break;

                case Key.Left:
                case Key.Down:                
                    {                        
                        if (frameScan.IsActivated)
                        {
                            e.Handled = true;
                            frameScan.InitiatePageUp();
                        }
                        else if (frameEdit.IsActivated)
                        {
                            e.Handled = true;
                            frameEdit.InitiatePageUp();
                        }
                        else if (frameExport.IsActivated)
                        {
                            e.Handled = true;
                            frameExport.InitiatePageUp();
                        }
                    } break;

                case Key.PageUp:
                    {                        
                        if (frameScan.IsActivated)
                        {
                            e.Handled = true;
                            frameScan.InitiateArticleUp();
                        }
                        else if (frameEdit.IsActivated)
                        {
                            e.Handled = true;
                            frameEdit.InitiateArticleUp();
                        }
                        else if (frameExport.IsActivated)
                        {
                            e.Handled = true;
                            frameExport.InitiateArticleUp();
                        }
                    } break;

                case Key.Home:
                    {                        
                        if (frameScan.IsActivated)
                        {
                            e.Handled = true;
                            frameScan.InitiateGoToHome();
                        }
                        else if (frameEdit.IsActivated)
                        {
                            e.Handled = true;
                            frameEdit.InitiateGoToHome();
                        }
                        else if (frameExport.IsActivated)
                        {
                            e.Handled = true;
                            frameExport.InitiateGoToHome();
                        }
                    } break;
                case Key.End:
                    {                        
                        if (frameScan.IsActivated)
                        {
                            e.Handled = true;
                            frameScan.InitiateGoToEnd();
                        }
                        else if (frameEdit.IsActivated)
                        {
                            e.Handled = true;
                            frameEdit.InitiateGoToEnd();
                        }
                        else if (frameExport.IsActivated)
                        {
                            e.Handled = true;
                            frameExport.InitiateGoToEnd();
                        }
                    } break;
				case Key.F1:
					{
						ActivateStage(Stage.Help);
					}break;
				case Key.F2:
					{
						if (frameStart.IsActivated)
						{
							e.Handled = true;
							frameStart.Scan();
						}
                        else
                        if (frameScan.IsActivated && _settings.General.MultiArticleSupportEnabled)
                        {
                            e.Handled = true;
                            frameScan.InitiateScanPullslip();
                        }
					} break;
				case Key.F3:
					{
						if (frameScan.IsActivated)
						{
							e.Handled = true;
							frameScan.InitiateScan();
						}
					} break;
				case Key.F4:
					{
						if (frameScan.IsActivated)
						{
							e.Handled = true;
							frameScan.InitiateInsertBefore();
						}
					} break;
				case Key.F5:
					{
						if (frameScan.IsActivated)
						{
							e.Handled = true;
							frameScan.InitiateRescan();
						}
					} break;
				case Key.F6:
					{
						if (frameScan.IsActivated)
						{
							e.Handled = true;							
							frameScan.InitiateSetColor('c');   //set Color
						}
					}
					break;
				case Key.F7:
					{
						if (frameScan.IsActivated)
						{
							e.Handled = true;						
							frameScan.InitiateSetColor('g'); //set Gray
						}
						else
						if (frameEdit.IsActivated)
						{
							e.Handled = true;
							frameEdit.InitiateRotate90CCV();
						}
					}
					break;
				case Key.F8:
                    {
						if (frameScan.IsActivated)
						{
							e.Handled = true;							
							frameScan.InitiateSetColor('b');  //set B&W
						}
						else
						if (frameEdit.IsActivated)
						{
							e.Handled = true;
							frameEdit.InitiateRotate90CV();
						}                        
                    } break;
				case Key.F9:
					{
						if (frameStart.IsActivated && BscanILL.SETTINGS.Settings.Instance.General.KicImportEnabled)
						{
							frameStart.KicImport_Click();
						}
						else
						if (frameScan.IsActivated)
						{
							e.Handled = true;
							// ResetSession();
							frameScan.InitiateReset();
						}
					}
					break;
				case Key.F11:
					{
						if (frameStart.IsActivated)
						{
							frameStart.DiskImport_Click();
						}
						else if (frameScan.IsActivated)
						{
							frameScan.DiskImport_Click();
						}
					}break;
                case Key.F12:
                    {
                        if (frameScan.IsActivated)
                        {
                            frameScan.InitiatePrint();
                        }
                        else if (frameExport.IsActivated)
                        {
                            frameExport.InitiatePrint();
                        }
                    } break;

				/*case System.Windows.Input.Key.S:
					{
						if (alt && ctrl && statistics != null)
							statistics.Open(this.ParentForm);
					} break;*/
				case System.Windows.Input.Key.T:
					{
						if (ctrl)
						{
							if (this.SecondWindow != null)
								this.SecondWindow.Visible = !this.SecondWindow.Visible;
						}
					} break;
				case System.Windows.Input.Key.O:
					{
						if (alt && ctrl)
						{
							if (_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.S2N)
							{
								try
								{
									Scanners.S2N.ScannerS2N scanner = Scanners.S2N.ScannerS2N.GetInstance(_settings.Scanner.S2NScanner.Ip);
									if (scanner != null && scanner.Settings.Light.IsDefined)
									{
										scanner.Settings.Light.Value = Scanners.S2N.LightSwitch.On;
										scanner.SetDevice(this);
									}
								}
								catch (Exception ex)
								{
									BscanILL.UI.Dialogs.AlertDlg.Show(this, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
								}
							}
						}
					} break;
				case System.Windows.Input.Key.P:
					{
						if (alt && ctrl)
						{
							if (_settings.Scanner.General.ScannerType == Scanners.SettingsScannerType.S2N)
							{
								try
								{
									Scanners.S2N.ScannerS2N scanner = Scanners.S2N.ScannerS2N.GetInstance(_settings.Scanner.S2NScanner.Ip);
									if (scanner != null && scanner.Settings.Light.IsDefined)
									{
										scanner.Settings.Light.Value = Scanners.S2N.LightSwitch.Off;
										scanner.SetDevice(this);
									}
								}
								catch (Exception ex)
								{
									BscanILL.UI.Dialogs.AlertDlg.Show(this, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
								}
							}
						}
					} break;
				case System.Windows.Input.Key.D7:
					{
						if (shift)
						{
							if (this.frameScan.IsActivated)
							{
								this.frameScan.InitiateScan();
								e.Handled = true;
							}
							/*else if (this.frameScan != null && this.frameScan.IsVisible)
							{
								this.frameScan.InitiateScan();
								e.Handled = true;
							}*/
						}
					} break;
				default:
					{
						if (this.frameScan.IsActivated)
							this.frameScanUi.CheckKeyStroke(e);
					}break;
			}
		}
		#endregion

		#region OpenSettings()
		private void OpenSettings()
		{
			try
			{                
				Lock();

				BscanILL.UI.Settings.SettingsDlg dlg = new BscanILL.UI.Settings.SettingsDlg(this.frameStart, this.Batch);

                this.frameStart.DeactivateScanner();

				this.frameStart.UnRegisterScanner();	//ClickMini's SettingsWizard unregisters listener and in order to Register back the ClickMinWrapper/ClickWrapper properly in code below when Cancel button pressed to close dialog, we need UnRegister the listener here

				if (dlg.ShowDialog() == true)
				{                    
					LoadScanner();
                                        
                    this.frameStart.ActivateSecondaryScanner();          //LoadScanner() does not activates secondary scanner   so we have to do it manually                           
				}
                else
                {
                    if (dlg.SettingsModified == false)
                    {
						this.frameStart.ReRegisterScanner();	//We need to Re-Register ClickMiniWrapper/ClickWrapper as listener to receive messsges/images from click mini
                        this.frameStart.ActivateScanner();
                    }
                    else
                    {
                        //for scenario where user changes scanner, hits Apply and then Cancel to close dialog we need to reload proper scanner
                        LoadScanner();
                        this.frameStart.ActivateSecondaryScanner();
                    }
                }

                SecondaryWindowCheck();
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region CleanUp()
		private void CleanUp()
		{
			try
			{
				LockWithProgressBar(true, "Cleaning Up Old Articles...");

				Thread thread = new Thread(new ThreadStart(CleanUpThread));
				thread.Name = "CleanUpControl, CleanUp_Click()";
				thread.SetApartmentState(ApartmentState.STA);
				thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				thread.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception while cleaning up! " + BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}
		}
		#endregion

		#region CleanUpThread()
		private void CleanUpThread()
		{
			try
			{
				BscanILL.Misc.CleanUp cleanUp = new BscanILL.Misc.CleanUp();
				cleanUp.DescriptionChanged += delegate(string description) { SetDescription(description); };
				cleanUp.ProgressChanged += delegate(double progress) { SetProgress(progress); };
				cleanUp.OperationDone += delegate() { OprerationSuccessfull(); };
				cleanUp.OperationError += delegate(Exception ex) { OprerationError(ex); };

				cleanUp.Execute(null, new DirectoryInfo(_settings.General.ArticlesDir));                
			}
			catch (Exception ex)
			{
				OprerationError(ex);
			}
		}
		#endregion

		#region SetProgress()
		private void SetProgress(double progress)
		{
			this.Dispatcher.Invoke((Action)delegate() { LockProgressChanged(progress); });
		}
		#endregion

		#region SetDescription()
		private void SetDescription(string description)
		{
			this.Dispatcher.Invoke((Action)delegate() { LockDescriptionChanged(description); });
		}
		#endregion

		#region OprerationSuccessfull()
		private void OprerationSuccessfull()
		{
			this.Dispatcher.Invoke((Action)delegate()
			{
				UnLock();
				//Application.Current.Shutdown();
				this.Close();
			});
		}
		#endregion

		#region OprerationError()
		private void OprerationError(Exception ex)
		{
			this.Dispatcher.Invoke((Action)delegate()
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			});
		}
		#endregion

		#endregion

	}
}
