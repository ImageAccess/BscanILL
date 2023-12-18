using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32.SafeHandles;
using ViewPane.Hierarchy;



namespace ViewPane.ImagePanel
{
	/// <summary>
	/// Summary description for Picture.
	/// </summary>
	internal class ViewBitmap : IViewImage
	{
		#region variables
		ImageProcessing.ImageFile.ImageInfo fullImageInfo;
		//double previewZoom;

		object locker = new object ();
		Bitmap fullImage = null;
		//ImageProcessing.IpSettings.ItImage itImage;

		#endregion variables


		#region constructor
		internal ViewBitmap (Bitmap bitmap)
		{
			this.fullImage = bitmap;
			this.fullImageInfo = new ImageProcessing.ImageFile.ImageInfo (bitmap);
			//this.previewZoom = 1.0;

			//this.itImage = new ImageProcessing.IpSettings.ItImage(bitmap);
		}
		#endregion constructor

		#region destructor
		public void Dispose ()
		{
			ReleaseBitmaps ();
		}
		#endregion destructor


		//PUBLIC PROPERTIES
		#region public properties
		public bool TwoPages { get { return false; } }

		public ImageProcessing.IpSettings.ItImage	ItImage { get { return null; } }
		public VpImage								VpImage { get { return null; } }

		public bool IsFixed { get { return true; } set { } }
		public bool IsIndependent { get { return true; } set { } }

		public ImageProcessing.ImageFile.ImageInfo FullImageInfo { get { return this.fullImageInfo; } }
		public System.Windows.Size FullImageSize { get { return new System.Windows.Size (fullImageInfo.Width, fullImageInfo.Height); } }

		public float Confidence { get { return 1.0F; } }

		#endregion public properties


		//PUBLIC METHODS
		#region public methods

		#region ReleaseBitmaps()
		public void ReleaseBitmaps ()
		{
			this.fullImage.Dispose ();
			this.fullImage = null;
			GC.Collect ();
		}
		#endregion ReleaseBitmaps()

		#region GetBitmap()
		public BitmapSource GetBitmap (Rectangle clip, double zoom)
		{
			//Bitmap bitmapClip = ImageProcessing.Resizing.Resize (this.fullImage, clip, zoom, ImageProcessing.Resizing.ResizeMode.Quality);
			int width = (int)(clip.Width * zoom);
			int height = (int)(clip.Height * zoom);

			Bitmap bitmapClip = ImageProcessing.Resizing.GetThumbnail(this.fullImage, clip, new System.Drawing.Size(width, height)); 

			BitmapSource bitmapImage = ViewPane.Misc.Misc.GetBitmapSource(bitmapClip);
			bitmapClip.Dispose ();

			return bitmapImage;
		}
		#endregion GetBitmap()

		#region GetBitmapAsync()
		public void GetBitmapAsync (ViewPane.IP.IPreviewCaller caller, int renderingId, bool highPriority, int width, int height)
		{
			lock (this.locker)
			{
				try
				{
					DateTime start = DateTime.Now;
					double zoom = Math.Min (width / (double)this.fullImage.Width, height / (double)this.fullImage.Height);
					System.Drawing.Rectangle imagePortion = new System.Drawing.Rectangle (0, 0, this.fullImage.Width, this.fullImage.Height);
					//Bitmap bitmapClip = ImageProcessing.Resizing.Resize (this.fullImage, imagePortion, zoom, ImageProcessing.Resizing.ResizeMode.Quality);
					Bitmap bitmapClip = ImageProcessing.Resizing.GetThumbnail(this.fullImage, imagePortion, new System.Drawing.Size(width, height)); 

/*#if DEBUG
					bitmapClip.Save(@"c:\delete\a.png", ImageFormat.Png);
#endif	*/				
					
					caller.ThumbnailCreatedDelegate (bitmapClip, renderingId, DateTime.Now.Subtract (start));
				}
				catch (Exception ex)
				{
					caller.ThumbnailErrorDelegate (ex);
				}
			}
		}

		public void GetBitmapAsync (ViewPane.IP.IPreviewCaller caller, int renderingId, bool highPriority, BIP.Geometry.RatioRect imageRect, int width, int height)
		{
			lock (this.locker)
			{
				try
				{
					DateTime start = DateTime.Now;
					double zoom = Math.Min (width / ((double)imageRect.Width * this.fullImage.Width), height / ((double)imageRect.Height * this.fullImage.Height));
					System.Drawing.Rectangle imagePortion = new System.Drawing.Rectangle (Convert.ToInt32 (imageRect.X * this.fullImage.Width), Convert.ToInt32 (imageRect.Y * this.fullImage.Height), Convert.ToInt32 (imageRect.Width * this.fullImage.Width), Convert.ToInt32 (imageRect.Height * this.fullImage.Height));
					//Bitmap bitmapClip = ImageProcessing.Resizing.Resize (this.fullImage, imagePortion, zoom, ImageProcessing.Resizing.ResizeMode.Quality);
					Bitmap bitmapClip = ImageProcessing.Resizing.GetThumbnail(this.fullImage, imagePortion, new System.Drawing.Size(width, height));

					caller.ThumbnailCreatedDelegate (bitmapClip, renderingId, DateTime.Now.Subtract (start));
				}
				catch (Exception ex)
				{
					caller.ThumbnailErrorDelegate (ex);
				}
			}
		}
		#endregion GetBitmapAsync()

		#endregion public methods


		//PRIVATE METHODS
		#region private methods

		#endregion private methods
	}
}