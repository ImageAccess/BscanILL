using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Misc;
using System.Drawing;

namespace BscanILL.UI.Frames.Shared
{
	abstract class FrameClick
	{
		#region variables

		IFrameBase _frameBase;
		System.Windows.Threading.Dispatcher _dispatcher;
		MainWindow							_mainWindow;

		protected Scanners.Click.ClickWrapper	_scanner = null;
		bool									_isActivated = false;
		protected int							_lastOperationId = 1;

		public event VoidHnd					ScanRequest;

		#endregion


		#region constructor
		public FrameClick(IFrameBase frameBase, System.Windows.Threading.Dispatcher dispatcher, MainWindow mainWindow, Scanners.Click.ClickWrapper click, bool isActive)
		{
			try
			{
				_frameBase = frameBase;
				_dispatcher = dispatcher;
				_mainWindow = mainWindow;
				_scanner = click;

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

		protected MainWindow							mainWindow { get { return _mainWindow; } }
		protected Notifications							notifications { get { return Notifications.Instance; } }
		protected BscanILL.Scan.ScannersManager			sm { get { return BscanILL.Scan.ScannersManager.Instance; } }
		protected BscanILL.SETTINGS.ScanSettings		scanSettings { get { return BscanILL.SETTINGS.ScanSettings.Instance; } }
        protected BscanILL.SETTINGS.ScanSettingsPullSlips scanSettingsPullSlips { get { return BscanILL.SETTINGS.ScanSettingsPullSlips.Instance; } }

		protected System.Windows.Threading.Dispatcher	Dispatcher { get { return _dispatcher; } }

		#endregion


		//PUBLIC METHODS	
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			Deactivate();
			_scanner = null;
		}
		#endregion

		#region Activate()
		public void Activate()
		{
			if (_scanner != null && _isActivated == false)
			{
				_scanner.PowerUpSuccessEvent += new Scanners.Click.PowerUpSuccessHnd(Scanner_PowerUpSuccess);
				_scanner.PowerUpErrorEvent += new Scanners.Click.ErrorHnd(Scanner_PowerUpError);
				_scanner.ImageScannedEvent += new Scanners.Click.ScanBitmapCompleteHnd(Scanner_ImageScanned);
				_scanner.ImagesScannedEvent += new Scanners.Click.ScanBitmapsCompleteHnd(Scanner_ImagesScanned);
				_scanner.ScanErrorEvent += new Scanners.Click.ScanErrorHnd(Scanner_ScanError);
				_scanner.ScannerInternalErrorEvent += new Scanners.Click.CameraInternalErrorClickHnd(Scanner_InternalError);
                _scanner.ScannerShutDownEvent += new Scanners.Click.CameraShutDownClickHnd(Scanner_ShutDownEvent);
                _scanner.ScanButtonPressedEvent += new Scanners.Click.ScanButtonPressedHnd(Scanner_ScanButtonPressed);
				
				_isActivated = true;
			}
		}
		#endregion

		#region Deactivate()
		public void Deactivate()
		{
			if (_scanner != null && _isActivated == true)
			{
				_scanner.PowerUpSuccessEvent -= new Scanners.Click.PowerUpSuccessHnd(Scanner_PowerUpSuccess);
				_scanner.PowerUpErrorEvent -= new Scanners.Click.ErrorHnd(Scanner_PowerUpError);
				_scanner.ImageScannedEvent -= new Scanners.Click.ScanBitmapCompleteHnd(Scanner_ImageScanned);
				_scanner.ImagesScannedEvent -= new Scanners.Click.ScanBitmapsCompleteHnd(Scanner_ImagesScanned);
				_scanner.ScanErrorEvent -= new Scanners.Click.ScanErrorHnd(Scanner_ScanError);
				_scanner.ScannerInternalErrorEvent -= new Scanners.Click.CameraInternalErrorClickHnd(Scanner_InternalError);
                _scanner.ScannerShutDownEvent -= new Scanners.Click.CameraShutDownClickHnd(Scanner_ShutDownEvent);
                _scanner.ScanButtonPressedEvent -= new Scanners.Click.ScanButtonPressedHnd(Scanner_ScanButtonPressed);

				_isActivated = false;
			}
		}
		#endregion

		#region ReRegisterScanner()
		public void ReRegisterScanner()
		{
			if (_scanner != null)
			{
				_scanner.ReRegisterClickEvents();
			}
		}
		#endregion

		#region UnRegisterScanner()
		public void UnRegisterScanner()
		{
			if (_scanner != null)
			{
				_scanner.UndoRegisterClickEvents();
			}
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			try
			{
				_scanner.Reset();
			}
			catch (Exception ex)
			{
				notifications.Notify(this, Notifications.Type.Error, "FrameClick, Reset(): " + ex.Message, ex);
			}
			finally
			{
			}
		}
		#endregion

		#endregion

		//PROTECTED METHODS	
		#region protected methods

		protected abstract void Scanner_ImageScanned(Bitmap bitmap, Scanners.Click.ScanPage scanPage);
		protected abstract void Scanner_ScanError(Exception ex);
		protected abstract void Scanner_ProgressChanged(string description, float progress);
		#endregion


		//PRIVATE METHODS	
		#region private methods

		#region Scanner_ImagesScanned()
		protected virtual void Scanner_ImagesScanned(Bitmap bitmapL, Bitmap bitmapR)
		{
			throw new Exception("FrameClick, Scanner_ImagesScanned() must be overriden");
		}
		#endregion

		#region Scanner_PowerUpSuccess()
		void Scanner_PowerUpSuccess()
		{
			this.Dispatcher.BeginInvoke((Action)delegate() { Scanner_PowerUpSuccessTU(); });
		}
		#endregion

		#region Scanner_PowerUpSuccessTU()
		void Scanner_PowerUpSuccessTU()
		{
			try
			{
				_scanner.Connect();
				_frameBase.UnLock();
			}
			catch (Exception ex)
			{
				_frameBase.Notify(this, Notifications.Type.Warning, "Scanner_PowerUpSuccessTU", "Click Scanner: Can't powercycle Click scanner! " + ex.Message, ex);
				_frameBase.ShowWarningMessage("Can't powercycle Click Scanner!");
				PowercycleCameras();
			}
		}
		#endregion
	
		#region Scanner_PowerUpError()
		void Scanner_PowerUpError(Exception ex)
		{
			this.Dispatcher.BeginInvoke((Action)delegate() { Scanner_PowerUpErrorTU(ex); });
		}
		#endregion

		#region Scanner_PowerUpErrorTU()
		void Scanner_PowerUpErrorTU(Exception ex)
		{
			try
			{
					_frameBase.HideLockUi();

					_frameBase.Notify(this, Notifications.Type.Warning, "Scanner_PowerUpErrorTU", "Click Scanner: Can't powercycle Click scanner! " + ex.Message, ex);
					_frameBase.ShowWarningMessage("Can't powercycle Click Scanner!");
			}
			finally
			{
				PowercycleCameras();
			}
		}
		#endregion

		#region Scanner_ScanButtonPressed()
		void Scanner_ScanButtonPressed()
		{
			this.Dispatcher.BeginInvoke((Action)delegate() 
			{
				if (ScanRequest != null)
					ScanRequest();
			});
		}
		#endregion

		#region Scanner_ShutDownEvent()
		void Scanner_ShutDownEvent(bool leftCamera)
		{
			this.Dispatcher.Invoke((Action)delegate() { Scanner_ShutDownEventTU(leftCamera); });
		}
		#endregion

		#region Scanner_ShutDownEventTU()
		void Scanner_ShutDownEventTU(bool leftCamera)
		{
			PowercycleCameras();
		}
		#endregion

		#region Scanner_InternalError()
		void Scanner_InternalError(bool leftCamera)
		{
			this.Dispatcher.Invoke((Action)delegate() { Scanner_InternalErrorTU(leftCamera); });
		}
		#endregion

		#region Scanner_InternalErrorTU()
		void Scanner_InternalErrorTU(bool leftCamera)
		{
			PowercycleCameras();
		}
		#endregion

		#region PowercycleCameras()
		private void PowercycleCameras()
		{
			try
			{
				_frameBase.LockWithProgressBar(false, "Powercycling Cameras...");

				_scanner.Powercycle();
			}
			catch (Exception ex)
			{
				_frameBase.HideLockUi();
				_frameBase.ShowErrorMessage(BscanILL.Misc.Misc.GetErrorMessage(ex));
				_frameBase.Notify(this, Notifications.Type.Warning, "PowercycleCameras", "FrameClick, PowercycleCameras(): " + ex.Message, ex);
				_frameBase.UnLock();
			}
		}
		#endregion

		#endregion
	
	}
}
