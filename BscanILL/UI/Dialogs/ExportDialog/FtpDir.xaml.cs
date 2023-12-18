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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;

namespace BscanILL.UI.Dialogs.ExportDialog
{
	/// <summary>
	/// Interaction logic for Ftp.xaml
	/// </summary>
	public partial class FtpDir : PanelBase, IPanel
	{
		bool	sendConfirmEmail;
		bool	saveToSubfolder;
        BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn subfolderNameBase;
		bool	updateIlliad;
		bool	changeStatusToRequestFinished;
		string	ftpAddress = "";
		string	exportDirectory = "";
		bool	includePullslip = true;
        string  transactionNumber = "";
        string  illNumber = "";
		
		#region constructor
		public FtpDir()
		{
			InitializeComponent();

			this.sendConfirmEmail = _settings.Export.FtpDirectory.SendConfirmationEmail;
            this.saveToSubfolder = _settings.Export.FtpDirectory.SaveToSubfolder;
            this.subfolderNameBase = _settings.Export.FtpDirectory.SubFolderNameBase;
			this.updateIlliad = _settings.Export.FtpDirectory.UpdateILLiad;
			this.changeStatusToRequestFinished = _settings.Export.FtpDirectory.ChangeRequestToFinished;
			this.ftpAddress = _settings.Export.FtpDirectory.FtpAddress;
			this.exportDirectory = _settings.Export.FtpDirectory.ExportDirectoryPath;

            this.FileNamePrefix = BscanILL.Export.Misc.GetUniqueExportFilePrefix(Scan.FileFormat.Pdf, true);

            this.fileformatCtrl.UpdateFileParamControl += delegate()
            {
                FileFormat_SelectionChanged();
            };

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Scan.FileFormat FileFormat { get { return this.fileformatCtrl.FileFormat; } }
		public bool MultiImage { get { return this.fileformatCtrl.MultiImage; } }
		public bool PdfA { get { return this.fileformatCtrl.PdfA; } }
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get { return this.fileparamCtrl.FileColor; } }
        public int FileQuality { get { return this.fileparamCtrl.FileQuality; } } 

		#region UpdateILLiad
		public bool UpdateILLiad
		{
			get { return this.updateIlliad; }
			set
			{
				this.updateIlliad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ChangeStatusToRequestFinished
		public bool ChangeStatusToRequestFinished
		{
			get { return this.changeStatusToRequestFinished; }
			set
			{
				this.changeStatusToRequestFinished = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region FtpLink
		public string FtpLink
		{
			get { return this.ftpAddress; }
			set
			{
				this.ftpAddress = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ExportDirectory
		public string ExportDirectory
		{
			get { return this.exportDirectory; }
			set
			{
				this.exportDirectory = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SendConfirmEmail
		public bool SendConfirmEmail
		{
			get { return this.sendConfirmEmail; }
			set
			{
				this.sendConfirmEmail = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion
  
        #region SaveToSubfolder
        public bool SaveToSubfolder
        {
            get { return this.saveToSubfolder; }
            set
            {
                if (value != this.saveToSubfolder)
                {
                    this.saveToSubfolder = value;
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
            }
        }
        #endregion

        #region SubfolderNameIndex
        public int SubfolderNameIndex
        {
            get
            {
                switch (this.subfolderNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    this.subfolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn.TransactionName;
                else
                    this.subfolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn.IllName;

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

		#region IncludePullslip
		public bool IncludePullslip
		{
			get { return this.includePullslip; }
			set
			{
				this.includePullslip = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region GetAdditionalInfo()
		public BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo()
		{
            return new BscanILL.Export.AdditionalInfo.AdditionalFtpDir(this.FtpLink, this.ExportDirectory, this.SendConfirmEmail, this.SaveToSubfolder, this.subfolderNameBase ,
                this.FileNamePrefix, this.FileFormat, this.MultiImage, this.PdfA, this.IncludePullslip, this.UpdateILLiad, this.ChangeStatusToRequestFinished, this.FileColor, this.FileQuality);
		}
		#endregion

        #region LoadArticle()
        public override void LoadArticle(BscanILL.Hierarchy.Article article)
        {
            this.article = article;

            this.FileNamePrefix = GetExportFileNamePrefixPreferringTransactionNumber(article);            
            this.ExportDirectory = _settings.Export.FtpDirectory.ExportDirectoryPath;
            fileformatCtrl.FileFormat = _settings.Export.FtpDirectory.ExportFileFormat;
            this.fileparamCtrl.FileColor = _settings.Export.FtpDirectory.FileExportColorMode;
            this.fileparamCtrl.FileQuality = _settings.Export.FtpDirectory.FileExportQuality;
        }
        #endregion

        #region BrowseOutputPath()
        public void BrowseOutputPath(bool exportDialogEnabled, string tnValue, string illValue)
        {
            if (this.ExportDirectory.Length == 0)
            {
                transactionNumber = tnValue;
                illNumber = illValue;
                //need to raise dir browser to select root dir                
                Browse_Click(this.browseButton, new RoutedEventArgs());
                if (this.ExportDirectory.Length == 0)
                {
                    if (exportDialogEnabled)
                    {
                      MessageBox.Show("Root export directory not specified! Please use 'Browse...' button to select FTP Directory destination before exporting!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                transactionNumber = "";
                illNumber = "";
            }

        }
        #endregion

        #region FileFormat_SelectionChanged
        public void FileFormat_SelectionChanged()
        {
            if (fileformatCtrl.FileFormat == Scan.FileFormat.Pdf || fileformatCtrl.FileFormat == Scan.FileFormat.SPdf)
            {
                this.fileparamCtrl.comboFileColor.Visibility = System.Windows.Visibility.Visible;
                this.fileparamCtrl.textFileColor.Visibility = System.Windows.Visibility.Visible;

                FileColor_SelectionChanged();
            }
            else
            {
                this.fileparamCtrl.comboFileColor.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.textFileColor.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.comboFileQuality.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.textFileQuality.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        #endregion

        #region FileColor_SelectionChanged
        public void FileColor_SelectionChanged()
        {
            if (this.fileparamCtrl.FileColor != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
            {
                this.fileparamCtrl.comboFileQuality.Visibility = System.Windows.Visibility.Visible;
                this.fileparamCtrl.textFileQuality.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.fileparamCtrl.comboFileQuality.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.textFileQuality.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        #endregion

		#endregion

		
		//PRIVATE METHODS
		#region private methods

		#region Enabled_CheckedChanged()
		private void Enabled_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.groupBox.Visibility = (_settings.Export.FtpDirectory.Enabled) ? Visibility.Visible : Visibility.Hidden;
		}
		#endregion

		#region CheckUpdateILLiad_CheckedChanged()
		private void CheckUpdateILLiad_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.checkChangeStatusToRequestFinished.IsEnabled = checkUpdateILLiad.IsChecked.Value;
		}
		#endregion

		#region Browse_Click()
		private void Browse_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				System.Windows.Forms.FolderBrowserDialog browserDlg = new System.Windows.Forms.FolderBrowserDialog();

				try { browserDlg.SelectedPath = (BscanILL.Misc.Io.DirectoryExists(this.ExportDirectory)) ? this.ExportDirectory : @"c:\"; }
				catch { browserDlg.SelectedPath = @"c:\"; }

				browserDlg.ShowNewFolderButton = true;				
                if ((transactionNumber.Length == 0) && (illNumber.Length == 0))
                {
                    browserDlg.Description = "Please select directory where the article(s) will be stored when FTP Directory delivery.";
                }
                else
                {
                    browserDlg.Description = "Please select directory where the article (TN:" + transactionNumber + ", ILL#:" + illNumber + ") will be stored when FTP Directory delivery.";
                }

				if (browserDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					this.ExportDirectory = browserDlg.SelectedPath;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

        #region CheckSubfolder_CheckedChanged
        private void CheckSubfolder_CheckedChanged(object sender, RoutedEventArgs e)
        {
            comboSubfolderName.Visibility = (checkSaveToSubfolder.IsChecked.Value ? Visibility.Visible : System.Windows.Visibility.Hidden);
        }
        #endregion		

		#endregion


	}
}
