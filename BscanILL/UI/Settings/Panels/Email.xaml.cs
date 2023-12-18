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
using System.Reflection;

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for Email.xaml
	/// </summary>
	public partial class Email : PanelBase
	{
		
		public delegate void	SendTestEmailRequestHnd(bool sendDataAttachment);
		public event			SendTestEmailRequestHnd SendTestEmailRequest;

        public delegate void    ValidateTestEmailAddressRequestHnd();
        public event            ValidateTestEmailAddressRequestHnd ValidateTestEmailAddressRequest;

		#region Email()
		public Email()
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

		#region EmailEnabled
		public bool EmailEnabled
		{
			get { return _settings.Export.Email.Enabled; }
			set
			{
				_settings.Export.Email.Enabled = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SmtpServer
		public string SmtpServer
		{
			get { return _settings.Export.Email.SmtpServer; }
			set
			{
				_settings.Export.Email.SmtpServer = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Port
		public ushort Port
		{
			get { return _settings.Export.Email.Port; }
			set
			{
				_settings.Export.Email.Port = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region DefaultCredentials
		public bool DefaultCredentials
		{
			get { return _settings.Export.Email.DefaultCredentials; }
			set
			{
				_settings.Export.Email.DefaultCredentials = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Username
		public string Username
		{
			get { return _settings.Export.Email.Username; }
			set
			{
				_settings.Export.Email.Username = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Password
		public string Password
		{
			get { return _settings.Export.Email.Password; }
			set
			{
				_settings.Export.Email.Password = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region DefaultEmailAddress
		public string DefaultEmailAddress
		{
			get { return _settings.Export.Email.From; }
			set
			{
				_settings.Export.Email.From = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region EmailSizeLimit
		public uint EmailSizeLimit
		{
			get { return (uint)_settings.Export.Email.SizeLimitInMB; }
			set
			{
				_settings.Export.Email.SizeLimitInMB = (int)value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SslEncryption
		public bool SslEncryption
		{
			get { return _settings.Export.Email.SslEncryption; }
			set
			{
				_settings.Export.Email.SslEncryption = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion
		
        #region EmailValidation
        public bool EmailValidation
		{
            get { return _settings.Export.Email.EmailValidation; }
			set
			{
                _settings.Export.Email.EmailValidation = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion		

		#region ExportEnabled
		public bool ExportEnabled
		{
			get { return _settings.Export.Email.ExportEnabled; }
			set
			{
				_settings.Export.Email.ExportEnabled = value;
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
					if (item.Value == _settings.Export.Email.FileFormat)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Export.Email.FileFormat = value.Value;
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
                    if (item == _settings.Export.Email.FileExportColorMode)
                        return item;

                return BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;
            }
            set
            {
                _settings.Export.Email.FileExportColorMode = value;

                if (_settings.Export.Email.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
            get { return _settings.Export.Email.FileExportQuality; }
            set
            {
                if (value != _settings.Export.Email.FileExportQuality)
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
                    _settings.Export.Email.FileExportQuality = value;
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
            }
        }
        #endregion

        #region ExportNameIndex
        public int ExportNameIndex
        {
            get
            {
                switch (_settings.Export.Email.ExportNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.EmailClass.ExportNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.Email.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.EmailClass.ExportNameBasedOn.TransactionName;
                else
                    _settings.Export.Email.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.EmailClass.ExportNameBasedOn.IllName;

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region  EmailDeliveryIndex
        public int EmailDeliveryIndex
        {
            get
            {
                switch (_settings.Export.Email.EmailDeliveryType)  
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both: return 2;       //try first HTTP, if validation fails ->try smtp method
                    case BscanILL.SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP: return 1;
                    default: return 0;   //HTTP
                }
            }
            set
            {
                if (value == 2)
                {
                    _settings.Export.Email.EmailDeliveryType = BscanILL.SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both;
                }
                else if (value == 1)
                {
                    _settings.Export.Email.EmailDeliveryType = BscanILL.SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP;
                }
                else
                {
                    _settings.Export.Email.EmailDeliveryType = BscanILL.SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.HTTP;
                }

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
	
		#region ExportMessageSubject
		public string ExportMessageSubject
		{
			get { return _settings.Export.Email.Subject; }
			set
			{
				_settings.Export.Email.Subject = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ExportMessageBody
		public string ExportMessageBody
		{
			get { return _settings.Export.Email.Body; }
			set
			{
				_settings.Export.Email.Body = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region UpdateILLiad
		public bool UpdateILLiad
		{
			get { return _settings.Export.Email.UpdateILLiad; }
			set
			{
				_settings.Export.Email.UpdateILLiad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ChangeStatusToRequestFinished
		public bool ChangeStatusToRequestFinished
		{
			get { return _settings.Export.Email.ChangeRequestToFinished; }
			set
			{
				_settings.Export.Email.ChangeRequestToFinished = value;
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
			this.groupBox.Visibility = (this.EmailEnabled) ? Visibility.Visible : Visibility.Hidden;
		}
		#endregion

		#region WindowsCredentials_CheckedChanged()
		private void WindowsCredentials_CheckedChanged(object sender, RoutedEventArgs e)
		{
			gridWindowsCredentials.IsEnabled = (checkWindowsCredentials.IsChecked == false);
		}
		#endregion

		#region ExportEnabled_CheckedChanged()
		private void ExportEnabled_CheckedChanged(object sender, RoutedEventArgs e)
		{
            //this.gridExport.IsEnabled = checkExportEnabled.IsChecked.Value;
            this.gridExport.IsEnabled = true;   //checkbox to enable was removed from dialog as it was duplicit to checkbox checkBoxEnabled
		}
		#endregion

		#region SendTestEmail_Click()
		private void SendTestEmail_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (SendTestEmailRequest != null)
					SendTestEmailRequest(this.checkSendData.IsChecked.Value);
				else
					MessageBox.Show("SendTestEmailRequest is not hooked up!", "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

        #region ValidateDfltEmail_Click()
        private void ValidateDfltEmail_Click(object sender, RoutedEventArgs e)
		{
			try
			{
                if (ValidateTestEmailAddressRequest != null)
                    ValidateTestEmailAddressRequest();
				else
                    MessageBox.Show("ValidateDfltEmailRequest is not hooked up!", "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region CheckUpdateILLiad_CheckedChanged()
		private void CheckUpdateILLiad_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.checkChangeStatusToRequestFinished.IsEnabled = checkUpdateILLiad.IsChecked.Value;
		}
		#endregion

        #region ComboFileFormat_SelectionChanged()
        private void ComboFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if(comboFileFormat.SelectedItem is ComboBoxItem)

            if (_settings.Export.Email.FileFormat == Scan.FileFormat.Pdf || _settings.Export.Email.FileFormat == Scan.FileFormat.SPdf)
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Visible;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Visible;

                if (_settings.Export.Email.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
        
        #region ComboEmailDeliveryType_SelectionChanged()
        private void ComboEmailDeliveryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_settings.Export.Email.EmailDeliveryType == BscanILL.SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP )
            {
                this.gridSMTPSettings.Visibility = System.Windows.Visibility.Visible;                
            }
            else
            {
                this.gridSMTPSettings.Visibility = System.Windows.Visibility.Hidden;                
            }
        }
        #endregion        

		#endregion

	}
}
