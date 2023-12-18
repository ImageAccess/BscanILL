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
using BscanILL.Scan;
using BscanILL.FileSystem;


namespace BscanILL.UI.Frames.Scan
{
	public class FrameScan : FrameBase
	{
		#region variables

		private BscanILL.UI.Frames.Scan.FrameScanUi	frameScanUi;
		IFrameScanScanner							frameScanScanner = null;

		ScanAction									scanAction = ScanAction.Scan;
		BscanILL.Hierarchy.IllImage					insertBeforeImage = null;

		object										addImageLocker = new object();

		#endregion


		#region constructor
		public FrameScan(BscanILL.MainWindow mainWindow)
			: base(mainWindow)
		{
			this.frameScanUi = this.MainWindow.FrameScanUi;
			            
			this.frameScanUi.ScanClick += new VoidHnd(Scan_Click);            

			this.frameScanUi.InsertBeforeClick += new IllImageHnd(InsertBefore_Click);
			this.frameScanUi.RescanClick += new IllImageHnd(Rescan_Click);
			this.frameScanUi.DeleteImageClick += new IllImageHnd(DeleteImage_Click);
			this.frameScanUi.DiskImportClick += new VoidHnd(DiskImport_Click);
            this.frameScanUi.PrintClick += new BatchImageHnd(Print_Click);
			
			this.frameScanUi.S2N_MoreSettingsClick += new VoidHnd(S2N_OpenAdditionalSettings);
			this.frameScanUi.Bookedge_MoreSettingsClick += new VoidHnd(Twain_OpenAdditionalSettings);
			this.frameScanUi.Click_MoreSettingsClick += new VoidHnd(Click_OpenAdditionalSettings);
			this.frameScanUi.ClickMini_MoreSettingsClick += new VoidHnd(ClickMini_OpenAdditionalSettings);
			this.frameScanUi.Adf_MoreSettingsClick += new VoidHnd(Adf_OpenAdditionalSettings);


            this.frameScanUi.ScanSettings_Changed += new VoidHnd(ScannerSetting_Changed);             
            this.frameScanUi.ScanSplittingSettings_Changed += delegate(Scanners.S2N.ScannerScanAreaSelection scanArea)
            {
                ScannerSplittingSetting_Changed(scanArea);
            };

			this.MainWindow.ArticleChanged += delegate()
			{
				if (this.IsActivated)
					this.frameScanUi.ArticleChanged(this.Article);
			};

			sm.PrimaryScannerChanged += delegate() { this.frameScanUi.Dispatcher.BeginInvoke((Action)delegate() { PrimaryScanner_Changed(); }); };
			sm.SecondaryScannerChanged += delegate() { this.frameScanUi.Dispatcher.BeginInvoke((Action)delegate() { SecondaryScanner_Changed(); }); };

			frameScanUi.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(Preview_KeyDown);

            frameScanUi.ArticleModifiedNotification += new BscanILL.Misc.ArticleHnd(ArticleChanged);
            
		}
		#endregion        

		#region ScanAction()
		enum ScanAction
		{
			Scan,
			InsertBefore,
			Rescan
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Hierarchy.IllImage			SelectedImage { get { return (BscanILL.Hierarchy.IllImage)this.frameScanUi.SelectedImage; } }
		public System.Windows.Controls.UserControl	UserControl { get { return (System.Windows.Controls.UserControl)this.frameScanUi; } }
		public BscanILL.Hierarchy.Article			Article
        {
            get { return this.MainWindow.Article; }
            set 
            {
                if (this.Article != value)
                { 
                  this.MainWindow.Article = value; 
                }
            }
        }

        public BscanILL.Hierarchy.SessionBatch Batch
        {
            get { return this.MainWindow.Batch; }
        }


        public bool ArticleLoadedInScan
        {
            get 
            {
                if (this.Article != null)
                {
                    return this.Article.IsLoadedInScan;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (this.Article != null)
                {
                  this.Article.IsLoadedInScan = value;             
                }
            }
        }

		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		private Scanners.FileFormat			FileFormat { get { return frameScanUi.FileFormat; } }

		/// <summary>
		/// -1, 1
		/// </summary>
		private double						Brightness { get { return (int)(frameScanUi.Brightness); } }
		/// <summary>
		/// -1, 1
		/// </summary>
		private double						Contrast { get { return (int)(frameScanUi.Contrast); } }

		private Scanners.ColorMode			ColorMode { get { return frameScanUi.ColorMode; } }
		
		private ushort						Dpi { get { return frameScanUi.Dpi; } }

		BscanILL.Misc.Notifications			notifications { get { return BscanILL.Misc.Notifications.Instance; } }
		BscanILL.Scan.ScannersManager		sm { get { return BscanILL.Scan.ScannersManager.Instance; } }
		BscanILL.SETTINGS.ScanSettings		scanSettings { get { return BscanILL.SETTINGS.ScanSettings.Instance; } }

		#endregion


		//PUBLIC METHODS
		#region public methods
	
		#region Activate()
		public void Activate()
		{
			this.frameScanUi.Open(this.Batch);

			if (this.frameScanScanner != null)
			{
				this.frameScanScanner.Activate();

				if (this.frameScanScanner is FrameScanS2N)
                {
					((FrameScanS2N)this.frameScanScanner).ScanRequest += new Scanners.S2N.ScanRequestHnd(FrameScanS2N_ScanRequest);
                    ((FrameScanS2N)this.frameScanScanner).ScanPullSlpRequest += new Scanners.S2N.ScanRequestHnd(FrameScanS2N_ScanPullSlpRequest);                   
                }                
			}

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
			
			this.IsActivated = true;
		}
		#endregion

		#region Deactivate()
		public void Deactivate()
		{
            //   Reset();             

			this.frameScanUi.Visibility = Visibility.Hidden;

			if (this.frameScanScanner != null)
			{
				this.frameScanScanner.Deactivate();

                if (this.frameScanScanner is FrameScanS2N)
                {
                    ((FrameScanS2N)this.frameScanScanner).ScanRequest -= new Scanners.S2N.ScanRequestHnd(FrameScanS2N_ScanRequest);
                    ((FrameScanS2N)this.frameScanScanner).ScanPullSlpRequest -= new Scanners.S2N.ScanRequestHnd(FrameScanS2N_ScanPullSlpRequest);                    
                }
			}

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

			this.IsActivated = false;
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			try
			{
				this.frameScanUi.Reset();

				if (this.MainWindow.SecondWindow != null)
					this.MainWindow.SecondWindow.ShowDefaultImage();

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

        #region ArticleChanged()
        public void ArticleChanged( BscanILL.Hierarchy.Article article )
        {
            this.Article = article;
            if (this.Article != null)
            {
              this.Article.IsLoadedInScan = true;
            }
        }
        #endregion

        #region UpdateSplittingButtons()
        public void UpdateSplittingButtons(Scanners.S2N.ScannerScanAreaSelection scanArea)
        {
            if (this.frameScanUi != null)
            {
                this.frameScanUi.UpdateSplittingButtons(scanArea);
            }
        }
        #endregion

		#region AddImage()
		public void AddImage(string file, Scanners.FileFormat fileFormat)
		{
			AddImage(file, this.ColorMode, fileFormat, true);
		}

		public void AddImage(string file, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, bool renameFile)
		{
            if (renameFile)
            {
                string newPath = this.Article.GetIdenticalScanPath(fileFormat);

                if (file.ToLower() != newPath.ToLower())
                {
                    File.Move(file, newPath);
                    file = newPath;
                }
            }

			BscanILL.Hierarchy.IllImage illImage = this.Article.Scans.Add(this.Article, new FileInfo(file), colorMode, fileFormat, this.Dpi, this.Brightness, this.Contrast);
			this.frameScanUi.ShowImage(illImage);
		}

		public void AddImage(Bitmap bitmap)
		{
			Scanners.FileFormat fileFormat = this.FileFormat;
			string filePath = this.Article.GetIdenticalScanPath(fileFormat);

			if (fileFormat == Scanners.FileFormat.Png)
				bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
			else if (fileFormat == Scanners.FileFormat.Tiff && bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
			{
				BscanILL.Misc.Io.SaveTiffG4(filePath, bitmap);
			}
			else
				bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

			//bitmap.Dispose();
			BscanILL.Hierarchy.IllImage illImage = this.Article.Scans.Add(this.Article, new FileInfo(filePath), this.ColorMode, fileFormat, this.Dpi, this.Brightness, this.Contrast);
			this.frameScanUi.ShowImage(illImage);
		}
		#endregion

		#region AddPages()
		public void AddPages(string fileL, string fileR, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, int pixelsOverlapping)
		{
			BscanILL.Hierarchy.IllImage illImage1 = this.Article.Scans.Add(this.Article, new FileInfo(fileL), colorMode, fileFormat, this.Dpi, this.Brightness, this.Contrast);
			BscanILL.Hierarchy.IllImage illImage2 = this.Article.Scans.Add(this.Article, new FileInfo(fileR), colorMode, fileFormat, this.Dpi, this.Brightness, this.Contrast);
			this.frameScanUi.ShowImage(illImage2);
		}

		public void AddPages(Bitmap bitmapL, Bitmap bitmapR, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, int pixelsOverlapping)
		{
			//Scanners.FileFormat fileFormat = (colorMode == Scanners.ColorMode.Bitonal) ? Scanners.FileFormat.Png : Scanners.FileFormat.Jpeg;
			string filePathL = this.Article.GetIdenticalScanPath(fileFormat);

			if (fileFormat == Scanners.FileFormat.Png)
				bitmapL.Save(filePathL, System.Drawing.Imaging.ImageFormat.Png);
			else if (fileFormat == Scanners.FileFormat.Tiff && bitmapL.PixelFormat == PixelFormat.Format1bppIndexed)
			{
				//bitmapL.Save(filePathL, System.Drawing.Imaging.ImageFormat.Tiff);
				BscanILL.Misc.Io.SaveTiffG4(filePathL, bitmapL);
			}
			else
				bitmapL.Save(filePathL, System.Drawing.Imaging.ImageFormat.Jpeg);

			//bitmapL.Dispose();
			BscanILL.Hierarchy.IllImage illImageL = this.Article.Scans.Add(this.Article, new FileInfo(filePathL), colorMode, fileFormat, this.Dpi, this.Brightness, this.Contrast);

			string filePathR = this.Article.GetIdenticalScanPath(fileFormat);
			if (fileFormat == Scanners.FileFormat.Png)
				bitmapR.Save(filePathR, System.Drawing.Imaging.ImageFormat.Png);
			else if (fileFormat == Scanners.FileFormat.Tiff && bitmapR.PixelFormat == PixelFormat.Format1bppIndexed)
			{
				//bitmapR.Save(filePathR, System.Drawing.Imaging.ImageFormat.Tiff);
				BscanILL.Misc.Io.SaveTiffG4(filePathR, bitmapR);
			}
			else
				bitmapR.Save(filePathR, System.Drawing.Imaging.ImageFormat.Jpeg);

			//bitmapR.Dispose();
			BscanILL.Hierarchy.IllImage illImage2 = this.Article.Scans.Add(this.Article, new FileInfo(filePathR), colorMode, fileFormat, this.Dpi, this.Brightness, this.Contrast);
			this.frameScanUi.ShowImage(illImage2);
		}
		#endregion

		#region InsertImage()
		public void InsertImage(int index, string file, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat)
		{
			string newPath = this.Article.GetIdenticalScanPath(fileFormat);

			if (file.ToLower() != newPath.ToLower())
			{
				File.Move(file, newPath);
				file = newPath;
			}

			BscanILL.Hierarchy.IllImage illImage = this.Article.Scans.Insert(index, this.Article, new FileInfo(file), colorMode, fileFormat, this.Dpi, this.Brightness, this.Contrast);
			this.frameScanUi.ShowImage(illImage);
		}

		public void InsertImage(int index, Bitmap bitmap)
		{
			Scanners.FileFormat fileFormat = this.FileFormat;
			string filePath = this.Article.GetIdenticalScanPath(fileFormat);

			if (fileFormat == Scanners.FileFormat.Png)
				bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
			else if (fileFormat == Scanners.FileFormat.Tiff && bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
			{
				//bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Tiff);
				BscanILL.Misc.Io.SaveTiffG4(filePath, bitmap);
			}
			else
				bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

			//bitmap.Dispose();
			BscanILL.Hierarchy.IllImage illImage = this.Article.Scans.Insert(index, this.Article, new FileInfo(filePath), this.ColorMode, fileFormat, this.Dpi, this.Brightness, this.Contrast);
			this.frameScanUi.ShowImage(illImage);
		}
		#endregion

        #region InitiatePageDown()
        public void InitiatePageDown()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameScanUi.viewPanel.SelectNextImage();     
            }
        }
        #endregion

        #region InitiateArticleDown()
        public void InitiateArticleDown()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameScanUi.viewPanel.SelectPreviousArticle();     
            }
        }
        #endregion

        #region InitiateArticleUp()
        public void InitiateArticleUp()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameScanUi.viewPanel.SelectNextArticle();
            }
        }
        #endregion

        #region InitiatePageUp()
        public void InitiatePageUp()
        {
            if (this.IsActivated && this.IsEnabled)
            {                
                this.frameScanUi.viewPanel.SelectPreviousImage();
            }
        }
        #endregion

        #region InitiateGoToHome()
        public void InitiateGoToHome()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameScanUi.viewPanel.SelectFirstImage();
            }
        }
        #endregion

        #region InitiateGoToEnd()
        public void InitiateGoToEnd()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameScanUi.viewPanel.SelectLastImage();
            }                
        }
        #endregion

		#region InitiateScan()
		public void InitiateScan()
		{
			if (this.IsActivated && this.IsEnabled)
				Scan_Click();
		}
		#endregion

        #region InitiateScanPullslip()
        public void InitiateScanPullslip()
        {
            if (this.IsActivated && this.IsEnabled)
                ScanPullSlip();
        }
        #endregion

		#region InitiateInsertBefore()
		public void InitiateInsertBefore()
		{
			if (this.IsActivated && this.IsEnabled)
			{
				if(this.SelectedImage != null && this.SelectedImage.IsPullslip == false)				
					InsertBefore_Click(this.SelectedImage);
			}
		}
		#endregion

		#region InitiateRescan()
		public void InitiateRescan()
		{
			if (this.IsActivated && this.IsEnabled)
			{
				if (this.SelectedImage != null && this.SelectedImage.IsPullslip == false)
					Rescan_Click(this.SelectedImage);
			}
		}
		#endregion

		#region InitiateSetColor()		
		public void InitiateSetColor(char colorDepth)
		{
			if (this.IsActivated && this.IsEnabled)
			{
				UpdateColorDepthButtons(colorDepth);
			}
		}
		#endregion

		#region InitiateReset()
		public void InitiateReset()
        {
            if (this.IsActivated && this.IsEnabled)
            {                
                 Reset_Click();
            }
        }
        #endregion

        #region InitiatePrint()
        public void InitiatePrint()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                if ((Batch != null || Article != null) && SelectedImage != null)
                {
                    Print_Click(Batch, Article, SelectedImage);
                }
            }
        }
        #endregion

		#region ReleaseMemory()
		private void ReleaseMemory()
		{
			BscanILL.Misc.MemoryManagement.ReleaseUnusedMemory();
		}
		#endregion

		#region Dispose()
		public override void Dispose()
		{
			base.Dispose();
		}
		#endregion

		#region Scanner_ImageScanned()
		public void Scanner_ImageScanned(Bitmap bitmap)
		{
			this.MainWindow.Dispatcher.BeginInvoke((Action)delegate() { Scanner_ImageScannedTU(bitmap); });
		}

		public void Scanner_ImageScanned(TwainApp.TwainImage twainImage, bool moreImagesToTransfer)
		{
			this.MainWindow.Dispatcher.BeginInvoke((Action)delegate() { Scanner_ImageScannedTU(twainImage, moreImagesToTransfer); });
		}

		public void Scanner_ImageScanned(string file)
		{
			this.MainWindow.Dispatcher.BeginInvoke((Action)delegate() { Scanner_ImageScannedTU(file); });
		}
		#endregion

		#region Scanner_ImagesScanned()
		public void Scanner_ImagesScanned(Bitmap bitmapL, Bitmap bitmapR)
		{
			this.MainWindow.Dispatcher.BeginInvoke((Action)delegate() { Scanner_ImagesScannedTU(bitmapL, bitmapR); });
		}

		public void Scanner_ImagesScanned(string pageL, string pageR)
		{
			this.MainWindow.Dispatcher.BeginInvoke((Action)delegate() { Scanner_ImagesScannedTU(pageL, pageR); });
		}
		#endregion

		#region Scanner_OperationSuccessdfull()
		public void Scanner_OperationSuccessdfull()
		{
			this.MainWindow.Dispatcher.Invoke((Action)delegate() { Scanner_OperationSuccessdfullTU(); });
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
				Lock();
				BscanILL.UI.Dialogs.ImportFromDiskDlg dlg = new Dialogs.ImportFromDiskDlg();
				ObservableCollection<FileInfo> list = dlg.Open();

				if (list != null && list.Count > 0)
				{
					LockWithProgressBar(true, "Importing images...");

					Thread thread = new Thread(new ParameterizedThreadStart(ImportImagesThread));
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

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region UpdateSplittingButtons()		
		private void UpdateColorDepthButtons(char color)
		{
			if (this.frameScanUi != null)
			{
				if (this.frameScanScanner is FrameScanS2N)  //Bookeye
				{
					Scanners.S2N.ColorMode colorDepth;
                    switch (color)
					{
						case 'c':
							colorDepth = Scanners.S2N.ColorMode.Color; break;
						case 'g':
							colorDepth = Scanners.S2N.ColorMode.Grayscale; break;
						default:
							colorDepth = Scanners.S2N.ColorMode.Lineart; break;
					}

					this.frameScanUi.UpdateS2NColorDepthButtons(colorDepth);
				}
				else
				if (this.frameScanScanner is FrameScanTwain ) // Bookedge
                {
					Scanners.Twain.ColorMode colorDepth;
					switch (color)
					{
						case 'c':
							colorDepth = Scanners.Twain.ColorMode.Color; break;
						case 'g':
							colorDepth = Scanners.Twain.ColorMode.Grayscale; break;
						default:
							colorDepth = Scanners.Twain.ColorMode.Bitonal; break;
					}

					this.frameScanUi.UpdateTwainColorDepthButtons(colorDepth);
				}
				else
				if (this.frameScanScanner is FrameScanClick) // Click
				{
					Scanners.Click.ClickColorMode colorDepth;
					switch (color)
					{
						case 'c':
							colorDepth = Scanners.Click.ClickColorMode.Color; break;
						case 'g':
							colorDepth = Scanners.Click.ClickColorMode.Grayscale; break;
						default:
							colorDepth = Scanners.Click.ClickColorMode.Bitonal; break;
					}

					this.frameScanUi.UpdateClickColorDepthButtons(colorDepth);
				}
				else
				if (this.frameScanScanner is FrameScanClickMini) // ClickMini
				{
					Scanners.Click.ClickColorMode colorDepth;
					switch (color)
					{
						case 'c':
							colorDepth = Scanners.Click.ClickColorMode.Color; break;
						case 'g':
							colorDepth = Scanners.Click.ClickColorMode.Grayscale; break;
						default:
							colorDepth = Scanners.Click.ClickColorMode.Bitonal; break;
					}

					this.frameScanUi.UpdateClickMiniColorDepthButtons(colorDepth);
				}
			}
		}
		#endregion

		#region ImportImagesThread()
		private void ImportImagesThread(object obj)
		{
			try
			{
				ImageImport					imageImport = new ImageImport();
				imageImport.ProgressChanged += delegate(double progress) { LockProgressChanged(progress); };
				List<ImportedImage>			importImages = imageImport.GetImportImages((ObservableCollection<FileInfo>)obj);

				for (int i = 0; i < importImages.Count; i++)
				{
					ImportedImage	import = importImages[i];
					string			newPath = this.Article.GetIdenticalScanPath(import.FileFormat);

					if (import.DeleteAfterImport)
						File.Move(import.File, newPath);
					else
						File.Copy(import.File, newPath);

					this.Article.Scans.Add(this.Article, new FileInfo(newPath), import.ColorMode, import.FileFormat, import.Dpi, 0, 0);
					LockProgressChanged((i + 1.0) / importImages.Count);
				}
			}
			catch (Exception ex)
			{
				this.MainWindow.Dispatcher.Invoke((Action)delegate()
				{
					MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
				});
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region ProcessScanSingle()
		internal void ProcessScanSingle(string file)
		{
			try
			{
				AddImage(file, GetScanFileFormat(file));
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				throw new IllException("The scanning process was interrupted!" + " " + ex.Message);
			}
		}

		internal void ProcessScanSingle(Bitmap bitmap)
		{
			try
			{
				AddImage(bitmap);
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				throw new IllException("The scanning process was interrupted!" + " " + ex.Message);
			}
		}
		#endregion

		#region ProcessScanPage()
		internal void ProcessScanPage(string file, bool leftPage)
		{
			try
			{
				FileInfo								sourceFile = new FileInfo(file);
				Scanners.FileFormat						fileFormat = GetScanFileFormat(file);
				ImageProcessing.FileFormat.IImageFormat saveFileFormat = (fileFormat == Scanners.FileFormat.Png) ?
					(ImageProcessing.FileFormat.IImageFormat)new ImageProcessing.FileFormat.Png() : 
					(ImageProcessing.FileFormat.IImageFormat)new ImageProcessing.FileFormat.Jpeg(85);

				string clipFile = this.Article.GetIdenticalScanPath(fileFormat);

				ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();

				using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(sourceFile.FullName))
				{
					if (leftPage)
						copier.Copy(itDecoder, clipFile, saveFileFormat, new Rectangle(0, 0, itDecoder.Width / 2, itDecoder.Height));
					else
						copier.Copy(itDecoder, clipFile, saveFileFormat, Rectangle.FromLTRB(itDecoder.Width / 2, 0, itDecoder.Width, itDecoder.Height));
				}

				sourceFile.Refresh();
				sourceFile.Delete();

				AddImage(clipFile, fileFormat);
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				throw new IllException("The scanning process was interrupted!" + " " + ex.Message);
			}
		}
		#endregion

		#region GetScanPage()
		internal FileInfo GetScanPage(string file, bool leftPage)
		{
			Scanners.FileFormat							fileFormat = GetScanFileFormat(file);
			ImageProcessing.FileFormat.IImageFormat		saveFileFormat = GetIImageFormat(file) ;
			string										clipFile = this.Article.GetIdenticalScanPath(fileFormat);
			ImageProcessing.BigImages.ImageCopier		copier = new ImageProcessing.BigImages.ImageCopier();

			using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(file))
			{
				if (leftPage)
					copier.Copy(itDecoder, clipFile, saveFileFormat, new Rectangle(0, 0, itDecoder.Width / 2, itDecoder.Height));
				else
					copier.Copy(itDecoder, clipFile, saveFileFormat, Rectangle.FromLTRB(itDecoder.Width / 2, 0, itDecoder.Width, itDecoder.Height));
			}

			return new FileInfo(clipFile);
		}
		#endregion
	
		#region ProcessScanBoth()
		/*internal void ProcessScanBoth(string file)
		{
			try
			{
				FileInfo sourceFile = new FileInfo(file);
				Scanners.FileFormat fileFormat = GetScanFileFormat(file);

				string clip1File = this.Article.GetIdenticalScanPath(fileFormat);
				string clip2File = this.Article.GetIdenticalScanPath(fileFormat);

				ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();

				using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(sourceFile.FullName))
				{
					copier.Copy(itDecoder, clip1File, new ImageProcessing.FileFormat.Jpeg(85), new Rectangle(0, 0, itDecoder.Width / 2, itDecoder.Height));
					copier.Copy(itDecoder, clip2File, new ImageProcessing.FileFormat.Jpeg(85), Rectangle.FromLTRB(itDecoder.Width / 2, 0, itDecoder.Width, itDecoder.Height));
				}

				sourceFile.Refresh();
				sourceFile.Delete();

				AddPages(clip1File, clip2File, this.ColorMode, fileFormat, 0);
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				throw new IllException("The scanning process was interrupted!" + " " + ex.Message);
			}
		}*/

		/*internal void ProcessScanBoth(Bitmap bitmap)
		{
			try
			{
				Bitmap clipL = ImageProcessing.ImageCopier.Copy(bitmap, new Rectangle(0, 0, bitmap.Width / 2, bitmap.Height));
				Bitmap clipR = ImageProcessing.ImageCopier.Copy(bitmap, Rectangle.FromLTRB(bitmap.Width / 2 + 1, 0, bitmap.Width, bitmap.Height));

				AddPages(clipL, clip2, this.ColorMode, this.FileFormat, 0);

				clip1.Dispose();
				clip2.Dispose();
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				throw new IllException("The scanning process was interrupted!" + " " + ex.Message);
			}
		}*/
		#endregion

		#region GetScanBoth()
		internal void GetScanBoth(string file, out string pageL, out string pageR)
		{
            bool leftPageFirst = true ;
			Scanners.FileFormat						fileFormat = GetScanFileFormat(file);
			ImageProcessing.FileFormat.IImageFormat saveFileFormat = GetIImageFormat(file);
			ImageProcessing.BigImages.ImageCopier	copier = new ImageProcessing.BigImages.ImageCopier();

			pageL = this.Article.GetIdenticalScanPath(fileFormat);
			pageR = this.Article.GetIdenticalScanPath(fileFormat);

            if (this.frameScanScanner is FrameScanS2N)   //check if Bookeyes splitting parameter is set to Left or Right page first when splitting set to Both page splitting
            {
                if (scanSettings.S2N.Splitting_StartPage.Value == Scanners.S2N.SplittingStartPage.Right)
                {
                    leftPageFirst = false;
                }
            }                

			using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(file))
			{                                    
                    //copier.Copy(itDecoder, pageL, saveFileFormat, new Rectangle(0, 0, itDecoder.Width / 2, itDecoder.Height));
                    //copier.Copy(itDecoder, pageR, saveFileFormat, Rectangle.FromLTRB(itDecoder.Width / 2, 0, itDecoder.Width, itDecoder.Height));
                    copier.Copy(itDecoder, leftPageFirst ? pageL : pageR, saveFileFormat, new Rectangle(0, 0, itDecoder.Width / 2, itDecoder.Height));
                    copier.Copy(itDecoder, leftPageFirst ? pageR : pageL, saveFileFormat, Rectangle.FromLTRB(itDecoder.Width / 2, 0, itDecoder.Width, itDecoder.Height));                
			}
		}
		#endregion

		#region ProcessBookScan()
		/*internal void ProcessBookScan(Bitmap bitmap)
		{
			try
			{
				float confidence;
				ImageProcessing.ItImage itImage = null;

				ImageProcessing.Operations operations = new ImageProcessing.Operations(true, 0.2F, true, false, false);

				itImage = new ImageProcessing.ItImage(bitmap);
				itImage.OpticsCenter = bitmap.Width / 2;

				DateTime itStart = DateTime.Now;
				confidence = itImage.Find(operations);

				if (confidence > 0)
				{
					if (itImage.TwoPages)
					{
						Bitmap clip1 = itImage.GetResult(0);
						Bitmap clip2 = itImage.GetResult(1);

						AddImage(clip1);
						AddImage(clip2);

						clip1.Dispose();
						clip2.Dispose();
					}
					else
					{
						Bitmap clip = itImage.GetResult(0);

						AddImage(clip);
						clip.Dispose();
					}
				}
				else
				{
					AddImage(bitmap);
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				throw new IllException("The scanning process was interrupted!" + " " + ex.Message);
			}
		}*/

		/*internal void ProcessBookScan(string file)
		{
			try
			{
				float								confidence;
				ImageProcessing.IpSettings.ItImage	itImage = null;
				Scanners.FileFormat					fileFormat = GetScanFileFormat(file);

				FileInfo sourceFile = new FileInfo(file);
				ImageProcessing.Operations operations = new ImageProcessing.Operations(true, 0.2F, true, false, false);

				itImage = new ImageProcessing.IpSettings.ItImage(sourceFile);
				itImage.OpticsCenter = Math.Max(0, itImage.InchSize.Height - 8) / itImage.InchSize.Height;

				DateTime itStart = DateTime.Now;
				confidence = itImage.Find(file, operations);

				if (confidence > 0)
				{
					if (itImage.TwoPages)
					{
						BIP.Geometry.InchSize clipsSize = new BIP.Geometry.InchSize(Math.Max(itImage.PageL.ClipRectInch.Width, itImage.PageR.ClipRectInch.Width), Math.Max(itImage.PageL.ClipRectInch.Height, itImage.PageR.ClipRectInch.Height));
						itImage.SetClipsSize(clipsSize);

						string clip1File = this.Article.GetIdenticalScanPath(fileFormat);
						string clip2File = this.Article.GetIdenticalScanPath(fileFormat);

						itImage.Execute(sourceFile.FullName, 0, clip1File, new ImageProcessing.FileFormat.Jpeg(85));
						itImage.Execute(sourceFile.FullName, 1, clip2File, new ImageProcessing.FileFormat.Jpeg(85));

						sourceFile.Refresh();
						sourceFile.Delete();

						AddImage(clip1File, fileFormat);
						AddImage(clip2File, fileFormat);
					}
					else
					{
						string clipFile = this.Article.GetIdenticalScanPath(fileFormat);

						itImage.Execute(sourceFile.FullName, 0, clipFile, new ImageProcessing.FileFormat.Jpeg(85));

						sourceFile.Refresh();
						sourceFile.Delete();

						AddImage(clipFile, fileFormat);
					}
				}
				else
				{
					AddImage(file, fileFormat);
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				throw new IllException("The scanning process was interrupted!" + " " + ex.Message);
			}
		}*/
		#endregion

		#region ProcessAutoSplitImage()
		internal void ProcessAutoSplitImage(string file)
		{
			try
			{
				FileInfo sourceFile = new FileInfo(file);
				int splitterL, splitterR;

				DateTime itStart = DateTime.Now;
				double confidence = BIP.Books.PagesSplitter.FindPagesSplitterStatic(sourceFile, out splitterL, out splitterR);

				if (confidence > 0)
				{
					using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(file))
					{
						Rectangle clip1 = new Rectangle(0, 0, splitterR, itDecoder.Height);
						Rectangle clip2 = Rectangle.FromLTRB(splitterL, 0, itDecoder.Width, itDecoder.Height);

						Scanners.FileFormat fileFormat = (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite) ?
							Scanners.FileFormat.Png : Scanners.FileFormat.Jpeg;

						string clip1File = this.Article.GetIdenticalScanPath(fileFormat);
						string clip2File = this.Article.GetIdenticalScanPath(fileFormat);

						ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();

						if (fileFormat == Scanners.FileFormat.Png)
						{
							copier.Copy(itDecoder, clip1File, new ImageProcessing.FileFormat.Png(), clip1);
							copier.Copy(itDecoder, clip2File, new ImageProcessing.FileFormat.Png(), clip2);
						}
						else
						{
							copier.Copy(itDecoder, clip1File, new ImageProcessing.FileFormat.Jpeg(85), clip1);
							copier.Copy(itDecoder, clip2File, new ImageProcessing.FileFormat.Jpeg(85), clip2);
						}

						sourceFile.Refresh();
						sourceFile.Delete();

						AddPages(clip1File, clip2File, this.ColorMode, fileFormat, splitterR - splitterL);
					}
				}
				else
				{
					AddImage(file, GetScanFileFormat(file));
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				throw new IllException("The scanning process was interrupted!" + " " + ex.Message);
			}
		}

		internal void ProcessAutoSplitImage(Bitmap bitmap)
		{
			try
			{
				int splitterL, splitterR;

				DateTime itStart = DateTime.Now;
				double confidence = BIP.Books.PagesSplitter.FindPagesSplitterStatic(bitmap, out splitterL, out splitterR);

				if (confidence > 0)
				{
					Rectangle clip1 = new Rectangle(0, 0, splitterR, bitmap.Height);
					Rectangle clip2 = Rectangle.FromLTRB(splitterL, 0, bitmap.Width, bitmap.Height);

					Bitmap bitmapL = ImageProcessing.ImageCopier.Copy(bitmap, clip1);
					Bitmap bitmapR = ImageProcessing.ImageCopier.Copy(bitmap, clip2);

					AddPages(bitmapL, bitmapR, this.ColorMode, this.FileFormat, splitterR - splitterL);

					bitmapL.Dispose();
					bitmapR.Dispose();
				}
				else
				{
					AddImage(bitmap);
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				throw new IllException("The scanning process was interrupted!" + " " + ex.Message);
			}
		}
		#endregion

		#region ShowDefaultImage()
		void ShowDefaultImage()
		{
			if (frameScanUi != null)
				frameScanUi.ShowDefaultImage();
		}
		#endregion

		#region ShowIllImage()
		void ShowIllImage(BscanILL.Hierarchy.IllImage illImage)
		{
			this.frameScanUi.ShowImage(illImage);
		}
		#endregion

		#region FrameWpf_ImageSelected()
		void FrameWpf_ImageSelected(BscanILL.UI.Misc.ImageEventArgs args)
		{
			/*BscanILL.Hierarchy.IllImage illImage = args.Image as BscanILL.Hierarchy.IllImage;
			bool formLocked = this.locked;

			try
			{
				Lock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "FrameScan, FrameWpf_ImageSelected: " + ex.Message, ex);
				ShowWarningMessage(this.MainWindow, BscanILL.Languages.BscanILLStrings.Frames_ErrorChangingImageSelection_STR, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				if (formLocked == false)
					UnLock();
			}*/
		}
		#endregion

		#region Scan_Click()
		void Scan_Click()
		{
			if (this.IsActivated)
			{
				this.insertBeforeImage = null;

				if (this.frameScanScanner is FrameScanS2N)
					((FrameScanS2N)this.frameScanScanner).ScanArea = null;

				Scan(ScanAction.Scan);
			}
		}
		#endregion

        #region ScannerSplittingSetting_Changed()
        void ScannerSplittingSetting_Changed(Scanners.S2N.ScannerScanAreaSelection scanArea)
        {
            if (this.IsActivated)
            {
                if (this.frameScanScanner is FrameScanS2N)
                {
                    ScannerSplittingSettingChanged(scanArea);
                }
            }
        }
        #endregion

        #region ScannerSetting_Changed()
        void ScannerSetting_Changed()
        {
            if (this.IsActivated)
            {
                if (this.frameScanScanner is FrameScanS2N)
                {
                    ScannerSettingChanged();
                }
            }
        }
        #endregion

        #region InsertBefore_Click()
        void InsertBefore_Click(BscanILL.Hierarchy.IllImage illImage)
		{
			this.insertBeforeImage = illImage;

			if (this.frameScanScanner is FrameScanS2N)
				((FrameScanS2N)this.frameScanScanner).ScanArea = null;

			Scan(ScanAction.InsertBefore);
		}
		#endregion

        #region Print_Click()

        void Print_Click(BscanILL.Hierarchy.SessionBatch currentBatch, BscanILL.Hierarchy.Article currentArticle , BscanILL.Hierarchy.IllImage illImage)
        {
            BscanILL.Misc.Printing printDocs = new BscanILL.Misc.Printing(currentBatch, currentArticle, illImage);
            printDocs.PrintPageDialog();
        }
        #endregion

        #region Rescan_Click()
        void Rescan_Click(BscanILL.Hierarchy.IllImage illImage)
		{
			this.insertBeforeImage = illImage;

			if (this.frameScanScanner is FrameScanS2N)
				((FrameScanS2N)this.frameScanScanner).ScanArea = null;

			Scan(ScanAction.Rescan);
		}
		#endregion

        #region Reset_Click()
        void Reset_Click()
        {            
           this.MainWindow.ResetSession();
        }
        #endregion

        #region ScanPullSlip()
        void ScanPullSlip()
        {
            this.MainWindow.ScanPullSlip_Click();
        }
        #endregion

        #region Scan()
        void Scan(ScanAction action)
		{
			if (this.frameScanScanner != null && this.IsLocked == false)
			{
				try
				{
					LockWithProgressBar(false, "Scanning...");
					this.scanAction = action;

					//this.MessageWindow.Show(this.MainWindow, "Scanning...");

					if (this.frameScanScanner is FrameScanClick)
					{
						((FrameScanClick)this.frameScanScanner).Scan();

#if !DEBUG
				this.MainWindow.Activate();
#endif
					}
					if (this.frameScanScanner is FrameScanClickMini)
					{
						((FrameScanClickMini)this.frameScanScanner).Scan();

#if !DEBUG
				this.MainWindow.Activate();
#endif
					}
					else if (this.frameScanScanner is FrameScanTwain)
					{
						((FrameScanTwain)this.frameScanScanner).Scan();
					}
					else if (this.frameScanScanner is FrameScanS2N)
					{
						((FrameScanS2N)this.frameScanScanner).Scan();
					}
                    else if (this.frameScanScanner is FrameScanAdf)
                    {
                        ((FrameScanAdf)this.frameScanScanner).Scan();
                    }
					else
						throw new Exception("FrameScan, Scan(): Unsupported scanner!");
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

        #region ScannerSettingChanged() 
        void ScannerSettingChanged()
        {
            //if (this.frameScanScanner != null && this.IsLocked == false)
            if (this.frameScanScanner != null)  //cannot include testing for Locked screen here as when calling this method when closing Additional Settings dialog, screen is still locked at this moment
            {
                try
                {
                    if (this.frameScanScanner is FrameScanS2N)
                    {
                        ((FrameScanS2N)this.frameScanScanner).ScannerSettingChanged();
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage("Updating Scanner with Settings was interrupted! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
                    Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
                    UnLock();
                }
            }
        }
        #endregion

        #region ScannerSplittingSettingChanged()
        void ScannerSplittingSettingChanged(Scanners.S2N.ScannerScanAreaSelection scanArea)
        {            
            if (this.frameScanScanner != null) 
            {
                try
                {
                    if (this.frameScanScanner is FrameScanS2N)
                    {
                        ((FrameScanS2N)this.frameScanScanner).ScannerSplittingSettingChanged(scanArea);
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage("Updating Scanner's touch screen was interrupted! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
                    Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
                    UnLock();
                }
            }
        }
        #endregion

		#region Crop_Requested()
		/*void Crop_Requested(BscanILL.IP.IllImageOperations.CropEventArgs args)
		{
			try
			{
				Lock();

				messageWindow.Show(this.MainWindow, "Please Wait...");
				this.illImageOperations.Crop(args);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
				UnLock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				ShowErrorMessage(this.MainWindow, "Can't crop image!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
				UnLock();
			}
		}*/
		#endregion

		#region SplitImage_Requested()
		/*void SplitImage_Requested(BscanILL.Hierarchy.IllImage image, int imageIndex)
		{
			try
			{
				Lock();

				messageWindow.Show(this.MainWindow, "Please Wait...");
				this.illImageOperations.SplitImage(image, imageIndex);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
				UnLock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				ShowErrorMessage(this.MainWindow, "Can't split image!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
				UnLock();
			}
		}*/
		#endregion

		#region Unsplit_Requested()
		/*void Unsplit_Requested(BscanILL.Hierarchy.IllImage image)
		{
			try
			{
				Lock();

				messageWindow.Show(this.MainWindow, "Please Wait...");
				this.illImageOperations.Unsplit(image);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
				UnLock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "FrameScan, Unsplit_Requested(): " + ex.Message, ex);
				ShowErrorMessage(this.MainWindow, "Can't split image!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
				UnLock();
			}
		}*/
		#endregion

		#region RotateImage_Requested()
		/*void RotateImage_Requested(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle)
		{
			try
			{
				Lock();

				messageWindow.Show(this.MainWindow, "Please Wait...");
				this.illImageOperations.RotateImage(image, imageIndex, angle);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
				UnLock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				ShowErrorMessage(this.MainWindow, "Can't rotate image!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
				UnLock();
			}
		}*/
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

		#region DeleteImage_Click()
		void DeleteImage_Click(BscanILL.Hierarchy.IllImage illImage)
		{
			if (this.Article != null && illImage != null)
            {
                if (illImage != this.Article.Pullslip)
                {
                    this.Article.Scans.Remove(illImage);
                }
                else
                {
                   //delete pull slip - at this moment it must be last image in article

                   this.frameScanUi.Remove_VpImage(illImage);
                   this.Article.Delete();   // deletes article's images from disk, flags article in database as deleted , does not clear 'scans' structure...

                   this.MainWindow.DeleteArticle(this.Article.DbArticle);   //delete article from batch list and sets 'deleted' flag for this article in database
                }
            }
		}
		#endregion

		#region DeleteAll_Request()
		void DeleteAll_Request(object sender, EventArgs e)
		{
			Lock();

			try
			{
				if ((this.Article != null) && (this.Article.Scans.Count <= 1 || BscanILL.UI.Dialogs.AlertDlg.Show(this.MainWindow, "Are you sure you want to delete all the images?", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Question)))
				{
					ShowDefaultImage();

					this.Article.Scans.Clear();
				}
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				ShowErrorMessage(ex.Message);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region FrameWpf_PrintClick()
		/*void FrameWpf_PrintClick(BscanILL.ExportSettings exportSettings)
		{
			try
			{
				Lock();
				Kic.Hierarchy.IllImages illImages = (exportSettings.Images == BscanILL.ImagesSelection.All) ? this.IllImages : this.SelectedImages;
				//ShowDefaultImage();

				if (illImages != null && illImages.Count > 0)
				{
					BscanILL.Export.PrintingOptions printingOptions;

					if (exportSettings.Images == BscanILL.ImagesSelection.All || illImages.Count > 1)
						printingOptions = Kic.Export.Printing.PrintOptions.OpenSimple(illImages, this.MainWindow);
					else
						printingOptions = Kic.Export.Printing.PrintOptions.OpenSimpleArea(illImages[0], this.MainWindow, this.imageWindow);

					if (printingOptions != null)
					{
						Kic.Export.ExportDlg.WorkUnit workUnit = new Kic.Export.ExportDlg.WorkUnit(illImages, exportSettings);
						workUnit.Tag = printingOptions;

						RunExport(workUnit);
					}
				}
				else
					ShowWarningMessage(this.MainWindow, BscanILL.Languages.BscanILLStrings.FrameScan_ScanAndSelectSomeImagesFirst_STR, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "FrameScan, FrameWpf_PrintClick: " + ex.Message, ex);
				ShowErrorMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				UnLock();
			}
		}*/
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

		#region DeleteImage()
		void DeleteImage(BscanILL.Hierarchy.IllImage illImage)
		{
			this.Article.Scans.Remove(illImage);
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
		}
		#endregion

		#region Collate_Request()
		void Collate_Request(object sender, EventArgs e)
		{
			/*Lock();

			try
			{
				Kic.Dialogs.CollateDlg dlg = new Kic.Dialogs.CollateDlg(this.imageWindow);

				dlg.Open(this.MainWindow, this.session, this.frameScanUi.SelectedImage);
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "FrameScan, Collate_Request(): " + ex.Message, ex);
				ShowErrorMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				UnLock();
			}*/
		}
		#endregion

		#region PrimaryScanner_Changed()
		void PrimaryScanner_Changed()
		{
			if (this.frameScanScanner != null)
				this.frameScanScanner.Dispose();

			this.frameScanScanner = null;
			
			Scanners.IScanner scanner = sm.PrimaryScanner;

			if (scanner is Scanners.Click.ClickWrapper)
			{
				this.frameScanScanner = new FrameScanClick(this, (Scanners.Click.ClickWrapper)scanner, this.IsActivated);
				((FrameScanClick)this.frameScanScanner).ScanRequest += Scanner_ScanRequest;
			}
			else if (scanner is Scanners.Click.ClickMiniWrapper)
			{
				this.frameScanScanner = new FrameScanClickMini(this, (Scanners.Click.ClickMiniWrapper)scanner, this.IsActivated);
				((FrameScanClickMini)this.frameScanScanner).ScanRequest += Scanner_ScanRequest;
			}
			else if (scanner is Scanners.Twain.TwainScanner)
				this.frameScanScanner = new FrameScanTwain(this, (Scanners.Twain.TwainScanner)scanner, this.IsActivated);
			else if (scanner is Scanners.S2N.Bookeye4Wrapper)
				this.frameScanScanner = new FrameScanS2N(this, (Scanners.S2N.Bookeye4Wrapper)scanner, this.IsActivated);
			else if (scanner is Scanners.S2N.ScannerS2NWrapper)
				this.frameScanScanner = new FrameScanS2N(this, (Scanners.S2N.ScannerS2NWrapper)scanner, this.IsActivated);            
            else if (scanner is Scanners.Twain.AdfScanner)
                this.frameScanScanner = new FrameScanAdf(this, (Scanners.Twain.AdfScanner)scanner, this.IsActivated);
		}
		#endregion

		#region SecondaryScanner_Changed()
		void SecondaryScanner_Changed()
		{
			/*if (this.frameScanScanner != null)
				this.frameScanScanner.Dispose();

			this.frameScanScanner = null;

			if (this.MainWindow.Scanner is Scanners.Click.Click)
				this.frameScanScanner = new FrameScanClick(this, (Scanners.Click.Click)this.MainWindow.Scanner, this.IsActivated);
			else if (this.MainWindow.Scanner is Scanners.Twain.TwainScanner)
				this.frameScanScanner = new FrameScanTwain(this, (Scanners.Twain.TwainScanner)this.MainWindow.Scanner, this.IsActivated);
			else if (this.MainWindow.Scanner is Scanners.S2N.Bookeye4Wrapper)
				this.frameScanScanner = new FrameScanBookeye4(this, (Scanners.S2N.Bookeye4Wrapper)this.MainWindow.Scanner, this.IsActivated);
			else if (this.MainWindow.Scanner is Scanners.S2N.ScannerS2NWrapper)
				this.frameScanScanner = new FrameScanBE2BE3(this, (Scanners.S2N.ScannerS2NWrapper)this.MainWindow.Scanner, this.IsActivated);

			this.frameScanUi.ScannerChanged(this.frameScanScanner != null, this.MainWindow.Scanner);*/
		}
		#endregion

		#region GetScanFileFormat()
		private static Scanners.FileFormat GetScanFileFormat(string file)
		{
			using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(file))
			{
				if (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite)
					return Scanners.FileFormat.Png;
				else
					return Scanners.FileFormat.Jpeg;
			}
		}
		#endregion

		#region Scanner_ImageScannedTU()
		void Scanner_ImageScannedTU(Bitmap bitmap)
		{
			try
			{
				if (this.MainWindow.SecondWindow != null)
					this.MainWindow.SecondWindow.ShowImage(bitmap);
				
				if (scanAction == ScanAction.InsertBefore && this.insertBeforeImage != null && this.Article.Scans.IndexOf(this.insertBeforeImage) >= 0)
					InsertImage(this.Article.Scans.IndexOf(this.insertBeforeImage), bitmap);
				else if (scanAction == ScanAction.Rescan && this.insertBeforeImage != null && this.Article.Scans.IndexOf(this.insertBeforeImage) >= 0)
				{
					this.InsertImage(this.Article.Scans.IndexOf(this.insertBeforeImage), bitmap);
					DeleteImage(this.insertBeforeImage);
				}
				else
					AddImage(bitmap);
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

		void Scanner_ImageScannedTU(TwainApp.TwainImage twainImage, bool moreImagesToTransfer)
		{
			lock (this.addImageLocker)
			{				
				try
				{
					if (this.MainWindow.SecondWindow != null && moreImagesToTransfer == false)
						this.MainWindow.SecondWindow.ShowImage(twainImage.Bitmap);

					if (scanAction == ScanAction.InsertBefore && this.insertBeforeImage != null && this.Article.Scans.IndexOf(this.insertBeforeImage) >= 0)
						this.InsertImage(this.Article.Scans.IndexOf(this.insertBeforeImage), twainImage.Bitmap);
					else if (scanAction == ScanAction.Rescan && this.insertBeforeImage != null && this.Article.Scans.IndexOf(this.insertBeforeImage) >= 0)
					{
						this.InsertImage(this.Article.Scans.IndexOf(this.insertBeforeImage), twainImage.Bitmap);
						DeleteImage(this.insertBeforeImage);
					}
					else
						this.AddImage(twainImage.Bitmap);

					if (this.scanSettings.BookEdge.ScanPage.Value == Scanners.Twain.BookedgePage.LeftPage)
						this.scanSettings.BookEdge.ScanPage.Value = Scanners.Twain.BookedgePage.RightPage;
					else if (this.scanSettings.BookEdge.ScanPage.Value == Scanners.Twain.BookedgePage.RightPage)
						this.scanSettings.BookEdge.ScanPage.Value = Scanners.Twain.BookedgePage.LeftPage;
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
					try
					{
						twainImage.Dispose();
					}
					catch { }

					if (moreImagesToTransfer == false)
						UnLock();
				}
			}
		}

		void Scanner_ImageScannedTU(string file)
		{
			try
			{
				if (this.MainWindow.SecondWindow != null)
					this.MainWindow.SecondWindow.ShowImage(file);

				Scanners.ColorMode colorMode;
				Scanners.FileFormat fileFormat;

				using (ImageProcessing.BigImages.ItDecoder decoder = new ImageProcessing.BigImages.ItDecoder(file))
				{
					switch (decoder.PixelsFormat)
					{
						case ImageProcessing.PixelsFormat.FormatBlackWhite: 
							colorMode = Scanners.ColorMode.Bitonal; break;
						case ImageProcessing.PixelsFormat.Format8bppGray:
						case ImageProcessing.PixelsFormat.Format8bppIndexed:
							colorMode = Scanners.ColorMode.Grayscale; break;
						default: 
							colorMode = Scanners.ColorMode.Color; break;
					}

					fileFormat = Scanners.Misc.GetImageFormat(decoder.ImageInfo.CodecInfo.FormatID);
				}

				if (scanAction == ScanAction.InsertBefore && this.insertBeforeImage != null && this.Article.Scans.IndexOf(this.insertBeforeImage) >= 0)
					this.InsertImage(this.Article.Scans.IndexOf(this.insertBeforeImage), file, colorMode, fileFormat);
				else if (scanAction == ScanAction.Rescan && this.insertBeforeImage != null && this.Article.Scans.IndexOf(this.insertBeforeImage) >= 0)
				{
					this.InsertImage(this.Article.Scans.IndexOf(this.insertBeforeImage), file, colorMode, fileFormat);
					DeleteImage(this.insertBeforeImage);
				}
				else
					this.AddImage(file, colorMode, fileFormat, true);
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

		#region Scanner_ImagesScannedTU()
		void Scanner_ImagesScannedTU(Bitmap bitmapL, Bitmap bitmapR)
		{
			try
			{
				if (this.MainWindow.SecondWindow != null)
					this.MainWindow.SecondWindow.ShowImage(bitmapR);

				if (this.insertBeforeImage != null && this.Article.Scans.IndexOf(this.insertBeforeImage) >= 0)
				{
					int index = this.Article.Scans.IndexOf(this.insertBeforeImage);

					this.InsertImage(index, bitmapL);
					this.InsertImage(index + 1, bitmapR);
				}
				else
				{
					this.AddImage(bitmapL);
					this.AddImage(bitmapR);
				}
			}
			catch (IllException ex)
			{
				ShowWarningMessage(ex.Message);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("The scanning process was interrupted!");
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
			finally
			{
				try
				{
					bitmapL.Dispose();
					bitmapR.Dispose();
				}
				catch { }

				UnLock();
			}
		}
		#endregion

		#region Scanner_ImagesScannedTU()
		internal void Scanner_ImagesScannedTU(string pageL, string pageR)
		{
			try
			{
				if (this.MainWindow.SecondWindow != null)
					this.MainWindow.SecondWindow.ShowImage(pageR);

				Scanners.ColorMode colorMode;
				Scanners.FileFormat fileFormat;

				using (ImageProcessing.BigImages.ItDecoder decoder = new ImageProcessing.BigImages.ItDecoder(pageL))
				{
					switch (decoder.PixelsFormat)
					{
						case ImageProcessing.PixelsFormat.FormatBlackWhite: 
							colorMode = Scanners.ColorMode.Bitonal; break;
						case ImageProcessing.PixelsFormat.Format8bppGray:
						case ImageProcessing.PixelsFormat.Format8bppIndexed:
							colorMode = Scanners.ColorMode.Grayscale; break;
						default: 
							colorMode = Scanners.ColorMode.Color; break;
					}

					fileFormat = Scanners.Misc.GetImageFormat(decoder.ImageInfo.CodecInfo.FormatID);
				}

				if (this.insertBeforeImage != null && this.Article.Scans.IndexOf(this.insertBeforeImage) >= 0)
				{
					int index = this.Article.Scans.IndexOf(this.insertBeforeImage);

					this.InsertImage(index, pageL, colorMode, fileFormat);
					this.InsertImage(index + 1, pageR, colorMode, fileFormat);
				}
				else
				{
					AddImage(pageL, colorMode, fileFormat, false);  //renameFile param set to false as at this point image has already correct name, with S2N scanner in splitting mode we query proper name when cropping images in GetScanBoth()
					AddImage(pageR, colorMode, fileFormat, false);
				}
			}
			catch (IllException ex)
			{
				ShowWarningMessage(ex.Message);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("The scanning process was interrupted!");
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region Scanner_OperationSuccessdfullTU()
		void Scanner_OperationSuccessdfullTU()
		{
			UnLock();
			this.MainWindow.Activate();
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

					ShowWarningMessage("The scanning process was interrupted!");
					Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				}
			}
			finally
			{
				UnLock();
				this.MainWindow.Activate();
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
					this.LockProgressChanged(progress);
					this.LockDescriptionChanged(description);
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

		#region FrameScanS2N_ScanRequest()
		void FrameScanS2N_ScanRequest(Scanners.S2N.ScannerScanAreaSelection scanArea)
		{	
			this.frameScanUi.Dispatcher.Invoke((Action)delegate() 
			{ 
				this.insertBeforeImage = null;

				if (this.frameScanScanner is FrameScanS2N)
					((FrameScanS2N)this.frameScanScanner).ScanArea = scanArea;

				Scan(ScanAction.Scan); 
			});
		}
		#endregion

        #region FrameScFrameScanS2N_ScanPullSlpRequestanS2N_ScanRequest()
        void FrameScanS2N_ScanPullSlpRequest(Scanners.S2N.ScannerScanAreaSelection scanArea)
		{	
			this.frameScanUi.Dispatcher.Invoke((Action)delegate() 
			{ 			
				ScanPullSlip(); 
			});
		}
		#endregion

		#region GetScanPage()
		internal ImageProcessing.FileFormat.IImageFormat GetIImageFormat(string file)
		{
			Scanners.FileFormat fileFormat = GetScanFileFormat(file);

			switch (fileFormat)
			{
				case Scanners.FileFormat.Tiff: return (ImageProcessing.FileFormat.IImageFormat)new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4); 
				case Scanners.FileFormat.Png: return (ImageProcessing.FileFormat.IImageFormat)new ImageProcessing.FileFormat.Png(); 
				default: return (ImageProcessing.FileFormat.IImageFormat)new ImageProcessing.FileFormat.Jpeg(85); 
			}
		}
		#endregion

		#region S2N_OpenAdditionalSettings()
		void S2N_OpenAdditionalSettings()
		{
			try
			{
				Lock();

				if (this.sm.PrimaryScanner != null && (this.sm.PrimaryScanner is Scanners.S2N.ScannerS2NWrapper || this.sm.PrimaryScanner is Scanners.S2N.Bookeye4Wrapper))
				{
                    BscanILL.UI.Dialogs.Scanning.S2NAdditionalSettingsDlg dlg = new BscanILL.UI.Dialogs.Scanning.S2NAdditionalSettingsDlg();

					dlg.ShowDialog();

                    if(this.sm.PrimaryScanner is Scanners.S2N.ScannerS2NWrapper)
                    {
                       //BE3 and BE5 we want to send new settings after more settings dialog is closed so in case we use foot pedal or hardware scan button, the new settings are applied for foot pedal scan
                       //we do not need to do it for BE4 as it does not use Wait mode for foot pedal scanning and in direct mode we update scanner settings before scanning
                       ScannerSettingChanged();
                    }

				}
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

		#region Click_OpenAdditionalSettings()
		void Click_OpenAdditionalSettings()
		{
			/*try
			{
				Lock();

				if (this.sm.PrimaryScanner != null && (this.sm.PrimaryScanner is Scanners.S2N.ScannerS2NWrapper || this.sm.PrimaryScanner is Scanners.S2N.Bookeye4Wrapper))
				{
					BscanILL.UI.Dialogs.Scanning.S2NAdditionalSettingsDlg dlg = new BscanILL.UI.Dialogs.Scanning.S2NAdditionalSettingsDlg();

					dlg.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}*/
		}
		#endregion

		#region ClickMini_OpenAdditionalSettings()
		void ClickMini_OpenAdditionalSettings()
		{
			/*try
			{
				Lock();

				if (this.sm.PrimaryScanner != null && (this.sm.PrimaryScanner is Scanners.S2N.ScannerS2NWrapper || this.sm.PrimaryScanner is Scanners.S2N.Bookeye4Wrapper))
				{
					BscanILL.UI.Dialogs.Scanning.S2NAdditionalSettingsDlg dlg = new BscanILL.UI.Dialogs.Scanning.S2NAdditionalSettingsDlg();

					dlg.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}*/
		}
		#endregion

		#region Twain_OpenAdditionalSettings()
		void Twain_OpenAdditionalSettings()
		{
			try
			{
				Lock();

				if (this.sm.PrimaryScanner != null && this.sm.PrimaryScanner is Scanners.Twain.TwainScanner)
					((Scanners.Twain.TwainScanner)this.sm.PrimaryScanner).OpenSetupWindow();
				if (this.sm.SecondaryScanner != null && this.sm.SecondaryScanner is Scanners.Twain.TwainScanner)
					((Scanners.Twain.TwainScanner)this.sm.SecondaryScanner).OpenSetupWindow();
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
			Scanner_ImageScanned(twainImage, moreImagesToTransfer);
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

		#region Scanner_ScanRequest()
		void Scanner_ScanRequest()
		{
			Scan(ScanAction.Scan);
		}
		#endregion

		#endregion

	}
}
