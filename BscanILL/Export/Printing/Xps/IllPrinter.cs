using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Printing;
using System.Xml;
using System.IO;
using System.Windows;

namespace BscanILL.Export.Printing.Xps
{
	class IllPrinter : BscanILL.Export.Printing.IIllPrinter
	{
		PrintQueue printQueue;

		List<BscanILL.Export.Printing.IPaperSize>		paperSizes = null;
		List<BscanILL.Export.Printing.IInputBin>		inputBins = null;

		BscanILL.Export.Printing.IPaperSize			paperSize = null;
		BscanILL.Export.Printing.IInputBin			inputBin = null;

		PrintBatch				printBatch = null;
		StreamString			streamString;
		int						printsFinished = 0;

		public event ProgressChangedHnd		ProgressChanged;


		#region constructor
		/*public IllPrinter(PrintServer printServer, string printQueueName)
		{
			this.printQueue = new PrintQueue(printServer, printQueueName);
		}*/

		public IllPrinter(PrintQueue printQueue)
		{
			this.printQueue = printQueue;
		}

		public IllPrinter(PrintQueue printQueue, string paperSizeKey, string inputBinKey)
		{
			this.printQueue = printQueue;
			SetPaperSizeAndInputBin(paperSizeKey, inputBinKey);
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#region Name
		public string Name
		{
			get { return this.printQueue.Name; }
		}
		#endregion

		#region PaperSizes
		public List<BscanILL.Export.Printing.IPaperSize> PaperSizes
		{
			get { return (paperSizes ?? (paperSizes = BscanILL.Export.Printing.Xps.IllPaperSize.GetPaperSizes(printQueue))); }
		}
		#endregion

		#region InputBins
		public List<BscanILL.Export.Printing.IInputBin> InputBins
		{
			get { return (inputBins ?? (inputBins = BscanILL.Export.Printing.Xps.IllInputBin.GetAllInputBins(printQueue))); }
		}
		#endregion

		#region PaperSize
		public BscanILL.Export.Printing.IPaperSize PaperSize
		{
			get 
			{
				if (this.paperSize == null)
				{
					List<BscanILL.Export.Printing.IPaperSize> sizes = this.PaperSizes;

					if (sizes.Count > 0)
						this.paperSize = sizes[0];
				}
				
				return this.paperSize; 
			}
			set
			{
				foreach (BscanILL.Export.Printing.IPaperSize ps in this.PaperSizes)
					if (ps.DisplayName == value.DisplayName)
					{
						this.paperSize = ps;
						SetPrintTicket();
						break;
					}
			}
		}
		#endregion

		#region InputBin
		public BscanILL.Export.Printing.IInputBin InputBin
		{
			get
			{
				if (this.inputBin == null)
				{
					List<BscanILL.Export.Printing.IInputBin> bins = this.InputBins;

					if (bins.Count > 0)
						this.inputBin = bins[0];
				}
				
				return this.inputBin;
			}
			set
			{
				foreach(BscanILL.Export.Printing.IInputBin bin in this.InputBins)
					if (bin.DisplayName == value.DisplayName)
					{
						this.inputBin = bin;
						SetPrintTicket();
						break;
					}
			}
		}
		#endregion

		#region DuplexAvailable
		public bool DuplexAvailable
		{
			get
			{
				return printQueue.GetPrintCapabilities().DuplexingCapability.Contains(System.Printing.Duplexing.TwoSidedLongEdge);
			}
		}
		#endregion

		#region PrintQueue
		public PrintQueue PrintQueue
		{
			get { return printQueue; }
		}
		#endregion

		#region PageImageableArea
		public BscanILL.Export.Printing.IllPrintableArea PageImageableArea
		{
			get 
			{
				PageImageableArea	printable = this.printQueue.GetPrintCapabilities().PageImageableArea;

				return new IllPrintableArea(printable);
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Init()
		/*public void Init()
		{
		}*/
		#endregion

		#region Print()
		public static void Print(StreamString streamString, PrintBatch printBatch)
		{
			IllPrinter illPrinter = IllPrinters.GetIllPrinter(printBatch.PrinterName, printBatch.PaperSize, printBatch.PaperTray);

			illPrinter.PrintBatch(streamString, printBatch);
		}
		#endregion

		#region Print()
		public void Print(PrintBatch printBatch)
		{
			PrintBatch(null, printBatch);
		}
		#endregion

		#region PrintTestSheet
		public void PrintTestSheet()
		{
			PrintCapabilities capabilities = printQueue.GetPrintCapabilities();
			PageMediaSize pageMediaSize = printQueue.UserPrintTicket.PageMediaSize;
			PageImageableArea printable = capabilities.PageImageableArea;
			Thickness margin = new Thickness(printable.OriginWidth, printable.OriginHeight,
				capabilities.OrientedPageMediaWidth.Value - (printable.OriginWidth + printable.ExtentWidth) + 2, capabilities.OrientedPageMediaHeight.Value - (printable.OriginHeight + printable.ExtentHeight) + 2);
			//Thickness margin = new Thickness(printable.OriginWidth + 10, printable.OriginHeight + 10, printable.OriginWidth - 10, printable.OriginHeight - 10);

			PrintTestSheetPaginator paginator = new PrintTestSheetPaginator(pageMediaSize, margin);

			System.Windows.Xps.XpsDocumentWriter writer = System.Printing.PrintQueue.CreateXpsDocumentWriter(printQueue);
			writer.Write(paginator);
		}
		#endregion

		#region GetStatus()
		public System.Printing.PrintQueueStatus GetStatus()
		{
			return printQueue.QueueStatus;
		}
		#endregion
		
		#endregion


		// PRIVATE METHODS
		#region private methods

		#region PrintBatch
		private void PrintBatch(StreamString streamString, PrintBatch printBatch)
		{
			this.printBatch = printBatch;
			this.streamString = streamString;

			if (this.streamString != null)
				streamString.WriteDescription("Printer: " + this.printBatch.PrinterName);

			//PrintQueue printQueue = IllPrinters.GetIllPrinter(this.printBatch.PrinterName, this.printBatch.PaperTray, this.printBatch.PaperSize);
			PrintCapabilities capabilities = printQueue.GetPrintCapabilities();
			PageMediaSize pageMediaSize = printQueue.UserPrintTicket.PageMediaSize;

			this.printsFinished = 0;

			PageImageableArea printable = capabilities.PageImageableArea;
			Thickness margin = new Thickness(printable.OriginWidth, printable.OriginHeight,
				capabilities.OrientedPageMediaWidth.Value - (printable.OriginWidth + printable.ExtentWidth) + 2, capabilities.OrientedPageMediaHeight.Value - (printable.OriginHeight + printable.ExtentHeight) + 2);

			//printing
			Upload_ProgressChanged(0);

			//set print job name
			printQueue.CurrentJobSettings.Description = printBatch.PrintJobName;
			bool duplexPrinting = printBatch.DuplexPrinting;

			if (duplexPrinting && this.printBatch.SplitCopiesIntoSeparatePrintJobs && ((this.printBatch.Files.Count % 2) == 1))
			{
				for (int copy = 0; copy < this.printBatch.Copies; copy++)
				{
					using (PrintPaginator paginator = new PrintPaginator(this.printBatch.Files, pageMediaSize, margin))
					{
						printQueue.UserPrintTicket.Duplexing = duplexPrinting ? Duplexing.TwoSidedLongEdge : Duplexing.OneSided;

						System.Windows.Xps.XpsDocumentWriter writer = System.Printing.PrintQueue.CreateXpsDocumentWriter(printQueue);

						writer.WritingProgressChanged += new System.Windows.Documents.Serialization.WritingProgressChangedEventHandler(Writer_WritingProgressChanged);
						writer.Write(paginator);
					}
				}
			}
			else
			{
				List<string> localList = new List<string>();

				for (int copy = 0; copy < this.printBatch.Copies; copy++)
				{
					localList.AddRange(this.printBatch.Files);

					if ((copy < this.printBatch.Copies - 1) && duplexPrinting && ((printBatch.Files.Count % 2) == 1))
					{
						//add empty page
						localList.Add(null);
					}
				}

				using (PrintPaginator paginator = new PrintPaginator(localList, pageMediaSize, margin))
				{
					printQueue.UserPrintTicket.Duplexing = duplexPrinting ? Duplexing.TwoSidedLongEdge : Duplexing.OneSided;

					System.Windows.Xps.XpsDocumentWriter writer = System.Printing.PrintQueue.CreateXpsDocumentWriter(printQueue);

					writer.WritingProgressChanged += new System.Windows.Documents.Serialization.WritingProgressChangedEventHandler(Writer_WritingProgressChanged);
					writer.Write(paginator);
				}
			}

			Upload_ProgressChanged(1);
			GC.Collect();
		}
		#endregion

		#region SetPrintTicket()
		private void SetPrintTicket()
		{
			PrintTicket ticket = this.printQueue.DefaultPrintTicket.Clone();

			// input bin first, it changes printQueue.UserPrintTicket
			if (this.InputBin != null)
			{
				// read Xml of the PrintTicket
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(ticket.GetXmlStream());

				// create NamespaceManager and add PrintSchemaFrameWork-Namespace hinzufugen (should be on DocumentElement of the PrintTicket)
				// Prefix: psf NameSpace: xmlDoc.DocumentElement.NamespaceURI = "http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework"
				XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
				manager.AddNamespace(xmlDoc.DocumentElement.Prefix, xmlDoc.DocumentElement.NamespaceURI);

				// search node with desired feature we're looking for and set newValue for it
				string xpath = "//psf:Feature[@name='psk:JobInputBin']/psf:Option";
				XmlNode node = xmlDoc.SelectSingleNode(xpath, manager);
				if (node != null)
				{
					node.Attributes["name"].Value = this.InputBin.Key;
				}

				/*xpath = "//psf:Feature[@name='psk:PageMediaSize']/psf:Option";
				node = xmlDoc.SelectSingleNode(xpath, manager);
				if (node != null)
				{
					node.Attributes["name"].Value = paperSize;
				}*/

				// create a new PrintTicket out of the XML
				PrintTicket modifiedPrintTicket;
				
				using (MemoryStream printTicketStream = new MemoryStream())
				{
					xmlDoc.Save(printTicketStream);
					printTicketStream.Position = 0;
					modifiedPrintTicket = new PrintTicket(printTicketStream);
				}

				// for testing purpose save the printticket to file
				//FileStream stream = new FileStream("modPrintticket.xml", FileMode.CreateNew, FileAccess.ReadWrite);
				//modifiedPrintTicket.GetXmlStream().WriteTo(stream);

				ticket = modifiedPrintTicket;
			}

			if (this.PaperSize != null)
				ticket.PageMediaSize = ((BscanILL.Export.Printing.Xps.IllPaperSize)this.PaperSize).PageMediaSize;

			this.printQueue.UserPrintTicket = ticket;
		}
		#endregion

		#region SetPaperSizeAndInputBin()
		private void SetPaperSizeAndInputBin(string paperSizeKey, string inputBinKey)
		{
			foreach (IllPaperSize size in this.PaperSizes)
				if (size.Key.ToLower().Contains(paperSizeKey.ToLower()))
				{
					this.paperSize = size;
					break;
				}

			foreach (IllInputBin bin in this.InputBins)
				if (bin.Key.ToLower().Contains(inputBinKey.ToLower()))
				{
					this.inputBin = bin;
					break;
				}

			SetPrintTicket();
		}
		#endregion

		#region SetPaperSize()
		private void SetPaperSize(string paperSizeKey)
		{
			foreach (IllPaperSize size in this.PaperSizes)
				if (size.Key.ToLower().Contains(paperSizeKey.ToLower()))
				{
					this.PaperSize = size;
					break;
				}
		}
		#endregion

		#region SetInputBin()
		private void SetInputBin(string inputBinKey)
		{
			foreach (IllInputBin bin in this.InputBins)
				if (bin.Key.ToLower().Contains(inputBinKey.ToLower()))
				{
					this.InputBin = bin;
					break;
				}
		}
		#endregion

		#region Writer_WritingProgressChanged()
		void Writer_WritingProgressChanged(object sender, System.Windows.Documents.Serialization.WritingProgressChangedEventArgs e)
		{
			try
			{
				if (printBatch != null && e.WritingLevel == System.Windows.Documents.Serialization.WritingProgressChangeLevel.FixedPageWritingProgress)
				{
					this.printsFinished++;
					Upload_ProgressChanged(Math.Min(1.0F, (this.printsFinished / (float)(this.printBatch.Files.Count * this.printBatch.Copies))));
				}
			}
			catch { }
		}
		#endregion

		#region Upload_ProgressChanged()
		void Upload_ProgressChanged(double progress)
		{
			if (this.streamString != null)
				streamString.WriteProgress(progress);

			if (ProgressChanged != null)
				ProgressChanged(progress);
		}
		#endregion

		#endregion

	}

}
