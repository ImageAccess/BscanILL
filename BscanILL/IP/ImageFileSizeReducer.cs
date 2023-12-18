using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BscanILL.Misc;

namespace BscanILL.IP
{
	/// <summary>
	/// ImageFileSizeReducer class was built to reduce images inserted into PDF file to fit in limited file space like emails. It tries to fit all images within
	/// specified max size limit, but minimum DPI for color and grayscale images is 100 and minimum bitonal DPI is 200, unles the image is bigger than max size.
	/// </summary>
	public class ImageFileSizeReducer
	{
		public event BscanILL.Export.ProgressChangedHnd ProgressChanged;
		public event BscanILL.Export.ProgressDescriptionHnd DescriptionChanged;


		#region constructor
		public ImageFileSizeReducer()
		{
		}
		#endregion


		#region class ResizedImage
		class ResizedImage
		{
			public readonly FileInfo				File;
			public readonly ushort					OrigDpi;
			public readonly long					OrigSize;
			public readonly Scanners.ColorMode ColorMode;
			public ushort							CurentDpi;
			public long								CurrentSize;
			public bool								CanReduce = true;

			public ResizedImage(FileInfo file, ushort dpi, long size, Scanners.ColorMode colorMode, bool canReduce)
			{
				this.File = file;
				this.OrigDpi = CurentDpi = dpi;
				this.OrigSize = this.CurrentSize = size;
				this.ColorMode = colorMode;
				this.CanReduce = canReduce;
			}
		}
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region ReduceImageSizesIfNecessary()
		public void ReduceImageSizesIfNecessary(List<FileInfo> images, double maxFileSize)
		{
			try
			{
				Description_Changed("Resizing Images...");
				ReduceOversizeImages(images, maxFileSize);

				ulong size = GetExportFilesSize(images);

				if (size < maxFileSize)
					return;

				List<ResizedImage> bitonalImages = new List<ResizedImage>();
				List<ResizedImage> colorImages = new List<ResizedImage>();

				foreach (FileInfo img in images)
				{
                    //recreate missing images - to safe processing, some images might be missing - if we did not need to recreate them at export time - in case we use the originals used for orignal Abbyy format xml file

                    img.Refresh();
					ImageProcessing.ImageFile.ImageInfo imageInfo = new ImageProcessing.ImageFile.ImageInfo(img);

					if (imageInfo.PixelsFormat != ImageProcessing.PixelsFormat.FormatBlackWhite)
						colorImages.Add(new ResizedImage(img, (ushort)imageInfo.DpiH, img.Length, Scanners.ColorMode.Color, (imageInfo.DpiH > 100)));
					else
						bitonalImages.Add(new ResizedImage(img, (ushort)imageInfo.DpiH, img.Length, Scanners.ColorMode.Bitonal, (imageInfo.DpiH > 200)));
				}

				ReduceImages(bitonalImages, colorImages, maxFileSize);
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "ImageFileSizeReducer, ReduceImageSizesIfNecessary(): " + ex.Message, ex);
			}
		}

        public void ReduceImageSizesIfNecessary(List<BscanILL.Export.ExportFiles.Abbyy.SpdfStruct> imagesStruct, double maxFileSize)
        {
            try
            {
                Description_Changed("Resizing Images...");
                ReduceOversizeImages(imagesStruct, maxFileSize);

                //ulong size = GetExportFilesSize(images);
                ulong size = GetExportFilesSize(imagesStruct);

                if (size < maxFileSize)
                    return;

                // now we must resize images to fit maxFileSize...

                List<ResizedImage> bitonalImages = new List<ResizedImage>();
                List<ResizedImage> colorImages = new List<ResizedImage>();

                //foreach (FileInfo img in images)
                foreach (BscanILL.Export.ExportFiles.Abbyy.SpdfStruct imgStruct in imagesStruct)
                {
                    FileInfo img;
                    
                    if (imgStruct.ImageToInsert != null)
                    {
                        img = imgStruct.ImageToInsert;
                    }
                    else
                    {
                        //recreate missing images - to safe processing time, some images might be missing - if we did not need to recreate them at export time - in case we use the originals used for orignal Abbyy format xml file    
                        //we are not 100% sure that all images will be reduced becasue ReduceImages() below first reduces just color images and if it fals below size limit, it does not touch bitonal images...
                        // but to make the code simplier - we will recreate all images at this point 

                        //we will need to recreate the old source file for resizing below 
                        DirectoryInfo tempDir = BscanILL.Export.ExportFiles.PdfsBuilder.Instance.OcrImageDirectoryInfo;
                        Scanners.ColorMode tempImageColorMode = Scanners.ColorMode.Unknown;

                        FileInfo ocrFile = BscanILL.Export.ExportFiles.PdfsBuilder.Instance.GetOcrImageExternal(imgStruct.IllPage, tempDir, imgStruct.IllPage.ColorDepthOfPreProcessedPDF, imgStruct.IllPage.ExportQualityOfPreProcessedPDF, ref tempImageColorMode);

                        ocrFile.Refresh();
                        imgStruct.ImageToInsert = ocrFile;                        

                        img = imgStruct.ImageToInsert;                        
                    }

                    //for safety we will just reprocess all images with Abbyy even we are not sure if bitonal images will be reduced in ReduceImages() below
                    imgStruct.NeedAbbyyProcessing = true;  //need to reprocess with Abbyy after image reduction is finished to get smaller pdf file size

                    img.Refresh();
                    ImageProcessing.ImageFile.ImageInfo imageInfo = new ImageProcessing.ImageFile.ImageInfo(img);

                    if (imageInfo.PixelsFormat != ImageProcessing.PixelsFormat.FormatBlackWhite)
                        colorImages.Add(new ResizedImage(img, (ushort)imageInfo.DpiH, img.Length, Scanners.ColorMode.Color, (imageInfo.DpiH > 100)));
                    else
                        bitonalImages.Add(new ResizedImage(img, (ushort)imageInfo.DpiH, img.Length, Scanners.ColorMode.Bitonal, (imageInfo.DpiH > 200)));
                }

                ReduceImages(bitonalImages, colorImages, maxFileSize);
            }
            catch (Exception ex)
            {
                Notifications.Instance.Notify(this, Notifications.Type.Error, "ImageFileSizeReducer, ReduceImageSizesIfNecessary(): " + ex.Message, ex);
            }
        }
        #endregion

        #endregion


        //PRIVATE METHODS
        #region private methods

        #region GetExportFilesSize()
        private ulong GetExportFilesSize(List<FileInfo> images)
		{
			ulong size = 0;

			foreach (FileInfo img in images)
			{
				img.Refresh();
				size += (ulong)img.Length;
			}

			return size;
		}

        private ulong GetExportFilesSize(List<BscanILL.Export.ExportFiles.Abbyy.SpdfStruct> imagesStruct)
        {
            ulong size = 0;            

            foreach (BscanILL.Export.ExportFiles.Abbyy.SpdfStruct imgStruct in imagesStruct)
            {
                if(imgStruct.ImageToInsert != null)
                {
                    imgStruct.ImageToInsert.Refresh();
                    size += (ulong)imgStruct.ImageToInsert.Length;
                }
                else
                {
                    size += (ulong)imgStruct.IllPage.SourceFileSize;
                }                
            }

            return size;
        }
        
        private ulong GetExportFilesSize(List<ResizedImage> images)
		{
			ulong size = 0;

			foreach (ResizedImage img in images)
			{
				img.File.Refresh();
				size += (ulong)img.File.Length;
			}

			return size;
		}
		#endregion

		#region Progress_Changed()
		private void Progress_Changed(float progress)
		{
			if (ProgressChanged != null)
				ProgressChanged(progress);
		}
		#endregion

		#region Description_Changed()
		private void Description_Changed(string description)
		{
			if (DescriptionChanged != null)
				DescriptionChanged(description);
		}
		#endregion

		#region ReduceOversizeImages()
		private void ReduceOversizeImages(List<FileInfo> images, double maxFileSize)
		{
			try
			{
				Progress_Changed(0);

				ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing();

				for (int i = 0; i < images.Count; i++)
				{
					FileInfo img = images[i];
					img.Refresh();

					while (img.Length > maxFileSize)
					{
						double ratio = Math.Sqrt(((maxFileSize) * 0.9) / (img.Length));

						FileInfo tempFile = new FileInfo(img.FullName + "_tmp");

						using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(img.FullName))
						{
							if (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite)
								resizing.Resize(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), ratio);
							else
								resizing.Resize(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg((byte)80), ratio);
						}

						img.Delete();
						tempFile.MoveTo(img.FullName);
						img.Refresh();
					}

					Progress_Changed((i + 1F) / images.Count);
				}
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "ImageFileSizeReducer, ReduceOversizeImages(): " + ex.Message, ex);
			}
		}

        private void ReduceOversizeImages(List<BscanILL.Export.ExportFiles.Abbyy.SpdfStruct> imagesStruct, double maxFileSize)
        {
            try
            {
                long fileLength = 0;
                Progress_Changed(0);

                ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing();

                for (int i = 0; i < imagesStruct.Count; i++)
                {
                    FileInfo img = null;

                    if (imagesStruct[i].ImageToInsert != null)
                    { 
                      img = imagesStruct[i].ImageToInsert;
                      img.Refresh();
                      fileLength = img.Length;
                    }
                    else
                    {
                        //original ocr'ed image file was already deleted -use data stored/backed up  in structure
                        fileLength = imagesStruct[i].IllPage.SourceFileSize;
                        if (fileLength > maxFileSize)
                        {
                            //we will need to recreate the old source file for resizing below in the loop
                            DirectoryInfo tempDir = BscanILL.Export.ExportFiles.PdfsBuilder.Instance.OcrImageDirectoryInfo;
                            Scanners.ColorMode tempImageColorMode = Scanners.ColorMode.Unknown;

                            FileInfo ocrFile = BscanILL.Export.ExportFiles.PdfsBuilder.Instance.GetOcrImageExternal(imagesStruct[i].IllPage, tempDir, imagesStruct[i].IllPage.ColorDepthOfPreProcessedPDF, imagesStruct[i].IllPage.ExportQualityOfPreProcessedPDF, ref tempImageColorMode);

                            ocrFile.Refresh();
                            imagesStruct[i].ImageToInsert = ocrFile;
                            imagesStruct[i].NeedAbbyyProcessing = true;  //need to reprocess with Abbyy to get smaller pdf file size

                            img = imagesStruct[i].ImageToInsert;
                            img.Refresh();
                            fileLength = img.Length;
                        }
                    }

                    while (fileLength > maxFileSize)
                    {
                        double ratio = Math.Sqrt(((maxFileSize) * 0.9) / (img.Length));

                        FileInfo tempFile = new FileInfo(img.FullName + "_tmp");

                        using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(img.FullName))
                        {
                            if (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite)
                                resizing.Resize(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), ratio);
                            else
                                resizing.Resize(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg((byte)80), ratio);
                        }

                        img.Delete();
                        tempFile.MoveTo(img.FullName);
                        img.Refresh();
                        fileLength = img.Length;

                        //!! maybe this while loop can go to infinite loop in case maxFileSize is too small (hor example 1kB) -> not sure how the loop will behave if image file size is getting close to small number
                    }

                    Progress_Changed((i + 1F) / imagesStruct.Count);
                }
            }
            catch (Exception ex)
            {
                Notifications.Instance.Notify(this, Notifications.Type.Error, "ImageFileSizeReducer, ReduceOversizeImages(): " + ex.Message, ex);
            }
        }
        #endregion

        #region ReduceImages()
        private void ReduceImages(List<ResizedImage> bitonalImages, List<ResizedImage> colorImages, double maxFileSize)
		{
			try
			{
				if (colorImages.Count == 0)
					return;

				ulong colorSize = GetExportFilesSize(colorImages);
				ulong bitonalSize = GetExportFilesSize(bitonalImages);

				ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing();

				//reduce color
				while ((colorSize + bitonalSize > maxFileSize) && (GetImagesToReduce(colorImages).Count > 0))
				{
					Progress_Changed(0);

					ulong canReduceImagesSize = GetExportFilesSize(colorImages);
					double ratio = Math.Sqrt(Math.Max(0.1, ((maxFileSize - bitonalSize - (colorSize - canReduceImagesSize)) * 0.9) / (canReduceImagesSize)));

					for (int i = 0; i < colorImages.Count; i++)
					{
						ResizedImage image = colorImages[i];

						if (image.CanReduce)
						{
							FileInfo img = image.File;
							img.Refresh();

							FileInfo tempFile = new FileInfo(img.FullName + "_tmp");
							ImageProcessing.ImageFile.ImageInfo imageInfo = new ImageProcessing.ImageFile.ImageInfo(img);
							int newDpi = Math.Max(100, Convert.ToInt32(imageInfo.DpiH * ratio));

							using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(img.FullName))
							{
								resizing.Resize(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Jpeg((byte)80), ((double)newDpi / itDecoder.DpiX));
							}

							img.Delete();
							tempFile.MoveTo(img.FullName);
							img.Refresh();

							image.CurentDpi = (ushort)newDpi;
							image.CurrentSize = img.Length;
							image.CanReduce = (newDpi > 100);
						}

						Progress_Changed((i + 1F) / colorImages.Count);
					}

					colorSize = GetExportFilesSize(colorImages);
				}

				//reduce bitonal
				while ((colorSize + bitonalSize > maxFileSize) && (GetImagesToReduce(bitonalImages).Count > 0))
				{
					Progress_Changed(0);

					ulong canReduceImagesSize = GetExportFilesSize(bitonalImages);
					double ratio = Math.Sqrt(Math.Max(0.1, ((maxFileSize - (bitonalSize - canReduceImagesSize) - colorSize) * 0.9) / (canReduceImagesSize)));

					for (int i = 0; i < bitonalImages.Count; i++)
					{
						ResizedImage image = bitonalImages[i];

						if (image.CanReduce)
						{
							FileInfo img = image.File;
							img.Refresh();

							FileInfo tempFile = new FileInfo(img.FullName + "_tmp");
							ImageProcessing.ImageFile.ImageInfo imageInfo = new ImageProcessing.ImageFile.ImageInfo(img);
							int newDpi = Math.Max(200, (Convert.ToInt32(imageInfo.DpiH * ratio) / 100) * 100);

							using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(img.FullName))
							{
								resizing.Resize(itDecoder, tempFile.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), ((double)newDpi / itDecoder.DpiX));
							}

							img.Delete();
							tempFile.MoveTo(img.FullName);
							img.Refresh();

							image.CurentDpi = (ushort)newDpi;
							image.CurrentSize = img.Length;
							image.CanReduce = (newDpi > 200);
						}

						Progress_Changed((i + 1F) / bitonalImages.Count);
					}

					bitonalSize = GetExportFilesSize(bitonalImages);
				}
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "ImageFileSizeReducer, ReduceImages(): " + ex.Message, ex);
			}
		}
		#endregion

		#region GetImagesToReduce()
		private List<ResizedImage> GetImagesToReduce(List<ResizedImage> images)
		{
			List<ResizedImage> list = new List<ResizedImage>();

			foreach (ResizedImage image in images)
				if (image.CanReduce)
					list.Add(image);

			return list;
		}
		#endregion

		#endregion

	}
}
