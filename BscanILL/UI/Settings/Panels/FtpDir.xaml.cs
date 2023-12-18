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

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for Ftp.xaml
	/// </summary>
	public partial class FtpDir : PanelBase
	{
		
		#region constructor
		public FtpDir()
		{
			InitializeComponent();

			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Tiff));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Pdf));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.SPdf));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Jpeg));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Png));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Text));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Audio));

            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto);
            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Color);
            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale);
            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal);

            for (int i = 1; i <= 100; i++)
            {
                this.comboFileExportQuality.Items.Add(i);
            }

			groupBox.Visibility = System.Windows.Visibility.Hidden;
			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region FtpEnabled
		public bool FtpEnabled
		{
			get { return _settings.Export.FtpDirectory.Enabled; }
			set
			{
				_settings.Export.FtpDirectory.Enabled = value;
				this.groupBox.Visibility = (value) ? Visibility.Visible : Visibility.Hidden;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region FtpAddress
		public string FtpAddress
		{
			get { return _settings.Export.FtpDirectory.FtpAddress; }
			set
			{
				_settings.Export.FtpDirectory.FtpAddress = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ExportDirectory
		public string ExportDirectory
		{
			get { return _settings.Export.FtpDirectory.ExportDirectoryPath; }
			set
			{
				_settings.Export.FtpDirectory.ExportDirectoryPath = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SendConfirmEmail
		public bool SendConfirmEmail
		{
			get { return _settings.Export.FtpDirectory.SendConfirmationEmail; }
			set
			{
				_settings.Export.FtpDirectory.SendConfirmationEmail = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SaveToSubfolder
		public bool SaveToSubfolder
		{
			//get { return _settings.Export.FtpDirectory.UseSubfolder; }
            get { return _settings.Export.FtpDirectory.SaveToSubfolder; }            
			set
			{
                if (value != _settings.Export.FtpDirectory.SaveToSubfolder)
                {
                    _settings.Export.FtpDirectory.SaveToSubfolder = value;
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
                switch (_settings.Export.FtpDirectory.SubFolderNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.FtpDirectory.SubFolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn.TransactionName;
                else
                    _settings.Export.FtpDirectory.SubFolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn.IllName;

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region ExportNameIndex
        public int ExportNameIndex
        {
            get
            {
                switch (_settings.Export.FtpDirectory.ExportNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.ExportNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.FtpDirectory.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.ExportNameBasedOn.TransactionName;
                else
                    _settings.Export.FtpDirectory.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.ExportNameBasedOn.IllName;

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

		#region FileFormatSelectedItem
		public ComboItemFileFormat FileFormatSelectedItem
		{
			get
			{
				foreach (ComboItemFileFormat item in comboFileFormat.Items)
					if (item.Value == _settings.Export.FtpDirectory.ExportFileFormat)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Export.FtpDirectory.ExportFileFormat = value.Value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

        #region FileExportColorModeSelectedItem
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileExportColorModeSelectedItem
        {
            get
            {
                foreach (BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth item in comboFileExportColorMode.Items)
                    if (item == _settings.Export.FtpDirectory.FileExportColorMode)
                        return item;

                return BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;
            }
            set
            {
                _settings.Export.FtpDirectory.FileExportColorMode = value;

                if (_settings.Export.FtpDirectory.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
                {                    
                    this.comboFileExportQuality.Visibility = System.Windows.Visibility.Visible;
                    this.textFileExportQuality.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.comboFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                    this.textFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                }
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion 

        #region FileExportQuality
        public int FileExportQuality
        {
            get { return _settings.Export.FtpDirectory.FileExportQuality; }
            set
            {
                if (value != _settings.Export.FtpDirectory.FileExportQuality)
                {
                    if (value < 1)
                    {
                        value = 1;
                    }
                    else
                        if (value > 100)
                        {
                            value = 100;
                        }
                    _settings.Export.FtpDirectory.FileExportQuality = value;
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
            }
        }
        #endregion

		#region UpdateILLiad
		public bool UpdateILLiad
		{
			get { return _settings.Export.FtpDirectory.UpdateILLiad; }
			set
			{
				_settings.Export.FtpDirectory.UpdateILLiad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ChangeStatusToRequestFinished
		public bool ChangeStatusToRequestFinished
		{
			get { return _settings.Export.FtpDirectory.ChangeRequestToFinished; }
			set
			{
				_settings.Export.FtpDirectory.ChangeRequestToFinished = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods
		#endregion

	
		//PRIVATE METHODS
		#region private methods

		#region Enabled_CheckedChanged()
		private void Enabled_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.groupBox.Visibility = (this.FtpEnabled) ? Visibility.Visible : Visibility.Hidden;
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
                browserDlg.Description = "Please select directory where the articles will be stored when FTP Directory delivery.";

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

        #region ComboFileFormat_SelectionChanged()
        private void ComboFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if(comboFileFormat.SelectedItem is ComboBoxItem)

            if (_settings.Export.FtpDirectory.ExportFileFormat == Scan.FileFormat.Pdf || _settings.Export.FtpDirectory.ExportFileFormat == Scan.FileFormat.SPdf)
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Visible;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Visible;

                if (_settings.Export.FtpDirectory.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
                {
                    this.comboFileExportQuality.Visibility = System.Windows.Visibility.Visible;
                    this.textFileExportQuality.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.comboFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                    this.textFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            else
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Hidden;
                this.comboFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Hidden;
                this.textFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
            }

        }
        #endregion

		#endregion


	}
}
