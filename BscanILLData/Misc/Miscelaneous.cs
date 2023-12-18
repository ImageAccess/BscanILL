using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Misc
{
	public static class Miscelaneous
	{
		public static DirectoryInfo StartupDir { get { return new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory; } }
	}
}
