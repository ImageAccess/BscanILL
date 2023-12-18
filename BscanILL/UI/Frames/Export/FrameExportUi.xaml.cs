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

namespace BscanILL.UI.Frames.Export
{
	/// <summary>
	/// Interaction logic for FrameExportUi.xaml
	/// </summary>
	public partial class FrameExportUi : UserControl
	{
		BscanILL.Hierarchy.Article article = null;
        BscanILL.Hierarchy.SessionBatch batch = null;
		
		public event BscanILL.Misc.VoidHnd GoToStartClick;
		public event BscanILL.Misc.VoidHnd GoToScanClick;
		public event BscanILL.Misc.VoidHnd GoToItClick;
		
		public event BscanILL.Misc.VoidHnd ResendClick;
		public event BscanILL.Misc.VoidHnd HelpClick;

        public event BscanILL.Misc.BatchPageHnd PrintClick;

		public event BscanILL.Misc.VoidHnd ExportAllClick;
        public event BscanILL.Misc.VoidHnd ExportCurrentClick;

        public event BscanILL.Misc.ArticleHnd ArticleModifiedExpStageNotification;

        public delegate void UpdateEditProgressHnd(double progress);        
        public event UpdateEditProgressHnd ProgressChanged;


		#region constructor
		public FrameExportUi()
		{
			InitializeComponent();

            this.viewPanel.ImageSelected += new ViewPane.ImageSelectedHnd(ViewPanel_ImageSelected);            
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties
		public BscanILL.Hierarchy.IllPage	SelectedImage { get { return GetIllPage(viewPanel.SelectedImage); } }
		public ProgressPanel				ProgressPanel { get { return this.progressPanel; } }
		#endregion

        // PRIVATE PROPERTIES
        #region private properties
        //BscanILL.SETTINGS.Settings settings { get { return BscanILL.SETTINGS.Settings.Instance; } }
        
        private BscanILL.Hierarchy.Article Article
        {
            set
            {
                this.article = value;   //update just local variable this.article that stores currently selected article in export stage

                if (ArticleModifiedExpStageNotification != null)
                    ArticleModifiedExpStageNotification(this.article);   //pass the newly selected article info into MainWindow
            }
        }        
        #endregion

        // PUBLIC METHODS
		#region public methods

		#region Open()
        public void Open(BscanILL.Hierarchy.SessionBatch batch)
		{
            LoadArticle(batch);
			this.Visibility = System.Windows.Visibility.Visible;

            //if ( (settings.General.MultiArticleSupportEnabled == false) || ( batch.Articles.Count <= 1 ) )
            if (batch.Articles.Count <= 1)
            {
                this.buttonExportCurrent.Visibility = Visibility.Collapsed;
                this.buttonExportName.Text = "Send";
            }
            else
            {
                this.buttonExportCurrent.Visibility = Visibility.Visible;
                this.buttonExportName.Text = "Send All";
            }
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
			this.viewPanel.Clear();
		}
		#endregion

		#region ShowDefaultImage()
		public void ShowDefaultImage()
		{
			this.viewPanel.SelectImage(null);
		}
		#endregion

		#region ShowImage()
		public void ShowImage(BscanILL.Hierarchy.IllPage illPage)
		{
			VpImage vpImage = GetVpImage(illPage);

			if (vpImage != null)
				vpImage.Select();
		}
		#endregion

		#region GetIllPage()
		public BscanILL.Hierarchy.IllPage GetIllPage(VpImage vpImage)
		{
			if (vpImage != null && vpImage.Tag is BscanILL.Hierarchy.IllPage)
				return (BscanILL.Hierarchy.IllPage)vpImage.Tag;

			return null;
		}
		#endregion

		#region ExportProcessStarted()
		public void ExportProcessStarted()
		{
			this.progressPanel.Visibility = System.Windows.Visibility.Visible;
		}
		#endregion

		#region ExportProcessEndedSuccessfully()
		public void ExportProcessEndedSuccessfully()
		{
			this.progressPanel.ExportArticleFinishedSuccessfully();
		}
		#endregion

		#region ExportProcessEndedWithError()
		public void ExportProcessEndedWithError()
		{
			this.progressPanel.ExportFinishedWithError();
		}
		#endregion

		#region ExportProgressChanged()
		public void ExportProgressChanged(double progress)
		{
			this.progressPanel.Progress = progress;
		}
		#endregion

		#region ExportProgressCurrentAction()
		public void ExportProgressCurrentAction(string action)
		{
			this.progressPanel.CurrentAction = action;
		}
		#endregion

		#region ExportProgressAddComment()
		public void ExportProgressAddComment(string comment)
		{
			this.progressPanel.AddComment(comment);
		}
		#endregion

		#region ExportProcessReset()
		public void ExportProcessReset()
		{
			this.progressPanel.Reset();
			this.progressPanel.Visibility = System.Windows.Visibility.Hidden;
		}
		#endregion

        #region ArticleChanged()
        public void ArticleChanged(BscanILL.Hierarchy.Article article)
        {
            //LoadArticle(article);
        }
        #endregion
        
		#endregion


		// PRIVATE METHODS
		#region private methods

		#region LoadArticle()
        private void LoadArticle(BscanILL.Hierarchy.SessionBatch batch)
		{
			//if (article != null)
            if (batch.Articles.Count > 0)
			{                
                this.batch = batch;
                this.article = this.batch.Articles[0];
                this.articleControl.LoadArticle(this.article);

                //when going from CleanUp to Send tab, this was already executed in LoadArticleInThread()  so execute it just when going to Send tab from Resend tab               

                foreach (BscanILL.Hierarchy.Article article_item in batch.Articles)
                {
                  foreach (BscanILL.Hierarchy.IllPage illPage in article_item.GetPages(true))
                  {                        
                    if (illPage.IllImage == article_item.Pullslip)
                    {
                        this.viewPanel.AddImage(CreateVpImage(illPage, false));                            
                    }
                    else
                    {
                        this.viewPanel.AddImage(CreateVpImage(illPage, true));//this call created new Thread that creates all image derivatives (reduced, thumbnail, preview) for current image                             
                    }                        
                  }
                }                 

                //set StripPane horizontal scroll to zero
     ///           this.viewPanel.ResetStripPaneOffset();

                //select(=flag red) first non pull slip page in article
				int selectIndex = Math.Min(this.viewPanel.Images.Count, 2) - 1;                

				if (selectIndex >= 0)
					this.viewPanel.SelectImage(this.viewPanel.Images[selectIndex]);
			}
			else
			{
				Reset();
			}
		}
		#endregion


        #region CreatePageDerivFiles()
        public void CreatePageDerivFiles(BscanILL.Hierarchy.SessionBatch batch)
		{
			//if (article != null)
            if (batch.Articles.Count > 0)
			{
                int count = 0, countMax = 0 ;	
                this.batch = batch;
                VpImage vpImg;
                string strTmp = "";
                //this.article = this.batch.Articles[0];
                ///// this.articleControl.LoadArticle(this.article);              
                
                foreach (BscanILL.Hierarchy.Article article_item in batch.Articles)
                {
                   countMax += article_item.GetPages(true).Count ;                    
                }
                 
                foreach (BscanILL.Hierarchy.Article article_item in batch.Articles)
                {
                    foreach (BscanILL.Hierarchy.IllPage illPage in article_item.GetPages(true))
                    {
                        //this.viewPanel.AddImage(CreateVpImage(illPage));
                        if (illPage.IllImage == article_item.Pullslip)
                        {
                            vpImg = CreateVpImage(illPage, false);
                            strTmp = vpImg.ThumbnailPath;                            
                            vpImg.Dispose();
                            // this.viewPanel.AddImage(CreateVpImage(illPage, false));
                            //this.viewPanel.AddImage(CreateVpImage(illPage, false), ((((double)count / (double)countMax) * 0.8) + 0.2));
                        }
                        else
                        {
                            vpImg = CreateVpImage(illPage, true);
                            strTmp = vpImg.ThumbnailPath;                            
                            vpImg.Dispose();

                            //this.viewPanel.AddImage(CreateVpImage(illPage, true));//this call created new Thread that creates all image derivatives (reduced, thumbnail, preview) for current image 
                            //this.viewPanel.AddImage(CreateVpImage(illPage, true), ((((double)count / (double)countMax) * 0.8) + 0.2) );
                        }

                        count++;

                        if (ProgressChanged != null)                            
                            ProgressChanged((((double)count / (double)countMax) * 0.8) + 0.2);      

                    }

                }                

                //select(=flag red) first non pull slip page in article
//				int selectIndex = Math.Min(this.viewPanel.Images.Count, 2) - 1;

//				if (selectIndex >= 0)
//					this.viewPanel.SelectImage(this.viewPanel.Images[selectIndex]);
			}
//			else
//			{
				//Reset();
//			}
		}
		#endregion

        #region ExportAll_Click()
        private void ExportAll_Click(object sender, RoutedEventArgs e)
		{
			if (ExportAllClick != null)
				ExportAllClick();
		}
		#endregion

        #region Export_ClickCurrent()
        private void ExportCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (ExportCurrentClick != null)
                ExportCurrentClick();
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

		#region GoToIt_Click()
		private void GoToIt_Click(object sender, RoutedEventArgs e)
		{
			if (GoToItClick != null)
				GoToItClick();
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

		#region GetVpImage()
		private VpImage GetVpImage(BscanILL.Hierarchy.IllPage illPage)
		{
			/*foreach (VpImage vpImage in this.viewPanel.Images)
				if (vpImage.FullPath.ToLower() == illPage.FilePath.FullName.ToLower())
					return vpImage;*/

			foreach (VpImage vpImage in this.viewPanel.Images)
				if (vpImage.Tag == illPage)
					return vpImage;

			return null;
		}
		#endregion

		#region CreateVpImage()
        private VpImage CreateVpImage(BscanILL.Hierarchy.IllPage illPage, bool itActive)
		{
			ViewPane.Hierarchy.VpImage vpImage = new ViewPane.Hierarchy.VpImage(illPage.FilePath.FullName, illPage.FilePath.FullName + ".reduced",
         //       illPage.FilePath.FullName + ".preview", illPage.FilePath.FullName + ".thumbnail", false, false);
                illPage.FilePath.FullName + ".preview", illPage.FilePath.FullName + ".thumbnail", itActive, false);

			vpImage.Tag = illPage;
			return vpImage;
		}
		#endregion

        #region ViewPanel_ImageSelected()
        void ViewPanel_ImageSelected(VpImage vpImage)
        {
            if (this.IsVisible && vpImage != null)
            {                
                if (this.article != ((BscanILL.Hierarchy.IllPage)vpImage.Tag).Article)
                {                    
                    SetArticle(((BscanILL.Hierarchy.IllPage)vpImage.Tag).Article);
                    //this.article = ((BscanILL.Hierarchy.IllPage)vpImage.Tag).Article;
                    //articleControl.LoadArticle(this.article);                    
                    //this.viewPanel.SelectedImage                    
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
                //this.article = ((BscanILL.Hierarchy.IllPage)vpImage.Tag).Article;
                articleControl.LoadArticle(article);                    
            }
        }
        #endregion
        
        #region Print_Click()
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (PrintClick != null)
            {
                BscanILL.Hierarchy.IllPage selectedPage = this.SelectedImage;                

                if ((batch != null || article != null) && selectedPage != null)                
                    PrintClick(batch, article, selectedPage);                
            }
        }
        #endregion

		#endregion

	}
}
