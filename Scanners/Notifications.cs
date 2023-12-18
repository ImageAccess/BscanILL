using System;
using System.IO ;
using System.Collections ;
using System.Configuration ;
using System.Diagnostics ;
using System.Net.Mail;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Windows;

namespace Scanners
{
	/// <summary>
	/// Summary description for Notifications.
	/// </summary>
	public class Notifications
	{
		public static Notifications		instance		= new Notifications();
		
		public delegate void			NotifyEventHandler(object sender, Type type, string message, System.Exception ex);
		public NotifyEventHandler		NotifyEvent;
		
		#region constructor
		private Notifications()
		{
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

		Scanners.Settings		settings = Scanners.Settings.Instance;

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Notify()
		public void Notify(object sender, Type type, string message, System.Exception ex)
		{			

#if DEBUG
			if(ex != null)
				Console.WriteLine("Notification! Type: " + type.ToString() + " Message: " + message + Environment.NewLine + Misc.GetErrorMessage(ex));
			else
				Console.WriteLine("Notification! Type: " + type.ToString() + " Message: " + message);
#endif

			if (this.NotifyEvent != null)
				this.NotifyEvent(sender, type, message, ex);
		}
		#endregion

		#endregion



	}
}
