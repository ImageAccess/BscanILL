using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Printing ;
using System.Drawing.Imaging ;
using System.IO ;
using System.Management ;
using System.Threading;
using System.Collections.Generic;
using System.Windows;
using System.Printing;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security;
using BscanILL.Misc;


namespace BscanILL.Export.Printing
{
	public class PrintOption : ExportBasics
	{
		#region variables
		//PrintBatch				lastBatch = null;
		Size					paperSize;
		bool					duplexEnabled = false;
		//int						printsFinished = 0;

		PrinterProfile			printerProfile;

		private object			queuesLocker = new object();
		private object			printLocker = new object();
		
		DirectoryInfo			temporaryDir;

		#endregion 


		#region constructor
		private PrintOption()
		{
		}

		public PrintOption(BscanILL.Export.Printing.PrinterProfile printerProfile)
		{
			try
			{
				this.printerProfile = printerProfile;

				IIllPrinter			printer = this.printerProfile.GetPrinter();
				IPaperSize			paperSize = printer.PaperSize;

				this.paperSize = new Size((paperSize != null && paperSize.WidthInInches != null) ? paperSize.WidthInInches.Value : 8.5, (paperSize != null && paperSize.HeightInInches != null) ? paperSize.HeightInInches.Value : 11);
				this.duplexEnabled = printer.DuplexAvailable;

				temporaryDir = new DirectoryInfo(_settings.General.TempDir + @"\Print");

				try
				{
					temporaryDir.Refresh();

					if (temporaryDir.Exists)
						foreach (FileInfo file in temporaryDir.GetFiles())
							file.Delete();
				}
				catch { }

				temporaryDir.Refresh();
				temporaryDir.Create();

				Init();
			}
			catch (Exception ex)
			{
				string message = ex.Message;
				Exception e = ex.InnerException;

				while (e != null)
				{
					message += e.Message + Environment.NewLine;
					e = e.InnerException;
				}

				Notifications.Instance.Notify(null, Notifications.Type.Error, "Can't initialize printer!" + " " + message, ex);
				throw new IllException("Can't initialize printer!");
			}
		}
		#endregion

		#region destructor
		protected void Dispose()
		{
			try
			{
				temporaryDir.Refresh();

				if (temporaryDir.Exists)
					foreach (FileInfo file in temporaryDir.GetFiles())
						file.Delete();
			}
			catch { }
		}
		#endregion

		#region class PrintedImage
		public class PrintedImage
		{
			public readonly BscanILL.Hierarchy.IllImage IllImage = null;
			public readonly System.Windows.Rect? Clip = null;

			public PrintedImage(BscanILL.Hierarchy.IllImage dlsgImage)
			{
				this.IllImage = dlsgImage;
			}

			public PrintedImage(BscanILL.Hierarchy.IllImage dlsgImage, System.Windows.Rect? clip)
			{
				this.IllImage = dlsgImage;
				this.Clip = clip;
			}

		}
		#endregion

		#region class PrintBatch
		public class PrintBatch : List<PrintedImage>
		{
			public readonly BscanILL.Export.Printing.PrintingOptions PrintingOptions;

			public PrintBatch(BscanILL.Export.Printing.PrintingOptions printingOptions)
			{
				this.PrintingOptions = printingOptions;
			}
		}
		#endregion

		#region class PrintSettings
		/*public class PrintSettings
		{
			public readonly bool DuplexPrinting;
			public readonly int NumberOfCopies;
			public readonly Scanners.ColorMode ColorMode;
			public readonly bool EnhanceColors;
			public readonly double AutoLevelsValue;

			public PrintSettings(bool duplexPrinting, int numberOfCopies, Scanners.ColorMode colorMode, bool enhanceColors, double autoLevelsValue)
			{
				this.DuplexPrinting = duplexPrinting;
				this.NumberOfCopies = numberOfCopies;
				this.ColorMode = colorMode;
				this.EnhanceColors = enhanceColors;
				this.AutoLevelsValue = autoLevelsValue;
			}
		}*/
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		
		//public string						PaperSizeName			{ get { return this.printerProfile.PageMediaSize.PageMediaSizeName.ToString(); } }
		//public string						PaperSizeHumanName		{ get { return this.printerProfile.PageMediaSizeHuman; } }
		//public int							DebitRequestHooksCount	{ get { return (this.DebitRequest == null) ? 0 : this.DebitRequest.GetInvocationList().Length; } }
		public double						InchWidth				{ get { return this.paperSize.Width; } }
		public double						InchHeight				{ get { return this.paperSize.Height; } }
		public bool							IsDuplexPrintingSupported { get { return this.duplexEnabled; } }
		public BscanILL.Export.Printing.PrinterProfile	PrinterProfile			{get{return printerProfile;}}
		
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region PrintTestSheet
		public void PrintTestSheet()
		{
			IIllPrinter printQueue = this.printerProfile.GetPrinter();

			printQueue.PrintTestSheet();
		}
		#endregion
	
		#region Print()
		/*public void Print(BscanILL.Hierarchy.DlsgImages dlsgImages, BscanILL.Export.Printing.PrintingOptions printingOptions)
		{
			try
			{
				if (dlsgImages.Count > 0)
				{
					PrintBatch printBatch = new PrintBatch(printingOptions);

					foreach (BscanILL.Hierarchy.DlsgImage dlsgImage in dlsgImages)
						printBatch.Add(new PrintedImage(dlsgImage));

					DoPrint(printBatch);
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				this.notifications.Notify(this, Notifications.Type.Error, "PrintOption, Print: " + ex.Message, ex);
				throw new IllException(BscanILL.Languages.BscanILLStrings.Printer_CanTPrintImages_STR + ex.Message);
			}
		}*/

		/*public void Print(BscanILL.Hierarchy.DlsgImage dlsgImage, BscanILL.Export.Printing.PrintingOptions printingOptions)
		{
			try
			{
				PrintBatch printBatch = new PrintBatch(printingOptions);
				printBatch.Add(new PrintedImage(dlsgImage, printingOptions.Clip));

				DoPrint(printBatch);
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				this.notifications.Notify(this, Notifications.Type.Error, "PrintOption, Print: " + ex.Message, ex);
				throw new IllException(BscanILL.Languages.BscanILLStrings.Printer_CanTPrintImages_STR + ex.Message);
			}
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Init()
		private void Init()
		{
		}
		#endregion

		#region DoPrint()
		/*private void DoPrint(PrintBatch printBatch)
		{
			try
			{
				using (BscanILL.Export.ExportFiles exportFiles = GetExportFiles(printBatch))
				{
					PrintQueue printQueue = this.printerProfile.GetPrintQueue();
					PrintCapabilities capabilities = printQueue.GetPrintCapabilities();
					PageMediaSize pageMediaSize = printQueue.UserPrintTicket.PageMediaSize;

					this.lastBatch = printBatch;
					this.printsFinished = 0;

					PageImageableArea printable = capabilities.PageImageableArea;
					Thickness margin = new Thickness(printable.OriginWidth, printable.OriginHeight,
						capabilities.OrientedPageMediaWidth.Value - (printable.OriginWidth + printable.ExtentWidth) + 2, capabilities.OrientedPageMediaHeight.Value - (printable.OriginHeight + printable.ExtentHeight) + 2);

					List<System.IO.FileInfo> images = new List<FileInfo>();

					foreach (BscanILL.Export.ExportFile exportFile in exportFiles)
						images.Add(exportFile.File);

					//printing
					Upload_ProgressChanged(0);

					ulong totalFilesSize;
					List<TimeSpan> timeSpans = GetTimeSpans(BscanILL.Scan.ExportMedium.Print, exportFiles, out totalFilesSize);
					Set_ProgressIntervals(ExportDlg.GetProgressName(BscanILL.Scan.ExportMedium.Print, BscanILL.Scan.FileFormat.Unknown), timeSpans);
					DateTime start = DateTime.Now;

					//set print job name
					printQueue.CurrentJobSettings.Description = printBatch.PrintingOptions.PrintJobName;
					bool duplexPrinting = printBatch.PrintingOptions.DuplexPrinting;

					if (duplexPrinting && _settings.Export.Printer.SplitCopiesIntoSeparatePrintJobs && ((images.Count % 2) == 1))
					{
						for (int copy = 0; copy < printBatch.PrintingOptions.Copies; copy++)
						{
							using (PrintPaginator paginator = new PrintPaginator(images, pageMediaSize, margin))
							{
								printQueue.UserPrintTicket.Duplexing = duplexPrinting ? Duplexing.TwoSidedLongEdge : Duplexing.OneSided;

								System.Windows.Xps.XpsDocumentWriter writer = System.Printing.PrintQueue.CreateXpsDocumentWriter(printQueue);

								writer.WritingProgressChanged += new System.Windows.Documents.Serialization.WritingProgressChangedEventHandler(Writer_WritingProgressChanged);
								writer.Write(paginator);
							}
						}
					}
					else
					{
						List<FileInfo> localList = new List<FileInfo>();

						for (int copy = 0; copy < printBatch.PrintingOptions.Copies; copy++)
						{
							localList.AddRange(images);

							//add empty page
							if ((copy < printBatch.PrintingOptions.Copies - 1) && duplexPrinting && ((images.Count % 2) == 1))
								localList.Add(null);
						}

						using (PrintPaginator paginator = new PrintPaginator(localList, pageMediaSize, margin))
						{
							printQueue.UserPrintTicket.Duplexing = duplexPrinting ? Duplexing.TwoSidedLongEdge : Duplexing.OneSided;

							System.Windows.Xps.XpsDocumentWriter writer = System.Printing.PrintQueue.CreateXpsDocumentWriter(printQueue);

							writer.WritingProgressChanged += new System.Windows.Documents.Serialization.WritingProgressChangedEventHandler(Writer_WritingProgressChanged);
							writer.Write(paginator);
						}
					}

					Upload_ProgressChanged(1);
					BscanILL.Misc.TimeEstimates.Instance.SaveExportBytesPerSecond(BscanILL.Scan.ExportMedium.Print, totalFilesSize, DateTime.Now.Subtract(start).TotalSeconds);

					foreach (BscanILL.Export.ExportFile exportFile in exportFiles)
					{
						 Scanners.ColorMode colorMode = (printBatch.PrintingOptions.ColorMode != Scanners.ColorMode.Auto) ? printBatch.PrintingOptions.ColorMode : exportFile.DlsgImages[0].ColorMode;
						exportFile.DlsgImages[0].ImageExported(BscanILL.Scan.FileFormat.Auto, colorMode, 300, BscanILL.Hierarchy.User.UserAction.PrintLetter, exportFile.File.Length);
					}

					GC.Collect();
					this.lastBatch = null;
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				this.notifications.Notify(this, Notifications.Type.Error, "PrintOption, DoPrint(): " + ex.Message, ex);
				throw new IllException("Exception while printing images!");
			}
		}*/
		#endregion
	
		#region DoPrint()
		/*private void DoPrint(PrintBatch printBatch)
		{
			try
			{
				using (BscanILL.Export.ExportFiles exportFiles = GetExportFiles(printBatch))
				{
					PrintQueue printQueue = this.printerProfile.GetPrintQueue();
					PrintCapabilities capabilities = printQueue.GetPrintCapabilities();
					PageMediaSize pageMediaSize = printQueue.UserPrintTicket.PageMediaSize;

					this.lastBatch = printBatch;
					this.printsFinished = 0;

					PageImageableArea printable = capabilities.PageImageableArea;
					Thickness margin = new Thickness(printable.OriginWidth, printable.OriginHeight,
						capabilities.OrientedPageMediaWidth.Value - (printable.OriginWidth + printable.ExtentWidth) + 2, capabilities.OrientedPageMediaHeight.Value - (printable.OriginHeight + printable.ExtentHeight) + 2);

					List<System.IO.FileInfo> images = new List<FileInfo>();

					foreach (BscanILL.Export.ExportFile exportFile in exportFiles)
						images.Add(exportFile.File);

					if (printBatch.PrintingOptions.Copies > 1)
					{
						FileInfo[] temp = new FileInfo[images.Count];
						images.CopyTo(temp);

						for (int i = 1; i < printBatch.PrintingOptions.Copies; i++)
						{
							for (int j = 0; j < temp.Length; j++)
								images.Add(temp[j]);
						}
					}

					//printing
					Upload_ProgressChanged(0);

					ulong totalFilesSize;
					List<TimeSpan> timeSpans = GetTimeSpans(BscanILL.Scan.ExportMedium.Print, exportFiles, out totalFilesSize);
					Set_ProgressIntervals(ExportDlg.GetProgressName(BscanILL.Scan.ExportMedium.Print, BscanILL.Scan.FileFormat.Unknown), timeSpans);
					DateTime start = DateTime.Now;

					for (int index = 0; index < images.Count; index += 2)
					{
						List<FileInfo> localList = new List<FileInfo>() { images[index] };

						if (index + 1 < images.Count)
							localList.Add(images[index + 1]);

						using (PrintPaginator paginator = new PrintPaginator(localList, pageMediaSize, margin))
						{
							printQueue.UserPrintTicket.Duplexing = printBatch.PrintingOptions.DuplexPrinting ? Duplexing.TwoSidedLongEdge : Duplexing.OneSided;

							System.Windows.Xps.XpsDocumentWriter writer = System.Printing.PrintQueue.CreateXpsDocumentWriter(printQueue);

							writer.WritingProgressChanged += new System.Windows.Documents.Serialization.WritingProgressChangedEventHandler(Writer_WritingProgressChanged);
							writer.Write(paginator);
						}
					}

					Upload_ProgressChanged(1);
					BscanILL.Misc.TimeEstimates.Instance.SaveExportBytesPerSecond(BscanILL.Scan.ExportMedium.Print, totalFilesSize, DateTime.Now.Subtract(start).TotalSeconds);

					foreach (BscanILL.Export.ExportFile exportFile in exportFiles)
					{
						 Scanners.ColorMode colorMode = (printBatch.PrintingOptions.ColorMode != Scanners.ColorMode.Auto) ? printBatch.PrintingOptions.ColorMode : exportFile.IllImages[0].ColorMode;
						exportFile.IllImages[0].ImageExported(BscanILL.Scan.FileFormat.Auto, colorMode, 300, BscanILL.Hierarchy.User.UserAction.PrintLetter, exportFile.File.Length);
					}

					GC.Collect();
					this.lastBatch = null;
				}
			}
			catch (Exception ex)
			{
				this.notifications.Notify(this, Notifications.Type.Error, "PrintOption, DoPrint(): " + ex.Message, ex);
				throw new IllException("Exception while printing images!");
			}
		}*/
		#endregion

		#region PrinterStateError()
		private bool PrinterStateError(PrintQueueStatus queueStatus, ref String errrorMessage)
		{
			errrorMessage = null;

			if ((queueStatus & PrintQueueStatus.PaperProblem) == PrintQueueStatus.PaperProblem)
			{
				errrorMessage = errrorMessage + "Has a paper problem. ";
			}
			if ((queueStatus & PrintQueueStatus.NoToner) == PrintQueueStatus.NoToner)
			{
				errrorMessage = errrorMessage + "Is out of toner. ";
			}
			if ((queueStatus & PrintQueueStatus.DoorOpen) == PrintQueueStatus.DoorOpen)
			{
				errrorMessage = errrorMessage + "Has an open door. ";
			}
			if ((queueStatus & PrintQueueStatus.Error) == PrintQueueStatus.Error)
			{
				errrorMessage = errrorMessage + "Is in an error state. ";
			}
			if ((queueStatus & PrintQueueStatus.NotAvailable) == PrintQueueStatus.NotAvailable)
			{
				errrorMessage = errrorMessage + "Is not available. ";
			}
			if ((queueStatus & PrintQueueStatus.Offline) == PrintQueueStatus.Offline)
			{
				errrorMessage = errrorMessage + "Is off line. ";
			}
			if ((queueStatus & PrintQueueStatus.OutOfMemory) == PrintQueueStatus.OutOfMemory)
			{
				errrorMessage = errrorMessage + "Is out of memory. ";
			}
			if ((queueStatus & PrintQueueStatus.PaperOut) == PrintQueueStatus.PaperOut)
			{
				errrorMessage = errrorMessage + "Is out of paper. ";
			}
			if ((queueStatus & PrintQueueStatus.OutputBinFull) == PrintQueueStatus.OutputBinFull)
			{
				errrorMessage = errrorMessage + "Has a full output bin. ";
			}
			if ((queueStatus & PrintQueueStatus.PaperJam) == PrintQueueStatus.PaperJam)
			{
				errrorMessage = errrorMessage + "Has a paper jam. ";
			}
			if ((queueStatus & PrintQueueStatus.Paused) == PrintQueueStatus.Paused)
			{
				errrorMessage = errrorMessage + "Is paused. ";
			}
			if ((queueStatus & PrintQueueStatus.TonerLow) == PrintQueueStatus.TonerLow)
			{
				errrorMessage = errrorMessage + "Is low on toner. ";
			}
			if ((queueStatus & PrintQueueStatus.UserIntervention) == PrintQueueStatus.UserIntervention)
			{
				errrorMessage = errrorMessage + "Needs user intervention. ";
			}

			if (errrorMessage == null)
			{
				errrorMessage = "OK.";
				return false;
			}
			else
				return true;
		}
		#endregion

		#region Writer_WritingProgressChanged()
		/*void Writer_WritingProgressChanged(object sender, System.Windows.Documents.Serialization.WritingProgressChangedEventArgs e)
		{
			try
			{
				if (lastBatch != null && e.WritingLevel == System.Windows.Documents.Serialization.WritingProgressChangeLevel.FixedPageWritingProgress)
				{
					this.printsFinished ++;
					Upload_ProgressChanged(Math.Min(1.0F, (this.printsFinished / (float)(lastBatch.Count * lastBatch.PrintingOptions.Copies))));
				}
			}
			catch { }
		}*/
		#endregion

		#region GetExportFiles()
		/*private BscanILL.Export.ExportFiles GetExportFiles(PrintBatch printBatch)
		{
			try
			{
				BscanILL.Export.ExportFiles exportFiles = new BscanILL.Export.ExportFiles();

				PrintQueue			printQueue = this.printerProfile.GetPrintQueue();
				PageImageableArea	printable = printQueue.GetPrintCapabilities().PageImageableArea;
				System.Drawing.Size printableInPixels = new System.Drawing.Size(Convert.ToInt32(printable.ExtentWidth * 300.0 / 96.0), Convert.ToInt32(printable.ExtentHeight * 300.0 / 96.0));

				//file preparation
				ulong totalFilesSize;
				List<TimeSpan> timeSpans = GetFileFormatTimeSpans(printBatch, out totalFilesSize);
				Set_ProgressIntervals(BscanILL.Languages.BscanILLStrings.Export_CreatingSupportImages_STR, timeSpans);
				DateTime start = DateTime.Now;

				Upload_Description(BscanILL.Languages.BscanILLStrings.Export_CreatingSupportImages_STR);

				for (int i = 0; i < printBatch.Count; i++)
				{
					ProgressUnit_Started((uint)i);

					PrintedImage				printedImage = printBatch[i];
					System.Drawing.Bitmap		image = null;
					BscanILL.Hierarchy.DlsgImage		k = printedImage.IllImage;
					BscanILL.IP.PreviewCreator		creator = BscanILL.IP.PreviewCreator.Instance;

					ImageProcessing.ImageFile.ImageInfo imageInfo = printedImage.IllImage.FullImageInfo;
					 Scanners.ColorMode						colorMode = (printBatch.PrintingOptions.ColorMode != Scanners.ColorMode.Auto) ? printBatch.PrintingOptions.ColorMode : k.ColorMode;
					double								zoom;

					if ((imageInfo.Width > imageInfo.Height) ^ (printableInPixels.Width > printableInPixels.Height))
					{
						int imageWidth = (int)Math.Min(printableInPixels.Height, (printedImage.Clip == null) ? imageInfo.Width : imageInfo.Width * printedImage.Clip.Value.Width);
						int imageHeight = (int)Math.Min(printableInPixels.Width, (printedImage.Clip == null) ? imageInfo.Height : imageInfo.Height * printedImage.Clip.Value.Height);
						zoom = Math.Min(1, Math.Min(imageInfo.Width / imageWidth, imageInfo.Height / imageHeight));
					}
					else
					{
						int imageWidth = (int)Math.Min(printableInPixels.Width, (printedImage.Clip == null) ? imageInfo.Width : imageInfo.Width * printedImage.Clip.Value.Width);
						int imageHeight = (int)Math.Min(printableInPixels.Height, (printedImage.Clip == null) ? imageInfo.Height : imageInfo.Height * printedImage.Clip.Value.Height);
						zoom = Math.Min(1, Math.Min(imageInfo.Width / imageWidth, imageInfo.Height / imageHeight));
					}

					if (printedImage.Clip != null)
						image = creator.GetPreview(k.FilePath, printedImage.Clip.Value, colorMode, k.BrightnessDelta, k.ContrastDelta, k.Histogram, zoom, ImageProcessing.Resizing.ResizeMode.Quality);
					else
						image = creator.GetPreview(k.FilePath, colorMode, k.BrightnessDelta, k.ContrastDelta, k.Histogram, zoom, ImageProcessing.Resizing.ResizeMode.Quality);

					if (printedImage != null && image != null && printBatch.PrintingOptions.AutoLevels && image.PixelFormat != PixelFormat.Format1bppIndexed)
					{
						ImageProcessing.AutoLevels.Get(image, System.Drawing.Rectangle.Empty, 0.05, printBatch.PrintingOptions.AutoLevelsValue);
					}

					//rotate if necessary
					if ((image.Width > image.Height) ^ (printableInPixels.Width > printableInPixels.Height))
						image.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);

					FileInfo fileToPrint = new FileInfo(string.Format("{0}\\{1}.png", temporaryDir.FullName, DateTime.Now.ToString("HH-mm-ss-ff")));
					fileToPrint.Directory.Refresh();
					fileToPrint.Directory.Create();
					image.Save(fileToPrint.FullName, ImageFormat.Png);
					image.Dispose();
					exportFiles.Add(new BscanILL.Export.ExportFile(fileToPrint, BscanILL.Scan.FileFormat.Auto, k));

					//Upload_ProgressChanged((((i + 1.0F) / printBatch.Count) * 0.9F) / 2.0F);
					ProgressUnit_Finished((uint)i);
				}

				BscanILL.Misc.TimeEstimates.Instance.SaveFilePreparationSpeed(BscanILL.Misc.TimeEstimates.FilePreparationType.Print, (uint)totalFilesSize, DateTime.Now.Subtract(start).TotalSeconds);

				return exportFiles;
			}
			catch (Exception ex)
			{
				this.notifications.Notify(this, Notifications.Type.Error, "PrintOption, GetExportFiles(): " + ex.Message, ex);
				throw new IllException("Exception while printing images!");
			}
		}*/
		#endregion

		#endregion

	}
}

