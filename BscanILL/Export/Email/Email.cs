using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;

using System.Net.Mail;
using System.Configuration;
using System.Threading;
using System.Collections.Generic;
using System.Windows;
using BscanILL.Misc;
using BscanILL.UI.Dialogs;

namespace BscanILL.Export.Email
{
	public class Email : ExportBasics, IDisposable
	{
		static Email	instance = new Email();
		private const string liveBasePath = @"https://q8x.us/Api/";

		SmtpClient		smtpClient;


		#region constructor
		private Email()
		{
			//SMTP SERVER
			try
			{
				_settings.Export.Email.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Settings_Changed);
				
				Refresh();
			}
			catch (Exception ex)
			{
				throw new IllException("Can't initialize Email instance!" + Environment.NewLine + ex.Message);
			}
		}

		private Email(BscanILL.SETTINGS.Settings.ExportClass.EmailClass emailSettings)
		{
			//SMTP SERVER
			try
			{
				this.smtpClient = new SmtpClient(emailSettings.SmtpServer);
				this.smtpClient.UseDefaultCredentials = emailSettings.DefaultCredentials;
				this.smtpClient.EnableSsl = emailSettings.SslEncryption;
				this.smtpClient.Port = emailSettings.Port;
				this.smtpClient.Timeout = 5 * 60 * 1000;

				if (this.smtpClient.UseDefaultCredentials == false)
					this.smtpClient.Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password);
			}
			catch (Exception ex)
			{
				throw new IllException("Can't initialize Email instance!" + Environment.NewLine + ex.Message);
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public static BscanILL.Export.Email.Email Instance { get { return instance; } }

		/// <summary>
		/// size in bytes
		/// </summary>
		public static ulong EmailSizeLimit { get { return (ulong)(BscanILL.SETTINGS.Settings.Instance.Export.Email.SizeLimitInMB * 1024 * 1024); } }
		public static string LiveBasePath { get { return liveBasePath; } }
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose
		public void Dispose()
		{
		}
		#endregion

		#region OpenAdvanced
		/*public static ExportDlg.WorkUnit OpenAdvanced(BscanILL.Hierarchy.Article article, System.Windows.Window parentForm)
		{
			BscanILL.Dialogs.EmailDlg dialog = new BscanILL.Dialogs.EmailDlg();
			dialog.Owner = parentForm;

			if (dialog.ShowDialog().Value)
			{
				//BscanILL.ExportSettings exportSettings = new BscanILL.ExportSettings(dialog.ExportFileSettings, BscanILL.ImagesSelection.All, article.Name);
				BscanILL.ExportSettings exportSettings = new BscanILL.ExportSettings(BscanILL.Scan.ExportMedium.Email, dialog.ExportFileSettings, BscanILL.ImagesSelection.All,
					BscanILL.Export.ExportDlg.GetDefaultFileNamePrefix(dialog.ExportFileSettings.FileFormat, dialog.ExportFileSettings.MultiImage));

				ExportDlg.WorkUnit exportUnit = new ExportDlg.WorkUnit(article.DlsgImages, exportSettings);

				exportUnit.Tag = dialog.Recipients;
				return exportUnit;
			}

			return null;
		}*/
		#endregion

		#region GetMessage()
		public static MailMessage GetMessage(string to, string subject, string body, FileInfo[] attachs)
		{
			return GetMessage(_settings.Export.Email.From, to, subject, body, new List<FileInfo>(attachs));
		}		
	
		public MailMessage GetMessage(string to, string subject, string body, FileInfo attachment)
		{
			return GetMessage(_settings.Export.Email.From, to, subject, body, new List<FileInfo>() { attachment });
		}

		public MailMessage GetMessage(string to, string subject, string body)
		{
			return GetMessage(_settings.Export.Email.From, to, subject, body, (List<FileInfo>)null);
		}
		#endregion

		#region SendEmail()
		/// <summary>
		/// it doesn't catch exceptions
		/// </summary>
		/// <param name="message"></param>
		public void SendEmail(MailMessage message)
		{
			System.Net.ServicePointManager.MaxServicePointIdleTime = 10;
			this.smtpClient.Send(message);
			Thread.Sleep(100);
		}
		#endregion

		#region IsValidEmail()
		public static bool IsValidEmail(string strIn)
		{
			// Return true if strIn is in valid e-mail format.
			return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
		}
		#endregion

		#region SplitRecipients()
		public static ArrayList SplitRecipients(string address)
		{
			ArrayList list = new ArrayList();

			list.AddRange(address.Split(new char[] { ',', ';' }));

			for (int i = list.Count - 1; i >= 0; i++)
			{
				if (IsValidEmail((string)list[i]) == false)
					list.RemoveAt(i);
			}

			return list;
		}
		#endregion

		#region SendTestEmail()
		public static void SendTestEmail(BscanILL.SETTINGS.Settings settings, bool sendDataAttachment)
		{
			using (Email email = new Email(settings.Export.Email))
			{
				string recipient = settings.Export.Email.From;

				if (sendDataAttachment)
				{
					FileInfo attachment = new FileInfo(settings.General.TempDir + @"\EmailTestFile.txt");

					using (FileStream stream = new FileStream(attachment.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
					{
						int bufferSize = settings.Export.Email.SizeLimitInMB * 1024 * 1024;
						byte[] buffer = new byte[bufferSize];

						stream.Write(buffer, 0, bufferSize);
					}

					using (MailMessage message = Email.GetMessage(recipient, recipient, "Bscan ILL Test Email", "Bscan ILL test message." + " '" + Environment.MachineName + "'.", new List<FileInfo>(){attachment}))
					{
						email.SendEmail(message);
					}

					try
					{
						attachment.Refresh();
						if (attachment.Exists)
							attachment.Delete();
					}
					catch { }
				}
				else
				{
					using (MailMessage message = Email.GetMessage(recipient, recipient, "Bscan ILL Test Email", "Bscan ILL test message." + " '" + Environment.MachineName + "'.", (List<FileInfo>)null))
					{
						email.SendEmail(message);
					}
				}
			}
		}
		#endregion

        #region SendTestEmailHttp()
        public static string SendTestEmailHttp(BscanILL.SETTINGS.Settings settings, bool sendDataAttachment)
		{
            string result = "";
			//using (Email email = new Email(settings.Export.Email))
			//{
				string recipient = settings.Export.Email.From;
                                
                    KssFolderAPIClientNS.KssFolderAPIClient client = null;

                    client = new KssFolderAPIClientNS.KssFolderAPIClient(LiveBasePath);

                    KssFolderAPIClientNS.EmailMessage message = new KssFolderAPIClientNS.EmailMessage(recipient, "ILL-Email@KICService.com", "Bscan ILL Test Email", "Bscan ILL test message." + " '" + Environment.MachineName + "'.");                                      
                    if (sendDataAttachment)
                    {
                        FileInfo attachment = new FileInfo(settings.General.TempDir + @"\EmailTestFile.txt");

                        using (FileStream stream = new FileStream(attachment.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            int bufferSize = settings.Export.Email.SizeLimitInMB * 1024 * 1024;
                            byte[] buffer = new byte[bufferSize];

                            stream.Write(buffer, 0, bufferSize);
                        }

                        message.AddAttachment(new KssFolderAPIClientNS.Attachment(attachment.FullName));
                    }
                    message.SMTPSenderID = "DLSG-ILL-Email@KICService.com";
                    message.SMTPSenderPassphrase = "G$h#296&";
                    if (settings.Export.Email.From.Length > 0)
                    {
                        message.ReplyTo = settings.Export.Email.From;
                    }
                    
                    result = client.KicSendEmailEx(message);                                    
			//}
            return result;
		}
		#endregion
		
		#region Refresh()
		internal void Refresh()
		{
			this.smtpClient = new SmtpClient(_settings.Export.Email.SmtpServer);
			this.smtpClient.UseDefaultCredentials = _settings.Export.Email.DefaultCredentials;
			this.smtpClient.EnableSsl = _settings.Export.Email.SslEncryption;
			this.smtpClient.Port = _settings.Export.Email.Port;
			this.smtpClient.Timeout = 5 * 60 * 1000;

			if (this.smtpClient.UseDefaultCredentials == false)
				this.smtpClient.Credentials = new NetworkCredential(_settings.Export.Email.Username, _settings.Export.Email.Password);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region GetMessage()
		private static MailMessage GetMessage(string from, string to, string subject, string body, List<FileInfo> attachs)
		{			
			to = to.Replace(';', ',');

			body = (body.Length == 0) ? "Bscan ILL message" : body;

			MailMessage message = new MailMessage(from, to, subject, body);

			if (attachs != null)
			{
				foreach (FileInfo file in attachs)
				{
					file.Refresh();

					if (file.Exists)
					{
						Attachment data = new Attachment(file.FullName);
						// Add time stamp information for the file.
						System.Net.Mime.ContentDisposition disposition = data.ContentDisposition;
						disposition.CreationDate = file.CreationTime;
						disposition.ModificationDate = file.LastWriteTime;
						disposition.ReadDate = file.LastAccessTime;
						// Add the file attachment to this e-mail message.
						message.Attachments.Add(data);
					}
				}
			}

			return message;
		}
		#endregion

		#region Notify()
		void Notify(string message, Exception ex)
		{
			Notifications.Instance.Notify(this, Notifications.Type.Warning, "Email, " + message, ex);
		}
		#endregion

		#region Settings_Changed()
		void Settings_Changed(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Refresh();
		}
		#endregion

		#endregion

	}
}
