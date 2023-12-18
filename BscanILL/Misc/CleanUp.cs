using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BscanILL.Hierarchy;
using System.IO;
using BscanILLData.Models;

namespace BscanILL.Misc
{
	/// <summary>
	/// Asynchronous
	/// </summary>
	class CleanUp
	{
		public delegate void ProgressChangedHnd(double progress);
		public delegate void ProcessSuccessfullHnd();
		public delegate void ProcessErrorHnd(Exception ex);
		public delegate void ProgressDescriptionHnd(string description);

		public event ProgressChangedHnd ProgressChanged;
		public event ProgressDescriptionHnd DescriptionChanged;
		public event ProcessSuccessfullHnd OperationDone;
		public event ProcessErrorHnd OperationError;


		#region constructor
		public CleanUp()
		{
		}
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		BscanILL.SETTINGS.Settings	_settings { get { return BscanILL.SETTINGS.Settings.Instance; } }
		BscanILL.DB.Database		_database { get { return BscanILL.DB.Database.Instance; } }
		#endregion



		// PUBLIC METHODS
		#region public methods

		#region Execute()
		//public void Execute(Article	currentArticle, DirectoryInfo exportDir)
        public void Execute(SessionBatch currentBatch, DirectoryInfo exportDir)
		{
			try
			{
				if (_settings.General.KeepArticlesFor > 0)
				{
					if (DescriptionChanged != null)
						DescriptionChanged("Seeking old articles...");

					//get all directories
					DirectoryInfo[]		allDirs = new DirectoryInfo(_settings.General.ArticlesDir).GetDirectories();
					List<string>		dirsToDelete = new List<string>();
					List<DbArticle>	articlesToKeep = new List<DbArticle>();

					foreach (DirectoryInfo dir in allDirs)
						dirsToDelete.Add(dir.Name);

					List<DbArticle> dbArticles = _database.GetArticles();

					if (DescriptionChanged != null)
						DescriptionChanged("Deleting old articles from database...");

					for (int i = 0; i < dbArticles.Count; i++)
					{
                        bool articleIsOpened = false ;
						DbArticle dbArticle = dbArticles[i];
                        
                        if(currentBatch != null)
                        {
                            foreach(Article article in currentBatch.Articles)
                            {
                               if(dbArticle.Id == article.Id)
                               {
                                   articleIsOpened = true;
                                   break ;
                               }
                            }
                        }

						//if ((currentArticle == null || dbArticle.Id != currentArticle.Id) && (dbArticle.LastModifiedDate < DateTime.Now.AddDays(-_settings.General.KeepArticlesFor)))
                        if (( ! articleIsOpened) && (dbArticle.LastModifiedDate < DateTime.Now.AddDays(-_settings.General.KeepArticlesFor)))
						{
							DeleteArticle(dbArticle);
						}
						else
						{
							articlesToKeep.Add(dbArticle);
							RemoveFromList(dirsToDelete, dbArticle.FolderName);
						}

						if (ProgressChanged != null)
							ProgressChanged((i + 1.0) / dbArticles.Count);
					}

					DeleteFolders(dirsToDelete);
					DeleteOldExportsFromArticles(articlesToKeep);
				}

				if (OperationDone != null)
					OperationDone();
			}
			catch (Exception ex)
			{
				if (OperationError != null)
					OperationError(ex);
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region DeleteArticle()
		private void DeleteArticle(DbArticle dbArticle)
		{
			_database.SetArticleStatus(dbArticle, Hierarchy.ArticleStatus.Deleted);
		}
		#endregion

		#region DeleteFolders()
		private void DeleteFolders(List<string> dirNames)
		{
			if (DescriptionChanged != null)
				DescriptionChanged("Deleting old articles from disk...");
			
			for (int i = 0; i < dirNames.Count; i++)
			{
				try
				{
					DirectoryInfo dir = new DirectoryInfo(Path.Combine(_settings.General.ArticlesDir, dirNames[i]));

					dir.Delete(true);
				}
				catch (Exception) { }

				if (ProgressChanged != null)
					ProgressChanged((i + 1.0) / dirNames.Count);
			}
		}
		#endregion

		#region DeleteOldExportsFromArticles()
		private void DeleteOldExportsFromArticles(List<DbArticle> articles)
		{
			if (DescriptionChanged != null)
				DescriptionChanged("Deleting old exports from disk...");

			for (int i = 0; i < articles.Count; i++)
			{
				try
				{
					DirectoryInfo dir = new DirectoryInfo(Path.Combine(_settings.General.ArticlesDir, articles[i].FolderName, "Exports"));

					if (dir.Exists)
					{
						IList<DbExport>	exports = _database.GetExports(articles[i]);
						DbExport		lastExport = exports.OrderByDescending(x => x.ExportDate).FirstOrDefault();
 
						/*foreach (DbExport export in _database.GetExports(articles[i]))
						{
							if(lastExport == null || lastExport.ExportDate < export.ExportDate)
								lastExport = export;
						}*/

						/*foreach (DbExport export in exports)
						{
							if (export != lastExport)
								_database.DeleteExport(export);
						}*/

						foreach (DirectoryInfo dirInfo in dir.GetDirectories())
						{
							if (lastExport == null || dirInfo.Name != lastExport.FolderName)
								dirInfo.Delete(true);
						}
					}
				}
				catch (Exception) { }

				if (ProgressChanged != null)
					ProgressChanged((i + 1.0) / articles.Count);
			}
		}
		#endregion

		#region RemoveFromList()
		private void RemoveFromList(List<string> list, string dirName)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (string.Compare(list[i], dirName, true) == 0)
				{
					list.RemoveAt(i);
					return;
				}
			}
		}
		#endregion

		#endregion
	}
}
