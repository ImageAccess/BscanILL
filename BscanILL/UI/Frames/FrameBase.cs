using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.UI.Frames
{
	public class FrameBase : IFrameBase, IDisposable
	{
		private BscanILL.MainWindow				mainWindow;
		bool									isLocked = false;
		bool									isActivated = false;

		//BscanILL.UI.Frames.StartUp.StartUpUi	lockUi = null;


		#region constructor
		public FrameBase(BscanILL.MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public BscanILL.MainWindow					MainWindow		{ get { return mainWindow; } }
		public bool									IsActivated		{ get { return this.isActivated; } set { this.isActivated = value; } }
		
		//protected BscanILL.UI.Dialogs.MessageDlg	MessageWindow	{ get { return messageWindow; } }
		protected bool								IsEnabled		{ get { return this.mainWindow.IsEnabled; } set { this.mainWindow.IsEnabled = value; } }
		protected bool								IsLocked		{ get { return this.isLocked; } set { this.isLocked = value; } }
		protected bool								IsCreated		{ get { return this.mainWindow.IsLoaded; } }

		#endregion

		//PROTECTED PROPERTIES
		#region protected properties

		protected BscanILL.SETTINGS.Settings _settings = BscanILL.SETTINGS.Settings.Instance;

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Dispose()
		public virtual void Dispose()
		{
			/*if (messageWindow != null)
			{
				messageWindow.Close();
				messageWindow = null;
			}*/
		}
		#endregion

		#region Lock()
		public virtual void Lock()
		{
			this.isLocked = true;
			this.MainWindow.Lock();
		}
		#endregion

		#region LockWithProgressBar()
		public virtual void LockWithProgressBar(bool progressVisible, string description)
		{
			this.isLocked = true;
			this.MainWindow.LockWithProgressBar(progressVisible, description);
		}
		#endregion

		#region UnLock()
		public void UnLock()
		{
			this.MainWindow.UnLock();
			this.isLocked = false;
		}
		#endregion

		#region LockProgressChanged()
		public virtual void LockProgressChanged(double progress)
		{
			this.MainWindow.LockProgressChanged(progress);
		}
		#endregion

		#region LockDescriptionChanged()
		public virtual void LockDescriptionChanged(string description)
		{
			this.MainWindow.LockDescriptionChanged(description);
		}
		#endregion

		#region HideLockUi()
		public virtual void HideLockUi()
		{
			this.MainWindow.HideLockUi();
		}
		#endregion

		#region ShowErrorMessage()
		public void ShowErrorMessage(string error)
		{
			BscanILL.UI.Dialogs.AlertDlg.Show(this.mainWindow, error, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
		}
		#endregion

		#region ShowWarningMessage()
		public void ShowWarningMessage(string warning)
		{
			BscanILL.UI.Dialogs.AlertDlg.Show(this.mainWindow, warning, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
		}
		#endregion

		#region ShowWarningMessage()
		public void ShowWarningMessage(BscanILL.Misc.IllException ex)
		{
			string message = BscanILL.Misc.IllException.GetErrorCodeMessage(ex.ErrorCode) + " " + ex.Message;

			BscanILL.UI.Dialogs.AlertDlg.Show(this.mainWindow, message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
		}
		#endregion

		#region Notify()
		public void Notify(object sender, BscanILL.Misc.Notifications.Type type, string methodName, string message, Exception ex)
		{
			BscanILL.Misc.Notifications.Instance.Notify(sender, type, "FrameBase, " + methodName + "(): " + message, ex);
		}
		#endregion

		#region Log()
		public void Log(string str)
		{
#if DEBUG
			Console.WriteLine(DateTime.Now.ToString("HH:mm:ss,ff") + "   " + str);
#endif
		}
		#endregion
		
		#endregion

	}
}
