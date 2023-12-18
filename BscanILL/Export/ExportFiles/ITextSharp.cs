using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace BscanILL.Export.ExportFiles
{
	public class ITextSharp : ExportFilesBasics, IDisposable
	{
		object locker = new object();

		Document document = null;
		PdfWriter writer = null;


		#region constructor()
		public ITextSharp()
		{
		}
		#endregion

		#region enum FileType
		public enum FileType
		{
			Pdf,
			Image
		}
		#endregion

		#region  class InsertImageStruct
		public class InsertImageStruct
		{
			public readonly FileType FileType;
			public readonly System.IO.FileInfo File;

			public InsertImageStruct(FileType fileType, System.IO.FileInfo file)
			{
				this.FileType = fileType;
				this.File = file;
			}
		}
		#endregion

		//PUBLIC METHODS
		#region public methods

		#region ExportImages()
        public void ExportImages(List<System.IO.FileInfo> sourceFiles, System.IO.FileInfo pdfFile, bool encrypt, bool pdfa1B, bool updateInfoBar)
		{
			lock (locker)
			{
				try
				{
                    if (updateInfoBar)
                    { 
					  Description_Changed("Creating multi-image PDF file...");
					  Progress_Changed(0);
                    }

					StartNewPDF(pdfFile.FullName, encrypt, pdfa1B);

					for (int i = 0; i < sourceFiles.Count; i++)
					{
						AddImage(sourceFiles[i]);

                        if (updateInfoBar)
						    Progress_Changed((i + 1.0F) / sourceFiles.Count);
					}

					ClosePDF(pdfa1B);

                    if (updateInfoBar)
					    Progress_Changed(1);
				}
				catch (Exception ex)
				{
					try
					{
						pdfFile.Refresh();
						if (pdfFile.Exists)
							pdfFile.Delete();
					}
					catch { }

					throw new Exception("Can't create PDF file!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
				}
			}
		}
		#endregion

		#region MergeFiles()
		public void MergeFiles(System.IO.FileInfo pdfFile, List<InsertImageStruct> filesToMerge, bool encrypt)
		{
			System.IO.FileInfo tempFile = new System.IO.FileInfo(pdfFile.FullName + "_temp.pdf");
			Document doc = null;

			try
			{
				pdfFile.Refresh();
				if (pdfFile.Exists)
					pdfFile.Delete();

				//just 1 pdf file, just rename it
				if (filesToMerge.Count == 1 && filesToMerge[0].FileType == ITextSharp.FileType.Pdf)
				{
					if (filesToMerge[0].File.FullName.ToLower() != pdfFile.FullName.ToLower())
						System.IO.File.Move(filesToMerge[0].File.FullName, pdfFile.FullName);
				}
				else
				{
					doc = new Document();
					doc.AddAuthor("DLSG at Image Access");
					System.IO.Stream stream = new System.IO.FileStream(tempFile.FullName, System.IO.FileMode.OpenOrCreate);
					PdfWriter pdfWriter = PdfWriter.GetInstance(doc, stream);

					pdfWriter.SetFullCompression();

					if (!doc.IsOpen())
						doc.Open();

					PdfContentByte contentByte = pdfWriter.DirectContent;

					for (int i = 0; i < filesToMerge.Count; i++)
					{
						if (filesToMerge[i].FileType == FileType.Image)
						{
							AddImageFromFile(filesToMerge[i].File, doc, contentByte);
						}
						else
						{
							AddPageFromPdf(filesToMerge[i].File, 1, doc, pdfWriter, contentByte);
						}
					}

					doc.Close();
					doc = null;

					pdfFile.Delete();
					tempFile.MoveTo(pdfFile.FullName);
				}
			}
			catch (Exception ex)
			{
				try
				{
					if (doc != null)
						doc.Close();

					pdfFile.Refresh();
					if (pdfFile.Exists)
						pdfFile.Delete();

					tempFile.Refresh();
					if (tempFile.Exists)
						tempFile.Delete();
				}
				catch { }

				throw new Exception("Can't create PDF file!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion

		#region ConvertSearchablePdfToPdfA()
		public static void ConvertSearchablePdfToPdfA(System.IO.FileInfo source)
		{
			string temp = source.FullName + @".temp";

			if (System.IO.File.Exists(temp))
				System.IO.File.Delete(temp);

			//open reader
			PdfReader inputPdf = new PdfReader(source.FullName);
			int pageCount = inputPdf.NumberOfPages;

			//set writer
			Document outputDocument = new Document();
			PdfWriter writer = PdfWriter.GetInstance(outputDocument, new System.IO.FileStream(temp, System.IO.FileMode.Create));

			writer.PDFXConformance = PdfWriter.PDFA1B;
			outputDocument.Open();

			PdfDictionary pdfDictionary = new PdfDictionary(PdfName.OUTPUTINTENT);
			pdfDictionary.Put(PdfName.OUTPUTCONDITIONIDENTIFIER, new PdfString("sRGB IEC61966-2.1"));
			pdfDictionary.Put(PdfName.INFO, new PdfString("sRGB IEC61966-2.1"));
			pdfDictionary.Put(PdfName.S, PdfName.GTS_PDFA1);
			ICC_Profile icc = ICC_Profile.GetInstance(BscanILL.Misc.Misc.StartupPath + @"\srgb.profile");
			PdfICCBased ib = new PdfICCBased(icc);
			ib.Remove(PdfName.ALTERNATE);

			pdfDictionary.Put(PdfName.DESTOUTPUTPROFILE, writer.AddToBody(ib).IndirectReference);
			writer.ExtraCatalog.Put(PdfName.OUTPUTINTENTS, new PdfArray(pdfDictionary));

			//copy pages
			PdfContentByte cb1 = writer.DirectContent;

			// copy pages from input to output outputDocument
			for (int i = 1; i <= pageCount; i++)
			{
				outputDocument.SetPageSize(inputPdf.GetPageSizeWithRotation(i));
				outputDocument.NewPage();

				PdfImportedPage page = writer.GetImportedPage(inputPdf, i);
				int rotation = inputPdf.GetPageRotation(i);

				if (rotation == 90 || rotation == 270)
					cb1.AddTemplate(page, 0, -1f, 1f, 0, 0, inputPdf.GetPageSizeWithRotation(i).Height);
				else
					cb1.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
			}

			//close writer
			writer.CreateXmpMetadata();
			outputDocument.Close();

			if (System.IO.File.Exists(source.FullName))
				System.IO.File.Delete(source.FullName);

			System.IO.File.Move(temp, source.FullName);
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region StartNewPDF()
		private void StartNewPDF(String fileName, bool encrypt, bool pdfa1B)
		{
			lock (locker)
			{
				// Setup the PDF
				document = new Document();
				document.AddAuthor("DLSG at Image Access");
				writer = PdfWriter.GetInstance(document, new System.IO.FileStream(fileName, System.IO.FileMode.Create));

				if (pdfa1B)
				{
					writer.PDFXConformance = PdfWriter.PDFA1B;
					document.Open();

					PdfDictionary pdfDictionary = new PdfDictionary(PdfName.OUTPUTINTENT);
					pdfDictionary.Put(PdfName.OUTPUTCONDITIONIDENTIFIER, new PdfString("sRGB IEC61966-2.1"));
					pdfDictionary.Put(PdfName.INFO, new PdfString("sRGB IEC61966-2.1"));
					pdfDictionary.Put(PdfName.S, PdfName.GTS_PDFA1);
					ICC_Profile icc = ICC_Profile.GetInstance(BscanILL.Misc.Misc.StartupPath + @"\srgb.profile");
					PdfICCBased ib = new PdfICCBased(icc);
					ib.Remove(PdfName.ALTERNATE);

					pdfDictionary.Put(PdfName.DESTOUTPUTPROFILE, writer.AddToBody(ib).IndirectReference);
					writer.ExtraCatalog.Put(PdfName.OUTPUTINTENTS, new PdfArray(pdfDictionary));
				}
				else if (encrypt)
				{
					writer.SetEncryption(PdfWriter.STRENGTH40BITS, null, null,
					PdfWriter.AllowAssembly |
					PdfWriter.AllowCopy |
					PdfWriter.AllowDegradedPrinting |
					PdfWriter.AllowFillIn |
					PdfWriter.AllowModifyAnnotations |
					PdfWriter.AllowModifyContents |
					PdfWriter.AllowPrinting |
					PdfWriter.AllowScreenReaders);
				}
			}
		}
		#endregion

		#region ClosePDF()
		private void ClosePDF(bool pdfa1B)
		{
			lock (locker)
			{
				if (document != null && document.IsOpen() == false)
				{
					document.Open();
					document.Add(new Paragraph("Bscan ILL Default PDF File."));
				}

				if (document != null && document.IsOpen())
				{
					if (writer != null && pdfa1B)
						writer.CreateXmpMetadata();

					document.Close();
				}

				document = null;
				writer = null;
			}
		}
		#endregion

		#region AddImage()
		private void AddImage(System.IO.FileInfo imageFile)
		{
			lock (locker)
			{
				iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageFile.FullName);

				iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(img.Width * 72 / img.DpiX, img.Height * 72 / img.DpiX);

				document.SetPageSize(rect);
				if (!document.IsOpen())
				{
					document.Open();
				}
				else
				{
					document.NewPage();
				}

				PdfContentByte cb = writer.DirectContent;
				writer.SetFullCompression();

				img.SetAbsolutePosition(0, 0);
				img.ScaleToFit(rect.Width, rect.Height);

				cb.AddImage(img);
			}
		}

		private void AddImage(System.Drawing.Bitmap bitmap)
		{
			lock (locker)
			{
				iTextSharp.text.Image img;

				using (System.IO.Stream stm = GetStream(bitmap))
				{
					img = iTextSharp.text.Image.GetInstance(stm);
				}

				iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(img.Width, img.Height);
				document.SetPageSize(rect);
				if (!document.IsOpen())
				{
					document.Open();
				}
				else
				{
					document.NewPage();
				}

				PdfContentByte cb = writer.DirectContent;
				writer.SetFullCompression();

				img.SetAbsolutePosition(0, 0);
				cb.AddImage(img);
			}
		}
		#endregion

		#region GetStream()
		private System.IO.Stream GetStream(System.Drawing.Bitmap bitmap)
		{
			System.IO.MemoryStream stm = new System.IO.MemoryStream();
			if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format1bppIndexed)
				bitmap.Save(stm, System.Drawing.Imaging.ImageFormat.Png);
			else
				bitmap.Save(stm, System.Drawing.Imaging.ImageFormat.Jpeg);

			stm.SetLength(stm.Length + 100);
			stm.Seek(0, System.IO.SeekOrigin.Begin);
			return (stm);
		}
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
		}

		// Implimentation Details...
		private bool hasBeenDisposed = false;

		protected virtual void Dispose(bool disposeManagedObjects)
		{
			if (!hasBeenDisposed)
			{
				// Mark your self as done...
				hasBeenDisposed = true;
				if (disposeManagedObjects)
				{
					// Dispose of all managed objects
					if (document != null && document.IsOpen())
					{
						document.Close();
					}
					// Dispose of all IDisposable objects

					// Take care of the garbage collector
					GC.SuppressFinalize(this);
				}
				// Release all unmanaged objects here...
			}
		}

		~ITextSharp()
		{
			Dispose(false);
		}
		#endregion

		#region AddImageFromFile()
		private static void AddImageFromFile(System.IO.FileInfo source, Document doc, PdfContentByte contentByte)
		{
			iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(source.FullName);
			iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(img.Width * 72 / img.DpiX, img.Height * 72 / img.DpiX);

			img.SetAbsolutePosition(0, 0);
			img.ScaleToFit(rect.Width, rect.Height);
			doc.SetPageSize(rect);
			doc.NewPage();
			contentByte.AddImage(img);
		}
		#endregion

		#region AddPageFromPdf()
		private static void AddPageFromPdf(System.IO.FileInfo source, int index, Document doc, PdfWriter pdfWriter, PdfContentByte contentByte)
		{
			PdfReader pdfReader = new PdfReader(source.FullName);

			doc.SetPageSize(pdfReader.GetPageSizeWithRotation(index));
			doc.NewPage();

			PdfImportedPage page = pdfWriter.GetImportedPage(pdfReader, index);
			int rotation = pdfReader.GetPageRotation(index);

			if (rotation == 90 || rotation == 270)
				contentByte.AddTemplate(page, 0, -1f, 1f, 0, 0, page.Height);
			else
				contentByte.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
		}
		#endregion

		#endregion

	}
}
