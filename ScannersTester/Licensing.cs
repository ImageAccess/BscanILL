using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows;

namespace BscanILL.Misc
{
	public class Licensing
	{
		[DllImport(@"\FileUtilities.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FileUtilCopyEx([MarshalAs(UnmanagedType.LPStr)] String Path, int Offset, int Index);


		#region LicenseResult
		public enum LicenseResult
		{
			OK,
			UserCanceled,
			LicenseRequestSentToDlsg
		}
		#endregion

		#region LicenseOptions()
		class LicenseOptions
		{
			public string SerialNumber = null;
			public string Product = null;
			public string Versions = null;

			public DateTime? StartDate = null;
			public DateTime? ExpirationDate = null;
			
			public bool Ocr = false;
			public bool Audio = false;
			public bool AdfScanner = false;

			public LicenseOptions()
			{
			}
		}
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		//static BscanILL.SETTINGS.Settings		settings { get { return BscanILL.SETTINGS.Settings.Instance; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region CheckLicensing()
		/// <summary>
		/// For BscanILL application, throws exception, when OCR is enabled, but not installed
		/// </summary>
		/// <param name="serialNumber"></param>
		/// <returns></returns>
		public static bool CheckLicensing(string serialNumber)
		{
			if (serialNumber == null || IsLicensingKosher(serialNumber) == false)
			{
				string sn = (serialNumber != null) ? serialNumber : "";

				return false;
			}

			return true;
		}
		#endregion

		#region CheckLicenseFile()
		/// <summary>
		/// For BscanILL Setup, check if license file exists and is valid.
		/// </summary>
		/// <param name="serialNumber"></param>
		/// <returns></returns>
		public static bool CheckLicenseFile(string serialNumber)
		{
			try
			{
				LicenseOptions licenseOptions = GetLicenseOptions(serialNumber);

				if (licenseOptions != null && (licenseOptions.ExpirationDate == null || licenseOptions.ExpirationDate > DateTime.Now))
					return true;
			}
			catch (Exception) { }

			return false;
		}
		#endregion

		#region GetLicenseFile()
		public static FileInfo GetLicenseFile(string serialNumber)
		{
			return new System.IO.FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\License" + @"\" + serialNumber + ".txt");
		}
		#endregion
		
		#endregion


		// PRIVATE METHODS
		#region private methods

		#region IsLicensingKosher()
		private static bool IsLicensingKosher(string serialNumber)
		{
			try
			{
				LicenseOptions licenseOptions = GetLicenseOptions(serialNumber);

				if ((licenseOptions != null) && (licenseOptions.ExpirationDate == null || licenseOptions.ExpirationDate > DateTime.Now))
				{
					/*settings.Licensing.OcrEnabled = true;
					settings.Licensing.AudioEnabled = (licenseOptions.Ocr && licenseOptions.Audio);
					settings.Licensing.AddidionalScannerEnabled = licenseOptions.AdfScanner;

					if (settings.Licensing.OcrEnabled)
					{
						System.IO.DirectoryInfo binDir = new System.IO.DirectoryInfo(settings.General.IrisBinDir);
						System.IO.DirectoryInfo resourcesDir = new System.IO.DirectoryInfo(settings.General.IrisResourcesDir);

						if (binDir.Exists == false || resourcesDir.Exists == false || (binDir.GetFiles().Length == 0) || resourcesDir.GetFiles().Length == 0)
							throw new Exception("OCR functionality is enabled, but required libraries are not provided. Please contact DLSG to resolve this problem.");
					}*/

					return true;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return false;
		}
		#endregion

		#region GetLicenseOptions()
		private static LicenseOptions GetLicenseOptions(string serialNumber)
		{
			return GetLicenseOptions(GetLicenseFile(serialNumber), serialNumber);
		}

		private static LicenseOptions GetLicenseOptions(FileInfo licenseFile, string serialNumber)
		{
			try
			{				
				if (licenseFile.Exists)
				{
					using (System.IO.StreamReader sr = licenseFile.OpenText())
					{
						while (sr.Peek() >= 0)
						{
							string line = sr.ReadLine();

							try
							{
								LicenseOptions licenseOptions = GetLicenseOptions(line, serialNumber);

								if (licenseOptions != null && (licenseOptions.ExpirationDate == null || licenseOptions.ExpirationDate > DateTime.Now))
									return licenseOptions;
							}
							catch (Exception ex)
							{ 
								throw ex; 
							}
						}
					}
				}
			}
			catch (Exception)
			{
				return null;
			}

			return null;
		}
		#endregion
	
		#region GetLicenseOptions()
		private static LicenseOptions GetLicenseOptions(string line, string sn)
		{
			// mach-100-24b54eb6779d, freeflow partner, , /P /I /O /B /E /M /T /G, 6/17/2011, ,7b565f649be295795e20865cb49b69dd13852011
			int			decodedHash = FileUtilCopyEx(line, 348975634, 97);
			string[]	sections = line.Split(',');

			// for now allow MAC mismatch in serial number check...
			if (decodedHash == 348975634 % 101 && sections.Length == 7)
			//if (answer == 348975634 % 101 && sections.Length == 7 && serialNumber == sections[0])
			{                 //012345678901234567890123456
				string serialNumber = sections[0].Trim();
				string product = sections[1].Trim();
				string versions = sections[2].Trim();
				string options = sections[3].Trim();
				string dateFrom = sections[4].Trim();
				string dateTo = sections[5].Trim();
				string hash = sections[6].Trim();

				if (product.ToLower() == "bscan ill" && serialNumber.ToLower() == sn.ToLower())
				{
					LicenseOptions licenseOptions = new LicenseOptions();
					
					licenseOptions.SerialNumber = serialNumber;
					licenseOptions.Product = product;
					licenseOptions.Versions = versions;

					if (dateFrom.Length > 4)
					{
						DateTime date = Convert.ToDateTime(dateFrom, new System.Globalization.CultureInfo("en-US")); //end date exists

						licenseOptions.StartDate = date;
					}

					if (dateTo.Length > 4)
					{
						DateTime date = Convert.ToDateTime(dateTo, new System.Globalization.CultureInfo("en-US")); //end date exists

						licenseOptions.ExpirationDate = date;
					}

					if (options != null)
					{
						licenseOptions.Ocr = options.ToLower().Contains("/o");
						licenseOptions.Audio = (options.ToLower().Contains("/a"));
						licenseOptions.AdfScanner = (options.ToLower().Contains("/s"));
					}

					return licenseOptions;
				}
			}

			return null;
		}
		#endregion


		#endregion
	
	}
}
