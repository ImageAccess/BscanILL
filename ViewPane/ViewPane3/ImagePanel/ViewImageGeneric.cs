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
using ViewPane;



namespace ViewPane.ImagePanel
{
	/*internal class ViewImageGeneric : IViewImage
	{
		#region variables
		ViewPane.IP.PreviewCreator creator = ViewPane.IP.PreviewCreator.Instance;
		OpusObjectManagerNS.OpusObjectWrapper wrapper;

		double reducedImageZoom;
		double previewZoom;

		object locker = new object();

		ImageProcessing.BigImages.ItDecoder fullImage = null;
		ImageProcessing.BigImages.ItDecoder reducedImage = null;
		ImageProcessing.BigImages.ItDecoder previewImage = null;
		int?		id;

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		#endregion

		#region constructor
		internal ViewImageGeneric(OpusObjectManagerNS.OpusObjectWrapper wrapper, int? id, string fullImagePath, string reducedImagePath, string previewPath)
		{
			if (File.Exists(fullImagePath) == false)
				throw new IOException(ViewPane2Strings.ImageFilePath_STR+" '" + fullImagePath + "' "+ViewPane2Strings.IsInvalid_STR);
			
			this.wrapper = wrapper;
			this.fullImage = new ImageProcessing.BigImages.ItDecoder(fullImagePath);
			this.reducedImage = new ImageProcessing.BigImages.ItDecoder(reducedImagePath);
			this.previewImage = new ImageProcessing.BigImages.ItDecoder(previewPath);
			this.id = id;

			this.reducedImageZoom = Math.Min(this.reducedImage.Width / (double)this.fullImage.Width, this.reducedImage.Height / (double)this.fullImage.Height);
			this.previewZoom = Math.Min(this.previewImage.Width / (double)this.fullImage.Width, this.previewImage.Height / (double)this.fullImage.Height);
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
		public OpusObjectWrapper					Wrapper	{ get { return wrapper; } }
		public bool									TwoPages{ get { return false; } }

		public ImageProcessing.IpSettings.ItImage	ItImage { get { return null; } }
		public int?									ImageId { get { return this.id; } }

		public bool									IsFixed { get { return true; } set {  } }
		public bool									IsIndependent { get { return true; } set { } }

		public ImageProcessing.ImageFile.ImageInfo	FullImageInfo { get { return this.fullImage.ImageInfo; } }
		public System.Windows.Size					FullImageSize { get { return new System.Windows.Size(this.fullImage.Width, this.fullImage.Height); } }

		public float								Confidence { get { return (float)1.0F; } }

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
			
			if (zoom <= this.previewZoom)
			{
				this.reducedImage.Deactivate();
				this.fullImage.Deactivate();

				System.Drawing.Rectangle imagePortion = new System.Drawing.Rectangle(Convert.ToInt32(clip.X * this.previewZoom), Convert.ToInt32(clip.Y * this.previewZoom),
					Convert.ToInt32(clip.Width * this.previewZoom), Convert.ToInt32(clip.Height * this.previewZoom));
				imagePortion.Intersect(new Rectangle(System.Drawing.Point.Empty, this.previewImage.Size));
				bitmapClip = this.previewImage.GetClip(imagePortion);

				if (zoom != 1)
				{
					Bitmap resizedBitmap = ImageProcessing.Resizing.Resize(bitmapClip, Rectangle.Empty, zoom / this.previewZoom, ImageProcessing.Resizing.ResizeMode.Quality);
					bitmapClip.Dispose();
					bitmapClip = resizedBitmap;
				}
			}
			else if (zoom <= this.reducedImageZoom)
			{
				this.previewImage.Deactivate();
				this.fullImage.Deactivate();

				System.Drawing.Rectangle imagePortion = new System.Drawing.Rectangle(Convert.ToInt32(clip.X * this.reducedImageZoom), Convert.ToInt32(clip.Y * this.reducedImageZoom),
					Convert.ToInt32(clip.Width * this.reducedImageZoom), Convert.ToInt32(clip.Height * this.reducedImageZoom));
				imagePortion.Intersect(new Rectangle(System.Drawing.Point.Empty, this.reducedImage.Size));
				bitmapClip = this.reducedImage.GetClip(imagePortion);

				if (zoom != 1)
				{
					Bitmap resizedBitmap = ImageProcessing.Resizing.Resize(bitmapClip, Rectangle.Empty, zoom / this.reducedImageZoom, ImageProcessing.Resizing.ResizeMode.Quality);
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
					Bitmap resizedBitmap = ImageProcessing.Resizing.Resize(bitmapClip, Rectangle.Empty, zoom, ImageProcessing.Resizing.ResizeMode.Quality);
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

		#region GetImageFormatFromExtension()
		private ImageFormat GetImageFormatFromExtension(string file)
		{
			switch (Path.GetExtension(file))
			{
				case ".tif": return ImageFormat.Tiff;
				case ".gif": return ImageFormat.Gif;
				case ".jpg": return ImageFormat.Jpeg;
				case ".bmp": return ImageFormat.Bmp;
				default: return ImageFormat.Png;
			}
		}
		#endregion

		#endregion
	}*/
}
