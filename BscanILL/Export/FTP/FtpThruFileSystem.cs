using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using System.Net.Mail;
using BscanILL.Misc;
using BscanILL.Export.ILL;
using BscanILL.Hierarchy;
using BscanILL.Export.AdditionalInfo;

namespace BscanILL.Export.FTP
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class FtpThruFileSystem : ExportBasics
	{
		BscanILL.Export.Email.Email			email;


		#region constructor
		public FtpThruFileSystem()
		{
			this.email = BscanILL.Export.Email.Email.Instance;
		}
		#endregion

		#region destructor
		public void Dispose()
		{
		}
		#endregion

		//PUBLIC METHODS
		#region public methods

        #region ExportArticle()
		public void ExportArticle(ExportUnit exportUnit)
		{
			Article				article = exportUnit.Article;
			AdditionalFtpDir	additional = (AdditionalFtpDir)exportUnit.AdditionalInfo;
			DirectoryInfo		exportDir = new DirectoryInfo(additional.ExportDirectory);
            
            Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via FTP Directory---");

            Progress_Changed(exportUnit, 5, "Refreshing FTP directory...");
            
            exportDir.Refresh();
            //not sure why we test it here as we create the destination below
			//if (exportDir.Exists == false)
			//	throw new IllException(ErrorCode.FtpCantConnectToExportDir);

			if (article.IllNumber.StartsWith("-") == false && this.CheckingArticleInDb)
				this.Illiad.CheckArticleInDb(exportUnit);

			//string id = article.TransactionId.HasValue ? article.TransactionId.Value.ToString() : article.IllNumber;
            string id = GetSubFolderName( additional, article) ;

            Progress_Changed(exportUnit, 10, "Ftp directory refreshed.");
			
			FileInfo dest;
           
            for (int i = 0; i < exportUnit.Files.Count; i++)
            {
                FileInfo source = exportUnit.Files[i];

                //string destPath = string.Format("{0}\\{1}_{2:0000}{3}", destDir.FullName, addOn.FileNamePrefix, index++, file.Extension);
                //while (File.Exists(destPath))
                //    destPath = string.Format("{0}\\{1}_{2:0000}{3}", destDir.FullName, addOn.FileNamePrefix, index++, file.Extension);

                if (additional.SaveToSubfolder)
                    dest = new FileInfo(exportDir.FullName + @"\" + id + @"\" + source.Name);
                else
                    dest = new FileInfo(exportDir.FullName + @"\" + source.Name);                

                if (dest.Exists)
                    dest.Delete();

                Progress_Changed(exportUnit, Convert.ToInt32(10 + (((i + 1.0) * 50) / exportUnit.Files.Count)), "Copying file " + dest.Name + "...");
                dest.Directory.Create();

                source.CopyTo(dest.FullName);
                Progress_Changed(exportUnit, Convert.ToInt32(11 + (((i + 1.0) * 50) / exportUnit.Files.Count)), string.Format("Done with copying file {0} of {1}", i + 1, exportUnit.Files.Count));
            }
			
			if (additional.SendConfirmEmail)
			{
                Progress_Changed(exportUnit, 70, "File(s) copied successfully. Sending confirmation email...");
                if (_settings.Export.Email.EmailDeliveryType != SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP)
                {
                    string result = SendConfirmationEmailHtml(exportUnit, id);
                    if (String.Compare(result, "Message Sent", false) != 0)
                    {
                        if (_settings.Export.Email.EmailDeliveryType == SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both)
                        {
                            //try also SMTP                            
                            SendConfirmationEmail(exportUnit, id);
                        }
                        else
                        {
                            throw new Exception("Confirmation email failed.");
                        }
                    }
                }
                else
                {                    
                    SendConfirmationEmail(exportUnit, id);
                }
				
                if (additional.UpdateILLiad)
                {
                    Progress_Changed(exportUnit, 95, "Confirmation email sent successfully. Updating ILLiad...");
                    this.Illiad.UpdateInfo(exportUnit, additional.ChangeStatusToRequestFinished);
                    Progress_Changed(exportUnit, 100, "ILLiad updated successfully.");
                }
                else
                {
                    Progress_Changed(exportUnit, 100, "Confirmation email sent successfully.");
                }
			}
            else
            {
                //if (additional.UpdateILLiad && article.IllNumber.StartsWith("-") == false)
                if (additional.UpdateILLiad)
                {
                    Progress_Changed(exportUnit, 95, "File(s) copied successfully. Updating ILLiad...");
                    this.Illiad.UpdateInfo(exportUnit, additional.ChangeStatusToRequestFinished);
                    Progress_Changed(exportUnit, 100, "ILLiad updated successfully.");
                }
                else
                {
                    Progress_Changed(exportUnit, 100, "File(s) copied successfully.");
                }
            }

		}
        #endregion

        #endregion

		//PRIVATE METHODS
		#region private methods

		#region SendConfirmationEmail()
		protected virtual void SendConfirmationEmail(ExportUnit exportUnit, string folder_id)
		{
			string				subject = "Document Delivery Notification";
			string				body ;
			string				address;
			//string id = article.TransactionId.HasValue ? article.TransactionId.Value.ToString() : article.IllNumber;
			Article				a = exportUnit.Article;
			AdditionalFtpDir	additional = (AdditionalFtpDir)exportUnit.AdditionalInfo;

			if (additional.SaveToSubfolder)
                address = string.Format("<A href=\"{1}/{0}\" >{1}/{0}</A>", folder_id, additional.FtpLink);
			else
                address = string.Format("<A href=\"{1}\" >{1}</A>", folder_id, additional.FtpLink);

            body = string.Format("To retrieve your requested document(s) {0}, click on the link: {1}", exportUnit.FileNamePrefix , address);
			
            using (MailMessage message = email.GetMessage(a.Address, subject, body))
            {
                message.IsBodyHtml = true;
                email.SendEmail(message);
            }
		}
		#endregion

        #region SendConfirmationEmailHtml()
        protected virtual string SendConfirmationEmailHtml(ExportUnit exportUnit, string folder_id)
		{
            string result = "";
            string subject = "Document Delivery Notification";            
            string address;
            //string id = article.TransactionId.HasValue ? article.TransactionId.Value.ToString() : article.IllNumber;
            Article a = exportUnit.Article;
            AdditionalFtpDir additional = (AdditionalFtpDir)exportUnit.AdditionalInfo;

            if (additional.SaveToSubfolder)
                address = string.Format("<A href=\"{1}/{0}\" >{1}/{0}</A>", folder_id, additional.FtpLink);                
            else
                address = string.Format("<A href=\"{1}\" >{1}</A>", folder_id, additional.FtpLink);                

            string body = string.Format("To retrieve your requested document(s) {0}, click on the link: {1}", exportUnit.FileNamePrefix, address);

            KssFolderAPIClientNS.KssFolderAPIClient client = null;
            client = new KssFolderAPIClientNS.KssFolderAPIClient(BscanILL.Export.Email.Email.LiveBasePath);
            
            KssFolderAPIClientNS.EmailMessage message = new KssFolderAPIClientNS.EmailMessage(a.Address, "ILL-Email@KICService.com", subject, body);  //if multiple 'To' addresses in first field - use comma to separate them            
            message.IsBodyHtml = true;
            message.SMTPSenderID = "DLSG-ILL-Email@KICService.com";
            message.SMTPSenderPassphrase = "G$h#296&";
            if (_settings.Export.Email.From.Length > 0)
            {
              message.ReplyTo = _settings.Export.Email.From;                
            }
            result = client.KicSendEmailEx(message);

            return result;
        }
        #endregion
        
        #region GetSubFolderName()
        private string GetSubFolderName(AdditionalFtpDir additional, Article article)
        {
            string subDirName = "" ;
            if ( additional.SaveToSubfolder )
            {
                if ((additional.SubfolderNameBase == BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn.TransactionName) &&
                    (article.TransactionId != null))
                    subDirName = article.TransactionId.ToString();
                else if ((additional.SubfolderNameBase == BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn.IllName) &&
                    (article.IllNumber != null && article.IllNumber.Trim().Length > 0))
                    subDirName = article.IllNumber.ToString();
                else
                    subDirName = article.Id.ToString();
            }	
            return subDirName ;
        }
    	#endregion


		#endregion
	}
}
