using System;
using System.Net.Mail;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using BscanILL.Misc;
using BscanILL.Hierarchy;
using BscanILL.Export.ILLiad;
using BscanILL.Export.AdditionalInfo;


namespace BscanILL.Export.Email
{
	/// <summary>
	/// Summary description for Email.
	/// </summary>
	public class EmailExport : ExportBasics
	{
		BscanILL.Export.Email.Email		email = BscanILL.Export.Email.Email.Instance;


		#region constructor
		public EmailExport()
		{
		}
		#endregion


		#region ExportArticle()
		public void ExportArticle(ExportUnit exportUnit)
		{			
            Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via Email---");
            Progress_Changed(exportUnit, 5, "Checking...");

			Article				article = exportUnit.Article;
			AdditionalEmail		additional = (AdditionalEmail)exportUnit.AdditionalInfo;

			if (article.IllNumber.StartsWith("-") == false && this.CheckingArticleInDb)
				this.Illiad.CheckArticleInDb(exportUnit);
			
            long sumFileSize = 0;
            for (int i = 0; i < exportUnit.Files.Count; i++)
            {
                exportUnit.Files[i].Refresh();
                if (exportUnit.Files[i].Exists == false)
                {
                    throw new IllException(ErrorCode.EmailAttachDoesntExist);
                }
                else
                {
                    sumFileSize += exportUnit.Files[i].Length;
                }

            }

            if (sumFileSize > (_settings.Export.Email.SizeLimitInMB * 1024 * 1024))
				throw new IllException(ErrorCode.EmailAttachToBig);

			string id = additional.FileNamePrefix;

            Progress_Changed(exportUnit, 10, "Creating email " + id + "...");
            			
           if (_settings.Export.Email.EmailDeliveryType != SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP)
            {
                string result = SentArticleInEmailHtml(exportUnit);
                if (String.Compare(result, "Message Sent", false) != 0)
                {
                    if (_settings.Export.Email.EmailDeliveryType == SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both)
                    {
                        //try also SMTP
                        SentArticleInEmail(exportUnit);
                    }
                    else
                    {
                        throw new Exception("Sending failed.");
                    }
                }
            }
            else
            {
                SentArticleInEmail(exportUnit);
            }						
			
			//if (additional.UpdateILLiad && article.IllNumber.StartsWith("-") == false)
            if (additional.UpdateILLiad)
			{
                Progress_Changed(exportUnit, 95, "Email  " + id + " sent successfully. Updating ILLiad...");
				this.Illiad.UpdateInfo(exportUnit, additional.ChangeStatusToRequestFinished);
                Progress_Changed(exportUnit, 100, "ILLiad updated successfully.");
			}
            else
            {
                Progress_Changed(exportUnit, 100, "Email  " + id + " sent successfully.");
            }
		}
		#endregion
		
        #region private methods
        private void SentArticleInEmail(ExportUnit exportUnit)
        {
            AdditionalEmail additional = (AdditionalEmail)exportUnit.AdditionalInfo;
            string id = additional.FileNamePrefix;

            MailMessage mail = BscanILL.Export.Email.Email.GetMessage(additional.Recipient, additional.Subject, additional.Body, exportUnit.Files.ToArray());
            Progress_Changed(exportUnit, 25, "Email  " + id.ToString() + " created successfully.");

            Progress_Comment(exportUnit, "Sending email " + id + "...");
            email.SendEmail(mail);
        }

        private string SentArticleInEmailHtml(ExportUnit exportUnit)
        {
            string result = "";
           
            AdditionalEmail additional = (AdditionalEmail)exportUnit.AdditionalInfo;
            string id = additional.FileNamePrefix;
           
            KssFolderAPIClientNS.KssFolderAPIClient client = null;
            client = new KssFolderAPIClientNS.KssFolderAPIClient(BscanILL.Export.Email.Email.LiveBasePath);

            KssFolderAPIClientNS.EmailMessage message = new KssFolderAPIClientNS.EmailMessage(additional.Recipient, "ILL-Email@KICService.com", additional.Subject, additional.Body);
            foreach (FileInfo fi in exportUnit.Files)
            {
               message.AddAttachment(new KssFolderAPIClientNS.Attachment(fi.FullName));
            }
            message.SMTPSenderID = "DLSG-ILL-Email@KICService.com";
            message.SMTPSenderPassphrase = "G$h#296&";
            if (_settings.Export.Email.From.Length > 0)
            {
                message.ReplyTo = _settings.Export.Email.From;
            }
            Progress_Changed(exportUnit, 25, "Email  " + id.ToString() + " created successfully.");

            Progress_Comment(exportUnit, "Sending email " + id + "...");                    
            result = client.KicSendEmailEx(message);                                    
			
            return result;

        }

        #endregion		

	}
}
