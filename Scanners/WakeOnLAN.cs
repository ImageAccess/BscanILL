using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Net.Cache;

namespace Scanners
{
	class WakeOnLAN
	{

		#region class WOLClass
		private class WOLClass : System.Net.Sockets.UdpClient
		{
			public WOLClass()
			//				: base()
			{
			}

			//this is needed to send broadcast packet
			public void SetClientToBrodcastMode()
			{
				if (this.Active)
					this.Client.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.Broadcast, 0);
			}
		}
		#endregion


		// PUBLIC METHODS
		#region public methods

		#region WakeS2NOnLan()
		//MAC_ADDRESS should  look like '013FA049EF88'
		public static bool WakeS2NOnLan(string macAddress, string ipAddress, Scanners.PingDeviceReceiver receiver)
		{
			WOLClass client = new WOLClass();
			int attmpts = 0;

			client.Connect(new IPAddress(0xffffffff),  //255.255.255.255  i.e broadcast
				0x2fff); // port=12287 let's use this one 

			client.SetClientToBrodcastMode();
			//set sending bites
			int counter = 0;

			//buffer to be send
			byte[] bytes = new byte[1024];   // more than enough :-)

			//first 6 bytes should be 0xFF
			for (int y = 0; y < 6; y++)
				bytes[counter++] = 0xFF;

			//now repeat MAC 16 times
			for (int y = 0; y < 16; y++)
			{
				int i = 0;

				for (int z = 0; z < 6; z++)
				{
					bytes[counter++] = byte.Parse(macAddress.Substring(i, 2), NumberStyles.HexNumber);
					i += 2;
				}
			}

			//now send wake up packet
			int reterned_value = client.Send(bytes, 1024);

			//wait for scanner to be fully functional
			DateTime start = DateTime.Now;
			bool scannerOn = false;

			while ((scannerOn == false) && (DateTime.Now.Subtract(start).TotalSeconds < 180))
			{
				try
				{
					attmpts++;
					if (receiver != null)						
                        receiver.PingDeviceProgressChanged(string.Format("Scanner IP:{1} Wake On LAN Attempt {0}", attmpts, ipAddress));
					scannerOn = CheckScanner(ipAddress);

					if (scannerOn == false)
						Thread.Sleep(new TimeSpan(0, 0, 5));
				}
#if DEBUG
				catch (Exception ex)
				{
					Console.WriteLine(DateTime.Now.Subtract(start) + " WakeOnLan: " + ex.Message);
					Thread.Sleep(new TimeSpan(0, 0, 5));
				}
#else
				catch (Exception)
				{
					Thread.Sleep(new TimeSpan(0, 0, 5));
				}
#endif
			}

			return scannerOn;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region static CheckScanner()
		private static bool CheckScanner(string ipAddress)
		{
			//scanning
			string htmlBody = "";
			bool connected = false;

			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://" + ipAddress + @"/cgi/s2ninfo");
			webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			webRequest.Timeout = 3000;
			webRequest.ReadWriteTimeout = 3000;

			using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
			{
				using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.ASCII))
				{
					if (reader.Peek() >= 0)
					{
						htmlBody = reader.ReadToEnd();

						if (htmlBody.IndexOf("OK") > 0)
							connected = true;
					}
				}
			}

			return connected;
		}
		#endregion

		#endregion

	}
}
