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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Printing;
using System.IO;

namespace BscanILL.UI.Settings.Dialogs
{
	/// <summary>
	/// Interaction logic for LicenseRequestDlg.xaml
	/// </summary>
	public partial class LicenseRequestDlg : Window
	{
		public delegate bool RequestHnd(string institution, string bscanILLSite, string scannerSn);

    SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn emailDeliveryType = SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.HTTP;

		#region constructor
		private LicenseRequestDlg()
		{
			InitializeComponent();

			this.DataContext = this;
		}

		public LicenseRequestDlg(string message, string scannerSn, SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn emailDelivType )
			:this()
		{
			this.textMessage.Text = message;
			this.textBlockScannerSn.Text = scannerSn;
            emailDeliveryType = emailDelivType;			
		}
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region TextBox_TextChanged()
		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			bool enable = true;

			if (textBoxInstitution.Text.Length == 0)
				enable = false;
			else if (textBoxBscanIllSiteName.Text.Length == 0)
				enable = false;
			else if (textBoxRequestorName.Text.Length == 0)
				enable = false;
			else if (IsValidPhoneNumber(textBoxRequestorPhone.Text) == false && BscanILL.Export.Email.Email.IsValidEmail(textBoxRequestorEmail.Text) == false)
				enable = false;

			this.buttonEmail.IsEnabled = enable;
			this.buttonSaveOnDisk.IsEnabled = enable;
		}
		#endregion

		#region Email_Click()
		private void Email_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				string institution = this.textBoxInstitution.Text;
				string bscanILLSite = this.textBoxBscanIllSiteName.Text;
				string requestorName = this.textBoxRequestorName.Text;
				string requestorPhone = this.textBoxRequestorPhone.Text;
				string requestorEmail = this.textBoxRequestorEmail.Text;
				string scannerSn = this.textBlockScannerSn.Text;

				string body = "License File Request" + Environment.NewLine + Environment.NewLine;

				body += "Institution: " + this.textBoxInstitution.Text + Environment.NewLine;
				body += "Bscan ILL Site: " + bscanILLSite + Environment.NewLine;
				body += "Requestor's Name: " + requestorName + Environment.NewLine;
				body += "Requestor's Phone: " + requestorPhone + Environment.NewLine;
				body += "Requestor's Email: " + requestorEmail + Environment.NewLine;
				body += "Scanner SN: " + scannerSn + Environment.NewLine;

				FileInfo file = new FileInfo(BscanILL.SETTINGS.Settings.Instance.General.LicenseDir + @"\LicenseFileRequest.xml");
				file.Directory.Create();

				CreateLicenseFile(file.FullName, "License File Request", institution, bscanILLSite, requestorName, requestorPhone, requestorEmail, scannerSn);
				
                if (emailDeliveryType != SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP)
                {                    
                    string result = "";
                    string recipient = "";
                    string from = "";

                    BscanILL.Export.Email.DlsgEmail.Instance.GetMailMessageParameters(BscanILL.Export.Email.DlsgEmail.SendTo.Admins, ref recipient, ref from);

                    KssFolderAPIClientNS.KssFolderAPIClient client = null;
                    client = new KssFolderAPIClientNS.KssFolderAPIClient(BscanILL.Export.Email.Email.LiveBasePath);

                    KssFolderAPIClientNS.EmailMessage message = new KssFolderAPIClientNS.EmailMessage(recipient, "ILL-Email@KICService.com", "License File Request", body);
                    
                    message.AddAttachment(new KssFolderAPIClientNS.Attachment(file.FullName));
                    
                    message.SMTPSenderID = "DLSG-ILL-Email@KICService.com";
                    message.SMTPSenderPassphrase = "G$h#296&";
                    //if (_settings.Export.Email.From.Length > 0)
                    if (from.Length > 0)
                    {
                        message.ReplyTo = from;
                    }
                    
                    result = client.KicSendEmailEx(message);

                    if (String.Compare(result, "Message Sent", false) != 0)
                    {
                        if (emailDeliveryType == SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both)
                        {
                            //try also SMTP
                            BscanILL.Export.Email.DlsgEmail.Instance.SendEmail(BscanILL.Export.Email.DlsgEmail.SendTo.Admins, "License File Request", body, file);
                        }
                        else
                        {
                            throw new Exception("Sending license file request failed.");
                        }
                    }
                }
                else
                {
                    BscanILL.Export.Email.DlsgEmail.Instance.SendEmail(BscanILL.Export.Email.DlsgEmail.SendTo.Admins, "License File Request", body, file);
                }
                				
				MessageBox.Show("License Request File was successfully sent to DLSG. The request will be processed as soon as possible. Try downloading the license file again later.");

				this.DialogResult = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region SaveOnDisk_Click()
		private void SaveOnDisk_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				string institution = this.textBoxInstitution.Text;
				string bscanILLSite = this.textBoxBscanIllSiteName.Text;
				string requestorName = this.textBoxRequestorName.Text;
				string requestorPhone = this.textBoxRequestorPhone.Text;
				string requestorEmail = this.textBoxRequestorEmail.Text;
				string scannerSn = this.textBlockScannerSn.Text;

				Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

				dlg.InitialDirectory = @"c:\temp";
				dlg.FileName = @"LicenseFileRequest.xml"; // Default file name
				dlg.DefaultExt = ".xml"; // Default file extension
				dlg.Filter = "Text documents (.xml)|*.xml"; // Filter files by extension

				Nullable<bool> result = dlg.ShowDialog();

				if (result == true)
				{
					// Save document
					string filePath = dlg.FileName;

					CreateLicenseFile(filePath, "License File Request", institution, bscanILLSite, requestorName, requestorPhone, requestorEmail, scannerSn);
					MessageBox.Show("License Request File was successfully saved. Please email the file '" + filePath + "'to BscanILL-Admins@Dlsg.net.");
					
					this.DialogResult = true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region CreateLicenseFile()
		void CreateLicenseFile(string filePath, string message, string institution, string site, string requestorName, string phone, string email, string scannerSn)
		{
			BscanILL.Misc.Licensing.LicenseFileRequest requestFile = new BscanILL.Misc.Licensing.LicenseFileRequest(message, institution, site, requestorName, phone, email, scannerSn);

			using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(BscanILL.Misc.Licensing.LicenseFileRequest));
				xmlSerializer.Serialize(fileStream, requestFile);
			}
		}
		#endregion

		#region IsValidPhoneNumber()
		bool IsValidPhoneNumber(string number)
		{
			int digits = 0;

			foreach (char ch in number)
				if (char.IsDigit(ch))
					digits++;

			return (digits >= 10);
		}
		#endregion

		#endregion

	}
}
