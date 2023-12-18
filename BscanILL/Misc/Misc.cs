using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Management;

namespace BscanILL.Misc
{
	public delegate void VoidHnd();
	public delegate void IllImageHnd(BscanILL.Hierarchy.IllImage illImage);
    public delegate void ArticleHnd(BscanILL.Hierarchy.Article article);
    public delegate void BatchImageHnd(BscanILL.Hierarchy.SessionBatch batch, BscanILL.Hierarchy.Article article , BscanILL.Hierarchy.IllImage illImage);
    public delegate void BatchPageHnd(BscanILL.Hierarchy.SessionBatch batch, BscanILL.Hierarchy.Article article, BscanILL.Hierarchy.IllPage illPage);    

	class Misc
	{
		public static string StartupPath	{ get { return new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName; } }
		public static string Version		{ get { return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString(); } }

		#region GetMacAddress()
		public static string GetMacAddress()
		{
			try
			{
				ObjectQuery theQuery = new ObjectQuery("SELECT MACAddress FROM Win32_NetworkAdapterConfiguration where IPEnabled = TRUE");

				ManagementObjectSearcher theSearcher = new ManagementObjectSearcher(theQuery);
				ManagementObjectCollection theCollectionOfResults = theSearcher.Get();

				foreach (ManagementObject theCurrentObject in theCollectionOfResults)
				{
					try
					{
						string macAddress = theCurrentObject["MACAddress"].ToString();
						return macAddress;
					}
					catch { }
				}
			}
			catch { }

			return "";
		}
		#endregion

		#region GetErrorMessage()
		/// <summary>
		/// returns error message lines including inner exceptions devided by new line characters, most significant error on the top
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		public static string GetErrorMessage(Exception ex)
		{
			string message = ex.Message;

			while ((ex = ex.InnerException) != null)
			{
				message += Environment.NewLine + ex.Message;
			}

			return message;
		}
		#endregion

	}
}
