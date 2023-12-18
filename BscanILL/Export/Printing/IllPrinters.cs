using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.Printing
{
	class IllPrinters
	{
		public static List<IIllPrinter> GetPrinters(Functionality functionality)
		{
			if (functionality == Functionality.Win32)
				return BscanILL.Export.Printing.Win32.IllPrinters.GetPrinters();
			else
				return BscanILL.Export.Printing.Xps.IllPrinters.GetPrinters();
		}

		public static IIllPrinter GetDefaultPrinter(Functionality functionality)
		{
			if (functionality == Functionality.Win32)
				return BscanILL.Export.Printing.Win32.IllPrinters.GetDefaultPrinter();
			else
				return BscanILL.Export.Printing.Xps.IllPrinters.GetDefaultPrinter();
		}
	}
}
