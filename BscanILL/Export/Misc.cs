using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Export.ILL;
using System.IO;
using BscanILL.Misc;

namespace BscanILL.Export
{

	public delegate void ProgressChangedHnd(double progress);
	public delegate void ProcessSuccessfullHnd(ExportUnit exportUnit);
	public delegate void ProcessErrorHnd(ExportUnit exportUnit, Exception ex);
	public delegate void ProgressDescriptionHnd(string description);

	public delegate void ExecutionStartedHandle();
	public delegate void ExecutionFinishedSuccessfullyHandle( int articleCount );
	//public delegate void ExecutionFinishedWithErrorHandle(Exception ex);
    public delegate void ExecutionFinishedWithErrorHandle();

	public delegate void ArticleExecutionStartedHandle(ExportUnit exportUnit);
	public delegate void ArticleExecutionSuccessfullHandle(ExportUnit exportUnit);
	public delegate void ArticleExecutionErrorHandle(ExportUnit exportUnit, IllException ex);

	public delegate void ProgressChangedHandle(ExportUnit exportUnit, int progress, string description);
	public delegate void ProgressCommentHandle(ExportUnit exportUnit, string comment);


	public class Misc
	{

		#region GetUniqueExportFilePath()
		internal static FileInfo GetUniqueExportFilePath(BscanILL.Export.ExportUnit exportUnit)
		{
			DirectoryInfo	dir = exportUnit.Directory;
			string			fileNameBase = exportUnit.FileNamePrefix;

			int			index = 1;
			string		extension = BscanILL.Misc.Io.GetFileExtension(exportUnit.FileFormat);
			string		fileName = fileNameBase + extension;

			while (File.Exists(dir.FullName + @"\" + fileName))
				fileName = string.Format("{0}_{1}{2}", fileNameBase, index++, extension);

			return new FileInfo(dir.FullName + @"\" + fileName);
		}
		#endregion

		#region GetTemporaryExportDir()
		/*internal static DirectoryInfo GetTemporaryExportDir(FileFormat fileFormat)
		{
			string dirPath = Settings.Instance.General.ExportDir + @"\";

			switch (fileFormat)
			{
				case FileFormat.SPdf: dirPath += "PDF"; break;
				case FileFormat.Pdf: dirPath += "PDF"; break;
				case FileFormat.Png: dirPath += "PNG"; break;
				case FileFormat.Text: dirPath += "Text"; break;
				case FileFormat.Audio: dirPath += "Audio"; break;
				case FileFormat.Jpeg: dirPath += "JPEG"; break;
				case FileFormat.Tiff: dirPath += "TIFF"; break;
				default: dirPath += "Unknown"; break;
			}

			DirectoryInfo dir = new DirectoryInfo(dirPath);

			dir.Refresh();
			dir.Create();

			return dir;
		}*/
		#endregion

		#region GetUniqueExportFilePrefix()
		internal static string GetUniqueExportFilePrefix(BscanILL.Scan.FileFormat format, bool multiImage)
		{
			switch (format)
			{
				case BscanILL.Scan.FileFormat.Audio:
					{
						if (multiImage)
							return "Bscan ILL Speech";
						else
							return "Bscan ILL Speech";
					}
				case BscanILL.Scan.FileFormat.Jpeg:
				case BscanILL.Scan.FileFormat.Png:
					{
						return "Bscan ILL Image";
					}
				case BscanILL.Scan.FileFormat.Pdf:
				case BscanILL.Scan.FileFormat.SPdf:
					{
						if (multiImage)
							return "Bscan ILL Document";
						else
							return "Bscan ILL Document";
					}
				case BscanILL.Scan.FileFormat.Text:
					{
						if (multiImage)
							return "Bscan ILL Text";
						else
							return "Bscan ILL Text";
					}
				case BscanILL.Scan.FileFormat.Tiff:
					{
						if (multiImage)
							return "Bscan ILL Images";
						else
							return "Bscan ILL Image";
					}
				default:
					return "Bscan ILL Image";
			}
		}
		#endregion

		#region GetExportTypeCaption()
		public static string GetExportTypeCaption(ExportType exportType)
		{
			switch (exportType)
			{
				case BscanILL.Export.ExportType.None: return "Bscan ILL Selection";
				case BscanILL.Export.ExportType.Odyssey: return "Odyssey";
				case BscanILL.Export.ExportType.Ariel: return "Ariel";
				case BscanILL.Export.ExportType.ArielPatron: return "Ariel";
				case BscanILL.Export.ExportType.Email: return "Email";
				case BscanILL.Export.ExportType.Ftp: return "FTP";
				case BscanILL.Export.ExportType.FtpDir: return "FTP Directory";
				case BscanILL.Export.ExportType.ILLiad: return "ILLiad";
				case BscanILL.Export.ExportType.SaveOnDisk: return "Save on Disk";
				case BscanILL.Export.ExportType.ArticleExchange: return "Article Exchange";
                case BscanILL.Export.ExportType.Tipasa: return "Tipasa";
                case BscanILL.Export.ExportType.WorldShareILL: return "WorldShare ILL";
				case BscanILL.Export.ExportType.Rapido: return "Rapido";
			}

			return exportType.ToString();
		}
		#endregion

	}
}
