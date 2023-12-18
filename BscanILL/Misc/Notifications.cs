using System;
using System.IO ;
using System.Collections ;
using System.Configuration ;
using System.Diagnostics ;
using System.Net.Mail;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Windows;
using BscanILL.Export.Email;

namespace BscanILL.Misc
{
	/// <summary>
	/// Summary description for Notifications.
	/// </summary>
	public class Notifications
	{
		public static Notifications		instance = new Notifications();
		FileInfo						logFile			= null ;
		string							adminEmail		= "" ;
		
		List<MessageRecord>				notificationsCache = new List<MessageRecord>();
		List<MessageRecord>				errorsCache = new List<MessageRecord>();

		public delegate	void			NotifyEventHandler(object sender, NotifyEventArgs e) ;

		BscanILL.SETTINGS.Settings		_settings = BscanILL.SETTINGS.Settings.Instance;
		
		#region constructor
		public Notifications()
		{
			try
			{		
				//logFile file
				if (_settings.StatsAndNotifs.LogToLogFile)
				{
					logFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\Logs\" + DateTime.Now.ToString("yyyy-MM") + ".txt");
					logFile.Directory.Create();
					if (!logFile.Exists)
						logFile.Create();
				}
			
				//email to admin
				string		setupValue = _settings.StatsAndNotifs.AdditionalAdminRecipients;
				if (_settings.Export.Email.Enabled && setupValue.Trim().Length > 0)
				{
					bool validEmail = true;
					string[] recipArray = setupValue.Split(string.Format(",;").ToCharArray());

					foreach (string recip in recipArray)
					{
						if (!BscanILL.Export.Email.Email.IsValidEmail(recip.Trim()))
							validEmail = false;
					}

					if (!validEmail)
						throw new Exception("Wrong data in Config! Group: Notifications, Item:Admin Emails. E-mail address is not valid. " +
							"The value must be valid e-mail address, group of e-mail adresses divided by ',' or to be empty.");

					adminEmail = setupValue;
				}

			}
			catch(Exception ex)
			{
				MessageBox.Show(null, "Notification exception: " + ex.Message + Environment.NewLine + Environment.NewLine + "Check Bscan ILL configuration.", "", MessageBoxButton.OK, MessageBoxImage.Error);

				throw new Exception("Notification exception: " + ex.Message + Environment.NewLine + Environment.NewLine + "Check Bscan ILL configuration.");
			}
		}
		#endregion

		#region class NotifyEventArgs
		public class NotifyEventArgs : EventArgs
		{
			public Type		type ;
			public string			message ;
			public Exception		exception = null ;
			public string			subject;

			public NotifyEventArgs(Type type, string errorMessage, Exception exception)
			{
				this.type = type ;
				this.message = errorMessage ;
				this.exception = exception ;
				this.subject = "";
			}

			public NotifyEventArgs(Type type, string message, Exception exception, string subject)
			{
				this.type = type ;
				this.message = message ;
				this.exception = exception ;
				this.subject = subject;
			}
		}
		#endregion

		#region class MessageRecord
		class MessageRecord
		{
			public readonly DateTime Time = DateTime.Now;
			public readonly string Message;

			public MessageRecord(string message)
			{
				this.Message = message;
			}
		}
		#endregion

		#region class DeveloperRecord
		[Serializable]
		public class DeveloperRecord
		{
			public Guid GUID;
			public string ComputerName;
			public string MacAddress;
			public string IllVersion;
			public DateTime Date;
			public string Message;
			public string StackTrace;
			public string Source;
			public string EnvironmentStackTrace;

			public DeveloperRecord()
			{
			}

			public DeveloperRecord(object sender, NotifyEventArgs args)
			{
				GUID = Guid.NewGuid();
				ComputerName = Environment.MachineName;
				MacAddress = BscanILL.Misc.Misc.GetMacAddress();
				IllVersion = BscanILL.Misc.Misc.Version;
				Date = DateTime.Now;

				if (sender != null)
					Message += sender.GetType().Name + ", " + args.message;
				else
					Message += args.message;

				if (args.exception != null)
				{
					StackTrace = args.exception.StackTrace;
					Source = args.exception.Source;
				}

				EnvironmentStackTrace = Environment.StackTrace;
			}

		}
		#endregion

		#region class DeveloperRecords 
		[Serializable]
		public class DeveloperRecords : List<DeveloperRecord>
		{
			public DeveloperRecords()
			{
			}
		}
		#endregion

		#region enum Type
		public enum Type : byte
		{
			Information,
			SuccessAudit,
			FailureAudit,
			Warning,
			Error
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		public static Notifications		Instance	{ get{return instance;} }
		#endregion


		// PRIVATE PROPERTIES
		#region private properties
		static BscanILL.Export.Email.Email email { get { return BscanILL.Export.Email.Email.Instance; } }

		#endregion



		//PUBLIC METHODS
		#region public methods

		#region Notify()
		private void Notify(object sender, NotifyEventArgs args)
		{			
			Type	messageType = args.type ;
			string		subject = args.subject ;
			string		message = args.message ;

#if DEBUG
			Console.WriteLine("Notification! Type: " + args.type.ToString() + " Message: " + args.message) ;
#endif
			
			if(logFile != null)
				LogFileNotification(messageType, message) ;

			if((messageType == Type.Warning)/* && (adminEmail.Length > 0)*/)
				AdminNotification(messageType, message, subject) ;
		
			if((messageType == Type.Error) /*&& (developerEmail.Length > 0)*/)
				DeveloperNotification(sender, args) ;
		}

		private void Notify(object sender, NotifyEventArgs args, List<FileInfo> images)
		{
			Type messageType = args.type;
			string subject = args.subject;
			string message = args.message;

#if DEBUG
			Console.WriteLine("Notification! Type: " + args.type.ToString() + " Message: " + args.message) ;
#endif

			if (logFile != null)
				LogFileNotification(messageType, message);

			if ((messageType == Type.Warning) /*&& (adminEmail.Length > 0)*/)
				AdminNotification(messageType, message, subject);

			if ((messageType == Type.Error) /*&& (developerEmail.Length > 0)*/)
				DeveloperNotification(sender, args);
		}

		public void Notify(object sender, Type type, string message, System.Exception ex)
		{			
			Notify(sender, new Notifications.NotifyEventArgs(type, message, ex));
		}

		public void Notify(object sender, Type type, string message, System.Exception ex, string subject)
		{			
			Notify(sender, new Notifications.NotifyEventArgs(type, message, ex, subject));
		}

		public void Notify(object sender, Type type, string message, System.Exception ex, FileInfo image)
		{
			if (image != null)
			{
				List<FileInfo> images = new List<FileInfo>();

				images.Add(image);

				Notify(sender, new Notifications.NotifyEventArgs(type, message, ex), images);
			}
			else
				Notify(sender, new Notifications.NotifyEventArgs(type, message, ex));
		}

		public void Notify(object sender, Type type, string message, System.Exception ex, List<FileInfo> images)
		{
			Notify(sender, new Notifications.NotifyEventArgs(type, message, ex), images);
		}
		#endregion

		#region ToAlertType()
		public static BscanILL.UI.Dialogs.AlertDlg.AlertDlgType ToAlertType(Type type)
		{
			switch (type)
			{
				case Type.Information: return BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Information;
				case Type.Warning: return BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning;
				case Type.SuccessAudit: return BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Information;
				default: return BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error;
			}
		}
		#endregion

		#region ReloadInstance()
		public static void ReloadInstance()
		{
			instance = new Notifications();
		}
		#endregion

		#region SendDeveloperReportIfNecessary()
		/// <summary>
		/// if error file exists, it compresses it to a new file and sends it.
		/// </summary>
		public void SendDeveloperReportIfNecessary()
		{
			try
			{
				if (_settings.StatsAndNotifs.SendToDLSG)
				{
					FileInfo errorFile = GetErrorFile();

					if (errorFile.Exists)
					{
						string message = "Bscan ILL Developer Report" + Environment.NewLine + Environment.NewLine +
							"Computer: " + Environment.MachineName + Environment.NewLine +
							"MAC Address: " + BscanILL.Misc.Misc.GetMacAddress() + Environment.NewLine +
							"Bscan ILL Version: " + BscanILL.Misc.Misc.Version + Environment.NewLine +
							"Time: " + DateTime.Now.ToString("HH:mm:ss,ff") + Environment.NewLine + Environment.NewLine;

						string errorFileContent = "";

						using (FileStream fileStream = errorFile.OpenRead())
						{
							using (StreamReader reader = new StreamReader(fileStream))
							{
								errorFileContent = reader.ReadToEnd();
							}
						}

						FileInfo compressedFile = new FileInfo(string.Format("{0}\\{1}.gz", errorFile.Directory.FullName, Path.GetFileNameWithoutExtension(errorFile.FullName)));

						if (compressedFile.Exists)
							compressedFile.Delete();

						using (MemoryStream stream = BscanILL.Misc.Io.Compress(errorFileContent))
						{
							using (FileStream fileStream = new FileStream(compressedFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
							{
								stream.WriteTo(fileStream);
							}
						}

						DlsgEmail.Instance.SendEmail(DlsgEmail.SendTo.Developers, "Developer message from " + Environment.MachineName, message, compressedFile);
					}
				}
			}
			catch { }
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region LogFileNotification()
		private void LogFileNotification(Type messageType, string message) 
		{
			try
			{
				if (logFile != null)
				{
					using (StreamWriter writer = new StreamWriter(logFile.FullName, true))
					{
						writer.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + messageType.ToString() + ": " + message);
					}
				}
			}
			catch
			{
			}
		}
		#endregion

		#region AdminNotification()
		private void AdminNotification(Type messageType, string message, string subject) 
		{
			try
			{
				if (CanSendAdminEmail(message))
				{
					message = "Bscan ILL Notification" + Environment.NewLine + Environment.NewLine +
						"Computer: " + Environment.MachineName + Environment.NewLine +
						"MAC Address: " + BscanILL.Misc.Misc.GetMacAddress() + Environment.NewLine +
						"Bscan ILL Version: " + BscanILL.Misc.Misc.Version + Environment.NewLine +
						"Time: " + DateTime.Now.ToString("HH:mm:ss,ff") + Environment.NewLine + Environment.NewLine +
						message;

					if (subject.Length == 0)
						subject = "Bscan ILL Notification from " + Environment.MachineName;

					if (this.adminEmail != null && this.adminEmail.Trim().Length > 0)
					{
						using (MailMessage emailMessage = email.GetMessage(adminEmail, subject, message))
						{
							email.SendEmail(emailMessage);
						}
					}

					if (BscanILL.SETTINGS.Settings.Instance.StatsAndNotifs.SendToDLSG)
						DlsgEmail.Instance.SendEmail(DlsgEmail.SendTo.Admins, subject, message);
					
					this.notificationsCache.Add(new MessageRecord(message));
				}
			}
			catch { }
		}
		#endregion

		#region DeveloperNotification()
		private void DeveloperNotification(object sender, NotifyEventArgs args)
		{
			try
			{
				if (CanLogErrorMessage(args.message))
				{
					FileInfo file = GetErrorFile();
					DeveloperRecords devRecords;

					if (file.Exists)
					{
						using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeveloperRecords));
							devRecords = (DeveloperRecords)xmlSerializer.Deserialize(fileStream);
						}
					}
					else
						devRecords = new DeveloperRecords();

					DeveloperRecord devRecord = new DeveloperRecord(sender, args);
					devRecords.Add(devRecord);

					using (FileStream fileStream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
					{
						XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeveloperRecords));
						xmlSerializer.Serialize(fileStream, devRecords);
					}

					this.errorsCache.Add(new MessageRecord(args.message));
				}
			}
			catch { }
		}
		#endregion

		#region GetErrorFile()
		private FileInfo GetErrorFile()
		{
			if (_settings.StatsAndNotifs.LogToLogFile)
			{
				FileInfo file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\Logs\DM" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
				file.Directory.Create();

				return file;
			}

			return null;
		}
		#endregion

		#region CanSendAdminEmail()
		/// <summary>
		/// true if the same notif was not sent in last 30 minutes
		/// </summary>
		/// <returns></returns>
		private bool CanSendAdminEmail(string message)
		{
			for(int i = notificationsCache.Count - 1; i >= 0; i--)
				if (DateTime.Now.Subtract(notificationsCache[i].Time).TotalMinutes > 30)
					notificationsCache.RemoveAt(i);

			for (int i = notificationsCache.Count - 1; i >= 0; i--)
				if (notificationsCache[i].Message == message)
					return false;

			return true;
		}
		#endregion

		#region CanLogErrorMessage()
		/// <summary>
		/// true if the same error message was not logged in in last 10 minutes
		/// </summary>
		/// <returns></returns>
		private bool CanLogErrorMessage(string message)
		{
			for (int i = this.errorsCache.Count - 1; i >= 0; i--)
				if (DateTime.Now.Subtract(errorsCache[i].Time).TotalMinutes > 10)
					errorsCache.RemoveAt(i);

			for (int i = errorsCache.Count - 1; i >= 0; i--)
				if (errorsCache[i].Message == message)
					return false;

			return true;
		}
		#endregion

		#endregion

	}
}
