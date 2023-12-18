using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Misc;
using System.Drawing;

// for BookEdge pull slip scanning 

namespace BscanILL.UI.Frames.Start
{
	class FrameStartTwain : IFrameStartScanner
	{
		#region variables

		FrameStart						frameStart;
		Scanners.Twain.TwainScanner		scanner = null;
		bool							isActivated = false;

		int								lastOperationId = 1;
				
		//Scanners.Twain.BookedgeSettings pullslipSettings = new Scanners.Twain.BookedgeSettings();

		#endregion


		#region constructor
		public FrameStartTwain(FrameStart frameStart, Scanners.Twain.TwainScanner scanner, bool isActive)
		{
			try
			{
				this.frameStart = frameStart;
				this.scanner = scanner;

/*
				pullslipSettings.Brightness.Value = 0;
				pullslipSettings.ColorMode.Value = Scanners.Twain.ColorMode.Bitonal;
				pullslipSettings.Contrast.Value = 0;
				pullslipSettings.DocSize.Value = Scanners.Twain.DocSize.Auto;
				pullslipSettings.Dpi.Value = 300;
				pullslipSettings.FileFormat.Value = Scanners.Twain.FileFormat.Tiff;
				pullslipSettings.ScanPage.Value = Scanners.Twain.BookedgePage.FlatMode;
*/
                        //brightness is loaded from BscanILL.temporary_pullslips.settings file
                //  _scanSettingsPullSlips.BookEdge.Brightness.Value = 0;
                //  _scanSettingsPullSlips.BookEdge.Contrast.Value = 0;

                _scanSettingsPullSlips.BookEdge.ColorMode.Value = Scanners.Twain.ColorMode.Bitonal;
                _scanSettingsPullSlips.BookEdge.DocSize.Value = Scanners.Twain.DocSize.Auto;
                _scanSettingsPullSlips.BookEdge.Dpi.Value = 300;
                _scanSettingsPullSlips.BookEdge.FileFormat.Value = Scanners.Twain.FileFormat.Tiff;
                _scanSettingsPullSlips.BookEdge.ScanPage.Value = Scanners.Twain.BookedgePage.FlatMode;

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


		//PRIVATE PROPERTIES
		#region private properties

		MainWindow						mainWindow { get { return this.frameStart.MainWindow; } }
		Notifications					notifications { get { return Notifications.Instance; } }
        BscanILL.SETTINGS.ScanSettingsPullSlips _scanSettingsPullSlips { get { return BscanILL.SETTINGS.ScanSettingsPullSlips.Instance; } }

		#endregion


		//PUBLIC METHODS	
		#region public methods
		
		#region Dispose()
		public void Dispose()
		{
			Deactivate();
			this.scanner = null;
		}
		#endregion

		#region Activate()
		public void Activate()
		{
			if (this.scanner != null && this.isActivated == false)
			{
				//pullslipSettings.Brightness.Value = BscanILL.SETTINGS.ScanSettings.Instance.BookEdge.Brightness.Value;
				//pullslipSettings.Contrast.Value = BscanILL.SETTINGS.ScanSettings.Instance.BookEdge.Contrast.Value;
                //_scanSettingsPullSlips.BookEdge.Brightness.Value = BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.BookEdge.Brightness.Value;
                //_scanSettingsPullSlips.BookEdge.Contrast.Value = BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.BookEdge.Contrast.Value;

				this.scanner.ImageScanned += new Scanners.Twain.TwainBase.ImageScannedHnd(Scanner_ImageScanned);
				this.scanner.ScanError += new Scanners.Twain.TwainBase.ScanErrorHnd(Scanner_ScanError);
				this.scanner.ProgressChanged += new Scanners.ProgressChangedHnd(Scanner_ProgressChanged);
				this.isActivated = true;
			}
		}
		#endregion

		#region Deactivate()
		public void Deactivate()
		{
			if (this.scanner != null && this.isActivated == true)
			{
				this.scanner.ImageScanned -= new Scanners.Twain.TwainBase.ImageScannedHnd(Scanner_ImageScanned);
				this.scanner.ScanError -= new Scanners.Twain.TwainBase.ScanErrorHnd(Scanner_ScanError);
				this.scanner.ProgressChanged -= new Scanners.ProgressChangedHnd(Scanner_ProgressChanged);
				this.isActivated = false;
			}
		}
		#endregion

		#region ReRegisterScanner()
		public void ReRegisterScanner()
		{

		}
		#endregion

		#region UnRegisterScanner()
		public void UnRegisterScanner()
		{

		}
		#endregion

		#region Reset()
		public void Reset()
		{
			try
			{				
				this.scanner.Reset();
			}
			catch (Exception ex)
			{
				notifications.Notify(this, Notifications.Type.Error, "FrameStartTwain, Reset(): " + ex.Message, ex);
			}
			finally
			{
			}
		}
		#endregion

		#region ScanPullslip()
		public void ScanPullslip()
		{
			try
			{
				//scanner.Scan(++this.lastOperationId, pullslipSettings);
                scanner.Scan(++this.lastOperationId, _scanSettingsPullSlips.BookEdge);

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
                notifications.Notify(this, Notifications.Type.Error, "FrameStartTwain, ScanPullslip(): " + ex.Message, ex);
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


		//PRIVATE METHODS	
		#region private methods

		#region Scanner_ImageScanned()
		void Scanner_ImageScanned(int operationId, TwainApp.TwainImage twainImage, bool moreImagesToTransfer)
		{
			this.frameStart.Scanner_ImageScanned(twainImage);
		}
		#endregion

		#region Scanner_ScanError()
		void Scanner_ScanError(int operationId, Exception ex)
		{
			this.frameStart.Scanner_ScanError(ex);
		}
		#endregion

		#region Scanner_ProgressChanged()
		void Scanner_ProgressChanged(string description, float progress)
		{
			this.frameStart.Scanner_ProgressChanged(description, progress);
		}
		#endregion

		#endregion

	}
}
