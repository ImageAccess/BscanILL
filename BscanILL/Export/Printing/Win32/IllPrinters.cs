using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Drawing.Printing;

namespace BscanILL.Export.Printing.Win32
{
	class IllPrinters 
	{
		
		// PUBLIC METHODS
		#region public methods

		#region GetIllPrinter()
		public static IllPrinter GetIllPrinter(string printerName, string paperTray, string paperSize)
		{			
			foreach (string printer in PrinterSettings.InstalledPrinters)
				if (printerName.ToLower() == printer.ToLower())
					return new IllPrinter(printer, paperSize, paperTray);

			return null;
		}
		#endregion

		#region GetPrinters()
		public static List<IIllPrinter> GetPrinters()
		{
			List<IIllPrinter> printers = new List<IIllPrinter>();

			foreach (string printerStr in PrinterSettings.InstalledPrinters)
			{
				try
				{
					IllPrinter printer = new IllPrinter(printerStr);
					printers.Add(printer);
				}
				catch { }
			}

			return printers;
		}
		#endregion

		#region GetDefaultPrinter()
		public static IIllPrinter GetDefaultPrinter()
		{
			PrintDocument printDocument = new PrintDocument();

			if (printDocument != null && printDocument.PrinterSettings != null)
			{
				return new IllPrinter(printDocument.PrinterSettings.PrinterName);
			}

			return null;
		}
		#endregion
	
		#endregion

	}
}
