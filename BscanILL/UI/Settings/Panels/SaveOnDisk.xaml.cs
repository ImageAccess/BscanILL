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
	public partial class SaveOnDisk : PanelBase
	{
		
		#region constructor
		public SaveOnDisk()
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
            
            for (int i = 1; i <= 100; i++ )
            {
                this.comboFileExportQuality.Items.Add(i);
            }

			groupBox.Visibility = System.Windows.Visibility.Hidden;
			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Enabled
		public bool Enabled
		{
			get { return _settings.Export.SaveOnDisk.Enabled; }
			set
			{
				_settings.Export.SaveOnDisk.Enabled = value;
				this.groupBox.Visibility = (value) ? Visibility.Visible : Visibility.Hidden;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ExportDirPath
		public string ExportDirPath
		{
			get { return _settings.Export.SaveOnDisk.ExportDirPath; }
			set
			{
				_settings.Export.SaveOnDisk.ExportDirPath = value;
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
					if (item.Value == _settings.Export.SaveOnDisk.ExportFileFormat)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Export.SaveOnDisk.ExportFileFormat = value.Value;

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
                    if (item == _settings.Export.SaveOnDisk.FileExportColorMode)
                        return item;

                return BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;
            }
            set
            {
                    _settings.Export.SaveOnDisk.FileExportColorMode = value;

                    if (_settings.Export.SaveOnDisk.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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

        #region FileExportQuality
        public int FileExportQuality
		{
            get { return _settings.Export.SaveOnDisk.FileExportQuality; }
			set
			{
                if (value != _settings.Export.SaveOnDisk.FileExportQuality)
				{
                    if(value < 1)
                    {
                        value = 1;
                    }
                    else
                        if (value > 100)
                        {
                            value = 100;
                        }
                    _settings.Export.SaveOnDisk.FileExportQuality = value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#region SaveToSubfolder
		public bool SaveToSubfolder
		{
			get { return _settings.Export.SaveOnDisk.SaveToSubfolder; }
			set
			{
				if (value != _settings.Export.SaveOnDisk.SaveToSubfolder)
				{
					_settings.Export.SaveOnDisk.SaveToSubfolder = value;
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
				switch (_settings.Export.SaveOnDisk.SubFolderNameBase)
				{
					case BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.TransactionName: return 1;
					default: return 0;
				}
			}
			set
			{
				if (value == 1)
					_settings.Export.SaveOnDisk.SubFolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.TransactionName;
				else
					_settings.Export.SaveOnDisk.SubFolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.IllName;

				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

        #region ExportNameIndex
        public int ExportNameIndex
        {
            get
            {
                switch (_settings.Export.SaveOnDisk.ExportNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.ExportNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.SaveOnDisk.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.ExportNameBasedOn.TransactionName;
                else
                    _settings.Export.SaveOnDisk.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.ExportNameBasedOn.IllName;

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

		#endregion


		//PUBLIC METHODS
		#region public methods
		#endregion
	
		
		//PRIVATE METHODS
		#region private methods

		#region Enabled_CheckedChanged()
		private void Enabled_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.groupBox.Visibility = (_settings.Export.SaveOnDisk.Enabled) ? Visibility.Visible : Visibility.Hidden;
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

				try { browserDlg.SelectedPath = (BscanILL.Misc.Io.DirectoryExists(this.ExportDirPath)) ? this.ExportDirPath : @"c:\"; }
				catch { browserDlg.SelectedPath = @"c:\"; }

				browserDlg.ShowNewFolderButton = true;
				browserDlg.Description = "Please select directory where the articles will be stored when Saving on Disk.";

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
			comboSubfolderName.Visibility = (checkSubfolder.IsChecked.Value ? Visibility.Visible : System.Windows.Visibility.Hidden);
		}
		#endregion

        #region ComboFileFormat_SelectionChanged()
        private void ComboFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            //if(comboFileFormat.SelectedItem is ComboBoxItem)

            if (_settings.Export.SaveOnDisk.ExportFileFormat == Scan.FileFormat.Pdf || _settings.Export.SaveOnDisk.ExportFileFormat == Scan.FileFormat.SPdf)
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Visible;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Visible;

                if (_settings.Export.SaveOnDisk.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
