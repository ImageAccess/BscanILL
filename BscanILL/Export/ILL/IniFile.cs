using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BscanILL.Export.ILL
{
	/// <summary>
	/// Summary description for IniFile.
	/// </summary>
	public class IniFile
	{
		public string path;

		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section,
			string key,string val,string filePath);
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section,
			string key,string def, StringBuilder retVal,
			int size,string filePath);

		public IniFile(string INIPath)
		{
			path = INIPath;
		}

		public void IniWriteValue(string section,string key,string theValue)
		{
			WritePrivateProfileString(section, key, theValue, this.path);
		}
        
		public static void IniWriteValue(string section,string key,string keyValue, string filePath)
		{
			WritePrivateProfileString(section, key, keyValue, filePath);
		}

		public string IniReadValue(string section,string key)
		{
			StringBuilder temp = new StringBuilder(1024);

			int i = GetPrivateProfileString(section, key, "", temp, 1023, this.path);
			return temp.ToString();
		}

		public static string IniReadValue(string section,string key, string filePath)
		{
			StringBuilder temp = new StringBuilder(1024);

			int i = GetPrivateProfileString(section, key, "", temp, 1023, filePath);
			
			return temp.ToString();
		}

	}
}