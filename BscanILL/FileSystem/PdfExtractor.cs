
//#define ITEXTSHARP_v5

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using iTextSharp.text.pdf;

#if ITEXTSHARP_v5
 using iTextSharp.text.pdf.parser ;   
#endif

namespace BscanILL.FileSystem
{

#if ITEXTSHARP_v5
  
    class ImageRenderListener : IRenderListener
    {        
        iTextSharp.text.pdf.PdfReader pdfReaderListener = null;        
        PdfExtractor currentExtractor = null ;
        Bitmap bitmapPageListener = null;

        iTextSharp.text.Rectangle pageSizeInUnits = null;
        int rotate = 0;

        //CONSTRUCTOR
    #region constructor                
        public ImageRenderListener(PdfExtractor extractor, iTextSharp.text.pdf.PdfReader Reader)
        {
            this.currentExtractor = extractor;
            this.pdfReaderListener = Reader;
        }
    #endregion

        //PUBLIC PROPERTIES
    #region public properties

    #region BitmapPage
        public Bitmap BitmapPage
        {
            get { return this.bitmapPageListener; }
        }
    #endregion

    #endregion

        public void ClearBitmap()
        {
            if(bitmapPageListener != null)
            {
                bitmapPageListener.Dispose();
                bitmapPageListener = null;
            }            
        }
        
        public void SetOverallPageSizeInUnits(iTextSharp.text.Rectangle size)
        {
            this.pageSizeInUnits = size;
        }

        public void SetRotate(int rotateImg)
        {
            this.rotate = rotateImg;
        }                            

        public void BeginTextBlock() { }
        public void RenderText( TextRenderInfo renderInfo ) { }
        public void EndTextBlock() { }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            try
            {
                int number = 0 ;                
                int pageCounterTemp = 0;

                PdfImageObject image = renderInfo.GetImage();
                if( image == null ) return ;
                if (renderInfo.GetRef() != null)
                {
                    number = renderInfo.GetRef().Number;
                    PdfIndirectReference obj = renderInfo.GetRef();

                    Vector vector = renderInfo.GetStartPoint();

                    Matrix matrix = renderInfo.GetImageCTM();
                    float width = matrix[Matrix.I11];  //Width in Units
                    float height = matrix[Matrix.I22];  //Height in Units

                    iTextSharp.text.Rectangle imageSizeInUnits = new iTextSharp.text.Rectangle(0, 0, width, height);

                    float xOffset = vector[Vector.I1];  //[0]
                    float yOffset = vector[Vector.I2];  //[1]                

                    currentExtractor.ProcessObject(this.pdfReaderListener, obj, ref pageCounterTemp, ref bitmapPageListener, imageSizeInUnits, pageSizeInUnits, rotate, xOffset, yOffset);
                    
                }
            }
            catch (Exception e)
            {
                if (bitmapPageListener != null)
                {
                    bitmapPageListener.Dispose();
                    bitmapPageListener = null;
                }
                throw e;
            }
        }
    }
#endif

    class PdfExtractor
	{
		public delegate void ProgressChangedHnd(double progress);
		public event ProgressChangedHnd ProgressChanged;

        static protected BscanILL.SETTINGS.Settings _settings = BscanILL.SETTINGS.Settings.Instance;

		#region constructor
		public PdfExtractor()
		{
		}
		#endregion


		// PUBLIC METHODS
		#region public methods

#if ITEXTSHARP_v5


        #region ExtractImages_iText()
        /// <summary>
        /// http://itext-general.2136553.n4.nabble.com/Extracting-CCITTFaxDecode-images-from-a-PDF-td3770168.html
        ///
        /// http://www.jamesewelch.com/2008/11/14/how-to-extract-pages-from-a-pdf-document/
        ///
        ///********
        /// PDFReaderContentParser.ProcessContent()
        ///http://psycodedeveloper.wordpress.com/2013/01/10/how-to-extract-images-from-pdf-files-using-c-and-itextsharp/
        ///http://www.java2v.com/Open-Source/CSharp/PDF/iTextSharp/iTextSharp/text/pdf/parser/PdfReaderContentParser.cs.htm
        ///http://stackoverflow.com/questions/10125117/convert-pdf-file-pages-to-images-with-itextsharp
        ///********		
        /// </summary>
        /// <param name="pdfSourcePath"></param>
        /// <param name="destDir"></param>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        /// 
        public List<ImportedImage> ExtractImages(String pdfSourcePath, DirectoryInfo destDir)
		{
#if DEBUG
			DateTime start = DateTime.Now;
#endif
	
			int					pageCounter = 0;
			List<ImportedImage>	imageFiles = new List<ImportedImage>();

			iTextSharp.text.pdf.RandomAccessFileOrArray randomAccessFile = null;
			iTextSharp.text.pdf.PdfReader pdfReader = null;
#if ITEXTSHARP_v5
            iTextSharp.text.pdf.parser.PdfReaderContentParser parser = null;
#endif
 

            Bitmap bitmapPage = null;

			try
			{
				randomAccessFile = new iTextSharp.text.pdf.RandomAccessFileOrArray(pdfSourcePath);
				pdfReader = new iTextSharp.text.pdf.PdfReader(randomAccessFile, null);

#if ITEXTSHARP_v5
                parser = new iTextSharp.text.pdf.parser.PdfReaderContentParser(pdfReader);
                ImageRenderListener listener = new ImageRenderListener(this, pdfReader); 
#endif
                int pageCountInDocument = pdfReader.NumberOfPages;
				int imagesInTheDocument = GetImagesCountInDocument(pdfReader);

                        //new way of parcing - supports multiple images per page
                        // get reference to first object of each page and lay over bitmaps within same page
                        //We had pdf sample that carried one grayscale image and one bitonal image for each page - result was overlay of these two bitmaps per page
                        
                        for(int pageNumber = 1 ; pageNumber <= pageCountInDocument ; pageNumber++)
                        {
#if ITEXTSHARP_v5
                                                        listener.ClearBitmap();

                                                        iTextSharp.text.Rectangle pageSizeInUnits = pdfReader.GetPageSize(pageNumber);
                                                        int rotate = pdfReader.GetPageRotation(pageNumber);

                                                        listener.SetOverallPageSizeInUnits(pageSizeInUnits);
                                                        listener.SetRotate(rotate);

                                                        parser.ProcessContent(pageNumber, listener);

                                                        bitmapPage = listener.BitmapPage;

                                                        if (bitmapPage != null)
                                                        {                                
                                                            if (rotate > 0)
                                                            {
                                                                //need to apply pdf rotation (images in pdf file stored rotated..)
                                                                switch (rotate)
                                                                {
                                                                    case 90:
                                                                        bitmapPage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                                                        break;
                                                                    case 180:
                                                                        bitmapPage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                                                        break;
                                                                    case 270:
                                                                        bitmapPage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                                                        break;
                                                                }

                                                            }
                                

                                                            SaveBitmapToList(ref bitmapPage, pageCountInDocument, destDir, imageFiles);
                                                        }                            
#else

                            //code below is implementation without listener. it cannot work with image offset though that is why I've implemented the listener that supports GetStartingPoint method to get vector with image offset
          //solution without listener (implemented in iText version 5.0 and higher)
                            // so it works with older iText with lighter licensing rules
//---------------------------------------------------------------------

                            // image size in user units
                            pageCounter++;
                            
                            iTextSharp.text.Rectangle sizeInUnits = pdfReader.GetPageSize(pageCounter);
                            int rotate = pdfReader.GetPageRotation(pageCounter);

                            PdfDictionary pageObj  = pdfReader.GetPageN(pageNumber);
                            ProcessImagesInPDFDictionary(pdfReader, pageObj, ref pageCounter, ref bitmapPage ,sizeInUnits, sizeInUnits , rotate, 0 ,0 );

                            if (bitmapPage != null)
                            {
                                SaveBitmapToList( ref bitmapPage, pageCountInDocument, destDir, imageFiles);
                            }
#endif                            

                        }
#if ITEXTSHARP_v5
                        listener.ClearBitmap();
#endif

			}
			catch (Exception ex)
			{
#if DEBUG
				Console.WriteLine("Error: " + ex.Message);
#endif
				throw ex;
			}

            finally
            {                
                if( bitmapPage != null )
                {
                    bitmapPage.Dispose();
                    bitmapPage = null;
                }
                
                if( pdfReader != null)
                { 
                  pdfReader.Close();
                  pdfReader = null;                    
                }
                if (randomAccessFile != null )
                {
                  randomAccessFile.Close();
                  randomAccessFile = null;
                }
            }
#if DEBUG
			Console.WriteLine("PdfExtractor, ExtractImages() for " + pageCounter + " images: " + DateTime.Now.Subtract(start).ToString());
#endif
	
			return imageFiles;
		}
        #endregion


#else
        #region ExtractImages_GS()
        public List<ImportedImage> ExtractImages(String pdfSourcePath, DirectoryInfo destDir)
        {
            iTextSharp.text.pdf.RandomAccessFileOrArray randomAccessFile = null;
            iTextSharp.text.pdf.PdfReader pdfReader = null;

            Bitmap bitmapPage = null;
            List<ImportedImage> imageFiles = new List<ImportedImage>();
            string strippedPDFFileName = pdfSourcePath.Replace(" ", "");
            bool cleanUpTempFile = false;

            try
            {
                int xRes;
                int yRes;
                PixelFormat pixelFormat;
                int rotate ;
                int bitDepth ;
                int width ;
                int height ;
                randomAccessFile = new iTextSharp.text.pdf.RandomAccessFileOrArray(pdfSourcePath);
                pdfReader = new iTextSharp.text.pdf.PdfReader(randomAccessFile, null);

                int pageCountInDocument = pdfReader.NumberOfPages;
                int imagesInTheDocument = GetImagesCountInDocument(pdfReader);
                
                if (pdfSourcePath != strippedPDFFileName)
                {
                    //as folder does not consists of space in BscanILL installation - space in pdf name - GS does not support space in file name -> copy into tempfile name without a space
                    FileInfo tempSourceFileInfo = new FileInfo(pdfSourcePath);
                    FileInfo tempStrippedNameFileInfo = new FileInfo(strippedPDFFileName);

                    strippedPDFFileName = Path.Combine(BscanILL.SETTINGS.Settings.Instance.General.TempDir, tempStrippedNameFileInfo.Name);

                    FileInfo tempDelete = new FileInfo(strippedPDFFileName);
                    if (tempDelete.Exists)
                    {
                        tempDelete.Delete();
                    }

                    tempDelete.Directory.Create();
                    tempSourceFileInfo.CopyTo(strippedPDFFileName);
                    cleanUpTempFile = true;
                }
                else
                {
                    strippedPDFFileName = pdfSourcePath;
                }

                for (int pageNumber = 1; pageNumber <= pageCountInDocument; pageNumber++)
                {                    
                    //start with default params
                    xRes = _settings.General.PdfImportDpi;
                    yRes = _settings.General.PdfImportDpi;

                    pixelFormat = PixelFormat.Format1bppIndexed;
                    switch (_settings.General.PdfImportColor)
                    {
                        case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal: pixelFormat = PixelFormat.Format1bppIndexed; break;
                        case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale: pixelFormat = PixelFormat.Format8bppIndexed; break;
                        case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Color: pixelFormat = PixelFormat.Format24bppRgb; break;
                        default: pixelFormat = PixelFormat.Format1bppIndexed; break;
                    }

                    if( (_settings.General.ForceDefaultPdfParams == false) || ( _settings.General.PdfImportColor == BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto ) )
                    {                                                
                        iTextSharp.text.Rectangle sizeInUnits = pdfReader.GetPageSize(pageNumber);
                        rotate = pdfReader.GetPageRotation(pageNumber);
                        bitDepth = 0;
                        width = 0;
                        height = 0;

                        PdfDictionary pageObj = pdfReader.GetPageN(pageNumber);
                        ProcessImagesInPDFDictionaryGS(pdfReader, pageObj, null, sizeInUnits, ref bitDepth, ref width, ref height);

                        if (bitDepth > 0)
                        {
                            switch (bitDepth)
                            {
                                case 1: pixelFormat = PixelFormat.Format1bppIndexed; break;
                                //case 8: pixelFormat = PixelFormat.Format24bppRgb; break;
                                case 8: pixelFormat = PixelFormat.Format8bppIndexed; break;
                                case 24: pixelFormat = PixelFormat.Format24bppRgb; break;
                                //default: throw new Exception(String.Format("Unknown pixel format {0}.", bitDepth));
                            }
                        }
                     
                        if(_settings.General.ForceDefaultPdfParams == true)
                        {
                            xRes = _settings.General.PdfImportDpi;
                            yRes = _settings.General.PdfImportDpi;
                        }
                        else
                        {
                            //if rotate - swap width and height or resX and ressY
                            // formula to calculate dpi from size in user units and actual pixel size:
                            // dpi = ( 'pixels' * 72 ) / 'size in user units'

                            if (pageCountInDocument == imagesInTheDocument)
                            {
                                if ((rotate == 90) || (rotate == 270))
                                {
                                    xRes = (int)((float)height * (float)72 / sizeInUnits.Height);
                                    yRes = (int)((float)width * (float)72 / sizeInUnits.Width);
                                }
                                else
                                {
                                    xRes = (int)((float)width * (float)72 / sizeInUnits.Width);
                                    yRes = (int)((float)height * (float)72 / sizeInUnits.Height);
                                }
                            }  
                        }
                    }                                                                

                    string destBitmapPath = Path.Combine( BscanILL.SETTINGS.Settings.Instance.General.TempDir ,"dest.bmp");
                    FileInfo file = new FileInfo(destBitmapPath);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                    Directory.CreateDirectory(file.DirectoryName);
                   
                    bitmapPage = ExtractBitmapGS(strippedPDFFileName, destBitmapPath, (pageNumber - 1), xRes, yRes, pixelFormat);

                    if (bitmapPage != null)
                    {
                        SaveBitmapToList(ref bitmapPage, pageCountInDocument, destDir, imageFiles);
                    }
                    
                    file.Refresh();
                    if( file.Exists )
                    {
                        file.Delete();
                    }
                }

                if (pdfSourcePath != strippedPDFFileName)
                {

                }
            }

			catch (Exception ex)
			{
				throw ex;
			}

            finally
            {                
                if(cleanUpTempFile)
                {
                    //delete temp file
                    FileInfo tempFileStrippedName = new FileInfo(strippedPDFFileName);
                    tempFileStrippedName.Delete();
                }
       
                if( bitmapPage != null )
                {
                    bitmapPage.Dispose();
                    bitmapPage = null;
                }
                
                if( pdfReader != null)
                { 
                  pdfReader.Close();
                  pdfReader = null;                    
                }
                if (randomAccessFile != null )
                {
                  randomAccessFile.Close();
                  randomAccessFile = null;
                }
            }
	
			return imageFiles;
        }
        #endregion
#endif

        #region ProcessObjectGS
        public void ProcessObjectGS(PdfReader pdfReader, PdfObject obj, iTextSharp.text.Rectangle imageSizeInUnits, iTextSharp.text.Rectangle pageSizeInUnits, ref int bitDepth, ref int width, ref int height)
        {
            if (obj != null)
                if (obj.IsIndirect())
                {
                    PdfDictionary tag = (PdfDictionary)PdfReader.GetPdfObject(obj);
                    PdfName type = (PdfName)PdfReader.GetPdfObject(tag.Get(PdfName.SUBTYPE));

                    //image at the root of the pdf
                    if (PdfName.IMAGE.Equals(type))
                    {
                        //process image
                        int XrefIndex = ((PRIndirectReference)obj).Number;
                        PdfObject pdfObject = pdfReader.GetPdfObject(XrefIndex);

                        if ((pdfObject != null) && pdfObject.IsStream())
                        {
                            DecodePdfImageForParams(pdfReader , pdfObject, imageSizeInUnits, pageSizeInUnits, ref bitDepth, ref width, ref height); 
                        }
                    }
                    //image inside a form
                    else if (PdfName.FORM.Equals(type))
                    {
                        ProcessImagesInPDFDictionaryGS(pdfReader, tag, imageSizeInUnits, pageSizeInUnits, ref bitDepth, ref width, ref height);
                    }
                    //image inside a group
                    else if (PdfName.GROUP.Equals(type))
                    {
                        ProcessImagesInPDFDictionaryGS(pdfReader, tag, imageSizeInUnits, pageSizeInUnits, ref bitDepth, ref width, ref height);
                    }
                }
        }
        #endregion

        #region ProcessObject
        public void ProcessObject(PdfReader pdfReader, PdfObject obj, ref int pageCounter, ref Bitmap bitmapPage, iTextSharp.text.Rectangle imageSizeInUnits, iTextSharp.text.Rectangle pageSizeInUnits,  int rotate, float xOffset, float yOffset)
        {
            if (obj != null)
                if (obj.IsIndirect())
                {
                    PdfDictionary tag = (PdfDictionary)PdfReader.GetPdfObject(obj);
                    PdfName type = (PdfName)PdfReader.GetPdfObject(tag.Get(PdfName.SUBTYPE));

                    //image at the root of the pdf
                    if (PdfName.IMAGE.Equals(type))
                    {
                        //process image
                        int XrefIndex = ((PRIndirectReference)obj).Number;
                        PdfObject pdfObject = pdfReader.GetPdfObject(XrefIndex);

                        if ((pdfObject != null) && pdfObject.IsStream())
                        {                            
                            DecodePdfImageToBitmap(pdfReader, pdfObject, ref pageCounter, false, ref bitmapPage, imageSizeInUnits, pageSizeInUnits, rotate, xOffset, yOffset);                            
                        }
                    }
                    //image inside a form
                    else if (PdfName.FORM.Equals(type))
                    {
                        ProcessImagesInPDFDictionary(pdfReader, tag, ref pageCounter, ref bitmapPage, imageSizeInUnits, pageSizeInUnits, rotate, xOffset, yOffset);
                    }
                    //image inside a group
                    else if (PdfName.GROUP.Equals(type))
                    {
                        ProcessImagesInPDFDictionary(pdfReader, tag, ref pageCounter, ref bitmapPage, imageSizeInUnits, pageSizeInUnits, rotate, xOffset, yOffset);
                    }
                }
        }
        #endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

        #region InvertBitonalBitmap
        private static void InvertBitonalBitmap( ref Bitmap bitmap )
        {
            BitmapData bitmapData = null;

            try
            {
                bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                int resultStride = bitmapData.Stride;                

                unsafe
                {
                    byte* scan0 = (byte*)bitmapData.Scan0.ToPointer();
                    byte* pCurrent;
                                       
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        pCurrent = scan0 + y * resultStride;

                        for (int x = 0; x < resultStride; x++)
                        {                                
                            pCurrent[x] ^= 0xff;
                        }
                    }                    
                }

            }
            finally
            {
                if (bitmapData != null)
                    bitmap.UnlockBits(bitmapData);
            }           
        }
        #endregion

        #region ExtractBitmapGS
        private Bitmap ExtractBitmapGS(string sourcePDFPath, string destBitmpaPath, int pageIndex, int resolutionX, int resolutionY, PixelFormat pixelFormat)
        {
            string colorDepth ;
            string gsAppPath = Path.Combine( BscanILL.SETTINGS.Settings.Instance.General.GsDir , "pdf2bmp.exe");  //"C:\\bs\\gs\\pdf2bmp.exe";
            Bitmap bitmap = null ;

            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed: colorDepth = "bmpmono"; break;
                case PixelFormat.Format8bppIndexed: colorDepth = "bmpgray"; break;
                case PixelFormat.Format24bppRgb: colorDepth = "bmp16m"; break;
                default: throw new Exception(String.Format("Unknown pixel format {0}.", pixelFormat));
            }

            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                // resolutionX = 200;
                // resolutionY = 200;
                // pageIndex = 4;                
                // sourcePDFPath = "C:\\temp\\currenttest.pdf";                
                // destBitmpaPath = "C:\\temp\\dest.bmp";                

                //--- to report page count in TXT file ---
                // string desTextPath = "C:\\temp\\dest.txt";              
                // string argument = string.Format("/c:count /s:{0} /d:{1}", sourcePDFPath, desTextPath);

                //--- to export one page from pdf into bitmap ---
                string argument = string.Format("/c:print /s:{0} /d:{1} /p:{2} /f:{3} /rx:{4} /ry:{5}", sourcePDFPath, destBitmpaPath, pageIndex, colorDepth, resolutionX, resolutionY);

                pProcess.StartInfo.FileName = gsAppPath;
                pProcess.StartInfo.Arguments = argument;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.Start();
                //System.Threading.Thread.Sleep(6000);
                //  do{
                //  if( ! pProcess.HasExited )
                //  {                
                //  }
                //  } while( ! pProcess.WaitForExit( 1000 ) ) ;
                pProcess.WaitForExit();
                bool returnCode = (pProcess.ExitCode == 0) ? true : false;
                if( returnCode )
                {
                    bitmap = new Bitmap( destBitmpaPath ) ;
                    if( bitmap.PixelFormat == PixelFormat.Format1bppIndexed )
                    {
                        //bitonal image comes inverted in BscanILL view window so we have to invert bits here to fix it.
                        InvertBitonalBitmap( ref bitmap );
                    }
                }
                else
                {
                    throw new Exception(String.Format("Unable to extract {0}. page in {1}.", ( pageIndex + 1 ) , sourcePDFPath));
                }
            }
            return bitmap ;
        }
        #endregion

        #region SaveBitmapToList()
        private void SaveBitmapToList(ref Bitmap bitmapPage, int totalDocuments, DirectoryInfo destDir, List<ImportedImage> imageFiles)       
        {
            if (bitmapPage != null)
            {
                //release to page level image
                if (bitmapPage.PixelFormat == PixelFormat.Format1bppIndexed)
                {
                    string filePath = GetUniqueFile(destDir, ImageFormat.Tiff);
                    SaveAsTiff(filePath, bitmapPage, EncoderValue.CompressionCCITT4);                    
                    imageFiles.Add(new ImportedImage(filePath, Scanners.ColorMode.Bitonal, Scanners.FileFormat.Tiff, Convert.ToUInt16(bitmapPage.HorizontalResolution), true));
                }
                else
                {
                    string filePath = GetUniqueFile(destDir, ImageFormat.Jpeg);
                    SaveAsJpeg(filePath, bitmapPage, bitmapPage.PixelFormat);

                    Scanners.ColorMode colorMode = (bitmapPage.PixelFormat == PixelFormat.Format8bppIndexed) ? Scanners.ColorMode.Grayscale : Scanners.ColorMode.Color;

                    imageFiles.Add(new ImportedImage(filePath, colorMode, Scanners.FileFormat.Jpeg, Convert.ToUInt16(bitmapPage.HorizontalResolution), true));
                }

                RaiseProgressChanged(imageFiles.Count / (double)totalDocuments);

                bitmapPage.Dispose();
                bitmapPage = null;
            }
        }
        #endregion

        #region ProcessImagesInPDFDictionaryGS()        
        private void ProcessImagesInPDFDictionaryGS(PdfReader pdfReader, PdfDictionary pageObj, iTextSharp.text.Rectangle imageSizeInUnits, iTextSharp.text.Rectangle pageSizeInUnits, ref int bitDepth , ref int width, ref int height)
        {
            PdfDictionary resources = (PdfDictionary)PdfReader.GetPdfObject(pageObj.Get(PdfName.RESOURCES));
            PdfDictionary xobj = (PdfDictionary)PdfReader.GetPdfObject(resources.Get(PdfName.XOBJECT));

            if (xobj != null)
            {
                foreach (PdfName name in xobj.Keys)
                {
                    PdfObject obj = xobj.Get(name);
                    ProcessObjectGS(pdfReader, obj, imageSizeInUnits, pageSizeInUnits, ref bitDepth , ref width, ref height);
                }
            }
        }
        #endregion

        #region ProcessImagesInPDFDictionary()
        private void ProcessImagesInPDFDictionary(PdfReader pdfReader, PdfDictionary pageObj, ref int pageCounter, ref Bitmap bitmapPage, iTextSharp.text.Rectangle imageSizeInUnits, iTextSharp.text.Rectangle pageSizeInUnits, int rotate, float xOffset, float yOffset)
        {
            PdfDictionary resources = (PdfDictionary)PdfReader.GetPdfObject(pageObj.Get(PdfName.RESOURCES));
            PdfDictionary xobj = (PdfDictionary)PdfReader.GetPdfObject(resources.Get(PdfName.XOBJECT));

            if (xobj != null)
            {
                foreach (PdfName name in xobj.Keys)
                {
                    PdfObject obj = xobj.Get(name);
                    ProcessObject(pdfReader, obj, ref pageCounter, ref bitmapPage, imageSizeInUnits, pageSizeInUnits, rotate, xOffset, yOffset);
                }
            }
        }
        #endregion

        #region DecodePdfImageForParams()                
        private void DecodePdfImageForParams( PdfReader pdfReader , PdfObject pdfObject, iTextSharp.text.Rectangle imageSizeInUnits, iTextSharp.text.Rectangle pageSizeInUnits, ref int bitDepthOut, ref int widthOut, ref int heightOut )           
        {
            if ((pdfObject != null) && pdfObject.IsStream())
            {                

                iTextSharp.text.pdf.PdfStream pdfStream = (iTextSharp.text.pdf.PdfStream)pdfObject;

                PdfDictionary pdfDictionary = (PdfDictionary)pdfObject;
                List<PdfName> filterArray = GetFilterArray(pdfDictionary);
                Bitmap bitmap = null;

                try
                {
                    //image size in pixels
                    int width = ((PdfNumber)pdfDictionary.Get(PdfName.WIDTH)).IntValue;
                    int height = ((PdfNumber)pdfDictionary.Get(PdfName.HEIGHT)).IntValue;

                    //pdfDictionary.Get(PdfName.LOCATION)
                    int bpp = pdfDictionary.GetAsNumber(PdfName.BITSPERCOMPONENT).IntValue;

                    //if(bpp == 8)
                    if( (bpp == 8) || (bpp == 16))
                    {                        
                        var colorSpace = pdfDictionary.Get(PdfName.COLORSPACE);
                        if (colorSpace != null)
                        {                            
                            if (!colorSpace.IsArray())
                            {
                                if (colorSpace.IsName()) 
                                {                                    
                                    if (colorSpace.Equals(PdfName.DEVICEGRAY))
                                    {
                                        //bpp = 8;
                                    }
                                    else
                                        if (colorSpace.Equals(PdfName.DEVICERGB))
                                        {
                                            bpp = 24;
                                        }
                                }
                                else
                                if (colorSpace.IsIndirect())                                    
                                {
                                    int XrefIndex = ((PRIndirectReference)colorSpace).Number;
                                    PdfObject pdfObjectColor = pdfReader.GetPdfObject(XrefIndex);
                                    if (pdfObjectColor.IsArray())
                                    {
                                        PdfArray ca = (PdfArray)pdfObjectColor;
                                        PdfObject tyca = ca.GetDirectObject(0);
                                        if (PdfName.DEVICEGRAY.Equals(tyca))
                                        {
                                            //bpp = 8;
                                        }
                                        else
                                        if (PdfName.DEVICERGB.Equals(tyca))
                                        {
                                           bpp = 24;
                                        }
                                        else
                                        if(PdfName.ICCBASED.Equals(tyca))
                                        {
                                            PRStream pr = (PRStream)ca.GetDirectObject(1);
                                            int n = pr.GetAsNumber(PdfName.N).IntValue;
                                            if(n == 1)
                                            {
                                              //bpp = 8
                                            }
                                            else
                                            if (n == 3)
                                            {
                                              bpp = 24;
                                            }
                                            else
                                            if (n == 4)   //sample I had for testing was color and reported this value = 4 ...
                                            {
                                              bpp = 24;
                                            }                                                        
                                        }
                                    }                                                
                                }                                            

                            }
                        }
                    }

                    if ( (filterArray.Count > 0) && PdfName.DCTDECODE.Equals(filterArray[0])) 
                    {
                        //test pixel format off bitmap with this decode. Becasue we do not test Colorspace Array above and we do not check when colorspace type ICCBased
                        // we need to get bitmap and check pixelformat property of the bitmap
                        
                        // case "/DCTDecode":  //jpeg compression
                        byte[] byteArray = iTextSharp.text.pdf.PdfReader.GetStreamBytesRaw((iTextSharp.text.pdf.PRStream)pdfStream);
                        System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(byteArray);

                        memoryStream.Position = 0;
                        bitmap = (Bitmap)System.Drawing.Image.FromStream(memoryStream);

                        //temp = ((float)(width * 72) / imageSizeInUnits.Width);
                        //int xRes = (int)temp;

                        //temp = ((float)(height * 72) / imageSizeInUnits.Height);
                        //int yRes = (int)temp;

                        //bitmap.SetResolution(xRes, yRes);
                        if(bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
                        {
                            bpp = 1;
                        }
                        else
                            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                            {
                                bpp = 8;
                            }
                            else
                                if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
                                {
                                    bpp = 24;
                                }
                    }    
#if ITEXTSHARP_v5                
                    else if ( (filterArray.Count > 0) && ( (PdfName.FLATEDECODE.Equals(filterArray[0])) || (PdfName.FL.Equals(filterArray[0])) ) )
#else
                    else if (  (filterArray.Count > 0) && PdfName.FLATEDECODE.Equals(filterArray[0])) 
#endif
                            {
                                // case "/FlateDecode":
                                // for example it is case if uncompressed color tiff was used to be wrapped with wrapper into pdf...
                                // in case of Tiff images this method returns uncompressed data in byte boundary

                                //GetStreamBytes should work fine for FLATEDECODE after reviewing PdfReader.cs from iTextSharp project but for now I'll code it manually as it is in PdfReader.cs (DecodeBytes method)
                                //byte[] pdfImageBuffer = PdfReader.GetStreamBytes((iTextSharp.text.pdf.PRStream)pdfStream);

                                

                                if (filterArray.Count > 1)
                                if (PdfName.DCTDECODE.Equals(filterArray[1]))
                                {
                                    byte[] pdfImageBuffer = PdfReader.FlateDecode(PdfReader.GetStreamBytesRaw((PRStream)pdfStream), true);                                                                    

                                    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(pdfImageBuffer);
                                    memoryStream.Position = 0;
                                    bitmap = (Bitmap)System.Drawing.Image.FromStream(memoryStream);

                                   // temp = ((float)(width * 72) / imageSizeInUnits.Width);
                                   // int xRes = (int)temp;
                                   // temp = ((float)(height * 72) / imageSizeInUnits.Height);
                                   // int yRes = (int)temp;
                                   // bitmap.SetResolution(xRes, yRes);                                

                                    if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
                                    {
                                        bpp = 1;
                                    }
                                    else
                                        if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                                        {
                                            bpp = 8;
                                        }
                                        else
                                            if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
                                            {
                                                bpp = 24;
                                            }
                                }
                            }

                    if (bitDepthOut < bpp)
                    {
                        bitDepthOut = bpp;
                    }

                    if (widthOut < width)
                    {
                        widthOut = width;
                    }
                    if (heightOut < height)
                    {
                        heightOut = height;
                    }
                }
                finally
                {                   
                        if (bitmap != null)       
                        {
                            bitmap.Dispose();
                            bitmap = null;
                        }
                }
            }
        }
        #endregion

        #region DecodePdfImageToBitmap()
        private void DecodePdfImageToBitmap(PdfReader pdfReader, PdfObject pdfObject, ref int pageCounter, bool incrementPageCounter, ref Bitmap bitmapPage, iTextSharp.text.Rectangle imageSizeInUnits, iTextSharp.text.Rectangle pageSizeInUnits, int rotate, float xOffset, float yOffset)
        {
            if ((pdfObject != null) && pdfObject.IsStream())
            {
                    PixelFormat pixelFormat;
                    Bitmap bitmap = null;

                    iTextSharp.text.pdf.PdfStream pdfStream = (iTextSharp.text.pdf.PdfStream)pdfObject;

                    PdfDictionary pdfDictionary = (PdfDictionary)pdfObject;                    
                    List<PdfName> filterArray = GetFilterArray(pdfDictionary);

                    try
                    {
                      if (filterArray.Count > 0 )
                      {                    	
                        if (PdfName.CCITTFAXDECODE.Equals(filterArray[0]) || PdfName.JBIG2DECODE.Equals(filterArray[0]) || PdfName.DCTDECODE.Equals(filterArray[0]) || PdfName.FLATEDECODE.Equals(filterArray[0]))                        
                        {
                            if (incrementPageCounter)
                            {
                                pageCounter++;
                            }

                            //image size in pixels
                            int width = ((PdfNumber)pdfDictionary.Get(PdfName.WIDTH)).IntValue;
                            int height = ((PdfNumber)pdfDictionary.Get(PdfName.HEIGHT)).IntValue;

                            bool imageMask = false;   //overlay flag??
                            
                            if (pdfDictionary.Get(PdfName.IMAGEMASK) != null)
                            {
                                imageMask = ((PdfBoolean)pdfDictionary.Get(PdfName.IMAGEMASK)).BooleanValue;                                
                            }

                            //pdfDictionary.Get(PdfName.LOCATION)
                            int bpp = pdfDictionary.GetAsNumber(PdfName.BITSPERCOMPONENT).IntValue;

                            if ((bpp == 8) || (bpp == 16))
                            {
                                var colorSpace = pdfDictionary.Get(PdfName.COLORSPACE);
                                if (colorSpace != null )
                                {
                                    if ( ! colorSpace.IsArray() )
                                    {
                                        if( colorSpace.IsName() ) 
                                        {
                                            if( colorSpace.Equals(PdfName.DEVICEGRAY))
                                            {
                                            //bpp = 8;
                                            }
                                            else
                                            if (colorSpace.Equals(PdfName.DEVICERGB))
                                            {
                                                bpp = 24;
                                            }
                                        }
															 else
                                            if (colorSpace.IsIndirect())
                                            {
                                                int XrefIndex = ((PRIndirectReference)colorSpace).Number;
                                                PdfObject pdfObjectColor = pdfReader.GetPdfObject(XrefIndex);
                                                if (pdfObjectColor.IsArray())
                                                {
                                                    PdfArray ca = (PdfArray)pdfObjectColor;
                                                    PdfObject tyca = ca.GetDirectObject(0);
                                                    if (PdfName.DEVICEGRAY.Equals(tyca))
                                                    {
                                                        //bpp = 8;
                                                    }
                                                    else
                                                        if (PdfName.DEVICERGB.Equals(tyca))
                                                        {
                                                            bpp = 24;
                                                        }
                                                        else
                                                            if (PdfName.ICCBASED.Equals(tyca))
                                                            {
                                                                PRStream pr = (PRStream)ca.GetDirectObject(1);
                                                                int n = pr.GetAsNumber(PdfName.N).IntValue;
                                                                if (n == 1)
                                                                {
                                                                    //bpp = 8
                                                                }
                                                                else
                                                                    if (n == 3)
                                                                    {
                                                                        bpp = 24;
                                                                    }
                                                                    else
                                                                        if (n == 4)   //sample I had for testing was color and reported this value = 4 ...
                                                                        {
                                                                            bpp = 24;
                                                                        }
                                                            }
                                                }
                                            }                                        

                                    }
                                }
                            }
                            /*string colorSpace = (pdfDictionary.Get(PdfName.COLORSPACE).IsNull() == false) ? pdfDictionary.Get(PdfName.COLORSPACE).ToString() : "";
                            // colorspace - DeviceGray, DeviceRGB                            
                            string unit = (pdfDictionary.Get(PdfName.USERUNIT) != null) ? pdfDictionary.Get(PdfName.USERUNIT).ToString() : "";
                            string bps = (pdfDictionary.Get(PdfName.BITSPERSAMPLE) != null) ? pdfDictionary.Get(PdfName.BITSPERSAMPLE).ToString() : "";
                            string resolution = (pdfDictionary.Get(PdfName.DR) != null) ? pdfDictionary.Get(PdfName.DR).ToString() : "";*/

                            switch (bpp)
                            {
                                case 1: pixelFormat = PixelFormat.Format1bppIndexed; break;
                                //case 8: pixelFormat = PixelFormat.Format24bppRgb; break;
                                case 8: pixelFormat = PixelFormat.Format8bppIndexed; break;
                                case 24: pixelFormat = PixelFormat.Format24bppRgb; break;
                                default: throw new Exception(String.Format("Unknown pixel format {0}.", bpp));
                            }

                                                        List<PdfObject> dp = new List<PdfObject>();
                                                        PdfObject dpo = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.DECODEPARMS));
                                                        if( dpo == null || (!dpo.IsDictionary() && !dpo.IsArray()) )
                                                        {
                                                            dpo = PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.DP));
                                                        }
                                                        if (dpo != null)
                                                        {
                                                            if (dpo.IsDictionary())
                                                            {
                                                                dp.Add(dpo);
                                                            }
                                                            else if (dpo.IsArray())
                                                            {
#if ITEXTSHARP_v5
                                                                dp = ((PdfArray)dpo).ArrayList;  
#else

                                                                  PdfArray listArray = (PdfArray)dpo;                           //code below substitutes commented line above
                                                                  for (int arrayIndex = 0; arrayIndex < listArray.Size; arrayIndex++)
                                                                  {
                                                                      PdfObject arrayItem = listArray[arrayIndex] as PdfObject;
                                                                      if (arrayItem != null)
                                                                      {
                                                                          dp.Add(arrayItem);
                                                                      }
                                                                  }
#endif
                                                            }
                                                        }
                           		

                            //---------------------------------------------------------------
                            //TO DO!! - current code support just one decompression method for image and one array filter combination/array: FLATEDECODE + DCTDECODE
                            //it would be better to always store filters into array and intterate through filtes and apply specify decompressions
                            // base on review of DecodeBytes() method in PdfReader.cs from iText source code, method GetStramBytes() loops through array of filters specified
                            // this method supports these filters FLATEDECODE (FL), ASCIIHEXDECODE, ASCII85DECODE,LZWDECODE,CCITTFAXDECODE,

                            // looks like just DCTDECODE (for jpegs for example),JBIG2DECODE are not supported by GetStramBytes() so just one filter is applied in my code wehn this fillter is specified as first filter

                            // because of DCTDECODE not supported by GetStreamBytes() and becasue I have sample that uses FLATEDECODE and DCTDECODE in filter array, I coded FLATEDECODE not to use GetStreamBytes()
                            // becasue the second filter DCTDECODE would not be applied

                            //---------------------------------------------------------------

                            //if (PdfName.ASCIIHEXDECODE.Equals(filterArray[0]))
                            //if (PdfName.ASCII85DECODE.Equals(filterArray[0]))
                            //if (PdfName.LZWDECODE.Equals(filterArray[0]))
                            //if (PdfName.RUNLENGTHDECODE.Equals(filterArray[0]))   // works with 8bit data samples
                            //if (PdfName.JPXDECODE.Equals(filterArray[0]))        //when this decoding, BITSPERCOMPONENT is determined from image data not from not from image dictionary (bpp above will not be set correctly) and colorspace may or may not be specified in dictionary
                            //if (PdfName.CRYPT.Equals(filterArray[0]))
                            
                            if (PdfName.CCITTFAXDECODE.Equals(filterArray[0]))                            
                            {
                                // !!!! seems based on review of DecodeBytes() method in PDFReader class in iTextSharp, GetStreamBytes returns data where 1 means white
                                // !!!! There is code in it, if black is zero then invert all bits in output data so I believe I do not need to check the meaninbg of 0 or 1
                                // !!!!  so I believe there is no need to verify PdfName.BLACKIS1 as it is commented out in my code below.

                                /*
                                  //here is a snippet from code in DecodeBytes() method:
                                  
                                                                if (!blackIs1)
                                                                {
                                                                    //invert pixels
                                                                    int len = pdfImageBuffer.Length;
                                                                    for (int t = 0; t < len; ++t )
                                                                    {
                                                                        pdfImageBuffer[t] ^= 0xff;
                                                                    }
                                                                }
                                */


                                //bool blackIs1 = false;

                                //find out what type of compression
                                int nK = 0;//Default to 0 like PDF Spec
                                
                                PdfObject oDecodeParams = pdfDictionary.Get(PdfName.DECODEPARMS);
                                if (oDecodeParams is PdfDictionary)
                                {
                                    PdfNumber oK0 = ((PdfDictionary)oDecodeParams).GetAsNumber(PdfName.K);
                                    if( oK0 != null)
                                    {
                                        nK = oK0.IntValue;
                                    }

                                    //PdfBoolean bo = ((PdfDictionary)oDecodeParams).GetAsBoolean(PdfName.BLACKIS1);
                                    //if(bo != null)
                                    //{
                                    //    blackIs1 = bo.BooleanValue;
                                    //}
                                }
                                //nK = 0 -> G3   Compression.CCITTFAX3
                                //nK > 0 -> G3 2D (1)
                                //nK < 0 -> G4   Compression.CCITTFAX4 (-1)
                                          
                                // in case of bitonal Tiff-G4 images this method returns uncompressed data in byte boundary
                                byte[] pdfImageBuffer = PdfReader.GetStreamBytes((iTextSharp.text.pdf.PRStream)pdfStream);

                                bitmap = GetBitmapFromBuffer(pdfImageBuffer, imageSizeInUnits, width, height, pixelFormat, false);                                                                 
                            }
                            else if (PdfName.DCTDECODE.Equals(filterArray[0]))
                            {
                                float temp;
                                // case "/DCTDecode":  //jpeg compression
                                byte[] byteArray = iTextSharp.text.pdf.PdfReader.GetStreamBytesRaw((iTextSharp.text.pdf.PRStream)pdfStream);
                                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(byteArray);

                                memoryStream.Position = 0;
                                bitmap = (Bitmap)System.Drawing.Image.FromStream(memoryStream);

                                temp = ((float)(width * 72) / imageSizeInUnits.Width);
                                int xRes = (int)temp;

                                temp = ((float)(height * 72) / imageSizeInUnits.Height);
                                int yRes = (int) temp;
                      
                                bitmap.SetResolution(xRes, yRes);
                            }
                            else if (PdfName.JBIG2DECODE.Equals(filterArray[0]))  //bitonal compression
                            {
                                byte[] byteArray = iTextSharp.text.pdf.PdfReader.GetStreamBytesRaw((iTextSharp.text.pdf.PRStream)pdfStream);
                                var jbg2 = new org.jpedal.jbig2.JBIG2Decoder();                                

                                //Some JBig2 will extract witout setting the JBig2Globals
                                var dedodeParams = pdfStream.GetAsDict(PdfName.DECODEPARMS);
                                if (dedodeParams != null)
                                {
                                    var globalRef = dedodeParams.GetAsIndirectObject(PdfName.JBIG2GLOBALS);
                                    if (globalRef != null)
                                    {                                        
                                        var globals = PdfReader.GetPdfObject(globalRef);
                                        var globalStream = globals as PRStream;
                                        var globalBytes = PdfReader.GetStreamBytesRaw(globalStream);

                                        if (globalBytes != null)
                                        {
                                            jbg2.setGlobalData(globalBytes);
                                        }

                                    }
                                }
                            
                                jbg2.decodeJBIG2(byteArray);

                                var pages = jbg2.getNumberOfPages();

                                for (int p = 0; p < pages; p++)
                                {                                    
                                    org.jpedal.jbig2.image.JBIG2Bitmap jbig2Bitmap = jbg2.getPageAsJBIG2Bitmap(p);

                                    int height2 = jbig2Bitmap.getHeight();  //height of bitmap
                                    int width2 = jbig2Bitmap.getWidth(); //pixel width                                    
                                    
                                    byte[] pdfImageBuffer = jbig2Bitmap.getData(true);
                                    bitmap = GetBitmapFromBuffer(pdfImageBuffer, imageSizeInUnits, width, height, pixelFormat, false);
                                }
                            }
                            else if ( (PdfName.ASCIIHEXDECODE.Equals(filterArray[0])) || (PdfName.ASCII85DECODE.Equals(filterArray[0])) || (PdfName.LZWDECODE.Equals(filterArray[0])))
                            {
                                byte[] pdfImageBuffer = PdfReader.GetStreamBytes((iTextSharp.text.pdf.PRStream)pdfStream);

                                bitmap = GetBitmapFromBuffer(pdfImageBuffer, imageSizeInUnits, width, height, pixelFormat, false);
                            }
#if ITEXTSHARP_v5
                            else if ((PdfName.FLATEDECODE.Equals(filterArray[0])) || (PdfName.FL.Equals(filterArray[0]))) 
#else
                            else if (PdfName.FLATEDECODE.Equals(filterArray[0])) 
#endif
                            {
                                // case "/FlateDecode":
                                // for example it is case if uncompressed color tiff was used to be wrapped with wrapper into pdf...
                                // in case of Tiff images this method returns uncompressed data in byte boundary

                                //GetStreamBytes should work fine for FLATEDECODE after reviewing PdfReader.cs from iTextSharp project but for now I'll code it manually as it is in PdfReader.cs (DecodeBytes method)
                                //byte[] pdfImageBuffer = PdfReader.GetStreamBytes((iTextSharp.text.pdf.PRStream)pdfStream);

                                byte[] pdfImageBuffer = PdfReader.FlateDecode(PdfReader.GetStreamBytesRaw((PRStream)pdfStream), true);
                                
                                //  consider /Decode key to know how to interpret the color !!! ...
                                PdfObject dicParam = null;
                                if( 0 < dp.Count )  //if decodeparams for filterArray[0] are available
                                {
                                    dicParam = (PdfObject)dp[0];                                    
                                    pdfImageBuffer = PdfReader.DecodePredictor(pdfImageBuffer, dicParam);
                                }

                                if (filterArray.Count == 1)
                                {
/*
                                    bool blackIs1 = false;
                                    var dedodeParams = pdfStream.GetAsDict(PdfName.DECODEPARMS);
                                    if (dedodeParams != null)
                                    {
                                        if (dedodeParams is PdfDictionary)
                                        {
                                            PdfBoolean bo = ((PdfDictionary)dedodeParams).GetAsBoolean(PdfName.BLACKIS1);
                                            if (bo != null)
                                            {
                                                blackIs1 = bo.BooleanValue;
                                            }
                                        }
                                    }
*/

                                    bitmap = GetBitmapFromBuffer(pdfImageBuffer, imageSizeInUnits, width, height, pixelFormat, true);
                                }
                                else
                                {
                                    if (PdfName.DCTDECODE.Equals(filterArray[1]))
                                    {
                                        float temp;

                                        System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(pdfImageBuffer);
                                        memoryStream.Position = 0;
                                        bitmap = (Bitmap)System.Drawing.Image.FromStream(memoryStream);

                                        temp = ((float)(width * 72) / imageSizeInUnits.Width);
                                        int xRes = (int)temp;
                                        temp = ((float)(height * 72) / imageSizeInUnits.Height);
                                        int yRes = (int)temp;

                                        bitmap.SetResolution(xRes, yRes);
                                    }
                                }
                            }

                            if (bitmap != null)
                            {
                                int iX = 0;
                                int iY = 0;  //float xOffset, float yOffset

                                //copy/overlay bitmaps
                                if (bitmapPage == null)
                                {
                                    //convert offsets to pixels
                                    if ((xOffset < -5.0) || (xOffset > 2.0))
                                    //if (xOffset != 0)
                                    {
                                        //convert X offset to pixels
                                        // for some reason a testing pdf where images should not be shifted report in vector values in range <-1.8,0.47> so I am ignoring the offset if value is too small in range<-5,2>..
                                        iX = (int)((float)((xOffset * (float)bitmap.HorizontalResolution)) / 72);
                                    }
                                    if ((yOffset < -5.0) || (yOffset > 2.0))
                                    //if (yOffset != 0)
                                    {
                                        //convert X offset to pixels
                                        // for some reason a testing pdf where images should not be shifted report in vector values in range <-1.8,0.47> so I am ignoring the offset if value is too small in range<-5,2>..
                                        iY = (int)((float)((yOffset * (float)bitmap.VerticalResolution)) / 72);
                                    }

                                    int maxPageWidth = (int)((float)((pageSizeInUnits.Width * (float)bitmap.HorizontalResolution)) / 72);
                                    int maxPageHeight = (int)((float)((pageSizeInUnits.Height * (float)bitmap.VerticalResolution)) / 72);

                                    BitmapPaste(ref bitmapPage, bitmap, iX, iY, maxPageWidth, maxPageHeight, bitmap.HorizontalResolution, bitmap.VerticalResolution, false);
                                }
                                else
                                {
                                    //rescale
                                    if ((bitmapPage.HorizontalResolution != bitmap.HorizontalResolution) || (bitmapPage.VerticalResolution != bitmap.VerticalResolution))
                                    {                                        
                                        long bitmapPageNewWidth = bitmapPage.Width ;
                                        long bitmapPageNewHeight = bitmapPage.Height;
                                        long bitmapNewWidth = bitmap.Width;
                                        long bitmapNewHeight = bitmap.Height;
                                        float outHorizResolution = bitmapPage.HorizontalResolution;
                                        float outVertResolution = bitmapPage.VerticalResolution;

                                        //convert bitmaps to same resolution so we can logically or bits to overlay them
                                        if(bitmapPage.HorizontalResolution != bitmap.HorizontalResolution)
                                        {
                                            //pick destination resolution by which bitmap is closer to 300dpi
                                            int b1 = Math.Abs( (int)bitmapPage.HorizontalResolution - 300 );
                                            int b2 = Math.Abs( (int)bitmap.HorizontalResolution - 300 );
                                            if(       ( b1 < b2 )
                                                 ||   ( ( b1 == b2 ) && ( bitmapPage.HorizontalResolution > bitmap.HorizontalResolution ) )  )  //if same differences from 300dpi, use higher resolution                                                                                        
                                            {
                                                //use bitmapPage resolution
                                                bitmapNewWidth = ( bitmapNewWidth * (long) bitmapPage.HorizontalResolution ) / (long) bitmap.HorizontalResolution;
                                                outHorizResolution = bitmapPage.HorizontalResolution;
                                            }
                                            else
                                            {
                                                //use bitmap resolution
                                                bitmapPageNewWidth = ( bitmapPageNewWidth * (long) bitmap.HorizontalResolution ) / (long) bitmapPage.HorizontalResolution;
                                                outHorizResolution = bitmap.HorizontalResolution;
                                            }
                                        }
                                        if (bitmapPage.VerticalResolution != bitmap.VerticalResolution)
                                        {
                                            //pick destination resolution by which bitmap is closer to 300dpi
                                            int b1 = Math.Abs((int)bitmapPage.VerticalResolution - 300);
                                            int b2 = Math.Abs((int)bitmap.VerticalResolution - 300);
                                            if ((b1 < b2)
                                                 || ((b1 == b2) && (bitmapPage.VerticalResolution > bitmap.VerticalResolution)))  //if same differences from 300dpi, use higher resolution                                                                                       
                                            {
                                                //use bitmapPage resolution
                                                bitmapNewHeight = (bitmapNewHeight * (long)bitmapPage.VerticalResolution) / (long)bitmap.VerticalResolution;
                                                outVertResolution = bitmapPage.VerticalResolution;
                                            }
                                            else
                                            {
                                                //use bitmap resolution
                                                bitmapPageNewHeight = (bitmapPageNewHeight * (long)bitmap.VerticalResolution) / (long)bitmapPage.VerticalResolution;
                                                outVertResolution = bitmap.VerticalResolution;
                                            }
                                        }

                                        if( ( bitmapPageNewWidth != bitmapPage.Width ) || ( bitmapPageNewHeight != bitmapPage.Height ) )
                                        {
                                            //page level bitmapPage needs to be rescaled
                                           // Bitmap resized = Rescale(bitmapPage, (int)bitmapPageNewWidth, (int)bitmapPageNewHeight, outHorizResolution, outVertResolution);
                                            Bitmap resized = ImageProcessing.Resizing.Resize(bitmapPage, (float)outHorizResolution, (float)outVertResolution, ImageProcessing.Resizing.ResizeMode.Quality);                                            
                                            bitmapPage.Dispose();
                                            bitmapPage = resized;
                                        }

                                        if ((bitmapNewWidth != bitmap.Width) || (bitmapNewHeight != bitmap.Height))
                                        {
                                            //current new bitmap needs to be rescaled
                                          //  Bitmap resized = Rescale(bitmap, (int)bitmapNewWidth, (int)bitmapNewHeight, outHorizResolution, outVertResolution);
                                            Bitmap resized = ImageProcessing.Resizing.Resize(bitmap, (float)outHorizResolution, (float)outVertResolution, ImageProcessing.Resizing.ResizeMode.Quality);
                                            bitmap.Dispose();
                                            bitmap = resized;
                                        }
                                    }
                                    //convert to same colordepth
                                    if (bitmapPage.PixelFormat != bitmap.PixelFormat)
                                    {                                        
                                        //need to convert to same color depth so we can overlay bitmaps on top of each other
                                        //convet to higher pixel format not to lose any color data
                                        Bitmap resampled = null;
                                        try
                                        {
                                            if (bitmapPage.PixelFormat < bitmap.PixelFormat)
                                            {
                                            
                                                switch( bitmap.PixelFormat )
                                                {
                                                    case PixelFormat.Format32bppArgb :                                                    
                                                         resampled = ImageProcessing.Resampling.Resample(bitmapPage, ImageProcessing.PixelsFormat.Format32bppRgb );
                                                         break;
                                                    case PixelFormat.Format24bppRgb:
                                                         resampled = ImageProcessing.Resampling.Resample(bitmapPage, ImageProcessing.PixelsFormat.Format24bppRgb);
                                                         break;
                                                    case PixelFormat.Format8bppIndexed:
                                                         resampled = ImageProcessing.Resampling.Resample(bitmapPage, ImageProcessing.PixelsFormat.Format8bppIndexed);
                                                         break;
                                                    default:                                                     
                                                         throw new Exception("Unsupported pixel format " + bitmap.PixelFormat.ToString() + " in Resample method!");
                                                     
                                                }
                                                if (resampled != null)
                                                {
                                                    bitmapPage.Dispose();
                                                    bitmapPage = resampled;
                                                }
                                            
                                            }
                                            else
                                            {
                                                switch (bitmapPage.PixelFormat)
                                                {
                                                    case PixelFormat.Format32bppArgb:
                                                        resampled = ImageProcessing.Resampling.Resample(bitmap, ImageProcessing.PixelsFormat.Format32bppRgb);
                                                        break;
                                                    case PixelFormat.Format24bppRgb:
                                                        resampled = ImageProcessing.Resampling.Resample(bitmap, ImageProcessing.PixelsFormat.Format24bppRgb);
                                                        break;
                                                    case PixelFormat.Format8bppIndexed:
                                                        resampled = ImageProcessing.Resampling.Resample(bitmap, ImageProcessing.PixelsFormat.Format8bppIndexed);
                                                        break;
                                                    default:
                                                        throw new Exception("Unsupported pixel format " + bitmapPage.PixelFormat.ToString() + " in Resample method!");                                                    
                                                }
                                                if (resampled != null)
                                                {
                                                    bitmap.Dispose();
                                                    bitmap = resampled;
                                                }
                                            }
                                        }

                                        catch (Exception ex)
			                            {
                                            if( bitmapPage != null)
                                            {
                                              bitmapPage.Dispose();
                                              bitmapPage = null;
                                            }
                                            if( bitmap != null)
                                            {
                                              bitmap.Dispose();
                                              bitmap = null;
                                            }
				                            throw ex;
			                            }
                                    }

                                    //convert offsets to pixels
                                    if ((xOffset < -5.0) || (xOffset > 2.0))
                                    //if (xOffset != 0)
                                    {
                                        //convert X offset to pixels
                                        // for some reason a testing pdf where images should not be shifted report in vector values in range <-1.8,0.47> so I am ignoring the offset if value is too small in range<-5,2>..
                                        iX = (int)((float)((xOffset * (float)bitmap.HorizontalResolution)) / 72);
                                    }
                                    if ((yOffset < -5.0) || (yOffset > 2.0))
                                    //if (yOffset != 0)
                                    {
                                        //convert X offset to pixels
                                        // for some reason a testing pdf where images should not be shifted report in vector values in range <-1.8,0.47> so I am ignoring the offset if value is too small in range<-5,2>..
                                        iY = (int)((float)((yOffset * (float)bitmap.VerticalResolution)) / 72);
                                    }

                                    int maxPageWidth = (int)((float)((pageSizeInUnits.Width * (float)bitmapPage.HorizontalResolution)) / 72);
                                    int maxPageHeight = (int)((float)((pageSizeInUnits.Height * (float)bitmapPage.VerticalResolution)) / 72);

                                    BitmapPaste(ref bitmapPage, bitmap, iX, iY, maxPageWidth, maxPageHeight, bitmapPage.HorizontalResolution, bitmapPage.VerticalResolution, imageMask);                                    

                                    if (bitmap != null)
                                    {
                                        bitmap.Dispose();
                                        bitmap = null;
                                    }                                    
                                }
/*                                
                                 //debugging
                                                                    DirectoryInfo dir2 = new DirectoryInfo(@"C:\ProgramData\DLSG\BscanILL\temp2");

                                                                    if (bitmapPage.PixelFormat == PixelFormat.Format1bppIndexed)
                                                                    {
                                                                        string filePath = GetUniqueFile(dir2, ImageFormat.Tiff);
                                                                        SaveAsTiff(filePath, bitmapPage, EncoderValue.CompressionCCITT4);                                        
                                                                    }
                                                                    else
                                                                    {
                                                                        string filePath = GetUniqueFile(dir2, ImageFormat.Jpeg);
                                                                        SaveAsJpeg(filePath, bitmapPage);
                                                                    }
*/                                 
                            }
                        }
                        else
                        {
                            //unsupported compression
                            throw new Exception("Unsupported image compression decoder:" + filterArray[0].ToString() + "!"); 
                        }
								 }                        
                    }

                    finally
                    {
                        if (bitmapPage == null)
                        if (bitmap != null)         //in case of issues dispose bitmap otherwise bitmap will be released in caller method
                        {
                            bitmap.Dispose();
                            bitmap = null;
                        }
                    }
                
            }
        }
        #endregion

        #region AddColorPalette()
        private static unsafe void AddColorPalette(Bitmap bitmap, Bitmap source)
        {
            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // if grayscale indexed - add color palette (maybe needs to be done for all Index typed images..)
                ColorPalette pal = bitmap.Palette;
                for (int i = 0; i <= 255; i++)
                {
                    //create grayscale color table
                    pal.Entries[i] = Color.FromArgb(i, i, i);
                }
                bitmap.Palette = pal; //must re-set the Palette property to force new color palette
            }
            else
            if (bitmap.PixelFormat == PixelFormat.Format1bppIndexed)
            {
                ColorPalette pal = bitmap.Palette;

                //copy over pallete from bitmap source to keep meaning of bits ( tifftag Photometrical Orientation)
                pal.Entries[0] = source.Palette.Entries[0];
                pal.Entries[1] = source.Palette.Entries[1];                

                //pal.Entries[0] = System.Drawing.Color.White;
                //pal.Entries[1] = System.Drawing.Color.Black;
                //pal.Entries[0] = Color.FromArgb(255, 255, 255);
                //pal.Entries[1] = Color.FromArgb(0, 0, 0);

                bitmap.Palette = pal; //must re-set the Palette property to force new color palette
            }
        }
        #endregion

        #region FillBitmapWithColor()
        private static void FillBitmapWithColor( ref Bitmap dest, byte color )
        {
            BitmapData bData = null;                                        

            try
            {
                bData = dest.LockBits(new Rectangle(0, 0, dest.Width, dest.Height), ImageLockMode.WriteOnly, dest.PixelFormat);                
                byte[] data = new byte[bData.Stride];   //length of one row of bitmap

                for (int x = 0; x < bData.Stride; x++)
                {
                    data[x] = color;
                }

                //fillup rest of rows line by line
                for (int y = 0; y < bData.Height ; y++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(data, 0,(  bData.Scan0 + ( y * bData.Stride )), data.Length);  // copies data row by row in bitomap
                }
            }
            finally
            {
                if (bData != null)
                    dest.UnlockBits(bData);
            }

        }
        #endregion

        #region BitmapPaste()
        private static unsafe void BitmapPaste(ref Bitmap dest, Bitmap insertedImage, int xOffset, int yOffset, int maxPageWidth, int maxPageHeight, float horizDPI, float verticalDPI, bool overlay )
        {
            bool needDispose = false;

            if (insertedImage != null)
            {
                bool skipPaste = false;
                int widthToUse = 0 ;
                int heightToUse = 0 ;

                if (dest == null)
                {                         
                        // total width of bitmap will be  insertedImage.Width + xOffset
                        // total height of bitmap will be  insertedImage.Height + yOffset
                        float dpiUseHoriz = insertedImage.HorizontalResolution;
                        float dpiUseVertical = insertedImage.VerticalResolution;

                        try
                        {
                            widthToUse = insertedImage.Width + xOffset ;
                            heightToUse = insertedImage.Height + yOffset;
                            // if this size is smaller then maxPageWidth and maxPageHeight -> use maxPageWidth and maxPageHeight to avoid too much of bitmap step-by-step resizing
                            if( widthToUse < maxPageWidth )
                            {
                                widthToUse = maxPageWidth ;
                                dpiUseHoriz = horizDPI;
                            }
                            if (heightToUse < maxPageHeight)
                            {
                                heightToUse = maxPageHeight;
                                dpiUseVertical = verticalDPI;
                            }

                            if ((Math.Abs(widthToUse - insertedImage.Width) > 10) || (Math.Abs(heightToUse - insertedImage.Height) > 10) || (xOffset != 0) || (yOffset != 0))
                            {
                                dest = new Bitmap(widthToUse, heightToUse, insertedImage.PixelFormat);
                                if (dest != null)
                                {
                                    needDispose = true;
                                    dest.SetResolution(dpiUseHoriz, dpiUseVertical);
                                    AddColorPalette(dest, insertedImage);
                                    // in case of bitonal image reset bytes from black (0) to white (255)
                                    if (dest.PixelFormat == PixelFormat.Format1bppIndexed)   //for now do it just for bitonal...
                                    {
                                        FillBitmapWithColor(ref dest, 0xff);
                                    }
                                }
                            }
                            else
                            {
                                //if current bitmap dimension is within 10 pixels in both directions to the max needed bitmap size -> just go with current bitmap - do not resize becasue of few pixels
                                dest = insertedImage;
                                skipPaste = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "Parameter is not valid.")
                            {
                                string msg = " Most likely not enough memory!";

                                throw new Exception(ex.Message + msg);
                            }
                            else
                            {
                                throw;
                            }
                        }                    
                }
                else
                {
                    //is destination bitmap big enough to fit image to be pasted?
                    //if( (dest.Width < insertedImage.Width + xOffset) || (dest.Height < insertedImage.Height + yOffset) )
                    if ((((insertedImage.Width + xOffset) - dest.Width) > 10) || (((insertedImage.Height + yOffset) - dest.Height) > 10))
                    {
                       // inserted image is bigger at least by 10 pixels at least in one direction - need to create bigger destination bitmap
                        Bitmap newBitmap = null;
                        //bitmap too small needs to be extended
                        try
                        {
                            widthToUse = insertedImage.Width + xOffset;
                            heightToUse = insertedImage.Height + yOffset;
                            // if this size is smaller then maxPageWidth and maxPageHeight -> use maxPageWidth and maxPageHeight to avoid too much of bitmap step-by-step resizing
                            if (widthToUse < maxPageWidth)
                            {
                                widthToUse = maxPageWidth;
                            }
                            if (heightToUse < maxPageHeight)
                            {
                                heightToUse = maxPageHeight;
                            }

                            newBitmap = new Bitmap(widthToUse, heightToUse, dest.PixelFormat);
                            if (newBitmap != null)
                            {
                                newBitmap.SetResolution(dest.HorizontalResolution, dest.VerticalResolution);
                                AddColorPalette(newBitmap, dest);
                                if (newBitmap.PixelFormat == PixelFormat.Format1bppIndexed)  //for now do it just for bitonal...
                                {
                                    // in case of bitonal image reset bytes from black (0) to white (255)
                                    FillBitmapWithColor(ref newBitmap , 0xff );
                                }
                            }

                            BitmapPaste(ref newBitmap, dest, 0, 0, maxPageWidth, maxPageHeight, dest.HorizontalResolution, dest.VerticalResolution, false);
                            if (newBitmap != null)
                            {
                                dest.Dispose();
                                dest = newBitmap;
                                needDispose = true;                                
                            }
                        }
                        catch (Exception ex)
                        {
                            if(newBitmap != null)
                            {
                                newBitmap.Dispose();
                                newBitmap = null;
                            }

                            if (ex.Message == "Parameter is not valid.")
                            {
                                string msg = " Most likely not enough memory!";

                                throw new Exception(ex.Message + msg);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }

                if ( ! skipPaste )
                {
                    if ((insertedImage.PixelFormat == PixelFormat.Format24bppRgb) || (insertedImage.PixelFormat == PixelFormat.Format8bppIndexed) || (insertedImage.PixelFormat == PixelFormat.Format1bppIndexed))
                        {
                            BitmapData bd1 = null;
                            BitmapData bd2 = null;
                            int interOffsetFromBottom, interOffsetFromTop;

                            try
                            {
                                bd1 = dest.LockBits(new Rectangle(0, 0, dest.Width, dest.Height), ImageLockMode.WriteOnly, dest.PixelFormat);
                                bd2 = insertedImage.LockBits(new Rectangle(0, 0, insertedImage.Width, insertedImage.Height), ImageLockMode.ReadOnly, insertedImage.PixelFormat);

                                byte* pResult = (byte*)bd1.Scan0.ToPointer();
                                byte* pSource = (byte*)bd2.Scan0.ToPointer();

                                int strideR = bd1.Stride;
                                int strideS = bd2.Stride;

                                //take dimension of bitmap to be inserted (smaller)
                                int width = bd2.Width;
                                int height = bd2.Height;
                                int startX = 0;
                                int startY = 0;

                                // we may need to tweak the code for negative offsets xOffset, yOffset when bitonal  !!!
                                if (xOffset < 0)
                                {
                                    startX = Math.Abs(xOffset);
                                }
                                if ((width + xOffset) > bd1.Width)
                                {
                                    width = bd1.Width - xOffset;
                                }

                                if (yOffset < 0)
                                {
                                    startY = Math.Abs(yOffset);
                                }                                                                                                                                        
                                if ((height + yOffset ) > bd1.Height)
                                {
                                    height = bd1.Height - yOffset;
                                }

                                if (dest.PixelFormat == PixelFormat.Format8bppIndexed)
                                {      
                                    // grayscale
                                    if ( ! overlay )
                                    {
                                        for (int y = startY; y < height; y++)
                                            for (int x = startX; x < width; x++)
                                            {
                                                try
                                                {
                                                    //use darker color
                                                    interOffsetFromBottom = insertedImage.Height + yOffset - y;
                                                    interOffsetFromTop = dest.Height - interOffsetFromBottom;

                                                        pResult[(interOffsetFromTop * strideR) + x + xOffset] = pSource[y * strideS + x];
                                                }
                                                catch (Exception)
                                                {
                                                    throw;
                                                }
                                            } 
                                    }
                                    else
                                    {
                                            for (int y = startY; y < height; y++)
                                              for (int x = startX; x < width; x++)
                                            {
                                                try
                                                {
                                                    //use darker color
                                                    interOffsetFromBottom = insertedImage.Height + yOffset - y;
                                                    interOffsetFromTop = dest.Height - interOffsetFromBottom;

                                                    if (pSource[y * strideS + x] < pResult[(interOffsetFromTop * strideR) + x + xOffset])
                                                    {
                                                      pResult[(interOffsetFromTop * strideR) + x + xOffset] = pSource[y * strideS + x];
                                                    }

                                                }
                                                catch (Exception)
                                                {
                                                    throw;
                                                }
                                            } 
                                    }
                                }
                                else
                                    if (dest.PixelFormat == PixelFormat.Format24bppRgb)
                                    {
                                        // color
                                        if (! overlay)
                                        {
                                              for (int y = startY; y < height; y++)
                                                for (int x = startX; x < width; x++)
                                                {
                                                    try
                                                    {
                                                        //use darker color when overlay flag                                                   
                                                        interOffsetFromBottom = insertedImage.Height + yOffset - y;
                                                        interOffsetFromTop = dest.Height - interOffsetFromBottom;

                                                                pResult[(interOffsetFromTop * strideR) + ((x + xOffset) * 3) + 0] = pSource[y * strideS + x * 3 + 0];
                                                                pResult[(interOffsetFromTop * strideR) + ((x + xOffset) * 3) + 1] = pSource[y * strideS + x * 3 + 1];
                                                                pResult[(interOffsetFromTop * strideR) + ((x + xOffset) * 3) + 2] = pSource[y * strideS + x * 3 + 2];
                                                    }
                                                    catch (Exception)
                                                    {
                                                        throw;
                                                    }
                                                }
                                        }
                                        else
                                        {
                                            for (int y = startY; y < height; y++)
                                                for (int x = startX; x < width; x++)
                                                {
                                                    try
                                                    {
                                                        //use darker color when overlay flag                                                   
                                                        interOffsetFromBottom = insertedImage.Height + yOffset - y;
                                                        interOffsetFromTop = dest.Height - interOffsetFromBottom;

                                                            if (pSource[y * strideS + x * 3 + 0] < pResult[(interOffsetFromTop * strideR) + ((x + xOffset) * 3) + 0]) 
                                                            {
                                                                pResult[(interOffsetFromTop * strideR) + ((x + xOffset) * 3) + 0] = pSource[y * strideS + x * 3 + 0];
                                                            }
                                                            if (pSource[y * strideS + x * 3 + 1] < pResult[(interOffsetFromTop * strideR) + ((x + xOffset) * 3) + 1])
                                                            {
                                                                pResult[(interOffsetFromTop * strideR) + ((x + xOffset) * 3) + 1] = pSource[y * strideS + x * 3 + 1];
                                                            }
                                                            if (pSource[y * strideS + x * 3 + 2] < pResult[(interOffsetFromTop * strideR) + ((x + xOffset) * 3) + 2])
                                                            {
                                                                pResult[(interOffsetFromTop * strideR) + ((x + xOffset) * 3) + 2] = pSource[y * strideS + x * 3 + 2];
                                                            }

                                                    }
                                                    catch (Exception)
                                                    {
                                                        throw;
                                                    }
                                                }
                                        }

                                    }
                                    else
                                        if (dest.PixelFormat == PixelFormat.Format1bppIndexed)
                                        {
                                            //bitonal
                                            // we may need to tweak the code for negative offsets xOffset, yOffset when bitonal  !!!

                                            Rectangle insertRectFromTop = new Rectangle(xOffset, dest.Height - (insertedImage.Height + yOffset), insertedImage.Width, insertedImage.Height);

                                            byte templateByteDestination = 0xFF;
                                            byte templateByteSource = 0xFF;
                                            byte endBitsMask = 0xFF;
                                            byte templateMaskDestUse = 0;
                                            byte templateMaskSourceUse = 0;
                                            byte tempDest = 0;
                                            byte tempSource = 0;
                                            byte tempByte = 0;
                                            ushort wSegmentToPrint = 0;   //16 bits of data

                                            int x = 0;
                                            int y = 0;
                                            int tempSourceByteWidth = 0 ; 

                                            //destination bitmap pointers
                                            long dStartOffset = insertRectFromTop.Top * strideR;
                                            int dLeftBytes = insertRectFromTop.Left / 8; 
                                            int bitLeftOffest = insertRectFromTop.Left % 8; 

                                            if (bitLeftOffest != 0)
                                            {
                                                templateByteDestination = 0xFF;
                                                templateByteDestination = (byte)((byte)templateByteDestination >> (byte)(bitLeftOffest));  // if 2 bits offset -> set template to 00111111

                                                templateByteSource = 0xFF;
                                                templateByteSource = (byte)((byte)templateByteSource << (byte)(8 - bitLeftOffest));  // if 2 bits offset -> set template to 11000000
                                            }

                                            tempSourceByteWidth = insertedImage.Width / 8 ;
                                            if( (insertedImage.Width % 8) != 0)
                                            {
                                                //width of source bitmap is not in byte boundary
                                                tempSourceByteWidth++;
                                            }
                                            int tempBitMove;
                                            int lastByteBitOverleft = (insertedImage.Width % 8) ;
                                            lastByteBitOverleft += bitLeftOffest;

                                            for (y = 0; y < insertedImage.Height; y++)
                                            {
                                                //loop through rows of inserted image
                                               // for (x = 0; x < strideS; x++)
                                                int bitsLeftToPrintInCurrentRow = insertedImage.Width;

                                                for (x = 0; x < tempSourceByteWidth; x++)
                                                {                                                     
                                                    //loop through bytes within current row of image to be inserted  
                                                    if( bitsLeftToPrintInCurrentRow > 0 )
                                                    if ((dLeftBytes + x) < strideR)
                                                    {
                                                        // if not out of destination width - print byte
//                                                        if (bitLeftOffest != 0)
//                                                        {                                                            
                                                            // left bit offset not in byte boundary -> we need to shift bits by 'bitLeftOffest' pixels to the right before copying byte
                                                            if (x == 0)
                                                            {
                                                                if( bitsLeftToPrintInCurrentRow + bitLeftOffest < 8 )
                                                                {
                                                                    //need to mask end of byte no to print source bits out of source width
                                                                    tempBitMove = 8 - ( bitsLeftToPrintInCurrentRow + bitLeftOffest ) ; 
                                                                    endBitsMask = 0xFF;
                                                                    endBitsMask = (byte)((byte)endBitsMask << (byte)(tempBitMove));  // 11000000
                                                                    templateMaskDestUse = (byte)((byte) templateByteDestination | (byte) endBitsMask);   // we have 0 on position in byte that will be kept from destination - it will not be overwritten by source bit

                                                                    tempBitMove = (bitsLeftToPrintInCurrentRow + bitLeftOffest); 
                                                                    endBitsMask = 0xFF;
                                                                    endBitsMask = (byte)((byte)endBitsMask >> (byte)(tempBitMove));  // 00111111
                                                                    templateMaskSourceUse = (byte)((byte)templateByteSource | (byte)endBitsMask);
                                                                    bitsLeftToPrintInCurrentRow -= bitsLeftToPrintInCurrentRow;
                                                                }
                                                                else
                                                                {
                                                                    templateMaskDestUse = templateByteDestination;
                                                                    templateMaskSourceUse = templateByteSource;
                                                                    bitsLeftToPrintInCurrentRow -= (8 - bitLeftOffest);
                                                                }
                                                                
                                                                //mask destination byte in case of first byte from left
                                                                //tempDest = (byte)((byte)pResult[dStartOffset + (y * strideR) + dLeftBytes + x] | (byte)templateByteDestination);
                                                                tempDest = (byte)((byte)pResult[dStartOffset + (y * strideR) + dLeftBytes + x] | (byte)templateMaskDestUse);

                                                                //shift source bits by necessary bits then OR with template
                                                                tempSource = (byte)((byte)pSource[(y * strideS) + x] >> (byte)bitLeftOffest);
                                                                tempSource = (byte)((byte)tempSource | (byte)templateMaskSourceUse);                                                                

                                                                tempByte = (byte)((byte)tempDest & (byte)tempSource);

                                                                if (overlay)
                                                                {
                                                                    //assuming white is zero so OR'ing bits will keep black bits..
                                                                    //pResult[dStartOffset + (y * strideR) + x] = (byte)((byte)pResult[dStartOffset + (y * strideR) + x] | (byte)tempByte);

                                                                    //assuming black is zero so and'ing bits will keep black bits..
                                                                    pResult[dStartOffset + (y * strideR) + dLeftBytes + x] = (byte)((byte)pResult[dStartOffset + (y * strideR) + dLeftBytes + x] & (byte)tempByte);                                                                    
                                                                }
                                                                else
                                                                {
                                                                    pResult[dStartOffset + (y * strideR) + dLeftBytes + x] = (byte)tempByte;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //overwrite entire byte in destination if not first byte from left
                                                                if (bitLeftOffest != 0)
                                                                {
                                                                    wSegmentToPrint = pSource[(y * strideS) + x - 1];
                                                                    wSegmentToPrint = (ushort)(wSegmentToPrint << (ushort)8);
                                                                    wSegmentToPrint = (ushort)((ushort)wSegmentToPrint | (ushort)(pSource[(y * strideS) + x]));
                                                                    wSegmentToPrint = (ushort)(wSegmentToPrint >> bitLeftOffest);   //now I have the bits I want to print to destination in lower byte..

                                                                    tempSource = (byte)wSegmentToPrint;
                                                                }
                                                                else
                                                                {
                                                                    tempSource = (byte) pSource[(y * strideS) + x] ;
                                                                }
                                                                if (bitsLeftToPrintInCurrentRow < 8 )
                                                                {
                                                                    tempBitMove = 8 - (bitsLeftToPrintInCurrentRow); 
                                                                    endBitsMask = 0xFF;
                                                                    templateMaskDestUse = (byte)((byte)endBitsMask << (byte)(tempBitMove));  // 11000000
                                                                    tempDest = (byte)((byte)pResult[dStartOffset + (y * strideR) + dLeftBytes + x] | (byte)templateMaskDestUse);

                                                                    //need mask source's last bits that are out of source image
                                                                    tempBitMove = bitsLeftToPrintInCurrentRow ;
                                                                    endBitsMask = 0xFF;
                                                                    templateMaskSourceUse = (byte)((byte)endBitsMask >> (byte)(tempBitMove));  // 00111111                                                                    
                                                                    tempSource = (byte)((byte)tempSource | (byte)templateMaskSourceUse);

                                                                    tempByte = (byte)((byte)tempDest & (byte)tempSource);

                                                                    bitsLeftToPrintInCurrentRow -= bitsLeftToPrintInCurrentRow;
                                                                }
                                                                else
                                                                {
                                                                    tempByte = (byte)tempSource; 
                                                                    bitsLeftToPrintInCurrentRow -= 8;
                                                                }

                                                                
                                                                if (overlay)
                                                                {
                                                                    //assuming white is zero so OR'ing bits will keep black bits..
                                                                    //pResult[dStartOffset + (y * strideR) + dLeftBytes + x] = (byte)((byte)pResult[dStartOffset + (y * strideR) + x] | (byte)tempSource);

                                                                    //assuming black is zero so and'ing bits will keep black bits..
                                                                    pResult[dStartOffset + (y * strideR) + dLeftBytes + x] = (byte)((byte)pResult[dStartOffset + (y * strideR) + dLeftBytes + x] & (byte)tempByte);
                                                                }
                                                                else
                                                                {
                                                                    pResult[dStartOffset + (y * strideR) + dLeftBytes + x] = (byte)tempByte;
                                                                }
                                                            }
                                                    }
                                                }

                                                //if (bitLeftOffest != 0)
                                                if (bitsLeftToPrintInCurrentRow > 0)
                                                {                                                    
                                                    // do not forget to  print last bits of insertedImage image 
                                                    x = tempSourceByteWidth;                                                    
                                                    if ((dLeftBytes + x) < strideR)
                                                    {
                                                        tempBitMove = 8 - (bitsLeftToPrintInCurrentRow);
                                                        endBitsMask = 0xFF;
                                                        templateMaskDestUse = (byte)((byte)endBitsMask << (byte)(tempBitMove));  // 11111100
                                                        tempDest = (byte)((byte)pResult[dStartOffset + (y * strideR) + dLeftBytes + x] | (byte)templateMaskDestUse);

                                                        //need mask source's last bits that are out of source image
                                                        tempBitMove = bitsLeftToPrintInCurrentRow;
                                                        endBitsMask = 0xFF;
                                                        templateMaskSourceUse = (byte)((byte)endBitsMask >> (byte)(tempBitMove));  // 00000011

                                                        tempSource = (byte)((byte)pSource[(y * strideS) + x - 1] << (byte)(8 - bitLeftOffest));  //use last bits
                                                        tempSource = (byte)((byte)tempSource | (byte)templateMaskSourceUse); 

                                                        tempByte = (byte)((byte)tempDest & (byte)tempSource);

                                                        bitsLeftToPrintInCurrentRow -= bitsLeftToPrintInCurrentRow;
                                                        if (overlay)
                                                        {
                                                            //assuming white is zero so OR'ing bits will keep black bits..
                                                            //pResult[dStartOffset + (y * strideR) + dLeftBytes + x] = (byte)((byte)pResult[dStartOffset + (y * strideR) + dLeftBytes + x] | (byte)tempByte);

                                                            //assuming black is zero so and'ing bits will keep black bits..
                                                            pResult[dStartOffset + (y * strideR) + dLeftBytes + x] = (byte)((byte)pResult[dStartOffset + (y * strideR) + dLeftBytes + x] & (byte)tempByte);                                                            
                                                        }
                                                        else
                                                        {
                                                            pResult[dStartOffset + (y * strideR) + dLeftBytes + x] = (byte)tempByte;
                                                        }
                                                    }
                                                }
                                            }


                                        }
                            }
                            finally
                            {
                                if (bd1 != null)
                                    dest.UnlockBits(bd1);
                                if (bd2 != null)
                                    insertedImage.UnlockBits(bd2);
                            }

                        }
                        else
                        {
                            PixelFormat destPixelFormat = dest.PixelFormat;
                            PixelFormat insertPixelFormat = insertedImage.PixelFormat;

                            if (needDispose)
                            {
                                if( dest != null )
                                {
                                    dest.Dispose();
                                    dest = null;
                                }
                                needDispose = false;
                            }
                            throw new Exception("Cannot paste bitmaps with specified pixel formats (" + insertPixelFormat.ToString() + ")!");
                        }
                    
                }
                
            }
        }
        #endregion

        #region Rescale()
        // this method: when input Bitonal bitmap - output is converted to 32bit color bitmap  because Graphics object cannot work with bitonal bitmaps
        private static Bitmap Rescale( Bitmap origBitmap , int newWidth , int newHeight , float outHorizResolution, float outVertResolution )
        {
            Bitmap outBitmap = new Bitmap(newWidth, newHeight, origBitmap.PixelFormat);
            using (Graphics gr = Graphics.FromImage(outBitmap))
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gr.DrawImage(origBitmap, new Rectangle(0, 0, newWidth, newHeight));
            }

            if (outBitmap != null)
            {
                outBitmap.SetResolution(outHorizResolution, outVertResolution);
            }
            return outBitmap;
        }
        #endregion

        #region GetFilter()
        private PdfName GetFilter(PdfDictionary pdfDictionary)
		{
			PdfName filter = pdfDictionary.Get(PdfName.FILTER) as PdfName;

			if (filter == null)
			{
				// try if filter is as array in pdf
				PdfArray filterArray = (PdfArray)pdfDictionary.Get(PdfName.FILTER);

				if (filterArray != null)
				{
					for (int arrayIndex = 0; arrayIndex < filterArray.Size; arrayIndex++)
					{
						PdfName arrayItem = filterArray[arrayIndex] as PdfName;

						// for this example we're making the assumption there's only __one__ image of this type on the page
						if (arrayItem != null)
						{
							if (PdfName.CCITTFAXDECODE.Equals(arrayItem) || PdfName.DCTDECODE.Equals(arrayItem) || PdfName.FLATEDECODE.Equals(arrayItem))
							{
								filter = arrayItem;
								break;
							}
						}
					}
				}
			}

			return filter;
		}
        #endregion

        #region GetFilterArray()
        private static List<PdfName> GetFilterArray(PdfDictionary pdfDictionary)
        {
            List<PdfName> filterArrayOut = new List<PdfName>();
            PdfName filter = pdfDictionary.Get(PdfName.FILTER) as PdfName;

            if (filter == null)
            {
                // try if filter is as array in pdf
                PdfArray filterArray = (PdfArray)pdfDictionary.Get(PdfName.FILTER);

                if (filterArray != null)
                {
                    for (int arrayIndex = 0; arrayIndex < filterArray.Size; arrayIndex++)
                    {
                        PdfName arrayItem = filterArray[arrayIndex] as PdfName;

                        // for this example we're making the assumption there's only __one__ image of this type on the page
                        if (arrayItem != null)
                        {
                            if (PdfName.CCITTFAXDECODE.Equals(arrayItem) || PdfName.DCTDECODE.Equals(arrayItem) || PdfName.FLATEDECODE.Equals(arrayItem))
                            {
                                //filter = arrayItem;                                
                                //break;
                                filterArrayOut.Add(arrayItem);
                            }
                        }
                    }
                }
            }
            else
            {
                filterArrayOut.Add(filter);
            }

            return filterArrayOut;
        }

		#endregion
	
		#region GetEncoderInfo()
		private static ImageCodecInfo GetEncoderInfo(string encoderName)
		{
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

			foreach (ImageCodecInfo encoder in encoders)
				if (encoder.MimeType.CompareTo(encoderName) == 0)
					return encoder;

			return encoders[0];
		}
		
		private static ImageCodecInfo GetEncoderInfo(ImageFormat imageFormat)
		{
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

			foreach(ImageCodecInfo encoder in encoders)
			{
				if(encoder.FormatID == imageFormat.Guid)
					return encoder;
			}

			return null;
		}		
		#endregion

		#region GenerateGrayJpegEncoderParams()
		private static EncoderParameters GenerateGrayEncoderParams()
		{
			EncoderParameters encoderParams = new EncoderParameters(1);
			//encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8L);
			return encoderParams;
		}
		#endregion	
		
		#region SaveAsJpeg()
		private static void SaveAsJpeg(string file, Bitmap bitmap, PixelFormat pixelFormat)
		{
			new FileInfo(file).Directory.Create();
			
            if( pixelFormat != PixelFormat.Format8bppIndexed)
            {
              //color
			  bitmap.Save(file, ImageFormat.Jpeg);
            }
            else
            {
              ImageCodecInfo imageCodecInfo = GetEncoderInfo("image/png"); ;            
              EncoderParameters	jpegEncoderParams = GenerateGrayEncoderParams();
              bitmap.Save(file, imageCodecInfo, jpegEncoderParams);
            }
		}
		#endregion

		#region SaveAsTiff()
		private static void SaveAsTiff(string file, Bitmap bitmap, EncoderValue compression)
		{
			new FileInfo(file).Directory.Create();

			ImageCodecInfo imageCodecInfo = GetEncoderInfo("image/tiff");

			if (imageCodecInfo != null)
			{
				EncoderParameters encoderParameters = new EncoderParameters(1);

				encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)compression);

				bitmap.Save(file, imageCodecInfo, encoderParameters);
			}
			else
			{
				throw new Exception("TIFF encoder is not installed on this computer!");
			}
		}
		#endregion

		#region GetBitmapFromBuffer()
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pdfStream"></param>
		/// <param name="sizeInUnits"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="pixelFormat"></param>
		/// <param name="swapColorBytes">if to change RGB to BGR</param>
		/// <returns></returns>
		private static Bitmap GetBitmapFromBuffer(byte[] pdfImageBuffer, iTextSharp.text.Rectangle sizeInUnits, int width, int height, PixelFormat pixelFormat, bool swapColorBytes)
		{
			int			pdfImageStride = pdfImageBuffer.Length / height;
			Bitmap		bitmap = new Bitmap(width, height, pixelFormat);	
			BitmapData	bitmapData = null;

			try
			{
                if (pixelFormat == PixelFormat.Format8bppIndexed)
                {
                    // if grayscale indexed - add color palette (maybe needs to be done for all Index typed images..)
                    ColorPalette pal = bitmap.Palette;
                    for (int i = 0; i <= 255; i++)
                    {
                        //create grayscale color table
                        pal.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    bitmap.Palette = pal; //must re-set the Palette property to force new color palette
                }
                else
                if (pixelFormat == PixelFormat.Format1bppIndexed)
                {
                    ColorPalette pal = bitmap.Palette;

                    pal.Entries[0] = System.Drawing.Color.White;   // Color.FromArgb(255, 255, 255); 
                    pal.Entries[1] = System.Drawing.Color.Black;   // Color.FromArgb(0, 0, 0);                    

                    //pal.Entries[0] = System.Drawing.Color.White;
                    //pal.Entries[1] = System.Drawing.Color.Black;
                    //pal.Entries[0] = Color.FromArgb(255, 255, 255);
                    //pal.Entries[1] = Color.FromArgb(0, 0, 0);

                    bitmap.Palette = pal; //must re-set the Palette property to force new color palette
                }                

                bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

				int resultStride = bitmapData.Stride;
				int minStride = Math.Min(pdfImageStride, resultStride);

				unsafe
				{
					byte* scan0 = (byte*)bitmapData.Scan0.ToPointer();
					byte* pCurrent;

					if (pixelFormat == PixelFormat.Format24bppRgb && swapColorBytes)
					{
						byte[] buffer = new byte[minStride];

						for (int y = 0; y < height; y++)
						{
							pCurrent = scan0 + y * resultStride;

							for (int x = 0; x < minStride; x = x + 3)
							{
								pCurrent[x] = pdfImageBuffer[y * pdfImageStride + x + 2];
								pCurrent[x + 1] = pdfImageBuffer[y * pdfImageStride + x + 1];
								pCurrent[x + 2] = pdfImageBuffer[y * pdfImageStride + x];
							}
						}
					}
                    else if (pixelFormat == PixelFormat.Format24bppRgb && ( ! swapColorBytes) )
                    {
                        byte[] buffer = new byte[minStride];

                        for (int y = 0; y < height; y++)
                        {
                            pCurrent = scan0 + y * resultStride;

                            for (int x = 0; x < minStride; x = x + 3)
                            {
                                pCurrent[x] = pdfImageBuffer[y * pdfImageStride + x ];
                                pCurrent[x + 1] = pdfImageBuffer[y * pdfImageStride + x + 1];
                                pCurrent[x + 2] = pdfImageBuffer[y * pdfImageStride + x + 2];
                            }
                        }
                    }
					else
					{
                        //gray or BW 
						for (int y = 0; y < height; y++)
						{
							pCurrent = scan0 + y * resultStride;

							for (int x = 0; x < minStride; x++)
							{
								pCurrent[x] = pdfImageBuffer[y * pdfImageStride + x];
							}
						}
					}
				}

			}
			finally
			{
				if (bitmapData != null)
					bitmap.UnlockBits(bitmapData);
			}

			// formula to calculate dpi from size in user units and actual pixel size:
			// dpi = ( 'pixels' * 72 ) / 'size in user units'
			int xRes = (int)((float)width * (float)72 / sizeInUnits.Width);
			int yRes = (int)((float)height * (float)72 / sizeInUnits.Height);

			bitmap.SetResolution(xRes, yRes);

			return bitmap;
		}
       
		#endregion

		#region GetUniqueFile()
		private static string GetUniqueFile(DirectoryInfo dir, ImageFormat imageFormat)
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

		#region GetImagesCountInDocument()
		private static int GetImagesCountInDocument(iTextSharp.text.pdf.PdfReader pdfReader)
		{
#if DEBUG
			DateTime start = DateTime.Now;
#endif
	
			int pageCounter = 0;

			for (int i = 0; i <= pdfReader.XrefSize - 1; i++)
			{
				iTextSharp.text.pdf.PdfObject pdfObject = pdfReader.GetPdfObject(i);

				if ((pdfObject != null) && pdfObject.IsStream())
				{
                    PdfDictionary pdfDictionary = (PdfDictionary)pdfObject;

					iTextSharp.text.pdf.PdfObject subtype = ((iTextSharp.text.pdf.PdfStream)pdfObject).Get(iTextSharp.text.pdf.PdfName.SUBTYPE);

					//if ((subtype != null) && subtype.ToString() == iTextSharp.text.pdf.PdfName.IMAGE.ToString())
                    if (iTextSharp.text.pdf.PdfName.IMAGE.Equals(subtype))
					{
                        //if (PdfName.ASCIIHEXDECODE.Equals(filterArray[0]))
                        //if (PdfName.ASCII85DECODE.Equals(filterArray[0]))
                        //if (PdfName.LZWDECODE.Equals(filterArray[0]))
                        //if (PdfName.RUNLENGTHDECODE.Equals(filterArray[0]))
                        //if (PdfName.JPXDECODE.Equals(filterArray[0]))
                        //if (PdfName.CRYPT.Equals(filterArray[0]))
                        //if (PdfName.JBIG2DECODE.Equals(filterArray[0]))  //bitonal compression

						//if (PdfName.CCITTFAXDECODE.Equals(filter) || PdfName.DCTDECODE.Equals(filter) || PdfName.FLATEDECODE.Equals(filter))
							pageCounter++;
					}
				}
			}

#if DEBUG
			Console.WriteLine("PdfExtractor, GetImagesCountInDocument() for " + pageCounter + " images: " + DateTime.Now.Subtract(start).ToString());
#endif
	
			return pageCounter;
		}
		#endregion

		#region RaiseProgressChanged()
		void RaiseProgressChanged(double progress)
		{
			if (ProgressChanged != null)
				ProgressChanged(progress);
		}
		#endregion

		#endregion

	}
}
