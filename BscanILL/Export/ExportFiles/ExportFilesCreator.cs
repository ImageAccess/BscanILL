using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BscanILL.Scan;
using BscanILL.Misc;
using System.Windows;

namespace BscanILL.Export.ExportFiles
{
	class ExportFilesCreator : IDisposable
	{
		BscanILL.Export.ExportFiles.PdfsBuilder			pdfsBuilder = null;
		BscanILL.Export.ExportFiles.AudioCreator		audioBuilder = null;
		BscanILL.Export.ExportFiles.TextFileCreator		textFileCreator = null;
		BscanILL.Export.ExportFiles.MultiTiffCreator	multiTiffCreator = null;

		public event BscanILL.Export.ProgressChangedHnd		ProgressChanged;
		public event BscanILL.Export.ProgressDescriptionHnd DescriptionChanged;        

		#region constructor
		public ExportFilesCreator()
		{
		}
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		#region PdfsBuilder
		private BscanILL.Export.ExportFiles.PdfsBuilder PdfsBuilder
		{
			get
			{
				if (this.pdfsBuilder == null)
				{
					this.pdfsBuilder = BscanILL.Export.ExportFiles.PdfsBuilder.Instance;                    

					this.pdfsBuilder.ProgressChanged += delegate(double progress) { if (ProgressChanged != null) ProgressChanged(progress); };
					this.pdfsBuilder.DescriptionChanged += delegate(string description) { if (DescriptionChanged != null) DescriptionChanged(description); };
				}

				return this.pdfsBuilder;
			}
		}
		#endregion

		#region AudioCreator
		private BscanILL.Export.ExportFiles.AudioCreator AudioCreator
		{
			get
			{
				if (this.audioBuilder == null)
				{
                    this.audioBuilder = new BscanILL.Export.ExportFiles.AudioCreator();

					this.audioBuilder.ProgressChanged += delegate(double progress) { if (ProgressChanged != null) ProgressChanged(progress); };
					this.audioBuilder.DescriptionChanged += delegate(string description) { if (DescriptionChanged != null) DescriptionChanged(description); };
				}

				return this.audioBuilder;
			}
		}
		#endregion

		#region TextFileCreator
		private BscanILL.Export.ExportFiles.TextFileCreator TextFileCreator
		{
			get
			{
				if (this.textFileCreator == null)
				{
                    this.textFileCreator = new TextFileCreator();

					this.textFileCreator.ProgressChanged += delegate(double progress) { if (ProgressChanged != null) ProgressChanged(progress); };
					this.textFileCreator.DescriptionChanged += delegate(string description) { if (DescriptionChanged != null) DescriptionChanged(description); };
				}

				return this.textFileCreator;
			}
		}
		#endregion

		#region MultiTiffCreator
		private BscanILL.Export.ExportFiles.MultiTiffCreator MultiTiffCreator
		{
			get
			{
				if (this.multiTiffCreator == null)
				{
					this.multiTiffCreator = new MultiTiffCreator();

					this.multiTiffCreator.ProgressChanged += delegate(double progress) { if (ProgressChanged != null) ProgressChanged(progress); };
					this.multiTiffCreator.DescriptionChanged += delegate(string description) { if (DescriptionChanged != null) DescriptionChanged(description); };
				}

				return this.multiTiffCreator;
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Create()
		public void Create(System.Windows.Window parentForm, BscanILL.Export.ExportUnit exportUnit)
		{
			lock (this)
			{
				ulong maxFileSize = GetMaxFileSize(exportUnit);
				
				if (exportUnit.IllPages.Count > 0)
				{
					if ((exportUnit.FileFormat == BscanILL.Scan.FileFormat.Pdf || exportUnit.FileFormat == BscanILL.Scan.FileFormat.SPdf) && exportUnit.MultiImage)
						CreateMultiPdf(exportUnit, maxFileSize);
					else if (exportUnit.FileFormat == BscanILL.Scan.FileFormat.Audio && exportUnit.MultiImage)
						CreateMultiAudio(parentForm, exportUnit, maxFileSize);
					else if (exportUnit.FileFormat == BscanILL.Scan.FileFormat.Tiff && exportUnit.MultiImage)
						CreateMultiTiff(exportUnit, maxFileSize);
					else if (exportUnit.FileFormat == BscanILL.Scan.FileFormat.Text && exportUnit.MultiImage)
						CreateMultiText(exportUnit, maxFileSize);
					else if (exportUnit.FileFormat == BscanILL.Scan.FileFormat.Audio)
						CreateSingleAudio(parentForm, exportUnit, maxFileSize);
					else
						CreateSingleFiles(exportUnit, maxFileSize);
				}

				//foreach (ExportFile exportFile in this)
				//	exportFile.File.Refresh();

				Progress_Changed(1);
			}
		}
        
		#endregion

		#region GetFile()
		public static FileInfo GetFile(BscanILL.Hierarchy.IllPage illPage, BscanILL.Export.ExportUnit exportUnit, List<string> warnings, ulong byteLimit)
		{
			return GetExportFile(illPage, exportUnit, warnings, byteLimit, true);
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region CreateMultiPdf()
		private void CreateMultiPdf(BscanILL.Export.ExportUnit exportUnit, ulong maxFileSize)
		{
			try
			{	
				ProgressDescription_Changed("Creating multi-image PDF file...");

				if (maxFileSize == 0)
				{
					FileInfo localFile = BscanILL.Export.Misc.GetUniqueExportFilePath(exportUnit);

					this.PdfsBuilder.GetMultiImagePdf(exportUnit, exportUnit.IllPages, localFile, exportUnit.FileFormat == BscanILL.Scan.FileFormat.SPdf, exportUnit.Warnings, exportUnit.PdfA, true, exportUnit.AdditionalInfo.FileColor, exportUnit.AdditionalInfo.FileQuality);
					
					exportUnit.AddExportFile(Scanners.ColorMode.Unknown, 0, FileFormat.SPdf, localFile, (short)exportUnit.IllPages.Count);
				}
				else
				{
					List<PdfsBuilder.MultiPdf> multiPdfList = this.PdfsBuilder.GetMultiImagePdfWithSizeLimit(exportUnit, exportUnit.IllPages, exportUnit.FileFormat == BscanILL.Scan.FileFormat.SPdf, maxFileSize, exportUnit.Warnings, exportUnit.PdfA, false, exportUnit.AdditionalInfo.FileColor, exportUnit.AdditionalInfo.FileQuality);

					foreach (BscanILL.Export.ExportFiles.PdfsBuilder.MultiPdf multiPdf in multiPdfList)
						exportUnit.AddExportFile(Scanners.ColorMode.Unknown, 0, FileFormat.SPdf, multiPdf.File, (short)multiPdf.ExportedImages.Count);
				}
			}
			catch (BscanILL.Misc.IllException ex)
			{
				if (ex.ErrorCode == BscanILL.Misc.ErrorCode.FileOverSizeLimit)
				{
					throw ex;
				}
				else
				{
					throw new Exception("Can't create multi-image PDF! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}
			}
			catch (Exception ex)
			{
				Notify("CreateMultiPdf(): " + ex.Message, ex);
				throw new IllException("Can't create multi-image PDF! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion

		#region CreateMultiAudio()
		private void CreateMultiAudio(System.Windows.Window parentForm, BscanILL.Export.ExportUnit exportUnit, ulong maxFileSize)
		{
			List<BscanILL.Export.AudioZoning.ZonesImage> zoneImages = null;
			
			try
			{
				ProgressDescription_Changed("Creating multi-image audio file...");
				BscanILL.Export.ExportFiles.AudioCreator.AudioType audioType = new BscanILL.Export.ExportFiles.AudioCreator.AudioType(BscanILL.SETTINGS.Settings.Instance.Export.Audio.DefaultVoice, 0, 100, 16, true);

				zoneImages = this.AudioCreator.GetAudioZones(parentForm, exportUnit.IllPages);

				FileInfo localFile = BscanILL.Export.Misc.GetUniqueExportFilePath(exportUnit);

				if (zoneImages != null)
					this.AudioCreator.CreateAudio(zoneImages, localFile, audioType, true);
				else
					this.AudioCreator.CreateAudio(exportUnit.IllPages, localFile, audioType, true);

				exportUnit.AddExportFile(Scanners.ColorMode.Unknown, 0, FileFormat.Audio, localFile, (short)exportUnit.IllPages.Count);
			}
			catch (Exception ex)
			{
				if (ex is BscanILL.Misc.IllException)
					throw ex;
				else
				{
					Notify("CreateMultiAudio(): " + ex.Message, ex);
					throw new IllException("Can't create multi-page audio file! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}
			}
		}
		#endregion

		#region CreateSingleAudio()
		private void CreateSingleAudio(System.Windows.Window parentForm, BscanILL.Export.ExportUnit exportUnit, ulong maxFileSize)
		{
			try
			{
				ProgressDescription_Changed("Creating Audio Files...");

				List<BscanILL.Export.AudioZoning.ZonesImage> zoneImages = this.AudioCreator.GetAudioZones(parentForm, exportUnit.IllPages);

				CreateSingleAudioNoUi(exportUnit, maxFileSize, zoneImages);
			}
			catch (BscanILL.Misc.IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify("CreateSingleAudio(): " + ex.Message, ex);
				throw new BscanILL.Misc.IllException("Can't create audio files! " + ex.Message);
			}
		}
		#endregion

		#region CreateSingleAudioNoUi()
		private void CreateSingleAudioNoUi(BscanILL.Export.ExportUnit exportUnit, ulong maxFileSize, List<BscanILL.Export.AudioZoning.ZonesImage> zoneImages)
		{
			try
			{
				ProgressDescription_Changed("Creating Audio Files...");
				AudioCreator.AudioType audioType = new AudioCreator.AudioType(BscanILL.SETTINGS.Settings.Instance.Export.Audio.DefaultVoice, 0, 100, 16, true);
				
				if (zoneImages != null && zoneImages.Count > 0)
				{					
					for (int i = 0; i < zoneImages.Count; i++)
					{
						try
						{
							FileInfo localFile = BscanILL.Export.Misc.GetUniqueExportFilePath(exportUnit);

							this.AudioCreator.CreateAudio(zoneImages[i], localFile, audioType, false);
							exportUnit.AddExportFile(Scanners.ColorMode.Unknown, 0, FileFormat.Audio, localFile, (short)exportUnit.IllPages.Count);
						}
						catch (BscanILL.Misc.IllException ex)
						{
							exportUnit.Warnings.Add(ex.Message);
						}
						catch (Exception ex)
						{
							Notify("CreateSingleAudioNoUi() 1: " + ex.Message, ex);
							exportUnit.Warnings.Add(string.Format("Can't create audio file from image {0}!", i + 1));
						}

						Progress_Changed((i + 1.0F) / zoneImages.Count);
					}
				}
				else
				{					
					for (int i = 0; i < exportUnit.IllPages.Count; i++)
					{						
						try
						{
							BscanILL.Hierarchy.IllPage illPage = exportUnit.IllPages[i];
							FileInfo localFile = BscanILL.Export.Misc.GetUniqueExportFilePath(exportUnit);

							this.AudioCreator.CreateAudio(exportUnit.IllPages[i], localFile, audioType, false);
							exportUnit.AddExportFile(Scanners.ColorMode.Unknown, 0, FileFormat.Audio, localFile, (short)exportUnit.IllPages.Count);
						}
						catch (BscanILL.Misc.IllException ex)
						{
							exportUnit.Warnings.Add(ex.Message);
						}
						catch (Exception ex)
						{
							Notify("CreateSingleAudioNoUi() 2: " + ex.Message, ex);
							exportUnit.Warnings.Add(string.Format("Can't create audio file from image {0}!", i + 1));
						}

						Progress_Changed((i + 1.0) / exportUnit.IllPages.Count);
					}
				}
			}
			catch (BscanILL.Misc.IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify("CreateSingleAudioNoUi() 3: " + ex.Message, ex);
				throw new BscanILL.Misc.IllException("Can't create audio files!");
			}
		}
		#endregion

		#region CreateMultiText()
		private void CreateMultiText(BscanILL.Export.ExportUnit exportUnit, ulong maxFileSize)
		{
			try
			{
				ProgressDescription_Changed("Creating Rich Text Format Files...");

				FileInfo localFile = BscanILL.Export.Misc.GetUniqueExportFilePath(exportUnit);
				this.TextFileCreator.CreateTextFile(exportUnit.IllPages, localFile, true);
				exportUnit.AddExportFile(Scanners.ColorMode.Unknown, 0, FileFormat.Text, localFile, (short)exportUnit.IllPages.Count);
			}
			catch (Exception ex)
			{
				if (ex is BscanILL.Misc.IllException)
					throw ex;
				else
				{
					Notify("CreateMultiText(): " + ex.Message, ex);
					throw new IllException("Can't create multi-image text file! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}
			}
		}
		#endregion

		#region CreateMultiTiff()
		private void CreateMultiTiff(BscanILL.Export.ExportUnit exportUnit, ulong maxFileSize)
		{
			try
			{
				ProgressDescription_Changed("Creating multi-image TIFF file...");

				FileInfo localFile = BscanILL.Export.Misc.GetUniqueExportFilePath(exportUnit);
				this.MultiTiffCreator.Export(exportUnit.IllPages, localFile);
				exportUnit.AddExportFile(Scanners.ColorMode.Unknown, 0, FileFormat.Tiff, localFile, (short)exportUnit.IllPages.Count);
			}
			catch (Exception ex)
			{
				if (ex is BscanILL.Misc.IllException)
					throw ex;
				else
				{
					Notify("CreateMultiTiff(): " + ex.Message, ex);
					throw new IllException("Can't create multi-image TIFF file! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}
			}
		}
		#endregion

		#region CreateSingleFiles()
		private void CreateSingleFiles(BscanILL.Export.ExportUnit exportUnit, ulong maxFileSize)
		{
			try
			{
				string description = "Creating single export files...";
				
				switch (exportUnit.FileFormat)
				{
					case BscanILL.Scan.FileFormat.Audio: 
						Notify("CreateSingleFiles(): unexpected audio format!", null);
						throw new Exception("Unexpected error!");
					case BscanILL.Scan.FileFormat.Jpeg: description = "Creating JPEG Files..."; break;
					case BscanILL.Scan.FileFormat.Pdf:
					case BscanILL.Scan.FileFormat.SPdf:
						description = "Creating PDF file..."; break;
					case BscanILL.Scan.FileFormat.Png: description = "Creating PNG files..."; break;
					case BscanILL.Scan.FileFormat.Text: description = "Creating Rich Text Format Files..."; break;
					case BscanILL.Scan.FileFormat.Tiff: description = "Creating TIFF Files..."; break;
				}
				
				Progress_Changed(0);
				ProgressDescription_Changed(description);

				for (int i = 0; i < exportUnit.IllPages.Count; i++)
				{
					BscanILL.Hierarchy.IllPage illPage = exportUnit.IllPages[i];				
					
					try
					{
						FileInfo file = GetExportFile(illPage, exportUnit, exportUnit.Warnings, maxFileSize, false);
						exportUnit.AddExportFile(Scanners.ColorMode.Unknown, 0, exportUnit.FileFormat, file, 1);
						//Progress_Changed((i + 1.0F) / exportUnit.IllPages.Count);
					}
					catch (Exception ex)
					{
						if (exportUnit.FileFormat != BscanILL.Scan.FileFormat.Jpeg)
						{
							exportUnit.FileFormat = FileFormat.Jpeg;
							FileInfo localFile = GetExportFile(illPage, exportUnit, exportUnit.Warnings, maxFileSize, false);
							exportUnit.AddExportFile(Scanners.ColorMode.Unknown, 0, exportUnit.FileFormat, localFile, 1);
						}
						else
						{
							Notify("CreateSingleFiles(): can't create JPEG file: " + ex.Message, ex);
							exportUnit.Warnings.Add("Can't create export file!");
						}
					}

					Progress_Changed((i + 1.0) / exportUnit.IllPages.Count);
				}
			}
			catch (BscanILL.Misc.IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notify("CreateSingleFiles(): " + ex.Message, ex);
				throw new BscanILL.Misc.IllException(ex.Message);
			}
		}
		#endregion

		#region GetExportFile()
		private static FileInfo GetExportFile(BscanILL.Hierarchy.IllPage illPage, BscanILL.Export.ExportUnit exportUnit, List<string> warnings, ulong byteLimit, bool updateProgressBar)
		{
			FileInfo exportFile = GetExportFilePath(exportUnit);
			ushort dpi = (ushort)illPage.Dpi;

			switch (exportUnit.FileFormat)
			{
				case BscanILL.Scan.FileFormat.Tiff: exportFile = CreateExportFileTiff(illPage, exportUnit, byteLimit); break;
				case BscanILL.Scan.FileFormat.Png: exportFile = CreateExportFilePng(illPage, exportUnit, byteLimit); break;
				case BscanILL.Scan.FileFormat.Pdf: exportFile = CreateExportFilePdf(illPage, false, exportUnit, warnings, exportUnit.PdfA, byteLimit); break;
				case BscanILL.Scan.FileFormat.SPdf: exportFile = CreateExportFilePdf(illPage, true, exportUnit, warnings, exportUnit.PdfA, byteLimit); break;
				case BscanILL.Scan.FileFormat.Audio: exportFile = CreateExportFileAudio(illPage, exportUnit, warnings, byteLimit); break;
                case BscanILL.Scan.FileFormat.Text: exportFile = CreateExportFileText(illPage, exportUnit, warnings, byteLimit, updateProgressBar); break;
				case BscanILL.Scan.FileFormat.Jpeg: exportFile = CreateExportFileJpeg(illPage, exportUnit, byteLimit); break;
				default:
					{
						Notifications.Instance.Notify(null, Notifications.Type.Error, "ExportFiles, GetExportFile(): Unexpected file format!", null);
						throw new BscanILL.Misc.IllException("Unexpected Bscan ILL Exception! Please contact system administrator.");
					}
			}

			exportFile.Refresh();
			return exportFile;
		}
		#endregion

		#region Notify()
		void Notify(string message, Exception ex)
		{
			Notifications.Instance.Notify(this, Notifications.Type.Error, "ExportFiles, " + message, ex);
		}
		#endregion

		#region Progress_Changed()
		void Progress_Changed(double progress)
		{
			if (ProgressChanged != null)
				ProgressChanged(progress);
		}
		#endregion

		#region ProgressDescription_Changed()
		void ProgressDescription_Changed(string description)
		{
			if (DescriptionChanged != null)
				DescriptionChanged(description);
		}
		#endregion

		#region GetExportFilePath
		private static FileInfo GetExportFilePath(BscanILL.Export.ExportUnit exportUnit)
		{
			FileInfo file = BscanILL.Export.Misc.GetUniqueExportFilePath(exportUnit);

			file.Directory.Create();
			return file;
		}
		#endregion

		#region CreateExportFileTiff()
		private static FileInfo CreateExportFileTiff(BscanILL.Hierarchy.IllPage illPage, BscanILL.Export.ExportUnit exportUnit, ulong byteLimit)
		{
			FileInfo exportFile = GetExportFilePath(exportUnit);
			BscanILL.Export.IP.ExportFileCreator creator = new BscanILL.Export.IP.ExportFileCreator();

			if (byteLimit == 0)
			{
				if (illPage.ColorMode == Scanners.ColorMode.Bitonal)
					creator.CreateExportFile(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), illPage.ColorMode,
						illPage.Dpi / (double)illPage.ScanDpi);
				else
					creator.CreateExportFile(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.LZW), illPage.ColorMode,
						illPage.Dpi / (double)illPage.ScanDpi);
			}
			else
			{
				if (illPage.ColorMode == Scanners.ColorMode.Bitonal)
					creator.CreateExportFileWithLimitedSize(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), illPage.ColorMode,
						illPage.Dpi / (double)illPage.ScanDpi, byteLimit);
				else
					creator.CreateExportFileWithLimitedSize(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.LZW), illPage.ColorMode,
						illPage.Dpi / (double)illPage.ScanDpi, byteLimit);
			}

			return exportFile;
		}
		#endregion

		#region CreateExportFilePng()
		private static FileInfo CreateExportFilePng(BscanILL.Hierarchy.IllPage illPage, BscanILL.Export.ExportUnit exportUnit, ulong byteLimit)
		{
			FileInfo exportFile = GetExportFilePath(exportUnit);

			if (byteLimit == 0)
			{
				BscanILL.Export.IP.ExportFileCreator creator = new BscanILL.Export.IP.ExportFileCreator();
				creator.CreateExportFile(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Png(), illPage.ColorMode,
					illPage.Dpi / (double)illPage.ScanDpi);
			}
			else
			{
				BscanILL.Export.IP.ExportFileCreator creator = new BscanILL.Export.IP.ExportFileCreator();
				creator.CreateExportFileWithLimitedSize(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Png(), illPage.ColorMode,
					illPage.Dpi / (double)illPage.ScanDpi, byteLimit);
			}

			return exportFile;
		}
		#endregion

		#region CreateExportFilePdf()
		private static FileInfo CreateExportFilePdf(BscanILL.Hierarchy.IllPage illPage, bool searchable, BscanILL.Export.ExportUnit exportUnit, List<string> warnings, bool pdfA1B, ulong byteLimit)
		{
			FileInfo exportFile = GetExportFilePath(exportUnit);

			try
			{
				if (exportFile.Exists)
					exportFile.Delete();

				BscanILL.Export.ExportFiles.PdfsBuilder pdfsBuilder = BscanILL.Export.ExportFiles.PdfsBuilder.Instance;

				pdfsBuilder.GetSingleFile(exportUnit, illPage, exportFile, searchable, byteLimit, pdfA1B);
			}
			catch (Exception ex)
			{
				if ((ex is BscanILL.Misc.IllException) == false)
					Notifications.Instance.Notify(null, Notifications.Type.Error, "ExportFiles, CreateExportFilePdf(): " + ex.Message, ex);

				if (searchable)
				{
					exportFile.Refresh();
					if (exportFile.Exists)
						exportFile.Delete();
				}

				exportFile = CreateExportFileJpeg(illPage, exportUnit, byteLimit);
				warnings.Add(string.Format("Can't create PDF file output! Image '{0}'  is being saved as JPEG.", exportFile.Name));
			}

			return exportFile;
		}
		#endregion

		#region CreateExportFileAudio()
		private static FileInfo CreateExportFileAudio(BscanILL.Hierarchy.IllPage illPage, BscanILL.Export.ExportUnit exportUnit, List<string> warnings, ulong byteLimit)
		{
			FileInfo exportFile = GetExportFilePath(exportUnit);

			try
			{
				if (exportFile.Exists)
					exportFile.Delete();

                BscanILL.Export.ExportFiles.AudioCreator audioCreator = new BscanILL.Export.ExportFiles.AudioCreator();
				BscanILL.Export.ExportFiles.AudioCreator.AudioType audioType = new BscanILL.Export.ExportFiles.AudioCreator.AudioType(BscanILL.SETTINGS.Settings.Instance.Export.Audio.DefaultVoice, 0, 100, 16, true);
				audioCreator.CreateAudio(illPage, exportFile, audioType, true);
				audioCreator.Dispose();
			}
			catch (Exception ex)
			{
				if ((ex is BscanILL.Misc.IllException) == false)
					Notifications.Instance.Notify(null, Notifications.Type.Error, "ExportFiles, CreateExportFileAudio(): " + ex.Message, ex);

				exportFile = CreateExportFileJpeg(illPage, exportUnit, byteLimit);
				warnings.Add(string.Format("Can't create Audio file output! Image '{0}' is being saved as JPEG.", exportFile.Name));
			}

			return exportFile;
		}
		#endregion

		#region CreateExportFileText()
        private static FileInfo CreateExportFileText(BscanILL.Hierarchy.IllPage illPage, BscanILL.Export.ExportUnit exportUnit, List<string> warnings, ulong byteLimit, bool updateProgressBar)
		{
			FileInfo exportFile = GetExportFilePath(exportUnit);

			try
			{
				if (exportFile.Exists)
					exportFile.Delete();

                BscanILL.Export.ExportFiles.TextFileCreator textCreator = new BscanILL.Export.ExportFiles.TextFileCreator();
                textCreator.CreateTextFile(illPage, exportFile, updateProgressBar);
			}
			catch (Exception ex)
			{
				if ((ex is BscanILL.Misc.IllException) == false)
					Notifications.Instance.Notify(null, Notifications.Type.Error, "ExportFiles, CreateExportFileText(): " + ex.Message, ex);

				exportFile = CreateExportFileJpeg(illPage, exportUnit, byteLimit);
				warnings.Add(string.Format("Can't create Text file output! Image '{0}' is being saved as JPEG.", exportFile.Name));
			}

			return exportFile;
		}
		#endregion

		#region CreateExportFileJpeg()
		private static FileInfo CreateExportFileJpeg(BscanILL.Hierarchy.IllPage illPage, BscanILL.Export.ExportUnit exportUnit, ulong byteLimit)
		{
			FileInfo exportFile = GetExportFilePath(exportUnit);
			BscanILL.Export.IP.ExportFileCreator creator = new BscanILL.Export.IP.ExportFileCreator();

			if (byteLimit == 0)
			{
				if (illPage.ColorMode == Scanners.ColorMode.Color)
					creator.CreateExportFile(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Jpeg(80),
						illPage.ColorMode, illPage.Dpi / (double)illPage.ScanDpi);
				else
					creator.CreateExportFile(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Jpeg(80),
						 Scanners.ColorMode.Grayscale, illPage.Dpi / (double)illPage.ScanDpi);
			}
			else
			{
				if (illPage.ColorMode == Scanners.ColorMode.Color)
					creator.CreateExportFileWithLimitedSize(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Jpeg(80),
						illPage.ColorMode, illPage.Dpi / (double)illPage.ScanDpi, byteLimit);
				else
					creator.CreateExportFileWithLimitedSize(illPage.FilePath, exportFile, new ImageProcessing.FileFormat.Jpeg(80),
						 Scanners.ColorMode.Grayscale, illPage.Dpi / (double)illPage.ScanDpi, byteLimit);
			}

			return exportFile;
		}
		#endregion

		#region GetMaxFileSize()
		/// <summary>
		/// Returns max sile size supported by export medium, in bytes. Returns 0 if no limit.
		/// </summary>
		/// <param name="exportUnit"></param>
		/// <returns>Returns max sile size supported by export medium, in bytes. Returns 0 if no limit.</returns>
		private ulong GetMaxFileSize(BscanILL.Export.ExportUnit exportUnit)
		{
			switch (exportUnit.ExportType)
			{
				case ExportType.Email: return (ulong)(BscanILL.SETTINGS.Settings.Instance.Export.Email.SizeLimitInMB * 1024 * 1024);
				default: return 0;
			}
		}
		#endregion

		#endregion
	
	}
}
