using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.Printing
{
	public class PrintingOptions
	{
		public readonly string PrintJobName;
		public readonly BscanILL.Export.Printing.PrinterProfile PrinterProfile = null;
		public readonly ushort Copies = 1;
		public readonly bool DuplexPrinting = false;
		public readonly bool AutoLevels = true;
		public readonly double AutoLevelsValue = 0.2;
		public System.Windows.Rect? Clip = null;
		public string PharosUserId = null;


		#region constructor
		public PrintingOptions(string printJobName, BscanILL.Export.Printing.PrinterProfile printerProfile, ushort copies, bool duplex, bool autoLevels, System.Windows.Rect? clip)
		{
			this.PrintJobName = printJobName;
			this.PrinterProfile = printerProfile;
			this.Copies = copies;
			this.DuplexPrinting = duplex;
			this.AutoLevels = autoLevels;
			this.Clip = clip;
		}
		#endregion

	}

}
