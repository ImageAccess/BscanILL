using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using System.IO;
using System.Drawing.Printing;
using System.Windows.Input;
using System.Windows;
using BscanILL.Misc;

namespace BscanILL.IP
{
	public delegate void ResetClipRequestHnd();
	public delegate void CropRequestHnd(IllImageOperations.CropEventArgs args);
	public delegate void SplitImageRequestHnd(BscanILL.Hierarchy.IllImage image, int imageIndex);
	public delegate void UnsplitImagesHnd(BscanILL.Hierarchy.IllImage illImage);
	public delegate void RotateImageRequestHnd(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle);

	public class IllImageOperations
	{
		#region variables
		Window				parentWindow;
		Notifications		notifications = Notifications.Instance;

		//public delegate void OperationSuccessfullHnd();		
		public delegate void OperationSuccessfullHnd(BscanILL.Hierarchy.IllImage illImage);
		public delegate void OperationErrorHnd(IllException ex);
		public delegate void InsertImageHnd(int index, string file, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast);
		public delegate void InsertPagesHnd(int index, string file1, string file2, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast, int pixelsOverlapping);
		public delegate void DeleteImageHnd(BscanILL.Hierarchy.IllImage illImage);
        public delegate void RotateImageHnd(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle);

		OperationSuccessfullHnd	dlgOperationSuccessfull;
		OperationErrorHnd		dlgOperationError;
		InsertImageHnd			dlgInsertImage;
		InsertPagesHnd			dlgInsertPages;
		DeleteImageHnd			dlgDeleteImage;

		public event OperationSuccessfullHnd	OperationSuccessfull;
		public event OperationErrorHnd			OperationError;
		public event InsertImageHnd				InsertImage;
		public event InsertPagesHnd				InsertPages;
		public event DeleteImageHnd				DeleteImage;
		#endregion


		#region constructor
		public IllImageOperations(Window parentWindow)
		{
			this.parentWindow = parentWindow;

			dlgOperationSuccessfull = delegate(BscanILL.Hierarchy.IllImage illImage)
			{
				if (OperationSuccessfull != null)
					OperationSuccessfull(illImage);
			};
			dlgOperationError = delegate(IllException ex)
			{
				if (OperationError != null)
					OperationError(ex);
			};
			dlgInsertImage = delegate(int index, string file, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast)
			{
				if(InsertImage != null)
					InsertImage( index, file, colorMode, dpi, brightness, contrast);
			};
			dlgInsertPages = delegate(int index, string file1, string file2, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast, int pixelsOverlapping)
			{
				if (InsertPages != null)
					InsertPages(index, file1, file2, colorMode, dpi, brightness, contrast, pixelsOverlapping);
			};
			dlgDeleteImage = delegate(BscanILL.Hierarchy.IllImage illImage)
			{
				if (DeleteImage != null)
					DeleteImage(illImage);
			};
		}
		#endregion


		#region class CropEventArgs
		public class CropEventArgs
		{
			public readonly BscanILL.Hierarchy.IllImage IllImage;
			public readonly int SessionIndex;
			/// <summary>
			/// in percents
			/// </summary>
			public readonly System.Windows.Rect Clip;
			public readonly bool DeleteOriginal;

			public CropEventArgs(BscanILL.Hierarchy.IllImage illImage, int sessionIndex, System.Windows.Rect clipInPercents, bool deleteOriginal)
			{
				this.IllImage = illImage;
				this.SessionIndex = sessionIndex;
				this.Clip = clipInPercents;
				this.DeleteOriginal = deleteOriginal;
			}
		}
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		BscanILL.SETTINGS.Settings _settings = BscanILL.SETTINGS.Settings.Instance;

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Crop()
		public void Crop(BscanILL.IP.IllImageOperations.CropEventArgs args)
		{
			BscanILL.Hierarchy.IllImage image = (BscanILL.Hierarchy.IllImage)args.IllImage;
			ImageProcessing.ImageFile.ImageInfo info = args.IllImage.FullImageInfo;
			Rect rect = args.Clip;

			if (((rect.Width * info.Width) > info.DpiH * 0.9) && ((rect.Height * info.Height) > (info.DpiV / 2)))
			{
				System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(CropTU));
				thread.Name = "ThreadIllImageOperations_Crop";
				thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
				thread.SetApartmentState(System.Threading.ApartmentState.STA);
				thread.Start(args);
			}
			else
			{
				throw new IllException("Selected clip is too small!");
			}
		}
		#endregion

		#region SplitImage()
		public void SplitImage(BscanILL.Hierarchy.IllImage image, int imageIndex)
		{
			if (image.FullImageInfo.Width > 2 * image.FullImageInfo.DpiH)
			{
				System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(SplitImageTU));
				thread.Name = "ThreadIllImageOperations_SplitImage";
				thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
				thread.SetApartmentState(System.Threading.ApartmentState.STA);
				thread.Start(new object[] { image, imageIndex });
			}
			else
			{
				throw new IllException("Selected image is too narrow to be split!");
			}
		}
		#endregion

		#region Unsplit()
		/*public void Unsplit(BscanILL.Hierarchy.IllImage image)
		{
			BscanILL.Hierarchy.IllImage illImage = image as BscanILL.Hierarchy.IllImage;

			if (illImage == null)
				throw new IllException(BscanILL.Languages.BscanILLStrings.General_SelectImageFirst_STR);
			if (illImage.IllImageType == BscanILL.Hierarchy.IllImageType.SingleImage)
				throw new IllException(BscanILL.Languages.BscanILLStrings.Frames_CantUnsplitSelectedImage_STR);
			if (illImage.AssociatedIllImage == null)
				throw new IllException(BscanILL.Languages.BscanILLStrings.Frames_CantUnsplitSelectedImage_STR);
			if (illImage.IllImageType == BscanILL.Hierarchy.IllImageType.LeftPage && illImage.AssociatedIllImage.IllImageType != BscanILL.Hierarchy.IllImageType.RightPage)
				throw new IllException(BscanILL.Languages.BscanILLStrings.Frames_CantUnsplitSelectedImage_STR);
			if (illImage.IllImageType == BscanILL.Hierarchy.IllImageType.RightPage && illImage.AssociatedIllImage.IllImageType != BscanILL.Hierarchy.IllImageType.LeftPage)
				throw new IllException(BscanILL.Languages.BscanILLStrings.Frames_CantUnsplitSelectedImage_STR);

			System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(UnsplitTU));
			thread.Name = "ThreadIllImageOperations_Unsplit";
			thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			thread.SetApartmentState(System.Threading.ApartmentState.STA);
			thread.Start(illImage);
		}*/
		#endregion
	
		#region RotateImage()
		public void RotateImage(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle)
		{
			BscanILL.Hierarchy.IllImage illImage = image as BscanILL.Hierarchy.IllImage;

			System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(RotateImageTU));
			thread.Name = "ThreadIllImageOperations_RotateImage";
			thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			thread.SetApartmentState(System.Threading.ApartmentState.STA);
			thread.Start(new object[] { image, imageIndex, angle });
		}
		#endregion

        #region RotateSmallAngleImage()
        public void RotateSmallAngleImage(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle)
        {
            BscanILL.Hierarchy.IllImage illImage = image as BscanILL.Hierarchy.IllImage;

            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(RotateSmallAngleImageTU));
            thread.Name = "ThreadIllImageOperations_RotateSmallAngleImage";
            thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start(new object[] { image, imageIndex, angle });
        }
        #endregion

		#endregion

		//PRIVATE METHODS
		#region private methods
	
		#region CropTU()
		protected void CropTU(object parameters)
		{
			try
			{
				BscanILL.IP.IllImageOperations.CropEventArgs args = (BscanILL.IP.IllImageOperations.CropEventArgs)parameters;
				BscanILL.Hierarchy.IllImage illImage = (BscanILL.Hierarchy.IllImage) args.IllImage;

				using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(illImage.FilePath.FullName))
				{
					ImageProcessing.BigImages.ImageCopier imageCopier = new ImageProcessing.BigImages.ImageCopier();
					string cropFile = illImage.FilePath.FullName + ".crop";
					Rectangle clipRect = new Rectangle(Convert.ToInt32(args.Clip.X * itDecoder.Width), Convert.ToInt32(args.Clip.Y * itDecoder.Height), Convert.ToInt32(args.Clip.Width * itDecoder.Width), Convert.ToInt32(args.Clip.Height * itDecoder.Height));

					imageCopier.Copy(itDecoder, cropFile, new ImageProcessing.FileFormat.Jpeg(85), clipRect);

					this.parentWindow.Dispatcher.Invoke(this.dlgInsertImage, args.SessionIndex, cropFile, illImage.ColorMode, illImage.Dpi, illImage.Brightness, illImage.Contrast);

					if (args.DeleteOriginal)
						illImage.Article.Scans.Remove(illImage);
				}

				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationSuccessfull, illImage);
			}
			catch(Exception ex)
			{
				Notify("CropTU(): " + ex.Message, ex);
				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationError, new IllException("Can't crop image!"));
			}
		}
		#endregion

		#region SplitImageTU()
		protected void SplitImageTU(object parameters)
		{
			try
			{
				BscanILL.Hierarchy.IllImage illImage = ((object[])parameters)[0] as BscanILL.Hierarchy.IllImage;
				int imageIndex = (int)((object[])parameters)[1];

				using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(illImage.FilePath.FullName))
				{
					ImageProcessing.BigImages.ImageCopier imageCopier = new ImageProcessing.BigImages.ImageCopier();
					string cropFile1 = illImage.FilePath.FullName + ".crop1";
					string cropFile2 = illImage.FilePath.FullName + ".crop2";

					Rectangle clipRect1 = new Rectangle(0, 0, itDecoder.Width / 2, itDecoder.Height);
					Rectangle clipRect2 = Rectangle.FromLTRB(itDecoder.Width / 2, 0, itDecoder.Width, itDecoder.Height);

					imageCopier.Copy(itDecoder, cropFile1, new ImageProcessing.FileFormat.Jpeg(85), clipRect1);
					imageCopier.Copy(itDecoder, cropFile2, new ImageProcessing.FileFormat.Jpeg(85), clipRect2);

					this.parentWindow.Dispatcher.Invoke(this.dlgInsertPages, imageIndex, cropFile1, cropFile2, illImage.ColorMode, illImage.Dpi, illImage.Brightness, illImage.Contrast, 0);
					//this.parentWindow.Dispatcher.Invoke(this.dlgInsertImage, imageIndex + 1, cropFile2, illImage.ColorMode, illImage.Dpi, illImage.Brightness, illImage.Contrast);
				}

				this.parentWindow.Dispatcher.Invoke(this.dlgDeleteImage, illImage);
				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationSuccessfull, illImage);
			}
			catch(Exception ex)
			{
				Notify("SplitImageTU(): " + ex.Message, ex);
				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationError, new IllException("Can't split image!"));
			}
		}
		#endregion

		#region UnsplitTU()
		/*protected void UnsplitTU(object parameter)
		{
			try
			{
				BscanILL.Hierarchy.IllImage illImage1 = parameter as BscanILL.Hierarchy.IllImage;
				BscanILL.Hierarchy.IllImage illImage2 = (BscanILL.Hierarchy.IllImage)illImage1.AssociatedIllImage;
				BscanILL.Hierarchy.IllImage illImageL = (illImage1.IllImageType == BscanILL.Hierarchy.IllImageType.LeftPage) ? illImage1 : illImage2;
				BscanILL.Hierarchy.IllImage illImageR = (illImageL == illImage2) ? illImage1 : illImage2;

				string file1 = illImageL.FilePath.FullName;
				string file2 = illImageR.FilePath.FullName;
				string result = file1 + ".merged";

				ImageProcessing.BigImages.Merging.MergeHorizontally(file1, file2, result, new ImageProcessing.FileFormat.Jpeg(85), illImageL.AssociatedImageOverlapping);

				int imageIndex = Math.Max(0, illImageL.Session.IllImages.IndexOf(illImageL));

				this.parentWindow.Dispatcher.Invoke(this.dlgInsertImage, imageIndex, result, illImage1.ColorMode, illImage1.Dpi, illImage1.Brightness, illImage1.Contrast);										
				this.parentWindow.Dispatcher.Invoke(this.dlgDeleteImage, illImage1);
				this.parentWindow.Dispatcher.Invoke(this.dlgDeleteImage, illImage2);
				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationSuccessfull);
			}
			catch (Exception ex)
			{
				Notify("UnsplitTU(): " + ex.Message, ex);
				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationError, new IllException("Can't unsplit selected image!"));
			}
		}*/
		#endregion

		#region RotateImageTU()
		protected void RotateImageTU(object parameters)
		{
			try
			{
				BscanILL.Hierarchy.IllImage illImage = ((object[])parameters)[0] as BscanILL.Hierarchy.IllImage;
				int imageIndex = (int)((object[])parameters)[1];
				int angle = (int)((object[])parameters)[2];

				illImage.Rotate(angle);

				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationSuccessfull, illImage);
			}
			catch (Exception ex)
			{
				Notify("RotateImageTU(): " + ex.Message, ex);
				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationError, new IllException("Can't rotate image!"));
			}
		}
		#endregion


        #region RotateSmallAngleImageTU()
        protected void RotateSmallAngleImageTU(object parameters)
		{
			try
			{
				BscanILL.Hierarchy.IllImage illImage = ((object[])parameters)[0] as BscanILL.Hierarchy.IllImage;
				int imageIndex = (int)((object[])parameters)[1];
				int angle = (int)((object[])parameters)[2];

                illImage.RotateSmallAngle(angle);

				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationSuccessfull, illImage);
			}
			catch (Exception ex)
			{
                Notify("RotateSmallImageTU(): " + ex.Message, ex);
				this.parentWindow.Dispatcher.BeginInvoke(this.dlgOperationError, new IllException("Can't small angle rotate image!"));
			}
		}
		#endregion

		#region Notify()
		protected void Notify(string message, Exception ex)
		{
			Notifications.Instance.Notify(this, Notifications.Type.Error, "IllImageOperations, " + message, ex);
		}
		#endregion

		#endregion
	
	}
}
