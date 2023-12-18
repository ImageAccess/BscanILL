#define TransNumber_LONG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILLData.Models;
using BscanILLData.Models.Helpers;


namespace BscanILL.DB
{
	public class Database
	{
		static Database				instance = null;

		BscanILLData.Helpers.SQLite.ISQLiteHelper _sqliteHelper;
		BscanILL.Misc.Notifications notifications = BscanILL.Misc.Notifications.Instance;
		

		object threadLocker = new object();


		#region constructor
		private Database()
		{
			try
			{
				_sqliteHelper = new BscanILLData.Helpers.SQLite.SQLiteHelper(GetConnectionString());

				/*
				//dataEntries.ContextOptions.LazyLoadingEnabled = false;

				List<Article> articles = GetArticles();

				foreach (Article a in articles)
					foreach (Image image in a.Images)
						Console.WriteLine("Article: " + a.Id + ", Image: " + image.SubPath);

				Article article = GetArticle();

				foreach (Scan image in article.DbScans)
					Console.WriteLine("Article: " + article.Id + ", Image: " + image.FileName);
				;*/

                //_sqliteHelper.UpdateDatabaseTables(GetConnectionString());
#if TransNumber_LONG                
                _sqliteHelper.UpgradeDatabaseTables(GetConnectionString() , BscanILL.SETTINGS.Settings.Instance.General.DatabaseFilePath);  //check if TN field is in bigint format to support over 10 digits over 'int' type
#endif
				_sqliteHelper.AdjustDataOnStartup();
			}
			catch (Exception ex)
			{
				throw new Exception("SQL Database error: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public static Database Instance { get { return instance ?? (instance = new Database()); } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region SaveObject()
		public void SaveObject(DbArticle article)
		{
			_sqliteHelper.SaveObject(article);
		}
	
		public void SaveObject(DbScan scan)
		{
			_sqliteHelper.SaveObject(scan);
		}
	
		public void SaveObject(DbPage page)
		{
			_sqliteHelper.SaveObject(page);
		}

		public void SaveObject(DbExport export)
		{
			_sqliteHelper.SaveObject(export);
		}

		public void SaveObject(DbExportFile exportFile)
		{
			_sqliteHelper.SaveObject(exportFile);
		}
		#endregion

	
		#region GetArticles()
		public List<DbArticle> GetArticles()
		{
			var articles = _sqliteHelper.GetActiveArticles();
			
			return articles.ToList();
		}
		#endregion

		#region GetArticle()
		public DbArticle GetArticle(int id)
		{
			var article = _sqliteHelper.GetArticle(id);
			return article;
		}
		#endregion

		#region InsertArticle()
		public DbArticle InsertArticle(NewDbArticle newArticle)
		{
			var article = _sqliteHelper.InsertArticle(newArticle);
			return article;
		}
		#endregion

		#region SetArticleStatus()
		public void SetArticleStatus(DbArticle article, BscanILL.Hierarchy.ArticleStatus status)
		{
			_sqliteHelper.SetArticleStatus(article, (ArticleStatus)status);
		}
		#endregion


		#region GetPullslip()
		public DbScan GetPullslip(DbArticle dbArticle)
		{
			DbScan pullslip = _sqliteHelper.GetPullslip(dbArticle);

			return pullslip;
		}
		#endregion

		#region GetScans()
		public List<DbScan> GetScans(DbArticle dbArticle)
		{
			List<DbScan> list = _sqliteHelper.GetActiveScans(dbArticle);
			
			return list;
		}
		#endregion

		#region InsertPullslip()
		public DbScan InsertPullslip(NewDbPullslip newPullslip)
		{
			var pullslip = _sqliteHelper.InsertPullslip(newPullslip);
			return pullslip;
		}
		#endregion

		#region InsertScan()
		public DbScan InsertScan(NewDbScan newScan)
		{
			var scan = _sqliteHelper.InsertScan(newScan);
			return scan;
		}
		#endregion

		#region DeleteScan()
		public void DeleteScan(DbScan dbScan)
		{
			_sqliteHelper.DeleteScan(dbScan);
		}
		#endregion


		#region SetScanDeleteStatus()
		public void SetScanDeleteStatus(DbScan dbScan)
		{
			_sqliteHelper.SetScanDeleteStatus(dbScan);
		}
		#endregion

		#region ChangeScanStatus()
		public void ChangeScanStatus(DbScan dbScan, BscanILL.Hierarchy.ScanStatus scanStatus)
		{
			if (scanStatus == Hierarchy.ScanStatus.Deleted)
				//DeleteScan(dbScan);
				SetScanDeleteStatus(dbScan);
			else
				_sqliteHelper.SetScanStatus(dbScan, (ScanStatus) scanStatus);
		}
		#endregion


		#region GetPages()
		public List<DbPage> GetPages(DbArticle dbArticle)
		{
			List<DbPage> pages = _sqliteHelper.GetActivePages(dbArticle);

			return pages;
		}

		public List<DbPage> GetActiveDbPages(DbScan dbScan)
		{
			List<DbPage> pages = _sqliteHelper.GetActivePages(dbScan);

			return pages;
		}
		#endregion

		#region InsertPage()
		public DbPage InsertPage(NewDbPage newPage)
		{
			var scan = _sqliteHelper.InsertPage(newPage);
			return scan;
		}
		#endregion

		#region DeletePage()
		public void DeletePage(DbPage dbPage)
		{
			_sqliteHelper.DeletePage(dbPage);
		}
		#endregion

		#region ChangePageStatus()
		public void ChangePageStatus(DbPage dbPage, BscanILL.Hierarchy.PageStatus pageStatus)
		{
			if (pageStatus == Hierarchy.PageStatus.Deleted)
				DeletePage(dbPage);
			else
			{
				_sqliteHelper.SetPageStatus(dbPage, (PageStatus)pageStatus);
			}
		}
		#endregion


		#region GetExports()
		public IList<DbExport> GetExports(DbArticle dbArticle)
		{
			IList<DbExport> exports = _sqliteHelper.GetExports(dbArticle.Id);

			return exports.ToList();
		}
		#endregion

		#region InsertExport()
		public DbExport InsertExport(NewDbExport newExport)
		{
			var export = _sqliteHelper.InsertExport(newExport);

			return export;
		}
		#endregion

		#region SetExportStatus()
		public void SetExportStatus(DbExport dbExport, ExportStatus status)
		{
			_sqliteHelper.SetExportStatus(dbExport, status);
		}
		#endregion


		#region GetExportFiles()
		public IList<DbExportFile> GetExportFiles(DbExport dbExport)
		{
			IList<DbExportFile> exports = _sqliteHelper.GetExportFiles(dbExport.Id);

			return exports.ToList();
		}
		#endregion

		#region InsertExportFile()
		public DbExportFile InsertExportFile(NewDbExportFile newExportFile)
		{
			var export = _sqliteHelper.InsertExportFile(newExportFile);

			return export;
		}
		#endregion

		#region ChangeExportFileStatus()
		public void ChangeExportFileStatus(DbExportFile dbExportFile, BscanILL.Hierarchy.ExportFileStatus status)
		{
			_sqliteHelper.SetExportFileStatus(dbExportFile, (ExportFileStatus)status);
		}
		#endregion


		#region AddArticleCountsThread()
		public void AddArticleCountsThread(object obj)
		{
			try
			{
				lock (threadLocker)
				{
#if DEBUG
					DateTime start = DateTime.Now;
#endif

					BscanILL.UI.Frames.Resend.ArticlesLocal articles = (BscanILL.UI.Frames.Resend.ArticlesLocal)obj;

                    List<DbScan> allScans = GetScans(null);
                    List<DbPage> allPages = GetActiveDbPages(null);
                    int scanCount = 0;
                    int pageCount = 0;
                    DbScan scanFound = null;
                    DbScan scanTemp = null;

					foreach (BscanILL.UI.Frames.Resend.ArticleLocal article in articles)
					{      
                        //too slow for big database..
						//List<DbScan> scans = GetScans(article.Article);
						//article.ScansCount = scans.Count;

						//List<DbPage> pages = GetPages(article.Article);
						//article.PagesCount = pages.Count;
                        
                        scanCount = 0;
                        pageCount = 0;
                        scanFound = null;                        

                        //get first scan of current Article
                        foreach (DbScan scan in allScans)
                        {                            
                            if ((scan.fArticleId == article.Id) && (scan.PreviousId == null) && (scan.Status == ScanStatus.Active))
                            {
                                scanFound = scan;
                                break;
                            }
                        }

                        while (scanFound != null)
                        {
                            scanCount++;

                            foreach (DbPage page in allPages)
                            {
                                if (page.fScanId == scanFound.Id && page.Status == PageStatus.Active)
                              {
                                  pageCount++;
                              }
                            }

                            if (scanFound.NextId.HasValue)
                            {
                                scanTemp = scanFound;
                                scanFound = null;
                                foreach (DbScan scan in allScans)
                                {
                                    if (scan.Id == scanTemp.NextId)
                                    {
                                        scanFound = scan;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                scanFound = null;
                            }							
                        }


                        article.ScansCount = scanCount;
                        article.PagesCount = pageCount;
					}

#if DEBUG
					Console.WriteLine("Getting average monthly scans: " + DateTime.Now.Subtract(start).ToString());
#endif
				}
			}
			catch (Exception)
			{
			}
		}
		#endregion
	
		#endregion


		// PRIVATE METHODS
		#region private methods

		#region GetConnectionString()
		private static string GetConnectionString()
		{
			try
			{
				//string dbFile = BscanILL.SETTINGS.Settings.Instance.General.DatabaseFilePath;

				//System.IO.FileInfo file = new System.IO.FileInfo(dbFile);

				//file.Directory.Create();

				//if (System.IO.File.Exists(dbFile) == false)
				//{
					//System.IO.FileInfo source = new System.IO.FileInfo(Misc.Misc.StartupPath + @"\data\BscanILLData.db3");
					//source.CopyTo(dbFile);
				//}

				//string connectionString = "metadata=res://*/DB.Model.csdl|res://*/DB.Model.ssdl|res://*/DB.Model.msl;provider=System.Data.SQLite;provider connection string='data source=\"" + dbFile + "\"'";
				//string connectionString = "metadata=res://*/;provider=System.Data.SQLite;provider connection string='data source=\"" + dbFile + "\"'";			
				
				
				string connectionString = BscanILL.SETTINGS.Settings.Instance.General.SQLiteConnectionString;

				return connectionString;
			}
			catch (Exception ex)
			{
				throw new Exception("SQL Database connection styring error: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion

		#endregion

	}
}
