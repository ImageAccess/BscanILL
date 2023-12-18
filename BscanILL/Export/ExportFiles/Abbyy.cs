using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BscanILL.Misc;
using OCREngineLibrary;

#if ! IRIS_ENGINE

namespace BscanILL.Export.ExportFiles
{
    public class Abbyy : ExportFilesBasics
    {
        static Abbyy instance = null;
        public static string runtimeInfo = @"aby$177289#ImageAccessInc";                
        public static string runtimeCode = @"SWAD-1000-0003-2569-3292-9294";
        
        ITextSharp iTextSharp = new ITextSharp();

        bool disposed = false;

		#region constructor
		private Abbyy()
		{
			this.iTextSharp.ProgressChanged += delegate(double progress) { Progress_Changed(progress); };
			this.iTextSharp.DescriptionChanged += delegate(string description) { Description_Changed(description); };
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
            if (! this.disposed)
            {
                if (AbbyyConnector.Instance != null)
                {
                    //AbbyyConnector.Instance.Dispose();                    
                    AbbyyConnector.Instance.ForcedUnload();                    
                }
            }
			this.disposed = true;
			instance = null;
		}
		#endregion

        #region class SpdfStruct
        public class SpdfStruct
        {
            public readonly BscanILL.Hierarchy.IllPage IllPage = null;
            //public readonly FileInfo Xml = null;
            public readonly DirectoryInfo XmlDir = null;
            private FileInfo imageToInsert = null;
            private bool needAbbyyProcessing = false;
            public readonly Scanners.ColorMode ImageColorMode = Scanners.ColorMode.Unknown;

            //public SpdfStruct(BscanILL.Hierarchy.IllPage illPage, FileInfo xml, FileInfo imageToInsert)
            public SpdfStruct(BscanILL.Hierarchy.IllPage illPage, DirectoryInfo xmlDir, FileInfo imageToInsert)
            {
                this.IllPage = illPage;
                //this.Xml = xml;
                this.XmlDir = xmlDir;                
                this.imageToInsert = imageToInsert;
                this.NeedAbbyyProcessing = false;
                this.ImageColorMode = Scanners.ColorMode.Unknown;
            }

            public SpdfStruct(BscanILL.Hierarchy.IllPage illPage, DirectoryInfo xmlDir, FileInfo imageToInsert, bool processAbbyy, Scanners.ColorMode imageColorMode)
            {
                this.IllPage = illPage;
                //this.Xml = xml;
                this.XmlDir = xmlDir;
                this.imageToInsert = imageToInsert;
                this.NeedAbbyyProcessing = processAbbyy;
                this.ImageColorMode = imageColorMode;
            }

            public FileInfo ImageToInsert
            {
                get
                {
                    return imageToInsert;
                }
                set
                {
                    imageToInsert = value;
                }
            }

            public bool NeedAbbyyProcessing
            {
                get
                {
                    return needAbbyyProcessing;
                }
                set
                {
                    needAbbyyProcessing = value;
                }
            }
        }
        #endregion
        
        //PUBLIC PROPERTIES
        #region public properties

        public static Abbyy Instance
        {
            get
            {
                if (instance == null)
                    instance = new Abbyy();

                return instance;
            }
        }

        public string OcrProfile
        {
            get 
            {
                if (AbbyyConnector.Instance == null)
                {
                    return "";
                }
                else
                {
                    return AbbyyConnector.Instance.PredefinedProfile;
                }
            }

            set
            {
                if (AbbyyConnector.Instance != null)
                {
                    AbbyyConnector.Instance.PredefinedProfile = value;
                }

            }

        }

        #endregion



        //PUBLIC METHODS
        #region public methods

        #region CreatePdf()
        public void CreatePdf(FileInfo destFile, List<SpdfStruct> images, ulong maxFileSize, List<string> warnings, bool updateInfoBar, int jpgQuality)
        {
            //string currentDirectory = Environment.CurrentDirectory;

            try
            {
                //Environment.CurrentDirectory = _settings.General.IrisBinDir;
                CreatePdfInternal(destFile, images, maxFileSize, warnings, updateInfoBar, jpgQuality);
            }
            finally
            {
                //Environment.CurrentDirectory = currentDirectory;
            }
        }

        public void CreatePdf(FileInfo destFile, SpdfStruct pdfStruct, bool updateInfoBar, int jpgQuality)
        {
            //string currentDirectory = Environment.CurrentDirectory;

            try
            {
                //Environment.CurrentDirectory = _settings.General.IrisBinDir;

                CreatePdfInternal(destFile, pdfStruct, updateInfoBar, jpgQuality);
            }
            finally
            {
                //Environment.CurrentDirectory = currentDirectory;
            }
        }

        #endregion

        #region CreateText()
        public void CreateText(FileInfo destFile, List<FileInfo> images, bool updateInfoBar)
        {
         //   string currentDirectory = Environment.CurrentDirectory;

            try
            {
                  ///no  need to switch to iris bin folder
       //         Environment.CurrentDirectory = _settings.General.IrisBinDir;

                CreateRtfInternal(destFile, images, updateInfoBar);

       //         Environment.CurrentDirectory = currentDirectory;
            }
            finally
            {
       //         Environment.CurrentDirectory = currentDirectory;
            }
        }
        #endregion


        #region Ocr()
        public string Ocr(FileInfo file)
        {
            //string currentDirectory = Environment.CurrentDirectory;

            try
            {
                //Environment.CurrentDirectory = _settings.General.IrisBinDir;

                return RunAbbyyOcrInternal(file);
            }
            finally
            {
                //Environment.CurrentDirectory = currentDirectory;
            }
        }
        #endregion

        #region CheckAbbyyEngineInstance()
        public static bool CheckAbbyyEngineInstance()
        {          
          if (AbbyyConnector.Instance == null)
          {
              return false;
          }
          else
          {
              return true; 
          }          
        }
        #endregion

        #region LoadAbbyy()
        //public static void LoadIris()
        public static void LoadAbbyy()
        {
            //string currentDirectory = Environment.CurrentDirectory;

            try
            {
                //string assemblyFileName = "IdrsWrapper3.dll";

               // long result = RunRegSVR32Process();  //no neeed to register here, it should be already registered during Abbyy Engine installation process

                //if (! AbbyyConnector.EngineIsLoaded)
                {
                    AbbyyConnector.InProc = true;
                    string p0 = AbbyyConnector.FindOCREngine();
                    if (p0.Length == 0)
                    {
                        throw new Exception("OCR Engine directory not found");
                    }
                    string p1 = runtimeInfo;  // @"aby$177289#ImageAccessInc"
                    string p2 = runtimeCode;  // @"SWAD-1000-0003-2569-3292-9294"

                    string p3 = Path.Combine(BscanILL.SETTINGS.Settings.Instance.General.OCRLicenseDir, @"SWAO-1020-0003-2569-3339-7310.ABBYY.LocalLicense");

                    FileInfo licFile = new FileInfo(p3);
                    if( ! licFile.Exists )
                    {
                        throw new Exception("OCR Engine license file not found");
                    }

                    AbbyyConnector.SetOCRInfo(p0, p1, p2, p3);
/*
                    while (!AbbyyConnector.Instance.Ready)
                    {
                        System.Threading.Thread.Sleep(10);
                    }

                    if (! AbbyyConnector.EngineIsLoaded)
                    {
                        throw new Exception( "OCR Engine has not been loaded");
                    }
*/ 
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Can't register Abbyy assembly! " + BscanILL.Misc.Misc.GetErrorMessage(ex));
            }
            finally
            {
                //Environment.CurrentDirectory = currentDirectory;
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
        //public void FormatImage(FileInfo reducedImage, FileInfo destXml)
        public void FormatImage(FileInfo reducedImage,  DirectoryInfo destXml)
        {
            lock (this)
            {
                if (this.disposed == false)
                {
                    //string currentDirectory = Environment.CurrentDirectory;

                    try
                    {
                  //      Environment.CurrentDirectory = _settings.General.IrisBinDir;

                        FormatImageInternal(reducedImage, destXml);
                    }
                    finally
                    {
                  //      Environment.CurrentDirectory = currentDirectory;
                    }
                }
            }
        }
        #endregion

        #endregion

        //PRIVATE METHODS
        #region private methods

        #region RunRegSVR32Process()

        // this registration is executed during engine installing where after installing files, we have to register the dll using test app included in installation
        private static long RunRegSVR32Process()
        {
            string OCRPath = AbbyyConnector.FindOCREngine();

            Process newprocess = new Process();
            string cmdLine = @"C:\Windows\SysWOW64\RegSVR32.exe";
            cmdLine = @"RegSVR32.exe";

            string args = @" /n /i:""" + OCRPath + @"\Inc"" """ + OCRPath + @"\FREngine.dll""";

            newprocess.StartInfo.FileName = cmdLine;
            //newprocess.StartInfo.CreateNoWindow = true;
            newprocess.StartInfo.Arguments = args;
            newprocess.StartInfo.UseShellExecute = true;
            //MessageBox.Show (args, "regsvr32 args", MessageBoxButtons.OK);
            newprocess.Start();

            // wait a max of 30 seconds for the process to be closed...
            newprocess.WaitForExit(30000);
            return newprocess.ExitCode;
        }
        #endregion

        #region CreatePdfInternal()
        private void CreatePdfInternal(FileInfo destFile, List<SpdfStruct> images, ulong maxFileSize, List<string> warnings, bool updateInfoBar, int jpgQuality)
        {
            lock (this)
            {
                List<ITextSharp.InsertImageStruct> filesToInsert = new List<ITextSharp.InsertImageStruct>();

                // create pdf files
                try
                {
                    try
                    {
//                        LoadAbbyy();
                        if (AbbyyConnector.Instance == null)
                        {
                            throw new Exception("OCR Engine is null in CreatePdfInternal()");
                        }
                       //////// AbbyyConnector.Instance.ProgressChanged += new ProgressChangedHandleEngine(Abbyy_ProgressChanged);  //not working for me current implementation OnProgress in AbbyyInterface

                        if (updateInfoBar)
                           Description_Changed("Creating multi-image PDF file...");

                       // using (Idrs.IdrsWrapper idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir))
                        //{
                            for (int i = 0; i < images.Count; i++)
                            {
                                SpdfStruct pdfStruct = images[i];
                                FileInfo f = new FileInfo(destFile.Directory.FullName + @"\" + Path.GetFileNameWithoutExtension(destFile.Name) + "_" + i.ToString() + ".pdf");

                                try
                                {

                                    if (pdfStruct.XmlDir != null)
                                    {
                                    //idrsWrapper.OpenPdf(f);
                                    //idrsWrapper.AddPdfImage(pdfStruct.Xml, pdfStruct.ImageToInsert);
                                    //idrsWrapper.Close();

                                    //AbbyyConnector.Instance.ExecutePDFFromDocument(f, pdfStruct.XmlDir, pdfStruct.ImageToInsert);
                                    AbbyyConnector.Instance.ExecutePDFFromDocument(f, pdfStruct.XmlDir, jpgQuality);   //we do not need image file when loading doc from xml format file

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

                                if (updateInfoBar)
                                   Progress_Changed((float)((i + 1.0) / images.Count));
                            }
                        //}
                    }
/*
                    catch (Idrs.IdrsWrapperException ex)
                    {
                        if (ex.File != null && File.Exists(ex.File.FullName))
                            Notifications.Instance.Notify(this, Notifications.Type.Error, "Abbyy, CreatePdfInternal() multi-image: " + ex.Message, ex, ex.File);
                        else
                            Notifications.Instance.Notify(this, Notifications.Type.Error, "Abbyy, CreatePdfInternal() multi-image: " + ex.Message, ex);

                        throw new IllException("Can't create PDF file!");
                    }
*/
                    catch (IllException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Abbyy, CreatePdfInternal() OCR: " + ex.Message, ex);
                        throw new IllException("Can't create PDF file!");
                    }

                    // merge single files
                    try
                    {
                        iTextSharp.MergeFiles(destFile, filesToInsert, updateInfoBar);

                        destFile.Refresh();
                        if (maxFileSize > 0 && (ulong)destFile.Length > maxFileSize)
                        {
                            throw new IllException(BscanILL.Misc.ErrorCode.FileOverSizeLimit, "PDF file size is over size limit.");
                        }
                        else
                        {
                            if (updateInfoBar)
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

                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Abbyy, CreatePdfInternal() merging: " + ex.Message, ex);
                        throw new IllException("Can't create PDF file!");
                    }
                }
                finally
                {
                    DeleteFiles(filesToInsert);
                    AbbyyConnector.Instance.ProgressChanged -= new ProgressChangedHandleEngine(Abbyy_ProgressChanged);
                }
            }
        }
        #endregion

        #region CreatePdfInternal()
        private void CreatePdfInternal(FileInfo destFile, SpdfStruct pdfStruct, bool updateInfoBar, int jpgQuality)
        {
            lock (this)
            {
                try
                {
                    if (updateInfoBar)
                       Progress_Changed(0);

//                    LoadAbbyy();   
                    if (AbbyyConnector.Instance == null)
                    {
                        throw new Exception("OCR Engine is null in CreatePdfInternal()");
                    }
                    ////// AbbyyConnector.Instance.ProgressChanged += new ProgressChangedHandleEngine(Abbyy_ProgressChanged);           //not working for me current implementation OnProgress in AbbyyInterface

                    /*
                                        using (Idrs.IdrsWrapper idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir))
                                        {
                                            idrsWrapper.OpenPdf(destFile);

                                            if (pdfStruct.Xml != null)
                                                idrsWrapper.AddPdfImage(pdfStruct.Xml, pdfStruct.ImageToInsert);
                                            else
                                                idrsWrapper.AddPdfImage(pdfStruct.ImageToInsert, false);

                                            idrsWrapper.Close();
                                        }
                    */

                    //AbbyyConnector.Instance.ExecutePDFFromDocument(destFile, pdfStruct.XmlDir, pdfStruct.ImageToInsert);
                    AbbyyConnector.Instance.ExecutePDFFromDocument(destFile, pdfStruct.XmlDir, jpgQuality);      //we do not need image file when loading doc from xml format file

                    if ( updateInfoBar)
                      Progress_Changed(1);
                }
/*
                catch (Idrs.IdrsWrapperException ex)
                {
                    if (ex.File != null && File.Exists(ex.File.FullName))
                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreatePdfInternal(): " + ex.Message, ex, ex.File);
                    else
                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreatePdfInternal(): " + ex.Message, ex);

                    throw new IllException("Can't create PDF file!");
                }
*/
                catch (Exception ex)
                {
                    Notifications.Instance.Notify(this, Notifications.Type.Error, "Abbyy, CreatePdfInternal(): " + ex.Message, ex);
                    throw new IllException("Can't create PDF file!");
                }

                finally
                {
                    AbbyyConnector.Instance.ProgressChanged -= new ProgressChangedHandleEngine(Abbyy_ProgressChanged);
                }
            }
        }
        #endregion

        #region FormatImageInternal()        
        private void FormatImageInternal(FileInfo reducedImage, DirectoryInfo destXml)            
        {
            lock (this)
            {
                try
                {                   
                       reducedImage.Refresh();
                       destXml.Refresh();
                       //LoadAbbyy();

                       if (AbbyyConnector.Instance == null )
                       {
                           throw new Exception("OCR Engine is null in FormatImageInternal()");
                       }

                       AbbyyConnector.Instance.GetFormatFile(reducedImage, destXml);                   
                }
/*
                catch (Idrs.IdrsWrapperException ex)
                {
                    throw ex;
                }
 */ 
                catch (Exception ex)
                {
                    Notifications.Instance.Notify(this, Notifications.Type.Error, "Abbyy, FormatImageInternal(): " + ex.Message, ex);
                    throw new IllException(ex);
                }
            }
        }
        #endregion

        #region CreateRtfInternal()
        private void CreateRtfInternal(FileInfo destFile, List<FileInfo> images, bool updateInfoBar)
        {
            lock (this)
            {
                try
                {
/*
                    using (Idrs.IdrsWrapper idrsWrapper = new Idrs.IdrsWrapper(_settings.General.IrisResourcesDir))
                    {
                        idrsWrapper.PdfImageAdded += new Idrs.ImageEventHndl(Abbyy_ImageAdded);
                        idrsWrapper.RtfOcrDone += new Idrs.ImageEventHndl(Abbyy_RtfOcrDone);
                        idrsWrapper.UniOcrDone += new Idrs.ImageEventHndl(Abbyy_UniOcrDone);
                        idrsWrapper.TextOcrDone += new Idrs.ImageEventHndl(Abbyy_TextOcrDone);

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
 */

//                        LoadAbbyy();
                    if (updateInfoBar)
                        Progress_Changed(0);

                        //AbbyyConnector.Instance.StatusUpdate += new OCREngineStatusUpdate(abbyyConnector_StatusUpdate);
                        if (AbbyyConnector.Instance == null)
                        {
                            throw new Exception("OCR Engine is null in CreateRtfInternal()");
                        }

                        if (updateInfoBar)
                        {
                            ///AbbyyConnector.Instance.ProgressChanged += new ProgressChangedHandleEngine(Abbyy_ProgressChanged);   
                            AbbyyConnector.Instance.ProgressExternalChanged += new ProgressChangedHandleEngine(Abbyy_ProgressChanged);
                        }
                        AbbyyConnector.Instance.ExecuteOCR("RTF", destFile, images);

                        if (updateInfoBar)
                            Progress_Changed(1);

                    
                }
  /*              catch (Idrs.IdrsWrapperException ex)
                {
                    string exceptionType = ex.ExceptionType.ToString();

                    if (ex.File != null && File.Exists(ex.File.FullName))
                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreateRtfInternal() multi-image X1: " + ex.Message + " Exception type: " + exceptionType, ex, ex.File);
                    else
                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreateRtfInternal() multi-image X2: " + ex.Message + " Exception type: " + exceptionType, ex);

                    throw new IllException("Can't create RTF file!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
                }  */
                catch (Exception ex)
                {
                    Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, CreateRtfInternal() multi-image X3: " + ex.Message, ex);
                    throw new IllException("Can't create text file!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
                }
                finally
                {
                    AbbyyConnector.Instance.ProgressChanged -= new ProgressChangedHandleEngine(Abbyy_ProgressChanged);
                    AbbyyConnector.Instance.ProgressExternalChanged -= new ProgressChangedHandleEngine(Abbyy_ProgressChanged);                    
                }
            }
        }
        #endregion

        #region RunAbbyyOcrInternal()
        private string RunAbbyyOcrInternal(FileInfo file)
        {
            lock (this)
            {
                string ocr = null;

                try
                {
//                    LoadAbbyy();
                    if (AbbyyConnector.Instance == null)
                    {
                        throw new Exception("OCR Engine is null in RunAbbyyOcrInternal()");
                    }

                    ////AbbyyConnector.Instance.ProgressChanged += new ProgressChangedHandleEngine(Abbyy_ProgressChanged);     //not working for me current implementation OnProgress in AbbyyInterface
                    ocr = AbbyyConnector.Instance.Ocr(file);

                }
/*
                catch (Idrs.IdrsWrapperException ex)
                {
                    if (ex.File != null && File.Exists(ex.File.FullName))
                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, RunIdrsOcrInternal(): " + ex.Message, ex, ex.File);
                    else
                        Notifications.Instance.Notify(this, Notifications.Type.Error, "Iris, RunIdrsOcrInternal(): " + ex.Message, ex);

                    throw new IllException("Can't OCR image!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
                }
*/
                catch (Exception ex)
                {
                    Notifications.Instance.Notify(this, Notifications.Type.Error, "Abbyy, RunAbbyyOcrInternal(): " + ex.Message, ex);
                    throw new IllException("Can't OCR image!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
                }
                finally
                {
                    AbbyyConnector.Instance.ProgressChanged -= new ProgressChangedHandleEngine(Abbyy_ProgressChanged);
                }
                return ocr;
            }
        }
        #endregion

        #region Abbyy_ImageAdded()
        private void Abbyy_ImageAdded(string image, float progress)
        {
            Progress_Changed(progress);
        }
        #endregion

        #region Abbyy_RtfOcrDone()
        private void Abbyy_RtfOcrDone(string image, float progress)
        {
            Progress_Changed(progress);
        }
        #endregion

        #region Abbyy_UniOcrDone()
        private void Abbyy_UniOcrDone(string image, float progress)
        {
            Progress_Changed(progress);
        }
        #endregion

        #region Abbyy_TextOcrDone()
        private void Abbyy_TextOcrDone(string image, float progress)
        {
            Progress_Changed(progress);
        }
        #endregion

        #region Abbyy_ProgressChanged()
        private void Abbyy_ProgressChanged(float progress)
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
