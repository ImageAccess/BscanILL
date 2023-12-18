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
	public partial class Ftp : PanelBase, IPanel
	{
		bool sendConfirmEmail;
		bool updateIlliad;
		bool changeStatusToRequestFinished;
		bool includePullslip = true;
        bool saveToSubfolder;
        BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn subfolderNameBase;
		
		//ObservableCollection<BscanILL.Export.FTP.FtpLogin> ftpProfiles = new ObservableCollection<BscanILL.Export.FTP.FtpLogin>();

		
		#region constructor
		public Ftp()
		{
			InitializeComponent();

			this.sendConfirmEmail = _settings.Export.FtpServer.SendConfirmationEmail;
			this.updateIlliad = _settings.Export.FtpServer.UpdateILLiad;
			this.changeStatusToRequestFinished = _settings.Export.FtpServer.ChangeRequestToFinished;
            this.saveToSubfolder = _settings.Export.FtpDirectory.SaveToSubfolder;
            this.subfolderNameBase = _settings.Export.FtpDirectory.SubFolderNameBase;
			
			this.comboProfiles.ItemsSource = _settings.Export.FtpServer.FtpProfiles;

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

		public BscanILL.Scan.FileFormat		FileFormat	{ get { return this.fileformatCtrl.FileFormat; } }
		public bool							MultiImage	{ get { return this.fileformatCtrl.MultiImage; } }
		public bool							PdfA		{ get { return this.fileformatCtrl.PdfA; } }
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get { return this.fileparamCtrl.FileColor; } }
        public int FileQuality { get { return this.fileparamCtrl.FileQuality; } } 

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
                    case BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    this.subfolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn.TransactionName;
                else
                    this.subfolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn.IllName;

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
            return new BscanILL.Export.AdditionalInfo.AdditionalFtp(ftpLoginControl.FtpLogin, this.SendConfirmEmail, this.SaveToSubfolder, this.subfolderNameBase, this.FileNamePrefix,
                this.FileFormat, this.MultiImage, this.PdfA, this.IncludePullslip, this.UpdateILLiad, this.ChangeStatusToRequestFinished, this.FileColor, this.FileQuality);
		}
		#endregion

        #region LoadArticle()
        public override void LoadArticle(BscanILL.Hierarchy.Article article)
        {
            this.article = article;

            this.FileNamePrefix = GetExportFileNamePrefixPreferringTransactionNumber(article);
            fileformatCtrl.FileFormat = _settings.Export.FtpServer.ExportFileFormat;
            this.fileparamCtrl.FileColor = _settings.Export.FtpServer.FileExportColorMode;
            this.fileparamCtrl.FileQuality = _settings.Export.FtpServer.FileExportQuality;
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

		#region CheckUpdateILLiad_CheckedChanged()
		private void CheckUpdateILLiad_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.checkChangeStatusToRequestFinished.IsEnabled = checkUpdateILLiad.IsChecked.Value;
		}
		#endregion

		#region ComboFtpLogin_SelectionChanged()
		private void ComboFtpLogin_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (comboProfiles.SelectedItem != null && comboProfiles.SelectedItem is BscanILL.Export.FTP.FtpLogin)
				ftpLoginControl.Load((BscanILL.Export.FTP.FtpLogin)comboProfiles.SelectedItem);
			else
				ftpLoginControl.Clear();
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
