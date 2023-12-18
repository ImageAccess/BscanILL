using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILL.Hierarchy
{
    public class SessionBatch 
    {
        List<BscanILL.Hierarchy.Article> articles ;
        int currentArticleIndex = -1;

		#region constructor
        public SessionBatch()
		{
            articles = new List<BscanILL.Hierarchy.Article>();
            currentArticleIndex = -1;
		}
		#endregion

        // PUBLIC PROPERTIES
        #region public properties

        public List<BscanILL.Hierarchy.Article> Articles
        {
            get
            {
                return articles;
            }
        }

        public int CurrentArticleIndex
        {
            get
            {
                return currentArticleIndex; 
            } 

            set
            {
                if (currentArticleIndex != value)
                {
                    if (currentArticleIndex < Articles.Count)
                    {
                      currentArticleIndex = value;
                    }
                }
            }
        }

        public BscanILL.Hierarchy.Article CurrentArticle
        {
            get
            {
                return ((CurrentArticleIndex >= 0) && (CurrentArticleIndex < Articles.Count)) ? Articles[CurrentArticleIndex] : null;
            }

            set
            {
                CurrentArticleIndex = -1;
                if( value != null )
                {
                    int index = -1;
                    foreach( BscanILL.Hierarchy.Article article in Articles )
                    {
                        index++;
                        if( article == value )
                        {
                            CurrentArticleIndex = index;
                            break;
                        }
                    }
                }                
            }
        }
        #endregion

        // PUBLIC METHODS
        #region public methods

        public void AddCurrentArticle(BscanILL.Hierarchy.Article newArticle , bool multiArticleEnabled )
        {
            if( ! multiArticleEnabled )
            {
                Reset();
            }
            Articles.Add(newArticle);
            this.CurrentArticleIndex = Articles.Count - 1;
        }

        public void DeleteArticle(BscanILL.Hierarchy.Article delArticle)
        {
            int index = Articles.IndexOf(delArticle);
            if (index >= 0)
            {                
                if( index == CurrentArticleIndex )
                {
                    CurrentArticleIndex = -1 ;
                }
                else
                if( index < CurrentArticleIndex )
                {
                    CurrentArticleIndex--;
                }
                Articles.Remove(delArticle);
            }
        }

        public void Reset()
        {
            Articles.Clear();
            ResetArticleIndex(); 
        }


        public bool ArticleExists( BscanILL.Hierarchy.Article articleToCheck )
        {
            foreach( Article article in Articles )
            {
                if (articleToCheck == article )
                {
                    return true;
                }
            }

            return false;
        }

        public bool ArticleLoadedInScanFrame(BscanILL.Hierarchy.Article articleToCheck)
        {
            foreach( Article article in Articles )
            {
                if ((articleToCheck == article) && (article.IsLoadedInScan))
                {
                    return true;
                }
            }

            return false;
        }
        


        #endregion

        // PRIVATE METHODS
        #region private methods

        private void AddArticle(BscanILL.Hierarchy.Article newArticle)
        {
            Articles.Add(newArticle);
        }

        private void ResetArticleIndex()
        {
            this.currentArticleIndex = -1;
        }
        #endregion
    }

}
