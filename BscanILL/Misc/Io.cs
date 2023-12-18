using System;
using System.IO ;
using System.Threading ;
using System.Security.Cryptography ;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Security.Principal;
using System.Security.AccessControl;


namespace BscanILL.Misc
{
	/// <summary>
	/// Summary description for Io.
	/// </summary>
	public class Io
	{
		[DllImport("Kernel32.dll")] private unsafe static extern bool GetDiskFreeSpaceEx(
			String name,  /*LPCTSTR*/
			ulong* lpFreeBytesAvailable, /*PULARGE_INTEGER*/
			ulong* lpTotalNumberOfBytes, /*PULARGE_INTEGER*/
			ulong* lpTotalNumberOfFreeBytes /*PULARGE_INTEGER*/
			); 

		[DllImport("Kernel32.dll")]
		private unsafe static extern uint GetLastError();


		#region GetFileExtension()
		/// <summary>
		/// returns extension in format .XXX
		/// </summary>
		/// <param name="fileFormat"></param>
		/// <returns></returns>
		public static string GetFileExtension(BscanILL.Scan.FileFormat fileFormat)
		{
			switch (fileFormat)
			{
				case BscanILL.Scan.FileFormat.Pdf: return ".pdf";
				case BscanILL.Scan.FileFormat.SPdf: return ".pdf";
				case BscanILL.Scan.FileFormat.Audio: return ".mp3";
				case BscanILL.Scan.FileFormat.Png: return ".png";
				case BscanILL.Scan.FileFormat.Tiff: return ".tif";
				case BscanILL.Scan.FileFormat.Text: return ".rtf";
				default: return ".jpg";
			}
		}

		public static string GetFileExtension(Scanners.FileFormat fileFormat)
		{
			switch (fileFormat)
			{
				case Scanners.FileFormat.Png: return ".png";
				case Scanners.FileFormat.Tiff: return ".tif";
				default: return ".jpg";
			}
		}
		#endregion

		#region GetHash()
		public static byte[] GetHash(string word)
		{
			HashAlgorithm	sha = HashAlgorithm.Create("SHA256") ;
			byte[]			hash = Str2Byte(word);
			hash = sha.ComputeHash(hash) ;

			return hash ;
		}
		#endregion

		#region Str2Byte()
		static private byte[] Str2Byte(string cStr)
		{
			//TOCharArray()
			byte[]		iByte = new byte[cStr.Length] ;
			
			for(int i = 0; i < cStr.Length; i++)
				iByte[i] = Convert.ToByte(cStr[i]) ;
			
			return iByte ;
		}
		#endregion

		#region DirSize()
		public static ulong DirSize(DirectoryInfo dir) 
		{    
			ulong	size = 0;    

			FileInfo[]		files = dir.GetFiles();
			foreach (FileInfo file in files) 
				size += (ulong) file.Length;    

			DirectoryInfo[]		subdirs = dir.GetDirectories();
			foreach (DirectoryInfo subdir in subdirs) 
				size += DirSize(subdir);   

			return(size);  
		}
		#endregion

		#region DiskFreeSpace()
		unsafe public static ulong DiskFreeSpace(DirectoryInfo drive)
		{
			if(drive != null)
			{
				ulong		lpFreeBytesAvailable = 0 ;
				ulong		lpTotalNumberOfBytes = 0 ;
				ulong		lpTotalNumberOfFreeBytes = 0 ;

				if (GetDiskFreeSpaceEx((String)drive.Root.FullName, &lpFreeBytesAvailable, &lpTotalNumberOfBytes, &lpTotalNumberOfFreeBytes))
					return lpFreeBytesAvailable;
				else
					throw new IllException(string.Format("Can't obtain disk space on drive '{0}'.", drive.Root.FullName));
			}
			else
				throw new IllException("Can't obtain disk space, disk drive is not specified.") ;
		}
		#endregion

		#region DiskSpace()
		unsafe public static bool DiskSpace(string drive, ref ulong freeBytesAvailable, ref ulong totalNumberOfBytes, ref ulong totalNumberOfFreeBytes)
		{
			if(drive != null)
			{
				fixed(ulong* lpFree = &freeBytesAvailable)
					fixed( ulong* lpTotal = &totalNumberOfBytes)
					fixed (ulong* lpTotalFree = &totalNumberOfFreeBytes)
					{
						bool ok = GetDiskFreeSpaceEx(drive, lpFree, lpTotal, lpTotalFree);

						/*if (ok == false)
						{
#if DEBUG
							uint error = GetLastError();

							error = error;
#endif
						}*/

						return ok;
					}
			}
				
			return false ;
		}
		#endregion

		#region DirectoryCopy()
		public static bool DirectoryCopy(string source, string destination, bool rewriteExisting)
		{
			try
			{
				Directory.CreateDirectory(destination) ;

				string[]	subdirs = Directory.GetDirectories(source) ;
				foreach(string subdir in subdirs)
					DirectoryCopy(source + @"\" + subdir, destination + @"\" + subdir, rewriteExisting) ;

				string[]	files = Directory.GetFiles(source) ;
				foreach(string file in files)
					File.Copy(file, destination + @"\" + Path.GetFileName(file), rewriteExisting) ;

				return true ;
			}
			catch (DirectoryNotFoundException ex) 
			{
				throw new Exception("The specified path is invalid, such as being on an unmapped drive. " + ex.Message);
			}
			catch (UnauthorizedAccessException ex) 
			{
				throw new Exception("The caller does not have the required permission. " + ex.Message);
			}
			catch (ArgumentNullException ex) 
			{
				throw new Exception("Path is a null reference. " + ex.Message);
			}
			catch (System.Security.SecurityException ex) 
			{
				throw new Exception("The caller does not have the required permission. " + ex.Message);
			}
			catch (ArgumentException ex) 
			{
				throw new Exception("Path is an empty string, contains only white spaces, or contains invalid characters. " + ex.Message);    
			}
			catch (System.IO.IOException ex) 
			{
				throw new Exception("An attempt was made to move a directory to a different volume, or destination directory name already exists. " + ex.Message); 
			}
		}
		#endregion  

		#region CanRead()
		/*public static bool CanRead(string dir)
		{
			System.Security.AccessControl.DirectorySecurity dirSecurity = Directory.GetAccessControl(dir);
			System.Security.AccessControl.AuthorizationRuleCollection accessRules = dirSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

			foreach (System.Security.AccessControl.FileSystemAccessRule accessRule in accessRules)
				if ((accessRule.AccessControlType == System.Security.AccessControl.AccessControlType.Allow) && (accessRule.IdentityReference.Value.ToLower() == Environment.UserDomainName.ToLower() + @"\" + Environment.UserName))
					return ((accessRule.FileSystemRights & System.Security.AccessControl.FileSystemRights.Read) == System.Security.AccessControl.FileSystemRights.Read);

			return false;
		}*/
		#endregion

		#region CanWrite()
		/*public static bool CanWrite(string dir)
		{
			System.Security.AccessControl.DirectorySecurity dirSecurity = Directory.GetAccessControl(dir);
			System.Security.AccessControl.AuthorizationRuleCollection accessRules = dirSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

			foreach (System.Security.AccessControl.FileSystemAccessRule accessRule in accessRules)
				if ((accessRule.AccessControlType == System.Security.AccessControl.AccessControlType.Allow) && (accessRule.IdentityReference.Value.ToLower() == Environment.UserDomainName.ToLower() + @"\" + Environment.UserName))
					return ((accessRule.FileSystemRights & System.Security.AccessControl.FileSystemRights.Write) == System.Security.AccessControl.FileSystemRights.Write);

			return false;
		}*/
		#endregion

		#region CanModify()
		/*public static bool CanModify(string dir)
		{
			System.Security.AccessControl.DirectorySecurity dirSecurity = Directory.GetAccessControl(dir);
			System.Security.AccessControl.AuthorizationRuleCollection accessRules = dirSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

			foreach (System.Security.AccessControl.FileSystemAccessRule accessRule in accessRules)
			{
				if ((accessRule.AccessControlType == System.Security.AccessControl.AccessControlType.Allow) && (accessRule.IdentityReference.Value.ToLower() == "everyone"))
					return ((accessRule.FileSystemRights & System.Security.AccessControl.FileSystemRights.Modify) == System.Security.AccessControl.FileSystemRights.Modify);

				if ((accessRule.AccessControlType == System.Security.AccessControl.AccessControlType.Allow) && (accessRule.IdentityReference.Value.ToLower() == Environment.UserDomainName.ToLower() + @"\" + Environment.UserName))
					return ((accessRule.FileSystemRights & System.Security.AccessControl.FileSystemRights.Modify) == System.Security.AccessControl.FileSystemRights.Modify);
			}

			return false;
		}*/
		#endregion

		#region SetFullControl()
		public static void SetFullControl(DirectoryInfo dir)
		{
			try
			{
				dir.Refresh();
				
				if (dir.Exists)
				{
					DirectorySecurity security = dir.GetAccessControl();
					SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
					security.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
					dir.SetAccessControl(security);
				}
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(null, Notifications.Type.Error, "Io, SetFullControl(): " + ex.Message, ex);
				throw new IllException(string.Format("Can't set full access permissions to the directory '{0}'!", dir.FullName + " " + ex.Message));
			}
		}

		public static void SetFullControl(FileInfo file)
		{
			try
			{
				file.Refresh();
				
				if (file.Exists)
				{
					FileSecurity fileSecurity = file.GetAccessControl();
					SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
					fileSecurity.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
					file.SetAccessControl(fileSecurity);
				}
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(null, Notifications.Type.Error, "Io, SetFullControl(): " + ex.Message, ex);
				throw new IllException(string.Format("Can't set full access permissions to the file '{0}'!", file.FullName + " " + ex.Message));
			}
		}
		#endregion

		#region AverageFileSize()
		public static ulong AverageFileSize(List<FileInfo> files)
		{
			ulong size = 0;

			foreach (FileInfo file in files)
			{
				file.Refresh();
				
				if (file.Exists)
					size += (ulong)file.Length;
			}

			if (files.Count > 0)
				return (ulong) (size / (ulong) files.Count);
			else
				return 0;
		}
		#endregion

		#region Compress()
		public static MemoryStream Compress(string str)
		{
			MemoryStream memoryStream = new MemoryStream();
			System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
			byte[] buffer = encoding.GetBytes(str);
			GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true);

			gZipStream.Write(buffer, 0, buffer.Length);
			gZipStream.Close();
			memoryStream.Seek(0, SeekOrigin.Begin);

			return memoryStream;
		}
		#endregion

		#region Decompress()
		/*public static BscanILL.StatisticsDs Decompress(FileInfo file)
		{
			ASCIIEncoding encoding = new ASCIIEncoding();
			FileStream fileStream = file.OpenRead();
			GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Decompress, false);
			byte[] buffer = new byte[4096];
			int bytesCounter;
			string fileContent = "";

			do
			{
				bytesCounter = gZipStream.Read(buffer, 0, 4096);

				if (bytesCounter > 0)
				{
					fileContent += encoding.GetString(buffer, 0, bytesCounter);
				}
			} while (bytesCounter > 0);

			gZipStream.Close();

			BscanILL.StatisticsDs statisticsDs = new BscanILL.StatisticsDs();
			using (StringReader stringReader = new StringReader(fileContent))
			{
				statisticsDs.ReadXml(stringReader);
			}

			return statisticsDs;
		}*/
		#endregion

		#region SaveTiffG4()
		public static void SaveTiffG4(string file, System.Drawing.Bitmap bitmap)
		{
			System.Drawing.Imaging.ImageCodecInfo		codecInfo = Scanners.Scanner.TiffImageCodecInfo;
			System.Drawing.Imaging.EncoderParameters	encoderParams = Scanners.Scanner.TiffBwEncoderParams;
	
			bitmap.Save(file, codecInfo, encoderParams);
		}
		#endregion

		#region DirectoryExists()
		public static bool DirectoryExists(string path)
		{
			Func<bool> func = () => Directory.Exists(path);
			System.Threading.Tasks.Task<bool> task = new System.Threading.Tasks.Task<bool>(func);
			
			task.Start();

			if (task.Wait(1000))
				return task.Result;
			else
				return false;		// Didn't get an answer back in time be pessimistic and assume it didn't exist
		}
		#endregion

	}

}
