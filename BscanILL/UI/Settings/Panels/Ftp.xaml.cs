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

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for Ftp.xaml
	/// </summary>
	public partial class Ftp : PanelBase
	{
		
		#region constructor
		public Ftp()
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
			
			if (_settings != null)
				this.listView.DataContext = _settings.Export.FtpServer.FtpProfiles;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region FtpEnabled
		public bool FtpEnabled
		{
			get { return _settings.Export.FtpServer.Enabled; }
			set
			{
				_settings.Export.FtpServer.Enabled = value;
				this.groupBox.Visibility = (value) ? Visibility.Visible : Visibility.Hidden;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SendConfirmEmail
		public bool SendConfirmEmail
		{
			get { return _settings.Export.FtpServer.SendConfirmationEmail; }
			set
			{
				_settings.Export.FtpServer.SendConfirmationEmail = value;
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
					if (item.Value == _settings.Export.FtpServer.ExportFileFormat)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Export.FtpServer.ExportFileFormat = value.Value;
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
                    if (item == _settings.Export.FtpServer.FileExportColorMode)
                        return item;

                return BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;
            }
            set
            {
                _settings.Export.FtpServer.FileExportColorMode = value;

                if (_settings.Export.FtpServer.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
            get { return _settings.Export.FtpServer.FileExportQuality; }
            set
            {
                if (value != _settings.Export.FtpServer.FileExportQuality)
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
                    _settings.Export.FtpServer.FileExportQuality = value;
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
            }
        }
        #endregion

		#region UpdateILLiad
		public bool UpdateILLiad
		{
			get { return _settings.Export.FtpServer.UpdateILLiad; }
			set
			{
				_settings.Export.FtpServer.UpdateILLiad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ChangeStatusToRequestFinished
		public bool ChangeStatusToRequestFinished
		{
			get { return _settings.Export.FtpServer.ChangeRequestToFinished; }
			set
			{
				_settings.Export.FtpServer.ChangeRequestToFinished = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

        #region SaveToSubfolder
        public bool SaveToSubfolder
        {            
            get { return _settings.Export.FtpServer.SaveToSubfolder; }
            set
            {
                if (value != _settings.Export.FtpServer.SaveToSubfolder)
                {
                    _settings.Export.FtpServer.SaveToSubfolder = value;
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
                switch (_settings.Export.FtpServer.SubFolderNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.FtpServer.SubFolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn.TransactionName;
                else
                    _settings.Export.FtpServer.SubFolderNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn.IllName;

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region ExportNameIndex
        public int ExportNameIndex
        {
            get
            {
                switch (_settings.Export.FtpServer.ExportNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.FtpClass.ExportNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.FtpServer.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpClass.ExportNameBasedOn.TransactionName;
                else
                    _settings.Export.FtpServer.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.FtpClass.ExportNameBasedOn.IllName;

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

		#region AddServer_Click()
		private void AddServer_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				BscanILL.UI.Settings.Dialogs.FtpServerDlg dlg = new BscanILL.UI.Settings.Dialogs.FtpServerDlg();

				BscanILL.Export.FTP.FtpLogin ftpProfile = dlg.Open();

				if (ftpProfile != null)
					_settings.Export.FtpServer.FtpProfiles.Add(ftpProfile);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
			}
		}
		#endregion

		#region Edit_MouseDown()
		private void Edit_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is TextBlock)
			{
				DependencyObject visualObject = (TextBlock)sender;

				while (visualObject != null)
				{
					if (visualObject is ListViewItem)
					{
						if (((ListViewItem)visualObject).Content is BscanILL.Export.FTP.FtpLogin)
						{
							BscanILL.Export.FTP.FtpLogin				ftpProfile = (BscanILL.Export.FTP.FtpLogin)((ListViewItem)visualObject).Content;
							BscanILL.UI.Settings.Dialogs.FtpServerDlg dlg = new BscanILL.UI.Settings.Dialogs.FtpServerDlg();

							if(dlg.Open(ftpProfile))
							{
								int index = _settings.Export.FtpServer.FtpProfiles.IndexOf(ftpProfile);
								_settings.Export.FtpServer.FtpProfiles.Remove(ftpProfile);
								_settings.Export.FtpServer.FtpProfiles.Insert(index, ftpProfile);
							}
						}

						return;
					}
					else
					{
						visualObject = VisualTreeHelper.GetParent(visualObject);
					}
				}
			}
		}
		#endregion

		#region Delete_MouseDown()
		private void Delete_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is TextBlock)
			{
				DependencyObject visualObject = (TextBlock)sender;

				while (visualObject != null)
				{
					if (visualObject is ListViewItem)
					{
						if (((ListViewItem)visualObject).Content is BscanILL.Export.FTP.FtpLogin)
						{
							BscanILL.Export.FTP.FtpLogin ftpProfile = (BscanILL.Export.FTP.FtpLogin)((ListViewItem)visualObject).Content;

							if (MessageBox.Show("Are you sure you want to remove FTP server '" + ftpProfile.Name + "' from the list?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								_settings.Export.FtpServer.FtpProfiles.Remove(ftpProfile);
							}
						}
							
						return;
					}
					else
					{
						visualObject = VisualTreeHelper.GetParent(visualObject);
					}
				}
			}
		}
		#endregion

		#region CheckUpdateILLiad_CheckedChanged()
		private void CheckUpdateILLiad_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.checkChangeStatusToRequestFinished.IsEnabled = checkUpdateILLiad.IsChecked.Value;
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

            if (_settings.Export.FtpServer.ExportFileFormat == Scan.FileFormat.Pdf || _settings.Export.FtpServer.ExportFileFormat == Scan.FileFormat.SPdf)
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Visible;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Visible;

                if (_settings.Export.FtpServer.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
