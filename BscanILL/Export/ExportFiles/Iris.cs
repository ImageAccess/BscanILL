using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BscanILL.Misc;
using System.Reflection;
using BscanILL.Scan;

#if IRIS_ENGINE

namespace BscanILL.Export.ExportFiles
{
	public class Iris : ExportFilesBasics
	{
		static Iris		instance = null;
		ITextSharp		iTextSharp = new ITextSharp();

		bool			disposed = false;


		#region constructor
		private Iris()
		{
			this.iTextSharp.ProgressChanged += delegate(double progress) { Progress_Changed(progress); };
			this.iTextSharp.DescriptionChanged += delegate(string description) { Description_Changed(description); };
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			this.disposed = true;
			instance = null;
		}
		#endregion


		#region class SpdfStruct
		public class SpdfStruct
		{
			public readonly BscanILL.Hierarchy.IllPage IllPage = null;
			public readonly FileInfo Xml = null;
			public readonly FileInfo ImageToInsert = null;

			public SpdfStruct(BscanILL.Hierarchy.IllPage illPage, FileInfo xml, FileInfo imageToInsert)
			{
				this.IllPage = illPage;
				this.Xml = xml;
				this.ImageToInsert = imageToInsert;
			}
		}
		#endregion

		#region class TextStruct
		public class TextStruct
		{
			public readonly BscanILL.Hierarchy.IllPage IllPage = null;
			public readonly FileInfo XmlFile = null;

			public TextStruct(BscanILL.Hierarchy.IllPage illPage, FileInfo xmlFile)
			{
				this.IllPage = illPage;
				this.XmlFile = xmlFile;
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public static Iris Instance 
		{ 
			get 
			{
				if (instance == null)
					instance = new Iris();

				return instance;
			}
		}

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region CreatePdf()
		public void CreatePdf(FileInfo destFile, List<SpdfStruct> images, ulong maxFileSize, List<string> warnings)
		{
			string currentDirectory = Environment.CurrentDirectory;

			try
			{
				Environment.CurrentDirectory = _settings.General.IrisBinDir;
				CreatePdfInternal(destFile, images, maxFileSize, warnings);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}

		public void CreatePdf(FileInfo destFile, SpdfStruct pdfStruct)
		{
			string currentDirectory = Environment.CurrentDirectory;

			try
			{
				Environment.CurrentDirectory = _settings.General.IrisBinDir;

				CreatePdfInternal(destFile, pdfStruct);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}

		/*public void CreatePdf(FileInfo destFile, FileInfo xmlFile, FileInfo imageFile)
		{
			string currentDirectory = Environment.CurrentDirectory;

			try
			{
				Environment.CurrentDirectory = _settings.General.IrisBinDir;

				CreatePdfXmlInternal(destFile, xmlFile, imageFile);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}*/
		#endregion

		#region CreateText()
		public void CreateText(FileInfo destFile, List<TextStruct> textStructList)
		{
			string currentDirectory = Environment.CurrentDirectory;

			try
			{
				Environment.CurrentDirectory = _settings.General.IrisBinDir;

				CreateRtfInternal(destFile, textStructList);

				Environment.CurrentDirectory = currentDirectory;
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}
		#endregion

		#region Ocr()
		public string Ocr(FileInfo file)
		{
			string currentDirectory = Environment.CurrentDirectory;

			try
			{
				Environment.CurrentDirectory = _settings.General.IrisBinDir;

				return RunIdrsOcrInternal(file);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}
		#endregion

		#region LoadIris()
		public static void LoadIris()
		{
//			string currentDirectory = Environment.CurrentDirectory;

			try
			{
				//string assemblyFileName = "IdrsWrapper3.dll";
				string assemblyName = "IdrsWrapper3";
				Directory.CreateDirectory(BscanILL.SETTINGS.Settings.Instance.General.IrisBinDir);
				
				/*try
				{*/
//					Environment.CurrentDirectory = BscanILL.SETTINGS.Settings.Instance.General.IrisBinDir;

					//System.Windows.MessageBox.Show("IDRS: '" + assemblyName + "', CurrentDirectory: '" + Environment.CurrentDirectory + "'");
					Assembly.Load(assemblyName);
				/*}
				catch
				{
					FileInfo fileBin = new FileInfo(BscanILL.SETTINGS.Settings.Instance.General.IrisBinDir + @"\" + assemblyFileName);
					FileInfo fileExe = new FileInfo(BscanILL.Misc.Misc.StartupPath + @"\" + assemblyName);

					if (fileBin.Exists == false || fileBin.CreationTimeUtc < fileExe.CreationTimeUtc)
						fileExe.CopyTo(fileBin.FullName);

					//System.Windows.MessageBox.Show("IDRS: '" + assemblyFile + "', CurrentDirectory: '" + Environment.CurrentDirectory + "'");
					Assembly.LoadFile(fileBin.FullName);
				}*/
			}
			catch (Exception ex)
			{
				throw new Exception("Can't load IDRS assembly! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
			finally
			{
//				Environment.CurrentDirectory = currentDirectory;
			}
		}
		#endregion

		#region FormatImage()
		/// <summary>
		/// 
		/// </summary>
		/// <param name="reducedImage">200dpi grayscale image</param>
		/// <param name="formatFile">out - file will be saved onto this file</param>
		/// <returns></returns>
		public void FormatImage(FileInfo reducedImage, FileInfo destXml)
		{
			lock (this)
			{
				if (this.disposed == false)
				{
					string currentDirectory = Environment.CurrentDirectory;

					try
					{
						Environment.CurrentDirectory = _settings.General.IrisBinDir;

						FormatImageInternal(reducedImage, destXml);
					}
					finally
					{
						Environment.CurrentDirectory = currentDirectory;
					}
				}
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region CreatePdfInternal()
		private void CreatePdfInternal(FileInfo destFile, List<SpdfStruct> images, ulong maxFileSize, List<string> warnings)
		{
			lock (this)
			{
				List<ITextSharp.InsertImageStruct> filesToInsert = new List<ITextSharp.InsertImageStruct>();

				// create pdf files
				try
				{
					try
					{
						using (Idrs.IdrsWrapper idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir))
						{							
							for (int i = 0; i < images.Count; i++)
							{
								SpdfStruct pdfStruct = images[i];
								FileInfo f = new FileInfo(destFile.Directory.FullName + @"\" + Path.GetFileNameWithoutExtension(destFile.Name) + "_" + i.ToString() + ".pdf");

								try
								{

									if (pdfStruct.Xml != null)
									{
										idrsWrapper.OpenPdf(f);
										idrsWrapper.AddPdfImage(pdfStruct.Xml, pdfStruct.ImageToInsert);
										idrsWrapper.Close();

										filesToInsert.Add(new ITextSharp.InsertImageStruct(ITextSharp.FileType.Pdf, f));
									}
									else
									{
										filesToInsert.Add(new ITextSharp.InsertImageStruct(ITextSharp.FileType.Image, pdfStruct.ImageToInsert));
									}
								}
								catch (Exception ex)
								{
									DeleteFileNoException(f);

									if ((ex is IllException) == false)
										filesToInsert.Add(new ITextSharp.InsertImageStruct(ITextSharp.FileType.Image, pdfStruct.ImageToInsert));
									else
										throw ex;
								}

								if ((maxFileSize > 0) && ((ulong)GetSize(filesToInsert) > maxFileSize))
									throw new IllException(BscanILL.Misc.ErrorCode.FileOverSizeLimit, "PDF file size is over size limit.");

								Progress_Changed((float)((i + 1.0) / images.Count));
							}
						}
					}
					catch (Idrs.IdrsWrapperException ex)
					{
						if (ex.File != null && File.Exists(ex.File.FullName))
							Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreatePdfInternal() multi-image: " + ex.Message, ex, ex.File);
						else
							Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreatePdfInternal() multi-image: " + ex.Message, ex);

						throw new IllException("Can't create PDF file!");
					}
					catch (IllException ex)
					{
						throw ex;
					}
					catch (Exception ex)
					{
						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreatePdfInternal() OCR: " + ex.Message, ex);
						throw new IllException("Can't create PDF file!");
					}

					// merge single files
					try
					{
						iTextSharp.MergeFiles(destFile, filesToInsert, false);

						destFile.Refresh();
						if (maxFileSize > 0 && (ulong)destFile.Length > maxFileSize)
						{
							throw new IllException(BscanILL.Misc.ErrorCode.FileOverSizeLimit, "PDF file size is over size limit.");
						}
						else
						{
							Progress_Changed(1.0F);
						}
					}
					catch (IllException ex)
					{
						DeleteFileNoException(destFile);
						throw ex;
					}
					catch (Exception ex)
					{
						DeleteFileNoException(destFile);

						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreatePdfInternal() merging: " + ex.Message, ex);
						throw new IllException("Can't create PDF file!");
					}
				}
				finally
				{
					DeleteFiles(filesToInsert);
				}
			}
		}
		#endregion

		#region CreatePdfInternal()
		private void CreatePdfInternal(FileInfo destFile, SpdfStruct pdfStruct)
		{
			lock (this)
			{
				try
				{
					Progress_Changed(0);
					using (Idrs.IdrsWrapper idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir))
					{
						idrsWrapper.OpenPdf(destFile);
						
						if (pdfStruct.Xml != null)
							idrsWrapper.AddPdfImage( pdfStruct.Xml, pdfStruct.ImageToInsert);
						else
							idrsWrapper.AddPdfImage(pdfStruct.ImageToInsert, false);
					
						idrsWrapper.Close();
					}
					Progress_Changed(1);
				}
				catch (Idrs.IdrsWrapperException ex)
				{
					if (ex.File != null && File.Exists(ex.File.FullName))
						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreatePdfInternal(): " + ex.Message, ex, ex.File);
					else
						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreatePdfInternal(): " + ex.Message, ex);

					throw new IllException("Can't create PDF file!");
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreatePdfInternal(): " + ex.Message, ex);
					throw new IllException("Can't create PDF file!");
				}
			}
		}
		#endregion

		#region FormatImageInternal()
		private void FormatImageInternal(FileInfo reducedImage, FileInfo destXml)
		{
			lock (this)
			{
				try
				{
					using (Idrs.IdrsWrapper idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir))
					{
						reducedImage.Refresh();
						destXml.Refresh();
						idrsWrapper.GetFormatFile(reducedImage, destXml);
					}
				}
				catch (Idrs.IdrsWrapperException ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, FormatImageInternal(): " + ex.Message, ex);
					throw new IllException(ex);
				}
			}
		}
		#endregion

		#region CreateRtfInternal()
		/*private void CreateRtfInternal(FileInfo destFile, List<TextStruct> images)
		{
			lock (this)
			{
				try
				{
					using (Idrs.IdrsWrapper idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir))
					{
						//idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir);
						idrsWrapper.PdfImageAdded += new Idrs.ImageEventHndl(Iris_ImageAdded);
						idrsWrapper.RtfOcrDone += new Idrs.ImageEventHndl(Iris_RtfOcrDone);
						idrsWrapper.UniOcrDone += new Idrs.ImageEventHndl(Iris_UniOcrDone);
						idrsWrapper.TextOcrDone += new Idrs.ImageEventHndl(Iris_TextOcrDone);

						idrsWrapper.OpenRtf(destFile);

						ulong totalPixels;
						List<TimeSpan> timeSpans = GetTimeSpans(BscanILL.Scan.FileFormat.Text, images, out totalPixels);
						Set_ProgressIntervals(BscanILL.Languages.BscanILLStrings.Export_CreatingMultiImageTextFile_STR, timeSpans);
						DateTime start = DateTime.Now;

						for (int i = 0; i < images.Count; i++)
						{
							ProgressUnit_Started((uint)i);
							idrsWrapper.AddRtfImage(images[i].File);

							if ((i % 10) == 9)
							{
								idrsWrapper.Flush();
								destFile.Refresh();
							}

							//Iris_ImageAdded(images[i].FullName, (float)((i + 1.0) / images.Count) / 1.2F);
							ProgressUnit_Finished((uint)i);
						}

						idrsWrapper.Close();
						BscanILL.Misc.TimeEstimates.Instance.SaveFileFormatSpeed(BscanILL.Scan.FileFormat.Text, (uint)totalPixels, DateTime.Now.Subtract(start).TotalSeconds);
					}
				}
				catch (Idrs.IdrsWrapperException ex)
				{
					string exceptionType = ex.ExceptionType.ToString();

					if (ex.File != null && File.Exists(ex.File.FullName))
						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreateRtfInternal() multi-image X1: " + ex.Message + " Exception type: " + exceptionType, ex, ex.File);
					else
						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreateRtfInternal() multi-image X2: " + ex.Message + " Exception type: " + exceptionType, ex);

					throw new IllException("Can't create RTF file!");
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreateRtfInternal() multi-image X3: " + ex.Message, ex);
					throw new IllException("Can't create text file!");
				}
			}
		}*/
		#endregion

		#region CreateRtfInternal()
		private void CreateRtfInternal(FileInfo destFile, List<Iris.TextStruct> textStructList)
		{
			lock (this)
			{
				try
				{
					using (Idrs.IdrsWrapper idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir))
					{
						idrsWrapper.PdfImageAdded += new Idrs.ImageEventHndl(Iris_ImageAdded);
						idrsWrapper.RtfOcrDone += new Idrs.ImageEventHndl(Iris_RtfOcrDone);
						idrsWrapper.UniOcrDone += new Idrs.ImageEventHndl(Iris_UniOcrDone);
						idrsWrapper.TextOcrDone += new Idrs.ImageEventHndl(Iris_TextOcrDone);

						idrsWrapper.OpenRtf(destFile);

						for (int i = 0; i < textStructList.Count; i++)
						{
							FileInfo file = textStructList[i].XmlFile;

							if (file != null)
								idrsWrapper.AddRtfFormatFile(file);

							if ((i % 10) == 9)
							{
								idrsWrapper.Flush();
								destFile.Refresh();
							}

							Progress_Changed((float)((i + 1.0) / textStructList.Count) / 1.2F);
						}

						idrsWrapper.Close();
					}
				}
				catch (Idrs.IdrsWrapperException ex)
				{
					string exceptionType = ex.ExceptionType.ToString();

					if (ex.File != null && File.Exists(ex.File.FullName))
						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreateRtfInternal() multi-image X1: " + ex.Message + " Exception type: " + exceptionType, ex, ex.File);
					else
						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreateRtfInternal() multi-image X2: " + ex.Message + " Exception type: " + exceptionType, ex);

					throw new IllException("Can't create RTF file!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreateRtfInternal() multi-image X3: " + ex.Message, ex);
					throw new IllException("Can't create text file!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}
			}
		}
		#endregion

		#region RunIdrsOcrInternal()
		private string RunIdrsOcrInternal(FileInfo file)
		{
			lock (this)
			{
				string ocr = null;
				
				try
				{
					using (Idrs.IdrsWrapper idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir))
					{
						ocr = idrsWrapper.OCR(file);
					}
				}
				catch (Idrs.IdrsWrapperException ex)
				{
					if (ex.File != null && File.Exists(ex.File.FullName))
						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, RunIdrsOcrInternal(): " + ex.Message, ex, ex.File);
					else
						Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, RunIdrsOcrInternal(): " + ex.Message, ex);

					throw new IllException("Can't OCR image!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, RunIdrsOcrInternal(): " + ex.Message, ex);
					throw new IllException("Can't OCR image!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}

				return ocr;
			}
		}
		#endregion

		#region Iris_ImageAdded()
		private void Iris_ImageAdded(string image, float progress)
		{
			Progress_Changed(progress);
		}
		#endregion

		#region Iris_RtfOcrDone()
		private void Iris_RtfOcrDone(string image, float progress)
		{
			Progress_Changed(progress);
		}
		#endregion

		#region Iris_UniOcrDone()
		private void Iris_UniOcrDone(string image, float progress)
		{
			Progress_Changed(progress);
		}
		#endregion

		#region Iris_TextOcrDone()
		private void Iris_TextOcrDone(string image, float progress)
		{
			Progress_Changed(progress);
		}
		#endregion

		#region GetSize()
		private long GetSize(List<ITextSharp.InsertImageStruct> filesToInsert)
		{
			long size = 0;

			foreach (ITextSharp.InsertImageStruct str in filesToInsert)
				size += str.File.Length;

			return size;
		}
		#endregion

		#region DeleteFiles()
		private void DeleteFiles(List<ITextSharp.InsertImageStruct> filesToInsert)
		{
			foreach (ITextSharp.InsertImageStruct str in filesToInsert)
			{
				try
				{
					if (str.FileType == ITextSharp.FileType.Pdf)
					{
						str.File.Refresh();

						if (str.File.Exists)
							str.File.Delete();
					}
				}
				catch { }
			}
		}
		#endregion

		#region DeleteFileNoException()
		private void DeleteFileNoException(FileInfo file)
		{
			try
			{
				file.Refresh();

				if (file.Exists)
					file.Delete();
			}
			catch { }
		}
		#endregion

		#region iDRS_SpdfProgressChanged()
		void iDRS_SpdfProgressChanged(double progress)
		{
			Progress_Changed((float)(progress));
		}
		#endregion

		#region ITextSharp_MergingProgressChanged()
		void ITextSharp_MergingProgressChanged(float progress)
		{
			Progress_Changed((float)(0.5 + progress / 2.0));
		}
		#endregion

		#endregion
	
	}
}

#endif