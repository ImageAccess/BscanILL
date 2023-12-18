using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using ImageProcessing;
using ImageProcessing.ImageFile;
using System.Threading;
using BscanILL.Hierarchy;


namespace BscanILL.UI.Frames.Edit
{
	class SkipItExecution
	{
		public delegate void ProgressChangedHnd(Article article, double progress);
		public delegate void ProcessSuccessfullHnd(Article article, List<SkipItExecutionPage> files);
		public delegate void ProcessErrorHnd(Article article, Exception ex);
		public delegate void ProgressDescriptionHnd(Article article, string description);

		public event ProgressChangedHnd		ProgressChanged;
		public event ProcessSuccessfullHnd	OperationDone;
		public event ProcessErrorHnd		OperationError;

        public event BscanILL.Misc.VoidHnd CreateExportImageDeriv;  
		
		#region constructor
		public SkipItExecution()
		{
		}
		#endregion


		public class SkipItExecutionPage
		{
			public readonly IllImage					IllImage;
			public readonly FileInfo					File;
			public readonly int							PageIndex;
			public readonly Scanners.FileFormat			FileFormat;
			public readonly Scanners.ColorMode			ColorMode;
			public readonly ushort						Dpi;
			public readonly double						Brightness;
			public readonly double						Contrast;

			public SkipItExecutionPage(IllImage illImage, FileInfo file, int pageIndex, Scanners.FileFormat fileFormat, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast)
			{
				this.IllImage = illImage;
				this.File = file;
				this.PageIndex = pageIndex;
				this.FileFormat = fileFormat;
				this.ColorMode = colorMode;
				this.Dpi = dpi;
				this.Brightness = brightness;
				this.Contrast = contrast;
			}
		}


		// PUBLIC METHODS
		#region public methods

		#region Execute()
		public void Execute(Article	article,  SessionBatch batch)
		{
			try
			{
                System.IO.DirectoryInfo exportDir;
                int index = 0;
                int pagesTotal = 0;

				List<SkipItExecutionPage> files = new List<SkipItExecutionPage>();

                foreach (Article article_item in batch.Articles)
                {
                    //pagesTotal += article_item.Scans.Count + 1;
                    pagesTotal += article_item.Scans.Count ;
                    if (article_item.Pullslip != null)
                    {
                        pagesTotal++;
                    }

                }

                foreach( Article article_item in batch.Articles )
                {
                    exportDir = new System.IO.DirectoryInfo(article_item.PagesPath);
                    exportDir.Create();

				    FileInfo[] existingFiles = exportDir.GetFiles();
                    
				    foreach (FileInfo file in existingFiles)
				    {
					    try { file.Delete(); }
					    catch { }
				    }

                    DirectoryInfo[] existingDirectories = exportDir.GetDirectories();
                    foreach (DirectoryInfo dirName in existingDirectories)
                    {
                        try { dirName.Delete(true); }
                        catch { }
                    }

				    //if (article.Pullslip != null)
                    if (article_item.Pullslip != null)
				    {
                        index++;
					    //DoTheImage(article.Pullslip, 0, exportDir, ref files);
                        DoTheImage(article_item.Pullslip, 0, exportDir, ref files);

					    if (ProgressChanged != null)
						   // ProgressChanged(article, ((1.0) / (article.Scans.Count + 1)));
                            ProgressChanged(article_item, ((double)index / (double) pagesTotal) * 0.2 );     //copying of all source scannned images treat as 20% of job done
				    }

				    //for (int i = 0; i < article.Scans.Count; i++)
                    for (int i = 0; i < article_item.Scans.Count; i++)
				    {
                        index++;
					    //DoTheImage(article.Scans[i], i + 1, exportDir, ref files);
                        DoTheImage(article_item.Scans[i], i + 1, exportDir, ref files);

					    if (ProgressChanged != null)
						    //ProgressChanged(article, ((i + 2.0) / (article.Scans.Count + 1)));
                            ProgressChanged(article_item, ((double)index / (double)pagesTotal) * 0.2);      //copying of all source scannned images treat as 20% of job done
				    }
                }

                //lines below are moved from OperationDone event in below ->FrameEditUi.SkipItExecutionSuccessfull()
                foreach (BscanILL.UI.Frames.Edit.SkipItExecution.SkipItExecutionPage pair in files)
                {
                				if (pair.IllImage != null)
                					pair.IllImage.IllPages.Add(pair.IllImage, pair.File, pair.FileFormat, pair.ColorMode, pair.Dpi, pair.Brightness, pair.Contrast);
                }
                
                if (CreateExportImageDeriv != null)     //creates thumbnail, reduced image, atd for all images in batch
                    CreateExportImageDeriv();

				if (OperationDone != null)
					OperationDone(article, files);
			}
			catch (Exception ex)
			{
				if (OperationError != null)
					OperationError(article, ex);
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region DoTheImage()
		private void DoTheImage(IllImage illImage, int imageNameCounter, DirectoryInfo exportDir, ref List<SkipItExecutionPage> itExecutionPages)
		{
			Scanners.FileFormat fileFormat;
			Scanners.ColorMode colorMode;
			ImageProcessing.FileFormat.IImageFormat imageFileFormat;
			ushort dpi = (ushort)illImage.FullImageInfo.DpiH;

			switch (illImage.FullImageInfo.PixelsFormat)
			{
				case PixelsFormat.FormatBlackWhite:
					fileFormat = Scanners.FileFormat.Png;
					imageFileFormat = new ImageProcessing.FileFormat.Png();
					colorMode = Scanners.ColorMode.Bitonal;
					break;
				case PixelsFormat.Format8bppGray:
				case PixelsFormat.Format8bppIndexed:
					fileFormat = Scanners.FileFormat.Png;
					imageFileFormat = new ImageProcessing.FileFormat.Png();
					colorMode = Scanners.ColorMode.Grayscale;
					break;
				default:
					fileFormat = Scanners.FileFormat.Jpeg;
					imageFileFormat = new ImageProcessing.FileFormat.Jpeg(85);
					colorMode = Scanners.ColorMode.Color;
					break;
			}

			string destinationFile = Path.Combine(exportDir.FullName, (imageNameCounter++).ToString("000000") + Path.GetExtension(illImage.FilePath.FullName));
			File.Copy(illImage.FilePath.FullName, destinationFile);
			itExecutionPages.Add(new SkipItExecutionPage(illImage, new FileInfo(destinationFile), 0, fileFormat, colorMode, dpi, 0, 0));
		}
		#endregion

		#endregion
	
	}
}
