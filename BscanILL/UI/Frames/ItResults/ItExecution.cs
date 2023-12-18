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


namespace BscanILL.UI.Frames.ItResults
{
	class ItExecution
	{
		public delegate void ProgressChangedHnd(Article article, double progress);
		public delegate void ProcessSuccessfullHnd(Article article, List<ItExecutionPage> files);
		public delegate void ProcessErrorHnd(Article article, Exception ex);
		public delegate void ProgressDescriptionHnd(Article article, string description);

		public event ProgressChangedHnd		ProgressChanged;
		public event ProcessSuccessfullHnd	OperationDone;
		public event ProcessErrorHnd		OperationError;
	
		
		
		#region constructor
		public ItExecution()
		{
		}
		#endregion


		public class ItExecutionPage
		{
			public readonly ViewPane.Hierarchy.VpImage	VpImage;
			public readonly FileInfo					File;
			public readonly int							PageIndex;
			public readonly Scanners.FileFormat			FileFormat;
			public readonly Scanners.ColorMode			ColorMode;
			public readonly ushort						Dpi;
			public readonly double						Brightness;
			public readonly double						Contrast;

			public ItExecutionPage(ViewPane.Hierarchy.VpImage vpImage, FileInfo file, int pageIndex, Scanners.FileFormat fileFormat, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast)
			{
				this.VpImage = vpImage;
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
		public void Execute(Article	article, ViewPane.Hierarchy.VpImages vpImages, DirectoryInfo exportDir)
		{
			try
			{
				List<ItExecutionPage> files = new List<ItExecutionPage>();
				int imageNameCounter = 1;

				FileInfo[] existingFiles = exportDir.GetFiles();

				foreach (FileInfo file in existingFiles)
				{
					try { file.Delete(); }
					catch { }
				}

				for (int i = 0; i < vpImages.Count; i++)
				{
					DoTheImage(vpImages[i], ref imageNameCounter, exportDir, ref files);

					if (ProgressChanged != null)
						ProgressChanged(article, ((i + 1.0) / vpImages.Count));
				}

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
		private void DoTheImage(ViewPane.Hierarchy.VpImage vpImage, ref int imageNameCounter, DirectoryInfo exportDir, ref List<ItExecutionPage> itExecutionPages)
		{
			Scanners.FileFormat						fileFormat ;
			string									extension ;
			Scanners.ColorMode						colorMode;
			ImageProcessing.FileFormat.IImageFormat imageFileFormat;
			ushort									dpi = (ushort)vpImage.FullImageInfo.DpiH;

			switch(vpImage.FullImageInfo.PixelsFormat)
			{
				case PixelsFormat.FormatBlackWhite:
					fileFormat = Scanners.FileFormat.Png;
					extension = ".png";
					imageFileFormat =  new ImageProcessing.FileFormat.Png();
					colorMode = Scanners.ColorMode.Bitonal;
					break;
				case PixelsFormat.Format8bppGray :
				case PixelsFormat.Format8bppIndexed:
					fileFormat = Scanners.FileFormat.Png;
					extension = ".png";
						imageFileFormat =  new ImageProcessing.FileFormat.Png();
				colorMode = Scanners.ColorMode.Grayscale;
					break;
				default:
					fileFormat = Scanners.FileFormat.Jpeg;
					extension = ".jpg";
					imageFileFormat =  new ImageProcessing.FileFormat.Jpeg(85);
					colorMode = Scanners.ColorMode.Color;
					break;
			}

			if (vpImage.IsFixed || vpImage.ItImage == null)
			{
				string destinationFile = Path.Combine(exportDir.FullName, (imageNameCounter++).ToString("000000") + Path.GetExtension(vpImage.FullPath));
				File.Copy(vpImage.FullPath, destinationFile);
				itExecutionPages.Add(new ItExecutionPage(vpImage, new FileInfo(destinationFile), 0, fileFormat, colorMode, dpi, 0, 0));
			}
			else
			{
				ImageProcessing.IpSettings.ItImage itImage = vpImage.ItImage;
				ImageProcessing.IpSettings.ItPage pageL = itImage.PageL;
				ImageProcessing.IpSettings.ItPage pageR = itImage.PageR;

				string destinationFile = Path.Combine(exportDir.FullName, (imageNameCounter++).ToString("000000") + extension);
				itImage.Execute(vpImage.FullPath, 0, destinationFile, new ImageProcessing.FileFormat.Png());
				itExecutionPages.Add(new ItExecutionPage(vpImage, new FileInfo(destinationFile), 0, fileFormat, colorMode, dpi, 0, 0));

				if (itImage.TwoPages)
				{
					destinationFile = Path.Combine(exportDir.FullName, (imageNameCounter++).ToString("000000") + extension);

					itImage.Execute(vpImage.FullPath, 1, destinationFile, new ImageProcessing.FileFormat.Png());
					itExecutionPages.Add(new ItExecutionPage(vpImage, new FileInfo(destinationFile), 1, fileFormat, colorMode, dpi, 0, 0));
				}
			}
		}
		#endregion

		#endregion
	
	}
}
