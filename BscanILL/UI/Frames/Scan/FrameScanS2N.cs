using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BscanILL.Misc;

namespace BscanILL.UI.Frames.Scan
{
	class FrameScanS2N : IFrameScanScanner
	{
		protected Scanners.S2N.IS2NWrapper scanner = null;

		protected FrameScan frameScan;
		protected bool isActivated = false;

		protected int lastOperationId = 1;
		protected System.Drawing.Rectangle turboZone = System.Drawing.Rectangle.Empty;
		protected Scanners.S2N.ScannerScanAreaSelection? scanArea = null;        
		
		public event Scanners.S2N.ScanRequestHnd ScanRequest;
        public event Scanners.S2N.ScanRequestHnd ScanPullSlpRequest;        

		#region constructor
		public FrameScanS2N(FrameScan frameScan, Scanners.S2N.IS2NWrapper scanner, bool isActive)
		{
			try
			{
				this.frameScan = frameScan;
				this.scanner = scanner;

				if (isActive)
					Activate();

                this.scanner.UpdateSplittingButtonsRequest += new Scanners.S2N.ScanRequestHnd(Scanner_UpdateSplittingButtonsRequest);
			}
			catch (Exception ex)
			{
				Dispose();
				throw ex;
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public Scanners.S2N.ScannerScanAreaSelection? ScanArea { get { return this.scanArea; } set { this.scanArea = value; } }        

		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		protected MainWindow						mainWindow { get { return this.frameScan.MainWindow; } }
		protected Notifications						notifications { get { return Notifications.Instance; } }
		protected BscanILL.SETTINGS.Settings		_settings { get { return BscanILL.SETTINGS.Settings.Instance; } }
		protected BscanILL.Scan.ScannersManager		sm { get { return BscanILL.Scan.ScannersManager.Instance; } }
		protected BscanILL.SETTINGS.ScanSettings	scanSettings { get { return BscanILL.SETTINGS.ScanSettings.Instance; } }

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

		#region Scan()
		public void Scan()
		{
			try
			{
				if (sm.IsPrimaryScannerSelected)
					scanner.Scan(++this.lastOperationId, BscanILL.SETTINGS.ScanSettings.Instance.S2N);
				else
					((Scanners.Twain.AdfScanner)sm.SecondaryScanner).Scan(++this.lastOperationId, scanSettings.Adf);

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
				notifications.Notify(this, Notifications.Type.Error, "FrameScanBE2BE3, DoScan(): " + ex.Message, ex);
				throw new IllException(ex.Message);
			}
			finally
			{
			}
		}
		#endregion

        #region ScannerSettingChanged()
        public void ScannerSettingChanged()
        {
			try
			{
                if (sm.IsPrimaryScannerSelected)
                {
                    scanner.SetScannerSettings(++this.lastOperationId, BscanILL.SETTINGS.ScanSettings.Instance.S2N);
                }
			}
            catch (IllException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                notifications.Notify(this, Notifications.Type.Error, "FrameScanBE2BE3, ScannerSettingChanged(): " + ex.Message, ex);
                throw new IllException(ex.Message);
            }
        }
        #endregion


        #region ScannerSplittingSettingChanged()
        public void ScannerSplittingSettingChanged(Scanners.S2N.ScannerScanAreaSelection scanArea)
        {
			try
			{
                if (_settings.Scanner.S2NScanner.FootPedal)
                if (scanner != null)                
                {
                    scanner.UpdateTouchScreenExternal(scanArea);
                }
			}
            catch (IllException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                notifications.Notify(this, Notifications.Type.Error, "FrameScanS2N, ScannerSplittingSettingChanged(): " + ex.Message, ex);
                throw new IllException(ex.Message);
            }
        }
        #endregion


        #region Reset()
        public void Reset()
		{
			try
			{
				this.turboZone = System.Drawing.Rectangle.Empty;
				this.scanner.Reset(++this.lastOperationId);
			}
			catch (Exception ex)
			{
				notifications.Notify(this, Notifications.Type.Error, "FrameScanS2N, Reset(): " + ex.Message, ex);
			}
			finally
			{
			}
		}
		#endregion

		#region Activate()
		public void Activate()
		{
			if (this.scanner != null && this.isActivated == false)
			{
				this.scanner.ImageScanned += new Scanners.FileScannedHnd(Scanner_ImageScanned);
				this.scanner.OperationSuccessfull += new Scanners.OperationSuccessfullHnd(Scanner_OperationSuccessfull);
				this.scanner.OperationError += new Scanners.OperationErrorHnd(Scanner_ScanError);
				this.scanner.ProgressChanged += new Scanners.ProgressChangedHnd(Scanner_ProgressChanged);
				this.scanner.ScanRequest += new Scanners.S2N.ScanRequestHnd(Scanner_ScanRequest);
                this.scanner.OverrideScanArea += new Scanners.S2N.ScanRequestHnd(Scanner_SetScanArea);

                if (_settings.General.MultiArticleSupportEnabled)
                {                    
                    this.scanner.ScanPullslipRequest += new Scanners.S2N.ScanRequestHnd(Scanner_ScanPullSlipRequest); 
                }

				if (_settings.Scanner.S2NScanner.FootPedal)
				{                    
					this.scanner.IsPedalAndButtonsActive = true;
					
					scanner.SetScannerSettings(++this.lastOperationId, BscanILL.SETTINGS.ScanSettings.Instance.S2N);  //after scan pull slip we have to set Bookeye setting for regular scanning in case a foot pedal or scanner button is used for scanning. without this call it would scan regular pages with a footpedal using pull slip scan settings
																							//this is needed just for BE3 and BE5 (handled in BookeyeS2NWrapper.cs) not needed for BE4 which handles foot pedal scanning differently
																							//no need to test if BE5 here as for BE4 the method that handles the scanner setting call is not imlemented (SetScannerSettings() method in Bookeye4Wrapper.cs)

					this.scanner.ResetTouchScreenEvents();
					this.scanner.StartTouchScreenMonitoring();
                    this.scanner.UnlockScannerUi( _settings.General.MultiArticleSupportEnabled ? false : true );                    
				}
                else
                {
                    this.scanner.IsPedalAndButtonsActive = false;
                    this.scanner.StopTouchScreenMonitoring();
                    this.scanner.LockScannerUi( false );
                }

				//this.scanner.UnlockScannerUi();
				this.isActivated = true;
			}
		}
		#endregion

		#region Deactivate()
		public void Deactivate()
		{
			if (this.scanner != null && this.isActivated == true)
			{
				this.scanner.ImageScanned -= new Scanners.FileScannedHnd(Scanner_ImageScanned);
				this.scanner.OperationError -= new Scanners.OperationErrorHnd(Scanner_ScanError);
				this.scanner.ProgressChanged -= new Scanners.ProgressChangedHnd(Scanner_ProgressChanged);
				this.scanner.ScanRequest -= new Scanners.S2N.ScanRequestHnd(Scanner_ScanRequest);
                this.scanner.OverrideScanArea -= new Scanners.S2N.ScanRequestHnd(Scanner_SetScanArea);

                if (_settings.General.MultiArticleSupportEnabled)
                {
                    this.scanner.ScanPullslipRequest -= new Scanners.S2N.ScanRequestHnd(Scanner_ScanPullSlipRequest);
                }

				this.scanner.IsPedalAndButtonsActive = false;
				this.scanner.StopTouchScreenMonitoring();
				this.scanner.LockScannerUi( false );
				this.isActivated = false;
			}
		}
		#endregion
	
		#endregion


		//PRIVATE METHODS	
		#region private methods

		#region Scanner_ImageScanned()
		protected void Scanner_ImageScanned(int operationId, string filePath)
		{
			if (this.ScanArea != null)
			{
				switch (this.ScanArea.Value)
				{
					case Scanners.S2N.ScannerScanAreaSelection.Left:
						{
							//System.IO.FileInfo page = this.frameScan.GetScanPage(filePath, true);
							//File.Delete(filePath);

							//this.frameScan.Scanner_ImageScanned(page.FullName);
                            this.frameScan.Scanner_ImageScanned(filePath);       //left side splitting is handled now by Bookeye - should be faster - shorter sweep (BTW, ScanArea variable is filled just if touch screen scanning)
						} break;
					case Scanners.S2N.ScannerScanAreaSelection.Right:
						{
							//System.IO.FileInfo page = this.frameScan.GetScanPage(filePath, false);
							//File.Delete(filePath);

							//this.frameScan.Scanner_ImageScanned(page.FullName);
                            this.frameScan.Scanner_ImageScanned(filePath);       //right side splitting is handled now by Bookeye - should be faster - shorter sweep (BTW, ScanArea variable is filled just if touch screen scanning)
						} break;
					case Scanners.S2N.ScannerScanAreaSelection.Both:
						{
							string pageL, pageR;
							this.frameScan.GetScanBoth(filePath, out pageL, out pageR);

							File.Delete(filePath);

							this.frameScan.Scanner_ImagesScanned(pageL, pageR);
						} break;
					default:
						{
							this.frameScan.Scanner_ImageScanned(filePath);
						} break;
				}
			}
			else
			{
				this.frameScan.Scanner_ImageScanned(filePath);
			}
		}
		#endregion

        #region Scanner_SetScanArea
        protected void Scanner_SetScanArea(Scanners.S2N.ScannerScanAreaSelection overrideScanArea)
        {
            this.ScanArea = overrideScanArea;
        }
        #endregion

        #region Scanner_OperationSuccessfull()
        protected void Scanner_OperationSuccessfull(int operationId)
		{
			this.frameScan.Scanner_OperationSuccessdfull();
		}
		#endregion

		#region Scanner_ScanError()
		protected void Scanner_ScanError(int operationId, Exception ex)
		{
			this.frameScan.Scanner_ScanError(ex);
		}
		#endregion

		#region Scanner_ProgressChanged()
		protected void Scanner_ProgressChanged(string description, float progress)
		{
			this.frameScan.Scanner_ProgressChanged(description, progress);
		}
		#endregion

		#region Scanner_ScanRequest
		protected void Scanner_ScanRequest(Scanners.S2N.ScannerScanAreaSelection scanArea)
		{
			if (this.isActivated && ScanRequest != null)
				ScanRequest(scanArea);
		}
		#endregion

        #region Scanner_UpdateSplittingButtonsRequest
        protected void Scanner_UpdateSplittingButtonsRequest(Scanners.S2N.ScannerScanAreaSelection scanArea)
		{
           // if (this.isActivated && UpdateSplittingButtonsRequest != null)
           //     UpdateSplittingButtonsRequest(scanArea);

            //run even if Scan stage not activated - in case we are in Start stage and user touches touch screen, we want to update the splitting setting into Bscan..
            if( this.frameScan != null)
            {
                this.frameScan.UpdateSplittingButtons(scanArea);
            }
		}
		#endregion

        #region Scanner_ScanPullSlipRequest
        void Scanner_ScanPullSlipRequest(Scanners.S2N.ScannerScanAreaSelection scanArea)
        {
            if (this.isActivated && ScanPullSlpRequest != null)
                ScanPullSlpRequest(scanArea);            
        }
        #endregion

		#endregion
	}
}
