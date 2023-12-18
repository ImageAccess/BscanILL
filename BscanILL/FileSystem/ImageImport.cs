using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;


namespace BscanILL.FileSystem
{
	class ImageImport
	{
		public delegate void ProgressChangedHnd(double progress);
		public event ProgressChangedHnd ProgressChanged;


		#region GetImportImages()
		public List<ImportedImage> GetImportImages(ObservableCollection<FileInfo> imageFiles)
		{
			List<ImportedImage> importImages = new List<ImportedImage>();

			for (int i = 0; i < imageFiles.Count; i++)
			{
				FileInfo file = imageFiles[i];
				Scanners.ColorMode colorMode;
				Scanners.FileFormat fileFormat;
				ushort dpi;
				DirectoryInfo destFolder = new DirectoryInfo(BscanILL.SETTINGS.Settings.Instance.General.TempDir);

				if (file.Extension.ToLower().Contains("pdf"))
				{
					BscanILL.FileSystem.PdfExtractor pdfExtractor = new BscanILL.FileSystem.PdfExtractor();

					pdfExtractor.ProgressChanged += delegate(double progress)
					{
						RaiseProgressChanged(((double)progress / imageFiles.Count) + ((double)i / imageFiles.Count));
					};

                    List<ImportedImage> fileImportImages = pdfExtractor.ExtractImages(file.FullName, destFolder);                    

					foreach (ImportedImage importImage in fileImportImages)
					{
						importImages.Add(importImage);
					}
				}
				else
				{
					using (ImageProcessing.BigImages.ItDecoder decoder = new ImageProcessing.BigImages.ItDecoder(file.FullName))
					{
						if (decoder.FramesCount > 1)
						{
							for (int frameIndex = 0; frameIndex < decoder.FramesCount; frameIndex++)
							{
								string newPath;

								decoder.SelectFrame((uint)frameIndex);

								using (Bitmap bitmap = decoder.GetImage(frameIndex))
								{
									dpi = Convert.ToUInt16(bitmap.HorizontalResolution);

									if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
									{
										newPath = GetUniqueFile(destFolder, ImageFormat.Tiff);
										fileFormat = Scanners.FileFormat.Tiff;
										colorMode = Scanners.ColorMode.Bitonal;
										bitmap.Save(newPath, ImageFormat.Tiff);
									}
									else
									{
										newPath = GetUniqueFile(destFolder, ImageFormat.Jpeg);
										fileFormat = Scanners.FileFormat.Jpeg;
										colorMode = (bitmap.PixelFormat == PixelFormat.Format8bppIndexed) ? Scanners.ColorMode.Grayscale : Scanners.ColorMode.Color;
										bitmap.Save(newPath, ImageFormat.Jpeg);
									}

									importImages.Add(new ImportedImage(newPath, colorMode, fileFormat, dpi, true));
								}

								RaiseProgressChanged((frameIndex + 1.0 / decoder.FramesCount) / imageFiles.Count + ((double)i / imageFiles.Count));
							}
						}
						else
						{
							switch (decoder.PixelsFormat)
							{
								case ImageProcessing.PixelsFormat.FormatBlackWhite: colorMode = Scanners.ColorMode.Bitonal; break;
								case ImageProcessing.PixelsFormat.Format8bppGray:
								case ImageProcessing.PixelsFormat.Format8bppIndexed:
									colorMode = Scanners.ColorMode.Grayscale; break;
								default: colorMode = Scanners.ColorMode.Color; break;
							}

							fileFormat = Scanners.Misc.GetImageFormat(decoder.ImageInfo.CodecInfo.FormatID);
							dpi = (ushort)decoder.DpiX;

							importImages.Add(new ImportedImage(file.FullName, colorMode, fileFormat, dpi, false));
						}
					}
				}

				RaiseProgressChanged((i + 1.0) / imageFiles.Count);
			}

			return importImages;
		}
		#endregion

		#region GetUniqueFile()
		public static string GetUniqueFile(DirectoryInfo dir, ImageFormat imageFormat)
		{
			int index = 0;
			string fileName;
			string extension = (imageFormat == ImageFormat.Tiff) ? ".tif" : ".jpg";

			do
			{
				fileName = index.ToString("000") + extension;
				index++;
			} while (File.Exists(dir.FullName + @"\" + fileName));

			return dir.FullName + @"\" + fileName;
		}
		#endregion

		#region RaiseProgressChanged()
		void RaiseProgressChanged(double progress)
		{
			if (ProgressChanged != null)
				ProgressChanged(progress);
		}
		#endregion

	}
}
