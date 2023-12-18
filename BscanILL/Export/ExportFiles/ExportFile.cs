using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BscanILL.Scan;
using System.ComponentModel;

namespace BscanILL.Export.ExportFiles
{
	class ExportFile : IDisposable, INotifyPropertyChanged
	{
		BscanILL.Export.ExportUnit			exportUnit;
		BscanILLData.Models.DbExportFile	dbExportFile;	
		FileInfo							file = null;

		public event PropertyChangedEventHandler PropertyChanged;


		#region constructor
		public ExportFile(BscanILL.Export.ExportUnit exportUnit, BscanILLData.Models.DbExportFile dbExportFile)
		{
			this.exportUnit = exportUnit;
			this.dbExportFile = dbExportFile;
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public long								Id { get { return this.exportUnit.pExportId; } }
		public FileFormat						FileFormat	{ get { return (FileFormat)dbExportFile.FileFormat; } }
		public BscanILLData.Models.DbExportFile DbExportFile { get { return this.dbExportFile; } }

		public FileInfo File
		{
			get
			{
				if (this.file == null)
					this.file = new FileInfo(Path.Combine(exportUnit.Directory.FullName, dbExportFile.FileName));

				return this.file;
			}
		}

		#region Status
		public BscanILL.Hierarchy.ExportFileStatus Status
		{
			get { return (BscanILL.Hierarchy.ExportFileStatus)dbExportFile.Status; }
			set
			{
				BscanILL.DB.Database.Instance.ChangeExportFileStatus(this.dbExportFile, value);
				RaisePropertyChanged(System.Reflection.MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
		}
		#endregion

		#region Delete()
		public void Delete()
		{
			try
			{
				this.File.Refresh();

				if (this.File.Exists)
					this.File.Delete();
			}
			catch { }
		}
		#endregion

		#endregion



		// PRIVATE METHODS
		#region private methods

		#region RaisePropertyChanged()
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
