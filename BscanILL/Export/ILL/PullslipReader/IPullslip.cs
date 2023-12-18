using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BscanILL.Export.ILL.PullslipReader
{
	public interface IPullslip
	{
		//string Id { get;}
		string TransactionId { get; set; }
		string IllId { get; set;}
		string Patron { get; set;}
		string Borrower { get; set;}
		string Journal { get; set;}
		string Year { get; set;}
		string Volume { get; set;}
		string Issue { get; set;}
		string Article { get; set;}
		string Author { get; set;}
		string CatalogNumber { get; set;}
		ColorMode ColorMode { get; set;}
		string Location { get; set;}
		string Pages { get; set;}
		ExportType ExportType { get; set; }
		string Address { get; set;}
	}

	#region enum ColorMode
	public enum ColorMode : byte
	{
		BW = 0,
		Gray = 1,
		Color = 2
	}
	#endregion

	#region enum ExportType
	/*public enum ExportType : byte
	{
		FTP = 0,
		Ariel = 1,
		ILLiad = 2,
		Email = 3,
		Odyssey = 4,
		SaveOnDisk = 5
	}*/
	#endregion

	#region PullslipType
	enum PullslipType
	{
		JournalArticle,
		BookChapter,
		ILLiad,
		Rapid,
		Unknown
	}
	#endregion

}
