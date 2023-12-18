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
using BscanILL.Hierarchy;
using System.IO;


namespace BscanILL.UI.Frames.Start
{
	/// <summary>
	/// Interaction logic for FrameStartUi.xaml
	/// </summary>
	public partial class FrameStartUi : UserControl
	{
		public event BscanILL.Misc.VoidHnd GoToScanClick;

		public event BscanILL.Misc.VoidHnd ResendClick;
		public event BscanILL.Misc.VoidHnd HelpClick;
		public event BscanILL.Misc.VoidHnd KicImportClick;
		public event BscanILL.Misc.VoidHnd DiskImportClick;
		public event BscanILL.Misc.VoidHnd OpenSettingsClick;

		public event BscanILL.Misc.VoidHnd ScanClick;


		#region constructor
		public FrameStartUi()
		{
			InitializeComponent();

			//this.gridShortKeys.Visibility = System.Windows.Visibility.Collapsed;
            
            if (BscanILL.SETTINGS.Settings.Instance.General.KicImportEnabled)                       
            {
                //rowKicImport.Height = new GridLength(1, GridUnitType.Auto);

                column1KicImport.Width = new GridLength(1, GridUnitType.Star);
                column2KicImport.Width = new GridLength(8, GridUnitType.Pixel);
                column3KicImport.Width = new GridLength(5.5, GridUnitType.Star);
            }
            else
            {
                //rowKicImport.Height = new GridLength(0);

                column1KicImport.Width = new GridLength(0);
                column2KicImport.Width = new GridLength(0);
                column3KicImport.Width = new GridLength(0);
            }
            
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public bool PrimaryScannerSelected
		{
			get { return scannersControl.PrimaryScannerSelected; }
			set { scannersControl.PrimaryScannerSelected = value; }
		}

		#endregion

		// PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			this.imagePanel.Dispose();
		}
		#endregion

		#region Open()
		public void Open(BscanILL.Hierarchy.Article article)
		{
			LoadArticle(article);
			this.Visibility = Visibility.Visible;
		}

        public void Open(BscanILL.Hierarchy.SessionBatch batch)
        {            
            LoadArticle(batch.CurrentArticle);
            this.Visibility = Visibility.Visible;
        }
		#endregion

		#region Reset()
		public void Reset()
		{
			articleControl.LoadArticle(null);
			//articleControl.Visibility = System.Windows.Visibility.Collapsed;

			this.imagePanel.Clear();
		}
		#endregion

		#region ArticleChanged()
		public void ArticleChanged(BscanILL.Hierarchy.Article article)
		{
			LoadArticle(article);
		}
		#endregion

		#region ScannerChanged()
		public void ScannerChanged(bool scannerConnected)
		{
			buttonScanPullslip.IsEnabled = scannerConnected;
		}
		#endregion

		#region ShowDefaultImage()
		public void ShowDefaultImage()
		{
		}
		#endregion

		#region ShowImage()
		public void ShowImage(string file)
		{
			this.imagePanel.ShowImage(file);
		}

		/*public void ShowImage(BscanILL.Hierarchy.IllImage illImage)
		{
		}*/
		#endregion

        #region ScanNextPullslip()
        public void ScanNextPullslip()
        {
            if (ScanClick != null)
                ScanClick();
        }
        #endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region ScanPullslip_Click()
		private void ScanPullslip_Click(object sender, RoutedEventArgs e)
		{
			if (ScanClick != null)
				ScanClick();
		}
		#endregion

		#region Settings_Click()
		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			if (OpenSettingsClick != null)
				OpenSettingsClick();
		}
		#endregion

		#region GoToScan_Click()
		private void GoToScan_Click(object sender, EventArgs e)
		{
			if (GoToScanClick != null)
				GoToScanClick();
		}
		#endregion

		#region KICImport_Click()
		private void KICImport_Click(object sender, RoutedEventArgs e)
		{
			if (KicImportClick != null)
				KicImportClick();
		}
		#endregion

		#region DiskImport_Click()
		private void DiskImport_Click(object sender, RoutedEventArgs e)
		{
			if (DiskImportClick != null)
				DiskImportClick();
		}
		#endregion

		#region LoadArticle()
		public void LoadArticle(BscanILL.Hierarchy.Article article)
		{
			if (article != null)
			{
				articleControl.LoadArticle(article);
				//articleControl.Visibility = System.Windows.Visibility.Visible;

				if (article.Pullslip != null)
					this.imagePanel.ShowImage(article.Pullslip.FilePath.FullName);
			}
			else
			{
				Reset();
			}
		}
		#endregion

		#region GoToScan_Click()
		private void GoToScan_Click(object sender, RoutedEventArgs e)
		{
			//if (this.articleControl.Article != null && GoToScanClick != null)
            if (GoToScanClick != null)
				GoToScanClick();
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
		private void Help_Click(object sender, EventArgs e)
		{
			if (HelpClick != null)
				HelpClick();
		}
		#endregion

		#region Preview_KeyUp()
		private void Preview_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F1)
			{
				/*if (HelpClick != null)
					HelpClick();*/
			}
            else if (e.Key == Key.F2)
            {
                // scan pullslip
            }
			else if (e.Key == Key.F9)
			{
				// kic import
			}
			else if (e.Key == Key.F11)
			{
				// import from disk
			}
		}
		#endregion

		#region UserControl_SizeChanged(0
		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.ActualHeight < 860)
				rowLogo.Height = new GridLength(0);
			else
				rowLogo.Height = new GridLength(1, GridUnitType.Auto);
		}
		#endregion

		#endregion

	}
}
