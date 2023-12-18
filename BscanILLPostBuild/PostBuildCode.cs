using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace BscanILLPostBuild
{
	[System.ComponentModel.RunInstaller(true)]
	public class PostBuildCode : System.Configuration.Install.Installer
	{
        string irisBin = @"C:\ProgramData\DLSG\BscanILL\IRIS\bin" ; //System.Reflection.Assembly.GetExecutingAssembly().Location;

		// PUBLIC METHODS
		#region public methods

		public override void Install(System.Collections.IDictionary stateSaver)
		{
			base.Install(stateSaver);

			string curPath = GetPath();
			stateSaver.Add("previousPath", curPath);
			string newPath = AddPath(curPath, MyPath());

			if (curPath != newPath)
			{
				stateSaver.Add("changedPath", true);
				SetPath(newPath);
			}
			else
				stateSaver.Add("changedPath", false);
		}

		public override void Uninstall(System.Collections.IDictionary savedState)
		{
			base.Uninstall(savedState);

			if ((bool)savedState["changedPath"])
				SetPath(RemovePath(GetPath(), MyPath()));
		}

		public override void Rollback(System.Collections.IDictionary savedState)
		{
			base.Rollback(savedState);

			if ((bool)savedState["changedPath"])
				SetPath((string)savedState["previousPath"]);
		}

		#endregion


		// PRIVATE METHODS
		#region private methods

		private string MyPath()
        {
            /*string myFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string myPath = System.IO.Path.GetDirectoryName(myFile);
            return myPath;*/

			return irisBin;
        }

        private static RegistryKey GetPathRegKey(bool writable)
        {
            // for the user-specific path...
            //return Registry.CurrentUser.OpenSubKey("Environment", writable);

            // for the system-wide path...
            return Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", writable);
        }

        private static void SetPath(string value)
        {
            using (RegistryKey reg = GetPathRegKey(true))
            {
                reg.SetValue("Path", value, RegistryValueKind.ExpandString);
            }
        }

        private static string GetPath()
        {
            using (RegistryKey reg = GetPathRegKey(false))
            {
                return (string)reg.GetValue("Path", "", RegistryValueOptions.DoNotExpandEnvironmentNames);
            }
        }

        private static string AddPath(string list, string item)
        {
            List<string> paths = new List<string>(list.Split(';'));

            foreach (string path in paths)
                if (string.Compare(path, item, true) == 0)
                {
                    // already present
                    return list;
                }

            paths.Add(item);
            return string.Join(";", paths.ToArray());
        }

        private static string RemovePath(string list, string item)
        {
            List<string> paths = new List<string>(list.Split(';'));

            for (int i = 0; i < paths.Count; i++)
                if (string.Compare(paths[i], item, true) == 0)
                {
                    paths.RemoveAt(i);
                    return string.Join(";", paths.ToArray());
                }

            // not present
            return list;
        }

		#endregion

	}
}
