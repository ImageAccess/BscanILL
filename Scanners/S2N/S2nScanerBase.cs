using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Net;
using System.Net.Cache;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections;

namespace Scanners.S2N
{
	/*
	* Warnings

		WARNING 128:  Mechanical problem: Left Book cradle is not adjusted
		WARNING 129:  Mechanical problem: Right Book cradle is not adjusted
		WARNING 130:  Foot Pedal 1: Switch permanently closed
		WARNING 130:  Foot Pedal 1: Switch permanently closed
		WARNING 131:  Foot Pedal 2: Switch permanently closed
		WARNING 131:  Foot Pedal 2: Switch permanently closed

		WARNING 144:  Light level is low
		WARNING 145:  Camera adjustment required
		WARNING 146:  Left lamp blocked
		WARNING 147:  Right lamp blocked

		WARNING 160:  No white balance data

		WARNING 180:  Deskew failed
		WARNING 181:  Stitching2D: out of memory. Using fixed stitching
		WARNING 182:  Ready to feed
		WARNING 183:  Document oversized (Out of Memory)
		WARNING 184:  Failed to execute Auto Exposure (Memory ?),
		WARNING 185:  Failed to execute Adaptive Stitch (Memory ?)
		WARNING 186:  Failed to execute Crop/Deskew (Memory ?
		WARNING 191:  Bookfold failed
		WARNING 192:  Bookfold failed 
		WARNING 193:  Bookfold failed 

		WARNING 194:  Crop failed (left edge!)
		WARNING 195:  Crop failed (right edge!)
		WARNING 196:  Crop failed (upper edge!)
		WARNING 197:  Crop failed (lower edge!)
		WARNING 198:  Crop failed (wide open!)
		WARNING 199:  Crop failed (res2)

	*/
	public abstract class S2nScanerBase
	{
		protected static object threadLocker = new object();
        
		string		ip;
		string		sessionId = null;
		DateTime	lastLogin = new DateTime(2000, 01, 01);
		
		protected DeviceInfo		deviceInfo;
		protected S2NSettings		settings = null;
		protected int				exceptionIterations = 3;

		object						isDisposedLocker = new object();
		bool						isDisposed = false;

		protected ImageProcessing.BigImages.Resampling resampling = null;

		System.Timers.Timer timerPingDevice = new System.Timers.Timer(290000);
        
		object timerPingDeviceLocker = new object();

		//touch screen monitoring
		bool						isTouchScreenAvailable = false;
        bool                        isHtmlTouchScreenApp = false;
        int                         htmlTouchScreenAppVersion = 0;
        bool                        touchScreenButtonsIni = false;
		System.Timers.Timer			touchScreenMonitoringTimer = new System.Timers.Timer();
		object						touchScreenMonitoringTimerLocker = new object();
		volatile bool				isTouchScreenMonitoringRunning = false;
		ScannerScanAreaSelection	lastScanAreaSelection = ScannerScanAreaSelection.Flat;
		bool						isUiLocked = true;

		public event Scanners.PreviewScannedHnd		PreviewScanned;
		public event Scanners.ProgressChangedHnd	ProgressChanged;

		public event ScanRequestHnd ScanRequest;
		public event ScanRequestHnd ScanPullslipRequest;
        public event ScanRequestHnd TouchScreenScanAreaButtonPressed;


		#region constructor
		public S2nScanerBase(string ip)
		{
			this.ip = ip;

			this.SessionId = Properties.ScannersSettings.Default.IpScannerLastSessionId;
			this.deviceInfo = GetDeviceInfo(this.Ip);

			Thread.Sleep(500);

			try
			{
				SetScanner("chopt.cgi?uset+starttimeout+1+admin", 5000);
				SetScanner("chopt.cgi?uset+standby+240+Admin", 5000);
			}
			catch { }

			Thread.Sleep(200);

			string settings = SendCommandToScanner(Scanners.S2N.S2nScanerBase.Command.Get, "+settings", 5000);

			this.settings = new S2NSettings(settings);

			if (this.IsColorScanner == false)
				resampling = new ImageProcessing.BigImages.Resampling();

            if (deviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE4 || deviceInfo.ScannerModel.ScanerModel == MODELS.ScanerModel.WT25_600)
            //if (deviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE4 || deviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE5 || deviceInfo.ScannerModel.ScanerModel == MODELS.ScanerModel.WT25_600)
            // for now disable touch screen with BE5
			{
				this.isTouchScreenAvailable = true;

				this.touchScreenMonitoringTimer.AutoReset = false;
				this.touchScreenMonitoringTimer.Interval = 500;
				this.touchScreenMonitoringTimer.Elapsed += new ElapsedEventHandler(TouchScreenMonitoring_Tick);
			}
			else
				this.isTouchScreenAvailable = false;

			timerPingDevice.Elapsed += new ElapsedEventHandler(TimerPing_Elapsed);
			timerPingDevice.AutoReset = true;
			timerPingDevice.Start();
		}
        #endregion

        #region enum Command
        public enum Command
		{
			Get,
			Set
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public string			Ip				{ get { return this.ip; } }// (this.DeviceInfo != null) ? ((DeviceInfoS2N)this.DeviceInfo).Ip : Scanners.Settings.Instance.S2NScanner.Ip; } }
		public DeviceInfo		DeviceInfo		{ get { return this.deviceInfo; } }
		public S2NSettings		Settings		{ get { return this.settings; } }
		public bool				IsColorScanner	{ get { return this.Settings.ColorMode.IsColorOptionInstalled; } }
		public short			MaxColorDpi		{ get { return (short)this.Settings.Dpi.Maximum; } }

		public bool				IsTouchScreenMonitoringRunning { get { return this.isTouchScreenMonitoringRunning; } }
		public bool				IsTouchScreenAvailable { get { return this.isTouchScreenAvailable; } }
        
        public int              HtmlTouchScreenAppVersion { get { return this.htmlTouchScreenAppVersion; } set { this.htmlTouchScreenAppVersion = value; } }
		#endregion


		//PROTECTED PROPERTIES
		#region protected properties

		#region SessionId
		protected string SessionId
		{
			get { return this.sessionId; }
			set
			{
				this.sessionId = value;
				Properties.ScannersSettings.Default.IpScannerLastSessionId = this.sessionId;
				Properties.ScannersSettings.Default.Save();
			}
		}
		#endregion

		#region IsDisposed
		protected bool IsDisposed
		{
			get { return this.isDisposed; }
			set
			{
				lock (this.isDisposedLocker)
				{
					this.isDisposed = value;
				}
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Dispose()
		public virtual void Dispose()
		{
            LockUiSpecific("dlsg" , true );   //lock the touch screen when closing BscanILL
			this.IsDisposed = true;

			Logout();
			StopTouchScreenMonitoring();
			lock (touchScreenMonitoringTimerLocker)
			{
				touchScreenMonitoringTimer = null;
			}

			StopTimerPing();
			lock (timerPingDeviceLocker)
			{
				this.timerPingDevice = null;
			}
		}
		#endregion


        #region UpdateTouchScreenExternal()
        public void UpdateTouchScreenExternal(Scanners.S2N.ScannerScanAreaSelection areaSelection)
        {
            this.lastScanAreaSelection = areaSelection;

            if (this.isTouchScreenAvailable && this.IsDisposed == false)
            if (! this.isUiLocked)				
            {                
                //splitting button in BscanILL screen was presset -> sinkup touchscreen button to show same setting on touchscreen
                SetTouchScreenButtons(this.lastScanAreaSelection);
            }
        }
        #endregion

		#region static GetDeviceInfo()
		public static Scanners.DeviceInfo GetDeviceInfo(string ip)
		{
			return GetDeviceInfoInternal(ip);
		}
		#endregion

		#region StartTouchScreenMonitoring()
		public void StartTouchScreenMonitoring()
		{
			lock (touchScreenMonitoringTimerLocker)
			{
				if (this.isTouchScreenAvailable && this.touchScreenMonitoringTimer != null && this.IsDisposed == false)
				{
					this.isTouchScreenMonitoringRunning = true;
					this.touchScreenMonitoringTimer.Stop();
					this.touchScreenMonitoringTimer.Start();
				}
			}
		}
		#endregion

		#region StopTouchScreenMonitoring()
		public void StopTouchScreenMonitoring()
		{
			lock (touchScreenMonitoringTimerLocker)
			{
				if (this.touchScreenMonitoringTimer != null)
					this.touchScreenMonitoringTimer.Stop();
			}
				
			this.isTouchScreenMonitoringRunning = false;
		}
		#endregion

        #region LockUiSpecific()
        public void LockUiSpecific(string screenName, bool forceLock)
        {
            if (isTouchScreenAvailable)
            {
                if ( ( forceLock || this.isUiLocked == false ) && this.IsDisposed == false)
                {
                    lock (threadLocker)
                    {
                        HttpWebRequest webRequest = null;
                        string htmlBody;

                        try
                        {
                            if (ProgressChanged != null)
                                ProgressChanged("Locking UI...", 0);

                            Login();

                            webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:ScreenLocked" + screenName);
                            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                            webRequest.Timeout = 5000;

                            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
                                {
                                    if (streamReader.Peek() >= 0)
                                    {
                                        htmlBody = streamReader.ReadToEnd();

                                        if (htmlBody.IndexOf("OK") < 0)
                                        {
                                            Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, LockUi():" + " " + Environment.NewLine + htmlBody + Environment.NewLine, null);
                                            throw new ScannersEx("Can't setup scanner!");
                                        }
                                        else
                                        {
                                            // everything is OK
                                            this.isUiLocked = true;
                                        }
                                    }
                                    else
                                    {
                                        Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't get scanner response!", null);
                                        throw new ScannersEx("Can't get scanner response!");
                                    }
                                }
                            }

                            if (ProgressChanged != null)
                                ProgressChanged("Locking UI...", 1);
                        }
                        catch (ScannersEx ex)
                        {
                            throw ex;
                        }
                        catch (Exception ex)
                        {
                            Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, LockUi():" + " " + ex.Message, ex);
                            throw new ScannersEx("BookEye exception while locking UI:" + " " + ex.Message, ScannersEx.AlertType.Error);
                        }
                        finally
                        {
                            StartTimerPing();
                        }
                    }
                }
            }
        }
        #endregion

		#region LockUi()
		public void LockUi( bool forceLock )
		{
			if (isTouchScreenAvailable)
			{
                if ( (forceLock || this.isUiLocked == false ) && this.IsDisposed == false)
				{
					lock (threadLocker)
					{
						HttpWebRequest webRequest = null;
						string htmlBody;

						try
						{
							if (ProgressChanged != null)
								ProgressChanged("Locking UI...", 0);

							Login();

							webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:ScreenLockedILL");
							webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
							webRequest.Timeout = 5000;

							using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
							{
								using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
								{
									if (streamReader.Peek() >= 0)
									{
										htmlBody = streamReader.ReadToEnd();

										if (htmlBody.IndexOf("OK") < 0)
										{
											Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, LockUi():" + " " + Environment.NewLine + htmlBody + Environment.NewLine, null);
											throw new ScannersEx("Can't setup scanner!");
										}
										else
										{
											// everything is OK
											this.isUiLocked = true;
										}
									}
									else
									{
										Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't get scanner response!", null);
										throw new ScannersEx("Can't get scanner response!");
									}
								}
							}

							if (ProgressChanged != null)
								ProgressChanged("Locking UI...", 1);
						}
						catch (ScannersEx ex)
						{
							throw ex;
						}
						catch (Exception ex)
						{
							Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, LockUi():" + " " + ex.Message, ex);
							throw new ScannersEx("BookEye exception while locking UI:" + " " + ex.Message, ScannersEx.AlertType.Error);
						}
						finally
						{
							StartTimerPing();
						}
					}
				}
			}
		}
		#endregion

		#region UnlockUi()
		public void UnlockUi( bool singleArticleMode )
		{
			if (isTouchScreenAvailable)
			{
				if (this.isUiLocked)
				{
					lock (threadLocker)
					{
						HttpWebRequest webRequest = null;
						string htmlBody;

						try
						{
							if (ProgressChanged != null)
								ProgressChanged("Unlocking UI...", 0);

							Login();

                            if( this.isHtmlTouchScreenApp )
                            {
                                //beginning html app we support unlocking of ILL,KIC, Opus in one app unlike in previous java app
                                if (singleArticleMode && (this.htmlTouchScreenAppVersion >= 26))
                                {
                                    //touch screen app since version 2.6 supports this SingleILL screen (no scan pull slip button)
                                    webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:ScreenUnlockedILLSingle");
                                }
                                else
                                {
                                    webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:ScreenUnlockedILL");
                                }
                            }
                            else
                            { 
							  webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:ScreenUnlocked");
                            }
							webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
							webRequest.Timeout = 5000;

							using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
							{
								using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
								{
									if (streamReader.Peek() >= 0)
									{
										htmlBody = streamReader.ReadToEnd();

										if (htmlBody.IndexOf("OK") < 0)
										{
											Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, UnlockUi():" + Environment.NewLine + htmlBody + Environment.NewLine, null);
											throw new ScannersEx("Can't setup scanner!");
										}
										else
										{
											// everything is OK
											this.isUiLocked = false;
										}
									}
									else
									{
										Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't get scanner response!", null);
										throw new ScannersEx("Can't get scanner response!");
									}
								}
							}

                            if (! this.touchScreenButtonsIni )
                            {
                                SetTouchScreenButtons( this.lastScanAreaSelection );                               
                            }

							if (ProgressChanged != null)
								ProgressChanged("Unlocking UI...", 1);
						}
						catch (ScannersEx ex)
						{
							throw ex;
						}
						catch (Exception ex)
						{
							Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, UnlockUi(): " + ex.Message, ex);
							throw new ScannersEx("BookEye exception while unlocking UI:" + " " + ex.Message, ScannersEx.AlertType.Error);
						}
						finally
						{
							StartTimerPing();
						}
					}
				}
			}
		}
		#endregion

		#region ResetTouchScreenEvents()
		/// <summary>
		/// Call when the application is launched and before UI is open.
		/// </summary>
		public void ResetTouchScreenEvents()
		{
			ScannerScanAreaSelection scanAreaSelection = ScannerScanAreaSelection.Flat;

			ScannerScanButtonPressed(ref scanAreaSelection);
		}
        #endregion

        #region RequestTouchScreenVersion()
        public void RequestTouchScreenVersion()
        {
            //send GetVErsion command to scanner. If we receive response 'AppGUI:Version:....' we know it is new html5 app (not Java app)
            if (isTouchScreenAvailable)
            {
                lock (threadLocker)
                {
                    HttpWebRequest webRequest = null;
                    string htmlBody;

                    try
                    {
                        Login();

                        webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:GetVersion");

                        webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                        webRequest.Timeout = 5000;

                        using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                        {
                            using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
                            {
                                if (streamReader.Peek() >= 0)
                                {
                                    htmlBody = streamReader.ReadToEnd();

                                    if (htmlBody.IndexOf("OK") < 0)
                                    {
                                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, RequestTouchScreenVersion():" + Environment.NewLine + htmlBody + Environment.NewLine, null);
                                        throw new ScannersEx("Can't setup scanner!");
                                    }
                                }
                                else
                                {
                                    Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't get scanner response!", null);
                                    throw new ScannersEx("Can't get scanner response!");
                                }
                            }
                        }

                    }
                    catch (ScannersEx ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, RequestTouchScreenVersion(): " + ex.Message, ex);
                        throw new ScannersEx("BookEye exception while request version UI:" + " " + ex.Message, ScannersEx.AlertType.Error);
                    }
                    finally
                    {
                        StartTimerPing();
                    }
                }

            }
        }        
        #endregion

        #region GetTouchScreenVersion()
        public void GetTouchScreenVersion(bool wakeOnLANExecuted)
        {            
            if (this.isTouchScreenAvailable)
            {
                lock (threadLocker)
                {
                    try
                    {
                        Login();

                        for (int i = 0; i <= exceptionIterations; i++)
                        {
                            try
                            {
                                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/events?" + this.SessionId + "+poll");
                                webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                                webRequest.Timeout = 5000;

                                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                                {
                                    using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
                                    {
                                        if (streamReader.Peek() >= 0)
                                        {
                                            string htmlBody = streamReader.ReadToEnd();

                                            if (htmlBody.IndexOf("OK") < 0 && htmlBody.IndexOf("ERROR 8") < 0)
                                            {
                                                if (htmlBody.Contains("Invalid"))
                                                {
                                                    Login();
                                                }
                                                else
                                                {
                                                    Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, GetTouchScreenVersion(): " + Environment.NewLine + htmlBody + Environment.NewLine, null);
                                                    throw new ScannersEx("Can't setup scanner!");
                                                }
                                            }
                                            else
                                            {
                                                string[] lines = htmlBody.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                                                for (int ii = lines.Length - 1; ii >= 0; ii--)
                                                {                                                    
                                                    if (lines[ii].Trim().ToLower().Contains("appgui:version:") )   
                                                    {
                                                        //htmlTouchScreenAppVersion
                                                        this.isHtmlTouchScreenApp = true;

                                                        string version = lines[ii].Trim().ToLower();
                                                        int index = version.LastIndexOf("appgui:version:");
                                                        if (index >= 0)
                                                        {
                                                            index += 15;
                                                            version = version.Substring(index);
                                                            string[] wordChunks = version.Split(' ');
                                                            if (wordChunks.Length > 0)
                                                            {
                                                                version = wordChunks[0].Replace(".", "");
                                                                if (version.Length > 0)
                                                                {
                                                                    this.htmlTouchScreenAppVersion = int.Parse(version);
                                                                }
                                                            }
                                                        }

                                                        i = exceptionIterations + 1;
                                                        ii = -1;
                                                    }
                                                }

                                                if (( ! this.isHtmlTouchScreenApp) && (wakeOnLANExecuted) && (i < exceptionIterations))
                                                {
                                                    Thread.Sleep(12000);// when waking on LAN performed, BscanILL needs to wait a bit in order to get response from a scanner that just boots up..
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't get scanner response!", null);
                                            throw new ScannersEx("Can't get scanner response!");
                                        }
                                    }
                                }
                            }
                            catch (ScannersEx ex)
                            {
                                throw ex;
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.ToLower().Contains("timed out") && i < exceptionIterations)
                                    Thread.Sleep(i * 100);
                                else
                                {
                                    Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, GetTouchScreenVersion(): " + ex.Message, ex);
                                    throw new ScannersEx("BookEye exception while get version UI:" + " " + ex.Message, ScannersEx.AlertType.Error);
                                }
                            }
                        }
                        
                    }
                    finally
                    {
                        StartTimerPing();                        
                    }
                }
            }            
        }        
        #endregion

        #region StartTimerPing()
        public void StartTimerPing()
        {
            lock (timerPingDeviceLocker)
            {
                if (this.timerPingDevice != null && this.IsDisposed == false)
                {
                    this.timerPingDevice.Stop();
                    this.timerPingDevice.AutoReset = true;
                    this.timerPingDevice.Start();
                }
            }
        }
        #endregion

        #region StopTimerPing()
        public void StopTimerPing()
        {
            lock (timerPingDeviceLocker)
            {
                if (this.timerPingDevice != null)
                {
                    this.timerPingDevice.AutoReset = false;
                    this.timerPingDevice.Stop();
                }
            }
        }
        #endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		protected abstract string SendCommandToScanner(S2nScanerBase.Command command, string parameters, int timeout);

		#region SetScanner()
		protected void SetScanner(string command, int timeout)
		{
			HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/" + command);
			webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			webRequest.Timeout = timeout;

			using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
			{
				using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
				{
					if (reader.Peek() < 0)
						throw new ScannersEx("Can't get scanner response!");
				}
			}
		}
		#endregion

		#region static GetDeviceInfoInternal()
		protected static Scanners.DeviceInfo GetDeviceInfoInternal(string ip)
		{
			try
			{
				//scanning
				string htmlBody = "";
				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://" + ip + @"/cgi/s2ninfo");
				webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
				webRequest.Timeout = 3000;
				
				using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
				{
					using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
					{
						if (reader.Peek() >= 0)
							htmlBody = reader.ReadToEnd();
					}
				}

				if (htmlBody.IndexOf("OK") < 0)
				{
					throw new Exception(htmlBody);
					//return Scanners.DeviceInfo.Empty;
				}
				else
				{
					string[] parameters = htmlBody.Split(new char[] { '\n' });
					char[] charArray = new char[] { '\n', '\r' };

					for (int i = 0; i < parameters.Length; i++)
						parameters[i] = parameters[i].Trim(charArray);

					Hashtable hashtable = new Hashtable();
					charArray = new char[] { ':' };

					for (int i = 0; i < parameters.Length; i++)
					{
						if (parameters[i].Length > 0)
						{
							string[] note = parameters[i].Split(charArray);

							hashtable.Add(note[0], note[1].Substring(1));
						}
					}					

					Scanners.MODELS.ScanerModel  scannerModel = (Scanners.MODELS.ScanerModel)Convert.ToByte(hashtable["DeviceType"]);
					Scanners.MODELS.Model iScannerModel = new Scanners.MODELS.Model(scannerModel);

					Scanners.DeviceInfo deviceInfo = new Scanners.DeviceInfoS2N(ip, iScannerModel,
						hashtable["FirmwareVersion"].ToString(), hashtable["hostname"].ToString(), hashtable["devicename"].ToString(), hashtable["netmask"].ToString(),
						hashtable["gateway"].ToString(), hashtable["dhcp"].ToString());

					return deviceInfo;
				}
			}
			catch (Exception ex)
			{
				throw new ScannersEx("Can't connect to the scanner!" + Environment.NewLine + ex.Message, ScannersEx.AlertType.Error);
			}
		}
		#endregion
	
		#region Login()
		protected void Login()
		{
			bool loginNeeded = true;

			StopTimerPing();

			if (this.SessionId != null && this.SessionId.Length > 0)
			{
                if (DateTime.Now.Subtract(lastLogin).TotalSeconds < 60)
                {
                    loginNeeded = false;
                }
                else
                {
                    for (int i = 0; i <= exceptionIterations; i++)
                    {
                        try
                        {
                            HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/get?" + this.SessionId + "+dpi");
                            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                            webRequest.Timeout = 1000;

                            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
                                {
                                    if (reader.Peek() >= 0)
                                    {
                                        string htmlBody = reader.ReadToEnd();

                                        if (htmlBody.IndexOf("OK") > 0)
                                        {
                                            loginNeeded = false;
                                            this.lastLogin = DateTime.Now;
                                            break;
                                        }
                                        else
                                        {
                                            if (htmlBody.IndexOf("Invalid") >= 0)
                                            {
                                                loginNeeded = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (ScannersEx ex)
                        {
                            throw ex;
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.ToLower().Contains("timed out") && i < exceptionIterations)
                                Thread.Sleep(i * 100);
                            else
                                throw new Exception("Can't open scanner session:" + " " + ex.Message);
                        }
                    }
                }
			}

			if (loginNeeded)
			{
				for (int i = 0; i <= exceptionIterations; i++)
				{
					try
					{
						HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/s2nopen");
						webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
						webRequest.Timeout = 5000;

						using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
						{
							using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
							{
								if (reader.Peek() >= 0)
								{
									string line = reader.ReadToEnd();

									if (line.Substring(0, 2).ToUpper() == "ID")
									{                                        
										int indexOf = line.IndexOf('\r');
										this.SessionId = line.Substring(4, indexOf - 4);
										this.lastLogin = DateTime.Now;
										break;
									}
									else
									{
										Notifications.Instance.Notify(null, Notifications.Type.Warning, "Can't open scanner session:" + " " + line, null);
										throw new ScannersEx("Can't open scanner session:" + " " + line, ScannersEx.AlertType.Warning);
									}
								}
								else
								{
									Notifications.Instance.Notify(null, Notifications.Type.Warning, "Can't open scanner session!", null);
									throw new ScannersEx("Can't connect to the scanner!", ScannersEx.AlertType.Error);
								}
							}
						}
					}
					catch (ScannersEx ex)
					{
						throw ex;
					}
					catch (Exception ex)
					{
						if (ex.Message.ToLower().Contains("timed out") && i < exceptionIterations)
							Thread.Sleep(i * 100);
						else
							throw new Exception("Can't open scanner session:" + " " + ex.Message);
					}
				}
			}
		}
		#endregion

		#region Logout()
		internal void Logout()
		{
			try
			{
				if (this.SessionId != null && this.SessionId.Length > 0)
				{
					for (int i = 0; i <= exceptionIterations; i++)
					{
						try
						{
							HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/s2nclose?" + this.SessionId);
							webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
							webRequest.Timeout = 5000;

							using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
							{
								using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
								{
									string line = reader.ReadLine();

									if (line.IndexOf("OK") < 0)
										throw new Exception(line);
								}
							}

							break;
						}
						catch (Exception ex)
						{
							if (ex.Message.ToLower().Contains("timed out") && i < exceptionIterations)
								Thread.Sleep(i * 100);
							else
							{
								//throw new Exception("Can't close scanner session: " + ex.Message);
							}
						}
					}
				}
			}
			finally
			{
				this.SessionId = null;
				StartTimerPing();
			}
		}
		#endregion
	
		#region TimerPing_Elapsed()
		void TimerPing_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				lock (threadLocker)
				{
					string response = SendCommandToScanner(Command.Set, "+speed:2", 5000);

					if (response.Contains("OK") == false)
					{
					}

				}
			}
			catch (Exception ex)
			{
				if (ex.Message.ToLower().Contains("timeout"))
					Notifications.Instance.Notify(this, Notifications.Type.Warning, "Warning: Scanner is not turned on!" + " " + ex.Message, ex);
			}
		}
		#endregion

		#region DownloadPreview()
		protected void DownloadPreview()
		{
			try
			{
				if (PreviewScanned != null)
				{
					//show preview
					HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/preview?" + this.SessionId);
					webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
					webRequest.Timeout = 20000;

					using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
					{
						using (StreamReader thumbReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII, true))
						{
							if (webResponse.ContentType.ToLower().StartsWith("image"))
							{
								Bitmap thumbnail = new Bitmap(thumbReader.BaseStream);
								PreviewScanned(thumbnail);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(null, Notifications.Type.Error, "S2N Scanner, DownloadPreview(): " + ex.Message, ex);
			}
		}
		#endregion

		#region SendScanCommand()
		/// <summary>
		/// returns web response
		/// </summary>
		/// <returns></returns>
		public string SendScanCommand()
		{
			try
			{
				HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/scan?" + this.SessionId);
				webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                webRequest.Timeout = 30000;
				using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
					{
						return streamReader.ReadToEnd();
					}
				}
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(null, Notifications.Type.Error, "ScannerS2N, Scan(): Exception while sending scan request. " + ex.Message, ex);
				throw new ScannersEx("There is not connection to the scanner!", ScannersEx.AlertType.Warning);
			}
		}
		#endregion
	
		#region DownloadImage()
		protected void DownloadImage(string filePath)
		{
			try
			{
				HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/image?" + this.SessionId);
				webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
				webRequest.Timeout = 40000;

				using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
					{
						if (webResponse.ContentType.ToLower().StartsWith("image") == false)
						{
							if (streamReader.Peek() >= 0)
								Notifications.Instance.Notify(null, Notifications.Type.Error, "S2N ScannerBase, Scan(): Web Response " + webResponse.ContentType + " " + streamReader.ReadToEnd(), null);
							else
								Notifications.Instance.Notify(null, Notifications.Type.Error, "S2N ScannerBase, Scan(): Web Response " + webResponse.ContentType, null);

							throw new ScannersEx("Can't get image!");
						}
						else
						{
							using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
							{
								int bufferSize = 0x20000;
								byte[] buffer = new byte[bufferSize];
								int bytesRead;

								while ((bytesRead = streamReader.BaseStream.Read(buffer, 0, bufferSize)) > 0)
								{
									stream.Write(buffer, 0, bytesRead);
								}
							}
						}
					}
				}
			}
			catch (ScannersEx ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(null, Notifications.Type.Error, "S2N ScannerBase, DownloadImage(): " + ex.Message, ex);
				throw new ScannersEx("BookEye exception while scanning:" + " " + ex.Message, ScannersEx.AlertType.Error);
			}
		}
		#endregion

		#region TouchScreenMonitoring_Tick()
		void TouchScreenMonitoring_Tick(object sender, ElapsedEventArgs e)
		{
			//lock (threadLocker)
			{
				try
				{
					if (this.IsDisposed == false)
					{
                        TouchScreenButton button = TouchScreenButton.None;
                        Scanners.S2N.ScannerScanAreaSelection selection = Scanners.S2N.ScannerScanAreaSelection.Flat;
                        lock (threadLocker)
                        { 
                            ScannerScanAreaSelection priorSplittingSelection = this.lastScanAreaSelection ;
						    
						    button = ScannerScanButtonPressed(ref selection);

                            if (priorSplittingSelection != this.lastScanAreaSelection)
                            {
                               //green splitting buttons on screen was pressed - select it in GUI
                                SetTouchScreenButtons(this.lastScanAreaSelection);

                                //update spliting buttons in BscanILL screen (BscanILL window)
                                if (TouchScreenScanAreaButtonPressed != null)
                                    TouchScreenScanAreaButtonPressed(this.lastScanAreaSelection);
                            }
                            if (button == TouchScreenButton.Scan)
                            {
#if DEBUG
                                Console.WriteLine("ScanRequest: " + DateTime.Now.ToString());
#endif

                                if (ScanRequest != null)
                                    ScanRequest(selection);
                            }
                        }

						if (button == TouchScreenButton.ScanPullslip)
						{
                            //needed to remove it from threadlocker as scan pull slip request calls method that goes to start tab
                            //which includes lockui and unlockui on touch screen which is also wrapped in threadlocker so code froze once it hit threadlocker second time in lockui
							if (ScanPullslipRequest != null)
								ScanPullslipRequest(selection);
						}

                       // ProcessScannerBuffer();
					}
				}
				catch (ScannersEx)
				{
					StopTouchScreenMonitoring();
				}
				catch (Exception ex)
				{
					StopTouchScreenMonitoring();

					Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, TouchScreenMonitoring_Tick(): " + ex.Message, ex);
				}

				if (this.isTouchScreenMonitoringRunning)
					StartTouchScreenMonitoring();
			}
		}
		#endregion                                

        #region ScannerScanButtonPressed()
        private TouchScreenButton ScannerScanButtonPressed(ref ScannerScanAreaSelection scanAreaSelection)
		{
			if (this.isTouchScreenAvailable)
			{
				lock (threadLocker)
				{
					try
					{
						Login();

						for (int i = 0; i <= exceptionIterations; i++)
						{
							try
							{
								HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/events?" + this.SessionId + "+poll");
								webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
								webRequest.Timeout = 5000;

								using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
								{
									using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
									{
										if (streamReader.Peek() >= 0)
										{
											string htmlBody = streamReader.ReadToEnd();

											if (htmlBody.IndexOf("OK") < 0 && htmlBody.IndexOf("ERROR 8") < 0)
											{
												if (htmlBody.Contains("Invalid"))
												{
													Login();
												}
												else
												{
													Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, ScannerScanButtonPressed(): " + Environment.NewLine + htmlBody + Environment.NewLine, null);
													throw new ScannersEx("Can't setup scanner!");
												}
											}
											else
											{
												string[] lines = htmlBody.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
												TouchScreenButton buttonPressed = WasScanButtonPressed(lines, ref scanAreaSelection);
												this.lastScanAreaSelection = GetLastScannerScanAreaSelection(lines);

												return buttonPressed;
											}
										}
										else
										{
											Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't get scanner response!", null);
											throw new ScannersEx("Can't get scanner response!");
										}
									}
								}
							}
							catch (ScannersEx ex)
							{
								throw ex;
							}
							catch (Exception ex)
							{
								if (ex.Message.ToLower().Contains("timed out") && i < exceptionIterations)
									Thread.Sleep(i * 100);
								else
								{
									Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, ScannerScanButtonPressed(): " + ex.Message, ex);
									throw new ScannersEx("BookEye exception while checking Bookeye4 UI buttons:" + " " + ex.Message, ScannersEx.AlertType.Error);
								}
							}
						}

						return TouchScreenButton.None;
					}
					finally
					{
						StartTimerPing();
						//Logout();
					}
				}
			}
			else
				return TouchScreenButton.None;
		}        
		#endregion

        #region SetTouchScreenButtons
        private void SetTouchScreenButtons(ScannerScanAreaSelection splittingValue)
        {
            if (isTouchScreenAvailable)
            {
                if (this.isHtmlTouchScreenApp)
                {
                    lock (threadLocker)
                    {
                        HttpWebRequest webRequest = null;
                        string htmlBody;

                        try
                        {

                            Login();

                            //with new html5 app set splitting buttons based on setting                                
                            //old Java app kept button pressed after pressing automatically 
                            if (splittingValue == ScannerScanAreaSelection.Both)
                            {
                                webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:SetTLButtonSelected");
                            }
                            else if (splittingValue == ScannerScanAreaSelection.Flat)
                            {
                                webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:SetTRButtonSelected");
                            }
                            else if (splittingValue == ScannerScanAreaSelection.Left)
                            {
                                webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:SetMLButtonSelected");
                            }
                            else if (splittingValue == ScannerScanAreaSelection.Right)
                            {
                                webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/sendcmd?" + this.SessionId + "+AppGUI+GUI:SetMRButtonSelected");
                            }

                            webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                            webRequest.Timeout = 5000;

                            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                            {
                                using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
                                {
                                    if (streamReader.Peek() >= 0)
                                    {
                                        htmlBody = streamReader.ReadToEnd();

                                        if (htmlBody.IndexOf("OK") < 0)
                                        {
                                            Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, SetTouchScreenButtons():" + Environment.NewLine + htmlBody + Environment.NewLine, null);
                                            throw new ScannersEx("Can't setup scanner's touchscreen!");
                                        }
                                        else
                                        {
                                            // everything is OK
                                            this.touchScreenButtonsIni = true;
                                        }
                                    }
                                    else
                                    {
                                        Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't get scanner response!", null);
                                        throw new ScannersEx("Can't get scanner response!");
                                    }
                                }
                            }
                        }

                        catch (ScannersEx ex)
                        {
                            throw ex;
                        }
                        catch (Exception ex)
                        {
                            Notifications.Instance.Notify(this, Notifications.Type.Error, "Bookeye4, SetTouchScreenButtons(): " + ex.Message, ex);
                            throw new ScannersEx("BookEye exception while setting UI:" + " " + ex.Message, ScannersEx.AlertType.Error);
                        }
                        finally
                        {
                            StartTimerPing();
                        }
                    }
                }
            }
        }
        #endregion

        #region ParseResponseBuffer()
        private void ParseResponseBuffer(string[] lines)
        {
            ScannerScanAreaSelection scanAreaSelection ;
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].Trim().ToLower() == "appgui:scanbutton" || lines[i].Trim().ToLower() == "appgui:footpedal")
                {
                    scanAreaSelection = this.lastScanAreaSelection;
                    if (ScanRequest != null)
                        ScanRequest(scanAreaSelection);                   
                }
                else if (lines[i].Trim().ToLower() == "appgui:scanpullslipbutton")
                {                   
                    scanAreaSelection = this.lastScanAreaSelection;
                    if (ScanPullslipRequest != null)
                        ScanPullslipRequest(scanAreaSelection);                    

                }
                else if (lines[i].Trim().ToLower() == "appgui:topleftbutton")
                {
                    scanAreaSelection = this.lastScanAreaSelection;
                }
                else if (lines[i].Trim().ToLower() == "appgui:toprightbutton")
                {
                    scanAreaSelection = this.lastScanAreaSelection;
                }
                else if (lines[i].Trim().ToLower() == "appgui:middleleftbutton")
                {
                    scanAreaSelection = this.lastScanAreaSelection;
                }
                else if (lines[i].Trim().ToLower() == "appgui:middlerightbutton")
                {
                    scanAreaSelection = this.lastScanAreaSelection;
                }                
                else if (lines[i].Trim().ToLower().Contains("appgui:version:") )   
                {
                    this.isHtmlTouchScreenApp = true;

                    string version = lines[i].Trim().ToLower();
                    int index = version.LastIndexOf("appgui:version:");
                    if (index >= 0)
                    {
                        index += 15;
                        version = version.Substring(index);
                        string[] wordChunks = version.Split(' ');
                        if (wordChunks.Length > 0)
                        {
                            version = wordChunks[0].Replace(".", "");
                            if (version.Length > 0)
                            {
                                this.htmlTouchScreenAppVersion = int.Parse(version);
                            }
                        }
                    }


                }
            }
            
        }
        #endregion

	
		#region WasScanButtonPressed()
		private TouchScreenButton WasScanButtonPressed(string[] lines, ref ScannerScanAreaSelection scanAreaSelection)
		{
			for (int i = lines.Length - 1; i >= 0; i--)
			{
				if (string.Compare(lines[i].Trim(), "scanner:ExternalScanRequest", true) == 0)
				{
					scanAreaSelection = ScannerScanAreaSelection.Flat;
					return TouchScreenButton.Scan;
				}                
                else if (string.Compare(lines[i].Trim(), 0, "scanner:version:", 0, 16 , true) == 0)
                {
                    //what command is this and why we set this flag to true
                    this.isHtmlTouchScreenApp = true;
                }
                else if (lines[i].Trim().ToLower() == "appgui:scanbutton" || lines[i].Trim().ToLower() == "appgui:footpedal")
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        string line = lines[j].Trim().ToLower();
                        
                        if (line.Contains("appgui:version:") )
                        {
                            this.isHtmlTouchScreenApp = true;

                            string version = line;
                            int index = version.LastIndexOf("appgui:version:");
                            if (index >= 0)
                            {
                                index += 15;
                                version = version.Substring(index);
                                string[] wordChunks = version.Split(' ');
                                if (wordChunks.Length > 0)
                                {
                                    version = wordChunks[0].Replace(".", "");
                                    if (version.Length > 0)
                                    {
                                        this.htmlTouchScreenAppVersion = int.Parse(version);
                                    }
                                }
                            }
                        }
                        else if (line == "appgui:topleftbutton")
                        {
                            scanAreaSelection = ScannerScanAreaSelection.Both;
                            return TouchScreenButton.Scan;
                        }
                        else if (line == "appgui:toprightbutton")
                        {
                            scanAreaSelection = ScannerScanAreaSelection.Flat;
                            return TouchScreenButton.Scan;
                        }
                        else if (line == "appgui:middleleftbutton")
                        {
                            scanAreaSelection = ScannerScanAreaSelection.Left;
                            return TouchScreenButton.Scan;
                        }
                        else if (line == "appgui:middlerightbutton")
                        {
                            scanAreaSelection = ScannerScanAreaSelection.Right;
                            return TouchScreenButton.Scan;
                        }
                    }

                    scanAreaSelection = this.lastScanAreaSelection;
                    return TouchScreenButton.Scan;
                }
                else if (lines[i].Trim().ToLower() == "appgui:scanpullslipbutton")
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        string line = lines[j].Trim().ToLower();
                        
                        if (line.Contains("appgui:version:") )
                        {
                            this.isHtmlTouchScreenApp = true;

                            string version = line;
                            int index = version.LastIndexOf("appgui:version:");
                            if (index >= 0)
                            {
                                index += 15;
                                version = version.Substring(index);
                                string[] wordChunks = version.Split(' ');
                                if (wordChunks.Length > 0)
                                {
                                    version = wordChunks[0].Replace(".", "");
                                    if (version.Length > 0)
                                    {
                                        this.htmlTouchScreenAppVersion = int.Parse(version);
                                    }
                                }
                            }
                        }
                        else if (line == "appgui:topleftbutton")
                        {
                            scanAreaSelection = ScannerScanAreaSelection.Both;
                            return TouchScreenButton.ScanPullslip;
                        }
                        else if (line == "appgui:toprightbutton")
                        {
                            scanAreaSelection = ScannerScanAreaSelection.Flat;
                            return TouchScreenButton.ScanPullslip;
                        }
                        else if (line == "appgui:middleleftbutton")
                        {
                            scanAreaSelection = ScannerScanAreaSelection.Left;
                            return TouchScreenButton.ScanPullslip;
                        }
                        else if (line == "appgui:middlerightbutton")
                        {
                            scanAreaSelection = ScannerScanAreaSelection.Right;
                            return TouchScreenButton.ScanPullslip;
                        }
                    }
                    scanAreaSelection = this.lastScanAreaSelection;
                    return TouchScreenButton.ScanPullslip;
                } 
			}

			return TouchScreenButton.None;
		}
		#endregion

		#region GetLastScannerScanAreaSelection()
		private ScannerScanAreaSelection GetLastScannerScanAreaSelection(string[] lines)
		{
			for (int i = lines.Length - 1; i >= 0; i--)
			{
				string line = lines[i].Trim().ToLower();

                if (line.Contains("appgui:version:"))
                {
                    this.isHtmlTouchScreenApp = true;

                    string version = line;
                    int index = version.LastIndexOf("appgui:version:");
                    if (index >= 0)
                    {
                        index += 15;
                        version = version.Substring(index);
                        string[] wordChunks = version.Split(' ');
                        if (wordChunks.Length > 0)
                        {
                            version = wordChunks[0].Replace(".", "");
                            if (version.Length > 0)
                            {
                                this.htmlTouchScreenAppVersion = int.Parse(version);
                            }
                        }
                    }
                }
                else if (line == "appgui:topleftbutton")
                    return ScannerScanAreaSelection.Both;
                else if (line == "appgui:toprightbutton")
                    return ScannerScanAreaSelection.Flat;
                else if (line == "appgui:middleleftbutton")
                    return ScannerScanAreaSelection.Left;
                else if (line == "appgui:middlerightbutton")
                    return ScannerScanAreaSelection.Right;
			}

			return this.lastScanAreaSelection;
		}
		#endregion
        

		#region RaisePreviewScanned()
		protected void RaisePreviewScanned(Bitmap preview)
		{
			if (PreviewScanned != null)
				PreviewScanned(preview);
		}
		#endregion

		#region RaiseProgressChanged()
		protected void RaiseProgressChanged(string description, float progress)
		{
			if(ProgressChanged != null)
				 ProgressChanged(description, progress);
		}
		#endregion

		#endregion

	}
}
