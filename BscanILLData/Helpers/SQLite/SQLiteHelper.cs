#define TransNumber_LONG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BscanILLData.Models;
using BscanILLData.Models.Helpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;


namespace BscanILLData.Helpers.SQLite
{
	public class SQLiteHelper : NHibernateHelper, ISQLiteHelper
	{
		object _threadLocker = new object();


		#region constructor
		public SQLiteHelper(string connectionString)
			: base(BscanILLData.Models.SqlEngine.SQLite, connectionString)
		{
			//Test();
		}
		#endregion


		//PUBLIC METHODS
		#region public methods

        #region UpdateDatabaseTables()
        //converts TN column type from int to bigint to carry bigget numbers
        //!! cannot use this method as SQLite does not support Drop Column in ALTER TABLE  !!
        public void UpdateDatabaseTables(string connectionString)
        {
            lock (_threadLocker)
            {
                System.Data.SQLite.SQLiteConnection sqlConnection = new System.Data.SQLite.SQLiteConnection();
                sqlConnection.ConnectionString = connectionString;
                sqlConnection.DefaultTimeout = 30;

                try
                {
                    sqlConnection.Open();

                    System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection);
                    command.CommandText = "PRAGMA foreign_keys = ON;";
                    command.ExecuteNonQuery();

                    string articlesTableName = "DbArticles";
                    string columnName = "TransactionNumber";
                    //string newColumnType = "nvarchar(16)";
                    string newColumnType = "bigint";

                    string columnType = GetTableColumnType(sqlConnection, articlesTableName, columnName);
                    if (String.Compare(columnType, newColumnType, true) != 0)
                    {
                        //if TransactionTable is not string type (it is old int type) - convert
                        ConvertColumnType(sqlConnection, articlesTableName, columnName, newColumnType);
                        // List<string> articlesTableColumnNames2 = GetTableColumnNames(sqlConnection, articlesTableName);
                    }
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
        }
        #endregion

        #region UpgradeDatabaseTables()
        //to make sure TN field is in bigint format in database to support over 10 digits (more than 'int' type does)
        //it adds new bigint column 'TransactionNumberBig' into database  if needed
        public void UpgradeDatabaseTables(string connectionString, string databasePath)
        {
            lock (_threadLocker)
            {
                System.Data.SQLite.SQLiteConnection sqlConnection = new System.Data.SQLite.SQLiteConnection();
                sqlConnection.ConnectionString = connectionString;
                sqlConnection.DefaultTimeout = 30;

                try
                {
                    sqlConnection.Open();

                    System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection);
                    command.CommandText = "PRAGMA foreign_keys = ON;";
                    command.ExecuteNonQuery();

                    //string articlesTableName = "DbArticles";
                    //string columnName = "TransactionNumber";
                    //string newColumnType = "nvarchar(16)";
                    //string newColumnType = "bigint";
                    
                    if( ! TableColumnNameExists (sqlConnection, "DbArticles", "TransactionNumberBig") )
                    {                         
                        //backup Database file
                        BackupDatabaseFile(databasePath);

                        AddTableColumn(sqlConnection, "DbArticles", "TransactionNumberBig", "bigint", "TransactionNumber");                        
                    }
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
        }
        #endregion

		#region TestConnection()
		public void TestConnection(string connectionString)
		{
			lock (_threadLocker)
			{
				CreateSQLiteDatabaseFile(connectionString);

				System.Data.SQLite.SQLiteConnection sqlConnection = new System.Data.SQLite.SQLiteConnection(connectionString);

				sqlConnection.Open();
				sqlConnection.Close();
				sqlConnection.Dispose();
			}
		}
		#endregion

		#region Test()
		public void Test()
		{
			/*			
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{

							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
			*/
		}
		#endregion
		
		#region AdjustDataOnStartup()
		public void AdjustDataOnStartup()
		{
			/*lock (_threadLocker)
			{
				using (IStatelessSession session = this.SessionFactory.OpenStatelessSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							FillInGuids(session);					
							SetUserSessionsClosed(session);
							SetUserSessionsStartAndEnd(session);

							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							throw;
						}
					}
				}
			}*/
		}
		#endregion

		#region SaveObject()
		public void SaveObject(object obj)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							session.SaveOrUpdate(obj);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion


		#region GetActiveArticles()
		public IList<BscanILLData.Models.DbArticle> GetActiveArticles()
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							IList<BscanILLData.Models.DbArticle> list = session.QueryOver<BscanILLData.Models.DbArticle>()
								.Where(x => x.Status == ArticleStatus.Active)
								.List();

							transaction.Commit();
							return list;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region GetArticle()
		public DbArticle GetArticle(int id)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							var article = session.QueryOver<BscanILLData.Models.DbArticle>()
												.Where(p => (p.Id == id))
												.SingleOrDefault();

							transaction.Commit();
							return article;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region InsertArticle()
		public DbArticle InsertArticle(NewDbArticle newArticle)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							DateTime now = DateTime.Now;

							BscanILLData.Models.DbArticle article = new DbArticle()
							{
#if TransNumber_LONG
                                TransactionNumberBig = newArticle.TransactionNumberBig,
#else
								TransactionNumber = newArticle.TransactionNumber,
#endif
								IllNumber = newArticle.IllNumber,
								Patron = newArticle.Patron,
								Address = newArticle.Address,
								ExportType = newArticle.ExportType,
								CreationDate = now,
								LastModifiedDate = now,
								FolderName = newArticle.FolderName,
								ExportScans = newArticle.ExportScans,
								Status = newArticle.Status
							};


							session.Save(article);
							transaction.Commit();

							return article;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region SetArticleStatus()
		public void SetArticleStatus(DbArticle article, ArticleStatus status)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							article.Status = status;

							session.SaveOrUpdate(article);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion


		#region GetPullslip()
		public DbScan GetPullslip(DbArticle dbArticle)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							var scan = session.QueryOver<BscanILLData.Models.DbScan>()
												.Where(p => (p.fArticleId == dbArticle.Id && p.PreviousId == null && p.Status == ScanStatus.Pullslip))
												.SingleOrDefault();

							transaction.Commit();
							return scan;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region GetActiveScans()
		public List<DbScan> GetActiveScans(DbArticle dbArticle)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							List<DbScan> list = new List<DbScan>();
                            if ( dbArticle != null )
                            {
							    var scan = session.QueryOver<BscanILLData.Models.DbScan>()
												    .Where(p => (p.fArticleId == dbArticle.Id && p.PreviousId == null && p.Status == ScanStatus.Active))                                                
												    .SingleOrDefault();

							    /*var results = session.QueryOver<User>(() => showAlias)
								    .Where(Restrictions.Or(
									    Restrictions.Where<Show>(s => s.Active),
									    Restrictions.And(
										    Restrictions.Where<Show>(s => s.ShowDate > DateTime.Now),
										    Subqueries.WhereExists(existing))))
								    .List();*/

							    while (scan != null)
							    {
								    list.Add(scan);

								    if (scan.NextId.HasValue)
									    scan = session.QueryOver<BscanILLData.Models.DbScan>()
										    .Where(p => (p.Id == scan.NextId))
										    .SingleOrDefault();
								    else
									    scan = null;
							    }
                            }
                            else
                            {
                                //get allactive DbScan records                                
                                IList<BscanILLData.Models.DbScan> dbScan = session.QueryOver<BscanILLData.Models.DbScan>()
                                                            .Where(p => (p.Status == ScanStatus.Active)).List();

                                if (dbScan.Count > 0)
                                {
                                    list.AddRange(dbScan);
                                }
                               
                            }
							transaction.Commit();
							return list;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region InsertPullslip()
		public DbScan InsertPullslip(NewDbPullslip newPullslip)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							DateTime now = DateTime.Now;

							BscanILLData.Models.DbScan pullslip = new DbScan()
							{
								fArticleId = newPullslip.fArticleId,
								PreviousId = null,
								NextId = null,
								FileName = newPullslip.FileName,
								ColorMode = newPullslip.ColorMode,
								FileFormat = newPullslip.FileFormat,
								Dpi = newPullslip.Dpi,
								Status = ScanStatus.Pullslip
							};

							session.Save(pullslip);
							transaction.Commit();

							return pullslip;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region InsertScan()
		public DbScan InsertScan(NewDbScan newScan)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							DateTime now = DateTime.Now;

							BscanILLData.Models.DbScan scan = new DbScan()
							{
								fArticleId = newScan.fArticleId,
								PreviousId = newScan.PreviousId,
								NextId = newScan.NextId,
								FileName = newScan.FileName,
								ColorMode = newScan.ColorMode,
								FileFormat = newScan.FileFormat,
								Dpi = newScan.Dpi,
								Status = newScan.Status
							};

							session.Save(scan);
							transaction.Commit();

							return scan;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region SetScanDeleteStatus()
		public void SetScanDeleteStatus(DbScan dbScan)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{							
							dbScan.Status = ScanStatus.Deleted;

							session.SaveOrUpdate(dbScan);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region DeleteScan()
		public void DeleteScan(DbScan dbScan)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							DbScan previous = null;
							DbScan next = null;

							if (dbScan.PreviousId != null)
							{
								previous = session.QueryOver<BscanILLData.Models.DbScan>()
									.Where(c => (c.Status == ScanStatus.Active && c.Id == dbScan.PreviousId))
									.SingleOrDefault();
							}

							if (dbScan.NextId != null)
							{
								next = session.QueryOver<BscanILLData.Models.DbScan>()
									.Where(c => c.Status == ScanStatus.Active && c.Id == dbScan.NextId)
									.SingleOrDefault();
							}

							if (previous != null && next != null)
							{
								previous.NextId = next.Id;
								next.PreviousId = previous.Id;
								session.SaveOrUpdate(previous);
								session.SaveOrUpdate(next);
							}
							else if (previous != null)
							{
								previous.NextId = null;
								session.SaveOrUpdate(previous);
							}
							else if (next != null)
							{
								next.PreviousId = null;
								session.SaveOrUpdate(next);
							}

							dbScan.Status = ScanStatus.Deleted;

							session.SaveOrUpdate(dbScan);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region SetScanStatus()
		public void SetScanStatus(DbScan scan, ScanStatus status)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							scan.Status = status;

							session.SaveOrUpdate(scan);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion
	

		#region GetActivePages()
		public List<DbPage> GetActivePages(DbArticle dbArticle)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							List<DbPage> list = new List<DbPage>();
							DbScan scan = session.QueryOver<BscanILLData.Models.DbScan>()
												.Where(p => (p.fArticleId == dbArticle.Id && p.PreviousId == null && p.Status == ScanStatus.Active))
												.SingleOrDefault();

							while (scan != null)
							{
								List<DbPage> scanPages = GetActiveDbPages(session, scan);

								list.AddRange(scanPages);

								if (scan.NextId.HasValue)
									scan = session.QueryOver<BscanILLData.Models.DbScan>()
										.Where(p => (p.Id == scan.NextId))
										.SingleOrDefault();
								else
									scan = null;
							}

							transaction.Commit();
							return list;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}

		public List<DbPage> GetActivePages(DbScan dbScan)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							List<DbPage> scanPages = GetActiveDbPages(session, dbScan);

							transaction.Commit();
							return scanPages;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region InsertPage()
		public DbPage InsertPage(NewDbPage newPage)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							DateTime now = DateTime.Now;

							BscanILLData.Models.DbPage page = new DbPage()
							{
								fScanId = newPage.fScanId,
								PreviousId = newPage.PreviousId,
								NextId = newPage.NextId,
								FileName = newPage.FileName,
								ColorMode = newPage.ColorMode,
								FileFormat = newPage.FileFormat,
								Dpi = newPage.Dpi,
								Status = newPage.Status
							};

							session.Save(page);
							transaction.Commit();

							return page;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion
	
		#region DeletePage()
		public void DeletePage(DbPage dbPage)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							DbPage previous = null;
							DbPage next = null;

							if (dbPage.PreviousId != null)
							{
								previous = session.QueryOver<BscanILLData.Models.DbPage>()
									.Where(c => (c.Status == PageStatus.Active && c.Id == dbPage.PreviousId))
									.SingleOrDefault();
							}

							if (dbPage.NextId != null)
							{
								next = session.QueryOver<BscanILLData.Models.DbPage>()
									.Where(c => (c.Status == PageStatus.Active && c.Id == dbPage.NextId))
									.SingleOrDefault();
							}

							if (previous != null && next != null)
							{
								previous.NextId = next.Id;
								next.PreviousId = previous.Id;
								session.SaveOrUpdate(previous);
								session.SaveOrUpdate(next);
							}
							else if (previous != null)
							{
								previous.NextId = null;
								session.SaveOrUpdate(previous);
							}
							else if (next != null)
							{
								next.PreviousId = null;
								session.SaveOrUpdate(next);
							}

							dbPage.Status = PageStatus.Deleted;

							session.SaveOrUpdate(dbPage);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region SetPageStatus()
		public void SetPageStatus(DbPage page, PageStatus status)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							page.Status = status;

							session.Update(page);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region GetExports()
		public IList<BscanILLData.Models.DbExport> GetExports(int articleId)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							IList<BscanILLData.Models.DbExport> list = session.QueryOver<BscanILLData.Models.DbExport>()
								.Where(x => x.fArticleId == articleId)
								.List();

							transaction.Commit();
							return list;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion


		#region InsertExport()
		public DbExport InsertExport(NewDbExport newExport)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							DateTime now = DateTime.Now;

							BscanILLData.Models.DbExport export = new DbExport()
							{
								fArticleId = newExport.fArticleId,
								ExportType = newExport.ExportType,
								ExportDate = now,
								FolderName = newExport.FolderName,
								FileFormat = newExport.FileFormat,
								FileNamePrefix = newExport.FileNamePrefix,
								PdfA = newExport.PdfA,
								MultiImage = newExport.MultiImage,
								Status = newExport.Status
							};

							session.Save(export);
							transaction.Commit();

							return export;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region SetExportStatus()
		public void SetExportStatus(DbExport export, ExportStatus status)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							export.Status = status;

							session.SaveOrUpdate(export);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion


		#region GetExportFiles()
		public List<DbExportFile> GetExportFiles(int exportId)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							List<DbExportFile> list = new List<DbExportFile>();
							DbExportFile exportFile = session.QueryOver<BscanILLData.Models.DbExportFile>()
												.Where(p => (p.fExportId == exportId && p.PreviousId == null && (p.Status == ExportFileStatus.Active)))
												.SingleOrDefault();

							while (exportFile != null)
							{
								list.Add(exportFile);

								if (exportFile.NextId.HasValue)
									exportFile = session.QueryOver<BscanILLData.Models.DbExportFile>()
										.Where(p => (p.Id == exportFile.NextId))
										.SingleOrDefault();
								else
									exportFile = null;
							}

							transaction.Commit();
							return list;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region InsertExportFile()
		public DbExportFile InsertExportFile(NewDbExportFile newExportFile)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							DateTime now = DateTime.Now;

							BscanILLData.Models.DbExportFile exportFile = new DbExportFile()
							{
								fExportId = newExportFile.fExportId,
								PreviousId = newExportFile.PreviousId,
								NextId = newExportFile.NextId,
								FileName = newExportFile.FileName,
								ColorMode = newExportFile.ColorMode,
								FileFormat = newExportFile.FileFormat,
								Dpi = newExportFile.Dpi,
								NumOfImages = newExportFile.NumOfImages,
								Status = newExportFile.Status,
							};

							session.Save(exportFile);
							transaction.Commit();

							return exportFile;
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion

		#region SetExportFileStatus()
		public void SetExportFileStatus(DbExportFile exportFile, ExportFileStatus status)
		{
			lock (_threadLocker)
			{
				using (ISession session = this.SessionFactory.OpenSession())
				{
					using (ITransaction transaction = session.BeginTransaction())
					{
						try
						{
							exportFile.Status = status;

							session.SaveOrUpdate(exportFile);
							transaction.Commit();
						}
						catch (Exception)
						{
							transaction.Rollback();
							session.Clear();
							throw;
						}
					}
				}
			}
		}
		#endregion


		#region SomeQuery()
		/*public void SomeQuery()
		{
			session.QueryOver<BscanILLData.Models.SQLite.Export>()
				.Where(p => (p.SessionId == userSession.Id) && p.ExportType == exportType)
				.OrderBy(r => r.CreatedAt).Asc
				.Take(1)
				.SingleOrDefault()
				.Sum(y => (int)y.ImagesExported);

		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

        #region BackupDatabaseFile()
        private bool BackupDatabaseFile(string databasePath)
        {
            bool backUpOK = false;

            System.IO.FileInfo sourceFile = new System.IO.FileInfo(databasePath);

            if (sourceFile.Exists)
            {                
                string destPath = sourceFile.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(sourceFile.Name) + "-bkp-" + DateTime.Today.ToString("MM-dd-yyyy") + sourceFile.Extension;
                System.IO.FileInfo destFile = new System.IO.FileInfo(destPath);

                destFile.Directory.Create();
                sourceFile.CopyTo(destPath, true);
                destFile.Refresh();
                if (destFile.Exists)
                {
                    backUpOK = true;
                }
            }
            return backUpOK;
        }
        #endregion

		#region GetActiveDbPages()
		private List<DbPage> GetActiveDbPages(ISession session, DbScan dbScan)
		{
			List<DbPage> list = new List<DbPage>();

            if (dbScan != null )
            {
                IList<BscanILLData.Models.DbPage> dbPage = session.QueryOver<BscanILLData.Models.DbPage>()
                                    .Where(p => (p.fScanId == dbScan.Id && p.Status == PageStatus.Active)).List();
			
                if (dbPage.Count > 0)
			    {                
                    list.AddRange(dbPage);                
			    }
            }
            else
            {
                //get all active DdbPage records
                IList<BscanILLData.Models.DbPage> dbPage = session.QueryOver<BscanILLData.Models.DbPage>()
                                    .Where(p => (p.Status == PageStatus.Active)).List();

                if (dbPage.Count > 0)
                {
                    list.AddRange(dbPage);
                }
            }

			return list;
		}
		#endregion

        #region GetDatabaseTableNames()
        private List<string> GetDatabaseTableNames(System.Data.SQLite.SQLiteConnection sqlConnection)
        {
            System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection);
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();

            //get tables list
            List<string> tablesList = new List<string>();

            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
            using (System.Data.SQLite.SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount > 0)
                    {
                        try
                        {
                            object val = reader.GetValue(0);
                            tablesList.Add(val.ToString());
                        }
                        catch { }
                    }
                }
            }

            return tablesList;
        }
        #endregion

        #region DatabaseTableExists()
        private bool DatabaseTableExists(System.Data.SQLite.SQLiteConnection sqlConnection , string tableName )
        {
            bool tableExists = false;
            System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection);
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();

            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
            using (System.Data.SQLite.SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount > 0)
                    {
                        try
                        {
                            object val = reader.GetValue(0);
                            if( string.Compare(val.ToString(), tableName, true ) == 0 )
                            {
                                tableExists = true;
                                break;
                            }
                        }
                        catch { }
                    }
                }
            }

            return tableExists;
        }
        #endregion


        #region GetTableColumnNames()
        /// <summary>
        /// PRAGMA table_info('table name') returns list of 6 fields: 
        ///   
        ///		cid         name        type        notnull     dflt_value  pk        
        ///		----------  ----------  ----------  ----------  ----------  ----------
        ///		0           id          integer     99                      1         
        ///		1           name                    0                       0

        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private List<string> GetTableColumnNames(System.Data.SQLite.SQLiteConnection sqlConnection, string tableName)
        {
            List<string> columnNames = new List<string>();
            //List<string> columnTypes = new List<string>();

            System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection);

            command.CommandText = "PRAGMA table_info('" + tableName + "');";

            using (System.Data.SQLite.SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount > 1)
                    {
                        columnNames.Add(reader.GetString(1));
                        //columnTypes.Add(reader.GetString(2));
                    }
                }
            }

            return columnNames;
        }
        #endregion


        #region TableColumnNameExists()
        private bool TableColumnNameExists(System.Data.SQLite.SQLiteConnection sqlConnection, string tableName, string columnName)
        {
            bool columnExists = false;
            System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection);

            command.CommandText = "PRAGMA table_info('" + tableName + "');";

            using (System.Data.SQLite.SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount > 1)
                    {                            
                            if( string.Compare(reader.GetString(1), columnName, true ) == 0 )
                            {
                                columnExists = true;
                                break;
                            }
                    }
                }
            }

            return columnExists;
        }
        #endregion


        #region GetTableColumnType()
       
        private string GetTableColumnType(System.Data.SQLite.SQLiteConnection sqlConnection, string tableName, string columnName)
        {
            string columnType = "";
            System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection);

            command.CommandText = "PRAGMA table_info('" + tableName + "');";

            using (System.Data.SQLite.SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount > 1)
                    {
                        if (reader.GetString(1) == columnName)
                        {
                            columnType = reader.GetString(2);
                            break;
                        }
                    }
                }
            }

            return columnType;
        }
        #endregion

        #region ConvertColumnType()
        private void ConvertColumnType(System.Data.SQLite.SQLiteConnection sqlConnection, string tableName, string columnName, string newColumnType)
        {
            using (System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection))
            {
                string tempColumnName = "TransactionNumbersTemp";

                //command.CommandText = "ALTER TABLE 'DbArticles' ADD 'TransactionNumbers2' 'int';";
                command.CommandText = string.Format("ALTER TABLE '{0}' ADD '{1}' '{2}';", tableName, tempColumnName, newColumnType);
                command.ExecuteNonQuery();
                //command.CommandText = "UPDATE 'DbArticles' SET 'TransactionNumbers2' = 'TransactionNumber';";
                command.CommandText = string.Format("UPDATE '{0}' SET '{1}' = '{2}';", tableName, tempColumnName, columnName);
                command.ExecuteNonQuery();
                //command.CommandText = "ALTER TABLE 'DbArticles' DROP COLUMN 'TransactionNumber';";
                command.CommandText = string.Format("ALTER TABLE '{0}' DROP COLUMN '{1}';", tableName, columnName);  //SQLite does not support Drop Column in ALTER TABLE  !!
                command.ExecuteNonQuery();                                                                           //new SQLite version supports at lease RENAME COLUMN!!
                //command.CommandText = "ALTER TABLE 'DbArticles' ADD 'TransactionNumbers' 'nvarchar(16)';";
                command.CommandText = string.Format("ALTER TABLE '{0}' ADD '{1}' '{2}';", tableName, columnName, newColumnType);
                command.ExecuteNonQuery();
                //command.CommandText = "UPDATE 'DbArticles' SET 'TransactionNumbers' = 'TransactionNumber2';";
                command.CommandText = string.Format("UPDATE '{0}' SET '{1}' = '{2}';", tableName, columnName, tempColumnName);
                command.ExecuteNonQuery();
                //command.CommandText = "ALTER TABLE 'DbArticles' DROP COLUMN 'TransactionNumber2';";
                command.CommandText = string.Format("ALTER TABLE '{0}' DROP COLUMN '{1}';", tableName, tempColumnName);
                command.ExecuteNonQuery();

                // command.CommandText = string.Format("ALTER TABLE '{0}' CHANGE '{1}' '{2}' '{3}';", tableName, columnName, tempColumnName, newColumnType);
                // command.ExecuteNonQuery();
            }

        }
        #endregion

        #region AddTableColumn()
        private void AddTableColumn(System.Data.SQLite.SQLiteConnection sqlConnection, string tableName, string columnName, string columnType, string columnNameSource)
        {
            using (System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection))
            {                
                //command.CommandText = "ALTER TABLE 'DbArticles' ADD 'TransactionNumbersBig' 'int';";
                command.CommandText = string.Format("ALTER TABLE '{0}' ADD '{1}' {2};", tableName, columnName, columnType);
                command.ExecuteNonQuery();

                if (columnNameSource.Length > 0)
                {
                  //command.CommandText = "UPDATE 'DbArticles' SET 'TransactionNumbersBig' = 'TransactionNumber';";
                  command.CommandText = string.Format("UPDATE '{0}' SET {1} = {2};", tableName, columnName, columnNameSource);
                  command.ExecuteNonQuery();
                }
            }

        }
        #endregion

        #region UpgradeTNInArticleTable()
        // using temp table change type of TransactionNumber field to fit bigger numbers
        // cannot use this aproach as it breaks record's relations among tables.
        private void UpgradeTNInArticleTable(System.Data.SQLite.SQLiteConnection sqlConnection)
        {
            bool tempTableExists = DatabaseTableExists(sqlConnection, "DbArticlesTemp");

            System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sqlConnection);

            if (tempTableExists)
            {
                command.CommandText = "DROP TABLE 'DbArticlesTemp';";
                command.ExecuteNonQuery();
            }
            command.CommandText = "CREATE TABLE IF NOT EXISTS 'DbArticlesTemp' ( 'pArticleId' 'integer' 'PRIMARY KEY' , 'TransactionNumber' 'bigint' , 'IllNumber' 'nvarchar(16)' , 'Patron' 'nvarchar(64)' , 'Address' 'nvarchar(128)' , 'ExportType' 'tinyint' , 'CreationDate' 'datetime' , 'LastModifiedDate' 'datetime' , 'FolderName' 'nvarchar(128)' , 'ExportScans' 'bit' , 'Status' 'tinyint' ) ;";                                    
            command.ExecuteNonQuery();
            command.CommandText = "INSERT INTO 'DbArticlesTemp' SELECT 'pArticleId' , 'TransactionNumber' , 'IllNumber' , 'Patron' , 'Address' , 'ExportType' , 'CreationDate' , 'LastModifiedDate' , 'FolderName' , 'ExportScans' , 'Status' FROM 'DbArticles' ;";
            command.ExecuteNonQuery();

            command.CommandText = "PRAGMA foreign_keys = OFF;";
            command.ExecuteNonQuery();

            command.CommandText = "DROP TABLE 'DbArticles';";
            command.ExecuteNonQuery();
            command.CommandText = "ALTER TABLE 'DbArticlesTemp' RENAME TO 'DbArticles';";
            command.ExecuteNonQuery();

            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();            
        }
        #endregion


		#endregion

	}
}
