using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Misc
{
	static class Io
	{
		
		#region SetFullControl()
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
				throw new Exception(string.Format("Can't set full access permissions to the file '{0}'!", file.FullName + " " + ex.Message), ex);
			}
		}
		#endregion

	}
}
