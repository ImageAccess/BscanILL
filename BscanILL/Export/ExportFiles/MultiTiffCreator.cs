using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Data;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using BscanILL.Misc;


namespace BscanILL.Export.ExportFiles
{
	class MultiTiffCreator : ExportFilesBasics
	{

		#region constructor
		public MultiTiffCreator()
		{
		}
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Export()
		public void Export(BscanILL.Hierarchy.IllPage illPage, FileInfo outputFile)
		{
			BscanILL.Hierarchy.IllPages illPages = new BscanILL.Hierarchy.IllPages();
			illPages.Add(illPage);

			Export(illPages, outputFile);
		}

		public void Export(BscanILL.Hierarchy.IllPages illPages, FileInfo outputFile)
		{
			ImageCodecInfo codecInfo = ImageProcessing.Encoding.GetCodecInfo(ImageFormat.Tiff);
			Bitmap multiImage = null;
			//List<FileInfo>	filesToDelete = new List<FileInfo>();

			outputFile.Directory.Create();
			outputFile.Refresh();

			if (outputFile.Exists)
				outputFile.Delete();

			try
			{
				List<string> warnings = new List<string>();

				for (int i = 0; i < illPages.Count; i++)
				{
					//FileInfo file = BscanILL.Export.ExportFiles.GetFile(illPages[i], new BscanILL.ExportFileSettings(BscanILL.Scan.FileFormat.Tiff, dpi), warnings, 0);
					//filesToDelete.Add(file);

					//Bitmap bitmap = illPages[i].GetPreview(, );// new Bitmap(file.FullName);
					BscanILL.Hierarchy.IllPage k = illPages[i];
					BscanILL.IP.PreviewCreator creator = BscanILL.IP.PreviewCreator.Instance;
					Bitmap bitmap = creator.GetPreview(k.FilePath, k.ColorMode, 1, ImageProcessing.Resizing.ResizeMode.Quality);

					if (i == 0)
					{
						EncoderParameters encoderParams = new EncoderParameters(3);

						multiImage = bitmap;
						encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
						if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
							encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionCCITT4);
						else
							encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionNone);
						encoderParams.Param[2] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, GetBitDepth(bitmap.PixelFormat));

						multiImage.Save(outputFile.FullName, codecInfo, encoderParams);
					}
					else
					{
						EncoderParameters encoderParams = new EncoderParameters(3);
						encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
						if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
							encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionCCITT4);
						else
							encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionNone);
						encoderParams.Param[2] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, GetBitDepth(bitmap.PixelFormat));

						multiImage.SaveAdd(bitmap, encoderParams);
						bitmap.Dispose();
					}

					Progress_Changed((i + 1.0F) / (illPages.Count));
				}

				EncoderParameters encoderParameters = new EncoderParameters(1);
				encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.Flush);

				multiImage.SaveAdd(encoderParameters);
				multiImage.Dispose();

				outputFile.Refresh();
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "MultiTiffCreator, Export(): " + ex.Message, ex);
				throw ex;
			}
			finally
			{
				/*foreach (FileInfo fileToDelete in filesToDelete)
				{
					try
					{
						fileToDelete.Refresh();
						fileToDelete.Delete();
					}
					catch { }
				}*/
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region GetPixelFormat()
		static PixelFormat GetPixelFormat(BscanILL.Hierarchy.IllPage illPage)
		{
			switch (illPage.ColorMode)
			{
				case Scanners.ColorMode.Bitonal:
					return PixelFormat.Format1bppIndexed;
				case Scanners.ColorMode.Grayscale:
					return PixelFormat.Format8bppIndexed;
				default:
					return PixelFormat.Format24bppRgb;
			}
		}
		#endregion

		#region GetBitDepth()
		static private long GetBitDepth(PixelFormat pixelFormat)
		{
			switch (pixelFormat)
			{
				case PixelFormat.Format1bppIndexed: return 1;
				case PixelFormat.Format4bppIndexed: return 4;
				case PixelFormat.Format8bppIndexed: return 8;
				case PixelFormat.Format32bppArgb: return 32;
				case PixelFormat.Format32bppPArgb: return 32;
				case PixelFormat.Format32bppRgb: return 32;
				default: return 24;
			}
		}
		#endregion

		#endregion

	}
}
