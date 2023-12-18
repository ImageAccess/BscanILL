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
using ViewPane.Hierarchy;
using BscanILL.Hierarchy;
using ImageProcessing.IpSettings;

namespace BscanILL.UI.Frames.Edit
{
	/// <summary>
	/// Interaction logic for FrameScanUi.xaml
	/// </summary>
	public partial class FrameEditUi : UserControl
	{
		BscanILL.Hierarchy.Article			article = null;
        BscanILL.Hierarchy.SessionBatch     batch = null;

		BscanILL.UI.Windows.ItResultsWindow itResultsWindow = null;

		LastItImageSettingsList			lastItImageSettingsList = new LastItImageSettingsList();

		public delegate void RunAutoHnd(ItSelection itAutoSelection, ItApplyTo applyTo);
		public delegate void RunManualHnd(ItSelection itManualSelection, ItApplyTo applyTo, ItApplyToPages applyToPages);

		public event BscanILL.Misc.VoidHnd GoToStartClick;
		public event BscanILL.Misc.VoidHnd GoToScanClick;
		public event BscanILL.Misc.VoidHnd GoToExportClick;        

		public event BscanILL.Misc.VoidHnd ResendClick;
		public event BscanILL.Misc.VoidHnd HelpClick;
        public event BscanILL.IP.IllImageOperations.RotateImageHnd RotateSmallAngle_Click;		
		public event BscanILL.IP.IllImageOperations.RotateImageHnd Rotate90_Click;		

		public event BscanILL.Misc.VoidHnd CreatePageDerivFilesEvent;


		#region constructor
		public FrameEditUi()
		{
			InitializeComponent();

			this.viewPanel.Licensing.Ip = ViewPane.ImageProcessingMode.Advanced;
			this.viewPanel.ImageSelected += new ViewPane.ImageSelectedHnd(ViewPanel_ImageSelected);
			//this.viewPanel.Licensing.PostProcessing = ViewPane.PostProcessingMode.Advanced;

			this.itResultsWindow = new BscanILL.UI.Windows.ItResultsWindow();

			this.controlPanel.RunAutoItClick += new RunAutoHnd(ControlPanel_AutoItClick);
			this.controlPanel.ApplyManualItClick += new RunManualHnd(ControlPanel_ApplyManualItClick);
			this.controlPanel.DoneClick += new BscanILL.Misc.VoidHnd(ControlPanel_DoneClick);
			this.controlPanel.CurrentOnlyClick += new BscanILL.Misc.VoidHnd(ControlPanel_CurrentOnlyClick);
			this.controlPanel.ItSettingsClick += new BscanILL.Misc.VoidHnd(ControlPanel_ItSettingsClick);
			this.controlPanel.ChangeDependencyClick += new BscanILL.Misc.VoidHnd(ControlPanel_ChangeDependencyClick);
			this.controlPanel.ResetCurrentClick += new BscanILL.Misc.VoidHnd(ControlPanel_ResetCurrentClick);
			this.controlPanel.ResetAllClick += new BscanILL.Misc.VoidHnd(ControlPanel_ResetAllClick);
			this.controlPanel.HelpClick += new BscanILL.Misc.VoidHnd(ControlPanel_HelpClick);
			this.controlPanel.SkipItClick += new BscanILL.Misc.VoidHnd(ControlPanel_SkipItClick);
			this.controlPanel.ShowResultsClick += new BscanILL.Misc.VoidHnd(ControlPanel_ShowResultsClick);
            this.controlPanel.UndoImageChangeClick += new BscanILL.Misc.VoidHnd(ControlPanel_UndoImageChangeClick);
            this.controlPanel.RotateCCVClick += new BscanILL.Misc.VoidHnd(ControlPanel_RotateCCVClick);
			this.controlPanel.RotateCVClick += new BscanILL.Misc.VoidHnd(ControlPanel_RotateCVClick);
			this.controlPanel.Rotate90CVClick += new BscanILL.Misc.VoidHnd(ControlPanel_Rotate90CVClick);
			this.controlPanel.Rotate90CCVClick += new BscanILL.Misc.VoidHnd(ControlPanel_Rotate90CCVClick);
		}
		#endregion


		#region enum ItApplyTo
		public enum ItApplyTo
		{
			Current,            
			EntireArticle, //former All
			RestOfArticle,
            EntireBatch,
            RestOfBatch
		}
		#endregion

		#region enum ItApplyToPages
		[Flags]
		public enum ItApplyToPages
		{
			Left	= 0x01,
			Right	= 0x02,
			All		= Left | Right
		}
		#endregion

		#region enum ItSelection
		[Flags]
		public enum ItSelection
		{
			None		= 0x00,
			Content		= 0x01,
			Deskew		= 0x02,
			Bookfold	= 0x04,
			Fingers		= 0x08
		}
		#endregion


		#region class LastItImageSettings
		class LastItImageSettingsList : List<LastItImageSettings>
		{
			public LastItImageSettingsList()
			{
			}

			public LastItImageSettings GetLastItImageSettings(IllImage illImage)
			{
				foreach (LastItImageSettings item in this)
					if (item.IllImageId == illImage.DbScan.Id)
						return item;

				return null;
			}
		}
		#endregion

		#region class LastItImageSettings
		class LastItImageSettings
		{
			public readonly long IllImageId;
			public readonly ImageProcessing.IpSettings.ItImage ItImage;

			public LastItImageSettings(long illImageId, ImageProcessing.IpSettings.ItImage itImage)
			{
				this.IllImageId = illImageId;
				this.ItImage = itImage;
			}
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Hierarchy.IllImage	SelectedImage { get { return GetIllImage(viewPanel.SelectedImage); } }
		//public ViewPane.ViewPanel			ViewP { get { return this.viewPanel; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Open()
		public void Open(BscanILL.Hierarchy.SessionBatch batch)
		{
            LoadArticle(batch);
			this.Visibility = System.Windows.Visibility.Visible;
			this.autoItFloatingPanel.Visibility = System.Windows.Visibility.Visible;
            this.controlPanel.autoItControl.AdjustMultiAticleMode();
            this.controlPanel.manualItControl.AdjustMultiAticleMode();
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			this.lastItImageSettingsList.Clear();
			this.viewPanel.Dispose();
			this.itResultsWindow.Dispose();
		}
		#endregion

		#region Reset()
		public void Reset(bool rememberItImages)
		{
			if (rememberItImages)
			{
				this.lastItImageSettingsList.Clear();

				foreach (VpImage vpImage in this.viewPanel.Images)
				{
					if (vpImage.ItImage != null && vpImage.Tag is IllImage)
						this.lastItImageSettingsList.Add(new LastItImageSettings(((IllImage)vpImage.Tag).DbScan.Id, vpImage.ItImage));
				}
			}
			
			this.viewPanel.Clear();
			this.itResultsWindow.Hide();
		}
		#endregion

		#region ShowDefaultImage()
		public void ShowDefaultImage()
		{
			this.viewPanel.SelectImage(null);
		}
		#endregion

		#region ShowImage()
		public void ShowImage(BscanILL.Hierarchy.IllImage illImage)
		{
			VpImage vpImage = GetVpImage(illImage);

			if (vpImage != null)
				vpImage.Select();
		}

		public void ShowImage(VpImage vpImage)
		{
			this.viewPanel.SelectImage(vpImage);
		}
		#endregion

		#region ScannerChanged()
		public void ScannerChanged(bool scannerConnected)
		{
			//buttonScanPullslip.IsEnabled = scannerConnected;
		}
		#endregion

		#region Lock()
		public void Lock()
		{
			Window parentWindow = Window.GetWindow(this);

			if(parentWindow != null)
			{
				parentWindow.IsEnabled = false;
				parentWindow.Cursor = Cursors.Wait;
			}
		}
		#endregion

		#region UnLock()
		public void UnLock()
		{
			Window parentWindow = Window.GetWindow(this);

			if (parentWindow != null)
			{
				parentWindow.IsEnabled = true;
				parentWindow.Cursor = null;
			}
		}
		#endregion

		#region GetIllImage()
		public static BscanILL.Hierarchy.IllImage GetIllImage(VpImage vpImage)
		{
			if (vpImage != null && vpImage.Tag is BscanILL.Hierarchy.IllImage)
				return (BscanILL.Hierarchy.IllImage)vpImage.Tag;

			return null;
		}
		#endregion

		#region LookUpVpImage()
		public VpImage LookUpVpImage(BscanILL.Hierarchy.IllImage illImage)
		{
			return GetVpImage(illImage) ;
		}
		#endregion

		#region UpdateProgress
		public void UpdateProgress( double progress )
        {
            if (this.Dispatcher.CheckAccess())
            {
               SkipItExecutionProgressChanged(progress);
            }
            else
            {
                this.Dispatcher.Invoke((Action)delegate() { SkipItExecutionProgressChanged(progress); });
            }
        }
		#endregion

		#region ControlPanel_RotateCCVClick()
		public void ControlPanel_RotateCCVClick()
		{
			if (RotateSmallAngle_Click != null)
			{
				//RotateImage_Requested(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle)

				RotateSmallAngle_Click(SelectedImage, 0, 359);
			}
		}
		#endregion

		#region ControlPanel_RotateCVClick()
		public void ControlPanel_RotateCVClick()
		{
			if (RotateSmallAngle_Click != null)
			{
				//RotateImage_Requested(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle)

				RotateSmallAngle_Click(SelectedImage, 0, 1);
			}
		}
		#endregion

		#region ControlPanel_Rotate90CVClick()
		public void ControlPanel_Rotate90CVClick()
		{
			if (Rotate90_Click != null)
			{
				//RotateImage_Requested(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle)

				Rotate90_Click(SelectedImage, 0, 90);
			}
		}
		#endregion

		#region ControlPanel_Rotate90CCVClick()
		public void ControlPanel_Rotate90CCVClick()
		{
			if (Rotate90_Click != null)
			{
				//RotateImage_Requested(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle)

				Rotate90_Click(SelectedImage, 0, 270);
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region GoToPrev_Click()
		private void GoToPrev_Click(object sender, EventArgs e)
		{
			if (GoToScanClick != null)
				GoToScanClick();
		}
		#endregion

		#region GoToNext_Click()
		private void GoToNext_Click(object sender, EventArgs e)
		{
			if (GoToExportClick != null)
				GoToExportClick();
		}
		#endregion

		#region ControlPanel_ApplyManualItClick()
		void ControlPanel_ApplyManualItClick(FrameEditUi.ItSelection itManualSelection, FrameEditUi.ItApplyTo applyTo, FrameEditUi.ItApplyToPages applyToPages)
		{
			ApplyManualIt(itManualSelection, applyTo, applyToPages);
		}
		#endregion

		#region ApplyManualIt()
		void ApplyManualIt(FrameEditUi.ItSelection itManualSelection, FrameEditUi.ItApplyTo applyTo, FrameEditUi.ItApplyToPages applyToPages)
		{
			if (controlPanel.ItManualSelected)
			{
				ViewPane.Hierarchy.VpImage selectedImage = viewPanel.SelectedImage;
				viewPanel.SelectImage(null);

				this.Lock();
				this.viewPanel.LockWithProgress();

				ManualItExecution manualItExecution = new ManualItExecution();

				manualItExecution.ExecutionSuccessfull += delegate(VpImage selected)
				{
					this.Dispatcher.Invoke((Action)delegate() { Ip_Successfull(selected); });
				};
				manualItExecution.ExecutionError += delegate(VpImage selected, Exception ex)
				{
					this.Dispatcher.Invoke((Action)delegate() { Ip_Exception(selected, ex); });
				};
				manualItExecution.ProgressChanged += delegate(double progress)
				{
					this.Dispatcher.Invoke((Action)delegate() { ItExecutionProgressChanged(this.article, progress); });
				};

				manualItExecution.RunManualIt(this.viewPanel.Images, selectedImage, itManualSelection, applyTo, applyToPages);
			}
		}
		#endregion

		#region ControlPanel_AutoItClick()
		void ControlPanel_AutoItClick(FrameEditUi.ItSelection itAutoSelection, FrameEditUi.ItApplyTo applyTo)
		{
			RunAutoIt(itAutoSelection, applyTo);
		}
		#endregion

		#region RunAutoIt()
		void RunAutoIt(FrameEditUi.ItSelection itAutoSelection, FrameEditUi.ItApplyTo applyTo)
		{
			try
			{
				if (controlPanel.ItAutoSelected)
				{
					ViewPane.Hierarchy.VpImage selectedImage = viewPanel.SelectedImage;
					viewPanel.SelectImage(null);

					this.Lock();
					this.viewPanel.LockWithProgress();

					AutoItExecution autoItExecution = new AutoItExecution();

					autoItExecution.ExecutionSuccessfull += delegate(VpImage selected)
					{
						this.Dispatcher.Invoke((Action)delegate() { Ip_Successfull(selected); });
					};
					autoItExecution.ExecutionError += delegate(VpImage selected, Exception ex)
					{
						this.Dispatcher.Invoke((Action)delegate() { Ip_Exception(selected, ex); });
					};
					autoItExecution.ProgressChanged += delegate(double progress)
					{
						this.Dispatcher.Invoke((Action)delegate() { ItExecutionProgressChanged(this.article, progress); });
					};

					autoItExecution.RunAutoIt(this.viewPanel.Images, selectedImage, itAutoSelection, applyTo);
				}
			}
			catch (Exception ex)
			{
				this.Dispatcher.Invoke((Action)delegate() { Ip_Exception(null, ex); });
			}
		}
		#endregion

		#region Ip_Successfull()
		private void Ip_Successfull(VpImage selectedImage)
		{
			this.UnLock();
			this.viewPanel.UnLock();

			/*controlPanel.ClSelection = ItSelection.None;
			controlPanel.SkewSelection = ItSelection.None;
			controlPanel.BfcSelection = ItSelection.None;
			controlPanel.FrSelection = ItSelection.None;*/

			if (selectedImage != null)
				ShowImage(selectedImage);
			else if (this.viewPanel.Images.Count > 1)
				ShowImage(this.viewPanel.Images[1]);
		}
		#endregion

		#region Ip_Exception()
		private void Ip_Exception(VpImage selectedImage, Exception ex)
		{
			//MessageBox.Show("Can't apply manual settings! Selected image is pullslip.", "", MessageBoxButton.OK, MessageBoxImage.Error);
			MessageBox.Show("Can't apply manual settings! " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);

			this.UnLock();
			this.viewPanel.UnLock();

			if (selectedImage != null)
				ShowImage(selectedImage);
			else if (this.viewPanel.Images.Count > 1)
				ShowImage(this.viewPanel.Images[1]);
		}
		#endregion

		#region ControlPanel_DoneClick()
		void ControlPanel_DoneClick()
		{
			try
			{
				Lock();
				this.viewPanel.LockWithProgress();

				Thread thread = new Thread(new ParameterizedThreadStart(ExecuteArticleThread));
				thread.Name = "ExecuteArticleThread";
				thread.SetApartmentState(ApartmentState.STA);
				thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				//thread.Start(new object[]{this.article, this.viewPanel.Images});
                thread.Start(new object[] { this.article, this.batch , this.viewPanel.Images });
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
				
				this.viewPanel.UnLock();
				UnLock();
			}
			finally
			{
			}
		}
		#endregion

		#region ControlPanel_CurrentOnlyClick()
		void ControlPanel_CurrentOnlyClick()
		{
			try
			{
				VpImage vpImage = GetVpImage(this.SelectedImage);

				if (this.article != null && this.SelectedImage != null && this.SelectedImage.IsPullslip == false && vpImage != null)
				{
					Lock();
					this.viewPanel.LockWithProgress();

					Thread thread = new Thread(new ParameterizedThreadStart(ExecuteIllImageThread));
					thread.Name = "ExecuteCurrentImageThread";
					thread.SetApartmentState(ApartmentState.STA);
					thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
					thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
					//thread.Start(new object[] { this.article, this.SelectedImage, vpImage });
                    thread.Start(new object[] { this.SelectedImage.Article , this.SelectedImage, vpImage });
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);

				this.viewPanel.UnLock();
				UnLock();
			}
			finally
			{
			}
		}
		#endregion

		#region ControlPanel_SkipItClick()
		void ControlPanel_SkipItClick()
		{
			try
			{
				Lock();
				this.viewPanel.LockWithProgress();

				Thread thread = new Thread(new ParameterizedThreadStart(SkipItExecution));
				thread.Name = "SkipArticleThread";
				thread.SetApartmentState(ApartmentState.STA);
				thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				//thread.Start(this.article);
                thread.Start(this.batch);                
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);

				this.viewPanel.UnLock();
				UnLock();
			}
			finally
			{
			}
		}
		#endregion

		#region ExecuteArticleThread()
		void ExecuteArticleThread(object obj)
		{
			try
			{
				object[]		objArray = (object[]) obj;
				Article			article = (Article)objArray[0];
				//VpImages		vpImages = (VpImages)objArray[1];
                SessionBatch batch = (SessionBatch)objArray[1];
                VpImages vpImages = (VpImages)objArray[2];
				ItExecution		itExecution = new ItExecution();

				//article.DeletePages(false);
                foreach( Article article_item in batch.Articles)
                {
                    article_item.DeletePages(false);
                }

				itExecution.OperationDone += delegate(Article a, List<BscanILL.UI.Frames.Edit.ItExecution.ItExecutionPage> pairs)
				{
					this.Dispatcher.Invoke((Action)delegate(){ItExecutionSuccessfull(a, pairs);});
				};

				itExecution.OperationError += delegate(Article a, Exception ex)
				{
					this.Dispatcher.Invoke((Action)delegate(){ItExecutionError(a, ex);});
				};
				
				itExecution.ProgressChanged += delegate(Article a, double progress)
				{
					this.Dispatcher.Invoke((Action)delegate(){ItExecutionProgressChanged(a, progress);});
				};

                itExecution.CreateExportImageDeriv += new BscanILL.Misc.VoidHnd(CreatePageFilesInExport);

				//System.IO.DirectoryInfo exportDir = new System.IO.DirectoryInfo(article.PagesPath);

				//exportDir.Create();

				this.itResultsWindow.Hide();
				//itExecution.Execute(article, vpImages, exportDir);
                itExecution.Execute(article, batch, vpImages);
			}
			catch (Exception ex)
			{
				this.Dispatcher.Invoke((Action)delegate() { ItExecutionError(article, ex); });
			}
		}
		#endregion

		#region ExecuteIllImageThread()
		void ExecuteIllImageThread(object obj)
		{
			try
			{
				object[] objArray = (object[])obj;
				Article article = (Article)objArray[0];
				IllImage illImage = (IllImage)objArray[1];
				VpImages vpImages = new VpImages() { (VpImage)objArray[2] };
				ItExecution itExecution = new ItExecution();

				illImage.DeletePages(false);

				itExecution.OperationDone += delegate(Article a, List<BscanILL.UI.Frames.Edit.ItExecution.ItExecutionPage> pairs)
				{
					this.Dispatcher.Invoke((Action)delegate() { ItExecutionSuccessfull(a, pairs); });
				};

				itExecution.OperationError += delegate(Article a, Exception ex)
				{
					this.Dispatcher.Invoke((Action)delegate() { ItExecutionError(a, ex); });
				};

				itExecution.ProgressChanged += delegate(Article a, double progress)
				{
					this.Dispatcher.Invoke((Action)delegate() { ItExecutionProgressChanged(a, progress); });
				};

                itExecution.CreateExportImageDeriv += new BscanILL.Misc.VoidHnd(CreatePageFilesInExport);

				System.IO.DirectoryInfo exportDir = new System.IO.DirectoryInfo(article.PagesPath);

				exportDir.Create();

				this.itResultsWindow.Hide();
				itExecution.Execute(article, vpImages, exportDir);                
			}
			catch (Exception ex)
			{
				this.Dispatcher.Invoke((Action)delegate() { ItExecutionError(article, ex); });
			}
		}
		#endregion

		#region ItExecutionSuccessfull()
		void ItExecutionSuccessfull(Article article, List<BscanILL.UI.Frames.Edit.ItExecution.ItExecutionPage> pairs)
		{
			/*foreach (IllImage illImage in article.Scans)
				illImage.DeletePages();*/
/*			
			foreach (BscanILL.UI.Frames.Edit.ItExecution.ItExecutionPage pair in pairs)
			{
				IllImage illImage = GetIllImage(pair.VpImage);

				if (illImage != null)
				{
					illImage.IllPages.Add(illImage, pair.File, pair.FileFormat, pair.ColorMode, pair.Dpi, pair.Brightness, pair.Contrast);
				}
			}
*/

            this.viewPanel.UnLock();            //this stops animation and hides progress bar in CleanUp Tab (used for example when Skip Image treatment ..)
			UnLock();

			if (GoToExportClick != null)
				GoToExportClick();
		}
		#endregion

		#region ItExecutionError()
		void ItExecutionError(Article article, Exception ex)
		{
			MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);

			this.viewPanel.UnLock();
			UnLock();
		}
		#endregion

		#region ItExecutionProgressChanged()
		void ItExecutionProgressChanged(Article article, double progress)
		{
			this.viewPanel.ProgressChanged(progress);
		}
		#endregion

		#region SkipItExecution()
		void SkipItExecution(object obj)
		{
			try
			{
				//Article article = (Article)obj;
                SessionBatch batch = (SessionBatch)obj;
                Article article = batch.Articles[0];

				SkipItExecution itExecution = new SkipItExecution();

				//article.DeletePages(false);
                foreach (Article article_item in batch.Articles)
                {
                    article_item.DeletePages(false);
                }

				itExecution.OperationDone += delegate(Article a, List<BscanILL.UI.Frames.Edit.SkipItExecution.SkipItExecutionPage> pairs)
				{
					this.Dispatcher.Invoke((Action)delegate() { SkipItExecutionSuccessfull(a, pairs); });
				};

				itExecution.OperationError += delegate(Article a, Exception ex)
				{
					this.Dispatcher.Invoke((Action)delegate() { SkipItExecutionError(a, ex); });
				};

				itExecution.ProgressChanged += delegate(Article a, double progress)
				{
					this.Dispatcher.Invoke((Action)delegate() { SkipItExecutionProgressChanged(a, progress); });
				};

                itExecution.CreateExportImageDeriv += new BscanILL.Misc.VoidHnd(CreatePageFilesInExport);


				//System.IO.DirectoryInfo exportDir = new System.IO.DirectoryInfo(article.PagesPath);

				//exportDir.Create();

				this.itResultsWindow.Hide();				
                itExecution.Execute(article, batch) ;
			}
			catch (Exception ex)
			{
				this.Dispatcher.Invoke((Action)delegate() { SkipItExecutionError(article, ex); });
			}
		}
		#endregion

        #region CreatePageFilesInExport()
        public void CreatePageFilesInExport()  
        {
            if (CreatePageDerivFilesEvent != null)
            {
                CreatePageDerivFilesEvent();
            }
        }
        #endregion


		#region SkipItExecutionSuccessfull()
		void SkipItExecutionSuccessfull(Article article, List<BscanILL.UI.Frames.Edit.SkipItExecution.SkipItExecutionPage> pairs)
		{
			/*foreach (IllImage illImage in article.Scans)
				illImage.DeletePages();*/

////			foreach (BscanILL.UI.Frames.Edit.SkipItExecution.SkipItExecutionPage pair in pairs)
////			{
////				if (pair.IllImage != null)
////					pair.IllImage.IllPages.Add(pair.IllImage, pair.File, pair.FileFormat, pair.ColorMode, pair.Dpi, pair.Brightness, pair.Contrast);
////			}

    		    this.viewPanel.UnLock();        //this stops animation and hides progress bar in CleanUp Tab (used for example when Skip Image treatment ..)
       		    UnLock();

               if (GoToExportClick != null)
  				  GoToExportClick();
		}
		#endregion

		#region SkipItExecutionError()
		void SkipItExecutionError(Article article, Exception ex)
		{
			MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);

			this.viewPanel.UnLock();
			UnLock();
		}
		#endregion

		#region SkipItExecutionProgressChanged()
		void SkipItExecutionProgressChanged(Article article, double progress)
		{
			this.viewPanel.ProgressChanged(progress);
		}

        void SkipItExecutionProgressChanged( double progress)
        {
            this.viewPanel.ProgressChanged(progress);
        }
		#endregion
		
		#region LoadArticle()
		void LoadArticle(BscanILL.Hierarchy.SessionBatch batch)
		{
           // if (currentArticle != null)
            if ( batch.Articles.Count > 0 )
			{
                this.article = batch.CurrentArticle;
                this.batch = batch;

				//this.articleControl.LoadArticle(article);
				ViewPane.Hierarchy.VpImage firstEditable = null;

                foreach (BscanILL.Hierarchy.Article article_item in batch.Articles)
                {
                    if (article_item.Pullslip != null)
                    {
                        VpImage vpImage = CreateVpImage(article_item.Pullslip, false);

                        this.viewPanel.AddImage(vpImage);                                                
                    }

                    foreach (BscanILL.Hierarchy.IllImage illImage in article_item.Scans)
                    {
                        ViewPane.Hierarchy.VpImage vpImage = CreateVpImage(illImage, true);

                        if (firstEditable == null)
                            firstEditable = vpImage;

                        LastItImageSettings lastSettings = lastItImageSettingsList.GetLastItImageSettings(illImage);

                        if (lastSettings == null)
                        {
                            vpImage.IsFixed = false;
                            vpImage.IsIndependent = true;
                        }
                        else
                        {
                            vpImage.ItImage = lastSettings.ItImage;
                        }

                        this.viewPanel.AddImage(vpImage);
                    }
                }

				if(BscanILL.SETTINGS.Settings.Instance.ImageTreatment.AutoImageTreatment.ImageDependency.SetDependencyAutomatically)
                {
					ItImages itImages = this.viewPanel.ItImages;

					if (itImages != null && itImages.Count > 0)
					{
						// set all images as dependand						
						SetAsDependent(itImages, itImages, false);						
					}
				}

				if (firstEditable != null)
					firstEditable.Select();
			}
			else
			{
				Reset(false);
			}
		}
		#endregion

		#region GoToStart_Click()
		private void GoToStart_Click(object sender, RoutedEventArgs e)
		{
			if (GoToStartClick != null)
				GoToStartClick();
		}
		#endregion

		#region GoToScan_Click()
		private void GoToScan_Click(object sender, RoutedEventArgs e)
		{
			if (GoToScanClick != null)
				GoToScanClick();
		}
		#endregion

		#region GoToResend_Click
		private void GoToResend_Click(object sender, RoutedEventArgs e)
		{
			if (ResendClick != null)
				ResendClick();
		}
		#endregion

		#region Help_Click
		private void Help_Click(object sender, RoutedEventArgs e)
		{
			if (HelpClick != null)
				HelpClick();
		}
		#endregion

		#region GetVpImage()
		private VpImage GetVpImage(BscanILL.Hierarchy.IllImage illImage)
		{
			foreach (VpImage vpImage in this.viewPanel.Images)
				if (vpImage.Tag == illImage)
					return vpImage;

			return null;
		}
		#endregion

		#region CreateVpImage()
		private VpImage CreateVpImage(BscanILL.Hierarchy.IllImage illImage, bool itActive)
		{
			ViewPane.Hierarchy.VpImage vpImage = new ViewPane.Hierarchy.VpImage(illImage.FilePath.FullName, illImage.FilePath.FullName + ".reduced",
				illImage.FilePath.FullName + ".preview", illImage.FilePath.FullName + ".thumbnail", itActive, false);

			vpImage.Tag = illImage;
			return vpImage;
		}
		#endregion

		#region FloatingControl_GoClick()
		private void FloatingControl_GoClick()
		{
			autoItFloatingPanel.Visibility = System.Windows.Visibility.Hidden;

            if (autoItFloatingPanel.ItAutoSelected)
                RunAutoIt(autoItFloatingPanel.ItSelection, (BscanILL.SETTINGS.Settings.Instance.General.MultiArticleSupportEnabled == false) ? FrameEditUi.ItApplyTo.EntireArticle : FrameEditUi.ItApplyTo.EntireBatch);                
		}
		#endregion

		#region FloatingControl_CancelClick()
		private void FloatingControl_CancelClick()
		{
			autoItFloatingPanel.Visibility = System.Windows.Visibility.Hidden;
		}
		#endregion

		#region ControlPanel_ItSettingsClick()
		void ControlPanel_ItSettingsClick()
		{
			try
			{
				Lock();
				BscanILL.UI.ItSettings.ItSettingsDlg dlg = new ItSettings.ItSettingsDlg();
				dlg.ShowDialog();
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region ControlPanel_UndoImageChangeClick()
		void ControlPanel_UndoImageChangeClick()
        {
            try
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region ControlPanel_ChangeDependencyClick()
        void ControlPanel_ChangeDependencyClick()
		{
			try
			{
				VpImage selectedVpImage = viewPanel.SelectedImage;				

				if (selectedVpImage != null && selectedVpImage.ItImage != null && selectedVpImage.IsFixed == false)
				{
					Lock();

					BscanILL.UI.Dialogs.DependencyDlg dlg = new BscanILL.UI.Dialogs.DependencyDlg();
					BscanILL.UI.Dialogs.DependencyDlg.Result result = dlg.Open();

					if (result == BscanILL.UI.Dialogs.DependencyDlg.Result.Dependent || result == BscanILL.UI.Dialogs.DependencyDlg.Result.Independent)
					{
						ChangeDependency(dlg.ResultParameters, selectedVpImage);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region ControlPanel_HelpClick()
		void ControlPanel_HelpClick()
		{
			if (HelpClick != null)
				HelpClick();
		}
        #endregion

        #region SetAsDependent()
		private void SetAsDependent(ItImages itImages, ItImages itImagesList, bool showMessage)
        {			
			BIP.Geometry.InchSize? clipSize = itImages.GetDependantClipsSize();

			if (clipSize == null)
			{
				double maxWidth = 0;
				double maxHeight = 0;

				foreach (ItImage itImage in itImagesList)
				{
					if (itImage.TwoPages)
					{
						if (maxWidth < itImage.PageL.ClipRectInch.Width)
							maxWidth = itImage.PageL.ClipRectInch.Width;
						if (maxHeight < itImage.PageL.ClipRectInch.Height)
							maxHeight = itImage.PageL.ClipRectInch.Height;
						if (maxWidth < itImage.PageR.ClipRectInch.Width)
							maxWidth = itImage.PageR.ClipRectInch.Width;
						if (maxHeight < itImage.PageR.ClipRectInch.Height)
							maxHeight = itImage.PageR.ClipRectInch.Height;
					}
					else
					{
						if (itImage.PageL.ClipSpecified)
						{
							if (maxWidth < itImage.PageL.ClipRectInch.Width)
								maxWidth = itImage.PageL.ClipRectInch.Width;
							if (maxHeight < itImage.PageL.ClipRectInch.Height)
								maxHeight = itImage.PageL.ClipRectInch.Height;
						}
					}
				}

				if (maxWidth > 0 && maxHeight > 0)
					clipSize = new BIP.Geometry.InchSize(maxWidth, maxHeight);
			}

			if (clipSize != null)
			{
				float dependencyHorizontalSizeMargin = (float)BscanILL.SETTINGS.Settings.Instance.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyHorizontalToleranceInches;
				float dependencyVericalSizeMargin = (float)BscanILL.SETTINGS.Settings.Instance.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyVerticalToleranceInches;

				foreach (ItImage itImage in itImagesList)
					itImage.IsIndependent = false;

				List<ItImage> exceptions = itImagesList.MakeDependantClipsSameSize(0, clipSize.Value, dependencyHorizontalSizeMargin, dependencyVericalSizeMargin);

				if (showMessage)
					if (exceptions != null && exceptions.Count > 0)
					{
						string message = "";
						int j;

						for (int i = 0; i < exceptions.Count; i++)
						{
							for (j = 0; j < this.viewPanel.Images.Count; j++)
							{
								if (this.viewPanel.Images[j].ItImage != null)
									if (this.viewPanel.Images[j].ItImage == exceptions[i])
									{
										break;
									}
							}
							if (j < this.viewPanel.Images.Count)  //image found in viewpnale image list
							{
								if (message.Length == 0)
								{
									message = string.Format("For the image size inconsistency, {0} image(s) had to be changed to independent. Image(s): {1}", exceptions.Count, (j + 1));

								}
								else
								{
									message += string.Format(", {0}", (j + 1));
								}
							}
						}

						MessageBox.Show(message + ".", "", MessageBoxButton.OK, MessageBoxImage.Warning);
					}
			}
			else
			if(showMessage)
			{
				throw new Exception("Can't get clips size!");
			}
		}

		#endregion

		#region ChangeDependency()
		private void ChangeDependency(BscanILL.UI.Dialogs.DependencyDlg.DependencyParameters parameters, VpImage selectedVpImage)
		{
			ItImages itImages = this.viewPanel.ItImages;

			if (itImages != null && itImages.Count > 0)
			{
				Lock();

				if (parameters.DialogResult == BscanILL.UI.Dialogs.DependencyDlg.Result.Dependent)
				{
					ItImages itImagesList = GetRelevantImagesList(itImages, parameters, selectedVpImage.ItImage);

					SetAsDependent( itImages, itImagesList, true);					
				}
				else if (parameters.DialogResult == BscanILL.UI.Dialogs.DependencyDlg.Result.Independent)
				{
					ItImages itImagesList = GetRelevantImagesList(itImages, parameters, selectedVpImage.ItImage);

					foreach (ItImage itImage in itImagesList)
						itImage.IsIndependent = true;
				}

				/*foreach (ViewPaneLibrary.Hierarchy.VpImage vpImage in this.viewPanel.Images)
					if (vpImage.ScanData != null)
						this.viewPanel.ImageItSettingsChanged(vpImage.ScanData);*/
			}
		}
		#endregion

		#region GetRelevantImagesList()
		ItImages GetRelevantImagesList(ItImages itImages, BscanILL.UI.Dialogs.DependencyDlg.DependencyParameters parameters, ItImage selectedImage)
		{
			ItImages selectedImages = new ItImages();

			if (parameters.SelectionRange == BscanILL.UI.Dialogs.DependencyDlg.SelectionRangeEnum.All)
			{
				foreach (ItImage itImage in itImages)
				{
					if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.AllSizes)
						selectedImages.Add(itImage);
					else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Portrait && itImage.InchSize.Width < itImage.InchSize.Height)
						selectedImages.Add(itImage);
					else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Landscape && itImage.InchSize.Width > itImage.InchSize.Height)
						selectedImages.Add(itImage);
				}
			}
			else if (parameters.SelectionRange == BscanILL.UI.Dialogs.DependencyDlg.SelectionRangeEnum.Range)
			{
				for (int i = Math.Max(0, parameters.From - 1); i <= Math.Min(itImages.Count - 1, parameters.To - 1); i++)
				{
					ItImage itImage = itImages[i];

					if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.AllSizes)
						selectedImages.Add(itImage);
					else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Portrait && itImage.InchSize.Width < itImage.InchSize.Height)
						selectedImages.Add(itImage);
					else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Landscape && itImage.InchSize.Width > itImage.InchSize.Height)
						selectedImages.Add(itImage);
				}
			}
			else if (parameters.SelectionRange == BscanILL.UI.Dialogs.DependencyDlg.SelectionRangeEnum.FromCurrentOn && selectedImage != null)
			{
				int from = itImages.IndexOf(selectedImage);

				if (from >= 0)
				{
					for (int i = from; i < itImages.Count; i++)
					{
						ItImage itImage = itImages[i];

						if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.AllSizes)
							selectedImages.Add(itImage);
						else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Portrait && itImage.InchSize.Width < itImage.InchSize.Height)
							selectedImages.Add(itImage);
						else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Landscape && itImage.InchSize.Width > itImage.InchSize.Height)
							selectedImages.Add(itImage);
					}
				}
			}

			return selectedImages;
		}
		#endregion

		#region ControlPanel_ResetCurrentClick()
		void ControlPanel_ResetCurrentClick()
		{
			try
			{
				this.Lock();
				ImageProcessing.IpSettings.ItImage itImage = this.viewPanel.SelectedItImage;

				if (itImage != null && itImage.IsFixed == false)
					itImage.Reset(false);
					//this.viewPanel.ItImages.ResetItImage(itImage);
			}
			catch (Exception ex)
			{
				Notify(this, BscanILL.Misc.Notifications.Type.Error, "ControlPanel_ResetCurrentClick", ex.Message, ex);
				MessageBox.Show("Unexpected exception raised: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			finally
			{
				this.UnLock();
			}
		}
		#endregion

		#region ControlPanel_ResetAllClick()
		void ControlPanel_ResetAllClick()
		{
			try
			{
				this.Lock();
				ImageProcessing.IpSettings.ItImages itImages = this.viewPanel.ItImages;

				foreach (ImageProcessing.IpSettings.ItImage itImage in itImages)
					itImage.Reset(false);
				
				//itImages.ResetSettings();
			}
			catch (Exception ex)
			{
				Notify(this, BscanILL.Misc.Notifications.Type.Error, "ControlPanel_ResetAllClick", ex.Message, ex);
				MessageBox.Show("Unexpected exception raised: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			finally
			{
				this.UnLock();
			}
		}
		#endregion

		#region Notify()
		protected void Notify(object sender, BscanILL.Misc.Notifications.Type type, string methodName, string message, Exception ex)
		{
			BscanILL.Misc.Notifications.Instance.Notify(sender, type, "FrameEditUi, " + methodName + "(): " + message, ex);
		}
		#endregion

		#region ControlPanel_ShowResultsClick()
		void ControlPanel_ShowResultsClick()
		{
			if (this.itResultsWindow.IsVisible == false)
			{
				this.itResultsWindow.LoadImages(this.viewPanel.Images, this.viewPanel.SelectedImage);
				this.itResultsWindow.Show();
			}
		}
		#endregion

		#region ViewPanel_ImageSelected()
		void ViewPanel_ImageSelected(VpImage vpImage)
		{
			if (this.itResultsWindow.IsVisible && vpImage != null)
			{
				this.itResultsWindow.SelectImage(this.viewPanel.SelectedImage);
			}

            if (vpImage != null)
            {
                this.article = ((IllImage)vpImage.Tag).Article;
            }

			this.controlPanel.IsCurrentOnlyEbanled = ExecuteCurrentImageEnabled();
            
            if (vpImage != null)
            {
                //selected image's IT settings are used for manual IT processing of the remaining images, if for example pull slip selected (which does not have ItImage specified)
                // it was causing exception in maual IT processing after hitting 'Apply' button to trigger manualIT
                if(vpImage.ItImage == null)
                {
                    this.controlPanel.manualItControl.applyManualIt.IsEnabled = false;
                }
                else
                {
                    this.controlPanel.manualItControl.applyManualIt.IsEnabled = true;
                }
            }
		}
		#endregion

		#region ExecuteCurrentImageEnabled()
		public bool ExecuteCurrentImageEnabled()
		{
			if (article != null && article.Scans.Count > 0 && this.SelectedImage != null && this.SelectedImage.IsPullslip == false)            
			{
                if (article.Scans.Count > 1)
                {
                    foreach (IllImage scan in article.Scans)
                    {
                        if (scan != this.SelectedImage && scan.IllPages.Count == 0)
                            return false;
                    }                    
                }
                else
                {
                    //just pull slip and one regular scan
                    //and focus is on the regular page
                    if (this.SelectedImage.IllPages.Count == 0)
                    {
                        return false;
                    }
                }
                return true;
			}

			return false;
		}
		#endregion

		#endregion

	}
}
