using System;
using System.Collections;
using System.Collections.Specialized ;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging ;
using System.Data;
using System.IO ;
using System.Net ;
using System.Text ;
using System.Diagnostics ;
using System.Configuration ;
using System.Threading;
using System.Collections.Generic;
using ClickDLL;
using ClickCommon.Settings;



namespace Scanners.Click
{
	public class ClickWrapper : ClickBase, IScanner, IDisposable, ClickDLL.Listeners.IClickListener
	{
		static ClickDLL.Click			_scanner = null;

		CanonCamera.CameraProperties.CaptureProperties captureProperties = null;

		public event PowerUpSuccessHnd			PowerUpSuccessEvent;
		public event ErrorHnd					PowerUpErrorEvent;
		public event ScanBitmapsCompleteHnd		ImagesScannedEvent;
		public event ScanBitmapCompleteHnd		ImageScannedEvent;
		public event ScanErrorHnd				ScanErrorEvent;
		public event CameraInternalErrorClickHnd		ScannerInternalErrorEvent;
		public event CameraShutDownClickHnd			ScannerShutDownEvent;
		public event LiveImageCapturedClickHnd   LiveViewCapturedEvent;
		public event ScanButtonPressedHnd		ScanButtonPressedEvent;

		Scanners.Settings.ClickScannerClass		_settings = Scanners.Settings.Instance.ClickScanner;
		Scanners.Click.ClickSettings			_scanSettings = null;

		#region constructor
		public ClickWrapper()
		{
			try
			{
				scannerModel = new Scanners.MODELS.Model(Scanners.MODELS.ScanerModel.ClickV1);

				if (_scanner == null)
				{
					_scanner = ClickDLL.Click.GetInstance();
					RegisterClickEvents();
					this.ClickScanner.PowerUpSync();

					Reset();
				}
        else
        {
          RegisterClickEvents();
        }
        				

				this.captureProperties = this.ClickScanner.GetDefaultCaptureProperties();
				this.deviceInfo = new DeviceInfoRebel(this.scannerModel, this.ClickScanner.SerialNumber, this.ClickScanner.Firmware);

				if (registeredCallers.Contains(this) == false)
					registeredCallers.Add(this);
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
		
		public ClickDLL.Click	ClickScanner { get { return _scanner; } }

		public bool				CapturingLiveView { get { return (this.ClickScanner != null) ? this.ClickScanner.CapturingLiveView : false; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			UnregisterClickEvents();

			if (registeredCallers.Contains(this))
				registeredCallers.Remove(this);

			if (registeredCallers.Count == 0)
			{
				HardDispose();
			}
		}
		#endregion

		#region HardDispose()
		public void HardDispose()
		{
			UnregisterClickEvents();

			registeredCallers.Clear();

			if (_scanner != null)
			{
				_scanner.Dispose();
			}

			_scanner = null;
		}
		#endregion

		#region HardDisposeIfNecessary()
		public static void HardDisposeIfNecessary()
		{
			registeredCallers.Clear();

			if (_scanner != null)
			{
				_scanner.Dispose();
				_scanner = null;
			}
		}
		#endregion

		#region PowerUpSync()
		public void PowerUpSync()
		{
			this.ClickScanner.PowerUpSync();
		}
		#endregion

		#region Connect()
		public void Connect()
		{
			this.captureProperties = this.ClickScanner.GetDefaultCaptureProperties();
		}
		#endregion

		#region ReRegisterClickEvents()
		public void ReRegisterClickEvents()
		{
			RegisterClickEvents();
		}
		#endregion

		#region UndoRegisterClickEvents()
		public void UndoRegisterClickEvents()
		{
			UnregisterClickEvents();
		}
		#endregion

		#region Scan()
		public void Scan(Scanners.Click.ClickSettings settings)
		{
			_scanSettings = settings;

			ClickCommon.ScanPage scanPage;

			switch (settings.ScanPage.Value)
			{
                case ClickScanPage.Left: scanPage = ClickCommon.ScanPage.FlatLeft; break;
                case ClickScanPage.Right: scanPage = ClickCommon.ScanPage.FlatRight; break;
                default: scanPage = ClickCommon.ScanPage.FlatBoth; break;
			}

			this.ClickScanner.Brightness = settings.Brightness.Value;
			this.ClickScanner.Contrast = settings.Contrast.Value;

			//ClickDLL.ScanParameters scanParams = new ClickDLL.ScanParameters(true, settings.ScanMode.Value == ClickScanMode.BookMode, true, true, false, 100, false, false, false);     //SL2 - autofocus and findMultipleObjects params added  
			ClickCommon.Scanning.ScanParameters scanParams = new ClickCommon.Scanning.ScanParameters( ( (settings.ScanMode.Value == ClickScanMode.BookMode) ? ClickCommon.Scanning.ScannedImageType.Bookfold : ClickCommon.Scanning.ScannedImageType.Regular ), true, true, true, false, 100, false);

			this.ClickScanner.ImageQuality = ClickCommon.CameraProperties.ImageQualityTriplet.JPEG_Large_Fine;
			this.ClickScanner.Scan(scanPage, scanParams);
		}
		#endregion

		#region PingDevice()
		public void PingDevice()
		{
			//scannerQueue.Enqueue(new ScannerOperation(operationId, ScannerOperationType.Ping, null));
		}
		#endregion

		#region StartLiveView()
		public void StartLiveView()
		{
			if (this.ClickScanner != null)
				this.ClickScanner.StartLiveView();
		}
		#endregion

		#region StopLiveView()
		public void StopLiveView()
		{
			if (this.ClickScanner != null)
				this.ClickScanner.StopLiveView();
		}
		#endregion

		#region SetFocus()
		public void SetFocus()
		{
			this.ClickScanner.SetFocus();
		}
		#endregion

		#region Powercycle()
		public void Powercycle()
		{
			//if (this.ClickScanner != null)			//in new version of ClickDLL PowerCycle method is not public anymore
			//	this.ClickScanner.Powercycle(true);
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			this.ClickScanner.IsLightOn = false;
			this.ClickScanner.IsLaserOn = false;
			this.ClickScanner.DeleteFiles();
		}
		#endregion

		//rebel
		#region OpenClickSettings()
		public  void OpenClickSettings()
		{
            ClickDLL.UI.Dialogs.ClickWizard.ClickDlg.Open(this.ClickScanner, null);
		}
		#endregion

		#region GetScannerSettings()
		public Scanners.Click.ClickSettings GetScannerSettings()
		{
			return new ClickSettings(this);
		}
		#endregion

		#region Listener_PowerUpSuccess()
        public void Listener_PowerUpSuccess()
        {
            try
            {
                if (this.PowerUpSuccessEvent != null)
                    this.PowerUpSuccessEvent();
            }
            catch (Exception ex)
            {
                if (this.PowerUpErrorEvent != null)
                    this.PowerUpErrorEvent(ex);
            }
        }
		#endregion

		#region Listener_PowerUpError()
        public void Listener_PowerUpError(Exception ex)
        {
            if (this.PowerUpErrorEvent != null)
                this.PowerUpErrorEvent(ex);
        }
		#endregion

		#region Listener_StatusChanged()
		public void Listener_StatusChanged(ClickCommon.ScannerStatus status)
		{

		}
		#endregion

		#region Listener_ImagesScanned()
		public void Listener_ImagesScanned(Bitmap bitmapL, Bitmap bitmapR)
        {
            try
            {
                //Bitmap bitmapL = e.BitmapL;
                //Bitmap bitmapR = e.BitmapR;

                PostProcessImage(ref bitmapL);
                PostProcessImage(ref bitmapR);

                if (ImagesScannedEvent != null)
                    ImagesScannedEvent(bitmapL, bitmapR);
            }
            catch (Exception ex)
            {
                if (ScanErrorEvent != null)
                    ScanErrorEvent(ex);
            }
        }
#endregion

		#region Listener_ImageScanned()
        public void Listener_ImageScanned(Bitmap bitmap, ClickCommon.CameraScanPage scanPage)
        {
            try
            {
                //Bitmap bitmap = e.Bitmap;

                PostProcessImage(ref bitmap);

                if (ImageScannedEvent != null)
                    //ImageScannedEvent(bitmap, (Scanners.Click.ScanPage)e.ScanPage);
                    ImageScannedEvent(bitmap, (Scanners.Click.ScanPage)scanPage);
            }
            catch (Exception ex)
            {
                if (ScanErrorEvent != null)
                    ScanErrorEvent(ex);
            }
        }
		#endregion

		#region Listener_ScanError()
        public void Listener_ScanError(Exception Ex)
        {
            if (ScanErrorEvent != null)
                ScanErrorEvent(Ex);
        }
		#endregion

		#region ScannerInternalError()
        public void ScannerInternalError(bool LeftCamera)
        {
            if (ScannerInternalErrorEvent != null)
                ScannerInternalErrorEvent(LeftCamera);
        }
		#endregion

		#region ScannerShutDown()
        public void ScannerShutDown(bool LeftCamera)
        {
            if (ScannerShutDownEvent != null)
                ScannerShutDownEvent(LeftCamera);
        }
		#endregion

		#region Listener_LiveViewCaptured()
        public void Listener_LiveViewCaptured(Bitmap bitmap, ClickCommon.CameraScanPage scanPage)
        {
            if (LiveViewCapturedEvent != null)
                LiveViewCapturedEvent(bitmap, scanPage);
        }
		#endregion

		#region Listener_ScanButtonPressed()
        public void Listener_ScanButtonPressed()
        {
            if (_settings.CatchScanButtonEvents)
            {
                if (ScanButtonPressedEvent != null)
                    ScanButtonPressedEvent();
            }
        }
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region LoadScannerAsync
		private void LoadScannerAsync()
		{
			this.ClickScanner.PowerUpAsync();
		}
		#endregion
		
		#region CropImage()
		Bitmap CropImage(Bitmap bitmap)
		{
			if (this.CropImages)
			{
				Rectangle rect;
				if (ImageProcessing.DocumentLocator.SeekDocument(bitmap, Rectangle.Empty, out rect, 100, 100, 100))
				{
					Bitmap clip = ImageProcessing.ImageCopier.Copy(bitmap, rect);
					bitmap.Dispose();
					bitmap = clip;
				}
			}

			return bitmap;
		}
		#endregion

		#region RegisterClickEvents
		private void RegisterClickEvents()
		{
			if (this.catchingEvents == false)
			{
                _scanner.RegisterListener(this);
                this.catchingEvents = true;
			}
		}
		#endregion

		#region UnregisterClickEvents()
		private void UnregisterClickEvents()
		{
			if (this.catchingEvents && this.ClickScanner != null)
			{
                _scanner.UnRegisterListener(this);
                this.catchingEvents = false;
			}
		}
		#endregion

		#region PostProcessImage()
		private void PostProcessImage(ref Bitmap bitmap)
		{
			if(_scanSettings != null)
			{
				if(_scanSettings.ColorMode.Value == ClickColorMode.Grayscale)
				{
					Bitmap grayBitmap = ImageProcessing.Resampling.Resample(bitmap, ImageProcessing.PixelsFormat.Format8bppGray);
					bitmap.Dispose();
					bitmap = grayBitmap;
				}
				else if (_scanSettings.ColorMode.Value == ClickColorMode.Bitonal)
				{
					Bitmap bitonalBitmap = ImageProcessing.Resampling.Resample(bitmap, ImageProcessing.PixelsFormat.FormatBlackWhite);
					bitmap.Dispose();
					bitmap = bitonalBitmap;
				}

				if(_scanSettings.Dpi.Value != 300)
				{
					double zoom = _scanSettings.Dpi.Value / (double)bitmap.HorizontalResolution;

					if (zoom < 0.95 || zoom > 1.05)
					{
						Bitmap resized = ImageProcessing.Resizing.Resize(bitmap, Rectangle.Empty, zoom, ImageProcessing.Resizing.ResizeMode.Quality);
						bitmap.Dispose();
						bitmap = resized;
					}
				}
			}
		}
		#endregion

		#endregion
	}

}
