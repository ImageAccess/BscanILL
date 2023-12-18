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
	public class Bookeye4 : S2nScanerBase, IS2NScanner
	{
		static Bookeye4		instance = null;


		#region constructor
		private Bookeye4(string ip)
			: base(ip)
		{
		}
		#endregion


		#region destructor
		~Bookeye4()
		{
			Dispose();
		}
		#endregion


		//PUBLIC METHODS
		#region public  methods

		#region GetInstance()
		public static Bookeye4 GetInstance(string ip)
		{	
			if (Bookeye4.instance == null)
				Bookeye4.instance = new Bookeye4(ip);

			return Bookeye4.instance;
		}
		#endregion

		#region Dispose()
		public override void Dispose()
		{
			try
			{
				base.Dispose();
			}
			catch { }
			finally
			{
				Bookeye4.instance = null;
			}
		}
		#endregion

		#region Scan()
		/// <summary>
		/// saves image into 'filePath' or throws exception
		/// </summary>
		/// <param name="filePath"></param>
		public void Scan(string filePath)
		{
#if DEBUG
			Console.WriteLine("ScannerBookeye4, Scan initiated at " + DateTime.Now.ToString());
#endif
	
			lock (threadLocker)
			{
				this.Settings.ScanMode.Value = Scanners.S2N.ScanMode.Direct;
				
				HttpWebRequest webRequest = null;
				string htmlBody;

				try
				{					
					RaiseProgressChanged("Scanning...", 0);

					Login();
					SetDevice(this);
					Thread.Sleep(100);

					//scanning
					try
					{
						webRequest = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/scan?" + this.SessionId);
						webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
						webRequest.Timeout = 30000;

						using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
						{
							using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
							{
								htmlBody = streamReader.ReadToEnd();
							}
						}
					}
					catch (Exception ex)
					{
						Notifications.Instance.Notify(null, Notifications.Type.Error, "Bookeye4, Scan(): Exception while sending scan request. " + ex.Message, ex);
						throw new ScannersEx("There is not connection to the scanner!", ScannersEx.AlertType.Warning);
					}

					if (htmlBody.IndexOf("OK") < 0)
					{
						if (htmlBody.ToLower().Contains("error 7") || htmlBody.ToLower().Contains("no focus"))
						{
							throw new ScannersEx(htmlBody);
						}
						else if (htmlBody.ToLower().Contains("crop failed"))
						{
							throw new ScannersEx(htmlBody);
						}
						else
						{
							Notifications.Instance.Notify(null, Notifications.Type.Error, "Bookeye4, Scan(): Invalid parameters returned while sending scan request. " + htmlBody, null);
							throw new ScannersEx("Invalid parameters returned while sending scan request!");
						}
					}

					DownloadPreview();

					//save image
					DownloadImage(filePath);
				}
				catch (ScannersEx ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(null, Notifications.Type.Error, "Bookeye4, Scan(), Scanner exception while scanning: " + ex.Message, ex);
					throw new ScannersEx("BookEye exception while scanning:" + " " + ex.Message, ScannersEx.AlertType.Error);
				}
				finally
				{
					StartTimerPing();
				}
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods
		
		#region SendCommandToScanner()
		protected override string SendCommandToScanner(Bookeye4.Command command, string parameters, int timeout)
		{
			string commandStr = (command == Command.Set) ? "set?" : "get?";

			try
			{
				Login();

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
							return response;
						}
						else
							throw new ScannersEx("Can't get scanner response!");
					}
				}
			}
			finally
			{
				StartTimerPing();
			}
		}
		#endregion
	
		#region SetDevice()
		private void SetDevice(object sender)
		{
			lock (threadLocker)
			{
				try
				{
//#if !DEBUG
					//if (this.Settings.SettingsChanged)
//#endif
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
								Notifications.Instance.Notify(sender, Notifications.Type.Error, "Bookeye4 SetDevice('" + settings + "'): " + Environment.NewLine + htmlBody, null);
								throw new ScannersEx(string.Format("Error while setting scanner: {0}\n\n{1}", htmlBody, settings), ScannersEx.AlertType.Error);
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
					if (ex.Message.Contains("has timed out"))
						Notifications.Instance.Notify(sender, Notifications.Type.Warning, "Can't connect to the scanner!" + " "  + ex.Message, ex);
					else
						Notifications.Instance.Notify(sender, Notifications.Type.Error, "Bookeye4 SetDevice('" + this.Settings + "'): " + ex.Message, ex);

					throw new ScannersEx("Scanner doesn't respond." + Environment.NewLine + ex.Message, ScannersEx.AlertType.Error);
				}
			}
		}
		#endregion

		#endregion

	}
}
