using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.UI.Frames
{
	public interface IFrameBase : IDisposable
	{

		// PROPERTIES
		BscanILL.MainWindow MainWindow { get; }
		bool IsActivated { get; set; }

		// METHODS
		void Lock();
		void LockWithProgressBar(bool progressVisible, string description);
		void UnLock();
		void LockProgressChanged(double progress);
		void LockDescriptionChanged(string description);
		void HideLockUi();
		void ShowErrorMessage(string error);
		void ShowWarningMessage(string warning);
		void ShowWarningMessage(BscanILL.Misc.IllException ex);
		void Notify(object sender, BscanILL.Misc.Notifications.Type type, string methodName, string message, Exception ex);
		void Log(string str);

	}
}
