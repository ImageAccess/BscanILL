using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BscanILLData.Models;
using BscanILLData.Models.Helpers;

namespace BscanILLData.Helpers.SQLite
{
	public interface ISQLiteHelper : INHibernateHelper
	{
		void					AdjustDataOnStartup();
        void                    UpdateDatabaseTables(string connectionString);
        void                    UpgradeDatabaseTables(string connectionString, string databasePath);

		void					TestConnection(string connectionString);
		
		void					SaveObject(object obj);

		IList<DbArticle>		GetActiveArticles();
		DbArticle				GetArticle(int id);
		DbArticle				InsertArticle(NewDbArticle newArticle);
		void					SetArticleStatus(DbArticle article, ArticleStatus status);

		DbScan					GetPullslip(DbArticle dbArticle);
		List<DbScan>			GetActiveScans(DbArticle dbArticle);
		DbScan					InsertPullslip(NewDbPullslip newPullslip);
		DbScan					InsertScan(NewDbScan newScan);
		void					DeleteScan(DbScan dbScan);
		void					SetScanDeleteStatus(DbScan dbScan);
		void					SetScanStatus(DbScan scan, ScanStatus status);

		List<DbPage>			GetActivePages(DbArticle dbArticle);
		List<DbPage>			GetActivePages(DbScan dbScan);
		DbPage					InsertPage(NewDbPage newPage);
		void					DeletePage(DbPage dbPage);
		void					SetPageStatus(DbPage page, PageStatus status);

		IList<DbExport>			GetExports(int articleId);
		DbExport				InsertExport(NewDbExport newExport);
		void					SetExportStatus(DbExport export, ExportStatus status);

		List<DbExportFile>		GetExportFiles(int exportId);
		DbExportFile			InsertExportFile(NewDbExportFile newExportFile);
		void					SetExportFileStatus(DbExportFile exportFile, ExportFileStatus status);
	}
}
