using System;
using System.Linq;
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
using BscanILL.DB;


namespace BscanILL.Hierarchy
{
	/// <summary>
	/// Summary description for IllImage.
	/// </summary>
	public class IllImage : INotifyPropertyChanged
	{
		Article article;

		ImageProcessing.ImageFile.ImageInfo fullImageInfo;

		FileInfo		origFile = null;
		bool			selected = false;
		//BscanILL.DSUsersSingle.ScansRow imageDb;
		BscanILLData.Models.DbScan		dbScan = null;

		public object bitmapDataLocker = new object();

		//image properties
		 Scanners.ColorMode colorMode;
		ushort origDpi;
		ushort dpi;
		double brightnessOrig;
		double contrastOrig;
		short brightnessDelta = 0;
		short contrastDelta = 0;

		//Size thumbnailSize = new Size(129, 98);
		Size thumbnailSize = new Size(125, 113);

		//events
		public event BscanILL.Hierarchy.IllImageHnd ImageSelected;
		public event BscanILL.Hierarchy.IllImageHnd ThumbnailChanged;
		public event BscanILL.Hierarchy.IllImageHnd ExportBitmapChanging;
		public event BscanILL.Hierarchy.IllImageHnd ExportBitmapChanged;
		public event BscanILL.Hierarchy.IllImageHnd SettingsChanged;

		volatile object					deleteFilesLocker = new object();
		volatile IllImageFilesStatus	filesStatus = IllImageFilesStatus.Ready;

		IllPages						illPages = new IllPages();
		
		public event PropertyChangedEventHandler PropertyChanged;

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);


		#region constructor
		/// <summary>
		/// 
		/// </summary>
		/// <param name="article"></param>
		/// <param name="origFile"></param>
		/// <param name="colorMode"></param>
		/// <param name="dpi"></param>
		/// <param name="brightness">from -1 to 1</param>
		/// <param name="contrast">from -1 to 1</param>
		/*public IllImage(Article article, FileInfo origFile, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast)
		{
			this.article = article;
			this.origFile = origFile;
			this.fullImageInfo = new ImageProcessing.ImageFile.ImageInfo(this.origFile.FullName);
			this.origDpi = Convert.ToInt16(this.fullImageInfo.DpiH);
			this.colorMode = colorMode;

			this.brightnessOrig = brightness;
			this.contrastOrig = contrast;
			this.dpi = dpi;

			//this.imageDb = this.User.ImageInserted(this.ColorMode, this.Dpi, origFile.Length);
			this.histogram = new ImageProcessing.Histogram(this.origFile.FullName);
			//BuildThumbnail();

			if (ThumbnailChanged != null)
				ThumbnailChanged(this);
		}*/

		public IllImage(Article article, BscanILLData.Models.DbScan dbScan )
		{
			this.article = article;
			this.dbScan = dbScan;
			this.origFile = new FileInfo(Path.Combine(this.article.ScansPath, this.dbScan.FileName));
			this.fullImageInfo = new ImageProcessing.ImageFile.ImageInfo(this.origFile.FullName);
			this.origDpi = Convert.ToUInt16(this.fullImageInfo.DpiH);
			this.colorMode = ( Scanners.ColorMode)this.dbScan.ColorMode;

			this.brightnessOrig = 0;
			this.contrastOrig = 0;
			this.dpi = (ushort)this.dbScan.Dpi;

			foreach (BscanILLData.Models.DbPage dbPage in database.GetActiveDbPages(this.DbScan))
			{
				try
				{
                    this.illPages.Add(new IllPage(this, dbPage, false));        //we want to run ocr (creation of Abbyy xml) in IllPdf.cs just when going from CleanUp to Send tab... we need to set flag to skip ocr
                    //this.illPages.Add(new IllPage(this, dbPage, true));           //cannot pass 'false' as last param as it crashes Abbyy when exiting BscanILL
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, ex.Message, ex);
				}
			}
			
			if (ThumbnailChanged != null)
				ThumbnailChanged(this);
		}
		#endregion

		#region destructor
		public void Dispose()
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
		public Article					Article { get { return this.article; } }
		public FileInfo					FilePath { get { return this.origFile; } }
		public Scanners.ColorMode		ColorMode { get { return this.colorMode; } }
		public byte						Brightness { get { return (byte)(this.brightnessOrig + this.brightnessDelta); } }
		public byte						Contrast { get { return (byte)(this.contrastOrig + this.contrastDelta); } }
		//public Bitmap			Thumbnail { get { return thumbnail; } }
		//public BscanILL.DSUsersSingle.ScansRow ImageDB { get { return this.imageDb; } }
		public object					BitmapDataLocker { get { return bitmapDataLocker; } }
		public ushort					Dpi { get { return this.dpi; } }
		public ushort					ScanDpi { get { return this.origDpi; } }
		public string					Name { get { return Path.GetFileNameWithoutExtension(this.origFile.Name); } }

		public double								BrightnessWpf { get { return this.Brightness / 255.0; } }
		public double								ContrastWpf { get { return this.Contrast / 255.0; } }
		public short								BrightnessDelta { get { return this.brightnessDelta; } }
		public short								ContrastDelta { get { return this.contrastDelta; } }
		public ImageProcessing.ImageFile.ImageInfo	FullImageInfo { get { return this.fullImageInfo; } }

		public BscanILLData.Models.DbScan	DbScan		{ get { return this.dbScan; } }
		public IllPages						IllPages	{ get { return this.illPages; } }

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
		public object					DeleteFilesLocker { get { return this.deleteFilesLocker; } }
		public IllImageFilesStatus		FilesStatus { get { return this.filesStatus; } set { this.filesStatus = value; } }

		public bool						IsPullslip { get { return this.Article.Pullslip == this; } }

		#region Status
		public ScanStatus Status
		{
			get { return (ScanStatus)dbScan.Status; }
			set
			{
				if (this.dbScan.Status != (BscanILLData.Models.ScanStatus)value)
				{
					BscanILL.DB.Database.Instance.ChangeScanStatus(this.dbScan, value);						
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#endregion


		// PRIVATE PROPERTIES
		#region private properties
		BscanILL.DB.Database		database { get { return BscanILL.DB.Database.Instance; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

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

						if (itDecoder.PixelFormat == PixelFormat.Format1bppIndexed)
                        {
							if (angle == 180)
								rotation.Go(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), RotateFlipType.Rotate180FlipNone);
							else if (angle == 270)
								rotation.Go(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), RotateFlipType.Rotate270FlipNone);
							else
								rotation.Go(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), RotateFlipType.Rotate90FlipNone);
						}
						else
						{
							if (angle == 180)
								rotation.Go(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg(85), RotateFlipType.Rotate180FlipNone);
							else if (angle == 270)
								rotation.Go(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg(85), RotateFlipType.Rotate270FlipNone);
							else
								rotation.Go(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg(85), RotateFlipType.Rotate90FlipNone);
						}
					}

					if (this.origFile.Exists)
						this.origFile.Delete();

					tempFile.MoveTo(this.origFile.FullName);

					this.fullImageInfo = new ImageProcessing.ImageFile.ImageInfo(this.origFile.FullName);
				}

				if (ThumbnailChanged != null)
					ThumbnailChanged(this);

				if (ExportBitmapChanged != null)
					ExportBitmapChanged(this);

				if (SettingsChanged != null)
					SettingsChanged(this);
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "IllImage, Rotate(): " + ex.Message, ex);
			}
		}
		#endregion

        #region RotateSmallAngle()
        public void RotateSmallAngle(int angle)
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
                        byte r, g, b;
                        double imagWidthHightRatio = 1 ;
                        double imageWidth = this.fullImageInfo.Width;
                        double imageHeight = this.fullImageInfo.Height ;
                        
                        r = g = b = 0;  //black

                        //ImageProcessing.BigImages.RotationFlipping rotation = new ImageProcessing.BigImages.RotationFlipping();
                        ImageProcessing.BigImages.Rotation rotation = new ImageProcessing.BigImages.Rotation();

                        rotation.RotateClip(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg(85), new ImageProcessing.IpSettings.Clip(0, 0, imageWidth - 1, imageHeight - 1, imagWidthHightRatio), r, g, b);

                    }

                    if (this.origFile.Exists)
                        this.origFile.Delete();

                    tempFile.MoveTo(this.origFile.FullName);

                    this.fullImageInfo = new ImageProcessing.ImageFile.ImageInfo(this.origFile.FullName);
                }

                if (ThumbnailChanged != null)
                    ThumbnailChanged(this);

                if (ExportBitmapChanged != null)
                    ExportBitmapChanged(this);

                if (SettingsChanged != null)
                    SettingsChanged(this);
            }
            catch (Exception ex)
            {
                Notifications.Instance.Notify(this, Notifications.Type.Error, "IllImage, RotateSmallAngle(): " + ex.Message, ex);
            }
        }
        #endregion

		#region DeletePages()
		public void DeletePages(bool deleteOnBackground)
		{
			this.IllPages.Clear(deleteOnBackground);
		}
		#endregion

		#region StopAllAutoProcessing()
		public void StopAllAutoProcessing()
		{
			foreach (IllPage illPage in this.IllPages)
				illPage.StopAllAutoProcessing();
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
				BscanILL.Misc.Notifications.Instance.Notify(this, BscanILL.Misc.Notifications.Type.Error, "IllImage, DeleteExportImages(): " + ex.Message, ex);
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

					this.FilesStatus = IllImage.IllImageFilesStatus.Deleted;
					this.Status = ScanStatus.Deleted;
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
