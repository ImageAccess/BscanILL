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

namespace BscanILL.UI.Dialogs.ExportDialog
{
	/// <summary>
	/// Interaction logic for FtpLoginControl.xaml
	/// </summary>
	public partial class FtpLoginControl : UserControl
	{
		public event BscanILL.Misc.VoidHnd		TextChanged;


		#region constructor
		public FtpLoginControl()
		{
			InitializeComponent();
			
			this.DataContext = this;

			this.Port = 21;
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties
		public ushort Port { get; set; }

		public BscanILL.Export.FTP.FtpLogin FtpLogin
		{
			get
			{
				FTPClientConnector.FTPType ftpType = FTPClientConnector.FTPType.Ftp;

				switch (this.comboEncryption.SelectedIndex)
				{
					case 1: ftpType = FTPClientConnector.FTPType.FtpSSL; break;
					case 2: ftpType = FTPClientConnector.FTPType.FtpTLS; break;
					case 3: ftpType = FTPClientConnector.FTPType.FtpSSH; break;
				}

				return new BscanILL.Export.FTP.FtpLogin(this.textDescription.Text, this.textServer.Text, this.textDirectory.Text, this.Port,
					ftpType, this.textUsername.Text, textPassword.SecurePassword);
			}
		}

		public bool IsValidationOk
		{
			get
			{
				int port;
				
				return (textDescription.Text.Trim().Length > 0 && textServer.Text.Trim().Length > 0 &&
					int.TryParse(textPort.Text, out port) && this.textUsername.Text.Trim().Length > 0 && this.textPassword.SecurePassword.Length > 0);
			}
		}

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Clear()
		public void Clear()
		{
			this.textDescription.Text = "";
			this.textServer.Text = "";
			this.textDirectory.Text = "";
			this.Port = 21;
			this.textUsername.Text = "";
			this.textPassword.Password = "";
			this.comboEncryption.SelectedIndex = 0;
		}
		#endregion

		#region Load()
		public void Load(BscanILL.Export.FTP.FtpLogin ftpLogin)
		{
			this.textDescription.Text = ftpLogin.Name;
			this.textServer.Text = ftpLogin.Server;
			this.textDirectory.Text = ftpLogin.Directory;
			this.Port = ftpLogin.Port;
			this.textUsername.Text = ftpLogin.Username;
			this.textPassword.Password = BscanILL.Misc.DataProtector.GetString(ftpLogin.Password);
			
			switch (ftpLogin.FtpType)
			{
				case FTPClientConnector.FTPType.FtpSSL: this.comboEncryption.SelectedIndex = 1; break;
				case FTPClientConnector.FTPType.FtpTLS: this.comboEncryption.SelectedIndex = 2; break;
				case FTPClientConnector.FTPType.FtpSSH: this.comboEncryption.SelectedIndex = 3; break;
				default: this.comboEncryption.SelectedIndex = 0; break;
			}
		}
		#endregion

		#region CopyTo()
		public void CopyTo(BscanILL.Export.FTP.FtpLogin ftpLogin)
		{
			ftpLogin.Name = this.textDescription.Text;
			ftpLogin.Server = this.textServer.Text;
			ftpLogin.Directory = this.textDirectory.Text;
			ftpLogin.Port = this.Port;

			switch (this.comboEncryption.SelectedIndex)
			{
				case 1: ftpLogin.FtpType = FTPClientConnector.FTPType.FtpSSL; break;
				case 2: ftpLogin.FtpType = FTPClientConnector.FTPType.FtpTLS; break;
				case 3: ftpLogin.FtpType = FTPClientConnector.FTPType.FtpSSH; break;
				default: ftpLogin.FtpType = FTPClientConnector.FTPType.Ftp; break;
			}

			ftpLogin.Username = this.textUsername.Text;
			ftpLogin.Password = this.textPassword.SecurePassword;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Text_Changed()
		private void Text_Changed(object sender, TextChangedEventArgs e)
		{
			if (TextChanged != null)
				TextChanged();
		}
		#endregion

		#region Text_Changed()
		private void Text_Changed(object sender, RoutedEventArgs e)
		{
			if (TextChanged != null)
				TextChanged();
		}
		#endregion

		#endregion
}
}
