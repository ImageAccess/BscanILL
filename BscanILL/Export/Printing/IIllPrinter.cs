using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.Printing
{
	public delegate void ProgressChangedHnd(double progress);

	public interface IIllPrinter
	{
		// PUBLIC PROPERTIES
		string										Name				{ get; }
		List<BscanILL.Export.Printing.IPaperSize>	PaperSizes			{ get; }
		List<BscanILL.Export.Printing.IInputBin>	InputBins			{ get; }
		BscanILL.Export.Printing.IPaperSize			PaperSize			{ get; set; }
		BscanILL.Export.Printing.IInputBin			InputBin			{ get; set; }
		bool										DuplexAvailable		{ get; }
		BscanILL.Export.Printing.IllPrintableArea	PageImageableArea	{ get; }
		//PrintQueue						PrintQueue{ get; }

		// PUBLIC METHODS
		//void Init();
		//void Print(StreamString streamString, PrintBatch printBatch);
		void								Print(PrintBatch printBatch);
		void								PrintTestSheet();
		System.Printing.PrintQueueStatus	GetStatus();

		//EVENTS
		event ProgressChangedHnd			ProgressChanged;
	}

	public enum Functionality
	{
		Win32,
		Xps
	}

}
