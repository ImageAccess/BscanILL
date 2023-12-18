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

namespace BscanILL.Export.WorldShareILL
{
    class WorldShareILL : ExportBasics
    {
        //BscanILL.Export.Email.Email		email;

        AEUploadWrapper aeWrapper;        

        BscanILL.SETTINGS.Settings.ExportClass.ArticleExchangeClass aeSettings = BscanILL.SETTINGS.Settings.Instance.Export.ArticleExchange;
        BscanILL.SETTINGS.Settings.ExportClass.WorldShareILLClass worldShareILLSettings = BscanILL.SETTINGS.Settings.Instance.Export.WorldShareILL;

		#region constructor
		public WorldShareILL()
		{
			//this.email = BscanILL.Export.Email.Email.Instance;
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

                Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via WorldShareILL---");

                Progress_Changed(exportUnit, 5, "Checking data..."); 

                //check if required fields for WorldShareILL delivery are filled in..
                if ((article.IllNumber == null) || (article.IllNumber.Length == 0))
                {
                    throw new Exception("ILL number not set!");
                }
                else
                if ((worldShareILLSettings.InstSymbol == null) || (worldShareILLSettings.InstSymbol.Length == 0))
                {
                    throw new Exception("Institutional symbol not set!");
                }

                this.aeWrapper.TimeoutSeconds = 600;
                this.aeWrapper.Clear();
                this.aeWrapper.ErrorLogPath = BscanILL.SETTINGS.Settings.Instance.General.ErrLogDir + @"\AE_Error_log.txt";
                this.aeWrapper.WSKey = aeSettings.WSKey;
                this.aeWrapper.Secret = DecryptString(aeSettings.SecretBuffer);//DataProtector.GetString(aeSettings.Secret);

                this.aeWrapper.autho = aeSettings.Autho;
                this.aeWrapper.password = BscanILL.Misc.DataProtector.GetString(aeSettings.Password);

                this.aeWrapper.principalID = aeSettings.PrincipalId;
                this.aeWrapper.principalIDNS = aeSettings.PrincipalDns;

                this.aeWrapper.instSymbol = worldShareILLSettings.InstSymbol;
                this.aeWrapper.requesterInstSymbol = aeSettings.RequesterInstSymbol;
                this.aeWrapper.requesterEmail = article.Patron;

                int iNum = 0;
                if (article.IllNumber != null)
                {
                    iNum = int.Parse(article.IllNumber);
                }

                //this.aeWrapper.oclcRequestId = (article.TransactionId != null) ? article.TransactionId.Value.ToString() : "";
                this.aeWrapper.oclcRequestId = iNum.ToString();  //WorldShareILL is ILL# based

                //per Ed Davidson with OCLC:
                //for WorldShareILL the field illiadRequestId is expected to be blank                
                this.aeWrapper.illiadRequestId = "";  

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
                    Progress_Changed(exportUnit, 10 + Convert.ToInt32(progress * 80.0), "");            //updating progress in range <10,90>
                };

                try
                {
                    aeWrapper.PostArticle();
                    Progress_Changed(exportUnit, 100, "Image '" + source.Name + "' uploaded successfully.");

                    //**** no sending of confirmation email unlike with plain Article Exchange delivery method****
                    //Progress_Comment(exportUnit, "Sending confirmation email...");
                    //SendConfirmationEmail(exportUnit, aeWrapper.RetrievalUrl, aeWrapper.RetrievalPasscode);
                    //Progress_Changed(exportUnit, 50, "Confirmation email sent successfully.");
                    
                }
                catch (Exception ex)
                {
                    Notify(this, Notifications.Type.Error, ex.Message, ex);
                    throw new IllException("WorldShareILL post has failed! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
                }
            }
            catch (IllException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Notify(this, Notifications.Type.Error, ex.Message, ex);
                throw new IllException("WorldShareILL can't be set! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
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
