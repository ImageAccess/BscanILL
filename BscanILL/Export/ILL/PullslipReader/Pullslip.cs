using System;

namespace BscanILL.Export.ILL.PullslipReader
{
	/// <summary>
	/// Summary description for Request.
	/// </summary>
	[Serializable]
	public class Pullslip : IPullslip
	{
		string transactionId = "";
		string illId = "";
		string patron = "";
		string borrower = "";
		string journal = "";
		string article = "";
		string author = "";
		string year = "";
		string volume = "";
		string issue = "";
		string catalogNumber = "";
		ColorMode	colorMode = ColorMode.BW;
		string location = "";
		string pages = "";
		ExportType	exportType = ExportType.Ariel;
		string address = "";

		public Pullslip()
		{
		}

		public string TransactionId { get { return transactionId; } set { transactionId = value; } }
		public string IllId { get { return illId; } set { illId = value; } }
		public string Patron { get { return patron; } set { patron = value; } }
		public string Borrower { get { return borrower; } set { borrower = value; } }
		public string Journal { get { return journal; } set { journal = value; } }
		public string Article { get { return article; } set { article = value; } }
		public string Author { get { return author; } set { author = value; } }
		public string Year { get { return year; } set { year = value; } }
		public string Volume { get { return volume; } set { volume = value; } }
		public string Issue { get { return issue; } set { issue = value; } }
		public string CatalogNumber { get { return catalogNumber; } set { catalogNumber = value; } }
		public ColorMode ColorMode { get { return colorMode; } set { colorMode = value; } }
		public string Location { get { return location; } set { location = value; } }
		public string Pages { get { return pages; } set { pages = value; } }
		public ExportType ExportType { get { return exportType; } set { exportType = value; } }
		public string Address { get { return address; } set { address = value; } }

		public override string ToString()
		{
			string str = "";

			str += "Transaction Id: " + this.transactionId + ", ";
			str += "ILL Id: " + this.illId + ", ";
			str += "Patron: " + this.patron + ", ";
			str += "Borrower: " + this.borrower + ", ";
			str += "Journal: " + this.journal + ", ";
			str += "Article: " + this.article + ", ";
			str += "Author: " + this.author + ", ";
			str += "Year: " + this.year + ", ";
			str += "Volume: " + this.volume + ", ";
			str += "Issue: " + this.issue + ", ";
			str += "Catalog Number: " + this.catalogNumber + ", ";
			str += "Color Mode: " + this.colorMode.ToString() + ", ";
			str += "Location: " + this.location + ", ";
			str += "Pages: " + this.pages + ", ";
			str += "Export Type: " + this.exportType.ToString() + ", ";
			str += "Address: " + this.address;
		
			return str;
		}


	}
}
