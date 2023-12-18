using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bscan;

namespace BscanILL.FP
{
		
	public class FormsProcessing
	{
		// PRIVATE PROPERTIES
		#region private properties
		static BscanILL.SETTINGS.Settings _settings { get { return BscanILL.SETTINGS.Settings.Instance; } }
		#endregion



		// PUBLIC METHODS
		#region public methods

		#region Init()
		public static void Init()
		{
			DirectoryInfo sourceDir = new DirectoryInfo(BscanILL.Misc.Misc.StartupPath + @"\RequiredFiles\FormsProcessing");
            
            string destinationDirRootName;
            string appFolderName;
            string destinationDirPath;            

			DirectoryInfo destinationDir = _settings.FormsProcessing.BsaFile.Directory;
            destinationDirPath = destinationDir.FullName;
            destinationDirRootName = destinationDir.Parent.ToString();
            appFolderName = destinationDir.Name;            
            destinationDirPath = destinationDirPath.Substring(0, destinationDirPath.IndexOf(appFolderName));

            // destinationDirectory carier path to FormsProcessing destination subfolder in ProgramData
            DirectoryInfo destinationDirectory = new DirectoryInfo(destinationDirPath);
            
            FileInfo[] filesToCopy = sourceDir.GetFiles("*.*", SearchOption.AllDirectories );

			foreach (FileInfo file in filesToCopy)
			{
                //points to beginning of \\apps
                int rootDirEnds = file.FullName.IndexOf(destinationDirRootName) + destinationDirRootName.Length;
                int fileDirLength = file.DirectoryName.Length;

                //carries apps or lib\default
                string subDirName = file.FullName.Substring((rootDirEnds + 1), (fileDirLength - rootDirEnds - 1));                               
				
                string folderInDestDir = Path.Combine(destinationDirectory.FullName, subDirName);
                string fileInDestDir = Path.Combine(folderInDestDir, file.Name);
                
				if (File.Exists(fileInDestDir) == false)
                {
                    DirectoryInfo folderDestDir = new DirectoryInfo(folderInDestDir);
                    folderDestDir.Create();

					file.CopyTo(fileInDestDir);
                }
			}
		}
		#endregion

		#region Go()
		public static FormsProcessingResult Go( MainWindow mainWindow, FileInfo image, FileInfo bsaFile, FileInfo scriptFile, string trainingName)
		{
//IRIS
#if IRIS_ENGINE
			string currentDir = Environment.CurrentDirectory;
			
			try
			{
				Environment.CurrentDirectory = _settings.General.IrisBinDir;
				return GoInternal(image, bsaFile, scriptFile, trainingName);
			}
			finally
			{
				Environment.CurrentDirectory = currentDir;
			}
#else
            try
            {                
                return GoInternal(mainWindow, image, bsaFile, scriptFile, trainingName);
            }
            finally
            {
                
            }
#endif
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region GoInternal()
		private static FormsProcessingResult GoInternal(MainWindow mainWindow, FileInfo image, FileInfo bsaFile, FileInfo scriptFile, string trainingName)
		{
			string currentDir = Environment.CurrentDirectory;
			//BscanWrapper bscanWrapper = null;

			if (image.Exists == false)
				throw new Exception("Pullslip image '" + image.FullName + "' doesn't exist!");

			if (bsaFile.Exists == false)
				throw new Exception("Application script file '" + bsaFile.FullName + "' doesn't exist!");

			if (scriptFile.Exists == false)
			{
				BscanILL.UI.Dialogs.AlertDlg.Show( mainWindow, "Pullslip script file '" + scriptFile.FullName + "' doesn't exist!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
				throw new Exception("Pullslip script file '" + scriptFile.FullName + "' doesn't exist!");
			}

			try
			{
				//bscanWrapper = new BscanWrapper();
				using (BscanWrapper bscanWrapper = new BscanWrapper())
				{
				//IRIS
#if IRIS_ENGINE
					DirectoryInfo iDrsResourcesDir = new DirectoryInfo(_settings.General.IrisResourcesDir);
					bscanWrapper.InitModule(BscanWrapper.EngineType.SCRIPT | BscanWrapper.EngineType.BARCODE | BscanWrapper.EngineType.TOCR, iDrsResourcesDir);
#else
				    bscanWrapper.SetOCRKeyCode(BscanILL.Export.ExportFiles.Abbyy.runtimeInfo, BscanILL.Export.ExportFiles.Abbyy.runtimeCode, _settings.General.OCRLicenseDir);
                    bscanWrapper.InitModule(BscanWrapper.EngineType.SCRIPT | BscanWrapper.EngineType.BARCODE | BscanWrapper.EngineType.TOCR);
#endif
					bscanWrapper.ApplicationPath = bsaFile.FullName;	// "C:\\BscanILL\\Apps\\ILLScan.bsa";
					bscanWrapper.TrainingName = trainingName;			// "ND-SHOW";
					bscanWrapper.TN_Always = true;						// read TN even for Ariel, etc.. sets #77=0 

					//if (bscanWrapper.LoadImage(-1, 0, @"C:\BSCANILL\Lib\ND-SHOW\ND-SHOW-Master-FormA.TIF") >= 0)
					bscanWrapper.LoadImage(-1, image.FullName);
					//bscanWrapper.ExecuteScript(@"C:\BSCANILL\Lib\ND-SHOW\ND-SHOW.bsa", 21);
					bscanWrapper.ExecuteScript(scriptFile.FullName, 21);

					FormsProcessingResult result = new FormsProcessingResult(bscanWrapper.ILL, bscanWrapper.TN, bscanWrapper.Address,
						bscanWrapper.Patron, bscanWrapper.Delivery, bscanWrapper.AddressFlag);

					return result;
				}
			}
			catch (Exception ex)
			{
				BscanILL.Misc.Notifications.Instance.Notify(null, Misc.Notifications.Type.Error, ex.Message, ex);
				//throw new BscanILL.Misc.IllException(Misc.ErrorCode.IrisGeneral, ex.Message);
				return new FormsProcessingResult("", "", "", "", "", false);
			}

           /* finally
            {
				if (bscanWrapper != null)
				{
					bscanWrapper.Dispose();
				}

			}*/
		}
		#endregion

		#endregion

	}
}
