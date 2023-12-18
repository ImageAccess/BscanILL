using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Misc;
using System.Drawing;

namespace BscanILL.UI.Frames.Start
{
    class FrameStartAdf : IFrameStartScanner
    {

        #region variables

        FrameStart frameStart;
        Scanners.Twain.AdfScanner scanner = null;
        bool isActivated = false;

        int lastOperationId = 1;

        //Scanners.Twain.AdfSettings pullslipSettings = new Scanners.Twain.AdfSettings();

        #endregion

		#region constructor
        public FrameStartAdf(FrameStart frameStart, Scanners.Twain.AdfScanner scanner, bool isActive)
		{
			try
			{
				this.frameStart = frameStart;
				this.scanner = scanner;

                        //brightness is loaded from BscanILL.temporary_pullslips.settings file
                //  _scanSettingsPullSlips.Adf.Brightness.Value = 0;
                //  _scanSettingsPullSlips.Adf.Contrast.Value = 0;

                _scanSettingsPullSlips.Adf.ColorMode.Value = Scanners.Twain.ColorMode.Bitonal;
                _scanSettingsPullSlips.Adf.DocSize.Value = Scanners.Twain.DocSize.Auto;
                _scanSettingsPullSlips.Adf.Dpi.Value = 300;
                _scanSettingsPullSlips.Adf.FileFormat.Value = Scanners.Twain.FileFormat.Tiff;
                _scanSettingsPullSlips.Adf.Duplex.Value = false;
                _scanSettingsPullSlips.Adf.TransferCount.Value = 1;
                
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

        MainWindow mainWindow { get { return this.frameStart.MainWindow; } }
        Notifications notifications { get { return Notifications.Instance; } }
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
                //_scanSettingsPullSlips.Brightness.Value = BscanILL.SETTINGS.ScanSettings.Instance.BookEdge.Brightness.Value;
                //_scanSettingsPullSlips.Contrast.Value = BscanILL.SETTINGS.ScanSettings.Instance.BookEdge.Contrast.Value;

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
                notifications.Notify(this, Notifications.Type.Error, "FrameStartAdf, Reset(): " + ex.Message, ex);
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
                scanner.Scan(++this.lastOperationId, _scanSettingsPullSlips.Adf);

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
				notifications.Notify(this, Notifications.Type.Error, "FrameStartAdf, ScanPullSlip(): " + ex.Message, ex);
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
