using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Printing;
using System.IO;
using System.Xml;

namespace BscanILL.Export.Printing.Xps
{
	class IllPrinters 
	{
		
		// PUBLIC METHODS
		#region public methods

		#region GetDefaultPrintQueue()
		public static PrintQueue GetDefaultPrintQueue()
		{
			PrintQueue printQueue = LocalPrintServer.GetDefaultPrintQueue();

			return printQueue;
		}
		#endregion

		#region GetPrintQueues()
		public static PrintQueueCollection GetPrintQueues()
		{
			PrintServer printServer = new PrintServer();
			EnumeratedPrintQueueTypes[] printTypes = new EnumeratedPrintQueueTypes[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections };
			PrintQueueCollection printQueues = printServer.GetPrintQueues(printTypes);

/*#if DEBUG			
			PrintQueue printQueue = printQueues.First();
			
			SavePrintTicket(printQueue);
			AdjustPrintTicket(printQueue);

			printQueues = new PrintQueueCollection();
			printQueues.Add(printQueue);
#endif*/

			return printQueues;
		}
		#endregion

		#region GetPrintQueue()
		/*public IllPrintQueue GetPrintQueue()
		{
			return GetPrintQueue(this.PrinterName, this.PaperTray, this.PaperSize);
		}*/

		public static IllPrinter GetIllPrinter(string printerName, string paperTray, string paperSize)
		{
			foreach (System.Printing.PrintQueue printQueue in IllPrinters.GetPrintQueues())
				if (printQueue.Name.ToLower() == printerName.ToLower())
				{
					//SetInputBinAndPaperSize(printQueue, paperTray, paperSize);
					//printQueue.UserPrintTicket.PageMediaSize = GetPageMediaSize(printQueue, this.PaperSize);
					//printQueue.UserPrintTicket.InputBin = GetInputBin(printQueue, this.PaperTray);

					IllPrinter illPrintQueue = new IllPrinter(printQueue, paperSize, paperTray);

					return illPrintQueue;
				}

			return null;
		}
		#endregion

		#region GetPrinters()
		public static List<IIllPrinter> GetPrinters()
		{
			List<IIllPrinter> printers = new List<IIllPrinter>();

			foreach (System.Printing.PrintQueue printQueue in IllPrinters.GetPrintQueues())
				{
					try
					{
						IllPrinter printer = new IllPrinter(printQueue);
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
			PrintQueue printQueue = LocalPrintServer.GetDefaultPrintQueue();

			if (printQueue != null)
				return new IllPrinter(printQueue);

			return null;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region AdjustPrintTicket()
		private static void AdjustPrintTicket(PrintQueue printQueue)
		{
			try
			{
				PrintTicket modifiedPrintTicket;

				// create a new PrintTicket out of the XML
				using (FileStream printTicketStream = new FileStream(@"C:\Users\jirka.stybnar\projects\printers\NARA\printer.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					modifiedPrintTicket = new PrintTicket(printTicketStream);
				}

				using (FileStream printTicketStream = new FileStream(@"C:\Users\jirka.stybnar\projects\printers\NARA\printerNara.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					modifiedPrintTicket = new PrintTicket(printTicketStream);
				}

				printQueue.UserPrintTicket = modifiedPrintTicket;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		#endregion

		#region SavePrintTicket()
		private static void SavePrintTicket(PrintQueue printQueue)
		{
			PrintTicket ticket = printQueue.DefaultPrintTicket.Clone();

			// read Xml of the PrintTicket
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(ticket.GetXmlStream());

			// create NamespaceManager and add PrintSchemaFrameWork-Namespace hinzufugen (should be on DocumentElement of the PrintTicket)
			// Prefix: psf NameSpace: xmlDoc.DocumentElement.NamespaceURI = "http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework"
			XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
			manager.AddNamespace(xmlDoc.DocumentElement.Prefix, xmlDoc.DocumentElement.NamespaceURI);

			// create a new PrintTicket out of the XML
			PrintTicket modifiedPrintTicket;

			using (MemoryStream printTicketStream = new MemoryStream())
			{
				xmlDoc.Save(printTicketStream);
				printTicketStream.Position = 0;
				modifiedPrintTicket = new PrintTicket(printTicketStream);
			}

			// for testing purpose save the printticket to file
			using (FileStream stream = new FileStream(@"C:\Users\jirka.stybnar\projects\printers\NARA\printer.xml", FileMode.Create, FileAccess.ReadWrite))
			{
				modifiedPrintTicket.GetXmlStream().WriteTo(stream);
			}
		}
		#endregion

		#endregion

	}
}
