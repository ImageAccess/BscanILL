using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
//using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
//using ImageProcessing;
//using ImageProcessing.ImageFile;
using Microsoft.Win32.SafeHandles;
using ViewPane.Thumbnails;



namespace ViewPane.Hierarchy
{
	/// <summary>
	/// Summary description for Picture.
	/// </summary>
	public class VpImage : ViewPane.ImagePanel.IViewImage
	{
		#region variables
		ViewPane.IP.PreviewCreator				creator = ViewPane.IP.PreviewCreator.Instance;
		ImageProcessing.IpSettings.ItImage		itImage = null;
		ImageProcessing.PostProcessing			postProcessing = new ImageProcessing.PostProcessing();
		ViewPane.IT.BookPartType				bookPart = IT.BookPartType.Unknown;
		string									toolTip = "";
		object									tag = null;

		VpImageType		imageType;
        bool            isPullSlip = false;
		bool			itActive = false;
		//bool			itChangedSinceLastSave = false;

		static object locker = new object ();
		static object createLocker = new object ();

		string fullPath;
		string reducedPath;
		string previewPath;
		string thumbnailPath;

		bool	recreateReducedImage = false;
		bool	recreatePreview = false;
		bool	recreateThumbnail = false;


		ImageProcessing.ImageFile.ImageInfo fullInfo = null;
		ImageProcessing.ImageFile.ImageInfo reducedInfo = null;
		ImageProcessing.ImageFile.ImageInfo previewInfo = null;
		ImageProcessing.ImageFile.ImageInfo thumbInfo = null;

		ManualResetEvent filesCreatedEvent = new ManualResetEvent (false);

		public delegate void VoidEventHnd ();
		public delegate void VpImageEventHnd (VpImage vpImage);

		public event VoidEventHnd		ItImageSettingsChanged;
		public event VoidEventHnd		ItImageChanging;
		public event VoidEventHnd		ItImageChanged;
		public event VoidEventHnd		PostProcessingChanging;
		public event VoidEventHnd		PostProcessingChanged;
		public event VoidEventHnd		Updated;
		public event VoidEventHnd		SelectRequested;
		public event VoidEventHnd		BringIntoViewRequested;
		public event VpImageEventHnd	UpdateAfterRotate;

		#endregion variables


		#region constructor
		public VpImage(string fullPath, string reducedPath, string previewPath, string thumbnailPath, bool itActive, bool recreateReducedImages )
		{
			if (File.Exists(fullPath) == false)
				throw new IOException(ViewPane.Languages.UiStrings.ImageFilePath_STR + " '" + fullPath + "' " + ViewPane.Languages.UiStrings.IsInvalid_STR);

			this.imageType = VpImageType.ScanImage;
			this.itActive = itActive;

            if (!itActive)
            {
                this.isPullSlip = true;  //flag itActive is set false for pull slips... in BscanILL we can use flag vpfimage.tag -> In case of Scan frame if that stores ILLImage in that object variable (.tag) and there is property IsPullSlip too
                                         // but in ViewPane project, the BscanILL.Hierarchy.illimage is not known so I cannout access the IsPullSlip tag in illimage object stored in tag variable of IpImage
            }

			this.fullPath = fullPath;
			this.reducedPath = reducedPath;
			this.previewPath = previewPath;
			this.thumbnailPath = thumbnailPath;

			this.recreateReducedImage = recreateReducedImages;
			this.recreatePreview = recreateReducedImages;
			this.recreateThumbnail = recreateReducedImages;

			CreateAllFiles ();
		}
		#endregion constructor


		#region VpImageType
		public enum VpImageType
		{
			ScanImage,
			ItImage
		}
		#endregion VpImageType


		//PUBLIC PROPERTIES
		#region public properties

		public bool								TwoPages { get { return (this.ItImage != null) ? this.ItImage.TwoPages : false; } }
		public VpImageType						ImageType { get { return this.imageType; } }
		public ViewPane.IT.BookPartType			BookPart { get { return bookPart; } set { this.bookPart = value; } }
		public string							ToolTip { get { return this.toolTip; } set { this.toolTip = value; } }
		public object							Tag { get { return this.tag; } set { this.tag = value; } }

		public bool								IsItActive { get { return this.itActive; } }

        #region IsPullSlip
        public bool IsPullSlip
        {
            get { return this.isPullSlip ; }
			set
			{
                this.isPullSlip = value;
			}
        }        
        #endregion

		#region IsFixed
		public bool IsFixed
		{
			get { return this.IsItActive == false || this.ItImage == null || this.ItImage.IsFixed; }
			set
			{
				if (this.ItImage != null)
					this.ItImage.IsFixed = value;
			}
		}
		#endregion IsFixed

		#region IsIndependent
		public bool IsIndependent
		{
			get
			{
				if (this.IsItActive == false)
					return true;
				return (this.ItImage == null || this.ItImage.IsIndependent);
			}
			set
			{
				if (this.ItImage != null)
					this.ItImage.IsIndependent = value;
			}
		}
		#endregion IsIndependent

		#region FullImageInfo
		public ImageProcessing.ImageFile.ImageInfo FullImageInfo
		{
			get
			{
				this.filesCreatedEvent.WaitOne ();
				return this.fullInfo;
			}
		}
		#endregion FullImageInfo

		#region FullImageSize
		public System.Windows.Size FullImageSize
		{
			get
			{
				this.filesCreatedEvent.WaitOne ();
				return new System.Windows.Size (this.fullInfo.Width, this.fullInfo.Height);
			}
		}
		#endregion FullImageSize

		#region Confidence
		public float Confidence
		{
			get
			{
				double confidence = 1.0;

				if (this.itActive)
				{
					confidence = (confidence < this.ItImage.PageL.Clip.ClipConfidence) ? confidence : this.ItImage.PageL.Clip.ClipConfidence;
					confidence = (confidence < this.ItImage.PageL.Bookfolding.Confidence) ? confidence : this.ItImage.PageL.Bookfolding.Confidence;
					confidence = (confidence < this.ItImage.PageL.Clip.SkewConfidence) ? confidence : this.ItImage.PageL.Clip.SkewConfidence;

					foreach (ImageProcessing.IpSettings.Finger artifact in this.ItImage.PageL.Fingers)
						confidence = (confidence < artifact.Confidence) ? confidence : artifact.Confidence;

					if (TwoPages)
					{
						confidence = (confidence < this.ItImage.PageR.Clip.ClipConfidence) ? confidence : this.ItImage.PageR.Clip.ClipConfidence;
						confidence = (confidence < this.ItImage.PageR.Bookfolding.Confidence) ? confidence : this.ItImage.PageR.Bookfolding.Confidence;
						confidence = (confidence < this.ItImage.PageR.Clip.SkewConfidence) ? confidence : this.ItImage.PageR.Clip.SkewConfidence;

						foreach (ImageProcessing.IpSettings.Finger artifact in this.ItImage.PageR.Fingers)
							confidence = (confidence < artifact.Confidence) ? confidence : artifact.Confidence;
					}
				}

				return (float)confidence;
			}
		}
		#endregion Confidence

		#region FullPath
		public string FullPath
		{
			get
			{
				return this.fullPath;
			}
		}
		#endregion FullPath

		#region ReducedPath
		public string ReducedPath
		{
			get
			{
				//if (File.Exists(this.thumbnailPath) == false)
				this.filesCreatedEvent.WaitOne ();

				return this.reducedPath;
			}
		}
		#endregion ReducedPath

		#region PreviewPath
		public string PreviewPath
		{
			get
			{
				//if (File.Exists(this.thumbnailPath) == false)
				this.filesCreatedEvent.WaitOne();

				return this.previewPath;
			}
		}
		#endregion

		#region ThumbnailPath
		public string ThumbnailPath
		{
			get
			{
				if (File.Exists (this.thumbnailPath) == false)
					this.filesCreatedEvent.WaitOne ();

				return this.thumbnailPath;
			}
		}
		#endregion ThumbnailPath

		#region ItImage
		public ImageProcessing.IpSettings.ItImage ItImage 
		{
			get 
			{
				if (this.itActive && this.itImage == null)
				{
					string path = this.ReducedPath;
					bool twoPages = fullInfo.Width > fullInfo.Height;
					this.ItImage = new ImageProcessing.IpSettings.ItImage(new FileInfo(path), twoPages);
				}

				return this.itImage; 
			}
			set
			{
				if (this.itImage != value)
				{
					if (this.ItImageChanging != null)
						this.ItImageChanging();

					if (this.itImage != null)
					{
						this.itImage.Changed += new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);
					} 
					
					this.itImage = value;

					if (this.itImage != null)
					{
						this.itImage.Changed += new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);
					}

					if (this.ItImageChanged != null)
						this.ItImageChanged();
				}
			}
		}
		#endregion

		#region PostProcessing
		public ImageProcessing.PostProcessing PostProcessing
		{
			get { return this.postProcessing; }
			set
			{
				if (this.postProcessing != value)
				{
					if (this.PostProcessingChanging != null)
						this.PostProcessingChanging();

					this.postProcessing = value;

					if (this.PostProcessingChanged != null)
						this.PostProcessingChanged();
				}
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region RecreateImageFiles()
		public void RecreateImageFiles()
        {			
			if (File.Exists(this.ReducedPath))
				File.Delete(this.ReducedPath);
			
			if (File.Exists(this.PreviewPath))
				File.Delete(this.PreviewPath);

			if (File.Exists(this.ThumbnailPath))
				File.Delete(this.ThumbnailPath);

			CreateAllFiles();
		}
        #endregion

        #region Dispose()
        public void Dispose ()
		{
			this.filesCreatedEvent.WaitOne ();
			ReleaseBitmaps ();
		}
		#endregion Dispose()

		#region ReleaseBitmaps()
		public void ReleaseBitmaps ()
		{
			//GC.Collect();
		}
		#endregion ReleaseBitmaps()

		#region GetBitmap()
		public Bitmap GetBitmap (int width, int height)
		{
			lock (locker)
			{
				this.filesCreatedEvent.WaitOne ();

				double fullZoom = Math.Min (width / (double)this.fullInfo.Width, height / (double)this.fullInfo.Height);
				double reducedZoom = Math.Min (width / (double)this.reducedInfo.Width, height / (double)this.reducedInfo.Height);
				double previewZoom = Math.Min (width / (double)this.previewInfo.Width, height / (double)this.previewInfo.Height);
				double thumbZoom = Math.Min (width / (double)this.thumbInfo.Width, height / (double)this.thumbInfo.Height);

				if (thumbZoom == 1 || thumbZoom <= 0.5)
					return creator.GetPreview (new FileInfo (this.thumbnailPath), thumbZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else if (previewZoom == 1 || previewZoom <= 0.5)
					return creator.GetPreview (new FileInfo (this.previewPath), previewZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else if (reducedZoom == 1 || reducedZoom <= 0.5)
					return creator.GetPreview (new FileInfo (this.reducedPath), reducedZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else
					return creator.GetPreview (new FileInfo (this.fullPath), fullZoom, ImageProcessing.Resizing.ResizeMode.Quality);
			}
		}

		public Bitmap GetBitmap (int width, int height, BIP.Geometry.RatioRect imageRect)
		{
			lock (locker)
			{
				this.filesCreatedEvent.WaitOne ();

				double fullZoom = Math.Min (width / (imageRect.Width * this.fullInfo.Width), height / (imageRect.Height * this.fullInfo.Height));
				double reducedZoom = Math.Min (width / (double)(imageRect.Width * this.reducedInfo.Width), height / (double)(imageRect.Height * this.reducedInfo.Height));
				double previewZoom = Math.Min (width / (imageRect.Width * this.previewInfo.Width), height / (imageRect.Height * this.previewInfo.Height));
				double thumbZoom = Math.Min (width / (imageRect.Width * this.thumbInfo.Width), height / (imageRect.Height * this.thumbInfo.Height));

				if (thumbZoom == 1 || thumbZoom <= 0.5)
					return creator.GetPreview (new FileInfo (this.thumbnailPath), imageRect, thumbZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else if (previewZoom == 1 || previewZoom <= 0.5)
					return creator.GetPreview (new FileInfo (this.previewPath), imageRect, previewZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else if (reducedZoom == 1 || reducedZoom <= 0.5)
					return creator.GetPreview (new FileInfo (this.reducedPath), imageRect, reducedZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else
					return creator.GetPreview (new FileInfo (this.fullPath), imageRect, fullZoom, ImageProcessing.Resizing.ResizeMode.Quality);
			}
		}
		#endregion GetBitmap()

		#region GetBitmapAsync()
		public void GetBitmapAsync (ViewPane.IP.IPreviewCaller caller, int renderingId, bool highPriority, int width, int height)
		{
			lock (locker)
			{
				this.filesCreatedEvent.WaitOne ();

				double fullZoom = Math.Min (width / (double)this.fullInfo.Width, height / (double)this.fullInfo.Height);
				double reducedZoom = Math.Min (width / (double)this.reducedInfo.Width, height / (double)this.reducedInfo.Height);
				double previewZoom = Math.Min (width / (double)this.previewInfo.Width, height / (double)this.previewInfo.Height);

				if (previewZoom <= 0.5)
					creator.GetPreviewAsync (caller, renderingId, highPriority, new FileInfo (this.previewPath), previewZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else if (reducedZoom <= 0.5)
					creator.GetPreviewAsync (caller, renderingId, highPriority, new FileInfo (this.reducedPath), reducedZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else
					creator.GetPreviewAsync (caller, renderingId, highPriority, new FileInfo (this.fullPath), fullZoom, ImageProcessing.Resizing.ResizeMode.Quality);
			}
		}

		public void GetBitmapAsync(ViewPane.IP.IPreviewCaller caller, int renderingId, bool highPriority, BIP.Geometry.RatioRect imageRect, int width, int height)
		{
			lock (locker)
			{
				this.filesCreatedEvent.WaitOne ();

				double fullZoom = Math.Min (width / (imageRect.Width * this.fullInfo.Width), height / (imageRect.Height * this.fullInfo.Height));
				double reducedZoom = Math.Min (width / (imageRect.Width * this.reducedInfo.Width), height / (imageRect.Height * this.reducedInfo.Height));
				double previewZoom = Math.Min (width / (double)(imageRect.Width * this.previewInfo.Width), height / (double)(imageRect.Height * this.previewInfo.Height));

				if (previewZoom <= 0.5)
					creator.GetPreviewAsync (caller, renderingId, highPriority, new FileInfo (this.previewPath), imageRect, previewZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else if (reducedZoom <= 0.5)
					creator.GetPreviewAsync (caller, renderingId, highPriority, new FileInfo (this.reducedPath), imageRect, reducedZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else
					creator.GetPreviewAsync (caller, renderingId, highPriority, new FileInfo (this.fullPath), imageRect, fullZoom, ImageProcessing.Resizing.ResizeMode.Quality);
			}
		}
		#endregion GetBitmapAsync()

		#region Update()
		public void Update()
		{
		}
		 
		/*public void Update (OpusObjectManagerNS.FullScanData scanData)
		{
			lock (locker)
			{
				if (this.ImageType == VpImage.VpImageType.ItImage)
				{
					OpusObjectManagerNS.ItScanImageData itScanImageData = this.Wrapper.GetItScanImage (scanData);

					List<OpusObjectManagerNS.ItImageData> itImageDataList = itScanImageData.GetItImages (this.Wrapper);

					foreach (OpusObjectManagerNS.ItImageData itImageDataItem in itImageDataList)
						if (itImageDataItem.fItPageID == this.ItImageData.fItPageID)
						{
							Update (itImageDataItem);
							break;
						}
				}
				else
				{
					this.ScanData = scanData;

					if (this.ScanData != null && this.ItImage != null)
					{
						OpusObjectManagerNS.ItScanImageData itScanImage = this.Wrapper.GetItScanImage (scanData);
						if (itScanImage == null)
						{
							OpusObjectManagerNS.ItSet itSet = GetDefaultItSet ();
							itScanImage = this.Wrapper.CreateItScanImage (scanData, itSet);
						}

						ItImageItSettingsConverter.ConvertItSettingsToItImage (this.ScanData, this.ItImage, itScanImage);
					}

					Refresh ();
				}

				if (this.Updated != null)
					this.Updated ();
			}
		}*/

		/*public void Update (OpusObjectManagerNS.ItImageData itImageData)
		{
			lock (locker)
			{
				this.itImageData = itImageData;
				Refresh ();
			}
		}*/
		#endregion Update()

		#region MakeItActive
		/*public void MakeItActive()
		{
			lock (this)
			{
				this.itActive = true;
				ConstructItImage();
			}
		}*/
		#endregion MakeItActive

		#region ReloadIt
		/*public void ReloadIt ()
		{
			lock (this)
			{
				this.itActive = true;

				ConstructItImage ();
			}
		}*/
		#endregion ReloadIt

		#region Select()
		public void Select ()
		{
			if (this.SelectRequested != null)
				this.SelectRequested ();
		}
		#endregion Select()

		#region BringIntoView()
		public void BringIntoView ()
		{
			if (this.BringIntoViewRequested != null)
				this.BringIntoViewRequested ();
		}
		#endregion BringIntoView()

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Refresh()
		/*private void Refresh ()
		{
			lock (locker)
			{
				this.filesCreatedEvent.Reset ();
				CreateAllFiles ();

				if (this.IsItActive)
					ReloadIt ();
			}
		}*/
		#endregion

		#region CreateAllFiles()
		private void CreateAllFiles()
		{
			Thread t = new Thread (new ThreadStart (CreateAllFilesThread));
			t.Name = "VpImage, CreateAllFiles()";
			t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			t.SetApartmentState (ApartmentState.STA);
			t.Start ();
		}
		#endregion CreateAllFiles()

		#region CreateAllFilesThread()
		private void CreateAllFilesThread ()
		{
			try
			{
				lock (createLocker)
				{
					CreateReducedImage();
					CreatePreviewImage();
					CreateThumbnail();

					this.fullInfo = new ImageProcessing.ImageFile.ImageInfo(this.fullPath);
					this.reducedInfo = new ImageProcessing.ImageFile.ImageInfo(this.reducedPath);
					this.previewInfo = new ImageProcessing.ImageFile.ImageInfo(this.previewPath);
					this.thumbInfo = new ImageProcessing.ImageFile.ImageInfo(this.thumbnailPath);
				}
			}
#if DEBUG
			catch (Exception ex)
			{
				Console.WriteLine("ERROR in VpImage, CreateAllFilesThread(): " + ViewPane.Misc.Misc.GetErrorMessage(ex));
				//Notifications.Instance.Notify(this, Notifications.Type.Error, "KicPreview, CreatePreviewFileThread(): " + ex.Message, ex);
			}
#else
			catch 
			{
				//Notifications.Instance.Notify(this, Notifications.Type.Error, "KicPreview, CreatePreviewFileThread(): " + ex.Message, ex);
			}
#endif
			finally
			{
				filesCreatedEvent.Set ();

				if (Updated != null)
					Updated ();

				if (UpdateAfterRotate != null)
				{
					UpdateAfterRotate(this);
				}
			}
		}
		#endregion 

		#region CreateReducedImage()
		private void CreateReducedImage ()
		{
			if (this.recreateReducedImage || File.Exists(this.reducedPath) == false)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(this.reducedPath));
				
				if (File.Exists(this.reducedPath))
					File.Delete(this.reducedPath);

				if (ViewPane.Misc.Misc.IsFileAvailable (this.fullPath) == false)
					throw new IOException (ViewPane.Languages.UiStrings.CanTAccessOriginalImageFile_STR + " '" + this.fullPath + "'!");

				using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(this.fullPath))
				{
					ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing ();
					double zoom = Math.Min (1.0, 300.0 / itDecoder.DpiX);

					if (itDecoder.PixelFormat == System.Drawing.Imaging.PixelFormat.Format1bppIndexed)
						resizing.Resize(itDecoder, this.reducedPath, new ImageProcessing.FileFormat.Png(), zoom);
					else
						resizing.Resize(itDecoder, this.reducedPath, new ImageProcessing.FileFormat.Jpeg(85), zoom);
				}
			}
		}
		#endregion 

		#region CreatePreviewImage()
		private void CreatePreviewImage ()
		{
			if (this.recreatePreview || File.Exists(this.previewPath) == false)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(this.previewPath));

				if (File.Exists(this.previewPath))
					File.Delete (this.previewPath);

				if (ViewPane.Misc.Misc.IsFileAvailable(this.reducedPath) == false)
					throw new IOException (ViewPane.Languages.UiStrings.CanTAccessReducedImageFile_STR + " '" + this.reducedPath + "'!");

				ImageProcessing.BigImages.ThumbnailCreator thumbnailCreator = new ImageProcessing.BigImages.ThumbnailCreator ();
				using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder (this.reducedPath))
				{
					double zoom = Math.Min (1.0, 96.0 / itDecoder.DpiX);
					int dpi = Convert.ToInt32 (itDecoder.DpiX * zoom);

					thumbnailCreator.Go (itDecoder, this.previewPath, new ImageProcessing.FileFormat.Jpeg (85), zoom);
				}
			}
		}
		#endregion 

		#region CreateThumbnail()
		private void CreateThumbnail ()
		{
			if (this.recreateThumbnail || File.Exists(this.thumbnailPath) == false)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(this.thumbnailPath));

				//create thumbnail
				if (File.Exists (this.thumbnailPath))
					File.Delete (this.thumbnailPath);

				if (ViewPane.Misc.Misc.IsFileAvailable(this.previewPath) == false)
					throw new IOException(ViewPane.Languages.UiStrings.CanTAccessPreviewFile_STR + " '" + this.previewPath + "'!");

				using (System.Drawing.Bitmap preview = ImageProcessing.ImageCopier.LoadFileIndependentImage(this.previewPath))
				{
					using (System.Drawing.Bitmap thumbnail = ImageProcessing.ThumbnailCreator.Get (preview, new System.Drawing.Size ((int)Thumbnail.ThumbnailSize.Width - 12, (int)Thumbnail.ThumbnailSize.Height - 12)))
					{
						thumbnail.Save (this.thumbnailPath, System.Drawing.Imaging.ImageFormat.Png);
					}
				}
			}
		}
		#endregion 

		#region itImage_ItSettingsChanged()
		void ItImage_Changed(ImageProcessing.IpSettings.ItImage itImage, ImageProcessing.IpSettings.ItProperty type)
		{
			if (((type & (ImageProcessing.IpSettings.ItProperty.ImageSettings | ImageProcessing.IpSettings.ItProperty.Clip)) > 0))
			{
				if (ItImageSettingsChanged != null)
					ItImageSettingsChanged();
			}
		}
		#endregion

		#endregion
	}
}