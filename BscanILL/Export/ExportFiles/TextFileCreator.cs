using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Data;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;

namespace BscanILL.Export.ExportFiles
{
	class TextFileCreator : ExportFilesBasics
	{        

//IRIS
#if IRIS_ENGINE
		Iris iris = Iris.Instance;
#else
        Abbyy abbyy = Abbyy.Instance;
#endif
		#region TextFileCreator()
		public TextFileCreator()
		{    
#if IRIS_ENGINE        
                this.iris.ProgressChanged += delegate(double progress) { Progress_Changed(progress); };
                this.iris.DescriptionChanged += delegate(string description) { Description_Changed(description); };
#else            
                this.abbyy.ProgressChanged += delegate(double progress) { Progress_Changed(progress); };
                this.abbyy.DescriptionChanged += delegate(string description) { Description_Changed(description); };            
#endif
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties
		//public static TextFileCreator Instance { get { return instance; } }
		#endregion

		//PUBLIC METHODS
		#region public methods

		#region CreateTextFile()
        public void CreateTextFile(BscanILL.Hierarchy.IllPage illPage, FileInfo outputFile, bool updateProgressBar)
		{
			BscanILL.Hierarchy.IllPages illPages = new BscanILL.Hierarchy.IllPages();
			illPages.Add(illPage);

            CreateTextFile(illPages, outputFile, updateProgressBar);
		}

        public void CreateTextFile(BscanILL.Hierarchy.IllPages illPages, FileInfo outputFile, bool updateProgressBar)
		{    
#if IRIS_ENGINE        
            List<BscanILL.Export.ExportFiles.Iris.TextStruct> xmlFiles = new List<BscanILL.Export.ExportFiles.Iris.TextStruct>();
#endif
            List<FileInfo> imageFiles = new List<FileInfo>();            

			DateTime start = DateTime.Now;

			for (int i = 0; i < illPages.Count; i++)
			{
      
//IRIS      
#if IRIS_ENGINE            
                    xmlFiles.Add(new BscanILL.Export.ExportFiles.Iris.TextStruct(illPages[i], illPages[i].FormatXmlFile));                
#else
                    imageFiles.Add(new FileInfo(illPages[i].FilePath.FullName));
#endif
                
			//	Progress_Changed((i + 1.0) / illPages.Count);
			}
            
//IRIS
#if IRIS_ENGINE
            iris.CreateText(outputFile, xmlFiles);
#else
            if (_settings.General.OcrEngProfile == BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Speed)
            {
                //Speed
                this.abbyy.OcrProfile = "Document Conversion Speed";     //Document Archiving is recommended profile for PDF creation. For RTF or PDF text only is recommended Document Conversion profile
            }
            else
            {
                //Accuracy
                this.abbyy.OcrProfile = "Document Conversion Accuracy";
            }
            abbyy.CreateText(outputFile, imageFiles, updateProgressBar);
#endif
		}
		#endregion

		#region CreateTextFile()
#if IRIS_ENGINE
		public void CreateTextFile(BscanILL.Hierarchy.IllPages illPages, FileInfo outputFile)
		{
			DirectoryInfo tempDir = new DirectoryInfo(outputFile.Directory.FullName + @"\temp");
			List<BscanILL.Export.ExportFiles.Iris.TextStruct> tempImages = new List<BscanILL.Export.ExportFiles.Iris.TextStruct>();

			tempDir.Create();

			ulong totalPixels;
			List<TimeSpan> timeSpans = GetTimeSpans(BscanILL.Misc.TimeEstimates.FilePreparationType.Text, illPages, out totalPixels);
			Set_ProgressIntervals(BscanILL.Languages.BscanILLStrings.Export_CreatingSupportImages_STR, timeSpans);
			DateTime start = DateTime.Now;

			for (int i = 0; i < illPages.Count; i++)
			{
				ProgressUnit_Started((uint)i);
				FileInfo tempFile = SaveImage(illPages[i], tempDir);
				tempImages.Add(new BscanILL.Export.ExportFiles.Iris.TextStruct(illPages[i], tempFile));

				//Progress_Changed((i + 1.0F) / (2.0F * illPages.Count));
				ProgressUnit_Finished((uint)i);
			}

			BscanILL.Misc.TimeEstimates.Instance.SaveFilePreparationSpeed(BscanILL.Misc.TimeEstimates.FilePreparationType.Text, (uint)totalPixels, DateTime.Now.Subtract(start).TotalSeconds);

			iris.CreateText(outputFile, tempImages);

			try { tempDir.Delete(true); }
			catch { }
		}
#endif
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region SaveImage()
		private static FileInfo SaveImage(BscanILL.Hierarchy.IllPage illPage, DirectoryInfo destDir)
		{
			BscanILL.Export.IP.ExportFileCreator creator = new BscanILL.Export.IP.ExportFileCreator();
			FileInfo file = new FileInfo(destDir.FullName + @"\" + illPage.FilePath.Name + ".tif");

			destDir.Create();

			creator.CreateExportFile(illPage.FilePath, file, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.G4),
				 Scanners.ColorMode.Bitonal, 200.0 / (double)illPage.FullImageInfo.DpiH);
			//file = illPage.GetExportFile(fileFormat, dpi.Value, warnings);

			file.Refresh();
			return file;
		}
		#endregion

		#endregion

	}
}
