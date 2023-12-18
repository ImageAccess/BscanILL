using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions ;
using System.Net;

using System.Net.Mail;
using System.Configuration;
using System.Threading;
using System.Collections.Generic;
using System.Windows;
using BscanILL.Misc;


namespace BscanILL.Export.Email
{
	public class DlsgEmail : ExportBasics, IDisposable
	{
		static DlsgEmail			instance = null;

        SmtpClient					smtpClient;
		bool						canSendUsingCustomerSmtp = true;
		bool						canSendUsingDlsgSmtp = true;
		object						sendEmailLocker = new object();


		#region constructor
		private DlsgEmail()
		{
			if (BscanILL.SETTINGS.Settings.Instance.Export.Email.Enabled == false)
				this.canSendUsingCustomerSmtp = false;
			
			//SMTP SERVER
			try
			{
				System.Net.IPHostEntry host = Dns.GetHostEntry(_settings.Export.DlsgClient.SmtpServer);
				string ip = host.AddressList[0].ToString();

				//Console.WriteLine("GetHostEntry({0}) returns:", hostname);
				//foreach (IPAddress ip in host.AddressList)
				//	Console.WriteLine("    {0}", ip);
				
				this.smtpClient = new SmtpClient(ip);
				this.smtpClient.UseDefaultCredentials = _settings.Export.DlsgClient.DefaultCredentials;
				this.smtpClient.EnableSsl = _settings.Export.DlsgClient.SslEncryption;
				this.smtpClient.Port = _settings.Export.DlsgClient.Port;
				this.smtpClient.Timeout = 30 * 1000;
			}
			catch (Exception)
			{
				this.canSendUsingDlsgSmtp = false;
			}
		}
		#endregion

		#region enum SendTo
		public enum SendTo
		{
			Management,
			Admins,
			Developers,
			LicenseProcessors
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties
		
		/// <summary>
		/// size in bytes
		/// </summary>
		public static ulong	EmailSizeLimit	{ get { return (ulong)(BscanILL.SETTINGS.Settings.Instance.Export.DlsgClient.SizeLimitInMB * 1024 * 1024); } }

		#region CanSendEmails
		public static bool CanSendEmails
		{ 
			get 
			{ 
				BscanILL.Export.Email.DlsgEmail instance = Instance ;

				if (BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.SendToDLSG == false)
					return false;
				if((BscanILL.SETTINGS.Settings.Instance.Export.Email.Enabled == false || instance.canSendUsingCustomerSmtp == false) && (instance.canSendUsingDlsgSmtp == false))
					return false;

				return true; 
			} 
		}
		#endregion

		#region Instance
		public static BscanILL.Export.Email.DlsgEmail Instance 
		{ 
			get 
			{
				if (instance == null)
					instance = new DlsgEmail();
				
				return instance; 
			} 
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose
		public void Dispose()
		{
			if (instance != null)
				instance = null;
		}
		#endregion

		#region SendEmail()
		public void SendEmail(SendTo sendTo, string subject, string body)
		{
			SendEmail(sendTo, subject, body, null);
		}

		public void SendEmail(SendTo sendTo, string subject, string body, FileInfo attachment)
		{
			using (MailMessage message = GetMessage(sendTo, subject, body, attachment))
			{
				SendEmail(message);
			}
		}

		public void SendEmail(SendTo sendTo, string subject, string body, MemoryStream stream, string streamName)
		{
			using (MailMessage message = GetMessage(sendTo, subject, body, stream, streamName))
			{
				SendEmail(message);
			}
		}
		#endregion

		#region SendTestEmail()
		public static void SendTestEmail(SendTo sendTo, bool sendDataAttachment)
		{
			using (DlsgEmail email = DlsgEmail.Instance)
			{
				
				if (sendDataAttachment)
				{
					FileInfo attachment = new FileInfo(BscanILL.SETTINGS.Settings.Instance.General.TempDir + @"\EmailTestFile.txt");

					using(FileStream stream = new FileStream(attachment.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
					{
						int bufferSize = BscanILL.SETTINGS.Settings.Instance.Export.DlsgClient.SizeLimitInMB * 1024 * 1024;
						byte[] buffer = new byte[bufferSize];

						stream.Write(buffer, 0, bufferSize);
					}

					using (MailMessage message = email.GetMessage(sendTo, "Bscan ILL Test Email", "Bscan ILL test message." + " '" + Environment.MachineName + "'.", attachment))
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
					using (MailMessage message = email.GetMessage(sendTo, "Bscan ILL Test Email", "Bscan ILL test message." + " '" + Environment.MachineName + "'.", null))
					{
						email.SendEmail(message);
					}
				}
			}
		}
		#endregion

        # region GetMailMessageParameters
        public void GetMailMessageParameters(SendTo sendTo, ref string recipient, ref string from)
        {
            GetMailMessageParams(sendTo, ref recipient, ref from);
        }
        #endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

        #region GetMailMessageParams
        private void GetMailMessageParams(SendTo sendTo, ref string recipient, ref string from)
        {
            recipient = "";
            from = Email.IsValidEmail(_settings.Export.Email.From) ? _settings.Export.Email.From : _settings.Export.DlsgClient.From;

            switch (sendTo)
            {
                case SendTo.Management: recipient = BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.DlsgManagementEmail; break;
                case SendTo.Admins: recipient = BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.DlsgAdminsEmail; break;
                case SendTo.Developers: recipient = BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.DlsgDevelopersEmail; break;
                case SendTo.LicenseProcessors: recipient = BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.DlsgLicenseEmail; break;
                default: throw new Exception("DlsgEmail, GetMessage(): Unknown Recipient!");
            }

            recipient = recipient.Replace(';', ',');
        }
        #endregion
        
		#region GetMessage()
		private MailMessage GetMessage(SendTo sendTo, string subject, string body, FileInfo attachment)
		{
			string recipient = "";
            string from = "";

            GetMailMessageParams(sendTo, ref recipient, ref from);

            body = (body.Length == 0) ? "Bscan ILL message" : body;

			MailMessage message = new MailMessage(from, recipient, subject, body);

			if (attachment != null)
			{
				attachment.Refresh();

				if (attachment.Exists)
				{
					Attachment data = new Attachment(attachment.FullName);
					// Add time stamp information for the file.
					System.Net.Mime.ContentDisposition disposition = data.ContentDisposition;
					disposition.CreationDate = attachment.CreationTime;
					disposition.ModificationDate = attachment.LastWriteTime;
					disposition.ReadDate = attachment.LastAccessTime;
					// Add the file attachment to this e-mail message.
					message.Attachments.Add(data);
				}
			}

			return message;
		}

		private MailMessage GetMessage(SendTo sendTo, string subject, string body, MemoryStream stream, string streamName)
		{
			string recipient = "";
			string from = Email.IsValidEmail(_settings.Export.Email.From) ? _settings.Export.Email.From : _settings.Export.DlsgClient.From;

			switch (sendTo)
			{
				case SendTo.Management: recipient = BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.DlsgManagementEmail; break;
				case SendTo.Admins: recipient = BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.DlsgAdminsEmail; break;
				case SendTo.Developers: recipient = BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.DlsgDevelopersEmail; break;
				case SendTo.LicenseProcessors: recipient = BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.DlsgLicenseEmail; break;
				default: throw new Exception("DlsgEmail, GetMessage(): Unknown Recipient!");
			}

			recipient = recipient.Replace(';', ',');
			body = (body.Length == 0) ? "Bscan ILL message" : body;

			MailMessage message = new MailMessage(from, recipient, subject, body);

			if (stream != null)
				message.Attachments.Add(new Attachment(stream, streamName));

			return message;
		}
		#endregion

		#region SendEmail()
		private void SendEmail(MailMessage message)
		{
			lock (this.sendEmailLocker)
			{
				try
				{
					if (this.canSendUsingCustomerSmtp && _settings.Export.Email.Enabled)
					{
						BscanILL.Export.Email.Email.Instance.SendEmail(message);
						return;
					}
					else
						this.canSendUsingCustomerSmtp = false;
				}
				catch (Exception)
				{
					this.canSendUsingCustomerSmtp = false;
				}

				try
				{
					//if (this.canSendUsingDlsgSmtp)
					{
						System.Net.ServicePointManager.MaxServicePointIdleTime = 10;
						this.smtpClient.Send(message);
						Thread.Sleep(100);
					}
				}
				catch (Exception ex)
				{
					this.canSendUsingDlsgSmtp = false;
					throw new IllException("Can't send Email '" + message.Subject + "' to DLSG! " + ex.Message);
				}
			}
		}
		#endregion

		#region Notify()
		void Notify(string message, Exception ex)
		{
			Notifications.Instance.Notify(this, Notifications.Type.Warning, "DlsgEmail, " + message, ex);
		}
		#endregion

		#endregion

	}
}
