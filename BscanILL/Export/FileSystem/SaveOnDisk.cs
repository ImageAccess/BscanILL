using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using BscanILL.Misc;
using BscanILL.Hierarchy;
using BscanILL.Export.AdditionalInfo;


namespace BscanILL.Export.FileSystem
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class SaveOnDisk : ExportBasics
	{

		#region constructor
		public SaveOnDisk()
		{
		}
		#endregion

		#region destructor
		public void Dispose()
		{
		}
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region ExportArticle()
		public void ExportArticle(ExportUnit exportUnit)
		{
            Article				article = exportUnit.Article;
			AdditionalSaveOnDisk	addOn = (AdditionalSaveOnDisk)exportUnit.AdditionalInfo;
            int iProgress = 0;
			//DirectoryInfo			destDir = new DirectoryInfo(addOn.ExportDirectory);

            Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via Save on Disk---");            

            if (addOn.ExportDirectory.Length == 0)
            	throw new IllException(ErrorCode.SaveOnDiskNoDirectory);

            DirectoryInfo destDir = GetDestinationFolder(addOn, article);

            iProgress = 10;
            Progress_Changed(exportUnit, iProgress, "Checking data...");

			if (exportUnit.Article.IllNumber.StartsWith("-") == false && this.CheckingArticleInDb)
				this.Illiad.CheckArticleInDb(exportUnit);

			if (destDir.Exists && addOn.ActionBeforeExport == BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.ActionBeforeExport.CleanExportDir)
			{
				destDir.Delete(true);

                iProgress += 10;
                Progress_Changed(exportUnit, iProgress, "Obsolete destination file(s) was deleted.");
			}

			destDir.Refresh();
			destDir.Create();

			if (exportUnit.Files.Count == 1)
			{
                Progress_Changed(exportUnit, Convert.ToInt32(iProgress + (((0 + 1) * 65) / exportUnit.Files.Count)), string.Format("Copying file {0} of {1}", 1, exportUnit.Files.Count));                    
				FileInfo file = exportUnit.Files[0];
				string destPath = string.Format("{0}\\{1}{2}", destDir.FullName, addOn.FileNamePrefix, file.Extension);
				int index = 1;

				while (File.Exists(destPath))
					destPath = string.Format("{0}\\{1}_{2}{3}", destDir.FullName, addOn.FileNamePrefix, index++, file.Extension);

				file.CopyTo(destPath);

                Progress_Changed(exportUnit, Convert.ToInt32(iProgress + 1 + (((0 + 1) * 65) / exportUnit.Files.Count)), string.Format("Done with copying file {0} of {1}", 1, exportUnit.Files.Count));                    
			}
			else
			{
				int index = 1;
				
				for (int i = 0; i < exportUnit.Files.Count; i++)
				{
                    Progress_Changed(exportUnit, Convert.ToInt32(iProgress + (((i + 1) * 65) / exportUnit.Files.Count)), string.Format("Copying file {0} of {1}", i + 1, exportUnit.Files.Count));                    

                    FileInfo file = exportUnit.Files[i];
					string destPath = string.Format("{0}\\{1}_{2:0000}{3}", destDir.FullName, addOn.FileNamePrefix, index++, file.Extension);

					while (File.Exists(destPath))
						destPath = string.Format("{0}\\{1}_{2:0000}{3}", destDir.FullName, addOn.FileNamePrefix, index++, file.Extension);

					file.CopyTo(destPath);

                    Progress_Changed(exportUnit, Convert.ToInt32(iProgress + 1 + (((i + 1) * 65) / exportUnit.Files.Count)), string.Format("Done with copying file {0} of {1}", i + 1, exportUnit.Files.Count));                    
				}
			}			

			//if (addOn.UpdateILLiad && exportUnit.Article.IllNumber.StartsWith("-") == false)
            if (addOn.UpdateILLiad)
			{
                Progress_Changed(exportUnit, 95, "File(s) copied successfully. Updating ILLiad...");
				this.Illiad.UpdateInfo(exportUnit, addOn.ChangeStatusToRequestFinished);
                Progress_Changed(exportUnit, 100, "ILLiad updated successfully.");
			}
            else
            {
                Progress_Changed(exportUnit, 100, "File(s) copied successfully.");
            }
		}
        #endregion

        #endregion


		//PRIVATE METHODS
		#region private methods

		#region CreateExportFiles()
		/*internal void CreateExportFiles(ExportUnit exportUnit)
		{
			BscanILL.Export.ExportFiles.ExportFilesCreator exportFilesCreator = new ExportFiles.ExportFilesCreator();
			exportFilesCreator.ProgressChanged += delegate(double progress) { Progress_Changed(exportUnit, Convert.ToInt32(progress * 50), null); };
			exportFilesCreator.DescriptionChanged += delegate(string description) { Progress_Comment(exportUnit, description); };
			
			exportFilesCreator.Create(null, exportUnit);
		}*/
		#endregion

        #region GetDestinationFolder()
        private DirectoryInfo GetDestinationFolder(AdditionalSaveOnDisk additional, Article article)
        {
            string subDirName = "";

            DirectoryInfo destDir;
            if (additional.SaveToSubfolder)
            {
                string destDirectory = "";
                if ((additional.SubfolderNameBase == BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.TransactionName) &&
                    (article.TransactionId != null))
                    subDirName = article.TransactionId.ToString();
                else if ((additional.SubfolderNameBase == BscanILL.SETTINGS.Settings.ExportClass.SaveOnDiskClass.SubfolderNameBasedOn.IllName) &&
                    (article.IllNumber != null && article.IllNumber.Trim().Length > 0))
                    subDirName = article.IllNumber.ToString();
                else
                    subDirName = article.Id.ToString();

                destDirectory = Path.Combine(additional.ExportDirectory, subDirName);
                destDir = new DirectoryInfo(destDirectory);
            }
            else
            {
                destDir = new DirectoryInfo(additional.ExportDirectory);
            }

            return destDir;
        }
        #endregion

		#endregion
	}
}
