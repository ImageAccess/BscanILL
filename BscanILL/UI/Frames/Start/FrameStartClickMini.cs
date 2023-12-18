using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Misc;
using System.Drawing;

namespace BscanILL.UI.Frames.Start
{
	class FrameStartClickMini : BscanILL.UI.Frames.Shared.FrameClickMini, IFrameStartScanner
	{
		FrameStart					frameStart;

		//Scanners.Click.ClickMiniSettings pullslipSettings = new Scanners.Click.ClickMiniSettings();

		#region constructor
		public FrameStartClickMini(FrameStart frameStart, Scanners.Click.ClickMiniWrapper click, bool isActive)
			: base(frameStart, frameStart.UserControl.Dispatcher, frameStart.MainWindow, click, isActive)
		{
			try
			{
				this.frameStart = frameStart;
/*
                pullslipSettings.Brightness.Value = -0.18;  //was 0      Bob: guessing brightness value is in range <-1,1>   1 light, -1 dark
				pullslipSettings.ColorMode.Value = Scanners.Click.ClickColorMode.Bitonal;
				pullslipSettings.Contrast.Value = 0;
				pullslipSettings.Dpi.Value = 300;
				pullslipSettings.FileFormat.Value = Scanners.FileFormat.Tiff;
				pullslipSettings.ScanMode.Value = Scanners.Click.ClickMiniScanMode.SingleScan;
				pullslipSettings.Focus.Value = false;
*/
                //brightness is loaded from BscanILL.temporary_pullslips.settings file
                //scanSettingsPullSlips.ClickMini.Brightness.Value = -0.18;  //was 0      Bob: guessing brightness value is in range <-1,1>   1 light, -1 dark
                //scanSettingsPullSlips.ClickMini.Contrast.Value = 0;

                scanSettingsPullSlips.ClickMini.ColorMode.Value = Scanners.Click.ClickColorMode.Bitonal;                
                scanSettingsPullSlips.ClickMini.Dpi.Value = 300;
                scanSettingsPullSlips.ClickMini.FileFormat.Value = Scanners.FileFormat.Tiff;
                scanSettingsPullSlips.ClickMini.ScanMode.Value = Scanners.Click.ClickMiniScanMode.SingleScan;
                scanSettingsPullSlips.ClickMini.Focus.Value = false;

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
                _scanner.Scan(scanSettingsPullSlips.ClickMini);

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
                notifications.Notify(this, Notifications.Type.Error, "FrameStartClickMini, ScanPullslip(): " + ex.Message, ex);
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
		protected override void Scanner_ImageScanned(Bitmap bitmap)
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
