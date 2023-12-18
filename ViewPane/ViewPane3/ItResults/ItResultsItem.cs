using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows;

namespace ViewPane.ItResults
{
	class ItResultsItem
	{
		ItResultsImage itResultsImage;
		bool leftPage;
		string sourcePath;
		string destPath;
		double zoom;
		ImageProcessing.ImageFile.ImageInfo sourceImageInfo;
		//bool refresh = true;
		object locker = new object();


		#region constructor
		public ItResultsItem(ItResultsImage itResultsImage, bool leftPage, string sourcePath, string destPath, double zoom, ImageProcessing.ImageFile.ImageInfo sourceImageInfo)
		{
			this.itResultsImage = itResultsImage;
			this.leftPage = leftPage;
			this.sourcePath = sourcePath;
			this.destPath = destPath;
			this.zoom = zoom;
			this.sourceImageInfo = sourceImageInfo;

			//if (this.ItImage != null)
			//	this.ItImage.Changed += delegate() { this.refresh = true; };
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public double Zoom { get { return this.zoom; } }
		ImageProcessing.IpSettings.ItImage ItImage { get { return this.itResultsImage.ItImage; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);
		
		#region GetClip()
		public Bitmap GetClip(Rect clip, double zoom)
		{
			lock (locker)
			{
				//if (this.refresh)
				{
					if (this.leftPage)
						this.ItImage.PageL.Execute(sourcePath, this.destPath, GetImageFormat(this.sourceImageInfo));
					else
						this.ItImage.PageR.Execute(sourcePath, this.destPath, GetImageFormat(this.sourceImageInfo));

					//this.refresh = false;
				}

				using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(this.destPath))
				{
					System.Drawing.Rectangle imagePortion = new System.Drawing.Rectangle(Convert.ToInt32(clip.X * this.zoom), Convert.ToInt32(clip.Y * this.zoom),
						Convert.ToInt32(clip.Width * this.zoom), Convert.ToInt32(clip.Height * this.zoom));
					imagePortion.Intersect(new System.Drawing.Rectangle(System.Drawing.Point.Empty, itDecoder.Size));
					System.Drawing.Bitmap bitmapClip = itDecoder.GetClip(imagePortion);

					if (zoom != 1)
					{
						System.Drawing.Size size = new System.Drawing.Size(Convert.ToInt32(clip.Width * zoom), Convert.ToInt32(clip.Height * zoom));
						System.Drawing.Bitmap resizedBitmap2 = ImageProcessing.Resizing.GetThumbnail(bitmapClip, size);
						//System.Drawing.Bitmap resizedBitmap = ImageProcessing.Resizing.Resize(bitmapClip, Rectangle.Empty, zoom / this.zoom, ImageProcessing.Resizing.ResizeMode.Quality);
						bitmapClip.Dispose();
						bitmapClip = resizedBitmap2;
					}
				
					IntPtr hBitmap = bitmapClip.GetHbitmap();
					DeleteObject(hBitmap);
					return bitmapClip;
				}

				//return new Bitmap(this.destPath);
			}
		}
		#endregion

		#region Refresh()
		public void Refresh()
		{
			//this.refresh = true;
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region GetImageFormat()
		private ImageProcessing.FileFormat.IImageFormat GetImageFormat(ImageProcessing.ImageFile.ImageInfo imageInfo)
		{
			if (imageInfo.PixelFormat == System.Drawing.Imaging.PixelFormat.Format1bppIndexed)
				return new ImageProcessing.FileFormat.Png();
			else
				return new ImageProcessing.FileFormat.Jpeg(80);
		}
		#endregion

		#endregion

	}
}
