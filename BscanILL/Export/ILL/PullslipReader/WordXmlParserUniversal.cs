using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using System.IO;

namespace BscanILL.Export.ILL.PullslipReader
{
	class WordXmlParserUniversal
	{

		#region Read()
		public static Pullslip[] Read(FileInfo file)
		{
			XmlDocument xmlDocument = new XmlDocument();
			List<Pullslip> pullslips = new List<Pullslip>();

			xmlDocument.Load(file.FullName);

			XmlNodeList xmlNodeListTables = xmlDocument.GetElementsByTagName("w:tbl");

			foreach (XmlElement xmlNodeTable in xmlNodeListTables)
			{
				XmlPullslip xmlPullslip = ReadEntireTable(xmlNodeTable);

				if (xmlPullslip.IsValid())
				{
					Pullslip pullslip = xmlPullslip.GetPullslip();

					if (pullslip != null)
						pullslips.Add(pullslip);
					else
						xmlPullslip.Clear();
				}
			}

			return pullslips.ToArray();
		}
		#endregion

		#region ReadEntireTable()
		static XmlPullslip ReadEntireTable(XmlElement xmlNodeTable)
		{
			XmlPullslip xmlPullslip = new XmlPullslip();
			XmlNodeList xmlNodeListColumns = xmlNodeTable.GetElementsByTagName("w:tc");

			for (int column = 0; column < xmlNodeListColumns.Count; column++)
			{
				XmlNodeList xmlNodeListLines = ((XmlElement)xmlNodeListColumns[column]).GetElementsByTagName("w:p");

				foreach (XmlElement xmlNodeLine in xmlNodeListLines)
				{
					XmlNodeList xmlNodeListLineElements = ((XmlElement)xmlNodeLine).GetElementsByTagName("w:t");
					List<string> strings = new List<string>();

					foreach (XmlElement xmlNodeLineElements in xmlNodeListLineElements)
						strings.Add(xmlNodeLineElements.InnerText);

					while (strings.Count > 0 && strings[0].Trim().Length == 0)
						strings.RemoveAt(0);

					while (strings.Count > 0 && strings[strings.Count - 1].Trim().Length == 0)
						strings.RemoveAt(strings.Count - 1);

					/*for (int i = strings.Count - 1; i >= 0; i--)
						if (strings[i].Trim().Length == 0)
							strings.RemoveAt(i);*/

					if (strings.Count > 0)
						xmlPullslip.Add(new XmlLine(column, strings));
				}
			}

			return xmlPullslip;
		}
		#endregion

		#region class XmlPullslip
		public class XmlPullslip : List<XmlLine>
		{
			public XmlPullslip()
			{
			}

			public bool IsValid()
			{
				return true;
			}

			public Pullslip GetPullslip()
			{
				Pullslip pullslip = new Pullslip();
				string keyVal;

				if ((keyVal = GetValue("Ship Via:")) != null)
				{
					switch (keyVal.ToLower())
					{
						case "odyssey":
							{
								pullslip.ExportType = ExportType.Odyssey;
							} break;
						case "ariel":
							{
								pullslip.ExportType = ExportType.Ariel;
								if ((keyVal = GetValue("Ariel:")) != null)
									pullslip.Address = keyVal;
							} break;
						case "email":
							{
								pullslip.ExportType = ExportType.Email;
								if ((keyVal = GetValue("Email:")) != null)
									pullslip.Address = keyVal;
							}
							break;
						case "illiad":
							{
								pullslip.ExportType = ExportType.ILLiad;
							} break;
						case "ftp":
							{
								pullslip.ExportType = ExportType.Ftp;
							} break;
						case "fax":
							{
								pullslip.ExportType = ExportType.SaveOnDisk;
								if ((keyVal = GetValue("Fax:")) != null)
									pullslip.Address = keyVal;
							} break;
						default:
							{
								pullslip.ExportType = ExportType.SaveOnDisk;
							} break;
					}
				}
				else
					pullslip.ExportType = ExportType.SaveOnDisk;

				if ((keyVal = GetValue("ILLiad TN:")) != null)
					pullslip.IllId = keyVal;
				if ((keyVal = GetValue("ILL:")) != null)
					pullslip.TransactionId = keyVal;

				if ((keyVal = GetValue("Journal")) != null)
					pullslip.Journal = keyVal;
				if ((keyVal = GetValue("Article Author:")) != null)
					pullslip.Author = keyVal;
				if ((keyVal = GetValue("Article Title:")) != null)
					pullslip.Article = keyVal;
				if ((keyVal = GetValue("Borrower:")) != null)
					pullslip.Borrower = keyVal;
				if ((keyVal = GetValue("Patron:")) != null)
					pullslip.Patron = keyVal;

				if ((keyVal = GetValue("Volume:")) != null)
					pullslip.Volume = keyVal;
				if ((keyVal = GetValue("Issue:")) != null)
					pullslip.Issue = keyVal;
				if ((keyVal = GetValue("Year:")) != null)
					pullslip.Year = keyVal;
				if ((keyVal = GetValue("Location:")) != null)
					pullslip.Location = keyVal;
				if ((keyVal = GetValue("Pages:")) != null)
					pullslip.Pages = keyVal;
				if ((keyVal = GetValue("Call #:")) != null)
					pullslip.CatalogNumber = keyVal;

				if ((pullslip.TransactionId != null && pullslip.TransactionId.Trim().Length > 0) || (pullslip.IllId != null && pullslip.IllId.Trim().Length > 0))
					return pullslip;
				else
					return null;

				/*if (pullslip.Id.Trim().Length > 0)
					return pullslip;
				else
					return null;*/
			}

			private string GetValue(string key)
			{
				foreach (XmlLine xmlLine in this)
				{
					if (xmlLine.OneString.ToLower().TrimStart().StartsWith(key.ToLower()) && xmlLine.OneString.IndexOf(":") >= 0)
					{
						int indexOf = xmlLine.OneString.IndexOf(":");

						if (indexOf >= 0 && indexOf < xmlLine.OneString.Length - 1)
							return xmlLine.OneString.Substring(indexOf + 1).Trim();
						
						else if (xmlLine.Items.Count > 1)
							return xmlLine.Items[xmlLine.Items.Count - 1];
						else
							return "";
					}
				}

				return null;
			}
		}
		#endregion

		#region class XmlLine
		public class XmlLine
		{
			public readonly int Column;
			public readonly List<string> Items;

			public XmlLine(int column, List<string> items)
			{
				this.Column = column;
				this.Items = items;
			}

			public string OneString
			{
				get
				{
					string oneString = "";

					foreach (string str in Items)
						oneString += str;

					return oneString;
				}
			}

			/*public bool Contains(string str)
			{
				foreach (string item in Items)
					if (item.ToLower() == str.ToLower())
						return true;

				return false;
			}*/
		}
		#endregion

	}
}
