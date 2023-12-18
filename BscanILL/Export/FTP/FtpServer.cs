using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using System.Net.Mail;

using FTPClientConnector;
using System.Collections.Generic;
using BscanILL.Misc;
using BscanILL.Export.FTP;
using BscanILL.Export.ILL;
using BscanILL.Hierarchy;
using BscanILL.Export.ILLiad;
using BscanILL.Export.AdditionalInfo;

namespace BscanILL.Export.FTP
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class FtpServer : ExportBasics
	{
		IFTPConnector						ftpConnector;
		BscanILL.Export.Email.Email			email;
		string								serverName;


		#region constructor
		public FtpServer()
		{
			this.email = BscanILL.Export.Email.Email.Instance;

			this.ftpConnector = new FTPConnector();

			this.ftpConnector.ConnectedEvent += new EventHandler(FtpConnector_ConnectedEvent);
			this.ftpConnector.DisconnectedEvent += new EventHandler(FtpConnector_DisconnectedEvent);
			this.ftpConnector.DownloadEvent += new EventHandler(FtpConnector_DownloadEvent);
			this.ftpConnector.ListingEvent += new EventHandler(FtpConnector_ListingEvent);
			this.ftpConnector.ProgressEvent += new EventHandler(FtpConnector_ProgressEvent);
			this.ftpConnector.UploadEvent += new EventHandler(FtpConnector_UploadEvent);
		}
		#endregion

		#region destructor
		public void Dispose()
		{
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		public bool IsConnected { get { return (this.ftpConnector != null && this.ftpConnector.IsConnected); } }
		public string ServerName { get { return this.serverName; } }

		public string RemoteDir
		{
			get { return this.ftpConnector.RemoteDir; }
			set { this.ftpConnector.SetRemoteDirectory(value); }
		}
		#endregion


		//PUBLIC METHODS
		#region public methods

        #region ExportArticle()
		public void ExportArticle(ExportUnit exportUnit)
		{
			try
			{
                Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via FTP---");
				BscanILL.Export.AdditionalInfo.AdditionalFtp additional = (BscanILL.Export.AdditionalInfo.AdditionalFtp)exportUnit.AdditionalInfo;

                Progress_Changed(exportUnit, 5, "Checking...");

				Login(exportUnit);
				FtpLogin ftpLofin = additional.FtpLogin;
				
				Article article = exportUnit.Article;
				string rootDirectory = this.RemoteDir;

                this.RemoteDir = "//"; // clear to fix problem with doubling directories in the path
                
                Progress_Changed(exportUnit, 10, "Creating remote article directory structure.");

                string id = GetSubFolderName(additional, article);
				
				string directory = ftpLofin.Directory;
                if (additional.SaveToSubfolder)
                {                    
                    directory = Path.Combine(directory, id);
                }                
                   // dest = new FileInfo(exportDir.FullName + @"\" + source.Name);                

				List<string> subDirectories = new List<string>(directory.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries));

				for (int i = subDirectories.Count - 1; i >= 0; i--)
					if (((string)subDirectories[i]).Trim().Length == 0)
						subDirectories.RemoveAt(i);
				
				foreach (string subDirectory in subDirectories)
				{
					if (DirectoryExists(subDirectory) == false)
						CreateDir(subDirectory);

					RemoteDir += @"/" + subDirectory;
				}
				Progress_Changed(exportUnit, 20, "Remote article directory structure created.");

                for (int i = 0; i < exportUnit.Files.Count; i++)
                {
                    FileInfo source = exportUnit.Files[i];

                    string fileName = string.Format("{0}{1}", additional.FileNamePrefix, source.Extension);
                    int index = 1;

                    while (FileExists(fileName))
                    {
                        fileName = string.Format("{0}_{1}{2}", additional.FileNamePrefix, index++, source.Extension);
                    }
                    
                    Progress_Changed(exportUnit, Convert.ToInt32(20 + (((i + 1.0) * 40) / exportUnit.Files.Count)), "Copying file " + source.Name + "...");

                    CopyFile(source, fileName);
                    
                    Progress_Changed(exportUnit, Convert.ToInt32(21 + (((i + 1.0) * 40) / exportUnit.Files.Count)), string.Format("Done with copying file {0} of {1}", i + 1, exportUnit.Files.Count));
                }
                
				if (additional.SendConfirmEmail)
				{                    
                    Progress_Changed(exportUnit, 70, "File(s) copied successfully. Sending confirmation email...");					                   					

                    if (_settings.Export.Email.EmailDeliveryType != SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.SMTP)
                    {
                        string result = SendConfirmationEmailHtml(exportUnit, RemoteDir, ftpLofin);
                        if (String.Compare(result, "Message Sent", false) != 0)
                        {
                            if (_settings.Export.Email.EmailDeliveryType == SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.Both)
                            {
                                //try also SMTP
                                //SendConfirmationEmail(exportUnit, aeWrapper.RetrievalUrl, aeWrapper.RetrievalPasscode);
                                SendConfirmationEmail(exportUnit, RemoteDir, ftpLofin);        
                            }
                            else
                            {
                                throw new Exception("Confirmation email failed.");
                            }
                        }
                    }
                    else
                    {                       
                        SendConfirmationEmail(exportUnit, RemoteDir, ftpLofin);        
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

                this.RemoteDir = rootDirectory;

			}
			finally
			{
				try { Logout(); }
				catch { }
			}
		}
        #endregion

        #endregion

		//PRIVATE METHODS
		#region private methods

		#region Login()
		private void Login(ExportUnit exportUnit)
		{
			try
			{
				BscanILL.Export.FTP.FtpLogin ftpLofin = ((BscanILL.Export.AdditionalInfo.AdditionalFtp)exportUnit.AdditionalInfo).FtpLogin;

				string server = ftpLofin.Server;
				int port = ftpLofin.Port;
				FTPType ftpType = ftpLofin.FtpType;
				string directory = ftpLofin.Directory;
				string username = ftpLofin.Username;
				string password = BscanILL.Misc.DataProtector.GetString(ftpLofin.Password);

				//Progress_Comment(exportUnit, "Connecting to '" + server + "'...");
				this.ftpConnector.Connect(server, username, password, port, 20000, ftpType);

                //should not I cleare RemoteDir = "//" ; ??
                RemoteDir = "//";  //bob for safety
/*
				string[] subDirectories = directory.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string subDirectory in subDirectories)
				{
					RemoteDir += @"/" + subDirectory;   //if directory is not created at destination, it fails here, why it is here when in ExportArticle() we create missing subfolders...
				}
*/
			}
			catch (Exception ex)
			{
				Logout();
				throw new IllException(ErrorCode.FtpCantLogin, "Can't login to the FTP server! " + ex.Message);
			}
		}
		#endregion

		#region Logout()
		private void Logout()
		{
			try
			{
				//Progress_Comment(exportUnit, "Disconnecting from server...");
				if (this.ftpConnector.IsConnected)
					this.ftpConnector.Disconnect();
			}
			catch (Exception)
			{
				//NotifyAction("Ftp, Logout(): " + ex.Message);
			}
			finally
			{
				this.serverName = "";
			}
		}
		#endregion

		#region SendConfirmationEmail()
        protected virtual void SendConfirmationEmail(ExportUnit exportUnit, string folder_id, FtpLogin ftpLogin)
		{
            Article a = exportUnit.Article;
            AdditionalFtp additional = (AdditionalFtp)exportUnit.AdditionalInfo;

			string		subject = "Document Delivery Notification";
            //string      address = string.Format("<A href=\"{0}/{1}/{2}\" >{1}/{0}</A>", ftpLogin.Server, ftpLogin.Directory, fileName);			

            string address = "" ;
            if(folder_id[0] == '/')
            {
                address = string.Format("<A href=\"{0}{1}\" >{0}{1}</A>", ftpLogin.Server, folder_id);			
            }
            else
            {
                address = string.Format("<A href=\"{0}/{1}\" >{0}/{1}</A>", ftpLogin.Server, folder_id);			
            }
//            if (additional.SaveToSubfolder)
//                address = string.Format("<A href=\"{1}/{0}\" >{1}/{0}</A>", folder_id, additional.FtpLink);
//            else
//                address = string.Format("<A href=\"{1}\" >{1}</A>", folder_id, additional.FtpLink);

            string body = string.Format("To retrieve your requested document(s) {0}, click on the link: {1}", exportUnit.FileNamePrefix, address);

            using (MailMessage message = email.GetMessage(a.Address, subject, body))
            {
                message.IsBodyHtml = true;
                email.SendEmail(message);
            }			
		}
		#endregion
		
        #region SendConfirmationEmailHtml()
        protected virtual string SendConfirmationEmailHtml(ExportUnit exportUnit, string folder_id, FtpLogin ftpLogin)
        {
            string result = "";

            Article a = exportUnit.Article;
            AdditionalFtp additional = (AdditionalFtp)exportUnit.AdditionalInfo;

            string subject = "Document Delivery Notification";
            //string      address = string.Format("<A href=\"{0}/{1}/{2}\" >{1}/{0}</A>", ftpLogin.Server, ftpLogin.Directory, fileName);			

            string address = "";
            if (folder_id[0] == '/')
            {
                address = string.Format("<A href=\"{0}{1}\" >{0}{1}</A>", ftpLogin.Server, folder_id);
            }
            else
            {
                address = string.Format("<A href=\"{0}/{1}\" >{0}/{1}</A>", ftpLogin.Server, folder_id);
            }

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

		#region FileExists()
		protected bool FileExists(string fileName)
		{
			return DirectoryExists(fileName);
		}
		#endregion

		#region DirectoryExists()
		protected bool DirectoryExists(string dirName)
		{
			IEnumerator enumerator = this.ftpConnector.GetDirListing(null);

			while (enumerator.MoveNext())
			{
				FTPFileInfo fileInfo = (FTPFileInfo)enumerator.Current;

				if (fileInfo.FileName == dirName)
					return true;
			}

			return false;
		}

		protected bool DirectoryExists(IEnumerator enumerator, string dirName)
		{
			enumerator.Reset();

			while (enumerator.MoveNext())
			{
				FTPFileInfo fileInfo = (FTPFileInfo)enumerator.Current;

				if (fileInfo.FileName == dirName)
					return true;
			}

			return false;
		}
		#endregion

		#region CopyFile()
		protected void CopyFile(FileInfo file, string fileName)
		{
			try
			{
				file.Refresh();
				this.ftpConnector.UploadFile (fileName, file);
			}
			catch(Exception ex)
			{
				throw new IllException(ErrorCode.FtpCantCopyFile, "Can't copy file '" + file.Name + "'. " + ex.Message);
			}
		}
		#endregion

		#region CreateDir()
		protected void CreateDir(string dir)
		{
			try
			{
				this.ftpConnector.CreateRemoteDir(dir);
			}
			catch(Exception ex)
			{
				throw new IllException(ErrorCode.FtpCantCreateRemoteDir, "Can't create remote directory '" + dir + "'. " + ex.Message);
			}
		}
		#endregion

		#region GetDirListing()
		protected IEnumerator GetDirListing(string filter)
		{
			try
			{
				if (this.IsConnected)
					return this.ftpConnector.GetDirListing(filter);
				else
				{
					throw new IllException(ErrorCode.FtpNotConnectedToServer);
				}
			}
			catch (Exception ex)
			{
				throw new IllException(ErrorCode.FtpCantgetRemoreDirList, "Can't obtain server Bscan ILL files list! " + ex.Message);
			}
		}
		#endregion

		#region GetNameListing()
		protected List<string> GetNameListing(string filter)
		{
			try
			{
				if (this.IsConnected)
				{
					IEnumerator enumerator = this.ftpConnector.GetDirListing(filter);
					List<string> files = new List<string>();

					while (enumerator.MoveNext())
					{
						try
						{
							files.Add(((FTPFileInfo)enumerator.Current).FileName.ToLower());
						}
						catch { }
					}
					return files;
				}
				else
				{
					throw new IllException(ErrorCode.FtpNotConnectedToServer, "Ftp, GetDirListing(): Not connected to FTP Server!");
				}
			}
			catch (Exception ex)
			{
				throw new IllException(ErrorCode.FtpCantgetRemoreDirList, "Can't obtain server Bscan ILL files list! " + ex.Message);
			}
		}
		#endregion

		#region FtpConnector_ConnectedEvent()
		void FtpConnector_ConnectedEvent(object sender, EventArgs e)
		{
			//NotifyAction("FTP Connection established successfully.");
		}
		#endregion

		#region FtpConnector_DisconnectedEvent()
		void FtpConnector_DisconnectedEvent(object sender, EventArgs e)
		{
			//NotifyAction("Disconnected from FTP server.");
		}
		#endregion

		#region FtpConnector_DownloadEvent()
		void FtpConnector_DownloadEvent(object sender, EventArgs e)
		{
			//NotifyAction("File '" + ((FTPClientConnector.TransferEventArgs)e).FileName + "' downloaded.");
		}
		#endregion

		#region FtpConnector_ListingEvent()
		void FtpConnector_ListingEvent(object sender, EventArgs e)
		{
			//NotifyAction("Listing...");
		}
		#endregion

		#region FtpConnector_ProgressEvent()
		void FtpConnector_ProgressEvent(object sender, EventArgs e)
		{
			/*FTPClientConnector.ProgressEventArgs args = (FTPClientConnector.ProgressEventArgs)e;

			if (exportForm != null)
				this.exportForm.CurrentValue = (int)Math.Max(0, Math.Min(100, args.Bytes * 100 / args.TotalBytes));*/
		}
		#endregion

		#region FtpConnector_UploadEvent()
		void FtpConnector_UploadEvent(object sender, EventArgs e)
		{
			/*NotifyAction("File '" + ((FTPClientConnector.TransferEventArgs)e).FileName + "' uploaded.");

			if (exportForm != null)
			{
				this.exportForm.CurrentValue = 99;
				//this.exportForm.CurrentArticleIndex++;
			}*/
		}
		#endregion

        #region GetSubFolderName()
        private string GetSubFolderName(AdditionalFtp additional, Article article)
        {
            string subDirName = "";
            if (additional.SaveToSubfolder)
            {
                if ((additional.SubfolderNameBase == BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn.TransactionName) &&
                    (article.TransactionId != null))
                    subDirName = article.TransactionId.ToString();
                else if ((additional.SubfolderNameBase == BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn.IllName) &&
                    (article.IllNumber != null && article.IllNumber.Trim().Length > 0))
                    subDirName = article.IllNumber.ToString();
                else
                    subDirName = article.Id.ToString();
            }
            return subDirName;
        }
        #endregion
		#endregion
	}


}
