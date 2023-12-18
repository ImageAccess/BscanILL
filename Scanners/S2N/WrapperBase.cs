using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading;

namespace Scanners.S2N
{
	public class WrapperBase
	{
		IS2NScanner scanner;
		
		protected Notifications notifications = null;
		protected DirectoryInfo imageDir = null;

		protected object scannerButtonsActivationLocker = new object();


		public event Scanners.ImageScannedHnd			PreviewScanned;
		public event Scanners.FileScannedHnd			ImageScanned;
        public event Scanners.S2N.ScanRequestHnd        OverrideScanArea;

		public event Scanners.S2N.ScanRequestHnd		ScanRequest;
		public event Scanners.S2N.ScanRequestHnd		ScanPullslipRequest;
        public event Scanners.S2N.ScanRequestHnd        UpdateSplittingButtonsRequest;

		public event Scanners.OperationSuccessfullHnd	OperationSuccessfull;
		public event Scanners.OperationErrorHnd			OperationError;
		public event Scanners.ProgressChangedHnd		ProgressChanged;


		#region constructor
		public WrapperBase(IS2NScanner scanner)
		{
			this.notifications = Notifications.Instance;
			this.imageDir = new DirectoryInfo(Scanners.Settings.Instance.General.TempImagesDir);

			this.scanner = scanner;

			this.scanner.PreviewScanned += delegate(Bitmap preview)
			{
				if (PreviewScanned != null)
					PreviewScanned(-1, preview);
			};
			this.scanner.ProgressChanged += delegate(string description, float progress)
			{
				if (ProgressChanged != null)
					ProgressChanged(description, progress);
			};

			this.scanner.ScanRequest += delegate(ScannerScanAreaSelection scanArea)
			{
				if (this.ScanRequest != null)
					this.ScanRequest(scanArea);
			};
			this.scanner.ScanPullslipRequest += delegate(ScannerScanAreaSelection scanArea)
			{
				if (this.ScanPullslipRequest != null)
					this.ScanPullslipRequest(scanArea);
			};

            this.scanner.TouchScreenScanAreaButtonPressed += delegate(ScannerScanAreaSelection scanArea)
			{
				if (this.UpdateSplittingButtonsRequest != null)
                    this.UpdateSplittingButtonsRequest(scanArea);
			};

            
		}
		#endregion


		#region enum ScannerOperationType
		protected enum ScannerOperationType
		{
			SetDevice,
			Scan
		}
		#endregion

		#region ScannerOperation
		protected class ScannerOperation
		{
			public int					OperationId;
			public ScannerOperationType OperationType;
			public object				Parameters;

			public ScannerOperation(int operationId, ScannerOperationType operationType, object parameters)
			{
				this.OperationId = operationId;
				this.OperationType = operationType;
				this.Parameters = parameters;
			}
		}
		#endregion

	
		// PUBLIC PROPERTIES
		#region public properties

		public Scanners.DeviceInfo		DeviceInfo { get { return scanner.DeviceInfo; } }
		public Scanners.MODELS.Model	Model { get { return this.DeviceInfo.ScannerModel; } }

		public S2NSettings		ScannerSettings { get { return this.scanner.Settings; } }
		public short			MaxColorDpi { get { return this.scanner.MaxColorDpi; } }
		public bool				IsColorScanner { get { return this.scanner.IsColorScanner; } }
	
		protected IS2NScanner Scanner { get { return this.scanner; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region GetDpi()
		public virtual int GetDpi()
		{
			return Math.Min(this.MaxColorDpi, (int)this.ScannerSettings.Dpi.Value);
		}
		#endregion

		#region LockScannerUi()
		public void LockScannerUi( bool forceLock )
		{
			lock (scannerButtonsActivationLocker)
			{
				if (this.scanner != null)
                    this.scanner.LockUi( forceLock );
			}
		}
		#endregion

		#region UnlockScannerUi()
        public void UnlockScannerUi(bool singleArticleMode)
		{
			lock (scannerButtonsActivationLocker)
			{
				if (this.scanner != null)
                    this.scanner.UnlockUi(singleArticleMode);
			}
		}
		#endregion

        #region UpdateTouchScreenExternal()
        public void UpdateTouchScreenExternal(Scanners.S2N.ScannerScanAreaSelection areaSelection)
        {
          if (this.scanner != null )
          {
             scanner.UpdateTouchScreenExternal(areaSelection) ;
          }
        }
        #endregion

        #region StartTouchScreenMonitoring()
        public void StartTouchScreenMonitoring()
		{
			if (this.scanner != null && scanner.IsTouchScreenMonitoringRunning == false)
			{
				// to reset scan buttons stack
				scanner.ResetTouchScreenEvents();
				scanner.StartTouchScreenMonitoring();
			}
		}
		#endregion

		#region StopTouchScreenMonitoring()
		public void StopTouchScreenMonitoring()
		{
			if (this.scanner != null)
			{
				scanner.StopTouchScreenMonitoring();
			}
		}
		#endregion
	
    #region StartTimerPing()
    public void StartTimerPing()
		{
			if (this.scanner != null)
			{
                scanner.StartTimerPing();
			}
		}
		#endregion

    #region StopTimerPing()
    public void StopTimerPing()
		{
			if (this.scanner != null)
			{
                scanner.StopTimerPing();
			}
		}
		#endregion
			
		#endregion


		// PROTECTED METHODS
		#region protected methods

		#region SetScanner()
		protected void SetScanner(Scanners.S2N.S2NSettings settings)
		{
			this.ScannerSettings.CopyFrom(settings);

			if (settings.ColorMode.Value == Scanners.S2N.ColorMode.Lineart)
				this.ScannerSettings.ColorMode.Value = Scanners.S2N.ColorMode.Lineart;
			else if (this.IsColorScanner && (settings.ColorMode.Value == Scanners.S2N.ColorMode.Color))
				this.ScannerSettings.ColorMode.Value = Scanners.S2N.ColorMode.Color;
			else
				this.ScannerSettings.ColorMode.Value = Scanners.S2N.ColorMode.Grayscale;

			switch (settings.FileFormat.Value)
			{
				case Scanners.S2N.FileFormat.Tiff:
					this.ScannerSettings.FileFormat.Value = Scanners.S2N.FileFormat.Tiff; break;
				default: this.ScannerSettings.FileFormat.Value = Scanners.S2N.FileFormat.Jpeg; break;
			}

			this.ScannerSettings.Dpi.Value = GetDpi();// Math.Max(this.ScannerSettings.Dpi.Minimum, Math.Min(this.ScannerSettings.Dpi.Maximum, this.dpi));
		}
		#endregion

		#region RaiseImageScanned()
		protected void RaiseImageScanned(int operationId, string filePath)
		{
			if (ImageScanned != null)
				ImageScanned(operationId, filePath);
		}
		#endregion

		#region RaiseOperationSuccessfull()
		protected void RaiseOperationSuccessfull(int operationId)
		{
			try
			{
				if (OperationSuccessfull != null)
					OperationSuccessfull(operationId);
			}
			catch { }
		}
		#endregion

		#region RaiseOperationError()
		protected void RaiseOperationError(int operationId, Exception ex)
		{
			try
			{
				if (OperationError != null)
					OperationError(operationId, ex);
			}
			catch { }
		}
		#endregion

		#region Scan_Thread()
		protected void Scan_Thread(int operationId)
		{
			try
			{
				Thread.Sleep(100);
				string filePath = string.Format("{0}\\{1}.jpg", imageDir.FullName, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff"));
				scanner.Scan(filePath);

				ImageScanned(operationId, filePath);
			}
			catch (ScannersEx ex)
			{
				RaiseOperationError(operationId, ex);
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't scan image!" + " " + ex.Message, ex);
				RaiseOperationError(operationId, ex);
			}
		}

        protected void Scan_Thread(int operationId, Scanners.S2N.S2NSettings settings)
        {
            Scanners.S2N.Splitting splittingStartValue = settings.Splitting.Value; 

            try
            {
                if (settings.Splitting.Value == Scanners.S2N.Splitting.Auto)
                {
                    // do not use scanner's built-in splitting in auto mode to make just one image transfer of full bed and we split it inside Bscan
                    //this is the way Jirka implement it for touch screen
                    // when scanning just left or just right side, we use scanners build in function - this way it may sweep just half way so it should be faster rather than scanning full bed and then use just half image.
                    settings.Splitting.Value = Scanners.S2N.Splitting.Off;

                    //in case we use scan button in BscanILL screen we need to set scanArea - flag used when scanning from touch screen button - this will trigger portScanning splitting (internal) in Bscan               
                    if (OverrideScanArea != null)
                      OverrideScanArea(Scanners.S2N.ScannerScanAreaSelection.Both);
                }

                SetScanner(settings);

                Thread.Sleep(100);
                string filePath = string.Format("{0}\\{1}.jpg", imageDir.FullName, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff"));
                scanner.Scan(filePath);

                ImageScanned(operationId, filePath);                
            }
            catch (ScannersEx ex)
            {
                RaiseOperationError(operationId, ex);
            }
            catch (Exception ex)
            {
                Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't scan image!" + " " + ex.Message, ex);
                RaiseOperationError(operationId, ex);
            }

            finally
            {
                if( splittingStartValue != settings.Splitting.Value )
                {
                    settings.Splitting.Value = splittingStartValue;
                }
            }
        }
        #endregion

        #region SetDevice_Thread()
        protected void SetDevice_Thread(int operationId, Scanners.S2N.S2NSettings settings)
        {
            try
            {
                SetScanner(settings);

                //Thread.Sleep(100);
                //string filePath = string.Format("{0}\\{1}.jpg", imageDir.FullName, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff"));
                //scanner.Scan(filePath);

                //ImageScanned(operationId, filePath);
            }
            catch (ScannersEx ex)
            {
                RaiseOperationError(operationId, ex);
            }
            catch (Exception ex)
            {
                Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't scan image!" + " " + ex.Message, ex);
                RaiseOperationError(operationId, ex);
            }
        }
        #endregion
        
        #endregion
    }
}
