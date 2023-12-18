using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using BscanILL.Scan;
using BscanILL.Misc;

namespace BscanILL.Export.IP
{
	class ExportFileCreator
	{
		//public ImageProcessing.ProgressHnd ProgressChanged;


		#region constructor
		public ExportFileCreator()
		{
		}
		#endregion


		#region CreateExportFile()
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="iImageFormat"></param>
		/// <param name="colorMode"></param>
		/// <param name="brightnessD">In interval <-1, +1></param>
		/// <param name="contrastD">In interval <-1, +1></param>
		/// <param name="histogram"></param>
		/// <param name="zoom"></param>
		public void CreateExportFile(FileInfo source, FileInfo destination, ImageProcessing.FileFormat.IImageFormat iImageFormat, Scanners.ColorMode colorMode, double zoom)
		{
			source.Refresh();
			destination.Refresh();
			
			if (iImageFormat is ImageProcessing.FileFormat.Jpeg && (colorMode == Scanners.ColorMode.Bitonal))
				colorMode = Scanners.ColorMode.Grayscale;
			
			using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(source.FullName))
			{
				try
				{
					switch (colorMode)
					{
						case Scanners.ColorMode.Color: CreateExportFile24bpp(itDecoder, destination.FullName, iImageFormat, zoom); break;
						case Scanners.ColorMode.Grayscale: CreateExportFile8bpp(itDecoder, destination.FullName, iImageFormat, zoom); break;
						case Scanners.ColorMode.Bitonal:
							CreateExportFile1bpp(itDecoder, destination.FullName, iImageFormat, zoom); break;
						default:
							Notifications.Instance.Notify(this, Notifications.Type.Error, "ExportFileCreator, CreateExportFile(): Unsupported format", null);
							throw new IllException("Can't create export image!");
					}
				}
				catch (IllException ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					source.Refresh();
					destination.Refresh();

					ImageProcessing.ImageFile.ImageInfo imageInfo = new ImageProcessing.ImageFile.ImageInfo(source);

					int width = imageInfo.Width;
					int height = imageInfo.Height;

					string message = string.Format("ExportFileCreator, CreateExportFile(): {0}, FileFormat: {1}, Scanners.ColorMode: {2}, Zoom: {3}, Source Exists: {4}, Dest Exists: {5}, W: {6}, H: {7}",
						ex.Message, iImageFormat, colorMode, zoom, source.Exists, destination.Exists, width, height);

					Notifications.Instance.Notify(this, Notifications.Type.Error, message, ex);
					throw new IllException("Can't Create Export Image!");
				}
			}
		}
		#endregion

		#region CreateExportFileWithLimitedSize()
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="iImageFormat"></param>
		/// <param name="colorMode"></param>
		/// <param name="brightnessD">In interval <-1, +1></param>
		/// <param name="contrastD">In interval <-1, +1></param>
		/// <param name="histogram"></param>
		/// <param name="zoom"></param>
		/// <param name="maxByteSize">Maximum size of the file created, in bytes</param>
		public void CreateExportFileWithLimitedSize(FileInfo source, FileInfo destination, ImageProcessing.FileFormat.IImageFormat iImageFormat, Scanners.ColorMode colorMode, double zoom, ulong maxByteSize)
		{
			if (maxByteSize <= 0)
			{
				CreateExportFile(source, destination, iImageFormat, colorMode, zoom);
				return;
			}

			double initialZoom = zoom;

			do
			{
				CreateExportFile(source, destination, iImageFormat, colorMode, zoom);
				destination.Refresh();

				if ((ulong)destination.Length > maxByteSize)
				{
					//adjust zoom
					double ratio = ((double)maxByteSize / destination.Length) * 0.9;
					//zoom = initialZoom * Math.Min(0.9, Math.Sqrt(ratio));
					zoom = zoom * Math.Min(0.9, Math.Sqrt(ratio));
				}
			} while ((ulong)destination.Length > maxByteSize);
		}
		#endregion

		//PRIVATE METHODS
		#region private methods

		#region CreateExportFile24bpp()
		private void CreateExportFile24bpp(ImageProcessing.BigImages.ItDecoder itDecoder, string destFile, ImageProcessing.FileFormat.IImageFormat iImageFormat, double zoom)
		{
			if (itDecoder.PixelsFormat != ImageProcessing.PixelsFormat.Format24bppRgb)
			{
				if (zoom != 1)
				{
					ImageProcessing.BigImages.ResizingAndResampling resizing = new ImageProcessing.BigImages.ResizingAndResampling();
					resizing.ResizeAndResample(itDecoder, destFile, iImageFormat, ImageProcessing.PixelsFormat.Format8bppGray, zoom);
				}
				else
				{
					ImageProcessing.BigImages.Resampling resampling = new ImageProcessing.BigImages.Resampling();
					resampling.Resample(itDecoder, destFile, iImageFormat, ImageProcessing.PixelsFormat.Format8bppGray);
				}
			}
			else
			{
				if (zoom != 1)
				{
					ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing();
					resizing.Resize(itDecoder, destFile, iImageFormat, zoom);
				}
				else
				{
					ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();
					copier.Copy(itDecoder, destFile, iImageFormat, Rectangle.Empty);
				}
			}
		}
		#endregion

		#region CreateExportFile8bpp()
		private void CreateExportFile8bpp(ImageProcessing.BigImages.ItDecoder itDecoder, string destFile, ImageProcessing.FileFormat.IImageFormat iImageFormat, double zoom)
		{
			if (itDecoder.PixelsFormat != ImageProcessing.PixelsFormat.Format8bppGray || itDecoder.PixelsFormat != ImageProcessing.PixelsFormat.Format8bppIndexed)
			{
				if (zoom != 1)
				{
					ImageProcessing.BigImages.ResizingAndResampling resizing = new ImageProcessing.BigImages.ResizingAndResampling();
					resizing.ResizeAndResample(itDecoder, destFile, iImageFormat, ImageProcessing.PixelsFormat.Format8bppGray, zoom);
				}
				else
				{
					ImageProcessing.BigImages.Resampling resampling = new ImageProcessing.BigImages.Resampling();
					resampling.Resample(itDecoder, destFile, iImageFormat, ImageProcessing.PixelsFormat.Format8bppGray);
				}
			}
			else
			{
				if (zoom != 1)
				{
					ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing();
					resizing.Resize(itDecoder, destFile, iImageFormat, zoom);
				}
				else
				{
					ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();
					copier.Copy(itDecoder, destFile, iImageFormat, Rectangle.Empty);
				}
			}
		}
		#endregion

		#region CreateExportFile1bpp()
		private void CreateExportFile1bpp(ImageProcessing.BigImages.ItDecoder itDecoder, string destFile, ImageProcessing.FileFormat.IImageFormat iImageFormat, double zoom)
		{
			if (itDecoder.PixelsFormat != ImageProcessing.PixelsFormat.FormatBlackWhite)
			{
				if (zoom != 1)
				{
					string tempFile = destFile + ".tmp";

					ImageProcessing.BigImages.ResizingAndResampling resizing = new ImageProcessing.BigImages.ResizingAndResampling();
					resizing.ResizeAndResample(itDecoder, tempFile, new ImageProcessing.FileFormat.Png(), ImageProcessing.PixelsFormat.Format8bppGray, zoom);

					ImageProcessing.Histogram histogram = new ImageProcessing.Histogram(tempFile);
					int gray = (int)(histogram.ThresholdB * 0.114F + histogram.ThresholdG * 0.587F + histogram.ThresholdR * 0.299F);
					byte threshold = (byte)Math.Max(1, Math.Min(254, gray));

					ImageProcessing.BigImages.Binarization binorization = new ImageProcessing.BigImages.Binarization();
					ImageProcessing.BigImages.Binarization.BinarizationParameters parameters = new ImageProcessing.BigImages.Binarization.BinarizationParameters(threshold, threshold, threshold, 0, new ImageProcessing.ColorD(127, 127, 127));

					binorization.Threshold(tempFile, destFile, iImageFormat, parameters);

					File.Delete(tempFile);
				}
				else
				{
					ImageProcessing.BigImages.Binarization binorization = new ImageProcessing.BigImages.Binarization();
					ImageProcessing.Histogram histogram = new ImageProcessing.Histogram(itDecoder);
					ImageProcessing.BigImages.Binarization.BinarizationParameters parameters = new ImageProcessing.BigImages.Binarization.BinarizationParameters(histogram.ThresholdR, histogram.ThresholdG, histogram.ThresholdB, 0, histogram.Mean);

					binorization.Threshold(itDecoder, destFile, iImageFormat, parameters);
				}
			}
			else
			{
				if (zoom != 1)
				{
					ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing();
					resizing.Resize(itDecoder, destFile, iImageFormat, zoom);
				}
				else
				{
					ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();
					copier.Copy(itDecoder, destFile, iImageFormat, Rectangle.Empty);
				}
			}
		}
		#endregion

		#region RaiseProgressChanged()
		/*private void RaiseProgressChanged(float progress)
		{
			if (ProgressChanged != null)
				ProgressChanged(0.33F);
		}*/
		#endregion

		#endregion
	
	}
}
