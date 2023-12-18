using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.Printing
{

	#region PrinterProfile
	[Serializable]
	public class PrinterProfile
	{
		string description = "";
		string printerName = "";
		string paperSize = "";
		string paperTray = "";
		Functionality functionality = Functionality.Xps;

		//BscanILL.Export.Printing.IllPrintQueue illPrintQueue = null;


		#region constructor
		public PrinterProfile()
		{
		}

		public PrinterProfile(string description, string printerName, string paperSize, string paperTray, Functionality functionality)
		{
			this.description = (description.Length > 23) ? description.Substring(0, 23) : description;
			this.printerName = printerName;
			this.paperSize = paperSize;
			this.paperTray = paperTray;
			this.functionality = functionality;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Functionality
		public Functionality Functionality
		{
			get { return functionality; }
			set { functionality = value; }
		}
		#endregion

		#region IsConnected
		public bool IsConnected
		{
			get
			{
				try
				{
					BscanILL.Export.Printing.IIllPrinter illPrintQueue = GetPrinter();

					//return (illPrintQueue != null && illPrintQueue.PaperSize != null && illPrintQueue.InputBin != null);
					return (illPrintQueue != null && illPrintQueue.PaperSize != null);
				}
				catch (Exception)
				{
					return false;
				}
			}
		}
		#endregion

		#region Description
		public string Description
		{
			get { return this.description; }
			set { this.description = (value.Length > 23) ? value.Substring(0, 23) : value; }
		}
		#endregion

		#region PrinterName
		public string PrinterName
		{
			get { return this.printerName; }
			set { this.printerName = value; }
		}
		#endregion

		#region PaperSize
		public string PaperSize
		{
			get { return this.paperSize; }
			set { this.paperSize = value; }
		}
		#endregion

		#region PaperTray
		public string PaperTray
		{
			get { return this.paperTray; }
			set { this.paperTray = value; }
		}
		#endregion

		#region DuplexAvailable
		public bool DuplexAvailable
		{
			get
			{
				IIllPrinter printQueue = GetPrinter();

				if (printQueue != null)
					return printQueue.DuplexAvailable;
				else
					return false;
			}
		}
		#endregion

		#region PaperTrayHuman
		public string PaperTrayHuman
		{
			get
			{
				IIllPrinter printQueue = GetPrinter();

				if (printQueue != null && printQueue.InputBin != null)
					return printQueue.InputBin.DisplayName;
				else
					return "Unknown";
			}
		}
		#endregion

		#region PaperSizeHuman
		public string PaperSizeHuman
		{
			get
			{
				IIllPrinter printQueue = GetPrinter();

				if (printQueue != null && printQueue.PaperSize != null)
					return printQueue.PaperSize.DisplayName;
				else
					return "Unknown";
			}
		}
		#endregion

		#region PageMediaSizeHuman
		/*public string PageMediaSizeHuman
		{
			get
			{
				PrintQueue printQueue = this.GetPrintQueue();

				if (printQueue != null)
				{
					switch (printQueue.UserPrintTicket.PageMediaSize.PageMediaSizeName.ToString())
					{
						case "Unknown": return "Unknown paper size";
						case "ISOA0": return "A0";
						case "ISOA1": return "A1";
						case "ISOA10": return "A10";
						case "ISOA2": return "A2";
						case "ISOA3": return "A3";
						case "ISOA3Rotated": return "A3 Rotated";
						case "ISOA3Extra": return "A3 Extra";
						case "ISOA4": return "A4";
						case "ISOA4Rotated": return "A4 Rotated";
						case "ISOA4Extra": return "A4 Extra";
						case "ISOA5": return "A5";
						case "ISOA5Rotated": return "A5 Rotated";
						case "ISOA5Extra": return "A5 Extra";
						case "ISOA6": return "A6";
						case "ISOA6Rotated": return "A6 Rotated";
						case "ISOA7": return "A7";
						case "ISOA8": return "A8";
						case "ISOA9": return "A9";
						case "ISOB0": return "B0";
						case "ISOB1": return "B1";
						case "ISOB10": return "B10";
						case "ISOB2": return "B2";
						case "ISOB3": return "B3";
						case "ISOB4": return "B4";
						case "ISOB4Envelope": return "B4 Envelope";
						case "ISOB5Envelope": return "B5 Envelope";
						case "ISOB5Extra": return "B5 Extra";
						case "ISOB7": return "B7";
						case "ISOB8": return "B8";
						case "ISOB9": return "B9";
						case "ISOC0": return "C0";
						case "ISOC1": return "C1";
						case "ISOC10": return "C10";
						case "ISOC2": return "C2";
						case "ISOC3": return "C3";
						case "ISOC3Envelope": return "C3 Envelope";
						case "ISOC4": return "C4";
						case "ISOC4Envelope": return "C4 Envelope";
						case "ISOC5": return "C5";
						case "ISOC5Envelope": return "C5 Envelope";
						case "ISOC6": return "C6";
						case "ISOC6Envelope": return "C6 Envelope";
						case "ISOC6C5Envelope": return "C6C5 Envelope";
						case "ISOC7": return "C7";
						case "ISOC8": return "C8";
						case "ISOC9": return "C9";
						case "ISODLEnvelope": return "DL Envelope";
						case "ISODLEnvelopeRotated": return "DL Envelope Rotated";
						case "ISOSRA3": return "SRA 3";
						case "JapanQuadrupleHagakiPostcard": return "Quadruple Hagaki Postcard";
						case "JISB0": return "Japanese Industrial Standard B0";
						case "JISB1": return "Japanese Industrial Standard B1";
						case "JISB10": return "Japanese Industrial Standard B10";
						case "JISB2": return "Japanese Industrial Standard B2";
						case "JISB3": return "Japanese Industrial Standard B3";
						case "JISB4": return "Japanese Industrial Standard B4";
						case "JISB4Rotated": return "Japanese Industrial Standard B4 Rotated";
						case "JISB5": return "Japanese Industrial Standard B5";
						case "JISB5Rotated": return "Japanese Industrial Standard B5 Rotated";
						case "JISB6": return "Japanese Industrial Standard B6";
						case "JISB6Rotated": return "Japanese Industrial Standard B6 Rotated";
						case "JISB7": return "Japanese Industrial Standard B7";
						case "JISB8": return "Japanese Industrial Standard B8";
						case "JISB9": return "Japanese Industrial Standard B9";
						case "JapanChou3Envelope": return "Chou 3 Envelope";
						case "JapanChou3EnvelopeRotated": return "Chou 3 Envelope Rotated";
						case "JapanChou4Envelope": return "Chou 4 Envelope";
						case "JapanChou4EnvelopeRotated": return "Chou 4 Envelope Rotated";
						case "JapanHagakiPostcard": return "Hagaki Postcard";
						case "JapanHagakiPostcardRotated": return "Hagaki Postcard Rotated";
						case "JapanKaku2Envelope": return "Kaku 2 Envelope";
						case "JapanKaku2EnvelopeRotated": return "Kaku 2 Envelope Rotated";
						case "JapanKaku3Envelope": return "Kaku 3 Envelope";
						case "JapanKaku3EnvelopeRotated": return "Kaku 3 Envelope Rotated";
						case "JapanYou4Envelope": return "You 4 Envelope";
						case "NorthAmerica10x11": return "10 x 11";
						case "NorthAmerica10x14": return "10 x 14";
						case "NorthAmerica11x17": return "11 x 17";
						case "NorthAmerica9x11": return "9 x 11";
						case "NorthAmericaArchitectureASheet": return "Architecture A Sheet";
						case "NorthAmericaArchitectureBSheet": return "Architecture B Sheet";
						case "NorthAmericaArchitectureCSheet": return "Architecture C Sheet";
						case "NorthAmericaArchitectureDSheet": return "Architecture D Sheet";
						case "NorthAmericaArchitectureESheet": return "Architecture E Sheet";
						case "NorthAmericaCSheet": return "C Sheet";
						case "NorthAmericaDSheet": return "D Sheet";
						case "NorthAmericaESheet": return "E Sheet";
						case "NorthAmericaExecutive": return "Executive";
						case "NorthAmericaGermanLegalFanfold": return "German Legal Fanfold";
						case "NorthAmericaGermanStandardFanfold": return "German Standard Fanfold";
						case "NorthAmericaLegal": return "Legal";
						case "NorthAmericaLegalExtra": return "Legal Extra";
						case "NorthAmericaLetter": return "Letter";
						case "NorthAmericaLetterRotated": return "Letter Rotated";
						case "NorthAmericaLetterExtra": return "Letter Extra";
						case "NorthAmericaLetterPlus": return "Letter Plus";
						case "NorthAmericaMonarchEnvelope": return "Monarch Envelope";
						case "NorthAmericaNote": return "Note";
						case "NorthAmericaNumber10Envelope": return "#10 Envelope";
						case "NorthAmericaNumber10EnvelopeRotated": return "#10 Envelope Rotated";
						case "NorthAmericaNumber9Envelope": return "#9 Envelope";
						case "NorthAmericaNumber11Envelope": return "#11 Envelope";
						case "NorthAmericaNumber12Envelope": return "#12 Envelope";
						case "NorthAmericaNumber14Envelope": return "#14 Envelope";
						case "NorthAmericaPersonalEnvelope": return "Personal Envelope";
						case "NorthAmericaQuarto": return "Quarto";
						case "NorthAmericaStatement": return "Statement";
						case "NorthAmericaSuperA": return "Super A";
						case "NorthAmericaSuperB": return "Super B";
						case "NorthAmericaTabloid": return "Tabloid";
						case "NorthAmericaTabloidExtra": return "Tabloid Extra";
						case "OtherMetricA4Plus": return "A4 Plus";
						case "OtherMetricA3Plus": return "A3 Plus";
						case "OtherMetricFolio": return "Folio";
						case "OtherMetricInviteEnvelope": return "Invite Envelope";
						case "OtherMetricItalianEnvelope": return "Italian Envelope";
						case "PRC1Envelope": return "People's Republic of China #1 Envelope";
						case "PRC1EnvelopeRotated": return "People's Republic of China #1 Envelope Rotated";
						case "PRC10Envelope": return "People's Republic of China #10 Envelope";
						case "PRC10EnvelopeRotated": return "People's Republic of China #10 Envelope Rotated";
						case "PRC16K": return "People's Republic of China 16K";
						case "PRC16KRotated": return "People's Republic of China 16K Rotated";
						case "PRC2Envelope": return "People's Republic of China #2 Envelope";
						case "PRC2EnvelopeRotated": return "People's Republic of China #2 Envelope Rotated";
						case "PRC32K": return "People's Republic of China 32K";
						case "PRC32KRotated": return "People's Republic of China 32K Rotated";
						case "PRC32KBig": return "People's Republic of China 32K Big";
						case "PRC3Envelope": return "People's Republic of China #3 Envelope";
						case "PRC3EnvelopeRotated": return "People's Republic of China #3 Envelope Rotated";
						case "PRC4Envelope": return "People's Republic of China #4 Envelope";
						case "PRC4EnvelopeRotated": return "People's Republic of China #4 Envelope Rotated";
						case "PRC5Envelope": return "People's Republic of China #5 Envelope";
						case "PRC5EnvelopeRotated": return "People's Republic of China #5 Envelope Rotated";
						case "PRC6Envelope": return "People's Republic of China #6 Envelope";
						case "PRC6EnvelopeRotated": return "People's Republic of China #6 Envelope Rotated";
						case "PRC7Envelope": return "People's Republic of China #7 Envelope";
						case "PRC7EnvelopeRotated": return "People's Republic of China #7 Envelope Rotated";
						case "PRC8Envelope": return "People's Republic of China #8 Envelope";
						case "PRC8EnvelopeRotated": return "People's Republic of China #8 Envelope Rotated";
						case "PRC9Envelope": return "People's Republic of China #9 Envelope";
						case "PRC9EnvelopeRotated": return "People's Republic of China #9 Envelope Rotated";
						case "Roll04Inch": return "4-inch wide roll";
						case "Roll06Inch": return "6-inch wide roll";
						case "Roll08Inch": return "8-inch wide roll";
						case "Roll12Inch": return "12-inch wide roll";
						case "Roll15Inch": return "15-inch wide roll";
						case "Roll18Inch": return "18-inch wide roll";
						case "Roll22Inch": return "22-inch wide roll";
						case "Roll24Inch": return "24-inch wide roll";
						case "Roll30Inch": return "30-inch wide roll";
						case "Roll36Inch": return "36-inch wide roll";
						case "Roll54Inch": return "54-inch wide roll";
						case "JapanDoubleHagakiPostcard": return "Double Hagaki Postcard";
						case "JapanDoubleHagakiPostcardRotated": return "Double Hagaki Postcard Rotated";
						case "JapanLPhoto": return "L Photo";
						case "Japan2LPhoto": return "2L Photo";
						case "JapanYou1Envelope": return "You 1 Envelope";
						case "JapanYou2Envelope": return "You 2 Envelope";
						case "JapanYou3Envelope": return "You 3 Envelope";
						case "JapanYou4EnvelopeRotated": return "You 4 Envelope Rotated";
						case "JapanYou6Envelope": return "You 6 Envelope";
						case "JapanYou6EnvelopeRotated": return "You 6 Envelope Rotated";
						case "NorthAmerica4x6": return "4 x 6";
						case "NorthAmerica4x8": return "4 x 8";
						case "NorthAmerica5x7": return "5 x 7";
						case "NorthAmerica8x10": return "8 x 10";
						case "NorthAmerica10x12": return "10 x 12";
						case "NorthAmerica14x17": return "14 x 17";
						case "BusinessCard": return "Business card";
						case "CreditCard": return "Credit card";
						default: return "Unknown";
					}
				}
				else
					return "Unknown";
			}
		}*/
		#endregion

		#endregion

		//PUBLIC METHODS
		#region public methods

		#region Set()
		public void Set(string description, string printerName, string paperSize, string paperTray, Functionality functionality)
		{
			this.description = (description.Length > 23) ? description.Substring(0, 23) : description;
			this.printerName = printerName;
			this.paperSize = paperSize;
			this.paperTray = paperTray;
			this.functionality = functionality;
		}
		#endregion

		#region GetPrinter()
		public BscanILL.Export.Printing.IIllPrinter GetPrinter()
		{
			if (this.functionality == BscanILL.Export.Printing.Functionality.Xps)
				return BscanILL.Export.Printing.Xps.IllPrinters.GetIllPrinter(this.PrinterName, this.PaperTray, this.PaperSize);
			else if (functionality == BscanILL.Export.Printing.Functionality.Win32)
				return BscanILL.Export.Printing.Win32.IllPrinters.GetIllPrinter(this.PrinterName, this.PaperTray, this.PaperSize);
			else
				throw new Exception("Unsupported Printing Functionality!");
		}
		#endregion

		#region GetPaperSizeName()
		/*public static string GetPaperSizeName(PageMediaSizeName imageableArea)
		{
			switch (imageableArea)
			{
				case PageMediaSizeName.Unknown: return "Unknown paper size";
				case PageMediaSizeName.ISOA0: return "A0";
				case PageMediaSizeName.ISOA1: return "A1";
				case PageMediaSizeName.ISOA10: return "A10";
				case PageMediaSizeName.ISOA2: return "A2";
				case PageMediaSizeName.ISOA3: return "A3";
				case PageMediaSizeName.ISOA3Rotated: return "A3 Rotated";
				case PageMediaSizeName.ISOA3Extra: return "A3 Extra";
				case PageMediaSizeName.ISOA4: return "A4";
				case PageMediaSizeName.ISOA4Rotated: return "A4 Rotated";
				case PageMediaSizeName.ISOA4Extra: return "A4 Extra";
				case PageMediaSizeName.ISOA5: return "A5";
				case PageMediaSizeName.ISOA5Rotated: return "A5 Rotated";
				case PageMediaSizeName.ISOA5Extra: return "A5 Extra";
				case PageMediaSizeName.ISOA6: return "A6";
				case PageMediaSizeName.ISOA6Rotated: return "A6 Rotated";
				case PageMediaSizeName.ISOA7: return "A7";
				case PageMediaSizeName.ISOA8: return "A8";
				case PageMediaSizeName.ISOA9: return "A9";
				case PageMediaSizeName.ISOB0: return "B0";
				case PageMediaSizeName.ISOB1: return "B1";
				case PageMediaSizeName.ISOB10: return "B10";
				case PageMediaSizeName.ISOB2: return "B2";
				case PageMediaSizeName.ISOB3: return "B3";
				case PageMediaSizeName.ISOB4: return "B4";
				case PageMediaSizeName.ISOB4Envelope: return "B4 Envelope";
				case PageMediaSizeName.ISOB5Envelope: return "B5 Envelope";
				case PageMediaSizeName.ISOB5Extra: return "B5 Extra";
				case PageMediaSizeName.ISOB7: return "B7";
				case PageMediaSizeName.ISOB8: return "B8";
				case PageMediaSizeName.ISOB9: return "B9";
				case PageMediaSizeName.ISOC0: return "C0";
				case PageMediaSizeName.ISOC1: return "C1";
				case PageMediaSizeName.ISOC10: return "C10";
				case PageMediaSizeName.ISOC2: return "C2";
				case PageMediaSizeName.ISOC3: return "C3";
				case PageMediaSizeName.ISOC3Envelope: return "C3 Envelope";
				case PageMediaSizeName.ISOC4: return "C4";
				case PageMediaSizeName.ISOC4Envelope: return "C4 Envelope";
				case PageMediaSizeName.ISOC5: return "C5";
				case PageMediaSizeName.ISOC5Envelope: return "C5 Envelope";
				case PageMediaSizeName.ISOC6: return "C6";
				case PageMediaSizeName.ISOC6Envelope: return "C6 Envelope";
				case PageMediaSizeName.ISOC6C5Envelope: return "C6C5 Envelope";
				case PageMediaSizeName.ISOC7: return "C7";
				case PageMediaSizeName.ISOC8: return "C8";
				case PageMediaSizeName.ISOC9: return "C9";
				case PageMediaSizeName.ISODLEnvelope: return "DL Envelope";
				case PageMediaSizeName.ISODLEnvelopeRotated: return "DL Envelope Rotated";
				case PageMediaSizeName.ISOSRA3: return "SRA 3";
				case PageMediaSizeName.JapanQuadrupleHagakiPostcard: return "Quadruple Hagaki Postcard";
				case PageMediaSizeName.JISB0: return "Japanese Industrial Standard B0";
				case PageMediaSizeName.JISB1: return "Japanese Industrial Standard B1";
				case PageMediaSizeName.JISB10: return "Japanese Industrial Standard B10";
				case PageMediaSizeName.JISB2: return "Japanese Industrial Standard B2";
				case PageMediaSizeName.JISB3: return "Japanese Industrial Standard B3";
				case PageMediaSizeName.JISB4: return "Japanese Industrial Standard B4";
				case PageMediaSizeName.JISB4Rotated: return "Japanese Industrial Standard B4 Rotated";
				case PageMediaSizeName.JISB5: return "Japanese Industrial Standard B5";
				case PageMediaSizeName.JISB5Rotated: return "Japanese Industrial Standard B5 Rotated";
				case PageMediaSizeName.JISB6: return "Japanese Industrial Standard B6";
				case PageMediaSizeName.JISB6Rotated: return "Japanese Industrial Standard B6 Rotated";
				case PageMediaSizeName.JISB7: return "Japanese Industrial Standard B7";
				case PageMediaSizeName.JISB8: return "Japanese Industrial Standard B8";
				case PageMediaSizeName.JISB9: return "Japanese Industrial Standard B9";
				case PageMediaSizeName.JapanChou3Envelope: return "Chou 3 Envelope";
				case PageMediaSizeName.JapanChou3EnvelopeRotated: return "Chou 3 Envelope Rotated";
				case PageMediaSizeName.JapanChou4Envelope: return "Chou 4 Envelope";
				case PageMediaSizeName.JapanChou4EnvelopeRotated: return "Chou 4 Envelope Rotated";
				case PageMediaSizeName.JapanHagakiPostcard: return "Hagaki Postcard";
				case PageMediaSizeName.JapanHagakiPostcardRotated: return "Hagaki Postcard Rotated";
				case PageMediaSizeName.JapanKaku2Envelope: return "Kaku 2 Envelope";
				case PageMediaSizeName.JapanKaku2EnvelopeRotated: return "Kaku 2 Envelope Rotated";
				case PageMediaSizeName.JapanKaku3Envelope: return "Kaku 3 Envelope";
				case PageMediaSizeName.JapanKaku3EnvelopeRotated: return "Kaku 3 Envelope Rotated";
				case PageMediaSizeName.JapanYou4Envelope: return "You 4 Envelope";
				case PageMediaSizeName.NorthAmerica10x11: return "10 x 11";
				case PageMediaSizeName.NorthAmerica10x14: return "10 x 14";
				case PageMediaSizeName.NorthAmerica11x17: return "11 x 17";
				case PageMediaSizeName.NorthAmerica9x11: return "9 x 11";
				case PageMediaSizeName.NorthAmericaArchitectureASheet: return "Architecture A Sheet";
				case PageMediaSizeName.NorthAmericaArchitectureBSheet: return "Architecture B Sheet";
				case PageMediaSizeName.NorthAmericaArchitectureCSheet: return "Architecture C Sheet";
				case PageMediaSizeName.NorthAmericaArchitectureDSheet: return "Architecture D Sheet";
				case PageMediaSizeName.NorthAmericaArchitectureESheet: return "Architecture E Sheet";
				case PageMediaSizeName.NorthAmericaCSheet: return "C Sheet";
				case PageMediaSizeName.NorthAmericaDSheet: return "D Sheet";
				case PageMediaSizeName.NorthAmericaESheet: return "E Sheet";
				case PageMediaSizeName.NorthAmericaExecutive: return "Executive";
				case PageMediaSizeName.NorthAmericaGermanLegalFanfold: return "German Legal Fanfold";
				case PageMediaSizeName.NorthAmericaGermanStandardFanfold: return "German Standard Fanfold";
				case PageMediaSizeName.NorthAmericaLegal: return "Legal";
				case PageMediaSizeName.NorthAmericaLegalExtra: return "Legal Extra";
				case PageMediaSizeName.NorthAmericaLetter: return "Letter";
				case PageMediaSizeName.NorthAmericaLetterRotated: return "Letter Rotated";
				case PageMediaSizeName.NorthAmericaLetterExtra: return "Letter Extra";
				case PageMediaSizeName.NorthAmericaLetterPlus: return "Letter Plus";
				case PageMediaSizeName.NorthAmericaMonarchEnvelope: return "Monarch Envelope";
				case PageMediaSizeName.NorthAmericaNote: return "Note";
				case PageMediaSizeName.NorthAmericaNumber10Envelope: return "#10 Envelope";
				case PageMediaSizeName.NorthAmericaNumber10EnvelopeRotated: return "#10 Envelope Rotated";
				case PageMediaSizeName.NorthAmericaNumber9Envelope: return "#9 Envelope";
				case PageMediaSizeName.NorthAmericaNumber11Envelope: return "#11 Envelope";
				case PageMediaSizeName.NorthAmericaNumber12Envelope: return "#12 Envelope";
				case PageMediaSizeName.NorthAmericaNumber14Envelope: return "#14 Envelope";
				case PageMediaSizeName.NorthAmericaPersonalEnvelope: return "Personal Envelope";
				case PageMediaSizeName.NorthAmericaQuarto: return "Quarto";
				case PageMediaSizeName.NorthAmericaStatement: return "Statement";
				case PageMediaSizeName.NorthAmericaSuperA: return "Super A";
				case PageMediaSizeName.NorthAmericaSuperB: return "Super B";
				case PageMediaSizeName.NorthAmericaTabloid: return "Tabloid";
				case PageMediaSizeName.NorthAmericaTabloidExtra: return "Tabloid Extra";
				case PageMediaSizeName.OtherMetricA4Plus: return "A4 Plus";
				case PageMediaSizeName.OtherMetricA3Plus: return "A3 Plus";
				case PageMediaSizeName.OtherMetricFolio: return "Folio";
				case PageMediaSizeName.OtherMetricInviteEnvelope: return "Invite Envelope";
				case PageMediaSizeName.OtherMetricItalianEnvelope: return "Italian Envelope";
				case PageMediaSizeName.PRC1Envelope: return "People's Republic of China #1 Envelope";
				case PageMediaSizeName.PRC1EnvelopeRotated: return "People's Republic of China #1 Envelope Rotated";
				case PageMediaSizeName.PRC10Envelope: return "People's Republic of China #10 Envelope";
				case PageMediaSizeName.PRC10EnvelopeRotated: return "People's Republic of China #10 Envelope Rotated";
				case PageMediaSizeName.PRC16K: return "People's Republic of China 16K";
				case PageMediaSizeName.PRC16KRotated: return "People's Republic of China 16K Rotated";
				case PageMediaSizeName.PRC2Envelope: return "People's Republic of China #2 Envelope";
				case PageMediaSizeName.PRC2EnvelopeRotated: return "People's Republic of China #2 Envelope Rotated";
				case PageMediaSizeName.PRC32K: return "People's Republic of China 32K";
				case PageMediaSizeName.PRC32KRotated: return "People's Republic of China 32K Rotated";
				case PageMediaSizeName.PRC32KBig: return "People's Republic of China 32K Big";
				case PageMediaSizeName.PRC3Envelope: return "People's Republic of China #3 Envelope";
				case PageMediaSizeName.PRC3EnvelopeRotated: return "People's Republic of China #3 Envelope Rotated";
				case PageMediaSizeName.PRC4Envelope: return "People's Republic of China #4 Envelope";
				case PageMediaSizeName.PRC4EnvelopeRotated: return "People's Republic of China #4 Envelope Rotated";
				case PageMediaSizeName.PRC5Envelope: return "People's Republic of China #5 Envelope";
				case PageMediaSizeName.PRC5EnvelopeRotated: return "People's Republic of China #5 Envelope Rotated";
				case PageMediaSizeName.PRC6Envelope: return "People's Republic of China #6 Envelope";
				case PageMediaSizeName.PRC6EnvelopeRotated: return "People's Republic of China #6 Envelope Rotated";
				case PageMediaSizeName.PRC7Envelope: return "People's Republic of China #7 Envelope";
				case PageMediaSizeName.PRC7EnvelopeRotated: return "People's Republic of China #7 Envelope Rotated";
				case PageMediaSizeName.PRC8Envelope: return "People's Republic of China #8 Envelope";
				case PageMediaSizeName.PRC8EnvelopeRotated: return "People's Republic of China #8 Envelope Rotated";
				case PageMediaSizeName.PRC9Envelope: return "People's Republic of China #9 Envelope";
				case PageMediaSizeName.PRC9EnvelopeRotated: return "People's Republic of China #9 Envelope Rotated";
				case PageMediaSizeName.Roll04Inch: return "4-inch wide roll";
				case PageMediaSizeName.Roll06Inch: return "6-inch wide roll";
				case PageMediaSizeName.Roll08Inch: return "8-inch wide roll";
				case PageMediaSizeName.Roll12Inch: return "12-inch wide roll";
				case PageMediaSizeName.Roll15Inch: return "15-inch wide roll";
				case PageMediaSizeName.Roll18Inch: return "18-inch wide roll";
				case PageMediaSizeName.Roll22Inch: return "22-inch wide roll";
				case PageMediaSizeName.Roll24Inch: return "24-inch wide roll";
				case PageMediaSizeName.Roll30Inch: return "30-inch wide roll";
				case PageMediaSizeName.Roll36Inch: return "36-inch wide roll";
				case PageMediaSizeName.Roll54Inch: return "54-inch wide roll";
				case PageMediaSizeName.JapanDoubleHagakiPostcard: return "Double Hagaki Postcard";
				case PageMediaSizeName.JapanDoubleHagakiPostcardRotated: return "Double Hagaki Postcard Rotated";
				case PageMediaSizeName.JapanLPhoto: return "L Photo";
				case PageMediaSizeName.Japan2LPhoto: return "2L Photo";
				case PageMediaSizeName.JapanYou1Envelope: return "You 1 Envelope";
				case PageMediaSizeName.JapanYou2Envelope: return "You 2 Envelope";
				case PageMediaSizeName.JapanYou3Envelope: return "You 3 Envelope";
				case PageMediaSizeName.JapanYou4EnvelopeRotated: return "You 4 Envelope Rotated";
				case PageMediaSizeName.JapanYou6Envelope: return "You 6 Envelope";
				case PageMediaSizeName.JapanYou6EnvelopeRotated: return "You 6 Envelope Rotated";
				case PageMediaSizeName.NorthAmerica4x6: return "4 x 6";
				case PageMediaSizeName.NorthAmerica4x8: return "4 x 8";
				case PageMediaSizeName.NorthAmerica5x7: return "5 x 7";
				case PageMediaSizeName.NorthAmerica8x10: return "8 x 10";
				case PageMediaSizeName.NorthAmerica10x12: return "10 x 12";
				case PageMediaSizeName.NorthAmerica14x17: return "14 x 17";
				case PageMediaSizeName.BusinessCard: return "Business card";
				case PageMediaSizeName.CreditCard: return "Credit card";
				default: return "Unknown";
			}
		}*/
		#endregion

		#region InvalidatePrinterQueue()
		public void InvalidatePrinterQueue()
		{
			//this.illPrintQueue = null;
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region GetPageMediaSize()
		/*private static System.Printing.PageMediaSize GetPageMediaSize(System.Printing.PrintQueue printQueue, string pageName)
		{
			if (printQueue != null)
				foreach (System.Printing.PageMediaSize pageSize in printQueue.GetPrintCapabilities().PageMediaSizeCapability)
					if (pageSize.PageMediaSizeName.HasValue && pageSize.PageMediaSizeName.ToString() == pageName)
						return pageSize;

			return new System.Printing.PageMediaSize(System.Printing.PageMediaSizeName.NorthAmericaLetter);
		}*/
		#endregion

		#region GetInputBin()
		/*private static System.Printing.InputBin GetInputBin(System.Printing.PrintQueue printQueue, string inputBinName)
		{
			if (printQueue != null)
				foreach (System.Printing.InputBin inputBin in printQueue.GetPrintCapabilities().InputBinCapability)
					if (inputBin.ToString() == inputBinName)
						return inputBin;

			return System.Printing.InputBin.AutoSelect;
		}*/
		#endregion

		#endregion

	}
	#endregion

}
