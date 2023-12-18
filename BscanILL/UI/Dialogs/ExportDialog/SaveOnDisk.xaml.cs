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
	public partial class SaveOnDisk : PanelBase, IPanel
	{
		string exportDir;
		bool includePullslip = true;
        bool saveToSubfolder;
        string transactionNumber = "";
        string illNumber = "";
        BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn subfolderNameBase;
                 
		#region constructor
		public SaveOnDisk()
		{
			InitializeComponent();

            this.saveToSubfolder = _settings.Export.SaveOnDisk.SaveToSubfolder;
            this.subfolderNameBase = _settings.Export.SaveOnDisk.SubFolderNameBase;

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
		public bool						MultiImage	{ get { return this.fileformatCtrl.MultiImage; } }
		public bool						PdfA		{ get { return this.fileformatCtrl.PdfA; } }        
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get { return this.fileparamCtrl.FileColor; } }
        public int                      FileQuality { get { return this.fileparamCtrl.FileQuality; } } 

		#region ExportDirPath
		public string ExportDirPath
		{
			get { return this.exportDir; }
			set
			{
				this.exportDir = value;
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
			get { return _settings.Export.SaveOnDisk.UpdateILLiad; }
			set
			{
				_settings.Export.SaveOnDisk.UpdateILLiad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ChangeStatusToRequestFinished
		public bool ChangeStatusToRequestFinished
		{
			get { return _settings.Export.SaveOnDisk.ChangeRequestToFinished; }
			set
			{
				_settings.Export.SaveOnDisk.ChangeRequestToFinished = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region BeforeActionIndex
		public int BeforeActionIndex
		{
			get
			{
				switch (_settings.Export.SaveOnDisk.BeforeExport)
				{
					case BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.ActionBeforeExport.CleanExportDir: return 1;
					default: return 0;
				}
			}
			set
			{
				if (value == 1)
					_settings.Export.SaveOnDisk.BeforeExport = BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.ActionBeforeExport.CleanExportDir;
				else
					_settings.Export.SaveOnDisk.BeforeExport = BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.ActionBeforeExport.KeepExistingFiles;

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
                    case BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    this.subfolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.TransactionName;
                else
                    this.subfolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.IllName;

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region LoadArticle()
		public override void LoadArticle(BscanILL.Hierarchy.Article article)
		{
			this.article = article;

            this.FileNamePrefix = GetExportFileNamePrefixPreferringTransactionNumber(article);
			//this.ExportDirPath = System.IO.Path.Combine(_settings.Export.SaveOnDisk.ExportDirPath, this.FileNamePrefix);
			this.ExportDirPath = _settings.Export.SaveOnDisk.ExportDirPath;
            fileformatCtrl.FileFormat = _settings.Export.SaveOnDisk.ExportFileFormat;
            this.fileparamCtrl.FileColor = _settings.Export.SaveOnDisk.FileExportColorMode;
            this.fileparamCtrl.FileQuality = _settings.Export.SaveOnDisk.FileExportQuality;
		}
		#endregion

        #region BrowseOutputPath()
        public void BrowseOutputPath(bool exportDialogEnabled, string tnValue, string illValue)
        {
            if (this.ExportDirPath.Length == 0)
            {
                transactionNumber = tnValue;
                illNumber = illValue;
                //need to raise dir browser to select root dir                
                Browse_Click(this.browseButton, new RoutedEventArgs());
                if (this.ExportDirPath.Length == 0)
                {
                    if (exportDialogEnabled)
                    {
                        MessageBox.Show("Root export directory not specified! Please use 'Browse...' button to select Save on Disk destination directory before exporting!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                transactionNumber = "";
                illNumber = "";
            }
/*
            if (this.ExportDirPath.Length != 0)
            if (_settings.Export.SaveOnDisk.SaveToSubfolder)
            {
                if ((_settings.Export.SaveOnDisk.SubFolderNameBase == BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.TransactionName) &&
                    (this.article.TransactionId != null))
                    this.ExportDirPath = System.IO.Path.Combine(this.ExportDirPath, this.article.TransactionId.ToString());
                else if ((_settings.Export.SaveOnDisk.SubFolderNameBase == BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.IllName) &&
                    (this.article.IllNumber != null && this.article.IllNumber.Trim().Length > 0))
                    this.ExportDirPath = System.IO.Path.Combine(this.ExportDirPath, this.article.IllNumber.ToString());
                else
                    this.ExportDirPath = System.IO.Path.Combine(this.ExportDirPath, this.article.Id.ToString());
            }			
*/
        }
        #endregion

        #region GetAdditionalInfo()
        public BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo()
		{
            return new BscanILL.Export.AdditionalInfo.AdditionalSaveOnDisk(this.ExportDirPath, this.SaveToSubfolder, this.subfolderNameBase, this.FileNamePrefix, this.FileFormat,
                this.MultiImage, this.PdfA, _settings.Export.SaveOnDisk.BeforeExport, this.IncludePullslip, this.UpdateILLiad, this.ChangeStatusToRequestFinished, this.FileColor, this.FileQuality);
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

		#region Browse_Click()
		private void Browse_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				System.Windows.Forms.FolderBrowserDialog browserDlg = new System.Windows.Forms.FolderBrowserDialog();

				try 
				{
					browserDlg.SelectedPath = (BscanILL.Misc.Io.DirectoryExists(this.ExportDirPath)) ? this.ExportDirPath : @"c:\";
				}
				catch { browserDlg.SelectedPath = @"c:\"; }

				browserDlg.ShowNewFolderButton = true;

                if( ( transactionNumber.Length == 0 ) && (illNumber.Length == 0) )
                {
				    browserDlg.Description = "Please select directory where the article(s) will be stored when Saving on Disk.";
                }
                else
                {
                    browserDlg.Description = "Please select directory where the article (TN:" + transactionNumber + ", ILL#:" + illNumber + ") will be stored when Saving on Disk.";
                }
                
				if (browserDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					this.ExportDirPath = browserDlg.SelectedPath;
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
