using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Misc;
using System.Drawing;

namespace BscanILL.UI.Frames.Scan
{
	class FrameScanClickMini : BscanILL.UI.Frames.Shared.FrameClickMini, IFrameScanScanner
	{
		FrameScan						frameScan;


		#region constructor
		public FrameScanClickMini(FrameScan frameScan, Scanners.Click.ClickMiniWrapper click, bool isActive)
			: base(frameScan, frameScan.UserControl.Dispatcher, frameScan.MainWindow, click, isActive)
		{
			try
			{
				this.frameScan = frameScan;
			}
			catch (Exception ex)
			{
				Dispose();
				throw ex;
			}
		}
		#endregion


		//PUBLIC METHODS	
		#region public methods

		#region Scan()
		public void Scan()
		{
			try
			{
				if (sm.IsPrimaryScannerSelected)
					_scanner.Scan(scanSettings.ClickMini);
				else
					((Scanners.Twain.AdfScanner)sm.SecondaryScanner).Scan(++_lastOperationId, scanSettings.Adf);

#if !DEBUG
				this.mainWindow.Activate();
#endif
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				notifications.Notify(this, Notifications.Type.Error, "FrameScanClickMini, DoScan(): " + ex.Message, ex);
				throw new IllException(ex.Message);
			}
			finally
			{
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS	
		#region private methods

		#region Scanner_ImageScanned()
		protected override void Scanner_ImageScanned(Bitmap bitmap)
		{
			this.frameScan.Scanner_ImageScanned(bitmap);
		}
		#endregion

		#region Scanner_ImagesScanned()
		protected override void Scanner_ImagesScanned(Bitmap bitmapL, Bitmap bitmapR)
		{
			this.frameScan.Scanner_ImagesScanned(bitmapL, bitmapR);
		}
		#endregion

		#region Scanner_ScanError()
		protected override void Scanner_ScanError(Exception ex)
		{
			this.frameScan.Scanner_ScanError(ex);
		}
		#endregion

		#region Scanner_ProgressChanged()
		protected override void Scanner_ProgressChanged(string description, float progress)
		{
			this.frameScan.Scanner_ProgressChanged(description, progress);
		}
		#endregion

		#endregion
	
	}
}
