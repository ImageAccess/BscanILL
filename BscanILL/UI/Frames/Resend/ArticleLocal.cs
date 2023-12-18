#define TransNumber_LONG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BscanILL.UI.Frames.Resend
{
	public class ArticlesLocal : ObservableCollection<ArticleLocal>
	{
		public ArticlesLocal()
		{
		}
	}

	public class ArticleLocal : INotifyPropertyChanged
	{
		int scansCount = 0;
		int pagesCount = 0;
		bool scansComputed = false;
		bool pagesComputed = false;

		public event PropertyChangedEventHandler PropertyChanged;

		public readonly BscanILLData.Models.DbArticle Article;

		public ArticleLocal(BscanILLData.Models.DbArticle article)
		{
			this.Article = article;
		}

		public long Id { get { return this.Article.Id; } }

#if TransNumber_LONG
		public long? Tn { get { return this.Article.TransactionNumberBig; } }
#else
        public long? Tn { get { return this.Article.TransactionNumber; } }
#endif
		public string IllNumber { get { return this.Article.IllNumber; } }
		public DateTime Created { get { return this.Article.CreationDate; } }
		public DateTime LastSent { get { return this.Article.LastModifiedDate; } }
		public string Patron { get { return this.Article.Patron; } }
        public string Address { get { return this.Article.Address; } }   
		public string ExportType { get { return BscanILL.Export.Misc.GetExportTypeCaption((BscanILL.Export.ExportType)this.Article.ExportType); } }
		public bool IsScansComputed { get { return this.scansComputed; } set { this.scansComputed = value; } }
		public bool IsPagesComputed { get { return this.pagesComputed; } set { this.pagesComputed = value; } }

		public int ScansCount 
		{ 
			get { return scansCount; }
			set
			{
				this.scansCount = value;
				this.scansComputed = true;

				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("ScansCount"));
					PropertyChanged(this, new PropertyChangedEventArgs("IsScansComputed"));
				}
			}
		}
		// BscanILL.DB.Database.Instance.GetActiveDbScans(this.Article).Count; } }

		public int PagesCount 
		{ 
			get { return pagesCount; }
			set
			{
				this.pagesCount = value;
				this.pagesComputed = true;

				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("PagesCount"));
					PropertyChanged(this, new PropertyChangedEventArgs("IsPagesComputed"));
				}
			}
		}
		// BscanILL.DB.Database.Instance.GetActiveDbPages(this.Article).Count; } }
	}
}
