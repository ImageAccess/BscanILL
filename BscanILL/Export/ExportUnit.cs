using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using BscanILL.Export.AdditionalInfo;
using BscanILL.Hierarchy;

namespace BscanILL.Export
{
	public class ExportUnit : INotifyPropertyChanged
	{
		BscanILL.Hierarchy.Article					article;
		BscanILLData.Models.DbExport				dbExport;
		BscanILL.SETTINGS.Settings					_settings = BscanILL.SETTINGS.Settings.Instance;
		List<string>								warnings = new List<string>();
		IAdditionalInfo								additionalInfo = null;
		BscanILL.Export.ExportFiles.ExportFiles		exportFiles = new ExportFiles.ExportFiles();
		BscanILL.DB.Database						_database = BscanILL.DB.Database.Instance;

		public event PropertyChangedEventHandler PropertyChanged;


		#region constructor
		public ExportUnit(Article article, BscanILLData.Models.DbExport dbExport)
		{
			this.article = article;
			this.dbExport = dbExport;

			string fileNamePrefix = this.article.TransactionId.HasValue ? this.article.TransactionId.Value.ToString() : null;

			if(fileNamePrefix == null)
				fileNamePrefix = this.article.IllNumber;

			if(fileNamePrefix == null)
				fileNamePrefix = this.fArticleId.ToString();

			bool multiImage = (this.FileFormat == Scan.FileFormat.SPdf || this.FileFormat == Scan.FileFormat.Pdf ||
				this.FileFormat == Scan.FileFormat.Audio || this.FileFormat == Scan.FileFormat.Text || this.FileFormat == Scan.FileFormat.Tiff);

			foreach (BscanILLData.Models.DbExportFile dbExportFile in _database.GetExportFiles(this.dbExport))
				this.exportFiles.Add(new BscanILL.Export.ExportFiles.ExportFile(this, dbExportFile));
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public Article										Article { get { return this.article; } }
		public List<string>									Warnings { get { return warnings; } }
		public IAdditionalInfo								AdditionalInfo { get { return this.additionalInfo; } set { this.additionalInfo = value; } }

		public BscanILL.Hierarchy.IllPages					IllPages 
		{ 
			get 
			{
				if (this.AdditionalInfo != null)
					return article.GetPages(this.AdditionalInfo.IncludePullslip);
				else
					return article.GetPages(true);
			} 
		}

		#region pExportId
		public long pExportId
        {
			get { return dbExport.Id; }
        }
		#endregion

		#region fArticleId
		public long fArticleId
        {
			get { return dbExport.fArticleId; }
        }
		#endregion

		#region ExportType
		public ExportType ExportType
		{
			get { return (BscanILL.Export.ExportType)this.dbExport.ExportType; }
			set
			{
				this.dbExport.ExportType = (BscanILLData.Models.ExportType)value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ExportDate
		public System.DateTime ExportDate
        {
			get { return dbExport.ExportDate; }
			set
			{
				this.dbExport.ExportDate = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Directory
		public DirectoryInfo Directory
        {
			get { return new DirectoryInfo(Path.Combine(this.article.ExportsPath, this.dbExport.FolderName)); }
		}
		#endregion

		#region FileFormat
		public BscanILL.Scan.FileFormat FileFormat
        {
			get { return (BscanILL.Scan.FileFormat)dbExport.FileFormat; }
			set
			{
				this.dbExport.FileFormat = (BscanILLData.Models.ExportFileFormat)value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
        }
		#endregion

		#region FileNamePrefix
		public string FileNamePrefix
		{
			get { return dbExport.FileNamePrefix; }
			set
			{
				this.dbExport.FileNamePrefix = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region PdfA
		public bool PdfA
		{
			get { return dbExport.PdfA; }
			set
			{
				this.dbExport.PdfA = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region MultiImage
		public bool MultiImage
		{
			get { return dbExport.MultiImage; }
			set
			{
				this.dbExport.MultiImage = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Status
		public ExportStatus Status
        {
			get { return (ExportStatus)dbExport.Status; }
			set
			{
				BscanILL.DB.Database.Instance.SetExportStatus(this.dbExport, (BscanILLData.Models.ExportStatus)value);
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Files
		public List<FileInfo> Files
		{
			get
			{
				DirectoryInfo dir = this.Directory;
				List<FileInfo> list = new List<FileInfo>();

				foreach (BscanILLData.Models.DbExportFile exportFile in _database.GetExportFiles(this.dbExport))
					list.Add(new FileInfo(Path.Combine(dir.FullName, exportFile.FileName)));

				return list;
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region AddExportFile()
		public void AddExportFile(Scanners.ColorMode colorMode, ushort dpi, BscanILL.Scan.FileFormat fileFormat, FileInfo file, short pagesCount)
		{
			BscanILL.Export.ExportFiles.ExportFile previous = (this.exportFiles.Count > 0) ?  this.exportFiles[this.exportFiles.Count - 1] : null;

			BscanILLData.Models.Helpers.NewDbExportFile newDbExportFile = new BscanILLData.Models.Helpers.NewDbExportFile()
			{
				ColorMode = (BscanILLData.Models.ColorMode)colorMode,
				fExportId = this.dbExport.Id,
				Dpi = (short)dpi,
				FileFormat = (BscanILLData.Models.ExportFileFormat)fileFormat,
				FileName = file.Name,
				NumOfImages = pagesCount,
				PreviousId = (previous != null) ? (int?)previous.Id : (int?)null,
				Status = BscanILLData.Models.ExportFileStatus.Creating
			};

			BscanILLData.Models.DbExportFile dbExportFile = BscanILL.DB.Database.Instance.InsertExportFile(newDbExportFile);

			BscanILL.Export.ExportFiles.ExportFile exportFile = new BscanILL.Export.ExportFiles.ExportFile(this, dbExportFile);
			this.exportFiles.Add(exportFile);

			if (previous != null)
			{
				previous.DbExportFile.NextId = dbExportFile.Id;
				BscanILL.DB.Database.Instance.SaveObject(previous.DbExportFile);
			}

			exportFile.Status = ExportFileStatus.Active;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region RaisePropertyChanged()>
		private void RaisePropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				if (propertyName.StartsWith("get_") || propertyName.StartsWith("set_"))
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName.Substring(4)));
				else
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion

		#endregion


	}
}
