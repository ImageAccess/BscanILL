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

	public class TestS2N : IDisposable
	{
		static TestS2N		instance = new TestS2N();
		static object		threadLocker = new object();
		string				sessionId = null;

		Version httpVersion = HttpVersion.Version10;
		bool keepAlive = true;



		#region constructor
		private TestS2N()
		{
			this.SessionId = Properties.ScannersSettings.Default.IpScannerLastSessionId;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public string				Ip					{ get { return Scanners.Settings.Instance.S2NScanner.Ip; } }

		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		#region SessionId
		string SessionId
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

		#endregion


		//PUBLIC METHODS
		#region public  methods

		#region GetInstance()
		public static TestS2N GetInstance()
		{
			if (instance == null)
			{
				instance = new TestS2N();
			}

			return instance;
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			Logout();
		}
		#endregion

		#region GetSettings()
		public string GetSettings()
		{
			string htmlBody;

			System.Net.ServicePointManager.Expect100Continue = false;

			HttpWebRequest webRequestLogin = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/s2nopen");
			webRequestLogin.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			webRequestLogin.Timeout = 5000;
			webRequestLogin.KeepAlive = keepAlive;
			webRequestLogin.ProtocolVersion = httpVersion;

			//login
			using (HttpWebResponse webResponse = (HttpWebResponse)webRequestLogin.GetResponse())
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
						}
						else
							throw new ScannersEx("Can't open scanner session:" + " " + line);
					}
					else
						throw new Exception("Can't connect to the scanner!");
				}
			}

			//set settings
			try
			{
				Thread.Sleep(200);
				HttpWebRequest webRequestSettings = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/get?" + this.SessionId + "+settings");
				webRequestSettings.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
				webRequestSettings.Timeout = 5000;

				using (HttpWebResponse webResponse = (HttpWebResponse)webRequestSettings.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
					{
						if (streamReader.Peek() >= 0)
						{
							htmlBody = streamReader.ReadToEnd();
						}
						else
							throw new Exception("Can't get scanner response!");
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Settings Exception: " + GetErrorMessage(ex), ex);
			}

			Logout();
			return htmlBody;
		}
		#endregion

		#region SetSettings()
		public void SetSettings(string settings)
		{
			string htmlBody;

			System.Net.ServicePointManager.Expect100Continue = false;

			HttpWebRequest webRequestLogin = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/s2nopen");
			webRequestLogin.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			webRequestLogin.Timeout = 5000;
			webRequestLogin.KeepAlive = keepAlive;
			webRequestLogin.ProtocolVersion = httpVersion;

			//login
			using (HttpWebResponse webResponse = (HttpWebResponse)webRequestLogin.GetResponse())
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
						}
						else
							throw new ScannersEx("Can't open scanner session:" + " " + line);
					}
					else
						throw new Exception("Can't connect to the scanner!");
				}
			}

			//set settings
			try
			{
				Thread.Sleep(200);
				HttpWebRequest webRequestSettings = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/set?" + this.SessionId + settings);
				webRequestSettings.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
				webRequestSettings.Timeout = 5000;

				using (HttpWebResponse webResponse = (HttpWebResponse)webRequestSettings.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
					{
						if (streamReader.Peek() >= 0)
						{
							htmlBody = streamReader.ReadToEnd();

							if (htmlBody.IndexOf("OK") < 0)
								throw new Exception("Can't setup scanner! " + htmlBody);
						}
						else
							throw new Exception("Can't get scanner response!");
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Settings Exception: " + GetErrorMessage(ex), ex);
			}

			Logout();
		}
		#endregion
	
		#region Scan()
		/// <summary>
		/// saves image into 'filePath' or throws exception
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filePath"></param>
		public void Scan(string settings, string filePath)
		{
			string htmlBody;

			System.Net.ServicePointManager.Expect100Continue = false;

			HttpWebRequest webRequestLogin = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/s2nopen");
			webRequestLogin.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			webRequestLogin.Timeout = 5000;
			webRequestLogin.KeepAlive = keepAlive;
			webRequestLogin.ProtocolVersion = httpVersion;

			//login
			using (HttpWebResponse webResponse = (HttpWebResponse)webRequestLogin.GetResponse())
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
						}
						else
							throw new ScannersEx("Can't open scanner session:" + " " + line);
					}
					else
						throw new Exception("Can't connect to the scanner!");
				}
			}

			//set settings
			if (settings != null)
			{
				try
				{
					Thread.Sleep(200);
					HttpWebRequest webRequestSettings = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/set?" + this.SessionId + settings);
					webRequestSettings.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
					webRequestSettings.Timeout = 5000;

					using (HttpWebResponse webResponse = (HttpWebResponse)webRequestSettings.GetResponse())
					{
						using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
						{
							if (streamReader.Peek() >= 0)
							{
								htmlBody = streamReader.ReadToEnd();

								if (htmlBody.IndexOf("OK") < 0)
									throw new Exception("Can't setup scanner! " + htmlBody);
							}
							else
								throw new Exception("Can't get scanner response!");
						}
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Settings Exception: " + GetErrorMessage(ex), ex);
				}
			}

			//scanning
			try
			{
				HttpWebRequest webRequestScan = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/scan?" + this.SessionId);
				webRequestScan.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
				webRequestScan.Timeout = 30000;
				webRequestScan.KeepAlive = keepAlive;
				webRequestScan.ProtocolVersion = httpVersion;

				using (HttpWebResponse webResponse = (HttpWebResponse)webRequestScan.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
					{
						htmlBody = streamReader.ReadToEnd();

						if (htmlBody.IndexOf("OK") < 0)
							throw new ScannersEx("Invalid parameters returned while sending scan request! " + htmlBody);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Scan Exception: " + GetErrorMessage(ex), ex);
			}

			Thread.Sleep(500);

			//save image
			HttpWebRequest webRequestGetImage = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/image?" + this.SessionId);
			webRequestGetImage.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			webRequestGetImage.Timeout = 40000;
			webRequestGetImage.KeepAlive = keepAlive;
			webRequestGetImage.ProtocolVersion = httpVersion;

			using (HttpWebResponse webResponse = (HttpWebResponse)webRequestGetImage.GetResponse())
			{
				using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
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

			Logout();
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Logout()
		private void Logout()
		{
			//logout
			try
			{
				if (this.SessionId != null)
				{
					HttpWebRequest webRequestLogout = (HttpWebRequest)System.Net.WebRequest.Create(@"http://" + this.Ip + @"/cgi/s2nclose?" + this.SessionId);
					webRequestLogout.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
					webRequestLogout.Timeout = 5000;
					webRequestLogout.KeepAlive = keepAlive;
					webRequestLogout.ProtocolVersion = httpVersion;

					using (HttpWebResponse webResponse = (HttpWebResponse)webRequestLogout.GetResponse())
					{
						using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
						{
							string line = reader.ReadLine();

							if (line.IndexOf("OK") < 0)
								throw new Exception(line);
						}
					}
				}
			}
			finally
			{
				this.SessionId = null;
			}
		}
		#endregion

		#region GetErrorMessage()
		private string GetErrorMessage(Exception ex)
		{
			string error = ex.Message;

			while ((ex = ex.InnerException) != null)
				error += Environment.NewLine + ex.Message;

			return error;
		}
		#endregion

		#endregion

	}
}
