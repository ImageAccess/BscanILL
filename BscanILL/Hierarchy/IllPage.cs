using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using BscanILL.Export.ILL;
using BscanILL.Misc;
using System.ComponentModel;
using System.Reflection;


namespace BscanILL.Hierarchy
{
	/// <summary>
	/// Summary description for IllPage.
	/// </summary>
	public class IllPage : INotifyPropertyChanged
	{
		IllImage							illImage;
		//Bitmap							thumbnailBase = null;
		BscanILL.Hierarchy.IllPdf			illPdf = null;
		ImageProcessing.ImageFile.ImageInfo fullImageInfo;

		FileInfo							origFile = null;
		bool								selected = false;

		BscanILLData.Models.DbPage			dbPage = null;

		public object						bitmapDataLocker = new object();

		//image properties
		Scanners.ColorMode	colorMode;
		short				origDpi;
		short				dpi;
		double				brightnessOrig;
		double				contrastOrig;

		//IllPage				associatedImage = null;
		//int					associatedImageOverlapping = 0;

		//events
		public event BscanILL.Hierarchy.IllPageHnd ImageSelected;
		public event BscanILL.Hierarchy.IllPageHnd ExportBitmapChanging;
		public event BscanILL.Hierarchy.IllPageHnd ExportBitmapChanged;
		public event BscanILL.Hierarchy.IllPageHnd SettingsChanged;

		volatile object					deleteFilesLocker = new object();
		volatile IllImageFilesStatus	filesStatus = IllImageFilesStatus.Ready;

		public event PropertyChangedEventHandler PropertyChanged;


		#region constructor
        public IllPage(IllImage illImage, BscanILLData.Models.DbPage dbPage, bool createOcrXml)
		{
			this.illImage = illImage;
			this.dbPage = dbPage;
			this.origFile = new FileInfo(Path.Combine(this.illImage.Article.PagesPath, this.dbPage.FileName));
			this.fullImageInfo = new ImageProcessing.ImageFile.ImageInfo(this.origFile.FullName);
			this.origDpi = Convert.ToInt16(this.fullImageInfo.DpiH);
			this.colorMode = ( Scanners.ColorMode)this.dbPage.ColorMode;

			this.brightnessOrig = 0;
			this.contrastOrig = 0;
			this.dpi = this.dbPage.Dpi;

            BscanILL.SETTINGS.Settings _settings = BscanILL.SETTINGS.Settings.Instance;

            //if (BscanILL.SETTINGS.Settings.Instance.Licensing.OcrEnabled)
            if ( _settings.Licensing.OcrEnabled && createOcrXml )
            {
                int quality = 100;
				this.illPdf = new IllPdf(this);

                if(this.illImage.Article.ExportType == Export.ExportType.Email)
                {
                    quality = _settings.Export.Email.FileExportQuality;
                }
                else
                if (this.illImage.Article.ExportType == Export.ExportType.SaveOnDisk)
                {
                    quality = _settings.Export.SaveOnDisk.FileExportQuality;
                }
                else
                if (this.illImage.Article.ExportType == Export.ExportType.Ftp)
                {
                    quality = _settings.Export.FtpServer.FileExportQuality;
                }
                else
                if (this.illImage.Article.ExportType == Export.ExportType.FtpDir)
                {
                    quality = _settings.Export.FtpDirectory.FileExportQuality;
                }
                else
                if (this.illImage.Article.ExportType == Export.ExportType.Odyssey)
                {
                    quality = _settings.Export.Odyssey.FileExportQuality;
                }
                else
                if (this.illImage.Article.ExportType == Export.ExportType.ArticleExchange)
                {
                    quality = _settings.Export.ArticleExchange.FileExportQuality;
                }
                else
                if (this.illImage.Article.ExportType == Export.ExportType.Tipasa)
                {
                    quality = _settings.Export.Tipasa.FileExportQuality;
                }
                else
                if (this.illImage.Article.ExportType == Export.ExportType.WorldShareILL)
                {
                    quality = _settings.Export.WorldShareILL.FileExportQuality;
                }

                this.illPdf.CreatePdfFile(this.colorMode , quality);
			}
		}
		#endregion

		#region destructor
		public void Dispose()
		{
			Delete(0);
		}
		#endregion

		#region Delete()
		public void Delete()
		{
			Delete(0);
		}
		#endregion


		#region class PdfSettings
		public class PdfSettings
		{
			public readonly ushort Dpi;
			public readonly Scanners.ColorMode ColorMode;
			public readonly short BrightnessDelta;
			public readonly short ContrastDelta;

			public PdfSettings(ushort dpi, Scanners.ColorMode colorMode, short brightnessDelta, short contrastDelta)
			{
				this.Dpi = dpi;
				this.ColorMode = colorMode;
				this.BrightnessDelta = brightnessDelta;
				this.ContrastDelta = contrastDelta;
			}
		}
		#endregion

		#region enum FileStatus
		public enum FileStatus
		{
			WaitingToStart,
			Creating,
			Created,
			Error
		}
		#endregion

		#region enum IllImageFilesStatus
		public enum IllImageFilesStatus
		{
			Ready,
			Deleting,
			Deleted
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		
		//public User					User { get { return this.session.Category.User; } }
		public Article					Article { get { return this.illImage.Article; } }
		public IllImage					IllImage { get { return this.illImage; } }
		public FileInfo					FilePath { get { return this.origFile; } }
		public Scanners.ColorMode		ColorMode { get { return this.colorMode; } }
		public double					BrightnessOrig { get { return this.brightnessOrig; } }
		public double					ContrastOrig { get { return this.contrastOrig; } }
		//public Bitmap			Thumbnail { get { return thumbnail; } }
		//public BscanILL.DSUsersSingle.ScansRow ImageDB { get { return this.imageDb; } }
		public object					BitmapDataLocker { get { return bitmapDataLocker; } }
		public short					Dpi { get { return this.dpi; } }
		public short					ScanDpi { get { return this.origDpi; } }
		public string					Name { get { return Path.GetFileNameWithoutExtension(this.origFile.Name); } }

		public ImageProcessing.ImageFile.ImageInfo	FullImageInfo { get { return this.fullImageInfo; } }

		public BscanILLData.Models.DbPage			DbPage { get { return this.dbPage; } }

		#region Selected
		public bool Selected
		{
			get { return this.selected; }
			set
			{
				if (this.selected != value)
				{
					this.selected = value;

					if (this.selected)
						this.ImageSelected(this);
					else
					{
					}
				}
			}
		}
		#endregion

		//pdf
		public FileInfo					FormatXmlFile { get { return (this.illPdf != null) ? this.illPdf.FormatXmlFile : null; } }
        public DirectoryInfo            FormatXmlFileDir { get { return (this.illPdf != null) ? this.illPdf.FormatXmlFileDir : null; } }

        public Scanners.ColorMode       ColorDepthOfPreProcessedPDF { get { return (this.illPdf != null) ? this.illPdf.ColorDepthOfPreProcessedPDF : Scanners.ColorMode.Unknown; } }
        public int                      ExportQualityOfPreProcessedPDF { get { return (this.illPdf != null) ? this.illPdf.ExportQualityOfPreProcessedPDF : 0; } }
        public long                     SourceFileSize { get { return (this.illPdf != null) ? this.illPdf.SourceFileSize : 0; } }

        public object					DeleteFilesLocker { get { return this.deleteFilesLocker; } }
		public IllImageFilesStatus		FilesStatus { get { return this.filesStatus; } set { this.filesStatus = value; } }

		#region Status
		public PageStatus Status
		{
			get { return (PageStatus)dbPage.Status; }
			set
			{
				if (this.dbPage.Status != (BscanILLData.Models.PageStatus)value)
				{
					BscanILL.DB.Database.Instance.ChangePageStatus(this.dbPage, value);
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
        #endregion

        #endregion


        //PUBLIC METHODS
        #region public methods

        #region ClearFormatXmlFileDir
        public void ClearFormatXmlFileDir()
        {
            if(this.illPdf != null)
            {
                this.illPdf.ClearFormatXmlFileDir();
            }

        }
        #endregion

        #region FormattedDirCreated
        public void FormattedDirCreated(DirectoryInfo formattedFileDir, Scanners.ColorMode colorDepth, int exportQuality, long imageFileSize)
        {
            if (this.illPdf != null)
            {
                this.illPdf.FormattedDirCreated(formattedFileDir , colorDepth, exportQuality, imageFileSize);
            }
        }
        #endregion

        #region Rotate()
        public void Rotate(int angle)
		{
			try
			{
				if (ExportBitmapChanging != null)
					ExportBitmapChanging(this);

				//DeleteExportFiles();

				FileInfo tempFile = new FileInfo(this.origFile.FullName + ".tmp");                

				if (tempFile.Exists)
					tempFile.Delete();

				lock (this.BitmapDataLocker)
				{
					using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(this.origFile.FullName))
					{
                        ImageProcessing.BigImages.RotationFlipping rotation = new ImageProcessing.BigImages.RotationFlipping();

						if (angle == 180)
							rotation.Go(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg(85), RotateFlipType.Rotate180FlipNone);
						else if (angle == 270)
							rotation.Go(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg(85), RotateFlipType.Rotate270FlipNone);
						else
							rotation.Go(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg(85), RotateFlipType.Rotate90FlipNone);
					}

					if(this.illPdf != null)
						this.illPdf.Delete();

					if (this.origFile.Exists)
						this.origFile.Delete();

					tempFile.MoveTo(this.origFile.FullName);

					this.fullImageInfo = new ImageProcessing.ImageFile.ImageInfo(this.origFile.FullName);

					if (this.illPdf != null)
						this.illPdf.CreatePdfFile(this.ColorMode, 85);
				}

				if (ExportBitmapChanged != null)
					ExportBitmapChanged(this);

				if (SettingsChanged != null)
					SettingsChanged(this);
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "IllPage, Rotate(): " + ex.Message, ex);
			}
		}
		#endregion

		#region StopAllAutoProcessing()
		public void StopAllAutoProcessing()
		{
			this.illPdf.CancelPdfFileCreation();
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region DeleteExportFiles()
		/*private void DeleteExportFiles()
		{
			try
			{
				FileInfo[] files = origFile.Directory.GetFiles(Path.GetFileNameWithoutExtension(this.origFile.Name) + ".*");

				foreach (FileInfo file in files)
					if (file.Exists && file.Extension != ".bscanill" && file.Extension != ".thm")
						file.Delete();
			}
			catch (Exception ex)
			{
				BscanILL.Misc.Notifications.Instance.Notify(this, BscanILL.Misc.Notifications.Type.Error, "IllPage, DeleteExportImages(): " + ex.Message, ex);
			}
		}*/
		#endregion

		#region Delete()
		private void Delete(int attempts)
		{
			try
			{
				lock (deleteFilesLocker)
				{
					FileInfo[] files = origFile.Directory.GetFiles(Path.GetFileNameWithoutExtension(this.origFile.Name) + ".*");
					foreach (FileInfo file in files)
						if (file.Exists)
							file.Delete();

					if (this.illPdf != null)
						this.illPdf.Delete();

					this.FilesStatus = IllPage.IllImageFilesStatus.Deleted;
				}
			}
			catch (Exception ex)
			{
				if (attempts > 5)
					throw new Exception("Can't delete filePath: " + ex.Message);
				else
				{
					Thread.Sleep(attempts * 200 + 200);
					Delete(attempts + 1);
				}
			}
		}
		#endregion

		#region GetPixelFormat()
		/*private static System.Windows.Media.PixelFormat GetPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
		{
			switch (pixelFormat)
			{
				case PixelFormat.Format1bppIndexed: return System.Windows.Media.PixelFormats.Indexed1;
				case PixelFormat.Format4bppIndexed: return System.Windows.Media.PixelFormats.Gray4;
				case PixelFormat.Format8bppIndexed: return System.Windows.Media.PixelFormats.Gray8;
				case PixelFormat.Format32bppArgb: return System.Windows.Media.PixelFormats.Bgra32;
				case PixelFormat.Format32bppRgb: return System.Windows.Media.PixelFormats.Bgr32;
				default: return System.Windows.Media.PixelFormats.Bgr24;
			}
		}*/
		#endregion

		#region DealWithSettingsChange()
		/*private void DealWithSettingsChange()
		{
			if (ExportBitmapChanging != null)
				ExportBitmapChanging(this);

			lock (this.BitmapDataLocker)
			{
				DeleteExportFiles();
			}

			if (ThumbnailChanged != null)
				ThumbnailChanged(this);

			if (ExportBitmapChanged != null)
				ExportBitmapChanged(this);

			if (SettingsChanged != null)
				SettingsChanged(this);
		}*/
		#endregion

		#region RaisePropertyChanged()
		private void RaisePropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				if (propertyName.StartsWith("get_") || propertyName.StartsWith("set_"))
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName.Substring(4)));
				else
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion

		#endregion

	}
}
