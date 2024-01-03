using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing.Printing;

namespace BscanILL.Export.Printing.Win32
{
	class IllInputBin : IInputBin
	{
		string displayName;
		string value;
		PaperSource bin;

		public IllInputBin(PaperSource bin)
		{
			this.bin = bin;
			this.displayName = bin.SourceName;
			this.value = bin.SourceName;
		}

		public string		DisplayName { get { return this.displayName; } }
		public string		Key { get { return this.value; } }
		public PaperSource	PaperSource { get { return this.bin; } }


		#region GetAllInputBins()
		public static List<IInputBin> GetAllInputBins(PrintDocument printDoc)
		{
			List<IInputBin> bins = new List<IInputBin>();

			foreach (PaperSource bin in printDoc.PrinterSettings.PaperSources)
				bins.Add(new IllInputBin(bin));

			return bins;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			return this.DisplayName;
		}
		#endregion

	}
}
