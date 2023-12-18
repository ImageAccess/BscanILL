using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
//using System.Windows.Forms;
using System.IO ;
using System.Drawing.Drawing2D ;
using System.Drawing.Imaging ;
using System.Threading ;
using System.Text;

//using ImageProcessing;
//using ImageProcessing.ImageFile;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows;



namespace ViewPane.ImagePanel
{
	/// <summary>
	/// Summary description for Picture.
	/// </summary>
	/*internal class ViewImage : IViewImage
	{
		#region variables
		ViewPane.IP.PreviewCreator creator = ViewPane.IP.PreviewCreator.Instance;
		OpusObjectManagerNS.OpusObjectWrapper wrapper;
		OpusObjectManagerNS.FullScanData scanData;
		ImageProcessing.IpSettings.ItImage itImage;

		bool itActive = false;
		double reducedImageZoom;
		double previewZoom;
		
		object locker = new object();

		ImageProcessing.BigImages.ItDecoder fullImage = null;
		ImageProcessing.BigImages.ItDecoder reducedImage = null;
		ImageProcessing.BigImages.ItDecoder previewImage = null;

		public event EventHandler ItImageSettingsChanged;

		#endregion

		#region constructor
		internal ViewImage(OpusObjectManagerNS.OpusObjectWrapper wrapper, OpusObjectManagerNS.FullScanData scanData, ImageProcessing.IpSettings.ItImage itImage, bool itActive)
		{
			this.wrapper = wrapper;
			this.scanData = scanData;
			this.itActive = itActive;

			if (File.Exists(this.scanData.TheScanImageData.FullImagePath) == false)
				throw new IOException(ViewPane2Strings.ImageFilePath_STR+" '" + this.scanData.TheScanImageData.FullImagePath + "' "+ViewPane2Strings.IsInvalid_STR);
				
			this.fullImage = new ImageProcessing.BigImages.ItDecoder(this.scanData.TheScanImageData.FullImagePath);
			this.reducedImage = new ImageProcessing.BigImages.ItDecoder(this.scanData.TheScanImageData.ReducedImagePath);
			this.previewImage = new ImageProcessing.BigImages.ItDecoder(this.scanData.TheScanImageData.PreviewImagePath);

			this.reducedImageZoom = Math.Min(this.reducedImage.Width / (double)this.fullImage.Width, this.reducedImage.Height / (double)this.fullImage.Height);
			this.previewZoom = Math.Min(this.previewImage.Width / (double)this.fullImage.Width, this.previewImage.Height / (double)this.fullImage.Height);

			this.itImage = itImage;
			this.itImage.ItPagesChanged += new ImageProcessing.IpSettings.ItImage.VoidHnd(ItImage_Changed);
		}
		#endregion

		#region destructor
		public void Dispose()
		{
			ReleaseBitmaps();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		public OpusObjectWrapper	Wrapper			{ get { return wrapper; } }
		public FullScanData			ScanData		{ get { return this.scanData; } set { this.scanData = value; } }
		public bool					TwoPages		{ get { return itImage.TwoPages; } }

		public ImageProcessing.IpSettings.ItImage	ItImage { get { return this.itImage; } }
		public int?									ImageId { get { return this.scanData.TheScanData.ScanID; } }

		public bool		IsFixed { get { return this.itImage == null || this.itImage.IsFixed; } set { this.itImage.IsFixed = value; } }
		public bool		IsIndependent { get { return this.itImage.IsIndependent; } set { this.itImage.IsIndependent = value; } }

		public ImageProcessing.ImageFile.ImageInfo	FullImageInfo { get { return this.fullImage.ImageInfo; } }
		public System.Windows.Size					FullImageSize { get { return new System.Windows.Size(this.fullImage.Width, this.fullImage.Height); } }

		#region Confidence
		public float Confidence
		{
			get 
			{
				double confidence = 1.0;

				if (this.itActive)
				{
					confidence = (confidence < itImage.PageL.Clip.ClipConfidence) ? confidence : itImage.PageL.Clip.ClipConfidence;
					confidence = (confidence < itImage.PageL.Bookfolding.Confidence) ? confidence : itImage.PageL.Bookfolding.Confidence;
					confidence = (confidence < itImage.PageL.Clip.SkewConfidence) ? confidence : itImage.PageL.Clip.SkewConfidence;

					foreach (ImageProcessing.IpSettings.Finger artifact in itImage.PageL.Fingers)
						confidence = (confidence < artifact.Confidence) ? confidence : artifact.Confidence;

					if (TwoPages)
					{
						confidence = (confidence < itImage.PageR.Clip.ClipConfidence) ? confidence : itImage.PageR.Clip.ClipConfidence;
						confidence = (confidence < itImage.PageR.Bookfolding.Confidence) ? confidence : itImage.PageR.Bookfolding.Confidence;
						confidence = (confidence < itImage.PageR.Clip.SkewConfidence) ? confidence : itImage.PageR.Clip.SkewConfidence;

						foreach (ImageProcessing.IpSettings.Finger artifact in itImage.PageR.Fingers)
							confidence = (confidence < artifact.Confidence) ? confidence : artifact.Confidence;
					}
				}

				return (float) confidence;
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region ReleaseBitmaps()
		public void ReleaseBitmaps()
		{		
			this.fullImage.Deactivate();
			this.reducedImage.Deactivate();
			this.previewImage.Deactivate();		
			GC.Collect();
		}
		#endregion

		#region GetBitmap()
		public BitmapSource GetBitmap(Rectangle clip, double zoom)
		{
			Bitmap bitmapClip;

			if (zoom <= this.previewZoom / 2)
			{
				this.reducedImage.Deactivate();
				this.fullImage.Deactivate();

				System.Drawing.Rectangle imagePortion = new System.Drawing.Rectangle(Convert.ToInt32(clip.X * this.previewZoom), Convert.ToInt32(clip.Y * this.previewZoom),
					Convert.ToInt32(clip.Width * this.previewZoom), Convert.ToInt32(clip.Height * this.previewZoom));
				imagePortion.Intersect(new Rectangle(System.Drawing.Point.Empty, this.previewImage.Size));
				bitmapClip = this.previewImage.GetClip(imagePortion);

				if (zoom != 1)
				{
					System.Drawing.Size thumbSize = new System.Drawing.Size(Convert.ToInt32(clip.Width * zoom), Convert.ToInt32(clip.Height * zoom));
					Bitmap resizedBitmap = ImageProcessing.Resizing.GetThumbnail(bitmapClip, thumbSize);
					bitmapClip.Dispose();
					bitmapClip = resizedBitmap;
				}
			}
			else if (zoom <= this.reducedImageZoom / 2)
			{
				this.previewImage.Deactivate();
				this.fullImage.Deactivate();

				System.Drawing.Rectangle imagePortion = new System.Drawing.Rectangle(Convert.ToInt32(clip.X * this.reducedImageZoom), Convert.ToInt32(clip.Y * this.reducedImageZoom),
					Convert.ToInt32(clip.Width * this.reducedImageZoom), Convert.ToInt32(clip.Height * this.reducedImageZoom));
				imagePortion.Intersect(new Rectangle(System.Drawing.Point.Empty, this.reducedImage.Size));
				bitmapClip = this.reducedImage.GetClip(imagePortion);

				if (zoom != 1)
				{
					System.Drawing.Size thumbSize = new System.Drawing.Size(Convert.ToInt32(clip.Width * zoom), Convert.ToInt32(clip.Height * zoom));
					Bitmap resizedBitmap = ImageProcessing.Resizing.GetThumbnail(bitmapClip, thumbSize);
					bitmapClip.Dispose();
					bitmapClip = resizedBitmap;
				}
			}
			else
			{
				this.previewImage.Deactivate();
				this.reducedImage.Deactivate();

				System.Drawing.Rectangle imagePortion = new System.Drawing.Rectangle(Convert.ToInt32(clip.X), Convert.ToInt32(clip.Y), Convert.ToInt32(clip.Width), Convert.ToInt32(clip.Height));
				imagePortion.Intersect(new Rectangle(System.Drawing.Point.Empty, this.fullImage.Size));
				bitmapClip = this.fullImage.GetClip(imagePortion);

				if (zoom != 1)
				{
					System.Drawing.Size thumbSize = new System.Drawing.Size(Convert.ToInt32(clip.Width * zoom), Convert.ToInt32(clip.Height * zoom));
					Bitmap resizedBitmap = ImageProcessing.Resizing.GetThumbnail(bitmapClip, thumbSize);
					bitmapClip.Dispose();
					bitmapClip = resizedBitmap;
				}
			}

			BitmapSource bitmapImage = Misc.GetBitmapSource(bitmapClip);
			bitmapClip.Dispose();

			return bitmapImage;
		}
		#endregion

		#region GetBitmapAsync()
		public void GetBitmapAsync(ViewPane.IP.IPreviewCaller caller, int renderingId, bool highPriority, int width, int height)
		{
			lock (this.locker)
			{				
				double fullZoom = Math.Min(width / (double)this.fullImage.Width, height / (double)this.fullImage.Height);
				double reducedZoom = Math.Min(width / (double)this.reducedImage.Width, height / (double)this.reducedImage.Height);
				double previewZoom = Math.Min(width / (double)this.previewImage.Width, height / (double)this.previewImage.Height);

				if (previewZoom <= 0.5)
					creator.GetPreviewAsync(caller, renderingId, highPriority, new FileInfo(this.previewImage.FilePath), previewZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else if (reducedZoom <= 0.5)
					creator.GetPreviewAsync(caller, renderingId, highPriority, new FileInfo(this.reducedImage.FilePath), reducedZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else
					creator.GetPreviewAsync(caller, renderingId, highPriority, new FileInfo(this.fullImage.FilePath), fullZoom, ImageProcessing.Resizing.ResizeMode.Quality);				
			}
		}

		public void GetBitmapAsync(ViewPane.IP.IPreviewCaller caller, int renderingId, bool highPriority, System.Windows.Rect imageRect, int width, int height)
		{
			lock (this.locker)
			{
				double fullZoom = Math.Min(width / (imageRect.Width * this.fullImage.Width), height / (imageRect.Height * this.fullImage.Height));
				double reducedZoom = Math.Min(width / (imageRect.Width * this.reducedImage.Width), height / (imageRect.Height * this.reducedImage.Height));
				double previewZoom = Math.Min(width / (double)(imageRect.Width * this.previewImage.Width), height / (double)(imageRect.Height * this.previewImage.Height));

				if (previewZoom <= 0.5)
					creator.GetPreviewAsync(caller, renderingId, highPriority, new FileInfo(this.previewImage.FilePath), imageRect, previewZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else if (reducedZoom <= 0.5)
					creator.GetPreviewAsync(caller, renderingId, highPriority, new FileInfo(this.reducedImage.FilePath), imageRect, reducedZoom, ImageProcessing.Resizing.ResizeMode.Quality);
				else
					creator.GetPreviewAsync(caller, renderingId, highPriority, new FileInfo(this.fullImage.FilePath), imageRect, fullZoom, ImageProcessing.Resizing.ResizeMode.Quality);
			}
		}
		#endregion	

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region ItImage_Changed()
		void ItImage_Changed()
		{
			if (ItImageSettingsChanged != null)
				ItImageSettingsChanged(this, null);
		}
		#endregion

		#endregion
	}*/
}
