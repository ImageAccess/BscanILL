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
using ClickDLL.Cameras;
using ClickCommon.Settings;

namespace Scanners.Click
{
	public class ClickMiniWrapper : ClickBase, IScanner, IDisposable, ClickDLL.Listeners.IMiniListener
	{
		static ClickDLL.ClickMini		_scanner = null;

		bool _isScannerLoaded = false;

		CanonCamera.CameraProperties.CaptureProperties captureProperties = null;

		public event PowerUpSuccessHnd			PowerUpSuccessEvent;
		public event ErrorHnd					PowerUpErrorEvent;
		//public event ScanBitmapsCompleteHnd		ImagesScanned;
		public event BitmapCompleteHnd			ImageScannedEvent;
		public event ScanErrorHnd				ScanErrorEvent;
		public event CameraInternalErrorHnd		ScannerInternalErrorEvent;
		public event CameraShutDownHnd			ScannerShutDownEvent;
   	public event LiveImageCapturedHnd		LiveViewCapturedEvent;
		public event ScanButtonPressedHnd		ScanButtonPressedEvent;

		Scanners.Settings.ClickMiniClass		_settings = Scanners.Settings.Instance.ClickMini;
		Scanners.Click.ClickMiniSettings		_scanSettings = null;

		#region constructor
		public ClickMiniWrapper()
		{
			try
			{
				scannerModel = new Scanners.MODELS.Model(Scanners.MODELS.ScanerModel.ClickMiniV1);
				
				if (_scanner == null)
				{
					_scanner = ClickDLL.ClickMini.GetInstance();
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
		
		public ClickDLL.ClickMini		ClickScanner { get { return _scanner; } }

		public bool IsScannerLoaded
        {
			get { return _isScannerLoaded; }
            set
            {
				if(_isScannerLoaded != value)
                {
					_isScannerLoaded = value;					
				}
            }
        }

		public bool CapturingLiveView { get { return (this.ClickScanner != null) ? this.ClickScanner.CapturingLiveView : false; } }

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

			this.IsScannerLoaded = false;
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
		public void Scan(Scanners.Click.ClickMiniSettings settings)
		{
			_scanSettings = settings;

			this.ClickScanner.Brightness = settings.Brightness.Value;
			this.ClickScanner.Contrast = settings.Contrast.Value;

			//ClickDLL.ScanParameters scanParams = new ClickDLL.ScanParameters(true, settings.ScanMode.Value == ClickMiniScanMode.BookMode, true, true, false, 100, false, false, false);  //new canon - added autofocus parameter, find multiple objects            
			//this.ClickScanner.Scan(ClickCommon.ScanPage.Stitch, scanParams);
			ClickCommon.Scanning.ScanParameters scanParams = new ClickCommon.Scanning.ScanParameters( ( ( settings.ScanMode.Value == ClickMiniScanMode.BookMode) ? ClickCommon.Scanning.ScannedImageType.Bookfold : ClickCommon.Scanning.ScannedImageType.Regular), true, true, true, false, 100, false);

			this.ClickScanner.ImageQuality = ClickCommon.CameraProperties.ImageQualityTriplet.JPEG_Large_Fine;
			this.ClickScanner.Scan(scanParams);
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
			//if (this.ClickScanner != null)				//in new version of ClickDLL PowerCycle method is not public anymore
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
            //ClickDLL.UI.Dialogs.ClickMiniWizard.ClickDlg.Open(this.ClickScanner, null);
            try
            {
				ClickDLL.UI.Dialogs.ClickMiniWizard.ViewModels.ClickDllViewModel viewModel = new ClickDLL.UI.Dialogs.ClickMiniWizard.ViewModels.ClickDllViewModel(this.ClickScanner);

				ClickDLL.UI.Dialogs.ClickMiniWizard.Views.ClickDlg dlg = new ClickDLL.UI.Dialogs.ClickMiniWizard.Views.ClickDlg() { Topmost = true, DataContext = viewModel };

				dlg.Closing += (sender, e) => { viewModel.Dispose(); };
				dlg.ShowDialog();

				//ClickCommon.Settings.SettingsMini.Instance.Save();	//do not save settings ClickMini.settings file on disk anytime we close ClickMini settings wizard
																		// -> save just when OK or Apply button pressed in main BscanILL Settings dialog
			}

			catch(Exception ex)
            {
				if (ScanErrorEvent != null)
					ScanErrorEvent(ex);
            }
		}
		#endregion

		#region GetScannerSettings()
		public Scanners.Click.ClickMiniSettings GetScannerSettings()
		{
			return new ClickMiniSettings(this);
		}
		#endregion

		#region Listener_PowerUpSuccess()
		public void Listener_PowerUpSuccess()
		{
            try
            {
				this.IsScannerLoaded = true;

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
			switch( status )
            {
				case ClickCommon.ScannerStatus.Running:
                    {
						this.IsScannerLoaded = true;
					}
					break;

				case ClickCommon.ScannerStatus.Loaded:
                    {
						this.IsScannerLoaded = false;
					}
					break;

				case ClickCommon.ScannerStatus.Disposed:
                    {
						UnregisterClickEvents();
						this.IsScannerLoaded = false;
					}
					break;
            }
			
		}
		#endregion

		#region Listener_QuickCalibrationSuccess
		public void Listener_QuickCalibrationSuccess()
		{

        }
        #endregion

		#region Listener_QuickCalibrationError
		public void Listener_QuickCalibrationError(Exception Ex)
		{

		}
        #endregion

        #region Listener_ImageScanned()
        public void Listener_ImageScanned(Bitmap bitmap)
        {
            try
            {
                //Bitmap bitmap = e.Bitmap;

                PostProcessImage(ref bitmap);

                if (ImageScannedEvent != null)
                    ImageScannedEvent(bitmap);
            }
            catch (Exception ex)
            {
                if (ScanErrorEvent != null)
                    ScanErrorEvent(ex);
            }
        }
		#endregion

		#region Listener_BookCoverScanned()
		public void Listener_BookCoverScanned(Bitmap bitmap, ClickCommon.Scanning.BookCoverScan bookCoverScan, ClickDLL.Models.IT.Crop.BookCoverClips clips)
		{

        }
		#endregion

		#region ScanError()
		public void ScanError(Exception Ex)
        {
            if (ScanErrorEvent != null)
                ScanErrorEvent(Ex);
        }
		#endregion

		#region Listener_ScanError()
		//public void ScanError( ClickCommon.Scanning.ScannedImageType scanType, Exception Ex)
		public void Listener_ScanError(ClickCommon.Scanning.ScannedImageType scanType, Exception Ex)
		{
			if (ScanErrorEvent != null)
				ScanErrorEvent(Ex);
		}
		#endregion

		#region ScannerInternalError()
		public void ScannerInternalError()
        {
            if (ScannerInternalErrorEvent != null)
                ScannerInternalErrorEvent();
        }
        #endregion

        #region ScannerShutDown()
        public void ScannerShutDown()
        {
            if (ScannerShutDownEvent != null)
                ScannerShutDownEvent();
        }
        #endregion

		#region Listener_LiveViewCaptured()
		public void Listener_LiveViewCaptured(Bitmap bitmap)
		{
            if (LiveViewCapturedEvent != null)
                LiveViewCapturedEvent(bitmap);
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
