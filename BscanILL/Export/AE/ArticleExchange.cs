using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BscanILL.Export.ILL;
using ArticleExchangeWrapper;
using BscanILL.Export.ILLiad;
using BscanILL.Hierarchy;
using System.IO;
using BscanILL.Misc;
using System.Net.Mail;
using BscanILL.Export.AdditionalInfo;
using System.Security.Cryptography;



namespace BscanILL.Export.AE
{


	public class ArticleExchange : ExportBasics
	{
		BscanILL.Export.Email.Email		email;
		//BscanILL.Export.ILLiad.IILLiad	illiad = null;
		AEUploadWrapper					aeWrapper;
		
		BscanILL.SETTINGS.Settings.ExportClass.ArticleExchangeClass aeSettings = BscanILL.SETTINGS.Settings.Instance.Export.ArticleExchange;


		#region constructor
		public ArticleExchange()
		{
			this.email = BscanILL.Export.Email.Email.Instance;
			this.aeWrapper = new AEUploadWrapper();


			//if (_settings.Export.ArticleExchange.UpdateILLiad)
				//illiad = ILLiadBasics.GetIlliadInstance();
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
			try
			{
				Article article = exportUnit.Article;
                FileInfo source = exportUnit.Files[0];
                
                Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via Article Exchange---");

                Progress_Changed(exportUnit, 5, "Checking data...");

				this.aeWrapper.TimeoutSeconds = 600;
				this.aeWrapper.Clear();
                this.aeWrapper.ErrorLogPath = BscanILL.SETTINGS.Settings.Instance.General.ErrLogDir + @"\AE_Error_log.txt";
				this.aeWrapper.WSKey = aeSettings.WSKey;
				this.aeWrapper.Secret = DecryptString(aeSettings.SecretBuffer);//DataProtector.GetString(aeSettings.Secret);

				this.aeWrapper.autho = aeSettings.Autho;
				this.aeWrapper.password = BscanILL.Misc.DataProtector.GetString(aeSettings.Password);

				this.aeWrapper.principalID = aeSettings.PrincipalId;
				this.aeWrapper.principalIDNS = aeSettings.PrincipalDns;

                this.aeWrapper.instSymbol = "";
				this.aeWrapper.requesterInstSymbol = aeSettings.RequesterInstSymbol;
				this.aeWrapper.requesterEmail = article.Patron;

                //per Ed Davidson with OCLC:
                //with ArticleExchange/no Tipasa the field oclcRequestId should be probably blank but per Ed's with OCLC info, these two fields (oclcRequestId,illiadRequestId) are pretty much just info and
                //are not related to any action so we can set it the way we want...
                this.aeWrapper.oclcRequestId = (article.TransactionId != null) ? article.TransactionId.Value.ToString() : "";
				this.aeWrapper.illiadRequestId = (article.IllNumber != null) ? article.IllNumber : "";


				this.aeWrapper.vdxRequestId = "";

				this.aeWrapper.aAuthor = "";
				this.aeWrapper.aDate = "";
				this.aeWrapper.aIssue = "";
				this.aeWrapper.aPages = "";
				this.aeWrapper.aTitle = "";
				this.aeWrapper.aVolume = "";
				this.aeWrapper.jTitle = "";

                SetFile(exportUnit.Files[0]);

                Progress_Changed(exportUnit, 10, "Data checked. Uploading File " + source.Name + "...");                

				aeWrapper.ProgressChanged += delegate(double progress)
				{
					//Progress_Changed(exportUnit, Convert.ToInt32(progress * 100.0), "");
                    Progress_Changed(exportUnit, 10 + Convert.ToInt32(progress * 70.0), "");            //updating progress in range <10,80>
				};                

				try
				{                                        
                    aeWrapper.PostArticle();
                    //Progress_Changed(exportUnit, 35 , "Image '" + source + "' uploaded successfully.");                                            

					//Progress_Comment(exportUnit, "Sending confirmation email...");
                    Progress_Changed(exportUnit, 85,  "Image '" + source.Name + "' uploaded successfully. Sending confirmation email...");
					
                    if (_settings.Export.Email.EmailDeliveryType != SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP)
                    {                        
                        string result = SendConfirmationEmailHtml(exportUnit, aeWrapper.RetrievalUrl, aeWrapper.RetrievalPasscode);
                        if (String.Compare(result, "Message Sent", false) != 0)
                        {
                            if (_settings.Export.Email.EmailDeliveryType == SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both)
                            {
                                //try also SMTP
                                SendConfirmationEmail(exportUnit, aeWrapper.RetrievalUrl, aeWrapper.RetrievalPasscode);
                            }
                            else
                            {
                                throw new Exception("Confirmation email failed.");
                            }
                        }
                    }
                    else
                    {
                        SendConfirmationEmail(exportUnit, aeWrapper.RetrievalUrl, aeWrapper.RetrievalPasscode);
                    }						

					/*if (_settings.Export.ArticleExchange.UpdateILLiad && article.IllNumber.StartsWith("-") == false)
					{
						Progress_Comment(exportUnit, "Updating ILLiad...");
						illiad.UpdateInfo(exportUnit, _settings.Export.ArticleExchange.ChangeRequestToFinished);
						Progress_Changed(exportUnit, 80, "ILLiad updated.");
					}*/

					//if (aeSettings.UpdateILLiad && exportUnit.Article.IllNumber.StartsWith("-") == false)
                    if (aeSettings.UpdateILLiad)
					{
                        Progress_Changed(exportUnit, 95, "Confirmation email sent successfully. Updating ILLiad...");
						this.Illiad.UpdateInfo(exportUnit, aeSettings.ChangeRequestToFinished);
                        Progress_Changed(exportUnit, 100, "ILLiad updated successfully.");
					}
                    else
                    {
                        Progress_Changed(exportUnit, 100, "Confirmation email sent successfully.");
                    }
				}
				catch (Exception ex)
				{
					Notify(this, Notifications.Type.Error, ex.Message, ex);
					throw new IllException("Article Exchange post has failed! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, ex.Message, ex);
				throw new IllException("Article Exchange can't be set! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region SetFile()
		private void SetFile(FileInfo file)
		{
			switch (file.Extension.ToLower())
			{
				case ".jpg":
				case ".jpeg":
					aeWrapper.FileType = ArticleExchangeFileTypes.Jpeg;
					break;

				case ".tif":
				case ".tiff":
					aeWrapper.FileType = ArticleExchangeFileTypes.Tiff;
					break;

				case ".bmp":
					aeWrapper.FileType = ArticleExchangeFileTypes.Bmp;
					break;

				case ".png":
					aeWrapper.FileType = ArticleExchangeFileTypes.Png;
					break;

				case ".zip":
					aeWrapper.FileType = ArticleExchangeFileTypes.Zip;
					break;

				case ".pdf":
					aeWrapper.FileType = ArticleExchangeFileTypes.Pdf;
					break;

				default:
					aeWrapper.FileType = ArticleExchangeFileTypes.None;
					break;
			}

			aeWrapper.FilePath = file.FullName;
		}
		#endregion

		#region SendConfirmationEmail()
		protected void SendConfirmationEmail(ExportUnit exportUnit, string url, string password)
		{
			AdditionalArticleExchange addInfo = (AdditionalArticleExchange)exportUnit.AdditionalInfo;

			//string id = addInfo.FileNamePrefix;
			string subject = addInfo.EmailSubject;
			//string address = string.Format("<A href=\"{0}\" >{0}</A>", url);
			/*string body = string.Format("To retrieve your requested document {0} posted by BscanILL:<br/><br/>", id);

			body += string.Format("Follow: {0}<br/>", url);
			body += string.Format("Passwd: {0}", password);*/
			string body = addInfo.EmailBody;

			body = body.Replace("{URL}", url);
			body = body.Replace("{PASS}", password);
			body = body.Replace("\n", "<BR/>");

			using (MailMessage message = email.GetMessage(addInfo.RecipientEmail, subject, body))
			{
				message.IsBodyHtml = true;
				email.SendEmail(message);

				if (addInfo.ConfirmationEmail != null && BscanILL.Export.Email.Email.IsValidEmail(addInfo.ConfirmationEmail))
				{
					using (MailMessage message2 = email.GetMessage(addInfo.ConfirmationEmail, subject, body))
					{
						message2.IsBodyHtml = true;
						email.SendEmail(message2);
					}
				}
			}
		}
		#endregion

        #region SendConfirmationEmailHtml()
        protected string SendConfirmationEmailHtml(ExportUnit exportUnit, string url, string password)
		{
            string result = "";
            KssFolderAPIClientNS.KssFolderAPIClient client = null;
            client = new KssFolderAPIClientNS.KssFolderAPIClient(BscanILL.Export.Email.Email.LiveBasePath);

			AdditionalArticleExchange addInfo = (AdditionalArticleExchange)exportUnit.AdditionalInfo;

			//string id = addInfo.FileNamePrefix;
			string subject = addInfo.EmailSubject;
			//string address = string.Format("<A href=\"{0}\" >{0}</A>", url);
			/*string body = string.Format("To retrieve your requested document {0} posted by BscanILL:<br/><br/>", id);

			body += string.Format("Follow: {0}<br/>", url);
			body += string.Format("Passwd: {0}", password);*/
			string body = addInfo.EmailBody;

			body = body.Replace("{URL}", url);
			body = body.Replace("{PASS}", password);
			body = body.Replace("\n", "<BR/>");
            
            KssFolderAPIClientNS.EmailMessage message = new KssFolderAPIClientNS.EmailMessage(addInfo.RecipientEmail, "ILL-Email@KICService.com", subject, body);  //if multiple 'To' addresses in first field - use comma to separate them
            message.IsBodyHtml = true;
            message.SMTPSenderID = "DLSG-ILL-Email@KICService.com";
            message.SMTPSenderPassphrase = "G$h#296&";
            if (_settings.Export.Email.From.Length > 0)
            {
                message.ReplyTo = _settings.Export.Email.From;
            }
            result = client.KicSendEmailEx(message);

            if (String.Compare(result, "Message Sent", false) == 0)
			if (addInfo.ConfirmationEmail != null && BscanILL.Export.Email.Email.IsValidEmail(addInfo.ConfirmationEmail))
            {
                KssFolderAPIClientNS.EmailMessage message2 = new KssFolderAPIClientNS.EmailMessage(addInfo.ConfirmationEmail, "ILL-Email@KICService.com", subject, body);
                message2.IsBodyHtml = true;
                message2.SMTPSenderID = "DLSG-ILL-Email@KICService.com";
                message2.SMTPSenderPassphrase = "G$h#296&";
                if (_settings.Export.Email.From.Length > 0)
                {
                    message2.ReplyTo = _settings.Export.Email.From;
                }
			    result = client.KicSendEmailEx(message2);  
            }
			
            return result;
		}
		#endregion
		
		#region DecryptString()
		string DecryptString(byte[] cipherText)
		{
			using (Aes aesAlg = Aes.Create())
			{
				Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes("panskr46784*(&)*(#*^$#", Encoding.UTF8.GetBytes("^*&*^845$#&*^$*#*$#JKDH F45546K"));
				Rfc2898DeriveBytes deriveBytes2 = new Rfc2898DeriveBytes("^*&*^845$#&*^$*#*$#JKDH F45546K", Encoding.UTF8.GetBytes("panskr46784*(&)*(#*^$#"));

				aesAlg.Key = deriveBytes.GetBytes(32);
				aesAlg.IV = deriveBytes2.GetBytes(16);

				ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

				using (MemoryStream msDecrypt = new MemoryStream(cipherText))
				{
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader srDecrypt = new StreamReader(csDecrypt))
						{
							return srDecrypt.ReadToEnd();
						}
					}
				}
			}
		}
		#endregion

		#endregion

	}
}
