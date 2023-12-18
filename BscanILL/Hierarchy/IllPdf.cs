using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;


namespace BscanILL.Hierarchy
{
    internal class IllPdf
    {
        IllPage illPage;

        FileInfo formatXmlFile = null;
        DirectoryInfo formatXmlFileDir = null;
        Scanners.ColorMode colorDepthOfPreProcessedPDF = Scanners.ColorMode.Unknown;
        int exportQualityOfPreProcessedPDF = 0;
        long sourceFileSize = 0;         //file size of original image used for pdf creation - we need this info when exporting searchable pdf with size limit (for email delivery)
        object pdfFileLocker = new object();
        BscanILL.Hierarchy.IllPage.FileStatus pdfFileStatus = IllPage.FileStatus.WaitingToStart;
        object fileStatusLocker = new object();
        volatile ManualResetEvent pdfFileResetEvent = new ManualResetEvent(false);


        #region constructor
        public IllPdf(IllPage illPage)
        {
            this.illPage = illPage;
        }
        #endregion


        //PUBLIC PROPERTIES
        #region public properties

        public IllPage IllPage { get { return this.illPage; } }

        #region PdfFileStatus
        public BscanILL.Hierarchy.IllPage.FileStatus PdfFileStatus
        {
            get { lock (this.fileStatusLocker) { return this.pdfFileStatus; } }
            set { lock (this.fileStatusLocker) { this.pdfFileStatus = value; } }
        }
        #endregion

        #region FormatXmlFile
        public FileInfo FormatXmlFile
        {
            get
            {
                this.pdfFileResetEvent.WaitOne();

                return this.formatXmlFile;
            }
        }
        #endregion

        #region FormatXmlFileDir
        public DirectoryInfo FormatXmlFileDir
        {
            get
            {
                this.pdfFileResetEvent.WaitOne();

                return this.formatXmlFileDir;
            }
        }
        #endregion

        #region ColorDepthOfPreProcessedPDF
        public Scanners.ColorMode ColorDepthOfPreProcessedPDF
        {
            get
            {
                return this.colorDepthOfPreProcessedPDF;
            }
        }
        #endregion

        #region ExportQualityOfPreProcessedPDF
        public int ExportQualityOfPreProcessedPDF
        {
            get
            {
                return this.exportQualityOfPreProcessedPDF;
            }
        }
        #endregion

        #region SourceFileSize
        public long SourceFileSize
        {
            get
            {
                return this.sourceFileSize;
            }
        }
        #endregion

        #endregion


        //PUBLIC METHODS
        #region public methods

        #region
        public void ClearFormatXmlFileDir()
        {
            if(this.formatXmlFileDir != null)
            {
                if (this.formatXmlFileDir.Exists)
                {
                    this.formatXmlFileDir.Delete(true);
                    this.formatXmlFileDir.Refresh();
                }
                this.formatXmlFileDir = null;
            }

        }
        #endregion


        #region CreatePdfFile()
        public void CreatePdfFile(Scanners.ColorMode colorMode, int quality)
		{           
            BscanILL.Export.ExportFiles.PdfsBuilder.Instance.QueueToFormattedFile(this, colorMode, quality);
		}
		#endregion

		#region CancelPdfFileCreation()
		public void CancelPdfFileCreation()
		{
			BscanILL.Export.ExportFiles.PdfsBuilder.Instance.RemoveFromQueue(this);
		}
		#endregion

		#region Delete()
		public void Delete()
		{
			pdfFileResetEvent.WaitOne();

			try
			{
				if (this.formatXmlFile != null && this.formatXmlFile.Exists)
					this.formatXmlFile.Delete();

                if (this.formatXmlFileDir != null && this.formatXmlFileDir.Exists)
                    this.formatXmlFileDir.Delete(true);
			}
			catch { }

			this.formatXmlFile = null;
            this.formatXmlFileDir = null;
		}
		#endregion

		#region FormattedFileCreated()
		public void FormattedFileCreated(FileInfo formattedFile)
		{
			this.formatXmlFile = formattedFile;
			this.PdfFileStatus = (this.formatXmlFile != null) ? BscanILL.Hierarchy.IllPage.FileStatus.Created : BscanILL.Hierarchy.IllPage.FileStatus.Error;
			pdfFileResetEvent.Set();
		}
		#endregion

        #region FormattedDirCreated()
        public void FormattedDirCreated(DirectoryInfo formattedFileDir, Scanners.ColorMode colorDepth, int exportQuality, long sourceFileSize)
		{
            this.formatXmlFileDir = formattedFileDir;
            this.colorDepthOfPreProcessedPDF = colorDepth;
            this.exportQualityOfPreProcessedPDF = exportQuality;
            this.sourceFileSize = sourceFileSize;
            this.PdfFileStatus = (this.formatXmlFileDir != null) ? BscanILL.Hierarchy.IllPage.FileStatus.Created : BscanILL.Hierarchy.IllPage.FileStatus.Error;
			pdfFileResetEvent.Set();
		}
		#endregion

		#region FormattedFileCreationError()
		public void FormattedFileCreationError()
		{
			this.PdfFileStatus = BscanILL.Hierarchy.IllPage.FileStatus.Error;
			Delete();
			pdfFileResetEvent.Set();
		}
		#endregion

		#region FormattedFileCreationCanceled()
		public void FormattedFileCreationCanceled()
		{
			this.PdfFileStatus = BscanILL.Hierarchy.IllPage.FileStatus.Error;
			pdfFileResetEvent.Set();
		}
		#endregion

		#endregion

	}
}
