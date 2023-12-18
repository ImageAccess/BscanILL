using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Misc;


namespace BscanILL.UI.Frames.Scan
{
    class FrameScanAdf : IFrameScanScanner
    {
        #region variables

        FrameScan frameScan;
        Scanners.Twain.AdfScanner scanner = null;
        bool isActivated = false;

        int lastOperationId = 1;
        //System.Drawing.Rectangle turboZone = System.Drawing.Rectangle.Empty;

        #endregion

		#region constructor
        public FrameScanAdf(FrameScan frameScan, Scanners.Twain.AdfScanner scanner, bool isActive)
		{
			try
			{
				this.frameScan = frameScan;
				this.scanner = scanner;

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

        MainWindow mainWindow { get { return this.frameScan.MainWindow; } }
        Notifications notifications { get { return Notifications.Instance; } }
        BscanILL.SETTINGS.Settings _settings = BscanILL.SETTINGS.Settings.Instance;
        BscanILL.Scan.ScannersManager sm { get { return BscanILL.Scan.ScannersManager.Instance; } }
        BscanILL.SETTINGS.ScanSettings scanSettings { get { return BscanILL.SETTINGS.ScanSettings.Instance; } }

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
            if (this.isActivated == false)
            {
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

        #region Reset()
        public void Reset()
        {
            try
            {
                //this.turboZone = System.Drawing.Rectangle.Empty;
                this.scanner.Reset();
            }
            catch (Exception ex)
            {
                notifications.Notify(this, Notifications.Type.Error, "FrameScanAdf, Reset(): " + ex.Message, ex);
            }
            finally
            {
            }
        }
        #endregion

        #region Scan()
        public void Scan()
        {
            try
            {
                if (sm.IsPrimaryScannerSelected)
                    scanner.Scan(++this.lastOperationId, scanSettings.Adf);
                //else                                                                  //secondary scanner not supported when Adf set as primary
                //    ((Scanners.Twain.AdfScanner)sm.SecondaryScanner).Scan(++this.lastOperationId, scanSettings.Adf);

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
                notifications.Notify(this, Notifications.Type.Error, "FrameScanAdf, DoScan(): " + ex.Message, ex);
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
        void Scanner_ImageScanned(int operationId, TwainApp.TwainImage twainImage, bool moreImagesToTransfer)
        {
            this.frameScan.Scanner_ImageScanned(twainImage, moreImagesToTransfer);
        }
        #endregion

        #region Scanner_ScanError()
        void Scanner_ScanError(int operationId, Exception ex)
        {
            this.frameScan.Scanner_ScanError(ex);
        }
        #endregion

        #region Scanner_ScanTimeout()
        void Scanner_ScanTimeout()
        {
            this.frameScan.Scanner_ScanError(new Exception("Timeout occured!"));
        }
        #endregion

        #region Scanner_ProgressChanged()
        void Scanner_ProgressChanged(string description, float progress)
        {
            this.frameScan.Scanner_ProgressChanged(description, progress);
        }
        #endregion

        #endregion

    }
}
