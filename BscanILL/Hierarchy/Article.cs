#define TransNumber_LONG

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace BscanILL.Hierarchy
{
	public class Article : INotifyPropertyChanged
	{
		BscanILLData.Models.DbArticle		dbArticle;
		BscanILL.SETTINGS.Settings			_settings = BscanILL.SETTINGS.Settings.Instance;
		BscanILL.DB.Database				_database = BscanILL.DB.Database.Instance;

		BscanILL.Hierarchy.IllImage			pullslip = null;
		BscanILL.Hierarchy.IllImages		scans = new BscanILL.Hierarchy.IllImages();
		BscanILL.Export.ExportUnits			exportUnits = new BscanILL.Export.ExportUnits();

		int newFileIndex = -1;
		
		public event PropertyChangedEventHandler PropertyChanged;

        bool isLoadedinScan = false;

		#region constructor
		public Article(BscanILLData.Models.DbArticle dbArticle)
		{
			this.dbArticle = dbArticle;

			var pullslip = _database.GetPullslip(dbArticle);

			if (pullslip != null)
				this.pullslip = new Hierarchy.IllImage(this, pullslip);
                                                                       
			var dbScans = _database.GetScans(this.DbArticle);
			List<DbScanWithErrors> scansWithErrors = new List<DbScanWithErrors>();

			foreach(BscanILLData.Models.DbScan dbScan in dbScans)
			{
				try
				{
                    this.scans.Add(new Hierarchy.IllImage(this, dbScan));  
                }                                                         
				catch (Exception ex)
				{
					//this.dbArticle.DbScans.Remove(scan);
					DB.Database.Instance.DeleteScan(dbScan);
					scansWithErrors.Add(new DbScanWithErrors(dbScan, ex));
				}
			}

			if (scansWithErrors.Count > 0)
			{
				string message = string.Format("Scan {0}, {1} was removed from collection, because: {2}",
					scansWithErrors[0].DbScan.Id, scansWithErrors[0].DbScan.FileName, scansWithErrors[0].Exception.Message);

				for(int i = 1; i < scansWithErrors.Count; i++)
					message += string.Format("\nScan {0}, {1} was removed from collection, because: {2}",
					scansWithErrors[i].DbScan.Id, scansWithErrors[i].DbScan.FileName, scansWithErrors[i].Exception.Message);

				MessageBox.Show(message, "Bscan ILL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			this.Directory.Create();
			System.IO.Directory.CreateDirectory(this.ScansPath);
			System.IO.Directory.CreateDirectory(this.PagesPath);

			foreach (BscanILLData.Models.DbExport dbExport in  _database.GetExports(this.dbArticle))
				this.exportUnits.Add(new BscanILL.Export.ExportUnit(this, dbExport));

			this.dbArticle.LastModifiedDate = DateTime.Now;
			_database.SaveObject(this.dbArticle);
		}
		#endregion


		#region DbScanWithErrors
		class DbScanWithErrors
		{
			public readonly BscanILLData.Models.DbScan DbScan;
			public readonly Exception Exception;

			public DbScanWithErrors(BscanILLData.Models.DbScan dbScan, Exception ex)
			{
				this.DbScan = dbScan;
				this.Exception = ex;
			}
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public IllImage Pullslip { get { return this.pullslip; } }

		public BscanILL.Hierarchy.IllImages Scans { get { return this.scans; } }
		public BscanILL.Export.ExportUnits	ExportUnits { get { return this.exportUnits; } }

		public string ScansPath { get { return this.Directory.FullName + @"\Scans\"; } }		
		public string PagesPath { get { return this.Directory.FullName + @"\Pages\"; } }
		public string ExportsPath { get { return this.Directory.FullName + @"\Exports\"; } }

		public BscanILLData.Models.DbArticle		DbArticle		{ get { return this.dbArticle; } }

		#region Id
		public long Id 
		{
			get { return this.dbArticle.Id; }
		}
		#endregion

		#region TransactionId

#if TransNumber_LONG
        public long? TransactionId
#else
		public int? TransactionId
#endif        
		{
#if TransNumber_LONG
			get { return this.dbArticle.TransactionNumberBig; }
#else
            get { return this.dbArticle.TransactionNumber; }
#endif
            set 
			{                
#if TransNumber_LONG
                this.dbArticle.TransactionNumberBig = value;
#else
                this.dbArticle.TransactionNumber = value;
#endif

                _database.SaveObject(this.dbArticle);
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region IllNumber
		public string IllNumber
		{
			get { return this.dbArticle.IllNumber; }
			set
			{
				this.dbArticle.IllNumber = value;
				_database.SaveObject(this.dbArticle);
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Patron
		public string Patron
		{
			get { return this.dbArticle.Patron; }
			set
			{
				this.dbArticle.Patron = value;
				_database.SaveObject(this.dbArticle);
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Address
		public string Address
		{
			get { return this.dbArticle.Address; }
			set
			{
				this.dbArticle.Address = value;
				_database.SaveObject(this.dbArticle);
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Directory
		public DirectoryInfo Directory
		{
			get { return new DirectoryInfo(Path.Combine(_settings.General.ArticlesDir, this.dbArticle.FolderName)); }
		}
		#endregion

		#region ExportType
		public BscanILL.Export.ExportType ExportType
		{
			get { return (BscanILL.Export.ExportType)this.dbArticle.ExportType; }
			set
			{
				this.dbArticle.ExportType = (BscanILLData.Models.ExportType)value;
				_database.SaveObject(this.dbArticle);
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ExportScans
		public bool ExportScans
		{
			get { return this.dbArticle.ExportScans; }
			set
			{
				this.dbArticle.ExportScans = value;
				_database.SaveObject(this.dbArticle);
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Status
		public ArticleStatus Status
		{
			get { return (ArticleStatus)dbArticle.Status; }
			set
			{
				if(this.dbArticle.Status != (BscanILLData.Models.ArticleStatus)value)
				{
					_database.SetArticleStatus(this.dbArticle, value);
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion


        #region IsLoadedInScan
        public bool IsLoadedInScan 
		{
            get { return this.isLoadedinScan; }
            set { this.isLoadedinScan = value; }
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region GetArticleFromFile()
		/*public static Article GetArticleFromFile(string filePath)
		{
			try
			{
				using (StreamReader reader = new StreamReader(filePath))
				{
					string line;

					if ((line = reader.ReadLine()) != null)
						return new Article(line);
				}
			}
			catch { }

			return null;
		}*/
		#endregion

		#region SetExportResult()
		/*public void SetExportResult(ErrorCode errorCode, string errorMessage)
		{
			this.errorCode = errorCode;
			this.errorMessage = errorMessage;
		}*/
		#endregion

		#region Set()

#if TransNumber_LONG
        public void Set(long? transactionId, string illNumber, string patron, string address, BscanILL.Export.ExportType exportType)
#else
		public void Set(int? transactionId, string illNumber, string patron, string address, BscanILL.Export.ExportType exportType)
#endif        
		{            
#if TransNumber_LONG
            this.dbArticle.TransactionNumberBig = transactionId;
#else
            this.dbArticle.TransactionNumber = transactionId;
#endif

            this.dbArticle.IllNumber = illNumber;
			this.dbArticle.Patron = patron;
			this.dbArticle.Address = address;
			this.dbArticle.ExportType = (BscanILLData.Models.ExportType) exportType;

			_database.SaveObject(this.dbArticle);

			RaisePropertyChanged("TransactionId");
			RaisePropertyChanged("IllNumber");
			RaisePropertyChanged("Patron");
			RaisePropertyChanged("Address");
			RaisePropertyChanged("ExportType");
		}
		#endregion

        #region SetExportType()
        public void SetExportType(BscanILL.Export.ExportType exportType)
        {
            this.dbArticle.ExportType = (BscanILLData.Models.ExportType)exportType;
            _database.SaveObject(this.dbArticle);
            RaisePropertyChanged("ExportType");
        }
        #endregion

        #region GetScans()
        public BscanILL.Hierarchy.IllImages GetScans(bool includePullslip)
        {
            BscanILL.Hierarchy.IllImages images = new Hierarchy.IllImages();

            if (includePullslip && this.Pullslip != null)                
            {
                if ((this.Pullslip.Status == ScanStatus.Active) || (this.Pullslip.Status == ScanStatus.Pullslip))
                {
                   images.Add(this.Pullslip);
                }
            }

            foreach (BscanILL.Hierarchy.IllImage illImage in this.Scans)                
            {
                if (illImage.Status == ScanStatus.Active)
                {
                   images.Add(illImage);
                }
            }

            return images;
        }
        #endregion

        #region GetPages()
        public BscanILL.Hierarchy.IllPages GetPages(bool includePullslip)
		{
			BscanILL.Hierarchy.IllPages pages = new Hierarchy.IllPages();

			if (includePullslip && this.Pullslip != null && this.Pullslip.IllPages.Count > 0)
                foreach (BscanILL.Hierarchy.IllPage page in this.Pullslip.IllPages)
                {
                    if (page.Status == PageStatus.Active)
                    {
                      pages.Add(page);
                    }
                }

			foreach (BscanILL.Hierarchy.IllImage illImage in this.Scans)
                foreach (BscanILL.Hierarchy.IllPage page in illImage.IllPages)
                {
                    if (page.Status == PageStatus.Active)
                    {
                        pages.Add(page);
                    }
                }

			return pages;
		}
		#endregion

		#region GetIdenticalScanPath()
		public string GetIdenticalScanPath(Scanners.FileFormat fileFormat)
		{
			if (this.newFileIndex < 0)
			{
				string[] files = System.IO.Directory.GetFiles(this.ScansPath);

				foreach (string file in files)
				{
					try
					{
						Path.GetFileNameWithoutExtension(file);
						int index;
						if (int.TryParse(Path.GetFileNameWithoutExtension(file), out index) && newFileIndex < index)
							newFileIndex = index;
					}
					catch { }
				}
			}

			this.newFileIndex++;

			return Path.Combine(this.ScansPath, (this.newFileIndex + 1).ToString("00000000") + BscanILL.Misc.Io.GetFileExtension(fileFormat));
		}
		#endregion

		#region GetIdenticalPagesFolder()
		/*public System.IO.DirectoryInfo GetIdenticalPagesFolder()
		{
			int			index = 0;
			string		path = Path.Combine(this.PagesPath, index.ToString("000000"));

			while (System.IO.Directory.Exists(path))
			{
				index++;
				path = Path.Combine(this.PagesPath, index.ToString("000000"));
			}

			return new System.IO.DirectoryInfo(path);
		}*/
		#endregion

		#region AddScanPullslip()
		public void AddScanPullslip(string file, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, ushort dpi)
		{
			BscanILLData.Models.Helpers.NewDbPullslip newPullslip = new BscanILLData.Models.Helpers.NewDbPullslip()
			{
				fArticleId = (int)this.Id,
				FileName = Path.GetFileName(file),
				ColorMode = (BscanILLData.Models.ColorMode)colorMode,
				FileFormat = (BscanILLData.Models.ScanFileFormat)fileFormat,
				Dpi = (short)dpi
			};

			BscanILLData.Models.DbScan dbScan = _database.InsertPullslip(newPullslip);

			this.pullslip = new IllImage(this, dbScan);
			this.pullslip.ImageSelected += new BscanILL.Hierarchy.IllImageHnd(Image_Selected);
		}
		#endregion

		#region AddScanPullslip()
		/*public void AddScanPullslip(BscanILLData.Models.DbScan dbScan)
		{
			this.DbArticle.DbScans.Add(dbScan);

			this.pullslip = new IllImage(this, dbScan);
			this.pullslip.ImageSelected += new BscanILL.Hierarchy.IllImageHnd(Image_Selected);
		}*/
		#endregion
	
		#region DeletePages()
		public void DeletePages(bool deleteOnBackground)
		{
            if (this.pullslip != null)
            {
                this.pullslip.DeletePages(deleteOnBackground);                
            }
			
			foreach (IllImage scan in this.Scans)
				scan.DeletePages(deleteOnBackground);
		}
		#endregion

		#region Delete()
		/// <summary>
		/// changes database status to Deleted and deletes all the files from hard drive
		/// </summary>
		public void Delete()
		{
			StopAllAutomatedProcessing();
			//DeletePages(true);
			//this.Scans.Clear();

			_database.SetArticleStatus(this.dbArticle, ArticleStatus.Deleted);

			try { this.Directory.Delete(true); }
                
#if DEBUG
			catch (Exception ex)
			{
				Console.WriteLine(BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
#else
			catch{}
#endif
                  


		}
		#endregion

		#region GetNewExportFolderName()
		public DirectoryInfo GetNewExportFolderName()
		{
			DirectoryInfo exportsDir = new DirectoryInfo(ExportsPath);
			exportsDir.Create();

			DirectoryInfo[] subDirs = exportsDir.GetDirectories();
			int maxIndex = 0;

			foreach (DirectoryInfo subDir in subDirs)
			{
				try
				{
					int dirNumber;
					if(int.TryParse(subDir.Name, out dirNumber) && maxIndex < dirNumber)
						maxIndex = dirNumber;
				}
				catch { }
			}

			return new DirectoryInfo(Path.Combine(exportsDir.FullName, ( maxIndex+1).ToString("00000000")));
		}
		#endregion

		#region StopAllAutomatedProcessing()
		public void StopAllAutomatedProcessing()
		{
			foreach (BscanILL.Hierarchy.IllImage scan in scans)
				scan.StopAllAutoProcessing();
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region RaisePropertyChanged()
		/// <summary>
		/// with get_
		/// </summary>
		/// <param name="propertyName"></param>
		private void RaisePropertyChanged(string propertyName)
		{
			if (propertyName.StartsWith("get_") || propertyName.StartsWith("set_"))
				propertyName = propertyName.Substring(4);
			
			if (this.PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		#region Image_Selected()
		private void Image_Selected(BscanILL.Hierarchy.IllImage illImage)
		{
			//if (ImageSelected != null)
			//	ImageSelected(illImage);
		}
		#endregion

		#endregion

	}

}

