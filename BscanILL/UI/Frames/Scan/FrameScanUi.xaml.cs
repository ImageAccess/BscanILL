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
using ViewPane.Hierarchy;

namespace BscanILL.UI.Frames.Scan
{
	/// <summary>
	/// Interaction logic for FrameScanUi.xaml
	/// </summary>
	public partial class FrameScanUi : UserControl
	{
		BscanILL.Hierarchy.Article article = null;
        BscanILL.Hierarchy.SessionBatch batch = null;
		
		public event BscanILL.Misc.VoidHnd GoToStartClick;
		public event BscanILL.Misc.VoidHnd GoToItClick;
        public event BscanILL.Misc.VoidHnd ResetClick;
        
		public event BscanILL.Misc.VoidHnd ResendClick;
		public event BscanILL.Misc.VoidHnd HelpClick;

		public event BscanILL.Misc.VoidHnd		ScanClick;
        public event BscanILL.Misc.VoidHnd      ScanPullslipClick;
		public event BscanILL.Misc.IllImageHnd	RescanClick;
		public event BscanILL.Misc.IllImageHnd	InsertBeforeClick;
		public event BscanILL.Misc.IllImageHnd	DeleteImageClick;
		public event BscanILL.Misc.VoidHnd		DiskImportClick;
        public event BscanILL.Misc.BatchImageHnd PrintClick;

		public event BscanILL.Misc.VoidHnd		S2N_MoreSettingsClick;
		public event BscanILL.Misc.VoidHnd		Bookedge_MoreSettingsClick;
		public event BscanILL.Misc.VoidHnd		Click_MoreSettingsClick;
		public event BscanILL.Misc.VoidHnd		ClickMini_MoreSettingsClick;
		public event BscanILL.Misc.VoidHnd		Adf_MoreSettingsClick;

        public event BscanILL.Misc.VoidHnd      ScanSettings_Changed;
        public event Scanners.S2N.ScanRequestHnd ScanSplittingSettings_Changed;

        public event BscanILL.Misc.ArticleHnd   ArticleModifiedNotification;

        public BscanILL.Misc.VoidHnd ScanSessionReset;

		#region constructor
		public FrameScanUi()
		{
			InitializeComponent();
                       
			this.scannersControl.S2N_MoreSettingsClick += delegate() { if (S2N_MoreSettingsClick != null) S2N_MoreSettingsClick(); };
			this.scannersControl.Bookedge_MoreSettingsClick += delegate() { if (Bookedge_MoreSettingsClick != null) Bookedge_MoreSettingsClick(); };
			this.scannersControl.Click_MoreSettingsClick += delegate() { if (Click_MoreSettingsClick != null) Click_MoreSettingsClick(); };
			this.scannersControl.ClickMini_MoreSettingsClick += delegate() { if (ClickMini_MoreSettingsClick != null) ClickMini_MoreSettingsClick(); };
			this.scannersControl.Adf_MoreSettingsClick += delegate() { if (Adf_MoreSettingsClick != null) Adf_MoreSettingsClick(); };

            this.scannersControl.S2N_SettingsChanged += delegate() { if (ScanSettings_Changed != null) ScanSettings_Changed(); };
            this.scannersControl.S2N_SplittingSettingsChanged += delegate(Scanners.S2N.ScannerScanAreaSelection scanArea) { if (ScanSplittingSettings_Changed != null) ScanSplittingSettings_Changed(scanArea); };            

         //   this.viewPanel. stripPane.ImageSelected += new ViewPane.Thumbnails.StripPane.ImageSelectedHnd(Strip_ImageSelected);
            this.viewPanel.ImageSelected += new ViewPane.ImageSelectedHnd(ViewPanel_ImageSelected);

                        
            if (settings.General.MultiArticleSupportEnabled == true)    
            {
                //rowKicImport.Height = new GridLength(1, GridUnitType.Auto);

                column1ScanPullslip.Width = new GridLength(1, GridUnitType.Star);
                column2ScanPullslip.Width = new GridLength(8, GridUnitType.Pixel);
                column3ScanPullslip.Width = new GridLength(4.0, GridUnitType.Star);
            }
            else
            {                
                column1ScanPullslip.Width = new GridLength(0);
                column2ScanPullslip.Width = new GridLength(0);
                column3ScanPullslip.Width = new GridLength(0);
            }

		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#region SelectedImage
		public BscanILL.Hierarchy.IllImage		SelectedImage { get { return GetIllImage(viewPanel.SelectedImage); } }
		#endregion
        
       // public BscanILL.Hierarchy.Article Article { get { return this.MainWindow.Article; } }

		public Scanners.FileFormat	FileFormat { get { return scannersControl.FileFormat; } }
		public double				Brightness { get { return scannersControl.Brightness; } }
		public double				Contrast { get { return scannersControl.Contrast; } }
		public Scanners.ColorMode	ColorMode { get { return scannersControl.ColorMode; } }
		public ushort				Dpi { get { return scannersControl.Dpi; } }

		#endregion


		// PRIVATE PROPERTIES
		#region private properties
		BscanILL.SETTINGS.Settings		settings { get { return BscanILL.SETTINGS.Settings.Instance; } }

        private BscanILL.Hierarchy.Article Article
        {
            set
            {
                this.article = value;   //update just local variable this.article that stores currently selected article in scan stage

                if (ArticleModifiedNotification != null)
                    ArticleModifiedNotification(this.article);   //pass the newly selected article info into MainWindow
            }
        }

        private BscanILL.Hierarchy.SessionBatch Batch
        {
            get { return this.batch; }

            set
            {
                this.batch = value;                  
            }
        }
		#endregion


		// PUBLIC METHODS
		#region public methods

		#region UpdateSplittingButtons()
		public void UpdateSplittingButtons(Scanners.S2N.ScannerScanAreaSelection  scanArea)
        {
            if (this.Dispatcher.CheckAccess())
                SplittingButtonsUpdate(scanArea);
            else
                this.Dispatcher.BeginInvoke((Action)delegate() { SplittingButtonsUpdate(scanArea); });
        }
		#endregion
		
		#region UpdateS2NColorDepthButtons()
		public void UpdateS2NColorDepthButtons(Scanners.S2N.ColorMode colorDepth)
		{
			if (this.Dispatcher.CheckAccess())
				ColorDepthS2NButtonsUpdate(colorDepth);
			else
				this.Dispatcher.BeginInvoke((Action)delegate () { ColorDepthS2NButtonsUpdate(colorDepth); });
		}
		#endregion

		#region UpdateTwainColorDepthButtons()
		public void UpdateTwainColorDepthButtons(Scanners.Twain.ColorMode colorDepth)
		{
			if (this.Dispatcher.CheckAccess())
				ColorDepthTwainButtonsUpdate(colorDepth);
			else
				this.Dispatcher.BeginInvoke((Action)delegate () { ColorDepthTwainButtonsUpdate(colorDepth); });
		}
		#endregion

		#region UpdateClickColorDepthButtons()
		public void UpdateClickColorDepthButtons(Scanners.Click.ClickColorMode colorDepth)
		{
			if (this.Dispatcher.CheckAccess())
				ColorDepthClickButtonsUpdate(colorDepth);
			else
				this.Dispatcher.BeginInvoke((Action)delegate () { ColorDepthClickButtonsUpdate(colorDepth); });
		}
		#endregion

		#region UpdateClickMiniColorDepthButtons()
		public void UpdateClickMiniColorDepthButtons(Scanners.Click.ClickColorMode colorDepth)
		{
			if (this.Dispatcher.CheckAccess())
				ColorDepthClickMiniButtonsUpdate(colorDepth);
			else
				this.Dispatcher.BeginInvoke((Action)delegate () { ColorDepthClickMiniButtonsUpdate(colorDepth); });
		}
		#endregion

		#region Open()
		//public void Open(BscanILL.Hierarchy.Article article, BscanILL.Hierarchy.SessionBatch batch)
		public void Open(BscanILL.Hierarchy.SessionBatch batch)
		{            
			LoadArticle(batch);            
			this.Visibility = System.Windows.Visibility.Visible;
            if (settings.General.MultiArticleSupportEnabled == false)
            {
                this.scanPullsip.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.scanPullsip.Visibility = Visibility.Visible;
            }

            if (settings.General.MultiArticleSupportEnabled == true)
            {                
                column1ScanPullslip.Width = new GridLength(1, GridUnitType.Star);
                column2ScanPullslip.Width = new GridLength(8, GridUnitType.Pixel);
                column3ScanPullslip.Width = new GridLength(4.0, GridUnitType.Star);
            }
            else
            {                
                column1ScanPullslip.Width = new GridLength(0);
                column2ScanPullslip.Width = new GridLength(0);
                column3ScanPullslip.Width = new GridLength(0);
            }

            this.Batch = batch;
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			this.viewPanel.Dispose();
		}
		#endregion

		#region Reset()
		public void Reset()
		{
            articleControlSmall.LoadArticle(null);
            this.Article = null;            
			this.viewPanel.Clear();
		}
		#endregion

		#region ArticleChanged()
		public void ArticleChanged(BscanILL.Hierarchy.Article article)
		{
			//LoadArticle(article);
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
		#endregion

		#region CheckKeyStroke()
		public void CheckKeyStroke(KeyEventArgs e)
		{
			bool ctrl = ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) > 0);
			bool alt = ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Alt) > 0);
			bool shift = ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Shift) > 0);

			switch (e.Key)
			{
				case System.Windows.Input.Key.F3:
					{
						if (this.ScanClick != null)
						{
							ScanClick();
							e.Handled = true;
						}
					} break;
				default:
					{
					} break;
			}
		}
		#endregion

        #region Remove_VpImage()

        public void Remove_VpImage( Hierarchy.IllImage illImage)
        {
            Scans_ImageRemoving( illImage) ;
        }
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		private void SplittingButtonsUpdate(Scanners.S2N.ScannerScanAreaSelection scanArea)
		{
			//this.scannersControl.s2nControl.pageSplittingControl.ScanSettings.Splitting.Value = (Scanners.S2N.Splitting)scanArea;
			if( this.scannersControl.s2nControl.pageSplittingControl.ScanSettings.Splitting.Value != (Scanners.S2N.Splitting)scanArea )
            {
				this.scannersControl.s2nControl.pageSplittingControl.ScanSettings.Splitting.Value = (Scanners.S2N.Splitting)scanArea;

				if (ScanSettings_Changed != null) ScanSettings_Changed();
			}
		}

		private void ColorDepthS2NButtonsUpdate(Scanners.S2N.ColorMode colorDepth)
		{
			//if Bookeye
			//changing color button setting by pressing F6,F7 or F8
			//if (this.scannersControl.clickMiniControl.IsVisible)
			if (this.scannersControl.s2nControl.colorModeControl.ScanSettings.ColorMode.Value != colorDepth)
			{
				this.scannersControl.s2nControl.colorModeControl.ScanSettings.ColorMode.Value = colorDepth;

				if (ScanSettings_Changed != null) ScanSettings_Changed();
			}
		}

		private void ColorDepthTwainButtonsUpdate(Scanners.Twain.ColorMode colorDepth)
		{
			//if Bookedge
			//changing color button setting by pressing F6,F7 or F8
			//if (this.scannersControl.bookedgeControl.IsVisible)
			if (this.scannersControl.bookedgeControl.colorModeControl.ScanSettings.ColorMode.Value != colorDepth)
			{
				this.scannersControl.bookedgeControl.colorModeControl.ScanSettings.ColorMode.Value = colorDepth;

				if (ScanSettings_Changed != null) ScanSettings_Changed();
			}
		}

		private void ColorDepthClickButtonsUpdate(Scanners.Click.ClickColorMode colorDepth)
		{
			//if Click
			//changing color button setting by pressing F6,F7 or F8
			//if (this.scannersControl.clickControl.IsVisible)
			if (this.scannersControl.clickControl.colorModeControl.ScanSettings.ColorMode.Value != colorDepth)
			{
				this.scannersControl.clickControl.colorModeControl.ScanSettings.ColorMode.Value = colorDepth;

				if (ScanSettings_Changed != null) ScanSettings_Changed();
			}
		}

		private void ColorDepthClickMiniButtonsUpdate(Scanners.Click.ClickColorMode colorDepth)
		{
			//if Click
			//changing color button setting by pressing F6,F7 or F8
			//if (this.scannersControl.clickMiniControl.IsVisible)
			if (this.scannersControl.clickMiniControl.colorModeControl.ScanSettings.ColorMode.Value != colorDepth)
			{
				this.scannersControl.clickMiniControl.colorModeControl.ScanSettings.ColorMode.Value = colorDepth;

				if (ScanSettings_Changed != null) ScanSettings_Changed();
			}
		}

		#region Scan_Click()
		private void Scan_Click(object sender, RoutedEventArgs e)
		{
			if (ScanClick != null)
				ScanClick();
		}
		#endregion


        #region ScanPullSlip_Click()
        private void ScanPullSlip_Click(object sender, RoutedEventArgs e)
		{
            if (ScanPullslipClick != null)
                ScanPullslipClick();
		}
		#endregion

		#region Rescan_Click()
		private void Rescan_Click(object sender, RoutedEventArgs e)
		{
			if (RescanClick != null)
			{
				BscanILL.Hierarchy.IllImage selectedImage = this.SelectedImage;

				if (selectedImage != null && selectedImage.IsPullslip == false)
					RescanClick(selectedImage);
			}
		}
		#endregion

        #region Reset_Click()
        private void Reset_Click(object sender, RoutedEventArgs e)
		{
			if (ResetClick != null)			
                 ResetClick();			
		}
		#endregion

		#region InsertBefore_Click()
		private void InsertBefore_Click(object sender, RoutedEventArgs e)
		{
			if (InsertBeforeClick != null)
			{
				BscanILL.Hierarchy.IllImage selectedImage = this.SelectedImage;

				if (selectedImage != null && selectedImage.IsPullslip == false)
					InsertBeforeClick(selectedImage);
			}
		}
		#endregion

		#region DeletePage_Click()
		private void DeletePage_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (DeleteImageClick != null)
				{
					BscanILL.Hierarchy.IllImage selectedImage = this.SelectedImage;

					//if (selectedImage != null && selectedImage.IsPullslip == false)  //now we allow to delete pull lsip in case it is only page in article
                    if (selectedImage != null && (selectedImage.IsPullslip == false || selectedImage.Article.Scans.Count == 0))  //now we allow to delete pull lsip in case it is only page in article
					{
						int index = this.viewPanel.SelectedImageIndex;

						if (this.viewPanel.Images.Count <= index + 1)
							index = index - 1;

						DeleteImageClick(selectedImage);

                        if (index >= 0 && index < this.viewPanel.Images.Count)
                        {
                            this.viewPanel.SelectImage(this.viewPanel.Images[index]);
                        }
                        else
                        {
                           //only  pull slip on screen was deleted - reset Scan stage and activate Start screen
                           if( ScanSessionReset != null)
                           {
                               ScanSessionReset();
                           }
                        }


					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region GoToStart_Click()
		private void GoToStart_Click(object sender, EventArgs e)
		{
			if (GoToStartClick != null)
				GoToStartClick();
		}
		#endregion

		#region GoToIt_Click()
		private void GoToIt_Click(object sender, EventArgs e)
		{
			if (GoToItClick != null)
				GoToItClick();
		}
		#endregion

		#region Scans_ImageAdded()
		void Scans_ImageAdded(Hierarchy.IllImage illImage)
		{
            //find index offset of beginning of current article in viewPanel, then add index to the end of article
            int index = 0;
            if (this.viewPanel.Images.Count > 0)
            {
                //foreach (VpImage img in this.viewPanel.Images)
                for (int i = 0; i < this.viewPanel.Images.Count; i++)
                {
                    if (GetIllImage(this.viewPanel.Images[i]).Article == this.article)
                    {
                        i++;
                        index++;

                        //find end of article
                        for ( int j = i; j < this.viewPanel.Images.Count; j++)
                        {
                           if (GetIllImage(this.viewPanel.Images[j]).Article == this.article)
                           {
                              index++;
                           }
                           else
                           {
                               break;
                           }
                        }
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
            }

			//this.viewPanel.AddImage(CreateVpImage(illImage));
            if (this.viewPanel.Images.Count == index)
            {
                //adding to the last article in batch
                this.viewPanel.AddImage(CreateVpImage(illImage, true));
            }
            else
            {
                //adding image to the article that is not last in batch
                this.viewPanel.InsertImageBefore(CreateVpImage(illImage, true), this.viewPanel.Images[index]);
            }
		}
		#endregion

		#region Scans_ImageInserted()
		/// <summary>
		/// index of the curent scan, not counting pullslip
		/// </summary>
		/// <param name="index">index of the curent scan, not counting pullslip</param>
		/// <param name="illImage"></param>
		void Scans_ImageInserted(int index, Hierarchy.IllImage illImage)
		{            
            if (this.viewPanel.Images.Count > 0)
            {
                // find index offset of beginning of current article in viewPanel and add it to index
                foreach (VpImage img in this.viewPanel.Images)
                {
                    if (GetIllImage(img).Article != this.article)
                    {
                        index++;
                    }
                    else
                    {
                        break;  //found begining of current article
                    }
                }
            }

			if (this.viewPanel.Images.Count > index)
			{
				if (this.article != null && this.article.Pullslip != null)
					index++;
				
				//this.viewPanel.InsertImageBefore(CreateVpImage(illImage), this.viewPanel.Images[index]);
                this.viewPanel.InsertImageBefore(CreateVpImage(illImage, true), this.viewPanel.Images[index]);
			}
		}
		#endregion

		#region Scans_ImageRemoving()
		void Scans_ImageRemoving(Hierarchy.IllImage illImage)
		{
			VpImage vpImage = GetVpImage(illImage);

			if(vpImage != null)
				this.viewPanel.RemoveImage(vpImage);
		}
		#endregion

		#region Scans_Clearing()
		void Scans_Clearing()
		{
			Reset();
		}
		#endregion

		#region LoadArticle()
        void LoadArticle(BscanILL.Hierarchy.SessionBatch batch)
		{
            bool skipLoading = false;            
            bool articleLoaded = false ;

            if (this.article != null)
                if (batch.CurrentArticle != null)
                {
                    if (this.article == batch.CurrentArticle)
                    {
                        skipLoading = true;  //no need to do anything current article has not changed
                    }
                    else
                   // if (Main_Window_Get != null)
                    {
                        //articleExists = Main_Window_Get().Batch.ArticleExistsInBatch(article);
                        articleLoaded = batch.ArticleLoadedInScanFrame(batch.CurrentArticle);
                    }
                }

            if( ! skipLoading )
            {
                if ( ! articleLoaded )
                {
			        if (this.article != null)
			        {
                        if (settings.General.MultiArticleSupportEnabled == false)  
                        {
                            this.article.Scans.ImageAdded -= new Hierarchy.ImageAddingEventHnd(Scans_ImageAdded);
                            this.article.Scans.ImageInserted -= new Hierarchy.ImageInsertingEventHnd(Scans_ImageInserted);
                            this.article.Scans.ImageRemoving -= new Hierarchy.ImageRemovingEventHnd(Scans_ImageRemoving);
                            this.article.Scans.Clearing -= new Hierarchy.ClearingEventHnd(Scans_Clearing);
                            this.article = null;
                        }
			        }

                    if (batch.CurrentArticle != null)
			        {
                        this.Article = batch.CurrentArticle;

                        articleControlSmall.LoadArticle(batch.CurrentArticle);

                        if (batch.CurrentArticle.Pullslip != null)
			        //		this.viewPanel.AddImage(CreateVpImage(article.Pullslip));
                            this.viewPanel.AddImage(CreateVpImage(batch.CurrentArticle.Pullslip, false));

                        foreach (BscanILL.Hierarchy.IllImage illImage in batch.CurrentArticle.Scans)
					        //this.viewPanel.AddImage(CreateVpImage(illImage));
                            this.viewPanel.AddImage(CreateVpImage(illImage, true));

				        this.article.Scans.ImageAdded += new Hierarchy.ImageAddingEventHnd(Scans_ImageAdded);
				        this.article.Scans.ImageInserted += new Hierarchy.ImageInsertingEventHnd(Scans_ImageInserted);
				        this.article.Scans.ImageRemoving += new Hierarchy.ImageRemovingEventHnd(Scans_ImageRemoving);
				        this.article.Scans.Clearing += new Hierarchy.ClearingEventHnd(Scans_Clearing);

				        this.viewPanel.SelectLastImage();                        
			        }
			        else
			        {
				        Reset();
			        }
                }
                else
                {
                    SetArticle(batch.CurrentArticle);
                }
            }
		}
		#endregion

        #region SetArticle()
        void SetArticle(BscanILL.Hierarchy.Article article)
        {
            if (article != null)
            {
                this.Article = article;
                articleControlSmall.LoadArticle(article);             
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

		#region GoToResend_Click()
		private void GoToResend_Click(object sender, RoutedEventArgs e)
		{
			if (ResendClick != null)
				ResendClick();
		}
		#endregion

		#region Help_Click()
		private void Help_Click(object sender, RoutedEventArgs e)
		{
			if (HelpClick != null)
				HelpClick();
		}
		#endregion

		#region GoToIt_Click()
		private void GoToIt_Click(object sender, RoutedEventArgs e)
		{
			if (GoToItClick != null)
				GoToItClick();
		}
		#endregion

		#region GetIllImage()
		private BscanILL.Hierarchy.IllImage GetIllImage(VpImage vpImage)
		{
			if (vpImage != null && vpImage.Tag is BscanILL.Hierarchy.IllImage)
				return (BscanILL.Hierarchy.IllImage)vpImage.Tag;

			return null;
		}
		#endregion
	
		#region GetVpImage()
		private VpImage GetVpImage(BscanILL.Hierarchy.IllImage illImage)
		{
			/*foreach (VpImage vpImage in this.viewPanel.Images)
				if (vpImage.FullPath.ToLower() == illImage.FilePath.FullName.ToLower())
					return vpImage;
			*/

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
            //    illImage.FilePath.FullName + ".preview", illImage.FilePath.FullName + ".thumbnail", false, false);
                illImage.FilePath.FullName + ".preview", illImage.FilePath.FullName + ".thumbnail", itActive, false);

			vpImage.Tag = illImage;

			return vpImage;
		}
		#endregion

        #region ViewPanel_ImageSelected()
        void ViewPanel_ImageSelected(VpImage vpImage)
        {
            if (this.IsVisible && vpImage != null)
            {
                //if ( vpImage != null)
                //if ( vpImage.IsPullSlip )
                if (this.article != ((BscanILL.Hierarchy.IllImage)vpImage.Tag).Article)
                {
                    SetArticle(((BscanILL.Hierarchy.IllImage)vpImage.Tag).Article);
                    //articleControlSmall.LoadArticle(((BscanILL.Hierarchy.IllImage)vpImage.Tag).Article);
                }

                // if current image is a pull slip -> disable Scan Insert button not to jump to previous Article when scanning in front ofcurrent article pull slip
                //BscanILL.Hierarchy.IllImage temp = (BscanILL.Hierarchy.IllImage)vpImage.Tag
                

                //if ( vpImage.IsPullSlip )
                if (GetIllImage(vpImage).IsPullslip)
                {
                    this.insertBeforeButton.IsEnabled = false;                    
                    this.insertBeforeLabel.IsEnabled = false;
                    this.rescanLabel.IsEnabled = false;   
                    
                    if( ((BscanILL.Hierarchy.IllImage)vpImage.Tag).Article.Scans.Count == 0 )
                    {
                        this.deletePageButton.IsEnabled = true;    
                    }
                    else
                    {
                        this.deletePageButton.IsEnabled = false;    
                    }
                }
                else
                {
                    this.insertBeforeButton.IsEnabled = true;
                    this.deletePageButton.IsEnabled = true;
                    this.insertBeforeLabel.IsEnabled = true;
                    this.rescanLabel.IsEnabled = true;                     
                }                
            }
        }
        #endregion

		#region DiskImport_Click()
		private void DiskImport_Click(object sender, RoutedEventArgs e)
		{
			if (DiskImportClick != null)
				DiskImportClick();
		}
		#endregion

        #region Print_Click()
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (PrintClick != null)
            {
                BscanILL.Hierarchy.IllImage selectedImage = this.SelectedImage;
                  
                if ( ( batch != null || article != null ) && selectedImage != null)
                {
                    PrintClick(batch , article, selectedImage) ;
                }
            }
        }
        #endregion

		#endregion

	}
}
