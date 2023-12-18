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
using System.Security;

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for Ftp.xaml
	/// </summary>
	public partial class ArticleExchange : PanelBase
	{
		
		#region constructor
		public ArticleExchange()
		{
			InitializeComponent();

            this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Tiff));
            this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Pdf));
            this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.SPdf));
            //this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Jpeg));
            //this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Png));
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

			if (_settings != null)
			{
				this.passwordBoxPassword.Password = BscanILL.Misc.DataProtector.GetString(_settings.Export.ArticleExchange.Password);
			}

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region ArticleExchangeEnabled
		public bool ArticleExchangeEnabled
		{
			get { return _settings.Export.ArticleExchange.Enabled; }
			set
			{
				_settings.Export.ArticleExchange.Enabled = value;
				this.groupBox.Visibility = (value) ? Visibility.Visible : Visibility.Hidden;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Autho
		public string Autho
		{
			get { return _settings.Export.ArticleExchange.Autho; }
			set
			{
				_settings.Export.ArticleExchange.Autho = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
        #endregion

        #region ConfirmEmailAddress
        public string ConfirmEmailAddress
        {
            get { return _settings.Export.ArticleExchange.ConfirmationEmailAddress; }
            set
            {
                _settings.Export.ArticleExchange.ConfirmationEmailAddress = value;
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }

        }
        #endregion

        #region ResetConfirmEmailAddress
        public bool ResetConfirmEmailAddress
		{
            get { return _settings.Export.ArticleExchange.ResetConfirmEmailAddress; }
			set
			{
                _settings.Export.ArticleExchange.ResetConfirmEmailAddress = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

        #region ConfirmationRecipients
        public ObservableCollection<string> ConfirmationRecipients
		{
			get { return _settings.Export.ArticleExchange.ConfirmationRecipients; }
			set
			{
				_settings.Export.ArticleExchange.ConfirmationRecipients = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region UpdateILLiad
		public bool UpdateILLiad
		{
            get { return _settings.Export.ArticleExchange.UpdateILLiad; }
			set
			{
				_settings.Export.ArticleExchange.UpdateILLiad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ChangeStatusToRequestFinished
		public bool ChangeStatusToRequestFinished
		{
			get { return _settings.Export.ArticleExchange.ChangeRequestToFinished; }
			set
			{
				_settings.Export.ArticleExchange.ChangeRequestToFinished = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ExportMessageSubject
		public string ExportMessageSubject
		{
			get { return _settings.Export.ArticleExchange.Subject; }
			set
			{
				_settings.Export.ArticleExchange.Subject = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ExportMessageBody
		public string ExportMessageBody
		{
			get { return _settings.Export.ArticleExchange.Body; }
			set
			{
				_settings.Export.ArticleExchange.Body = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

        #region ExportNameIndex
        public int ExportNameIndex
        {
            get
            {
                switch (_settings.Export.ArticleExchange.ExportNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.ArticleExchangeClass.ExportNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.ArticleExchange.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.ArticleExchangeClass.ExportNameBasedOn.TransactionName;
                else
                    _settings.Export.ArticleExchange.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.ArticleExchangeClass.ExportNameBasedOn.IllName;

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
                    if (item.Value == _settings.Export.ArticleExchange.ExportFileFormat)
                        return item;

                return null;
            }
            set
            {
                if (value != null)
                {
                    _settings.Export.ArticleExchange.ExportFileFormat = value.Value;
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
                    if (item == _settings.Export.ArticleExchange.FileExportColorMode)
                        return item;

                return BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;
            }
            set
            {
                _settings.Export.ArticleExchange.FileExportColorMode = value;

                if (_settings.Export.ArticleExchange.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
            get { return _settings.Export.ArticleExchange.FileExportQuality; }
            set
            {
                if (value != _settings.Export.ArticleExchange.FileExportQuality)
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
                    _settings.Export.ArticleExchange.FileExportQuality = value;
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
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
			this.groupBox.Visibility = (this.ArticleExchangeEnabled) ? Visibility.Visible : Visibility.Hidden;
		}
		#endregion

		#region PasswordBoxPassword_PasswordChanged()
		private void PasswordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			_settings.Export.ArticleExchange.Password = this.passwordBoxPassword.SecurePassword;
		}
		#endregion

		#region CheckUpdateILLiad_CheckedChanged()
		private void CheckUpdateILLiad_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.checkChangeStatusToRequestFinished.IsEnabled = checkUpdateILLiad.IsChecked.Value;
		}
		#endregion

		#region Edit_Click()
		private void Edit_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILL.UI.Settings.Dialogs.ArticleExchangeRecipientsDlg dlg = new Dialogs.ArticleExchangeRecipientsDlg();

				if (dlg.Open(this.ConfirmationRecipients))
				{
					_settings.Export.ArticleExchange.ConfirmationRecipients.Clear();

					foreach (string item in dlg.Items)
						_settings.Export.ArticleExchange.ConfirmationRecipients.Add(item);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
        #endregion

        #region SetDefault_Click()
        private void SetDefault_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxRecipients.SelectedValue != null)
            {
                ConfirmEmailAddress = listBoxRecipients.SelectedValue.ToString();
            }
            else
            {
                MessageBox.Show("Select Default E-mail Address From Recepients List!", "Bscan ILL", MessageBoxButton.OK);
            }
        }
        #endregion

        #region ComboFileFormat_SelectionChanged()
        private void ComboFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if(comboFileFormat.SelectedItem is ComboBoxItem)

            if (_settings.Export.ArticleExchange.ExportFileFormat == Scan.FileFormat.Pdf || _settings.Export.ArticleExchange.ExportFileFormat == Scan.FileFormat.SPdf)
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Visible;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Visible;

                if (_settings.Export.ArticleExchange.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
