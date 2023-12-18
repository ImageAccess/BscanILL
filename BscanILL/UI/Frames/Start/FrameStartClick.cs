using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Misc;
using System.Drawing;

namespace BscanILL.UI.Frames.Start
{
	class FrameStartClick : BscanILL.UI.Frames.Shared.FrameClick, IFrameStartScanner
	{
		FrameStart					frameStart;

		//Scanners.Click.ClickSettings pullslipSettings = new Scanners.Click.ClickSettings();

		#region constructor
		public FrameStartClick(FrameStart frameStart, Scanners.Click.ClickWrapper click, bool isActive)
			: base(frameStart, frameStart.UserControl.Dispatcher, frameStart.MainWindow, click, isActive)
		{
			try
			{
				this.frameStart = frameStart;

/*
				pullslipSettings.Brightness.Value = 0;
				pullslipSettings.ColorMode.Value = Scanners.Click.ClickColorMode.Bitonal;
				pullslipSettings.Contrast.Value = 0;
				pullslipSettings.Dpi.Value = 300;
				pullslipSettings.FileFormat.Value = Scanners.FileFormat.Tiff;
				pullslipSettings.ScanPage.Value = Scanners.Click.ClickScanPage.Right;
				pullslipSettings.ScanMode.Value = Scanners.Click.ClickScanMode.SplitImage;
				pullslipSettings.Focus.Value = false;
*/
                //brightness is loaded from BscanILL.temporary_pullslips.settings file
                //scanSettingsPullSlips.Click.Brightness.Value = 0;       Bob: guessing brightness value is in range <-1,1>   1 light, -1 dark
                //scanSettingsPullSlips.Click.Contrast.Value = 0;

                scanSettingsPullSlips.Click.ColorMode.Value = Scanners.Click.ClickColorMode.Bitonal;
                scanSettingsPullSlips.Click.Dpi.Value = 300;
                scanSettingsPullSlips.Click.FileFormat.Value = Scanners.FileFormat.Tiff;
                scanSettingsPullSlips.Click.ScanPage.Value = Scanners.Click.ClickScanPage.Right;
                scanSettingsPullSlips.Click.ScanMode.Value = Scanners.Click.ClickScanMode.SplitImage;
                scanSettingsPullSlips.Click.Focus.Value = false;

				if (isActive)
					Activate();
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

		#region ScanPullslip()
		public void ScanPullslip()
		{
			try
			{
				//_scanner.Scan(pullslipSettings);
                _scanner.Scan(scanSettingsPullSlips.Click);

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
				notifications.Notify(this, Notifications.Type.Error, "FrameStartClick, DoScan(): " + ex.Message, ex);
				throw new IllException(ex.Message);
			}
			finally
			{
			}
		}
		#endregion

        #region TouchApp_Request()
        public int TouchApp_Request()
        {
            return 0;
        }
        #endregion
        
		#endregion

		//PROTECTED METHODS	
		#region protected methods

		#region Scanner_ImageScanned()
		protected override void Scanner_ImageScanned(Bitmap bitmap, Scanners.Click.ScanPage scanPage)
		{
			this.frameStart.Scanner_ImageScanned(bitmap);
		}
		#endregion

		#region Scanner_ScanError()
		protected override void Scanner_ScanError(Exception ex)
		{
			this.frameStart.Scanner_ScanError(ex);
		}
		#endregion

		#region Scanner_ProgressChanged()
		protected override void Scanner_ProgressChanged(string description, float progress)
		{
			this.frameStart.Scanner_ProgressChanged(description, progress);
		}
		#endregion

		#endregion
	
	}
}
