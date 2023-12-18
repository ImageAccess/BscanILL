using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Misc;

namespace BscanILL.UI.Frames.Start
{
	class FrameStartS2N : IFrameStartScanner
	{
		protected Scanners.S2N.IS2NWrapper scanner = null;
		protected FrameStart frameStart;
		protected bool isActivated = false;

		protected int lastOperationId = 1;
		protected System.Drawing.Rectangle turboZone = System.Drawing.Rectangle.Empty;

		protected Scanners.DeviceInfo s2nInfo;
		//protected Scanners.S2N.S2NSettings pullslipSettings = new Scanners.S2N.S2NSettings();


		#region constructor
		public FrameStartS2N(FrameStart frameStart, Scanners.S2N.IS2NWrapper scanner, bool isActive)
		{
			try
			{
				this.frameStart = frameStart;
				this.scanner = scanner;

                /*
                                pullslipSettings.CopyFrom(scanner.ScannerSettings);                 
                                pullslipSettings.DocSize.Value = Scanners.S2N.DocumentSize.Auto;
                                pullslipSettings.ColorMode.Value = Scanners.S2N.ColorMode.Lineart;
                                pullslipSettings.FileFormat.Value = Scanners.S2N.FileFormat.Tiff;
                                pullslipSettings.TiffCompression.Value = Scanners.S2N.TiffCompression.G4;
                                pullslipSettings.Dpi.Value = 300;

                                if (scanner is Scanners.S2N.Bookeye4Wrapper)
                                {
                                    pullslipSettings.DocMode.Value = Scanners.S2N.DocumentMode.Auto;
                                }
                                else
                                {
                                    pullslipSettings.DocMode.Value = Scanners.S2N.DocumentMode.Flat;
                                }

                                pullslipSettings.BitonalThreshold.Value = Scanners.S2N.BitonalThreshold.Fixed;
                                pullslipSettings.Bidirectional.Value = true;
*/
                int tempBright = _scanSettingsPullSlips.S2N.Brightness.Value;
                int tempContr = _scanSettingsPullSlips.S2N.Contrast.Value;

				s2nInfo = Scanners.S2N.ScannerS2N.GetDeviceInfo(_settings.Scanner.S2NScanner.Ip);

				_scanSettingsPullSlips.S2N.CopyFrom(scanner.ScannerSettings);

                _scanSettingsPullSlips.S2N.Brightness.Value = tempBright ;          //do not overwire brightness/contrast with settings for regular pages as these values are stored for pull slips in separate settings file for pull slips
                _scanSettingsPullSlips.S2N.Contrast.Value = tempContr;

                        //brightness is loaded from BscanILL.temporary_pullslips.settings file
                //  _scanSettingsPullSlips.S2N.Brightness.Value = 127;
                //  _scanSettingsPullSlips.S2N.Contrast.Value = 127;
                _scanSettingsPullSlips.S2N.DocSize.Value = Scanners.S2N.DocumentSize.Auto;
                _scanSettingsPullSlips.S2N.ColorMode.Value = Scanners.S2N.ColorMode.Lineart;
                _scanSettingsPullSlips.S2N.FileFormat.Value = Scanners.S2N.FileFormat.Tiff;
                _scanSettingsPullSlips.S2N.TiffCompression.Value = Scanners.S2N.TiffCompression.G4;
                _scanSettingsPullSlips.S2N.Dpi.Value = 300;

				AdjustPullSlipSettings(_scanSettingsPullSlips.S2N.DocMode.Value);

                _scanSettingsPullSlips.S2N.BitonalThreshold.Value = Scanners.S2N.BitonalThreshold.Fixed;
                _scanSettingsPullSlips.S2N.Bidirectional.Value = true;
               // _scanSettingsPullSlips.S2N.Splitting.Value = Scanners.S2N.Splitting.Off;

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

		MainWindow					mainWindow { get { return this.frameStart.MainWindow; } }
		Notifications				notifications { get { return Notifications.Instance; } }
		BscanILL.SETTINGS.Settings	_settings { get { return BscanILL.SETTINGS.Settings.Instance; } }
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
				//pullslipSettings.Brightness.Value = BscanILL.SETTINGS.ScanSettings.Instance.S2N.Brightness.Value;  //after separate pull lsip settings got implemented, do not overwrite the value with regular page settings
				//pullslipSettings.Contrast.Value = BscanILL.SETTINGS.ScanSettings.Instance.S2N.Contrast.Value;

				this.scanner.ImageScanned += new Scanners.FileScannedHnd(Scanner_ImageScanned);
				this.scanner.OperationError += new Scanners.OperationErrorHnd(Scanner_ScanError);
				this.scanner.ProgressChanged += new Scanners.ProgressChangedHnd(Scanner_ProgressChanged);
				this.scanner.ScanRequest += new Scanners.S2N.ScanRequestHnd(Scanner_ScanRequest);
				this.scanner.ScanPullslipRequest += new Scanners.S2N.ScanRequestHnd(Scanner_ScanRequest);

				if (_settings.Scanner.S2NScanner.FootPedal)
				{
                    SetTouchScreenSplittingButtons();
					this.scanner.ResetTouchScreenEvents();
					this.scanner.StartTouchScreenMonitoring();
                    this.scanner.UnlockScannerUi(false); 
				}
                else
                {
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
				this.scanner.ScanPullslipRequest -= new Scanners.S2N.ScanRequestHnd(Scanner_ScanRequest);

				this.scanner.StopTouchScreenMonitoring();
				this.scanner.LockScannerUi( false );
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

		#region TouchApp_Request()
		public int TouchApp_Request()
        {
            int version = 0;
            if (this.scanner != null)
            {
                this.scanner.ResetTouchScreenEvents();

                this.scanner.RequestTouchScreenVersion();
                System.Threading.Thread.Sleep(200);

                this.scanner.GetTouchScreenVersion(false);

                version = this.scanner.HtmlTouchScreenAppVersion;
            }
            return version;
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
				notifications.Notify(this, Notifications.Type.Error, "FrameStartS2N, Reset(): " + ex.Message, ex);
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
				this.turboZone = System.Drawing.Rectangle.Empty;
				//scanner.Scan(++this.lastOperationId, this.pullslipSettings);

				AdjustPullSlipSettings(scanner.ScannerSettings.DocMode.Value);

				scanner.Scan(++this.lastOperationId, _scanSettingsPullSlips.S2N);                

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
				notifications.Notify(this, Notifications.Type.Error, "FrameStartS2N, DoScan(): " + ex.Message, ex);
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

        #region SetTouchScreenSplittingButtons();
        void SetTouchScreenSplittingButtons()
        {
            this.scanner.UpdateTouchScreenExternal((Scanners.S2N.ScannerScanAreaSelection)BscanILL.SETTINGS.ScanSettings.Instance.S2N.Splitting.Value);
        }
        #endregion

		#region Scanner_ImageScanned()
		void Scanner_ImageScanned(int operationId, string filePath)
		{
			this.frameStart.Scanner_ImageScanned(filePath);
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

		#region Scanner_ScanRequest
		void Scanner_ScanRequest(Scanners.S2N.ScannerScanAreaSelection scanArea)
		{
			if (this.isActivated)
				this.frameStart.FrameScanBookeye4_ScanRequest(scanArea);
		}
		#endregion

		#region AdjustPullSlipSettings
		void AdjustPullSlipSettings( Scanners.S2N.DocumentMode docMode )
		{					
				if(  (scanner is Scanners.S2N.Bookeye4Wrapper) || ( (s2nInfo != null ) && (s2nInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE5)) )
				{
				    //if BE4 or BE5force Auto mode
					_scanSettingsPullSlips.S2N.DocMode.Value = Scanners.S2N.DocumentMode.Auto;
				    _scanSettingsPullSlips.S2N.Splitting.Value = Scanners.S2N.Splitting.Off;				    
				    //_scanSettingsPullSlips.S2N.Splitting.Value = Scanners.S2N.Splitting.Left;
			    }
				else
				{
				  if (docMode == Scanners.S2N.DocumentMode.V)
				  {
					 _scanSettingsPullSlips.S2N.DocMode.Value = Scanners.S2N.DocumentMode.V;     //if scanning book with V-cradle-> keep this mode even for pull slips
					 //_scanSettingsPullSlips.S2N.Splitting.Value = Scanners.S2N.Splitting.Left;
					 _scanSettingsPullSlips.S2N.Splitting.Value = Scanners.S2N.Splitting.Off;
				}
				  else
				  {
					_scanSettingsPullSlips.S2N.DocMode.Value = Scanners.S2N.DocumentMode.Flat;
					_scanSettingsPullSlips.S2N.Splitting.Value = Scanners.S2N.Splitting.Off;
				  }
				}
		}
		#endregion

		#endregion
	}
}
