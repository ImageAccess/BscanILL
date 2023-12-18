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


		#region class LicenseFileRequest
		[Serializable]
		public class LicenseFileRequest
		{
			public string ComputerName;
			public string MacAddress;
			public string BscanILLVersion;
			public DateTime Date;
			public string Message;
			public string Institution;
			public string BscanILLSite;
			public string RequestorName;
			public string RequestorPhone;
			public string RequestorEmail;
			public string ScannerSn;

			public LicenseFileRequest()
			{
			}

			public LicenseFileRequest(string message, string institution, string bscanILLSite, string requestorName, string phone, string email, string scannerSn)
			{
				this.ComputerName = Environment.MachineName;
				this.MacAddress = BscanILL.Misc.Misc.GetMacAddress();
				this.BscanILLVersion = BscanILL.Misc.Misc.Version;
				this.Date = DateTime.Now;
				this.Message = message;
				this.Institution = institution;
				this.BscanILLSite = bscanILLSite;
				this.RequestorName = requestorName;
				this.RequestorPhone = phone;
				this.RequestorEmail = email;
				this.ScannerSn = scannerSn;
			}
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

			public bool Rapido = false;    //   /r param in license file

			public LicenseOptions()
			{
			}
		}
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		static BscanILL.SETTINGS.Settings		_settings { get { return BscanILL.SETTINGS.Settings.Instance; } }

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

				Notifications.Instance.Notify(null, Notifications.Type.Error, "Unlicensed scanner '" + sn + "'", null);
				Notifications.Instance.SendDeveloperReportIfNecessary();

				DeleteOcrFiles();
                DeleteGsFiles();
                DeleteAbbyyOcrFiles();

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


        #region ExtractLicenseFileInfo()
        /// <summary>
		/// Format output string with license file info.
		/// </summary>
		/// <param name="serialNumber"></param>
		/// <returns></returns>
        public static string ExtractLicenseFileInfo(string serialNumber)
		{
            string licInfo = string.Empty;
			try
			{
				LicenseOptions licenseOptions = GetLicenseOptions(serialNumber);

				if (licenseOptions != null)
                {
                    licInfo = "Serial Number: " + licenseOptions.SerialNumber + "\r\n";

                    licInfo += "Expiration Date: ";
                    if(licenseOptions.ExpirationDate == null)
                    {
                        licInfo += "Never Expires\r\n";
                    }
                    else
                    {
                        licInfo += licenseOptions.ExpirationDate.Value.Month.ToString() + "/" + licenseOptions.ExpirationDate.Value.Day.ToString() + "/"
                                   + licenseOptions.ExpirationDate.Value.Year.ToString() + "\r\n";
                    }

                    licInfo += "OCR Enabled: " + (licenseOptions.Ocr ? "YES" : "NO") + "\r\n";
                    licInfo += "Audio Enabled: " + (licenseOptions.Audio ? "YES" : "NO") + "\r\n";
                    licInfo += "Additional Scanner Enabled: " + (licenseOptions.AdfScanner ? "YES" : "NO") + "\r\n";
					licInfo += "Rapido Export Enabled: " + (licenseOptions.Rapido ? "YES" : "NO") + "\r\n";
				}                
			}
			catch (Exception) { }

            return licInfo;
		}
		#endregion

        #region EncryptOCRBin()
        public static void EncryptOCRBin()
        {
            GenerateOCRFiles();
        }
        #endregion

        #region InstallNecessaryComponentsBasedOnLicensing()
        public static bool InstallNecessaryComponentsBasedOnLicensing(string serialNumber)
		{
            bool ocrFileJustInstalled = false;
			try
			{
				LicenseOptions licenseOptions = GetLicenseOptions(serialNumber);

                if (licenseOptions != null)
                {
                    if (licenseOptions.Ocr)
                    {
//IRIS
#if IRIS_ENGINE
                        DeployOcrFiles( true, true );
#else
                        ocrFileJustInstalled = DeployAbbyyOcrFiles();   //for Abbyy to deploy local license file                        
                        DeleteOcrFiles();   //delete IRIS when Abbyy is used
#endif
                        DeployGsFiles();
                    }
                    else
                    {
                        DeleteOcrFiles();
                        DeleteGsFiles();
                        DeleteAbbyyOcrFiles();
                    }
                }
                else
                {
                    DeleteOcrFiles();
                    DeleteGsFiles();
                    DeleteAbbyyOcrFiles();
                }
                return ocrFileJustInstalled;
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				throw ex;
			}            
		}
		#endregion

		#region GetLicenseFile()
		public static FileInfo GetLicenseFile(string serialNumber)
		{
			return new System.IO.FileInfo(BscanILL.SETTINGS.Settings.Instance.General.LicenseDir + @"\" + serialNumber + ".txt");
		}
		#endregion

		#region DownloadLicenseFile()
		public static FileInfo DownloadLicenseFile(string sn)
		{
			FileInfo tempFile = null;
			string localDir = null;

			try
			{
				string uri = _settings.General.ServerLicenseFilesUri;

				if (uri.Length > 0 && uri[uri.Length - 1] != '/')
					uri += "/";

				uri += sn + ".txt";

				localDir = _settings.General.LicenseDir + @"\";
				tempFile = new FileInfo(_settings.General.LicenseDir + @"\" + sn + ".txt_temp");

				tempFile.Directory.Create();

				using (FileStream fs = new FileStream(tempFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					//show preview
					System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
					webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
					webRequest.Timeout = 20000;

					using (System.Net.HttpWebResponse webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse())
					{
						if (webResponse.ContentType.ToLower().StartsWith("text/plain"))
						{
							byte[] buffer = new byte[256];
							int count;

							using (Stream stream = webResponse.GetResponseStream())
							{
								while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
								{
									fs.Write(buffer, 0, count);
								}
							}
						}
					}
				}

				LicenseOptions licenseOptions = GetLicenseOptions(tempFile, sn);

				if (licenseOptions != null && (licenseOptions.ExpirationDate == null || licenseOptions.ExpirationDate > DateTime.Now))
				{
					FileInfo licenseFile = new FileInfo(_settings.General.LicenseDir + @"\" + sn + ".txt"); 

					if(licenseFile.Exists)
						licenseFile.Delete();

					tempFile.CopyTo(licenseFile.FullName, true);
					BscanILL.Misc.Licensing.InstallNecessaryComponentsBasedOnLicensing(sn);

					licenseFile.Refresh();
					return licenseFile;
				}
				else
				{
					tempFile.Delete();
					throw new Exception("License file was downloaded successfully, but it is not valid for your system.");
				}
			}
			catch (Exception ex)
			{
				if (sn != null && ex.Message.Contains("(404) Not Found"))
					throw new IllException("There is no license file associated with scanner '" + sn + "'!");
				else
					throw ex;
			}
			finally
			{
				try
				{
					if (tempFile != null && tempFile.Exists)
						tempFile.Delete();
				}
				catch { }
			}
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
                    _settings.Licensing.OcrEnabled = licenseOptions.Ocr;
                    _settings.Licensing.AudioEnabled = (licenseOptions.Ocr && licenseOptions.Audio);
                    _settings.Licensing.AddidionalScannerEnabled = licenseOptions.AdfScanner;
					_settings.Licensing.RapidoEnabled = licenseOptions.Rapido;

					if (_settings.Licensing.OcrEnabled)
                    {
//IRIS
#if IRIS_ENGINE
                        System.IO.DirectoryInfo binDir = new System.IO.DirectoryInfo(_settings.General.IrisBinDir);
                        System.IO.DirectoryInfo resourcesDir = new System.IO.DirectoryInfo(_settings.General.IrisResourcesDir);
                                                
                        //check if msvcr120.dll file present - needed for new IdrsWrapper3.dll
                        //when updating new version - we may need to update IRIS folder even if it exists because of lack of msvcr120.dll file
                        binDir.Refresh();                        
                        if(binDir.Exists == true)
                        if (binDir.GetFiles("msvcr120.dll").Length == 0)
                        {                                                
                              //unzip new data file to add the dll
                              DeployOcrFiles( true, false );                       
                              binDir.Refresh();
                        }

                        if (binDir.Exists == false || resourcesDir.Exists == false || (binDir.GetFiles().Length == 0) || resourcesDir.GetFiles().Length == 0)
                            throw new IllException("OCR functionality is enabled, but required libraries are not provided. Please contact DLSG to resolve this problem.");
#endif
                        System.IO.DirectoryInfo gsDir = new System.IO.DirectoryInfo(_settings.General.GsDir);

                        if (gsDir.Exists == false || (gsDir.GetFiles().Length == 0))
                        {
                            //when updating BscanILL with newer version 3.3.3.1 and higher, we can have IRIS folder installed but no GS folder when launching first time after update at this point of code
                            DeployGsFiles();
                            gsDir.Refresh();
                        }

                        if (gsDir.Exists == false || (gsDir.GetFiles().Length == 0))
                            throw new IllException("Required libraries for PDF import are not provided. Please contact DLSG to resolve this problem.");

                    }

                    return true;
                }
            }
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception)
			{
				return false;
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
							catch (Exception)
							{ }
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
           try
            {
                if (String.Compare(line, "Blank", true) != 0)
                {			
			// mach-100-24b54eb6779d, freeflow partner, , /P /I /O /B /E /M /T /G /R, 6/17/2011, ,7b565f649be295795e20865cb49b69dd13852011			
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

						licenseOptions.Rapido = (options.ToLower().Contains("/r"));  // Rapido export support
					}

					return licenseOptions;
				}
			}
		}
                return null;
            }
            catch (Exception)  
            {
                return null;
            }			
		}
		#endregion

        #region GenerateOCRFiles()
        // drop files (in our case IRIS files or GS files) that needs to be zipped and encypted into C:\\Temp500\in\ folder. This method will zip the files
        // into C:\\TEmp500\data12632.zip and then this file will be encrypted into  C:\\Temp500\\outdata12632.data            
        // this is for BscanILL installation purposes. You will need to process files from 'iris\bin' folder and 'iris\resources' folder
        // or 'gs' folder (GhostScript for pdf import purposes) separately with this method...

        // IRIS bin folder zip  must use file name  @"\Data\data12632.data"
        // IRIS resource folder zip  must use file name  @"\Data\data12633.data"
        // 'gs'zip  must use file name  @"\Data\data12634.data"
        // Abbyy's local license file's zip  must use file name  @"\Data\data12635.data"

        //to call this function, search for "CheckLicenseFileThread" method and uncomment line that calls "EncryptOCRBin()" method. Run BscanILL, go to Settings dialog -> Scanners -> press check license

        private static void GenerateOCRFiles()
        {
            DirectoryInfo sourceDir = new DirectoryInfo("C:\\Temp500\\in");            
            FileInfo[] binFiles = sourceDir.GetFiles("*.*", SearchOption.AllDirectories );


            DirectoryInfo inputBinDir = new DirectoryInfo("C:\\Temp500");
           // FileInfo inputBinDirFiles = new FileInfo(inputBinDir + @"\data12632.zip");  //for IRIS bin folder
           // FileInfo inputBinDirFiles = new FileInfo(inputBinDir + @"\data12633.zip");  //for IRIS resource folder
           // FileInfo inputBinDirFiles = new FileInfo(inputBinDir + @"\data12634.zip");    //for Ghost Script folder
            FileInfo inputBinDirFiles = new FileInfo(inputBinDir + @"\data12635.zip");    //for Abbyy's local license file

            DirectoryInfo outputEncryptedDir = new DirectoryInfo("C:\\Temp500\\out");
            if (outputEncryptedDir.Exists == false)
            {
                outputEncryptedDir.Create();
            }

            FileInfo binZip = null;            
            string password = "panskr46784*(&)*(#*^$#";
            string passwordSalt = "^*&*^845$#&*^$*#*$#JKDH F45546K";

            try
            {
                BscanILL.Misc.Zipping.Zip(binFiles, inputBinDirFiles);

                if (inputBinDirFiles.Exists == false)
                    throw new IllException("Appropriate OCR file doesn't exist!");

                //binZip = new FileInfo(Path.Combine(outputEncryptedDir.FullName, @"data12632.data"));  //for IRIS bin folder
                //binZip = new FileInfo(Path.Combine(outputEncryptedDir.FullName, @"data12633.data"));  //for IRIS resource folder
                //binZip = new FileInfo(Path.Combine(outputEncryptedDir.FullName, @"data12634.data"));    //for Ghost Script folder             
                binZip = new FileInfo(Path.Combine(outputEncryptedDir.FullName, @"data12635.data"));    //for Abbyy's local license file

                if (binZip.Exists)
                    binZip.Delete();


                BscanILL.Misc.FileEncryption.Encrypt(inputBinDirFiles, binZip, password, passwordSalt);                
                
            }
            finally
            {

            }

        }
        #endregion

		#region DeployOcrFiles()

        //for IRIS
		private static void DeployOcrFiles( bool deployBin , bool deployResourceDir )
		{
            if (deployBin)
            {
                DirectoryInfo binDir = new DirectoryInfo(_settings.General.IrisBinDir);                

                if (binDir.Exists == false || binDir.GetFiles("*.*").Length != 209 )
                {
                        binDir.Create();                                        
                        FileInfo encodedBinFile = new FileInfo(BscanILL.Misc.Misc.StartupPath + @"\Data\data12632.data");                        

                        if (encodedBinFile.Exists == false)
                            throw new IllException("Appropriate OCR file doesn't exist!");

                        //BscanILL.Misc.Io.SetFullControl(encodedBinFile);
                        //BscanILL.Misc.Io.SetFullControl(encodedResourcesFile); 

                        FileInfo binZip = null;
                        string password = "panskr46784*(&)*(#*^$#";
                        string passwordSalt = "^*&*^845$#&*^$*#*$#JKDH F45546K";

                        try
                        {
                            binZip = new FileInfo(Path.Combine(binDir.FullName, @"data12632.zip"));                            

                            if (binZip.Exists)
                                binZip.Delete();

                            BscanILL.Misc.FileEncryption.Decrypt(encodedBinFile, binZip, password, passwordSalt);                            
                            BscanILL.Misc.Zipping.UnZip(binZip, binDir);                            
                        }
                        finally
                        {
                            try
                            {
                                if (binZip != null)
                                {
                                    binZip.Refresh();
                                    binZip.Delete();
                                }                                
                            }
                            catch { }
                        }                    
                }
            }

            if (deployResourceDir)
            {                
                DirectoryInfo resourcesDir = new DirectoryInfo(_settings.General.IrisResourcesDir);

                if (resourcesDir.Exists == false || resourcesDir.GetFiles("*.*").Length < 1)
                {                    
                        resourcesDir.Create();                                           
                        FileInfo encodedResourcesFile = new FileInfo(BscanILL.Misc.Misc.StartupPath + @"\Data\data12633.data");

                        if (encodedResourcesFile.Exists == false)
                            throw new IllException("Appropriate OCR resource file doesn't exist!");

                        //BscanILL.Misc.Io.SetFullControl(encodedBinFile);
                        //BscanILL.Misc.Io.SetFullControl(encodedResourcesFile); 
                        
                        FileInfo resourcesZip = null;
                        string password = "panskr46784*(&)*(#*^$#";
                        string passwordSalt = "^*&*^845$#&*^$*#*$#JKDH F45546K";

                        try
                        {                            
                            resourcesZip = new FileInfo(Path.Combine(resourcesDir.FullName, @"data12633.zip"));

                            if (resourcesZip.Exists)
                                resourcesZip.Delete();
                            
                            BscanILL.Misc.FileEncryption.Decrypt(encodedResourcesFile, resourcesZip, password, passwordSalt);                            
                            BscanILL.Misc.Zipping.UnZip(resourcesZip, resourcesDir);
                        }
                        finally
                        {
                            try
                            {
                                if (resourcesZip != null)
                                {
                                    resourcesZip.Refresh();
                                    resourcesZip.Delete();
                                }
                            }
                            catch { }
                        }                    
                }
            }
		}

        //for Abbyy - deploys Abbyy's local license file
        private static bool DeployAbbyyOcrFiles()
        {
            bool ocrFileJustInstalled = false ;
            
            DirectoryInfo licFileDir = new DirectoryInfo(_settings.General.OCRLicenseDir);

            licFileDir.Create();
                        
            string p3 = Path.Combine(licFileDir.FullName, @"SWAO-1020-0003-2569-3339-7310.ABBYY.LocalLicense");

            FileInfo licFile = new FileInfo(p3);
            if (!licFile.Exists)            
            {                
                    FileInfo encodedLicFile = new FileInfo(BscanILL.Misc.Misc.StartupPath + @"\Data\data12635.data");

                    if (encodedLicFile.Exists == false)
                        throw new IllException("Appropriate OCR file doesn't exist!");

                    //BscanILL.Misc.Io.SetFullControl(encodedBinFile);
                    //BscanILL.Misc.Io.SetFullControl(encodedResourcesFile); 

                    FileInfo licZip = null;
                    string password = "panskr46784*(&)*(#*^$#";
                    string passwordSalt = "^*&*^845$#&*^$*#*$#JKDH F45546K";

                    try
                    {
                        licZip = new FileInfo(Path.Combine(licFileDir.FullName, @"data12635.zip"));

                        if (licZip.Exists)
                            licZip.Delete();

                        BscanILL.Misc.FileEncryption.Decrypt(encodedLicFile, licZip, password, passwordSalt);
                        BscanILL.Misc.Zipping.UnZip(licZip, licFileDir);
                        ocrFileJustInstalled = true;                        
                    }
                    finally
                    {
                        try
                        {
                            if (licZip != null)
                            {
                                licZip.Refresh();
                                licZip.Delete();
                            }
                        }
                        catch { }
                    }
                
            }
            return ocrFileJustInstalled;
        }
		#endregion

        #region DeployGsFiles()
        private static void DeployGsFiles()
        {
            DirectoryInfo gsDir = new DirectoryInfo(_settings.General.GsDir);                        

            if (gsDir.Exists == false || gsDir.GetFiles("*.*").Length < 422)
            {
                gsDir.Create();

                // if(resourcesDir.GetFiles("eng.ytr").Length < 1)
                {
                    FileInfo encodedGsFile = new FileInfo(BscanILL.Misc.Misc.StartupPath + @"\Data\data12634.data");

                    if (encodedGsFile.Exists == false)
                        throw new IllException("Appropriate PDF import dll file doesn't exist!");

                    //BscanILL.Misc.Io.SetFullControl(encodedBinFile);
                    //BscanILL.Misc.Io.SetFullControl(encodedResourcesFile); 

                    FileInfo gsZip = null;
                    string password = "panskr46784*(&)*(#*^$#";
                    string passwordSalt = "^*&*^845$#&*^$*#*$#JKDH F45546K";

                    try
                    {
                        gsZip = new FileInfo(Path.Combine(gsDir.FullName, @"data12634.zip"));

                        if (gsZip.Exists)
                            gsZip.Delete();

                        BscanILL.Misc.FileEncryption.Decrypt(encodedGsFile, gsZip, password, passwordSalt);
                        BscanILL.Misc.Zipping.UnZip(gsZip, gsDir);
                    }
                    finally
                    {
                        try
                        {
                            if (gsZip != null)
                            {
                                gsZip.Refresh();
                                gsZip.Delete();
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        #endregion

		#region DeleteOcrFiles()
		private static void DeleteOcrFiles()
		{
			try
			{
				DirectoryInfo binDir = new DirectoryInfo(_settings.General.IrisBinDir);
				DirectoryInfo resourcesDir = new DirectoryInfo(_settings.General.IrisResourcesDir);

				if (binDir.Exists)
					binDir.Delete(true);

				if (resourcesDir.Exists)
					resourcesDir.Delete(true);
			}
			catch { }
		}
		#endregion

		#region DeleteAbbyyOcrFiles()
		private static void DeleteAbbyyOcrFiles()
		{
			try
			{
				DirectoryInfo licDir = new DirectoryInfo(_settings.General.OCRLicenseDir);

                if (licDir.Exists)
                    licDir.Delete(true);
			}
			catch { }
		}
		#endregion        

        #region DeleteGsFiles()
        private static void DeleteGsFiles()
        {
            try
            {
                DirectoryInfo gsDir = new DirectoryInfo(_settings.General.GsDir);

                if (gsDir.Exists)
                    gsDir.Delete(true);
            }
            catch { }
        }
        #endregion

		#region RequestLicenseFile()
		private static LicenseResult RequestLicenseFile(Dispatcher dispatcher, string sn, string errorMessage)
		{
			try
			{
				LicenseResult result = (LicenseResult)dispatcher.Invoke((Func<LicenseResult>)delegate()
				{
					try
					{
                        //hardcode to use Keith's http delivery method for license requests...
                        //BscanILL.UI.Settings.Dialogs.LicenseRequestDlg dlg = new BscanILL.UI.Settings.Dialogs.LicenseRequestDlg(errorMessage, sn, _settings.Export.Email.EmailDeliveryType);
                        BscanILL.UI.Settings.Dialogs.LicenseRequestDlg dlg = new BscanILL.UI.Settings.Dialogs.LicenseRequestDlg(errorMessage, sn, SETTINGS.Settings.ExportClass.EmailClass.EmailMethodBasedOn.HTTP);

						if (dlg.ShowDialog() == true)
							return LicenseResult.LicenseRequestSentToDlsg;
						else
							return LicenseResult.UserCanceled;
					}
					catch (Exception) { return LicenseResult.UserCanceled; }
				});

				return result;
			}
			catch (Exception) { return LicenseResult.UserCanceled; }
		}
		#endregion
	
		#endregion
	
	}
}
