using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTPClientConnector;
using System.Security;
using System.Xml.Serialization;

namespace BscanILL.Export.FTP
{

	#region class FtpLogin
	[Serializable]
	public class FtpLogin
	{
		string			name = "";
		FTPType			ftpType = FTPType.Ftp;
		string			server;
		string			directory;
		ushort			port;
		string			username;
		byte[]			passwordBuffer = new byte[0];

		public FtpLogin()
		{
		}

		public FtpLogin(string profileName, string server, string directory, ushort port, FTPType ftpType, string username, SecureString password)
		{
			this.Name = profileName;
			this.Server = server;
			this.Directory = directory;
			this.Port = port;
			this.FtpType = ftpType;
			this.Username = username;
			this.Password = password;
		}

		public string		Name { get { return this.name; } set { this.name = value; } }
		public FTPType		FtpType { get { return this.ftpType; } set { this.ftpType = value; } }
		public string		Server { get { return this.server; } set { this.server = value; } }
		public string		Directory { get { return this.directory; } set { this.directory = value; } }
		public ushort		Port { get { return this.port; } set { this.port = value; } }
		public string		Username { get { return this.username; } set { this.username = value; } }
		public byte[]		PasswordBuffer { get { return this.passwordBuffer; } set { this.passwordBuffer = value; } }

		#region FtpTypeName
		public string FtpTypeName
		{
			get
			{
				switch (this.FtpType)
				{
					case FTPType.Ftp: return "Regular FTP";
					case FTPType.FtpSSH: return "SFTP/SCP, over SSH";
					case FTPType.FtpTLS: return "FTPS, external SSL/TLS";
					default: return "FTPS, internal SSL";
				}
			}
		}
		#endregion

		#region Password
		[XmlIgnore]
		public SecureString Password
		{
			get
			{
				try { return BscanILL.Misc.DataProtector.GetSecureString(this.PasswordBuffer); }
				catch (Exception) { return new SecureString(); }
			}
			set { this.PasswordBuffer = BscanILL.Misc.DataProtector.GetArray(value); }
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			return (this.Name != null) ? this.Name : "Manual...";
		}
		#endregion
	}
	#endregion

}
