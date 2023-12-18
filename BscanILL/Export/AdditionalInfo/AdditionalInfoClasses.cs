using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Export.ILL;

namespace BscanILL.Export.AdditionalInfo
{
	public class AdditionalBase : IAdditionalInfo
	{
		public readonly BscanILL.Scan.FileFormat	FileFormat;
		public readonly string						FileNamePrefix;
		public readonly bool						MultiImage;
		public readonly bool						PdfA;
		public readonly bool						UpdateILLiad;
		public readonly bool						ChangeStatusToRequestFinished;
    public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get; private set; }
    public int                                  FileQuality { get; private set; }		
		public bool									IncludePullslip { get; private set; }

		public AdditionalBase(string fileNamePrefix, BscanILL.Scan.FileFormat format, bool multiImage, bool pdfA, bool includePullslip, bool updateILLiad, bool changeStatusToRequestFinished, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
		{
			this.FileFormat = format;
      this.FileColor = fileColor;
      this.FileQuality = fileQuality;			
			this.FileNamePrefix = fileNamePrefix;
			this.MultiImage = multiImage;
			this.PdfA = pdfA;
			this.IncludePullslip = includePullslip;
			this.UpdateILLiad = updateILLiad;
			this.ChangeStatusToRequestFinished = changeStatusToRequestFinished;
		}
	}

	public class AdditionalFtp : AdditionalBase
	{
		public readonly BscanILL.Export.FTP.FtpLogin FtpLogin;
		public readonly bool SendConfirmEmail;        
        public readonly bool SaveToSubfolder;
        public readonly BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn SubfolderNameBase;

        public AdditionalFtp(BscanILL.Export.FTP.FtpLogin ftpLogin, bool sendConfirmEmail, bool saveToSubfolder, BscanILL.SETTINGS.Settings.ExportClass.FtpClass.SubfolderNameBasedOn subfolderNameBase, string fileNamePrefix, BscanILL.Scan.FileFormat format, 
			bool multiImage, bool pdfA, bool includePullslip, bool updateILLiad, bool changeStatusToRequestFinished, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
			: base(fileNamePrefix, format, multiImage, pdfA, includePullslip, updateILLiad, changeStatusToRequestFinished, fileColor, fileQuality)
		{
            this.SaveToSubfolder = saveToSubfolder;
            this.SubfolderNameBase = subfolderNameBase;            
			this.FtpLogin = ftpLogin;
			this.SendConfirmEmail = sendConfirmEmail;
		}
	}

	public class AdditionalFtpDir : AdditionalBase
	{
		public readonly string FtpLink;
		public readonly string ExportDirectory;
		public readonly bool SendConfirmEmail;
		public readonly bool SaveToSubfolder;
        public readonly BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn SubfolderNameBase;

        public AdditionalFtpDir(string ftpLink, string exportDir, bool sendConfirmEmail, bool saveToSubfolder, BscanILL.SETTINGS.Settings.ExportClass.FtpDirClass.SubfolderNameBasedOn subfolderNameBase, string fileNamePrefix,
			BscanILL.Scan.FileFormat format, bool multiImage, bool pdfA, bool includePullslip, bool updateILLiad, bool changeStatusToRequestFinished, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
			: base(fileNamePrefix, format, multiImage, pdfA, includePullslip, updateILLiad, changeStatusToRequestFinished, fileColor, fileQuality)
		{
            this.SaveToSubfolder = saveToSubfolder;
            this.SubfolderNameBase = subfolderNameBase;
			this.FtpLink = ftpLink;
			this.ExportDirectory = exportDir;
            this.SendConfirmEmail = sendConfirmEmail;
		}
	}
	
	public class AdditionalSaveOnDisk : AdditionalBase
	{
		public readonly string ExportDirectory;
		public readonly BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.ActionBeforeExport ActionBeforeExport;
        public readonly bool SaveToSubfolder;
        public readonly BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn SubfolderNameBase;

        public AdditionalSaveOnDisk(string exportDir, bool saveToSubfolder, BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn subfolderNameBase, string fileNamePrefix, BscanILL.Scan.FileFormat format, bool multiImage, bool pdfA,
			BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.ActionBeforeExport action, bool includePullslip, bool updateILLiad, bool changeStatusToRequestFinished, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
			: base(fileNamePrefix, format, multiImage, pdfA, includePullslip, updateILLiad, changeStatusToRequestFinished, fileColor, fileQuality)
		{
            this.SaveToSubfolder = saveToSubfolder;
            this.SubfolderNameBase = subfolderNameBase;
			this.ExportDirectory = exportDir;
			this.ActionBeforeExport = action;
		}
	}

	public class AdditionalPrinter : AdditionalBase
	{
		public readonly BscanILL.Export.Printing.IIllPrinter Printer;

		public AdditionalPrinter(BscanILL.Export.Printing.IIllPrinter printer, bool includePullslip, bool updateILLiad, bool changeStatusToRequestFinished)
			: base("prefix", Scan.FileFormat.Png, true, false, includePullslip, updateILLiad, changeStatusToRequestFinished, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto, 100)
		{
			this.Printer = printer;
		}
	}

	public class AdditionalArticleExchange : AdditionalBase
	{
		public readonly string RecipientEmail;
		public readonly string ConfirmationEmail;
		public readonly string EmailSubject;
		public readonly string EmailBody;


		public AdditionalArticleExchange(string recipientEmail, string confirmEmail, string fileNamePrefix, BscanILL.Scan.FileFormat format, 
			string emailSubject, string emailBody, bool multiImage, bool pdfA, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
			: base(fileNamePrefix, format, multiImage, pdfA, true, false, false, fileColor, fileQuality)
		{
			this.RecipientEmail = recipientEmail;
			this.ConfirmationEmail = confirmEmail;
			this.EmailSubject = emailSubject;
			this.EmailBody = emailBody;
		}
	}


    public class AdditionalWorldShareILL : AdditionalBase
    {
        public readonly string InstSymbol;

        public AdditionalWorldShareILL(string institutionalSymbol, string fileNamePrefix, BscanILL.Scan.FileFormat format,
             bool multiImage, bool pdfA, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
            : base(fileNamePrefix, format, multiImage, pdfA, true, false, false, fileColor, fileQuality)
        {
            this.InstSymbol = institutionalSymbol;
        }
    }

    public class AdditionalTipasa : AdditionalBase
    {
        public readonly string InstSymbol;

        public AdditionalTipasa(string institutionalSymbol, string fileNamePrefix, BscanILL.Scan.FileFormat format,
             bool multiImage, bool pdfA, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
            : base(fileNamePrefix, format, multiImage, pdfA, true, false, false, fileColor, fileQuality)
        {
            this.InstSymbol = institutionalSymbol;
        }
    }

	public class AdditionalRapido : AdditionalBase
	{
		public readonly string ApiKey;

		public AdditionalRapido(string apiKey, string fileNamePrefix, BscanILL.Scan.FileFormat format,
			 bool multiImage, bool pdfA, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
			: base(fileNamePrefix, format, multiImage, pdfA, true, false, false, fileColor, fileQuality)
		{
			this.ApiKey = apiKey;
		}
	}

	public class AdditionalAriel : AdditionalBase
	{
		public readonly bool UpdateArticlesWithNegativeId;
		public readonly List<string> AvoidIp;


		public AdditionalAriel(string fileNamePrefix, bool updateILLiad, bool changeStatusToRequestFinished, bool updateArticlesWithNegativeId, List<string> avoidIp)
			: base(fileNamePrefix, Scan.FileFormat.Tiff, true, false, true, updateILLiad, changeStatusToRequestFinished, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto, 100)
		{
			this.UpdateArticlesWithNegativeId = updateArticlesWithNegativeId;
			this.AvoidIp = avoidIp;
		}
	}

	public class AdditionalIlliad : AdditionalBase
	{
		//public readonly string FileName;

		public AdditionalIlliad(string fileName, BscanILL.Scan.FileFormat format, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
			: base(System.IO.Path.GetFileNameWithoutExtension(fileName), format, true, false, true, false, false, fileColor, fileQuality)
		{
			//this.FileName = fileName;
		}
	}

	public class AdditionalOdyssey : AdditionalBase
	{
		//public readonly string FileName;

		public AdditionalOdyssey(string fileName, BscanILL.Scan.FileFormat format, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
			: base(System.IO.Path.GetFileNameWithoutExtension(fileName), format, true, false, true, false, false, fileColor, fileQuality)
		{
			//this.FileName = fileName;
		}
	}

	public class AdditionalEmail : AdditionalBase
	{
		public readonly string Recipient;
		public readonly string Subject;
		public readonly string Body;


		public AdditionalEmail(string recipient, string subject, string body, string fileNamePrefix, BscanILL.Scan.FileFormat format, bool multiImage, bool pdfA,
			bool includePullslip, bool updateILLiad, bool changeStatusToRequestFinished, BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor, int fileQuality)
			: base(fileNamePrefix, format, multiImage, pdfA, includePullslip, updateILLiad, changeStatusToRequestFinished, fileColor, fileQuality)
		{
			this.Recipient = recipient;
			this.Subject = subject;
			this.Body = body;
		}
	}

}
