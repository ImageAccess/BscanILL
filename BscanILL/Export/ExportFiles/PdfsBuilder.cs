using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Data;

//using iTextSharp.text;
//using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.Threading;
using BscanILL.Misc;
using BscanILL.Hierarchy;
using System.Windows;

namespace BscanILL.Export.ExportFiles
{
	class PdfsBuilder : ExportFilesBasics
	{
		static PdfsBuilder			instance = null;
		static object				instanceLocker = new object();
        static object               abbyyInstanceLocker = new object();        
		
        Abbyy                       abbyy = null;        

		ITextSharp					iTextSharp = new ITextSharp();

		Queue<QueueFormatFileItem>	queueFormatFile = new Queue<QueueFormatFileItem>();
		object						queueFormatFileLocker = new object();
		AutoResetEvent				resetEvent = new AutoResetEvent(true);
		object						resetEventLocker = new object();
		volatile bool				keepQueueFormatFileRunning = true;

        DirectoryInfo ocrImageDirectoryInfo = null;         //temp directory for temp images for ocr/pdf generation

		#region constructor
		private PdfsBuilder()
		{
			this.iTextSharp.ProgressChanged += delegate(double progress) { Progress_Changed(progress); };
			this.iTextSharp.DescriptionChanged += delegate(string description) { Description_Changed(description); };
            
			Thread t = new Thread(new ThreadStart(QueueFormatFileThread));
			t.SetApartmentState(ApartmentState.STA);
			t.Name = "ThreadPdfBuilder_QueueFormatFile";
			t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			t.Start();
		}
		#endregion

		#region Dispose()
		private void Dispose()
		{
//			if (this.iris != null)
				//this.iris.Dispose();

            if (this.abbyy != null)
                this.abbyy.Dispose();

			this.keepQueueFormatFileRunning = false;

			lock (queueFormatFileLocker)
			{
				this.queueFormatFile.Clear();
			}

			lock (resetEventLocker)
			{
				this.resetEvent.Set();
			}
		}
		#endregion


		#region class ExportedIllPage
		public class ExportedIllPage
		{
			public readonly BscanILL.Hierarchy.IllPage IllPage;
			public readonly bool Searchable;

			public ExportedIllPage(BscanILL.Hierarchy.IllPage illPage, bool searchable)
			{
				this.IllPage = illPage;
				this.Searchable = searchable;
			}

            public ExportedIllPage(Abbyy.SpdfStruct spdfStruct)
            {
                this.IllPage = spdfStruct.IllPage;
                this.Searchable = (spdfStruct.XmlDir != null);
            }
		}
		#endregion

		#region class ExportedImages
		public class ExportedImages : List<ExportedIllPage>
		{

			public ExportedImages()
			{
			}

			public ExportedImages(ExportedIllPage exportedIllImage)
			{
				this.Add(exportedIllImage);
			}

            public ExportedImages(List<Abbyy.SpdfStruct> list)
            {
                foreach (Abbyy.SpdfStruct spdfStruct in list)
                    this.Add(new ExportedIllPage(spdfStruct));
            }

			public IllPages IllPages
			{
				get
				{
					IllPages illPages = new IllPages();

					foreach (ExportedIllPage exportedIllImage in this)
						illPages.Add(exportedIllImage.IllPage);

					return illPages;
				}
			}
		}
		#endregion

		#region class MultiPdf
		public class MultiPdf
		{
			public readonly FileInfo File;
			public readonly ExportedImages ExportedImages;

			public MultiPdf(FileInfo file, ExportedImages exportedIllImages)
			{
				this.File = file;
				this.ExportedImages = exportedIllImages;
			}
		}
		#endregion

		#region class QueueFormatFileItem
		internal class QueueFormatFileItem
		{
			public readonly BscanILL.Hierarchy.IllPdf	IllPdf;
			public volatile bool Canceled = false;
            public readonly Scanners.ColorMode colorMode = Scanners.ColorMode.Unknown ;
            public readonly int quality = 100;

			public QueueFormatFileItem(BscanILL.Hierarchy.IllPdf illPdf, Scanners.ColorMode imageColorMode, int imageQuality)
			{
				this.IllPdf = illPdf;
                this.colorMode = imageColorMode;
                this.quality = imageQuality;

            }
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		public static PdfsBuilder Instance 
		{ 
			get
			{
				lock (instanceLocker)
				{
					if (instance == null)
						instance = new PdfsBuilder();
				}

				return instance;
			}
		}

        public DirectoryInfo OcrImageDirectoryInfo
        {
            get
            {
                return ocrImageDirectoryInfo;
            }
            set
            {
                ocrImageDirectoryInfo = value;
            }
        }

        #endregion

        // PRIVATE PROPERTIES
        #region private properties

        #region Abbyy
        private Abbyy Abbyy
        {
            get
            {
                if (this.abbyy == null)
                {
                    this.abbyy = Abbyy.Instance;

                    this.abbyy.ProgressChanged += delegate(double progress) { Progress_Changed(progress); };
                    this.abbyy.DescriptionChanged += delegate(string description) { Description_Changed(description); };
                }

                return this.abbyy;
            }
        }
        #endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region DisposeIfNecessary()
		public static void DisposeIfNecessary()
		{
			lock (instanceLocker)
			{
              lock (abbyyInstanceLocker)
              {
				if (instance != null)
				{
					instance.Dispose();
					instance = null;
				}
              }
			}
		}
        #endregion


        #region GetOcrImageExternal()
        public FileInfo GetOcrImageExternal(BscanILL.Hierarchy.IllPage illPage, DirectoryInfo destDir, Scanners.ColorMode pdfColorDepth, int fileExportQuality, ref Scanners.ColorMode imageColorMode)
        {
            FileInfo ocrImage = GetOcrImage(illPage, destDir, pdfColorDepth, fileExportQuality, ref imageColorMode, true);

            return ocrImage;
        }
        #endregion

        #region GetSingleFile()
        /// <summary>
        /// searchable, maximum dpi is 400, because of IRIS
        /// </summary>
        /// <param name="illPage"></param>
        /// <param name="pdfFile"></param>
        /// <param name="dpi"></param>
        /// <param name="searchable"></param>
        /// <param name="maxFileSize"></param>
        /// <param name="pdfA1B"></param>
        public void GetSingleFile(BscanILL.Export.ExportUnit exportUnit, BscanILL.Hierarchy.IllPage illPage, FileInfo pdfFile, bool searchable, ulong maxFileSize, bool pdfA1B)
		{
			BscanILL.Hierarchy.IllPages illPages = new BscanILL.Hierarchy.IllPages() { illPage };
			List<string>				warnings = new List<string>();

			if (maxFileSize == 0)
				GetMultiImagePdf(exportUnit, illPages, pdfFile, searchable, warnings, pdfA1B, false, exportUnit.AdditionalInfo.FileColor, exportUnit.AdditionalInfo.FileQuality);
			else
			{
				List<MultiPdf> list = GetMultiImagePdfWithSizeLimit(exportUnit, illPages, searchable, maxFileSize, warnings, pdfA1B, false, exportUnit.AdditionalInfo.FileColor, exportUnit.AdditionalInfo.FileQuality);
				pdfFile = list[0].File;
			}

			if (warnings.Count > 0)
				throw new Exception(warnings[0]);
		}
		#endregion

		#region GetMultiImagePdf()
		/// <summary>
		/// searchable, maximum dpi is 400, because of IRIS
		/// </summary>
		/// <param name="illPages">null if to get IllPages from Article</param>
		/// <param name="pdfFile"></param>
		/// <param name="dpi"></param>
		/// <param name="searchable"></param>
		/// <param name="maxFileSize"></param>
		/// <param name="warnings"></param>
		/// <param name="pdfA1B"></param>
		public void GetMultiImagePdf(BscanILL.Export.ExportUnit exportUnit, BscanILL.Hierarchy.IllPages illPages, FileInfo pdfFile, bool searchable, List<string> warnings, bool pdfA1B, bool updateInfoBar, SETTINGS.Settings.GeneralClass.PdfColorDepth pdfColor, int pdfExportQuality)
		{			
			DirectoryInfo tempDir = null;
			int order = 0;

			try
			{
				pdfFile.Refresh();
				if (pdfFile.Exists)
					pdfFile.Delete();

				order = 1;

				tempDir = new DirectoryInfo(pdfFile.Directory.FullName + @"\temp");

				pdfFile.Directory.Create();
				tempDir.Create();

                this.OcrImageDirectoryInfo = tempDir;

                order = 2;

                if (updateInfoBar)
                  Description_Changed("Loading source files...");

				if (searchable)
				{                    
                    Scanners.ColorMode pdfExportColorDepth = Scanners.ColorMode.Unknown;

                    order = 600;

                    List<Abbyy.SpdfStruct> tempImagesAbbyy = new List<Abbyy.SpdfStruct>();

					order = 601;

                    //getting xml files
                                        
                    //not sure why it is here in code (Bob) - maybe it is to make sure the xml file was already created, there is lock code when getting FormatXmlFileDir to make sure Abbyy has finished generating the xml folder
                    for (int i = 0; i < illPages.Count; i++)
                    {
                        DirectoryInfo xmlFileDir = illPages[i].FormatXmlFileDir;                        
                       //Progress_Changed((i + 1.0F) / illPages.Count);
                    }

                    order = 602;
                    if (pdfColor == SETTINGS.Settings.GeneralClass.PdfColorDepth.Color)
                    {
                        pdfExportColorDepth = Scanners.ColorMode.Color;

                    }
                    else
                    if (pdfColor == SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale)
                    {
                        pdfExportColorDepth = Scanners.ColorMode.Grayscale;

                    }
                    else
                    if (pdfColor == SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
                    {
                        pdfExportColorDepth = Scanners.ColorMode.Bitonal;

                    }                    

                    //creating supporting images
                    for (int i = 0; i < illPages.Count; i++)
					{                        
                        Scanners.ColorMode imageColorMode = Scanners.ColorMode.Unknown;

                        if (pdfColor == SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto)
                        {
                            //auto color -> set by original image
                            pdfExportColorDepth = illPages[i].ColorMode;
                        }
                        //////FileInfo imageToInsert = SaveImage(exportUnit, illPages[i], tempDir, GetPixelFormat(illPages[i]), searchable, color, pdfExportQuality);
                        FileInfo imageToInsert = GetOcrImage(illPages[i], tempDir, pdfExportColorDepth, pdfExportQuality, ref imageColorMode, false);                        
                        if (imageToInsert != null)
                        {
                            imageToInsert.Refresh();

                            //new image was created it means parameters of image for pdf has changes so we need to recreate the XML folder with Abbyy preprocessed files                            
                            DirectoryInfo formatFileDir = illPages[i].FormatXmlFileDir;                            
                            if(formatFileDir == null)
                            {
                                formatFileDir = new DirectoryInfo(illPages[i].FilePath.DirectoryName + "\\" + illPages[i].FilePath.Name + "_xml");
                            }

                            illPages[i].ClearFormatXmlFileDir();

                            formatFileDir.Create();
                            formatFileDir.Refresh();

                            order = 31 + i;

                            if (_settings.General.OcrEngProfile == BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Speed)
                            {
                                //Speed
                                this.Abbyy.OcrProfile = "Document Archiving Speed";     //Document Archiving is recommended profile for PDF creation. For RTF or PDF text only is recommended Document Conversion profile

                            }
                            else
                            {
                                //Accuracy
                                this.Abbyy.OcrProfile = "Document Archiving Accuracy";
                            }

                            this.Abbyy.FormatImage(imageToInsert, formatFileDir);
                            
                            formatFileDir.Refresh();
                            if (formatFileDir.Exists == false)
                            {
                                    throw new Exception("Abbyy FormatImage Process Failed!");
                            }                            

                            illPages[i].FormattedDirCreated(formatFileDir, imageColorMode, pdfExportQuality, imageToInsert.Length);
                            
                        }

                        DirectoryInfo xmlFileDir = illPages[i].FormatXmlFileDir;
                        tempImagesAbbyy.Add(new Abbyy.SpdfStruct(illPages[i], xmlFileDir, imageToInsert));   //in  case we do not reprocess image with Abbyy the imageToInsert will be null but it is OK, we do not need it for Abbyy as source image is already integrated into the xml data folder

                        if (updateInfoBar)
						        Progress_Changed((i + 1.0F) / illPages.Count);
					}

                    if (updateInfoBar)
                        Progress_Changed(1.0F);

					order = 605;
                    
                        if (tempImagesAbbyy.Count > 1)
                            this.Abbyy.CreatePdf(pdfFile, tempImagesAbbyy, 0, warnings, updateInfoBar, pdfExportQuality);
                        else
                            this.Abbyy.CreatePdf(pdfFile, tempImagesAbbyy[0], updateInfoBar, pdfExportQuality);
                    
					order = 606;
					if (pdfA1B)
						ITextSharp.ConvertSearchablePdfToPdfA(pdfFile);
				}
				else
				{
					order = 700;
					List<FileInfo> tempImages = new List<FileInfo>();

                    for (int i = 0; i < illPages.Count; i++)
                    {                        
                        FileInfo tempFile = SaveImage(exportUnit, illPages[i], tempDir, GetPixelFormat(illPages[i]), searchable, pdfColor, pdfExportQuality);
						order = 701 + i;

						tempImages.Add(tempFile);

                        if (updateInfoBar)
						    Progress_Changed((i + 1.0F) / (illPages.Count));
					}

					order = 800;
                    iTextSharp.ExportImages(tempImages, pdfFile, false, pdfA1B, updateInfoBar);				
					order = 801;
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "PdfBuilder, GetMultiImageFiles() " + order.ToString() + ": " + ex.Message, ex);

				throw ex;
			}

			try
            {
                if (tempDir != null)
                {
                    tempDir.Delete(true);
                }
                this.OcrImageDirectoryInfo = null;
            }
			catch { }
		}
		#endregion

		#region GetMultiImagePdfWithSizeLimit()		
		/// <summary>
		/// searchable, maximum dpi is 400, because of IRIS
		/// </summary>
		/// <param name="exportUnit"></param>
		/// <param name="illPages">null if to get IllPages from Article</param>
		/// <param name="searchable"></param>
		/// <param name="maxFileSize"></param>
		/// <param name="warnings"></param>
		/// <param name="pdfA1B"></param>
		/// <returns></returns>
        public List<MultiPdf> GetMultiImagePdfWithSizeLimit(BscanILL.Export.ExportUnit exportUnit, BscanILL.Hierarchy.IllPages illPages, bool searchable, ulong maxFileSize, List<string> warnings, bool pdfA1B, bool updateInfoBar, SETTINGS.Settings.GeneralClass.PdfColorDepth pdfColor, int pdfExportQuality)
		{	
			if(illPages == null)
				illPages = exportUnit.Article.GetPages(true);
			
			DirectoryInfo tempDir = null;
			int order = 0;
			List<MultiPdf> multiPdfs = new List<MultiPdf>();

			try
			{
				tempDir = new DirectoryInfo(exportUnit.Directory.FullName + @"\temp");
				tempDir.Create();
                this.OcrImageDirectoryInfo = tempDir;

                order = 2;

                if (updateInfoBar)
                    Description_Changed("Loading source files...");

				if (searchable)
				{
					order = 3;
                    //getting xml files  

                    //not sure why it is here in code (Bob) - maybe it is to make sure the xml file was already created, there is lock code when getting FormatXmlFile to make sure Abbyy has finished generating the xml folder
                    for (int i = 0; i < illPages.Count; i++)
					{
						DirectoryInfo xmlFileDir = illPages[i].FormatXmlFileDir;		
						//Progress_Changed((i + 1.0F) / illPages.Count);
					}
				}

                Scanners.ColorMode pdfExportColorDepth = Scanners.ColorMode.Unknown;

                //creating supporting images
                List<Abbyy.SpdfStruct> spdfStructListAbbyy = new List<Abbyy.SpdfStruct>();

                if (pdfColor == SETTINGS.Settings.GeneralClass.PdfColorDepth.Color)
                {
                    pdfExportColorDepth = Scanners.ColorMode.Color;
                }
                else
                if (pdfColor == SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale)
                {
                    pdfExportColorDepth = Scanners.ColorMode.Grayscale;
                }
                else
                if (pdfColor == SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
                {
                    pdfExportColorDepth = Scanners.ColorMode.Bitonal;
                }

                FileInfo imageToInsert = null;

                //creating supporting images
                for (int i = 0; i < illPages.Count; i++)
				{
                    bool needAbbyyProcess = false;
                    Scanners.ColorMode imageColorMode = Scanners.ColorMode.Unknown;
                    
                    if (!searchable)
                    {
                        imageToInsert = SaveImage(exportUnit, illPages[i], tempDir, GetPixelFormat(illPages[i]), searchable, pdfColor, pdfExportQuality);                    
                    }
                    else
                    {
                        if (pdfColor == SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto)
                        {
                            //auto color -> set by original image
                            pdfExportColorDepth = illPages[i].ColorMode;
                        }

                        imageToInsert = GetOcrImage(illPages[i], tempDir, pdfExportColorDepth, pdfExportQuality, ref imageColorMode, false);
                    }

                    order = 31;

                    if (imageToInsert != null) 
                    {
                        //new image was created it means parameters of image for pdf has changes so we need to recreate the XML folder with Abbyy preprocessed files
                        imageToInsert.Refresh();

                        needAbbyyProcess = true;
/*
                        ///re-process with Abbyy
                        DirectoryInfo formatFileDir = illPages[i].FormatXmlFileDir;
                        if (formatFileDir == null)
                        {
                            formatFileDir = new DirectoryInfo(illPages[i].FilePath.DirectoryName + "\\" + illPages[i].FilePath.Name + "_xml");
                        }

                        illPages[i].ClearFormatXmlFileDir();

                        formatFileDir.Create();
                        formatFileDir.Refresh();
   
                        this.Abbyy.FormatImage(imageToInsert, formatFileDir);

                        formatFileDir.Refresh();
                        if (formatFileDir.Exists == false)
                        {
                            throw new Exception("Abbyy FormatImage Process Failed!");
                        }

                        illPages[i].FormattedDirCreated(formatFileDir, imageColorMode, pdfExportQuality);
*/
                    }
                    else
                    {
                        //use previously ocred imaged backed up in xml folder                        
      //                  string extension = "";

      //                  extension = System.IO.Path.GetExtension(illPages[i].FilePath.Name);
      //                  imageToInsert = new FileInfo(string.Format("{0}\\{1}_ocr{2}", tempDir.FullName, System.IO.Path.GetFileNameWithoutExtension(illPages[i].FilePath.Name), extension));
                    }
                    DirectoryInfo xmlFileDir = illPages[i].FormatXmlFileDir;

                    //in  case we do not reprocess image with Abbyy the imageToInsert will be null but it is OK, we do not need it 
                    //for Abbyy as source image is already integrated into the xml data folder
                    spdfStructListAbbyy.Add(new Abbyy.SpdfStruct(illPages[i], xmlFileDir, imageToInsert, needAbbyyProcess, imageColorMode));

                    if (updateInfoBar)
    			       Progress_Changed((i + 1.0F) / illPages.Count);
				}

				//order = 4;
				order = 665;

                if (maxFileSize > 0)
                {
                    //images processed with Abbyy just when launching export dialog in method GetOCRFormatedDir (images not created in above call to GetOcrImage() method because pdf output 
                    //color mode matches original abbyy pre-processing) are not present on disk anymore., if we will be reducing their file size, we will need to recreate them before rescaling
                    //ReduceImageSizesIfNecessaryAbbyy(spdfStructListAbbyy, maxFileSize * 0.9);                    
                    ReduceImageSizesIfNecessary(spdfStructListAbbyy, maxFileSize * 0.9);
                }

                if (searchable)
                {
                    //now when we have a correct image sizes -> run Abbyy on new images
                    for (int i = 0; i < spdfStructListAbbyy.Count; i++)
                    {
                        if(spdfStructListAbbyy[i].NeedAbbyyProcessing)
                        {
                            ///re-process with Abbyy
                            DirectoryInfo formatFileDir = spdfStructListAbbyy[i].IllPage.FormatXmlFileDir;
                            if (formatFileDir == null)
                            {
                                formatFileDir = new DirectoryInfo(spdfStructListAbbyy[i].IllPage.FilePath.DirectoryName + "\\" + spdfStructListAbbyy[i].IllPage.FilePath.Name + "_xml");
                            }

                            spdfStructListAbbyy[i].IllPage.ClearFormatXmlFileDir();

                            formatFileDir.Create();
                            formatFileDir.Refresh();

                            if (_settings.General.OcrEngProfile == BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Speed)
                            {
                                //Speed
                                this.Abbyy.OcrProfile = "Document Archiving Speed";     //Document Archiving is recommended profile for PDF creation. For RTF or PDF text only is recommended Document Conversion profile

                            }
                            else
                            {
                                //Accuracy
                                this.Abbyy.OcrProfile = "Document Archiving Accuracy";
                            }

                            this.Abbyy.FormatImage(spdfStructListAbbyy[i].ImageToInsert, formatFileDir);

                            formatFileDir.Refresh();
                            if (formatFileDir.Exists == false)
                            {
                                throw new Exception("Abbyy FormatImage Process Failed!");
                            }

                            spdfStructListAbbyy[i].IllPage.FormattedDirCreated(formatFileDir, spdfStructListAbbyy[i].ImageColorMode, pdfExportQuality, spdfStructListAbbyy[i].ImageToInsert.Length);
                        }
                    }
                }

                    order = 666;

                if (spdfStructListAbbyy.Count == 1)
				{
					FileInfo pdfFile = GetExportFilePath(exportUnit);

                    if (searchable)
                    {
                        this.Abbyy.CreatePdf(pdfFile, spdfStructListAbbyy[0], updateInfoBar, pdfExportQuality);                        
                    }
                    else
                    {
                        iTextSharp.ExportImages(GetFilesAbbyy(spdfStructListAbbyy), pdfFile, false, pdfA1B, updateInfoBar);                                               
                    }

					order = 667;

                        multiPdfs.Add(new MultiPdf(pdfFile, new ExportedImages(new ExportedIllPage(spdfStructListAbbyy[0]))));                    
				}
				else
				{
                    long lCurrentImgSize = 0;
                    order = 700;

                        List<Abbyy.SpdfStruct> listAbbyy = new List<Abbyy.SpdfStruct>();
                   
                        for (int i = 0; i < spdfStructListAbbyy.Count; i++)
                        {
                            if (spdfStructListAbbyy[i].ImageToInsert != null)
                            {
                               lCurrentImgSize = spdfStructListAbbyy[i].ImageToInsert.Length;
                            }
                            else
                            {
                               lCurrentImgSize = spdfStructListAbbyy[i].IllPage.SourceFileSize;
                            }

                            //if (GetExportFilesSizeAbbyy(listAbbyy) + (ulong)spdfStructListAbbyy[i].ImageToInsert.Length < maxFileSize * 0.9)
                            if (GetExportFilesSizeAbbyy(listAbbyy) + (ulong)lCurrentImgSize < maxFileSize * 0.9)
                            {
                              listAbbyy.Add(spdfStructListAbbyy[i]);
                            }
                            else
                            {
                              if (listAbbyy.Count > 0)
                              { 
                                FileInfo pdf = GetExportFilePath(exportUnit);

                                if (searchable)
                                  this.Abbyy.CreatePdf(pdf, listAbbyy, 0, warnings, updateInfoBar, pdfExportQuality);
                                else
                                  iTextSharp.ExportImages(GetFilesAbbyy(listAbbyy), pdf, false, pdfA1B, updateInfoBar);

                                ExportedImages exportedIllImages = new ExportedImages(listAbbyy);

                                multiPdfs.Add(new MultiPdf(pdf, exportedIllImages));
                                listAbbyy.Clear();
                              }
                              listAbbyy.Add(spdfStructListAbbyy[i]);
                            }
                        }

                        order = 701;                 
                   
                        if (listAbbyy.Count > 0)
                        {
                            FileInfo pdf = GetExportFilePath(exportUnit);

                            if (searchable)
                                this.Abbyy.CreatePdf(pdf, listAbbyy, 0, warnings, updateInfoBar, pdfExportQuality);
                            else
                                iTextSharp.ExportImages(GetFilesAbbyy(listAbbyy), pdf, false, pdfA1B, updateInfoBar);

                            ExportedImages exportedIllImages = new ExportedImages(listAbbyy);

                            multiPdfs.Add(new MultiPdf(pdf, exportedIllImages));
                            listAbbyy.Clear();
                        }
				}

				if (pdfA1B)
				{
					order = 800;
					foreach (MultiPdf multiPdf in multiPdfs)
						ITextSharp.ConvertSearchablePdfToPdfA(multiPdf.File);
					order = 801;
				}

				order = 900;
				return multiPdfs;
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "PdfBuilder, GetMultiImageFiles() " + order.ToString() + ": " + ex.Message, ex);

				throw ex;
			}
			finally
			{
				try 
				{
					if (tempDir != null)
						tempDir.Delete(true);

                    this.OcrImageDirectoryInfo = null;
                }
				catch { }
			}
		}
		#endregion

		#region Export()
		/// <summary>
		/// searchable, maximum dpi is 400, because of IRIS
		/// </summary>
		/// <param name="illPages"></param>
		/// <param name="pdfFile"></param>
		/// <param name="dpi"></param>
		/// <param name="searchable"></param>
		/// <param name="maxFileSize"></param>
		/// <param name="warnings"></param>
		/// <param name="pdfA1B"></param>        
		#endregion


		#region QueueToFormattedFile()
		internal void QueueToFormattedFile(BscanILL.Hierarchy.IllPdf illPdf, Scanners.ColorMode colorMode, int quality)
		{
			lock (queueFormatFileLocker)
			{
				this.queueFormatFile.Enqueue(new QueueFormatFileItem(illPdf, colorMode, quality));
			}

			lock (resetEventLocker)
			{
				this.resetEvent.Set();
			}
		}
		#endregion

		#region RemoveFromQueue()
		internal void RemoveFromQueue(BscanILL.Hierarchy.IllPdf illPdf)
		{
			lock (queueFormatFileLocker)
			{
				foreach (QueueFormatFileItem item in this.queueFormatFile)
					if (item.IllPdf == illPdf)
						item.Canceled = true;
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region QueueFormatFileThread()
		private void QueueFormatFileThread()
		{
                while (this.keepQueueFormatFileRunning)
                {
                    this.resetEvent.WaitOne();

                    lock (resetEventLocker)
                    {
                        this.resetEvent.Reset();
                    }

                    while (this.queueFormatFile.Count > 0)
                    {
                        QueueFormatFileItem item = null;

                        lock (queueFormatFileLocker)
                        {
                            item = this.queueFormatFile.Dequeue();
                        }

                        if (item.Canceled)
                            item.IllPdf.FormattedFileCreationCanceled();

                        try
                        {
                        //ABBYY  
                        lock (abbyyInstanceLocker)
                            {
                                if (this.keepQueueFormatFileRunning)
                                {
                                    Scanners.ColorMode imageColorMode = Scanners.ColorMode.Unknown;
                                    long imageFileSize = 0;
                                    //DirectoryInfo formattedDir = GetOcrFormatedDir(item.IllPdf.IllPage, item.colorMode, item.quality, ref imageColorMode, ref imageFileSize);
                                    DirectoryInfo formattedDir = GetOcrFormatedDir(item.IllPdf.IllPage, item.colorMode, 100, ref imageColorMode, ref imageFileSize);   //force ocr on best quality image

                                    item.IllPdf.FormattedDirCreated(formattedDir, imageColorMode, item.quality, imageFileSize);

#if DEBUG
                                    if (formattedDir == null)
                                        System.Windows.MessageBox.Show("QueueFormatDirThread null!");
#endif                                
                                }
                            }        

                        }
                        catch (Exception)
                        {
                            item.IllPdf.FormattedFileCreationError();
                        }
                    }
                }            
		}
        #endregion

        #region GetFileFormat()
        static ImageProcessing.FileFormat.IImageFormat GetFileFormat(BscanILL.Hierarchy.IllPage illPage)
		{
			switch (illPage.ColorMode)
			{
				case Scanners.ColorMode.Bitonal:
					return new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4);
				case Scanners.ColorMode.Grayscale:
					return new ImageProcessing.FileFormat.Png();
				default:
					return new ImageProcessing.FileFormat.Jpeg(80);
			}
		}
		#endregion

		#region GetImageFileFormat()
		static ImageFormat GetImageFileFormat(BscanILL.Hierarchy.IllPage illPage)
		{
			switch (illPage.ColorMode)
			{
				case Scanners.ColorMode.Bitonal:
					return ImageFormat.Tiff;
				case Scanners.ColorMode.Grayscale:
					return ImageFormat.Png;
				default:
					return ImageFormat.Jpeg;
			}
		}
		#endregion

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

		#region SaveImage()
		/// <summary>
		/// maximum dpi will be 400
		/// </summary>
		/// <param name="illPage"></param>
		/// <param name="destDir"></param>
		/// <param name="pixelFormat"></param>
		/// <param name="dpi"></param>
		/// <param name="searchable"></param>
		/// <returns></returns>
		private static FileInfo SaveImage(BscanILL.Export.ExportUnit exportUnit, BscanILL.Hierarchy.IllPage illPage, DirectoryInfo destDir, PixelFormat pixelFormat, bool searchable, SETTINGS.Settings.GeneralClass.PdfColorDepth exportColor, int exportQuality)
		{
			BscanILL.Export.IP.ExportFileCreator		creator = new BscanILL.Export.IP.ExportFileCreator();
			ImageProcessing.FileFormat.IImageFormat		fileFormat;
			List<string>								warnings = new List<string>();
			string										outputFile = destDir.FullName + @"\" + illPage.FilePath.Name;

/*
			switch (illPage.ColorMode)
			{
				case Scanners.ColorMode.Bitonal:
					{
                      //not sure why Jirka used PNG for bitonal - G4 is better compression
						if (searchable)
						{
							fileFormat = new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4);
							file += ".tif";
						}
						else
						{
							fileFormat = new ImageProcessing.FileFormat.Png();
							file += ".png";
						}
					} break;
				case Scanners.ColorMode.Grayscale:
					{
						fileFormat = new ImageProcessing.FileFormat.Jpeg(85);
						file += ".jpg";
					} break;
				default:
					{
						fileFormat = new ImageProcessing.FileFormat.Jpeg(85);
						file += ".jpg";
					} break;
			}
*/
            Scanners.ColorMode colorMode = Scanners.ColorMode.Bitonal;
            switch (exportColor)
            {
                case SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal:
                    {
                        //not sure why Jirka used PNG for bitonal - G4 is better compression, maybe iTextSharp doesnot handle TIFF-G4, so I'll leave this code in place for safety...
                        if (searchable)
                        {
                            fileFormat = new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4);
                            outputFile += ".tif";
                        }
                        else
                        {
                            fileFormat = new ImageProcessing.FileFormat.Png();
                            outputFile += ".png";
                        }
                        colorMode = Scanners.ColorMode.Bitonal;
                    } break;
                case SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale:
                    {
                        fileFormat = new ImageProcessing.FileFormat.Jpeg((byte)exportQuality);  //jpeg quality in range <1,100>
                        outputFile += ".jpg";
                        colorMode = Scanners.ColorMode.Grayscale;
                    } break;
                case SETTINGS.Settings.GeneralClass.PdfColorDepth.Color:
                    {
                        fileFormat = new ImageProcessing.FileFormat.Jpeg((byte)exportQuality);
                        outputFile += ".jpg";
                        colorMode = Scanners.ColorMode.Color;
                    } break;
                default:   //auto - use bit depth of original image
                    {
                        switch (illPage.ColorMode)
                        {
                            case Scanners.ColorMode.Bitonal:
                                {
                                    if (searchable)
                                    {
                                        fileFormat = new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4);
                                        outputFile += ".tif";
                                    }
                                    else
                                    {
                                        fileFormat = new ImageProcessing.FileFormat.Png();
                                        outputFile += ".png";
                                    }
                                    colorMode = Scanners.ColorMode.Bitonal;

                                } break;
                            case Scanners.ColorMode.Grayscale:
                                {
                                    fileFormat = new ImageProcessing.FileFormat.Jpeg((byte)exportQuality);
                                    outputFile += ".jpg";
                                    colorMode = Scanners.ColorMode.Grayscale;
                                } break;
                            default:
                                {
                                    fileFormat = new ImageProcessing.FileFormat.Jpeg((byte)exportQuality);
                                    outputFile += ".jpg";
                                    colorMode = Scanners.ColorMode.Color;
                                } break;
                        }
                    } break;
            }

			ushort dpi = (ushort)illPage.Dpi;

			if(searchable)
				dpi = (ushort)Math.Min((short)300, illPage.Dpi);

			destDir.Create();

			FileInfo fileInfo = new FileInfo(outputFile);

            //creator.CreateExportFile(illPage.FilePath, fileInfo, fileFormat, illPage.ColorMode, dpi / (double)illPage.FullImageInfo.DpiH);
            creator.CreateExportFile(illPage.FilePath, fileInfo, fileFormat, colorMode, dpi / (double)illPage.FullImageInfo.DpiH);

			fileInfo.Refresh();
			return fileInfo;
		}
		#endregion

		#region GetOcrImage()
		private FileInfo GetOcrImage(BscanILL.Hierarchy.IllPage illPage, DirectoryInfo destDir, Scanners.ColorMode pdfColorDepth, int fileExportQuality, ref Scanners.ColorMode imageColorMode, bool forceCreation)
		{
			FileInfo	source = illPage.FilePath;
			FileInfo	ocrImage;

            string extension = "";

            extension = System.IO.Path.GetExtension(source.Name);
            ocrImage = new FileInfo(string.Format("{0}\\{1}_ocr{2}", destDir.FullName, System.IO.Path.GetFileNameWithoutExtension(source.Name), extension));

            ocrImage.Refresh();
            if (ocrImage.Exists)
                ocrImage.Delete();

            ocrImage.Refresh();
                       
            using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(source.FullName))
            {
                bool needCreateFile = false;
                bool firstCreation = false;

                int sourceImageColor = 1;   //ImageProcessing.PixelsFormat.FormatBlackWhite
                int lastProcessedColor = 0;

                if ((itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format4bppGray) || (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format8bppGray) || (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format8bppIndexed))
                {
                    sourceImageColor = 8;
                }
                else
                if ((itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format24bppRgb) || (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format32bppRgb))
                {
                    sourceImageColor = 24;
                }

                if ((illPage.ExportQualityOfPreProcessedPDF == 0) || (illPage.ColorDepthOfPreProcessedPDF == Scanners.ColorMode.Unknown))
                {
                    //image for pdf was not created yet -> check color of current source image                    
                    firstCreation = true;
                }
                else
                {
                    // check parameters of already created file for OCR purposes to see if we need to create new image with new color or quality parameters
                    if (illPage.ColorDepthOfPreProcessedPDF == Scanners.ColorMode.Bitonal)
                    {
                           lastProcessedColor = 1;
                    }
                    else
                    if (illPage.ColorDepthOfPreProcessedPDF == Scanners.ColorMode.Grayscale )
                    {
                            lastProcessedColor = 8;
                    }
                    else
                    if(illPage.ColorDepthOfPreProcessedPDF == Scanners.ColorMode.Color)
                    {
                            lastProcessedColor = 24;
                    }
                }

                //check desired/output color debth                
                int outColor = 1;

                if(pdfColorDepth == Scanners.ColorMode.Grayscale)
                {
                    outColor = 8;
                }
                else
                if (pdfColorDepth == Scanners.ColorMode.Color)
                {
                    outColor = 24;
                }

                if(sourceImageColor < outColor)   //make sure outColor is actual color of created file not just desired pdf color mode
                {
                    outColor = sourceImageColor;
                }

                bool needImageProcessing = false;
                if (firstCreation || forceCreation)
                {
                        needCreateFile = true;
                        if (sourceImageColor > outColor)
                        {
                           needImageProcessing = true;
                           if (outColor > 1)
                           {
                             fileExportQuality = 100;      //when changing to a different output color - use highest quality image for ocr
                           }
                        }

                        //do not worry about image quality of source image - Abbyy will reduce it with jpeg quality when creating pdf file
/*
                        if ( (fileExportQuality != 100) && (outColor > 1) )     //we cannot manage image quality of bitonal images in this function so do not worry about quality if BW
                            needImageProcessing = true;
*/                        
                }
                else
                {
                        //do we need to rerun Abbyy with modified image?
                        if( (lastProcessedColor == outColor) )
                        {
                            //do not worry about image quality of source image - Abbyy will reduce it with jpeg quality when creating pdf file
/*
                            if ( (illPage.ExportQualityOfPreProcessedPDF == fileExportQuality) || (outColor == 1) )
                            {
                                //new params match last processing - no need to reprocess
                                needCreateFile = false;
                            }
                            else
                            {
                                //need to reprocess with resampling because of image quality
                                needCreateFile = true;
                                needImageProcessing = true;
                            }
*/
                        }
                        else
                        {
                            needCreateFile = true;
                            if (sourceImageColor > outColor)
                            {                                
                                needImageProcessing = true;
                                if(outColor > 1)
                                {
                                  fileExportQuality = 100;      //when changing to a different output color - use highest quality image for ocr
                                }
                            }
                            else
                            {
                              //do not worry about image quality of source image - Abbyy will reduce it with jpeg quality when creating pdf file
/*
                                // color mode of source image is same as outColorMode - check if need to create image becasue of image quality
                                if (outColor > 1)              //if we want to export to color or gray
                                {
                                    if(fileExportQuality < 100)   //if we want to reduce output image quality
                                    {
                                        needImageProcessing = true;
                                    }
                                }  
*/                                
                            }
                        }
                }
           
                if (needCreateFile)
                {
                        if (!needImageProcessing)
                        {
                            Directory.CreateDirectory(destDir.FullName);
                            File.Copy(source.FullName, ocrImage.FullName, true);        //currently we just copy an image for OCR - maybe consider not to copy, just use original image for ocr purposes..
                        }
                        else
                        {
                            if (sourceImageColor <= outColor)
                            {
                                // we are just changing image quality and output image is color or gray image
                                ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();
                                copier.Copy(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Jpeg((byte)fileExportQuality));
                            }
                            else
                            {
                                //changing also color mode
                                ImageProcessing.BigImages.ResizingAndResampling resizing = new ImageProcessing.BigImages.ResizingAndResampling();

                                if (outColor == 1)
                                {
                                  //resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), ImageProcessing.PixelsFormat.FormatBlackWhite, 1.0, 0, 0, new ImageProcessing.ColorD(150, 150, 150));
                                  resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4), ImageProcessing.PixelsFormat.FormatBlackWhite, 1.0, 0, 0);
                                }
                                else
                                {
                                  //resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Jpeg((byte)fileExportQuality), ImageProcessing.PixelsFormat.Format8bppGray, 1.0, 0, 0, new ImageProcessing.ColorD(150, 150, 150));
                                  resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Jpeg((byte)fileExportQuality), ImageProcessing.PixelsFormat.Format8bppGray, 1.0, 0, 0);
                                }
                            }

                        }


                        //make sure outColor is actual color of created file not just desired pdf color mode
                        if (outColor == 1)
                        {
                            imageColorMode = Scanners.ColorMode.Bitonal;
                        }
                        else
                        if (outColor == 8)
                        {
                            imageColorMode = Scanners.ColorMode.Grayscale;
                        }
                        else
                        {
                            imageColorMode = Scanners.ColorMode.Color;
                        }                   
                }
                else
                {
                   ocrImage = null;
                }
            }




            //modification - per Jirka Abbyy can handle big files so no need to scale down as it was needed for Iris..
            /*
                        using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(source.FullName))
                        {
                            string extension = (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite) ? "tif" : "jpg";
                            ocrImage = new FileInfo(string.Format("{0}\\{1}_ocr.{2}", destDir.FullName, System.IO.Path.GetFileNameWithoutExtension(source.Name), extension));

                            ocrImage.Refresh();
                            if (ocrImage.Exists)
                                ocrImage.Delete();

                            ocrImage.Refresh();

                            ImageProcessing.BigImages.ResizingAndResampling resizing = new ImageProcessing.BigImages.ResizingAndResampling();

                            if (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite)
                            {
                                if (itDecoder.DpiX <= 300)
                                {
                                    ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();
                                    copier.Copy(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.None));
                                }
                                else
                                {
                                    double zoom = 300.0 / itDecoder.DpiX;
                                    resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.None), ImageProcessing.PixelsFormat.FormatBlackWhite, zoom, 0, 0, new ImageProcessing.ColorD(150, 150, 150));
                                }
                            }
                            else
                            {
                                if (itDecoder.DpiX <= 200 && itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format8bppGray)
                                {
                                    ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();
                                    copier.Copy(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Jpeg(85));
                                }
                                else
                                {
                                    double zoom = 200.0 / itDecoder.DpiX;
                                    resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Jpeg(80), ImageProcessing.PixelsFormat.Format8bppGray, zoom, 0, 0, new ImageProcessing.ColorD(150, 150, 150));
                                }
                            }
                        }
            */

            if (ocrImage != null)
            {
                ocrImage.Refresh();
            }
			return ocrImage;
		}
		#endregion




        #region ReduceImageSizesIfNecessary()
        /// <summary>
        /// Function leaves bitonal images untouched.
        /// </summary>
        /// <param name="images"></param>
        /// <param name="maxFileSize"></param>
        /// 
/*
        #region ReduceImageSizesIfNecessaryAbbyy()
        private void ReduceImageSizesIfNecessaryAbbyy(List<Abbyy.SpdfStruct> images, double maxFileSize)
        {
            List<FileInfo> list = new List<FileInfo>();

            foreach (Abbyy.SpdfStruct img in images)
                list.Add(img.ImageToInsert);

            ReduceImageSizesIfNecessary(list, maxFileSize);
        }
        #endregion
        public void ReduceImageSizesIfNecessary(List<FileInfo> images, double maxFileSize)
		{
			try
			{
				BscanILL.IP.ImageFileSizeReducer reducer = new BscanILL.IP.ImageFileSizeReducer();

				reducer.DescriptionChanged += delegate(string description) { Description_Changed(description); };
				reducer.ProgressChanged += delegate(double progress) { Progress_Changed(progress); };
				reducer.ReduceImageSizesIfNecessary(images, maxFileSize);
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Error, "PdfsBuilder, ReduceImageSizesIfNecessary(): " + ex.Message, ex); 
			}
		}
*/
        private void ReduceImageSizesIfNecessary(List<Abbyy.SpdfStruct> images, double maxFileSize)
        {
            try
            {
                BscanILL.IP.ImageFileSizeReducer reducer = new BscanILL.IP.ImageFileSizeReducer();

                reducer.DescriptionChanged += delegate (string description) { Description_Changed(description); };
                reducer.ProgressChanged += delegate (double progress) { Progress_Changed(progress); };
                reducer.ReduceImageSizesIfNecessary(images, maxFileSize);
            }
            catch (Exception ex)
            {
                Notifications.Instance.Notify(this, Notifications.Type.Error, "PdfsBuilder, ReduceImageSizesIfNecessary(): " + ex.Message, ex);
            }
        }
        #endregion

        #region GetExportFilesSizeAbbyy()
        private ulong GetExportFilesSizeAbbyy(List<Abbyy.SpdfStruct> images)
        {
            ulong size = 0;

            foreach (Abbyy.SpdfStruct img in images)
            {
                if (img.ImageToInsert != null)
                {
                    img.ImageToInsert.Refresh();
                    size += (ulong)img.ImageToInsert.Length;
                }
                else
                {                    
                    size += (ulong)img.IllPage.SourceFileSize;
                }

                
            }

            return size;
        }
        #endregion

		#region GetExportFilePath
		private static FileInfo GetExportFilePath(BscanILL.Export.ExportUnit exportUnit)
		{
			DirectoryInfo dir = exportUnit.Directory;

			FileInfo file = BscanILL.Export.Misc.GetUniqueExportFilePath(exportUnit);

			file.Directory.Create();
			return file;
		}
		#endregion

        #region GetFilesAbbyy
        private static List<FileInfo> GetFilesAbbyy(List<Abbyy.SpdfStruct> spdfStructList)
        {
            List<FileInfo> files = new List<FileInfo>();

            foreach (Abbyy.SpdfStruct spdfStruct in spdfStructList)
                files.Add(spdfStruct.ImageToInsert);

            return files;
        }
        #endregion
		
        #region GetOcrFormatedDir()
        private DirectoryInfo GetOcrFormatedDir(BscanILL.Hierarchy.IllPage illPage, Scanners.ColorMode pdfExportColorDepth, int pdfExportQuality, ref Scanners.ColorMode imageColorMode, ref long imageFileSize)
        {
            int order = 0;
            FileInfo ocrFile = null;

            try
            {
                if (illPage.FilesStatus == IllPage.IllImageFilesStatus.Ready)
                {
                    lock (illPage.DeleteFilesLocker)
                    {                        
                        if (illPage.FilesStatus != IllPage.IllImageFilesStatus.Ready)
                            return null;

                        DirectoryInfo formatFileDir = new DirectoryInfo(illPage.FilePath.DirectoryName + "\\" + illPage.FilePath.Name + "_xml");
                        DirectoryInfo ocrFileDir = new DirectoryInfo(illPage.FilePath.DirectoryName);
                        order = 1;


                        if (formatFileDir.Exists)
                            formatFileDir.Delete(true);

                        formatFileDir.Create();

                        formatFileDir.Refresh();
                        
                        order = 2;

                        ocrFile = GetOcrImage(illPage, ocrFileDir, pdfExportColorDepth, pdfExportQuality, ref imageColorMode, false);

                        ocrFile.Refresh();
                        imageFileSize = ocrFile.Length;

                        order = 3;

#if DEBUG
                        //System.Windows.MessageBox.Show(order.ToString());
#endif

                        if(_settings.General.OcrEngProfile == BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Speed)
                        {
                            //Speed
                            this.Abbyy.OcrProfile = "Document Archiving Speed";     //Document Archiving is recommended profile for PDF creation. For RTF or PDF text only is recommended Document Conversion profile
                        
                        }
                        else
                        {
                            //Accuracy
                            this.Abbyy.OcrProfile = "Document Archiving Accuracy";
                        }
                        this.Abbyy.FormatImage(ocrFile, formatFileDir);

                        order = 4;

#if DEBUG
                        //System.Windows.MessageBox.Show(order.ToString());
#endif

                        if (formatFileDir != null)
                        {
                            formatFileDir.Refresh();
                            if (formatFileDir.Exists == false)
                                return null;
                        }

                        return formatFileDir;
                    }
                }
                else
                    return null;
            }
#if DEBUG
            catch (IllException ex)
            {
                System.Windows.MessageBox.Show("ILL Exception: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
                return null;
            }
#else
			catch (IllException)
			{
				return null;
			}
#endif

            catch (Exception ex)
            {
#if DEBUG
                System.Windows.MessageBox.Show("Exception: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
#endif

                Notifications.Instance.Notify(this, Notifications.Type.Error, "PdfBuilder, GetFormatedDir() " + order.ToString() + ": " + ex.Message, ex);
                return null;
            }
            finally
            {
                try
                {
                   if (ocrFile != null)
                   {
                      ocrFile.Refresh();
                      if (ocrFile.Exists)
                         ocrFile.Delete();
                   }                 
                }
                catch { }
            }
        }
        #endregion
	
		#endregion
	
	}
}
