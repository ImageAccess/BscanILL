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
	public class AdfScanner : TwainBase, IDisposable, IScanner
	{
		//bool											isFeederLoaded = true;
		//System.Windows.Threading.DispatcherTimer		timer = new System.Windows.Threading.DispatcherTimer();

		//public delegate void FeederLoadedHnd();
		//public event Kic.Scanners.AdfScanner.FeederLoadedHnd		FeederLoaded;
		//public event Kic.Scanners.AdfScanner.FeederLoadedHnd		FeederUnloaded;
		public event TwainApp.TwainStateChangedHandler				TwainStateChanged;


		#region constructor
		/// <summary>
		/// must be called from main window thread!!!
		/// </summary>
		private AdfScanner(object caller, Scanners.MODELS.Model model)
			: base(caller, model)
		{
			scanner.TwainStateChanged += new TwainApp.TwainStateChangedHandler(Scanner_TwainStateChanged);
		}
		#endregion


		#region enum AdfScannerType
		/*public enum AdfScannerType
		{
			Standalone,
			AddOnScanner
		}*/
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public bool						Initialized		{ get { return (scanner != null); } }
		public TwainApp.TwainState		TwainState		{ get { return (scanner != null) ? scanner.State : TwainApp.TwainState.PreSession; } }

		#region IsFeederLoaded
		/*public bool IsFeederLoaded 
		{ 
			get { return this.isFeederLoaded; }
			private set
			{
				if (this.isFeederLoaded != value)
				{
					this.isFeederLoaded = value;

					if (this.isFeederLoaded)
					{
						if (FeederLoaded != null)
							FeederLoaded();
					}
					else
					{
						if (FeederUnloaded != null)
							FeederUnloaded();
					}
				}
			}
		}*/
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region static GetInstance()
		/// <summary>
		/// must be called from main window thread!!!
		/// </summary>
		public static AdfScanner GetInstance(object caller, Scanners.MODELS.Model model)
		{
			foreach (IScanner instance in instances)
			{
				if (instance.Model.ScanerModel == model.ScanerModel)
				{
					if (((AdfScanner)instance).registeredCallers.Contains(caller) == false)
						((AdfScanner)instance).registeredCallers.Add(caller);

					return (AdfScanner)instance;
				}
			}

			AdfScanner adfScanner = new AdfScanner(caller, model);
			instances.Add(adfScanner);

			return adfScanner;
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
					return ((AdfScanner)instance).DeviceInfo.SerialNumber;
				}
			}

			AdfScanner adfScanner = null;
			try
			{
				adfScanner = GetInstance(caller, model);
				return adfScanner.DeviceInfo.SerialNumber;
			}
			finally
			{
				if (adfScanner != null)
					adfScanner.Dispose(caller);
			}
		}
		#endregion
	
		#region Dispose()
		[Obsolete]
		public void Dispose()
		{
			throw new Exception("AdfScanner, Dispose() can't be used! ");
		}
		#endregion

		#region Dispose()
		/*public void Dispose()
		{
			base.Dispose(this);
		}*/
		#endregion
	
		#region Scan()
		public void Scan(int operationId, Scanners.Twain.AdfSettings adfSettings)
		{
			if (scanner != null)
			{
				lock (this.locker)
				{
                    //adfSettings.TransferCount.Value = 1;  //debug to scan just 1 page with Kodak                    

					Thread t = new Thread(new ParameterizedThreadStart(TurnLightOnAndScanAdfTU));
					t.Name = "ThreadAdfScanner_TurnLightOnAndScanAdf";
					t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
					t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
					t.SetApartmentState(ApartmentState.STA);
					t.Start(new object[] { operationId, adfSettings });
				}
			}
		}
		#endregion

		#region GetScannerSettings()
		public Scanners.Twain.AdfSettings GetScannerSettings()
		{
			return new AdfSettings(this.scanner);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region TurnLightOnAndScanAdfTU()
		void TurnLightOnAndScanAdfTU(object obj)
		{
			int operationId = (int)((object[])obj)[0];
			Scanners.Twain.AdfSettings settings = (Scanners.Twain.AdfSettings)((object[])obj)[1];

			try
			{
				ScanAdfTU(operationId, settings);
			}
			catch (Exception ex)
			{
				Scanner_ScanError(operationId, ex);
			}
		}
		#endregion

		#region ScanAdfTU()
		void ScanAdfTU(int operationId, Scanners.Twain.AdfSettings settings)
		{
			this.operationId = operationId;
			scanner.PredefinedScanArea = TwainApp.PredefinedScanArea.None;

			Progress_Changed("Scanning...", 0);

			scanner.PixelDepth = GetPixelFormat(settings.ColorMode.Value);
			scanner.PixelType = GetPixelType(settings.ColorMode.Value);
			scanner.Resolution = settings.Dpi.Value;
			scanner.CaptureIndicators = false;
			scanner.TransferCount = settings.TransferCount.Value;
			scanner.DocumentFeederDuplexEnabled = settings.Duplex.Value;

			if (scanner.IsCapabilitySupported((UInt16)TwainApp.SharedCap.Icap_BRIGHTNESS))
				scanner.Brightness = settings.Brightness.Value;
			if (scanner.IsCapabilitySupported((UInt16)TwainApp.SharedCap.Icap_CONTRAST))
				scanner.Contrast = settings.Contrast.Value;

			try
			{
				scanner.Scan();
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "AdfScanner, Scan(): " + ex.Message, ex);
				throw new ScannersEx("Scanning process was not successfull!");
			}
		}
		#endregion

		#region Timer_Tick()
		void Timer_Tick(object sender, EventArgs e)
		{
			/*try
			{
				this.timer.Stop();

				if (scanner != null)
				{
					lock (this.locker)
					{
						this.IsFeederLoaded = scanner.DocumentFeederLoaded;

#if DEBUG
						Console.WriteLine("ADF Feeder: " + (this.IsFeederLoaded ? "Loaded" : "Unloaded"));
#endif
					}
					this.timer.Start();
				}
			}
			catch (Exception ex)
			{
				notifications.Notify(this, Notifications.Type.Warning, "The ADF scanner is not accessible. ", ex);
			}*/
		}
		#endregion

		#region Scanner_TwainStateChanged()
		void Scanner_TwainStateChanged(TwainApp.TwainState twainState)
		{	
			if (TwainStateChanged != null)
				TwainStateChanged(twainState);
		}
		#endregion
	
		#endregion

	}

}
