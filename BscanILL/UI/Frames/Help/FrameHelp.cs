using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using System.IO;
using System.Drawing.Printing;
using System.Windows.Input;
using System.Windows;
using System.Runtime.InteropServices;
using BscanILL.Misc;


namespace BscanILL.UI.Frames.Help
{
	public class FrameHelp : FrameBase
	{
		#region variables

		private BscanILL.UI.Frames.Help.FrameHelpUi	frameHelpUi;

		#endregion


		#region constructor
		public FrameHelp(BscanILL.MainWindow mainWindow)
			: base(mainWindow)
		{
			this.frameHelpUi = this.MainWindow.FrameHelpUi;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public System.Windows.Controls.UserControl	UserControl { get { return (System.Windows.Controls.UserControl)this.frameHelpUi; } }

		#endregion


		//PUBLIC METHODS
		#region public methods
	
		#region Activate()
		public void Activate()
		{
			this.frameHelpUi.Open();
			
			this.IsActivated = true;
		}
		#endregion

		#region Deactivate()
		public void Deactivate()
		{
			Reset();
			this.frameHelpUi.Visibility = Visibility.Hidden;
			
			this.IsActivated = false;
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			try
			{
				this.frameHelpUi.Reset();

				ReleaseMemory();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
			finally
			{
			}
		}
		#endregion

		#region ReleaseMemory()
		private void ReleaseMemory()
		{
			BscanILL.Misc.MemoryManagement.ReleaseUnusedMemory();
		}
		#endregion

		#region Dispose()
		public override void Dispose()
		{
			base.Dispose();
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region UserInteracted()
		void UserInteracted()
		{
		}
		#endregion

		#endregion

	}
}
