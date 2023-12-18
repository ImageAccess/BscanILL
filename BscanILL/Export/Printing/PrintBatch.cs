using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;
using System.Runtime.Serialization;
using System.Printing;

namespace BscanILL.Export.Printing
{
	[Serializable]
	public class PrintBatch
	{
		string			printJobName = "Bscan ILL " + DateTime.Now.ToString("HH-mm-ss");
		string			printerName = "";
		string			paperSize = "";
		string			paperTray = "";
		ushort			copies = 1;
		bool			duplexPrinting = false;
		bool			splitCopiesIntoSeparatePrintJobs = true;
		Functionality	functionality = Functionality.Xps;
		List<string>	files = new List<string>();


		public PrintBatch()
		{
		}

		public PrintBatch(string printJobName, string printerName, string paperSize, string paperTray, ushort copies, bool duplexPrinting,
			bool splitCopiesIntoSeparatePrintJobs, Functionality functionality, List<string> files)
		{
			this.printJobName = printJobName;
			this.printerName = printerName;
			this.paperSize = paperSize;
			this.paperTray = paperTray;
			this.copies = copies;
			this.duplexPrinting = duplexPrinting;
			this.splitCopiesIntoSeparatePrintJobs = splitCopiesIntoSeparatePrintJobs;
			this.functionality = functionality;
			this.files = files;
		}

		// PUBLIC PROPERTIES
		#region public properties

		public string		PrintJobName						{ get { return this.printJobName; } set { this.printJobName = value; } }
		public string		PrinterName							{ get { return this.printerName; } set { this.printerName = value; } }
		public string		PaperSize							{ get { return this.paperSize; } set { this.paperSize = value; } }
		public string		PaperTray							{ get { return this.paperTray; } set { this.paperTray = value; } }
		public ushort		Copies								{ get { return this.copies; } set { this.copies = value; } }
		public bool			DuplexPrinting						{ get { return this.duplexPrinting; } set { this.duplexPrinting = value; } }
		public bool			SplitCopiesIntoSeparatePrintJobs	{ get { return this.splitCopiesIntoSeparatePrintJobs; } set { this.splitCopiesIntoSeparatePrintJobs = value; } }
		public Functionality Functionality						{ get { return this.functionality; } set { this.functionality = value; } }
		public List<string> Files								{ get { return this.files; } set { this.files = value; } }

		#region DuplexAvailable
		/*[XmlIgnore]
		public bool DuplexAvailable
		{
			get
			{
				IllPrintQueue printQueue = GetPrintQueue();
				return printQueue.DuplexAvailable;
			}
		}*/
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region GetPrintQueue()
		/*public IllPrintQueue GetPrintQueue()
		{
			return GetPrintQueue(this.PrinterName, this.PaperTray, this.PaperSize);
		}

		public static IllPrintQueue GetPrintQueue(string printerName, string paperTray, string paperSize)
		{
			foreach (System.Printing.PrintQueue printQueue in IllPrintQueues.GetPrintQueues())
				if (printQueue.Name.ToLower() == printerName.ToLower())
				{
					//SetInputBinAndPaperSize(printQueue, paperTray, paperSize);
					//printQueue.UserPrintTicket.PageMediaSize = GetPageMediaSize(printQueue, this.PaperSize);
					//printQueue.UserPrintTicket.InputBin = GetInputBin(printQueue, this.PaperTray);

					IllPrintQueue illPrintQueue = new IllPrintQueue(printQueue, paperSize, paperTray);

					return illPrintQueue;
				}

			return null;
		}*/
		#endregion

		#region SerializeToString()
		public string SerializeToString()
		{
			DataContractSerializer serializer = new DataContractSerializer(typeof(PrintBatch));

			using (MemoryStream memoryStream = new MemoryStream())
			{
				serializer.WriteObject(memoryStream, this);
				memoryStream.Seek(0, SeekOrigin.Begin);

				using (StreamReader streamReader = new StreamReader(memoryStream))
				{
					return streamReader.ReadToEnd();
				}
			}
		}
		#endregion

		#region DeserializeFromString()
		public static PrintBatch DeserializeFromString(string xml)
		{
			using (TextReader textReader = new StringReader(xml))
			{
				using (XmlReader xmlReader = new XmlTextReader(textReader))
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(PrintBatch));
					PrintBatch printBatch = (PrintBatch)serializer.ReadObject(xmlReader);
					return printBatch;
				}
			}
		}
		#endregion

		#endregion

	}
}
