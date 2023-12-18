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
using System.Runtime.InteropServices;


namespace Scanners.Twain
{
	public class TwainBase
	{
		protected static List<IScanner>		instances = new List<IScanner>();

	
		protected volatile List<object>		registeredCallers = new List<object>();
		protected TwainApp.TwainScanner		scanner = null;
		protected MessageHandler			messageHandler;
		protected Scanners.DeviceInfo		deviceInfo;
		
		protected Notifications						notifications = Notifications.Instance;
		protected Scanners.MODELS.Model				model = new Scanners.MODELS.Model(MODELS.ScanerModel.iVinaFB6280E);

		protected Scanners.Twain.BookedgePage		bookedgePage = Scanners.Twain.BookedgePage.FlatMode;
		protected int								operationId = -1;
		protected object							locker = new object();

		public delegate void ImageScannedHnd(int operationId, TwainApp.TwainImage twainImage, bool moreImagesToTransfer);
		public delegate void BitmapScannedHnd(int operationId, Bitmap bitmap, Scanners.Twain.BookedgePage bookedgePage, bool moreImagesToTransfer);
		public delegate void ScanErrorHnd(int operationId, Exception ex);
		public delegate void ScannerInitializedHnd(Scanners.DeviceInfo deviceInfo);

		public event ImageScannedHnd					ImageScanned;
		//public event BitmapScannedHnd					BitmapScanned;
		public event ScanErrorHnd						ScanError;
		public event Scanners.ProgressChangedHnd		ProgressChanged;

		/*[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = false)]
		public static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = false)]
		public static extern bool FreeLibrary(IntPtr library);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = false)]
		public static extern IntPtr GetModuleHandle(string lpFileName);*/


		#region constructor
		protected TwainBase(object caller, Scanners.MODELS.Model model)
		{
			this.registeredCallers.Add(caller);
			
			messageHandler = new MessageHandler();
			messageHandler.MessageReceived += new MessageHandler.MessageReceivedHnd(MessageHandler_MessageReceived);

			Init(model);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		public Scanners.DeviceInfo		DeviceInfo { get { return deviceInfo; } }
		public Scanners.MODELS.Model	Model { get { return this.DeviceInfo.ScannerModel; } }

		#region Dpi
		public short Dpi
		{
			get { return (short)scanner.Resolution; }
			set { scanner.Resolution = value; }
		}
		#endregion

		#region Brightness
		/// <summary>
		/// double from interval <-1, 1>
		/// </summary>
		public double Brightness
		{
			get { return scanner.Brightness; }
			set { scanner.Brightness = value; }
		}
		#endregion

		#region Contrast
		/// <summary>
		/// double from interval <-1, 1>
		/// </summary>
		public double Contrast
		{
			get { return scanner.Contrast; }
			set { scanner.Contrast = value; }
		}
		#endregion

		#region ColorMode
		public Scanners.Twain.ColorMode ColorMode
		{
			get
			{
				switch (scanner.PixelDepth)
				{
					case PixelFormat.Format1bppIndexed: return Scanners.Twain.ColorMode.Bitonal;
					case PixelFormat.Format8bppIndexed: return Scanners.Twain.ColorMode.Grayscale;
					default: return Scanners.Twain.ColorMode.Color;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.Twain.ColorMode.Bitonal: scanner.PixelDepth = PixelFormat.Format1bppIndexed; break;
					case Scanners.Twain.ColorMode.Grayscale: scanner.PixelDepth = PixelFormat.Format8bppIndexed; break;
					default: scanner.PixelDepth = PixelFormat.Format24bppRgb; break;
				}
			}
		}
		#endregion

		#region MaxColorDpi
		public short MaxColorDpi
		{
			get { return 600; }
		}
		#endregion

		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		protected Scanners.Settings			settings { get { return Scanners.Settings.Instance; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose(object caller)
		{
			if (registeredCallers.Contains(caller))
				registeredCallers.Remove(caller);

			if (registeredCallers.Count == 0)
			{
				HardDispose();

				for (int i = instances.Count - 1; i >= 0; i--)
				{
					if (instances[i] == this)
					{
						instances.RemoveAt(i);
						break;
					}
				}
			}
		}
		#endregion

		#region HardDispose()
		public virtual void HardDispose()
		{
			if (scanner != null)
				scanner.Shutdown();

			scanner = null;
			registeredCallers.Clear();

			if (messageHandler != null)
			{
				messageHandler.DestroyHandle();
				messageHandler.ReleaseHandle();
				messageHandler = null;
			}
		}
		#endregion

		#region PingDevice()
		public void PingDevice()
		{
			//scannerQueue.Enqueue(new ScannerOperation(operationId, ScannerOperationType.Ping, null));
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			if (scanner != null)
			{
				lock (locker)
				{
					scanner.PixelDepth = PixelFormat.Format24bppRgb;
					scanner.Resolution = 300;
					scanner.PredefinedScanArea = TwainApp.PredefinedScanArea.None;

					if (this.model.ScannerSubGroup == MODELS.ScannerSubGroup.TwainAdf)
					{
					}
					else
					{
						scanner.AutomaticBorderDetection = true;
						scanner.AutomaticDeskewDetection = true;
						scanner.UndefinedImageSize = true;
					}

					scanner.CaptureIndicators = false;
				}
			}
		}
		#endregion

		#region OpenSetupWindow()
		public void OpenSetupWindow()
		{
			if (scanner != null)
				scanner.OpenSettingsWindow();
		}
		#endregion

		#region GetEnumSetting()
		public void GetEnumSetting()
		{
			if (scanner != null)
				scanner.OpenSettingsWindow();
		}
		#endregion

		#endregion


		//PROTECTED METHODS
		#region protected methods

		#region Init()
		void Init(Scanners.MODELS.Model model)
		{
			try
			{
				IntPtr mainFormHnd = messageHandler.Handle;// new System.Windows.Interop.WindowInteropHelper(mainForm).Handle;
				string serialNumber = "";
				string firmware = "";

				this.model = model;

				if (this.model.ScanerModel == MODELS.ScanerModel.iVinaFB6280E)
				{
					scanner = new TwainApp.TwainScanner(mainFormHnd, "FB6280E");

					scanner.Mirror = false;
					scanner.Rotation = 0;
					scanner.ScanOrientation = TwainApp.ScanOrientation.Rotate0;
					scanner.PixelDepth = PixelFormat.Format24bppRgb;
					scanner.BackgroundColor = TwainApp.BackgroundColor.Black;
					scanner.IsIccProfilesOn = false;
					scanner.FileTransferMode = TwainApp.TransferMode.Memory;
					scanner.Compression = TwainApp.Compression.None;

					this.Brightness = settings.TwainScanner.BrightnessDelta;
					this.Contrast = settings.TwainScanner.ContrastDelta;
					serialNumber = "FB6280E";
				}
				else if (this.model.ScanerModel == MODELS.ScanerModel.iVinaFB6080E)
				{
					scanner = new TwainApp.TwainScanner(mainFormHnd, "FB6080E");

					scanner.Mirror = false;
					scanner.Rotation = 0;
					scanner.ScanOrientation = TwainApp.ScanOrientation.Rotate0;
					scanner.PixelDepth = PixelFormat.Format24bppRgb;
					scanner.BackgroundColor = TwainApp.BackgroundColor.Black;
					scanner.IsIccProfilesOn = false;
					scanner.FileTransferMode = TwainApp.TransferMode.Memory;
					scanner.Compression = TwainApp.Compression.None;

					this.Brightness = settings.TwainScanner.BrightnessDelta;
					this.Contrast = settings.TwainScanner.ContrastDelta;
					serialNumber = "FB6080E";
				}
				else if (this.model.ScanerModel == MODELS.ScanerModel.KodakI1405)
				{
					scanner = new TwainApp.TwainScanner(mainFormHnd, "Kodak Scanner: i14");

					scanner.FileTransferMode = TwainApp.TransferMode.Memory;
					scanner.Compression = TwainApp.Compression.None;
					scanner.KodakIndicatorsWarmUp = false;
					scanner.KodakEnergySavingsTimeout = Math.Max(5, Math.Min(240, this.settings.TwainScanner.EnergyStarTimeout));

					this.Brightness = settings.TwainScanner.BrightnessDelta;
					this.Contrast = settings.TwainScanner.ContrastDelta;
					serialNumber = "i1405";
				}
				else if (this.model.ScanerModel == MODELS.ScanerModel.KodakI1120)
				{
					scanner = new TwainApp.TwainScanner(mainFormHnd, "Kodak Scanner: i112");

					scanner.FileTransferMode = TwainApp.TransferMode.Memory;
					scanner.Compression = TwainApp.Compression.None;
					scanner.KodakIndicatorsWarmUp = false;
					scanner.KodakEnergySavingsTimeout = Math.Max(5, Math.Min(240, this.settings.TwainScanner.EnergyStarTimeout));

					this.Brightness = settings.TwainScanner.BrightnessDelta;
					this.Contrast = settings.TwainScanner.ContrastDelta;

					serialNumber = "i1120";
				}
				else if ( (this.model.ScanerModel == MODELS.ScanerModel.KodakI1150) || (this.model.ScanerModel == MODELS.ScanerModel.KodakI1150New) )
				{
					if (this.model.ScanerModel == MODELS.ScanerModel.KodakI1150)
					{
						scanner = new TwainApp.TwainScanner(mainFormHnd, "Kodak Scanner: i115");
					}
					else
                    {
						// new driver for i1100 series that we use for i1150 running on Win10 
						scanner = new TwainApp.TwainScanner(mainFormHnd, "Kodak Scanner: i110");
					}

					scanner.FileTransferMode = TwainApp.TransferMode.Memory;
					scanner.Compression = TwainApp.Compression.None;
					scanner.KodakIndicatorsWarmUp = false;
					scanner.KodakPowerOffTimeout = Math.Max(5, Math.Min(240, this.settings.TwainScanner.EnergyStarTimeout));

					if (scanner.IsCapabilitySupported((UInt16)TwainApp.SharedCap.Icap_BRIGHTNESS))
						this.Brightness = settings.TwainScanner.BrightnessDelta;
					if (scanner.IsCapabilitySupported((UInt16)TwainApp.SharedCap.Icap_CONTRAST))
						this.Contrast = settings.TwainScanner.ContrastDelta;

					serialNumber = "i1150";
				}
				else if ((this.model.ScanerModel == MODELS.ScanerModel.KodakE1035) || (this.model.ScanerModel == MODELS.ScanerModel.KodakE1040))
				{
					if (this.model.ScanerModel == MODELS.ScanerModel.KodakE1035)
					{
						// for E1035 we need driver for E1000 series
						try
						{
							scanner = new TwainApp.TwainScanner(mainFormHnd, "Alaris Scanner: e100");   //E1000 series - older driver used with KIC and Opus							
						}
												
						catch (Exception ex)
						{
							if (string.Compare(ex.Message, "Specified source was not found!") == 0)
							{
								scanner = new TwainApp.TwainScanner(mainFormHnd, "Kodak Scanner: e100");   //E1000 series - newest driver
							}
							else
                            {
								throw ex;
                            }
						}

					}
					else
					{
						// for E1040 we need driver for E1xxx scanners
						scanner = new TwainApp.TwainScanner(mainFormHnd, "Kodak Scanner: e1x");    //E1xxx scanners
					}

					scanner.FileTransferMode = TwainApp.TransferMode.Memory;
					scanner.Compression = TwainApp.Compression.None;
					scanner.KodakIndicatorsWarmUp = false;
					scanner.KodakPowerOffTimeout = Math.Max(5, Math.Min(120, this.settings.TwainScanner.EnergyStarTimeout));  //E1035 supports max 120 minutes power-off timeout

					if (scanner.IsCapabilitySupported((UInt16)TwainApp.SharedCap.Icap_BRIGHTNESS))
						this.Brightness = settings.TwainScanner.BrightnessDelta;
					if (scanner.IsCapabilitySupported((UInt16)TwainApp.SharedCap.Icap_CONTRAST))
						this.Contrast = settings.TwainScanner.ContrastDelta;

					if (this.model.ScanerModel == MODELS.ScanerModel.KodakE1035)
					{
						serialNumber = "e1035";
					}
					else
                    {
						serialNumber = "e1040";
					}
				}
				else
					throw new Exception("Unexpected scanner type!");


				TwainApp.DeviceInfo twainDeviceInfo = scanner.GetDeviceInfo();

				string productName = twainDeviceInfo.ProductName;

				try
				{
					if (scanner.SerialNumber != null && scanner.SerialNumber.Trim().Length > 0)
						serialNumber += "-" + scanner.SerialNumber;
				}
				catch { }

				try
				{
					if (scanner.Firmware != null && scanner.Firmware.Trim().Length > 0)
						firmware = scanner.Firmware;
					else
						firmware = twainDeviceInfo.Version.MajorNum.ToString() + "." + twainDeviceInfo.Version.MinorNum.ToString();
				}
				catch
				{
					firmware = twainDeviceInfo.Version.MajorNum.ToString() + "." + twainDeviceInfo.Version.MinorNum.ToString();
				}

				deviceInfo = new DeviceInfoTwain(model, serialNumber, firmware);

				try
				{
#if DEBUG
					Console.WriteLine("EnergySavings: " + scanner.EnergySavings.ToString());
					Console.WriteLine("EnergySavingsTimeout: " + scanner.EnergySavingsTimeout.ToString());
#endif
					
					scanner.EnergySavingsTimeout = Math.Max(1,settings.TwainScanner.EnergyStarTimeout) ;						
					scanner.EnergySavings = settings.TwainScanner.IsEnergyStarOn;
				}
				catch { }
			}
			catch (Exception ex)
			{
				HardDispose();
				notifications.Notify(this, Notifications.Type.Warning, "Can't connect to the scanner!" + " " + ex.Message, ex);
				throw new ScannersEx("Can't connect to the scanner!" + " " + ex.Message);
			}
			
			scanner.ImageScanned += new TwainApp.ImageEventHandler(Scanner_ImageScanned);
			scanner.ScanError += new TwainApp.ErrorEventHandler(Scanner_ScanError);

			Reset();
		}
		#endregion

		#region Progress_Changed()
		protected void Progress_Changed(string description, float progress)
		{
			if (ProgressChanged != null)
				ProgressChanged(description, progress);
		}
		#endregion
	
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Scanner_ImageScanned()
		void Scanner_ImageScanned(TwainApp.TwainImage twainImage, bool moreImagesToTransfer)
		{
			if (twainImage.Bitmap != null)
			{
				if (this.model.ScannerSubGroup == MODELS.ScannerSubGroup.TwainAdf)
				{
				}
				else
				{
					//iVina
					/*if (this.bookedgePage == Scanners.Twain.BookedgePage.LeftPage)
						twainImage.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipNone);
					else*/
					if (this.bookedgePage == Scanners.Twain.BookedgePage.FlatMode)
					{
						Bitmap b = ImageProcessing.RotationFlipping.Go(twainImage.Bitmap, RotateFlipType.Rotate90FlipNone);
						twainImage.SwapBitmap(b);
					}
					else if (this.bookedgePage == Scanners.Twain.BookedgePage.RightPage)
					{
						Bitmap b = ImageProcessing.RotationFlipping.Go(twainImage.Bitmap, RotateFlipType.Rotate180FlipNone);
						twainImage.SwapBitmap(b);
					}
				}

				if (ImageScanned != null)
					ImageScanned(this.operationId, twainImage, moreImagesToTransfer);

				try
				{
					if (this.model.ScannerSubGroup == MODELS.ScannerSubGroup.BookEdge)
					{
						scanner.EnergySavingsTimeout = Math.Max(1, settings.TwainScanner.EnergyStarTimeout);
						scanner.EnergySavings = settings.TwainScanner.IsEnergyStarOn;
					}
				}
#if DEBUG
				catch (Exception ex)
				{
					Console.WriteLine("ERROR in TwainBase, Scanner_ImageScanned(): " + Scanners.Misc.GetErrorMessage(ex));
				};
#else
				catch { };
#endif
			}
		}
		#endregion

		#region Scanner_ScanError()
		protected void Scanner_ScanError(Exception ex)
		{
			Scanner_ScanError(this.operationId, ex);
		}

		protected void Scanner_ScanError(int operationId, Exception ex)
		{
			if (ScanError != null)
			{
				if (ex.Message.ToLower().Contains("canceled"))
					ScanError(operationId, new ScannersEx(ex.Message));
				else
					ScanError(operationId, ex);
			}
		}
		#endregion

		#region GetPixelFormat()
		protected PixelFormat GetPixelFormat(Scanners.Twain.ColorMode colorMode)
		{
			switch (colorMode)
			{
				case Scanners.Twain.ColorMode.Bitonal: return PixelFormat.Format1bppIndexed;
				case Scanners.Twain.ColorMode.Grayscale: return PixelFormat.Format8bppIndexed;
				default: return PixelFormat.Format24bppRgb;
			}
		}
		#endregion

		#region GetPixelType()
		protected TwainApp.PixelType GetPixelType(Scanners.Twain.ColorMode colorMode)
		{
			switch (colorMode)
			{
				case Scanners.Twain.ColorMode.Bitonal: return TwainApp.PixelType.BW;
				case Scanners.Twain.ColorMode.Grayscale: return TwainApp.PixelType.GRAY;
				default: return TwainApp.PixelType.RGB;
			}
		}
		#endregion

		#region MessageHandler_MessageReceived()
		void MessageHandler_MessageReceived(ref System.Windows.Forms.Message msg)
		{
			  if (scanner != null)
				scanner.WndProc(msg);
		}
		#endregion

		#endregion

	}

}
