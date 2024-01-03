using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing.Printing;
using System.Drawing;
using System.Threading;
using System.Management;

namespace BscanILL.Export.Printing.Win32
{
	class IllPrinter : BscanILL.Export.Printing.IIllPrinter
	{
		Queue<List<string>>		batchesToPrint = new Queue<List<string>>();
		List<string>			currentBatch = null;
		int						currentImageIndex = 0;
		object					queuesLocker = new object();
		object					printLocker = new object();

		//error detecting
		//private uint lastErrorState = 0;
		//private System.Timers.Timer timer = null;
		AutoResetEvent wait = new AutoResetEvent(true);

		//counters
		/*int				sheetsCounter;
		PageSettings	pageSettings = null;*/

		//public event ExportDlg.ProgressChangedHnd UploadProgressChanged;

		PrintDocument						printDocument;
	
		List<BscanILL.Export.Printing.IPaperSize>	paperSizes = null;
		List<BscanILL.Export.Printing.IInputBin>		inputBins = null;

		BscanILL.Export.Printing.IPaperSize paperSize = null;
		BscanILL.Export.Printing.IInputBin inputBin = null;

		PrintBatch			printBatch = null;
		StreamString		streamString;
		int					printsFinished = 0;

		public event ProgressChangedHnd	ProgressChanged;


		#region constructor
		public IllPrinter(string printerName)
		{
			this.printDocument = new PrintDocument();
			bool printerSet = false;

			foreach (string printer in PrinterSettings.InstalledPrinters)
			{
				if (printerName.ToLower() == printer.ToLower())
				{
					printDocument.PrinterSettings.PrinterName = printer;

					if (printDocument.PrinterSettings.IsValid)
					{
						printerSet = true;
						break;
					}
				}
			}

			if (printerSet == false)
				throw new Exception("Printer '" + printerName + "' is not valid!");
		}
				
		public IllPrinter(string printerName, string paperSizeKey, string inputBinKey)
			: this(printerName)
		{
			foreach (IllPaperSize size in this.PaperSizes)
				if (size.Key.ToLower().Contains(paperSizeKey.ToLower()))
				{
					this.PaperSize = size;
					break;
				}

			foreach (IllInputBin bin in this.InputBins)
				if (bin.Key.ToLower().Contains(inputBinKey.ToLower()))
				{
					this.InputBin = bin;
					break;
				}
		}
		#endregion


		#region enum PrinterStatus
		enum PrinterStatus : ushort
		{
			Other = 0x1,
			Unknown = 0x2,
			Idle = 0x3,
			Printing = 0x4,
			WarmingUp = 0x5,
			StoppedPrinting = 0x6,
			Offline = 0x7
		}
		#endregion

		#region enum ExtendedPrinterStatus
		enum ExtendedPrinterStatus : ushort
		{
			Other = 1,
			Unknown = 2,
			Idle = 3,
			Printing = 4,
			WarmingUp = 5,
			StoppedPrinting = 6,
			Offline = 7,
			Paused = 8,
			Error = 9,
			Busy = 10,
			NotAvailable = 11,
			Waiting = 12,
			Processing = 13,
			Initialization = 14,
			PowerSave = 15,
			PendingDeletion = 16,
			IoActive = 17,
			ManualFeed = 18
		}
		#endregion

		#region enum DetectedErrorState
		enum DetectedErrorState : ushort
		{
			Unknown = 0,
			Other = 1,
			NoError = 2,
			LowPaper = 3,
			NoPaper = 4,
			LowToner = 5,
			NoToner = 6,
			DoorOpen = 7,
			Jammed = 8,
			Offline = 9,
			ServiceRequested = 10,
			OutputBinFull = 11,
		}
		#endregion

		#region enum ExtendedDetectedErrorState
		enum ExtendedDetectedErrorState : ushort
		{
			Unknown = 0,
			Other = 1,
			NoError = 2,
			LowPaper = 3,
			NoPaper = 4,
			LowToner = 5,
			NoToner = 6,
			DoorOpen = 7,
			Jammed = 8,
			ServiceRequested = 9,
			OutputBinFull = 10,
			PaperProblem = 11,
			CannotPrintPage = 12,
			UserInterventionRequired = 13,
			OutOfMemory = 14,
			ServerUnknown = 15,
		}
		#endregion
	
		
		// PUBLIC PROPERTIES
		#region public properties

		#region Name
		public string Name
		{
			get { return this.printDocument.PrinterSettings.PrinterName; }
		}
		#endregion

		#region PaperSizes
		public List<BscanILL.Export.Printing.IPaperSize> PaperSizes
		{
			get { return (paperSizes ?? (paperSizes = BscanILL.Export.Printing.Win32.IllPaperSize.GetPaperSizes(this.printDocument))); }
		}
		#endregion

		#region InputBins
		public List<BscanILL.Export.Printing.IInputBin> InputBins
		{
			get { return (this.inputBins ?? (this.inputBins = BscanILL.Export.Printing.Win32.IllInputBin.GetAllInputBins(this.printDocument))); }
		}
		#endregion

		#region PaperSize
		public BscanILL.Export.Printing.IPaperSize PaperSize
		{
			get
			{
				if (this.paperSize == null)
				{
					List<BscanILL.Export.Printing.IPaperSize> sizes = PaperSizes;

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
						this.paperSize = value;
						this.printDocument.DefaultPageSettings.PaperSize = ((BscanILL.Export.Printing.Win32.IllPaperSize)this.paperSize).PaperSize;
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
					List<BscanILL.Export.Printing.IInputBin> bins = InputBins;

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
						this.inputBin = value;
						this.printDocument.DefaultPageSettings.PaperSource = ((BscanILL.Export.Printing.Win32.IllInputBin)this.inputBin).PaperSource;
						break;
					}
			}
		}
		#endregion

		#region DuplexAvailable
		public bool DuplexAvailable
		{
			get { return printDocument.PrinterSettings.CanDuplex; }
		}
		#endregion

		#region PageImageableArea
		public BscanILL.Export.Printing.IllPrintableArea PageImageableArea
		{
			get
			{
				this.printDocument.OriginAtMargins = false;	
				RectangleF rect = this.printDocument.DefaultPageSettings.PrintableArea;

				return new IllPrintableArea(rect.Left / 100.0, rect.Top / 100.0, rect.Width / 100.0, rect.Height / 100.0);
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Init()
		/*public void Init()
		{
			PrintDocument printDoc = new PrintDocument();
			if (printDoc.PrinterSettings.CanDuplex && _settings.Export.Printer.DuplexPrintingIfAvailable)
				printDoc.PrinterSettings.Duplex = Duplex.Vertical;
			Margins margins = new Margins(50, 50, 50, 50);

			sheetsCounter = Convert.ToInt16(_settings.Export.Printer.Option1Capacity);

			//paper source (Tray)
			PaperSource tray1 = null;
			PaperSource tray2 = null;
			PaperSize size1 = null;
			PaperSize size2 = null;

			foreach (PaperSource paperSource in printDoc.PrinterSettings.PaperSources)
				if ((option1Enabled) && (_settings.Export.Printer.Option1Tray.Length > 0 && paperSource.SourceName == _settings.Export.Printer.Option1Tray))
				{
					tray1 = paperSource;
					break;
				}


			foreach (PaperSource paperSource in printDoc.PrinterSettings.PaperSources)
				if ((option2Enabled) && (_settings.Export.Printer.Option2Tray.Length > 0) && (paperSource.SourceName == _settings.Export.Printer.Option2Tray))
				{
					tray2 = paperSource;
					break;
				}

			if (option1Enabled && option2Enabled && (tray1 == null || tray2 == null))
				throw new BscanILL.IllException(BscanILL.Languages.BscanILLStrings.Printer_PrinterPaperSourcesAreNotSetWellCheckConfiguration_STR);

			//paper size
			foreach (PaperSize paperSize in printDoc.PrinterSettings.PaperSizes)
				if (paperSize.PaperName == _settings.Export.Printer.Option1Size)
				{
					size1 = paperSize;
					break;
				}

			foreach (PaperSize paperSize in printDoc.PrinterSettings.PaperSizes)
				if (paperSize.PaperName == _settings.Export.Printer.Option2Size)
				{
					size2 = paperSize;
					break;
				}

			if (option1Enabled && option2Enabled && (size1 == null || size2 == null))
				throw new BscanILL.IllException(BscanILL.Languages.BscanILLStrings.Printer_PrinterPaperSizesAreNotSetWellCheckConfiguration_STR);

			if (option1Enabled)
			{
				pageSettings1 = new PageSettings(printDoc.PrinterSettings);
				pageSettings1.Margins = margins;

				if (tray1 != null)
					pageSettings1.PaperSource = tray1;
				if (size1 != null)
					pageSettings1.PaperSize = size1;
			}

			if (option2Enabled)
			{
				pageSettings2 = new PageSettings(printDoc.PrinterSettings);
				pageSettings2.Margins = margins;

				if (tray2 != null)
					pageSettings2.PaperSource = tray2;
				if (size2 != null)
					pageSettings2.PaperSize = size2;
			}

			if (_settings.Export.Printer.CheckStatus)
			{
				timer = new System.Timers.Timer(300000);
				timer.AutoReset = true;
				timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
				timer.Start();
			}
		}*/
		#endregion

		#region Print()
		public static void Print(StreamString streamString, PrintBatch printBatch)
		{
			IllPrinter illPrinter = new IllPrinter(printBatch.PrinterName, printBatch.PaperSize, printBatch.PaperTray);

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
			printDocument.PrintController = new StandardPrintController();
			printDocument.PrintPage += new PrintPageEventHandler(Printer_PrintSheet);
			printDocument.Print();
		}
		#endregion
	
		#region GetStatus()
		public System.Printing.PrintQueueStatus GetStatus()
		{
			ObjectQuery oq = new ObjectQuery("SELECT Name, ExtendedPrinterStatus, DetectedErrorState FROM Win32_Printer");
			ManagementObjectSearcher query1 = new ManagementObjectSearcher(oq);

			foreach (ManagementObject mo in query1.Get())
			{
				if (this.Name == mo["Name"].ToString())
				{
					ExtendedPrinterStatus printerStatus = (ExtendedPrinterStatus) Convert.ToUInt16(mo["ExtendedPrinterStatus"]);
					DetectedErrorState errorState = (DetectedErrorState)Convert.ToUInt16(mo["DetectedErrorState"]);

					switch (errorState)
					{
						/*case DetectedErrorState.Unknown: return System.Printing.PrintQueueStatus.ServerUnknown;
						case DetectedErrorState.Other: return System.Printing.PrintQueueStatus.None;
						case DetectedErrorState.NoError: return System.Printing.PrintQueueStatus.Waiting;
						case DetectedErrorState.LowPaper: return System.Printing.PrintQueueStatus.PaperOut;*/
						case DetectedErrorState.NoPaper: return System.Printing.PrintQueueStatus.PaperOut;
						case DetectedErrorState.LowToner: return System.Printing.PrintQueueStatus.TonerLow;
						case DetectedErrorState.NoToner: return System.Printing.PrintQueueStatus.NoToner;
						case DetectedErrorState.DoorOpen: return System.Printing.PrintQueueStatus.DoorOpen;
						case DetectedErrorState.Jammed: return System.Printing.PrintQueueStatus.PaperJam;
						case DetectedErrorState.Offline: return System.Printing.PrintQueueStatus.Offline;
						case DetectedErrorState.ServiceRequested: return System.Printing.PrintQueueStatus.UserIntervention;
						case DetectedErrorState.OutputBinFull: return System.Printing.PrintQueueStatus.OutputBinFull;
					}

					switch (printerStatus)
					{
						/*case ExtendedPrinterStatus.Other: return System.Printing.PrintQueueStatus.Waiting;
						case ExtendedPrinterStatus.Unknown: return System.Printing.PrintQueueStatus.Waiting;
						case ExtendedPrinterStatus.Idle: return System.Printing.PrintQueueStatus.Waiting;
						case ExtendedPrinterStatus.Printing: return System.Printing.PrintQueueStatus.Printing;
						case ExtendedPrinterStatus.WarmingUp: return System.Printing.PrintQueueStatus.WarmingUp;*/
						case ExtendedPrinterStatus.StoppedPrinting: return System.Printing.PrintQueueStatus.Error;
						case ExtendedPrinterStatus.Offline: return System.Printing.PrintQueueStatus.Offline;
						case ExtendedPrinterStatus.Paused: return System.Printing.PrintQueueStatus.Paused;
						case ExtendedPrinterStatus.Error: return System.Printing.PrintQueueStatus.Error;
						case ExtendedPrinterStatus.Busy: return System.Printing.PrintQueueStatus.Busy;
						case ExtendedPrinterStatus.NotAvailable: return System.Printing.PrintQueueStatus.NotAvailable;
						case ExtendedPrinterStatus.Waiting: return System.Printing.PrintQueueStatus.Waiting;
						case ExtendedPrinterStatus.Processing: return System.Printing.PrintQueueStatus.Processing;
						case ExtendedPrinterStatus.Initialization: return System.Printing.PrintQueueStatus.Initializing;
						case ExtendedPrinterStatus.PowerSave: return System.Printing.PrintQueueStatus.PowerSave;
						case ExtendedPrinterStatus.PendingDeletion: return System.Printing.PrintQueueStatus.PendingDeletion;
						case ExtendedPrinterStatus.IoActive: return System.Printing.PrintQueueStatus.IOActive;
						case ExtendedPrinterStatus.ManualFeed: return System.Printing.PrintQueueStatus.ManualFeed;
					}

					return System.Printing.PrintQueueStatus.None;
				}
			}

			return System.Printing.PrintQueueStatus.None;
		}
		#endregion
	
		#endregion


		// PRIVATE METHODS
		#region private methods

		#region PrintBatch()
		private void PrintBatch(StreamString streamString, PrintBatch printBatch)
		{
			this.printBatch = printBatch;
			this.streamString = streamString;
			this.wait.Reset();

			if (this.streamString != null)
				streamString.WriteDescription("Printer: " + this.printBatch.PrinterName);

			this.printsFinished = 0;

			bool duplexPrinting = printBatch.DuplexPrinting && printDocument.PrinterSettings.CanDuplex;
			if (duplexPrinting)
				printDocument.PrinterSettings.Duplex = Duplex.Vertical;
			else
				printDocument.PrinterSettings.Duplex = Duplex.Simplex;

			//printing
			Upload_ProgressChanged(0);

			//set print job name
			printDocument.DocumentName = printBatch.PrintJobName;
			printDocument.PrintController = new StandardPrintController();
			printDocument.PrintPage += new PrintPageEventHandler(Printer_Print);

			if (duplexPrinting && this.printBatch.SplitCopiesIntoSeparatePrintJobs && ((this.printBatch.Files.Count % 2) == 1))
			{
				for (int copy = 0; copy < this.printBatch.Copies; copy++)
				{
					batchesToPrint.Enqueue(this.printBatch.Files);
					printDocument.Print();
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

				batchesToPrint.Enqueue(localList);
				printDocument.Print();
			}


			GC.Collect();
			this.wait.WaitOne(60000, false);
			Upload_ProgressChanged(1);
		}
		#endregion
			
		#region Printer_Print()
		private void Printer_Print(object sender, PrintPageEventArgs e)
		{
			try
			{
				string printedImage = null;
				Bitmap image = null;

				//gets the image to print
				lock (this.queuesLocker)
				{
					if (this.currentBatch == null && this.batchesToPrint.Count > 0)
					{
						this.currentBatch = this.batchesToPrint.Dequeue();
						this.currentImageIndex = 0;
					}

					if (this.currentBatch != null)
					{
						printedImage = this.currentBatch[currentImageIndex];

						e.Graphics.PageUnit = GraphicsUnit.Pixel;
						RectangleF margs = e.Graphics.VisibleClipBounds;

						image = new Bitmap(printedImage);
					}
				}

				if (image != null)
				{
					e.Graphics.PageUnit = GraphicsUnit.Inch;
					RectangleF margsInInches = e.Graphics.VisibleClipBounds; //MarginBounds ;

					//rotate if necessary
					if ((image.Width > image.Height) ^ (margsInInches.Width > margsInInches.Height))
						image.RotateFlip(RotateFlipType.Rotate90FlipNone);

					//RectangleF	imageRect = new RectangleF(margs.X, margs.Y, image.Width, image.Height) ;
					RectangleF	imageRect = new RectangleF(0, 0, image.Width, image.Height);
					SizeF		imageSizeInInches = new SizeF(image.Width / image.HorizontalResolution, image.Height / image.VerticalResolution);

					if (imageSizeInInches.Width < margsInInches.Width && imageSizeInInches.Height < margsInInches.Height)
					{
						imageRect.Size = new SizeF(imageSizeInInches.Width, imageSizeInInches.Height);
						imageRect.Location = new PointF((margsInInches.Width - imageRect.Width) / 2, (margsInInches.Height - imageRect.Height) / 2);
					}
					else
					{
						//resize image
						float zoom = Math.Min((float)margsInInches.Width / imageSizeInInches.Width, (float)margsInInches.Height / imageSizeInInches.Height);

						imageRect.Size = new SizeF(imageSizeInInches.Width * zoom, imageSizeInInches.Height * zoom);
						imageRect.Location = new PointF((margsInInches.Width - imageRect.Width) / 2, (margsInInches.Height - imageRect.Height) / 2);
					}

					//print image
					e.Graphics.DrawImage(image, imageRect);
					image.Dispose();
				}

				// If more images exist, print another page.
				lock (this.queuesLocker)
				{
					if (this.currentBatch != null && this.currentImageIndex < this.currentBatch.Count - 1)
					{
						this.currentImageIndex++;
						e.HasMorePages = true;
						Upload_ProgressChanged((this.currentImageIndex + 1.0F) / this.currentBatch.Count);
					}
					else
					{
						if (this.currentBatch != null)
						{
							this.currentBatch = null;
							this.currentImageIndex = 0;
							Upload_ProgressChanged(1);
						}

						e.HasMorePages = false;
					}
				}
			}
			finally
			{
				this.wait.Set();
			}
		}
		#endregion

		#region Printer_PrintSheet()
		private void Printer_PrintSheet(object sender, PrintPageEventArgs e)
		{
			try
			{
				RectangleF rect = this.printDocument.DefaultPageSettings.PrintableArea;
				Bitmap image = new Bitmap((int)e.Graphics.VisibleClipBounds.Width, (int)e.Graphics.VisibleClipBounds.Height);
				Graphics g = Graphics.FromImage(image);

				g.DrawRectangle(new Pen(Color.Black, 2), 1, 1, image.Width - 2, image.Height - 2);
				g.DrawString("Bscan ILL test sheet.", new Font("Arial", 16), new SolidBrush(Color.Black), new Rectangle(20, 20, (int)e.Graphics.VisibleClipBounds.Width, 24), StringFormat.GenericDefault);
				g.DrawString("2004-2012 Image Access, Inc.", new Font("Arial", 12), new SolidBrush(Color.Black), new Rectangle(20, 46, (int)e.Graphics.VisibleClipBounds.Width, 24), StringFormat.GenericDefault);
				g.DrawString("For more information, visit http://DLSG.net", new Font("Arial", 12), new SolidBrush(Color.Black), new Rectangle(20, 68, (int)e.Graphics.VisibleClipBounds.Width, 24), StringFormat.GenericDefault);

				g.Flush();

				if (image != null)
				{
					//print image
					e.Graphics.DrawImage(image, e.Graphics.VisibleClipBounds);

					image.Dispose();
				}

				e.HasMorePages = false;
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show("Error while printing test sheet! " + ex.Message, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
