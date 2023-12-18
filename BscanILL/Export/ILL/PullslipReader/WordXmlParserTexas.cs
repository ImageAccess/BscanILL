using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using System.IO;

namespace BscanILL.Export.ILL.PullslipReader
{
	class WordXmlParserTexas
	{

		#region Read()
		public static Pullslip[] Read(FileInfo file)
		{
			XmlDocument xmlDocument = new XmlDocument();
			ArrayList requestsPulledFromXml = new ArrayList();

			xmlDocument.Load(file.FullName);

			XmlNodeList xmlNodeListTables = xmlDocument.GetElementsByTagName("w:tbl");

			foreach (XmlElement xmlNodeTable in xmlNodeListTables)
			{
				XmlNodeList xmlNodeListCells = xmlNodeTable.GetElementsByTagName("w:tc");

				if(xmlNodeListCells.Count >= 4)
				{
					string tableCaption = "";
					XmlNodeList textElements;
					Pullslip pullslip = null;

					textElements = ((XmlElement)xmlNodeListCells[0]).GetElementsByTagName("w:t");

					foreach (XmlElement textElement in textElements)
						tableCaption += textElement.InnerText + " ";

					if (tableCaption.ToLower().Contains("document") && tableCaption.ToLower().Contains("delivery"))
						pullslip = ReadDocumentDelivery(xmlNodeListCells);
					else if (tableCaption.ToLower().Contains("ill"))
						pullslip = ReadIll(xmlNodeListCells);

					if(pullslip != null)
						requestsPulledFromXml.Add(pullslip);
				}
			}

			return (Pullslip[]) requestsPulledFromXml.ToArray(typeof(Pullslip));
		}
		#endregion

		#region ReadDocumentDelivery()
		public static Pullslip ReadDocumentDelivery(XmlNodeList xmlNodeListCells)
		{
			Pullslip pullslip = null;
			string illiadNumberCellText = "";
			XmlNodeList illCellElements = ((XmlElement)xmlNodeListCells[1]).GetElementsByTagName("w:t");

			foreach (XmlElement illCellElement in illCellElements)
				illiadNumberCellText += illCellElement.InnerText;

			int index1 = illiadNumberCellText.IndexOf('*');
			int index2 = illiadNumberCellText.LastIndexOf('*');
			if (index1 >= 0 && index1 != index2)
			{
				string transactionId = illiadNumberCellText.Substring(index1 + 1, index2 - index1 - 1).Trim();

				if (transactionId.Length > 0)
				{
					pullslip = new Pullslip();
					pullslip.TransactionId = transactionId;
					pullslip.ExportType = ExportType.ILLiad;

					XmlNodeList rowElements = ((XmlElement)xmlNodeListCells[2]).GetElementsByTagName("w:p");
					foreach (XmlElement rowElement in rowElements)
					{
						XmlNodeList textElements = rowElement.GetElementsByTagName("w:t");

						if (textElements.Count >= 2)
						{
							if (textElements[0].InnerText.ToLower().StartsWith("journal title"))
								pullslip.Journal = textElements[1].InnerText;
							else if (textElements[0].InnerText.ToLower().StartsWith("volume"))
								pullslip.Volume = textElements[1].InnerText;
							else if (textElements[0].InnerText.ToLower().StartsWith("issue"))
								pullslip.Issue = textElements[1].InnerText;
							else if (textElements[0].InnerText.ToLower().StartsWith("pages"))
								pullslip.Pages = textElements[1].InnerText;
							else if (textElements[0].InnerText.ToLower().StartsWith("article title"))
								pullslip.Article = textElements[1].InnerText;
							else if (textElements[0].InnerText.ToLower().StartsWith("article author"))
								pullslip.Author = textElements[1].InnerText;
							else if (textElements[0].InnerText.ToLower().StartsWith("month/year"))
								pullslip.Year = textElements[1].InnerText;
						}
					}

					rowElements = ((XmlElement)xmlNodeListCells[3]).GetElementsByTagName("w:p");
					foreach (XmlElement rowElement in rowElements)
					{
						XmlNodeList textElements = rowElement.GetElementsByTagName("w:t");

						if (textElements.Count >= 2)
						{
							if (textElements[0].InnerText.ToLower().StartsWith("location"))
								pullslip.Location = textElements[1].InnerText;
							if (textElements[0].InnerText.ToLower().StartsWith("call #:"))
								pullslip.CatalogNumber = textElements[1].InnerText;
						}
					}
				}
			}

			return pullslip;
		}
		#endregion

		#region ReadIll()
		public static Pullslip ReadIll(XmlNodeList xmlNodeListCells)
		{
			Pullslip pullslip = null;
			string illiadNumberCellText = "";
			XmlNodeList illCellElements = ((XmlElement)xmlNodeListCells[1]).GetElementsByTagName("w:t");

			foreach (XmlElement illCellElement in illCellElements)
				illiadNumberCellText += illCellElement.InnerText;

			int index1 = illiadNumberCellText.IndexOf('*');
			int index2 = illiadNumberCellText.LastIndexOf('*');
			if (index1 >= 0 && index1 != index2)
			{
				string transactionId = illiadNumberCellText.Substring(index1 + 1, index2 - index1 - 1).Trim();

				if (transactionId.Length > 0)
				{
					pullslip = new Pullslip();
					pullslip.TransactionId = transactionId;
					pullslip.ExportType = ExportType.Ariel;

					XmlNodeList rowElements = ((XmlElement)xmlNodeListCells[2]).GetElementsByTagName("w:p");
					foreach (XmlElement rowElement in rowElements)
					{
						XmlNodeList textElements = rowElement.GetElementsByTagName("w:t");

						if (textElements.Count >= 2)
						{
							if (textElements[0].InnerText.ToLower().StartsWith("borrower"))
								pullslip.Borrower = textElements[1].InnerText;
							else if (textElements[0].InnerText.ToLower().StartsWith("journal title"))
								pullslip.Journal = textElements[1].InnerText;
							else if (textElements.Count >= 4 && textElements[0].InnerText.ToLower().StartsWith("volume"))
							{
								pullslip.Volume = textElements[1].InnerText;
								pullslip.Issue = textElements[3].InnerText;
							}
							else if (textElements.Count >= 4 && textElements[2].InnerText.ToLower().StartsWith("pages"))
							{
								pullslip.Year = textElements[1].InnerText;
								pullslip.Pages = textElements[3].InnerText;
							}
							else if (textElements[0].InnerText.ToLower().StartsWith("article title"))
								pullslip.Article = textElements[1].InnerText;
							else if (textElements[0].InnerText.ToLower().StartsWith("article author"))
								pullslip.Author = textElements[1].InnerText;
							else if (textElements[0].InnerText.ToLower().StartsWith("ill number"))
								pullslip.IllId = textElements[1].InnerText;
						}
					}

					rowElements = ((XmlElement)xmlNodeListCells[3]).GetElementsByTagName("w:p");
					foreach (XmlElement rowElement in rowElements)
					{
						XmlNodeList textElements = rowElement.GetElementsByTagName("w:t");

						if (textElements.Count >= 2)
						{
							if (textElements[0].InnerText.ToLower().StartsWith("location"))
								pullslip.Location = textElements[1].InnerText;
							if (textElements[0].InnerText.ToLower().StartsWith("patron"))
								pullslip.Patron = textElements[1].InnerText;
							if (textElements[0].InnerText.ToLower().StartsWith("ariel"))
								pullslip.Address = textElements[1].InnerText;
							if (textElements[0].InnerText.ToLower().StartsWith("call #:"))
								pullslip.CatalogNumber = textElements[1].InnerText;
						}
					}
				}
			}

			return pullslip;
		}
		#endregion


	}
}
