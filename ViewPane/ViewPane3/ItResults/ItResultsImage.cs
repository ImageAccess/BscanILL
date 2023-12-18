using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;
using ViewPane.Hierarchy;

namespace ViewPane.ItResults
{
	class ItResultsImage
	{
		ImageProcessing.IpSettings.ItImage itImage;

		ItResultsItem fullL = null;
		ItResultsItem reducedL = null;
		ItResultsItem previewL = null;
		ItResultsItem fullR = null;
		ItResultsItem reducedR = null;
		ItResultsItem previewR = null;
		DirectoryInfo tempDir;

		ImageProcessing.ImageFile.ImageInfo fullInfo ;
		ImageProcessing.ImageFile.ImageInfo reducedInfo;
		ImageProcessing.ImageFile.ImageInfo previewInfo;

		string				fileName;
		static object		threadLocker = new object();


		#region constructor
		public ItResultsImage(VpImage vpImage)
		{
			if (vpImage.ItImage != null)
				this.itImage = vpImage.ItImage;
			else
			{
				this.itImage = new ImageProcessing.IpSettings.ItImage(new FileInfo(vpImage.FullPath), false);
				this.itImage.PageL.Activate(new BIP.Geometry.RatioRect(0, 0, 1, 1), true);
				this.itImage.PageR.Deactivate();
				this.itImage.IsFixed = true;
			}

			this.itImage.Changed += new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);

			FileInfo fi = new FileInfo(vpImage.FullPath);

			this.fileName = Path.GetFileNameWithoutExtension(fi.Name);
			tempDir = new DirectoryInfo(fi.Directory.FullName + @"\" + fileName);

			tempDir.Create();

			this.fullInfo = new ImageProcessing.ImageFile.ImageInfo(vpImage.FullPath);
			this.reducedInfo = new ImageProcessing.ImageFile.ImageInfo(vpImage.ReducedPath);
			this.previewInfo = new ImageProcessing.ImageFile.ImageInfo(vpImage.PreviewPath);

			fullL = new ItResultsItem(this, true, vpImage.FullPath, tempDir.FullName + @"\" + fileName + "_full_L" + GetExtension(fullInfo), 1.0, fullInfo);
			reducedL = new ItResultsItem(this, true, vpImage.ReducedPath, tempDir.FullName + @"\" + fileName + "_reduced_L" + GetExtension(reducedInfo), reducedInfo.Width / (double)fullInfo.Width, reducedInfo);
			previewL = new ItResultsItem(this, true, vpImage.PreviewPath, tempDir.FullName + @"\" + fileName + "_preview_L" + GetExtension(previewInfo), previewInfo.Width / (double)fullInfo.Width, previewInfo);

			if (this.itImage.TwoPages)
			{
				fullR = new ItResultsItem(this, false, vpImage.FullPath, tempDir.FullName + @"\" + fileName + "_full_R" + GetExtension(fullInfo), 1.0, fullInfo);
				reducedR = new ItResultsItem(this, false, vpImage.ReducedPath, tempDir.FullName + @"\" + fileName + "_reduced_R" + GetExtension(reducedInfo), reducedInfo.Width / (double)fullInfo.Width, reducedInfo);
				previewR = new ItResultsItem(this, false, vpImage.PreviewPath, tempDir.FullName + @"\" + fileName + "_preview_R" + GetExtension(previewInfo), previewInfo.Width / (double)fullInfo.Width, previewInfo);
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		internal ImageProcessing.IpSettings.ItImage	ItImage { get { return this.itImage; } }
		
		public System.Drawing.Size			FullImageSizeL 
		{ 
			get 
			{
				if (this.ItImage != null && this.ItImage.IsFixed == false)
					return new System.Drawing.Size((int)(fullInfo.Width * this.itImage.PageL.ClipRect.Width), (int)(fullInfo.Height * this.itImage.PageL.ClipRect.Height)); 
				else
					return fullInfo.Size;
			}
		}

		public System.Drawing.Size			FullImageSizeR 
		{ 
			get
			{
				if (this.ItImage != null && this.ItImage.IsFixed == false && this.itImage.PageR.IsActive)			
					return new System.Drawing.Size((int)(fullInfo.Width * this.itImage.PageR.ClipRect.Width), (int)(fullInfo.Height * this.itImage.PageR.ClipRect.Height));
				else
					return fullInfo.Size;
			} 
		}
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			this.itImage.Changed -= new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);
			
			DeleteTempDir();
		}
		#endregion

		#region GetClip()
		public Bitmap GetClip(bool leftPage, Rect clip, double zoom)
		{
			lock (threadLocker)
			{
				if (leftPage)
				{
					if (zoom <= this.previewL.Zoom / 2)
						return this.previewL.GetClip(clip, zoom);
					else if (zoom <= this.reducedL.Zoom / 2)
						return this.reducedL.GetClip(clip, zoom);
					else
						return this.fullL.GetClip(clip, zoom);
				}
				else
				{
					if (zoom <= this.previewR.Zoom / 2)
						return this.previewR.GetClip(clip, zoom);
					else if (zoom <= this.reducedR.Zoom / 2)
						return this.reducedR.GetClip(clip, zoom);
					else
						return this.fullR.GetClip(clip, zoom);
				}
			}
		}
		#endregion

		#region GetClipAsync()
		public void GetClipAsync(ViewPane.IP.IitResultsCaller caller, int renderingId, bool leftPage, bool highPriority, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			ViewPane.IP.ItResultsCreator.Instance.GetPreviewAsync(caller, renderingId, this, leftPage, highPriority, imageRect, zoom, quality);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region GetExtension()
		private string GetExtension(ImageProcessing.ImageFile.ImageInfo imageInfo)
		{
			if (imageInfo.PixelFormat == System.Drawing.Imaging.PixelFormat.Format1bppIndexed)
				return ".png";
			else
				return ".jpg";
		}
		#endregion

		#region DeleteTempDir()
		public void DeleteTempDir()
		{
			if (tempDir != null)
			{
				tempDir.Refresh();
				try { tempDir.Delete(true); }
				catch { }
			}
		}
		#endregion

		#region ItImage_Changed()
		void ItImage_Changed(ImageProcessing.IpSettings.ItImage itImage, ImageProcessing.IpSettings.ItProperty type)
		{
			if (((type & ImageProcessing.IpSettings.ItProperty.ImageSettings) > 0) || ((type & ImageProcessing.IpSettings.ItProperty.Clip) > 0))
			{
				this.fullL.Refresh();
				this.reducedL.Refresh();
				this.previewL.Refresh();

				if (this.fullR != null)
					this.fullR.Refresh();
				if (this.reducedR != null)
					this.reducedR.Refresh();
				if (this.previewR != null)
					this.previewR.Refresh();
			}
		}
		#endregion

		#endregion

	}
}
