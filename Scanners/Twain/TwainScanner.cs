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
	public class TwainScanner : TwainBase, IScanner
	{


		#region constructor
		/// <summary>
		/// must be called from main window thread!!!
		/// </summary>
		private TwainScanner(object caller, Scanners.MODELS.Model model)
			: base(caller, model)
		{
		}
		#endregion

	
		//PUBLIC METHODS
		#region public methods

		#region static GetInstance()
		/// <summary>
		/// must be called from main window thread!!!
		/// </summary>
		public static TwainScanner GetInstance(object caller, Scanners.MODELS.Model model)
		{
			foreach (IScanner instance in instances)
			{
				if (instance.Model.ScanerModel == model.ScanerModel)
				{
					if (((TwainScanner)instance).registeredCallers.Contains(caller) == false)
						((TwainScanner)instance).registeredCallers.Add(caller);

					return (TwainScanner)instance;
				}
			}

			TwainScanner twainScanner = new TwainScanner(caller, model);
			instances.Add(twainScanner);

			return twainScanner;
		}
		#endregion

		#region static GetSerialNumber()
		/// <summary>
		/// must be called from main window thread!!!
		/// </summary>
		public static string GetSerialNumber(object caller, Scanners.MODELS.Model model)
		{
			foreach (IScanner instance in instances)
			{
				if (instance.Model.ScanerModel == model.ScanerModel)
				{
					return ((TwainScanner)instance).DeviceInfo.SerialNumber;
				}
			}

			TwainScanner twainScanner = null;
			try
			{
				twainScanner = GetInstance(caller, model);
				return twainScanner.DeviceInfo.SerialNumber;
			}
			finally
			{
				if (twainScanner != null)
					twainScanner.Dispose(caller);
			}
		}
		#endregion

		#region Dispose()
		[Obsolete]
		public void Dispose()
		{
			throw new Exception("TwainScanner, Dispose() can't be used! ");
		}
		#endregion

		#region Scan()
		public void Scan(int operationId, Scanners.Twain.BookedgeSettings settings)
		{
			if (scanner != null)
			{
				lock (this.locker)
				{					
					Thread t = new Thread(new ParameterizedThreadStart(TurnLightOnAndScanTU));
					t.Name = "ThreadTwainBase_TurnLightOnAndScan";
					t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
					t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
					t.SetApartmentState(ApartmentState.STA);
					t.Start(new object[] { operationId, settings });
				}
			}
		}
		#endregion

		#region GetScannerSettings()
		public Scanners.Twain.BookedgeSettings GetScannerSettings()
		{
			return new BookedgeSettings(this.scanner);
		}
		#endregion

		#endregion


		//PROTECTED METHODS
		#region protected methods

		#region TurnLightOnAndScanTU()
		void TurnLightOnAndScanTU(object obj)
		{
			int operationId = (int)((object[])obj)[0];
			Scanners.Twain.BookedgeSettings settings = (Scanners.Twain.BookedgeSettings)((object[])obj)[1];

			try
			{
				DateTime start = DateTime.Now;

				if (scanner.LampState == false)
				{
					Progress_Changed("Warming up Lights...", 0);

					while (scanner.LampState == false)
					{
						if (DateTime.Now.Subtract(start).TotalSeconds > 60)
							throw new ScannersEx("Problem with scanner! The light can't be warmed up in reasonable time.");
						else
							Thread.Sleep(1000);
					}
				}

				ScanTU(operationId, settings);
			}
			catch (Exception ex)
			{
				Scanner_ScanError(ex);
			}
		}
		#endregion	
	
		#region ScanTU()
		/// <summary>
		/// 
		/// </summary>
		/// <param name="operationId"></param>
		/// <param name="bookedgePage"></param>
		/// <param name="docSize"></param>
		/// <param name="dpi"></param>
		/// <param name="brightness">interval -1, 1</param>
		/// <param name="contrast">interval -1, 1</param>
		protected void ScanTU(int operationId, Scanners.Twain.BookedgeSettings settings)
		{
			this.operationId = operationId;
			scanner.PredefinedScanArea = TwainApp.PredefinedScanArea.None;

			Progress_Changed("Scanning...", 0);

			if (settings.DocSize.Value == Scanners.Twain.DocSize.Max)
			{
				scanner.AutomaticBorderDetection = false;
				scanner.AutomaticDeskewDetection = false;
				scanner.UndefinedImageSize = false;
				scanner.AutomaticRotate = false;
			}
			else
			{
				scanner.AutomaticBorderDetection = true;
				scanner.AutomaticDeskewDetection = true;
				scanner.UndefinedImageSize = true;
				scanner.AutomaticRotate = (settings.ScanPage.Value == Scanners.Twain.BookedgePage.Automatic);
			}

			scanner.PixelDepth = GetPixelFormat(settings.ColorMode.Value);
			scanner.PixelType = GetPixelType(settings.ColorMode.Value);
			scanner.Resolution = settings.Dpi.Value;
			scanner.CaptureIndicators = false;

			if (scanner.IsCapabilitySupported((UInt16)TwainApp.SharedCap.Icap_BRIGHTNESS))
				scanner.Brightness = settings.Brightness.Value;
			if (scanner.IsCapabilitySupported((UInt16)TwainApp.SharedCap.Icap_CONTRAST))
				scanner.Contrast = settings.Contrast.Value;
			
			this.bookedgePage = settings.ScanPage.Value;

			try
			{
				scanner.Scan();
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "TwainScanner, Scan(): " + ex.Message, ex);
				throw new ScannersEx("Scanning process was not successfull!");
			}
		}
		#endregion

		#endregion

	}
}
