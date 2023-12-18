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
using System.Timers;
using System.Net.Cache;
using System.Threading;



namespace Scanners.S2N
{
	public class ScannerS2N : S2nScanerBase, IS2NScanner
	{
		static ScannerS2N	instance = null;

		System.Timers.Timer timerSaveMode = null;


		#region constructor
		private ScannerS2N(string ip)
			: base(ip)
		{
			if (Scanners.Settings.Instance.General.SaveModeTimeoutInMins > 0)
			{
				timerSaveMode = new System.Timers.Timer(Scanners.Settings.Instance.General.SaveModeTimeoutInMins * 60000);
				timerSaveMode.Elapsed += new ElapsedEventHandler(TimerSafeMode_Elapsed);
				timerSaveMode.AutoReset = false;
				timerSaveMode.Start();
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public bool					CanTurnOffAutoFocus { get { return this.Settings.Autofocus.IsDefined; } }
		public bool					CanTurnOffLights	{ get { return this.Settings.Light.IsDefined; } }
		public bool					CanSetDocMode		{ get { return this.Settings.DocMode.IsDefined; } }
		
		public bool					IsLightOn			
		{
			get { return (this.Settings != null) ? (this.Settings.Light.IsDefined == false || this.Settings.Light.Value == Scanners.S2N.LightSwitch.On) : true; }
			set 
			{
				if (this.Settings != null && this.Settings.Light.IsDefined)
					this.Settings.Light.Value = (value) ? Scanners.S2N.LightSwitch.On : Scanners.S2N.LightSwitch.Off;
			}
		}

		#endregion


		//PUBLIC METHODS
		#region public  methods

		#region GetInstance()
		public static ScannerS2N GetInstance(string ip)
		{
			if (ScannerS2N.instance == null)
				ScannerS2N.instance = new ScannerS2N(ip);

			return instance;
		}
		#endregion

		#region Dispose()
		public override void Dispose()
		{
			try
			{
				base.Dispose();

				StopSaveModeTimer();
				
				lock (threadLocker)
					this.timerSaveMode = null;
			}
			finally
			{
				ScannerS2N.instance = null;
			}
		}
		#endregion
	
		#region Scan()
		/// <summary>
		/// saves image into 'filePath' or throws exception
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filePath"></param>
		public void Scan(string filePath)
		{
#if DEBUG
			Console.WriteLine("Scan initiated at " + DateTime.Now.ToString());
#endif
	
			lock (threadLocker)
			{
				StopSaveModeTimer();

				try
				{
					int delay = TurnTheLightsOn();

					if (delay > 0)
					{
						for (int i = 0; i < delay; i++)
						{
							float progress = i / (float)delay;
							RaiseProgressChanged(string.Format("Warming up Lights, {0:0}%.", progress * 100), progress); ;

							Thread.Sleep(1000);
						}
					}

					//ChangeSettingColorToGrayscaleIfNeeded(ref settings);
					RaiseProgressChanged("Scanning...", 0);

					Login();
					Thread.Sleep(200);
					this.Settings.ScanMode.Value = Scanners.S2N.ScanMode.Direct;
					SetDevice(this);
					Thread.Sleep(200);

					//scanning
					string htmlBody = SendScanCommand();

					if (htmlBody.IndexOf("OK") < 0)
					{
						Notifications.Instance.Notify(null, Notifications.Type.Error, "ScannerS2N, Scan(): Invalid parameters returned while sending scan request. " + htmlBody, null);
						throw new ScannersEx("Invalid parameters returned while sending scan request!");
					} 
					
					try
					{
						DownloadPreview();
					}
					catch (Exception ex)
					{
						Notifications.Instance.Notify(null, Notifications.Type.Error, "ScannerS2N, Scan(), Get preview Exception:" + ex.Message, ex);
					}

					//Thread.Sleep(2000);

					//save image
					DownloadImage(filePath);
				}
				catch (ScannersEx ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(null, Notifications.Type.Error, "ScannerS2N, Scan(), Scanner exception while scanning: " + ex.Message, ex);
					throw new ScannersEx("BookEye exception while scanning:" + " " + ex.Message, ScannersEx.AlertType.Error);
				}
				finally
				{
					StartTimerPing();
					StartSaveModeTimer();
				}
			}
		}
		#endregion

		#region ScanWait()
		/// <summary>
		/// returns null if timeout occured 
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		public string ScanWait(string filePath)
		{
#if DEBUG
			Console.WriteLine("ScannerS2N, Scan wait initiated at " + DateTime.Now.ToString());
#endif

			lock (threadLocker)
			{
				StopSaveModeTimer();

				try
				{
					//ChangeSettingColorToGrayscaleIfNeeded(ref settings);

					Login();
					this.Settings.ScanMode.Value = Scanners.S2N.ScanMode.Wait;
					Thread.Sleep(200);
					SetDevice(this);
					Thread.Sleep(200);

					//scanning
					string htmlBody = SendScanCommand();

					if (htmlBody.ToLower().IndexOf("error 7") >= 0)
					{
					}
					else if (htmlBody.IndexOf("OK") < 0 && htmlBody.IndexOf("timeout") < 0)
					{
						Notifications.Instance.Notify(null, Notifications.Type.Error, "ScannerS2N, ScanWait(): Invalid parameters returned while sending scan request. " + htmlBody, null);
						throw new ScannersEx("Invalid parameters returned while sending scan request!");
					}

					if (htmlBody.IndexOf("OK") >= 0)
					{
						try
						{
							DownloadPreview();
						}
						catch (Exception ex)
						{
							Notifications.Instance.Notify(null, Notifications.Type.Error, "ScannerS2N, DownloadPreview(), Get preview Exception:" + ex.Message, ex);
						}

						//save image
						DownloadImage(filePath);
						return filePath;
					}
					else
						return null;
				}
				catch (ScannersEx ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(null, Notifications.Type.Error, "Scanner exception while scanning:" + " " + ex.Message, ex);
					throw new ScannersEx("BookEye exception while scanning:" + " " + ex.Message, ScannersEx.AlertType.Error);
				}
				finally
				{
					StartTimerPing();
					StartSaveModeTimer();
				}
			}
		}
		#endregion

		#region StartSaveModeTimer()
		public void StartSaveModeTimer()
		{
			lock (threadLocker)
			{
				if (timerSaveMode != null && this.IsDisposed == false)
				{
					timerSaveMode.Stop();
					timerSaveMode.Start();
				}
			}
		}
		#endregion

		#region StopSaveModeTimer()
		public void StopSaveModeTimer()
		{
			lock (threadLocker)
			{
				if (timerSaveMode != null)
					timerSaveMode.Stop();
			}
		}
		#endregion

		#region IsBookeye2()
		/*public static bool IsBookeye2(Scanners.S2nType s2nType)
		{
			switch (s2nType)
			{
				//Bookeye 2
				case Scanners.S2nType.BE2_SCL_N2:
				case Scanners.S2nType.BE2_SGS_N2:
				case Scanners.S2nType.BE2_SCL_R1:
				case Scanners.S2nType.BE2_SCL_R2:
				case Scanners.S2nType.BE2_SGS_R2:
				case Scanners.S2nType.BE2_SGS_R1:
				case Scanners.S2nType.BE2_SCL_R1_PLUS:
				case Scanners.S2nType.BE2_SGS_R1_PLUS:
				case Scanners.S2nType.BE2_SCL_R2_PLUS:
				case Scanners.S2nType.BE2_SGS_R2_PLUS:
				case Scanners.S2nType.BE2_CSL_N2_PLUS:
				case Scanners.S2nType.BE2_SGS_N2_PLUS:
				case Scanners.S2nType.BE2_CGS_N2:
				case Scanners.S2nType.BE2_SGS_N3:
				case Scanners.S2nType.BE2_SCL_N2_R:
					return true;
				default:
					return false;
			}
		}*/
		#endregion

		#region IsBookeye2N2()
		/*public static bool IsBookeye2N2(Scanners.S2nType s2nType)
		{
			switch (s2nType)
			{
				//Bookeye 2
				case Scanners.S2nType.BE2_SCL_N2:
				case Scanners.S2nType.BE2_SGS_N2:
				case Scanners.S2nType.BE2_CSL_N2_PLUS:
				case Scanners.S2nType.BE2_SGS_N2_PLUS:
				case Scanners.S2nType.BE2_CGS_N2:
				case Scanners.S2nType.BE2_SCL_N2_R:
					return true;
				default:
					return false;
			}
		}*/
		#endregion

		#region SetDevice()
		public void SetDevice(object sender)
		{
			lock (threadLocker)
			{
				try
				{
					if (this.Settings.SettingsChanged)
					{
						if (this.Settings.ColorMode.Value != ColorMode.Lineart && this.Settings.ColorMode.Value != ColorMode.Photo)
							this.Settings.TiffCompression.Value = Scanners.S2N.TiffCompression.None;

						string settings = this.Settings.ToString();

						if (settings.Length > 0)
						{
							//scanning
							string htmlBody = SendCommandToScanner(Command.Set, settings, 5000);

							if (htmlBody.IndexOf("OK") < 0)
							{
								Notifications.Instance.Notify(sender, Notifications.Type.Error, "ScannerS2N SetDevice('" + settings + "'): " + Environment.NewLine + htmlBody, null);
								throw new ScannersEx(string.Format("Error while setting scanner: {0}\n\n{1}", htmlBody, settings), ScannersEx.AlertType.Error);
							}
							else
							{
								if (htmlBody.Contains("light: on"))
									this.IsLightOn = true;
								else if (htmlBody.Contains("light: off"))
									this.IsLightOn = false;
							}

							this.Settings.SettingsChanged = false;
						}
					}
				}
				catch (ScannersEx ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "ScannerS2N, Scan(): Can't setup scanner! " + ex.Message, ex);
					throw new ScannersEx("Scanner doesn't respond." + Environment.NewLine + ex.Message, ScannersEx.AlertType.Error);
				}
			}
		}
		#endregion
	
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region SendCommandToScanner()
		protected override string SendCommandToScanner(ScannerS2N.Command command, string parameters, int timeout)
		{
			lock (threadLocker)
			{
				string commandStr = (command == Command.Set) ? "set?" : "get?";

				try
				{
					Login();

					Thread.Sleep(250);
					HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/" + commandStr + this.SessionId + parameters);
					webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
					webRequest.Timeout = timeout;

					using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
					{
						using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
						{
							if (reader.Peek() >= 0)
							{
								string response = reader.ReadToEnd();

								if (response.Contains("light:"))
								{
									if (response.Contains("light: on"))
										this.IsLightOn = true;
									else if (response.Contains("light: off"))
										this.IsLightOn = false;
								}

								return response;
							}
							else
								throw new ScannersEx("Can't get scanner response!" );
						}
					}
				}
				catch(Exception ex)
				{
					throw ex;
				}
				finally
				{
					StartTimerPing();

					if (parameters.IndexOf("+light:on") >= 0)
						StartSaveModeTimer();
				}
			}
		}
		#endregion

		#region TimerSafeMode_Elapsed()
		void TimerSafeMode_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				if (this.IsDisposed == false)
				{
					string htmlBody;

					lock (threadLocker)
					{
						htmlBody = SendCommandToScanner(ScannerS2N.Command.Set, "+light:off", 5000);
					}

					if (htmlBody.IndexOf("OK") < 0)
						Notifications.Instance.Notify(this, Notifications.Type.Warning, "Warning: Scanner is not turned on!" + Environment.NewLine + htmlBody, null);
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.ToLower().Contains("timeout"))
					Notifications.Instance.Notify(this, Notifications.Type.Warning, "Warning: Scanner is not turned on!" + " " + ex.Message, ex);
			}
		}
		#endregion

		#region TurnTheLightsOn()
		/// <summary>
		/// Turns light on if applicable and returns number of seconds needed for the bulbs to warm up.
		/// </summary>
		/// <returns></returns>
		private int TurnTheLightsOn()
		{
			if (this.CanTurnOffLights && this.IsLightOn == false)
			{
				lock (threadLocker)
				{
					try
					{
						//start = DateTime.Now;
						string result = SendCommandToScanner(ScannerS2N.Command.Set, "+light:on", 5000);
						//span = DateTime.Now.Subtract(start);

						if (result.ToLower().Trim().StartsWith("light: on"))
						{
							if (this.deviceInfo.ScannerModel.ScannerSubGroup == MODELS.ScannerSubGroup.BE2_N2)
								return 10;
							else if (this.deviceInfo.ScannerModel.ScannerSubGroup == MODELS.ScannerSubGroup.BE2_N3)
								return 5;
							else
								return 0;
						}
					}
					catch { }
				}
			}

			return 0;
		}
		#endregion
	
		#endregion

	}
}
