using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BscanILL.Hierarchy;
using System.ComponentModel;

namespace BscanILL.UI.Frames.Resend
{
	/// <summary>
	/// Interaction logic for FrameResendUi.xaml
	/// </summary>
	public partial class FrameResendUi : UserControl
	{
		//Article article = null;
        SessionBatch batch = null;

		System.Threading.Thread countsThread = null;

		CompareBy lastCompareBy = CompareBy.Id;
		ListSortDirection lastHeaderDirection = ListSortDirection.Descending;

		
		public event BscanILL.Misc.VoidHnd GoToStartClick;
		public event BscanILL.Misc.VoidHnd GoToScanClick;
		public event BscanILL.Misc.VoidHnd GoToItClick;

		public event BscanILL.Misc.VoidHnd HelpClick;

		public delegate void DbArticleHnd(BscanILLData.Models.DbArticle article);
		public event DbArticleHnd	OpenDbArticleInScan;
		public event DbArticleHnd	OpenDbArticleInCleanUp;
		public event DbArticleHnd	OpenDbArticleInSend;
		public event DbArticleHnd	ResendDbArticle;
		public event DbArticleHnd	DeleteDbArticle;



		#region constructor
		public FrameResendUi()
		{
			InitializeComponent();
		}
		#endregion



		// PUBLIC METHODS
		#region public methods

		#region Open()
		public void Open(SessionBatch batch)
		{
			//this.article = article;
            this.batch = batch;
			Refresh();
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			//this.article = null;
            this.batch = null;
			StopCountsThread();
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			this.listView.DataContext = new ArticlesLocal();
			this.listView.GetBindingExpression(ListView.ItemsSourceProperty).UpdateTarget();
			//this.listView.Items.Clear();
		}
		#endregion

		#endregion



		// PRIVATE METHODS
		#region private methods

		#region SelectedArticle
		private BscanILLData.Models.DbArticle SelectedArticle
		{
			get
			{
				if (this.listView.SelectedItems != null && this.listView.SelectedItems.Count == 1 && this.listView.SelectedItem != null && this.listView.SelectedItem is ArticleLocal)
					return ((ArticleLocal)this.listView.SelectedItem).Article;

				return null;
			}
		}
		#endregion

		#region SelectedArticles
		private List<BscanILLData.Models.DbArticle> SelectedArticles
		{
			get
			{
				List<BscanILLData.Models.DbArticle> list = new List<BscanILLData.Models.DbArticle>();

				if (this.listView.SelectedItems != null && this.listView.SelectedItems.Count > 0)
				{
					foreach (object item in this.listView.SelectedItems)
						if (item is ArticleLocal)
							list.Add(((ArticleLocal)item).Article);
				}

				return list;
			}
		}
		#endregion

		#endregion



		// PRIVATE METHODS
		#region private methods

		#region OpenInScan_Click()
		private void OpenInScan_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILLData.Models.DbArticle article = this.SelectedArticle;

				if (article != null)
				{
					if (OpenDbArticleInScan != null)
						OpenDbArticleInScan(article);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception occured while opening article: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion

		#region OpenInCleanUp_Click()
		private void OpenInCleanUp_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILLData.Models.DbArticle article = this.SelectedArticle;

				if (article != null)
				{
					if (OpenDbArticleInCleanUp != null)
						OpenDbArticleInCleanUp(this.SelectedArticle);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception occured while opening article: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion

		#region OpenInSend_Click()
		private void OpenInSend_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILLData.Models.DbArticle article = this.SelectedArticle;

				if (article != null)
				{
					if (OpenDbArticleInSend != null)
						OpenDbArticleInSend(article);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception occured while opening article: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion

		#region Send_Click()
		private void Send_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILLData.Models.DbArticle article = this.SelectedArticle;

				if (article != null)
				{
					if (ResendDbArticle != null)
						ResendDbArticle(article);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception occured while sending article: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion

		#region Remove_Click()
		private void Remove_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				List<BscanILLData.Models.DbArticle> articles = this.SelectedArticles;

				if (articles.Count == 1)
				{
					BscanILLData.Models.DbArticle article = articles[0];

					if ((article != null) && (DeleteDbArticle != null))
					{
						if (MessageBox.Show("Are you sure you want to delete article '" + article.Id.ToString() + "'?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							DeleteDbArticle(article);
							Refresh();
						}
					}
				}
				else if (articles.Count > 1)
				{
					if (DeleteDbArticle != null)
					{
						if (MessageBox.Show("Are you sure you want to delete " + articles.Count.ToString() + " selected articles?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							foreach (BscanILLData.Models.DbArticle article in articles)
								DeleteDbArticle(article);

							Refresh();
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception occured while removing article: " + BscanILL.Misc.Misc.GetErrorMessage(ex));
			}
		}
		#endregion

		#region Close_Click()
		private void Close_Click(object sender, RoutedEventArgs e)
		{
			if (GoToStartClick != null)
				GoToStartClick();
		}
		#endregion

		#region Refresh()
		public void Refresh()
		{
			List<BscanILLData.Models.DbArticle> articles = BscanILL.DB.Database.Instance.GetArticles();
			ArticlesLocal list = new ArticlesLocal();

			foreach (BscanILLData.Models.DbArticle article in articles)
				list.Add(new ArticleLocal(article));

			this.listView.DataContext = list;

			StartCountsThread(list);
		}
		#endregion
	
		#region GoToStart_Click()
		private void GoToStart_Click(object sender, RoutedEventArgs e)
		{
			if (GoToStartClick != null)
				GoToStartClick();
		}
		#endregion

		#region GoToScan_Click()
		private void GoToScan_Click(object sender, RoutedEventArgs e)
		{
			if (GoToScanClick != null)
				GoToScanClick();
		}
		#endregion

		#region GoToIt_Click()
		private void GoToIt_Click(object sender, RoutedEventArgs e)
		{
			if (GoToItClick != null)
				GoToItClick();
		}
		#endregion

		#region Help_Click()
		private void Help_Click(object sender, RoutedEventArgs e)
		{
			if (HelpClick != null)
				HelpClick();
		}
		#endregion

		#region ListView_SelectionChanged()
		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			BscanILLData.Models.DbArticle article = this.SelectedArticle;

			if (article != null)
			{
				List<BscanILLData.Models.DbScan> activeScans = BscanILL.DB.Database.Instance.GetScans(article);
				
				this.buttonOpenInScan.IsEnabled = true;
				this.buttonOpenInCleanUp.IsEnabled = (activeScans.Count > 0);

				bool canExport = (activeScans.Count > 0);

				if (canExport)
				{
					foreach (BscanILLData.Models.DbScan dbScan in activeScans)
						if (BscanILL.DB.Database.Instance.GetActiveDbPages(dbScan).Count == 0)
						{
							canExport = false;
							break;
						}
				}

				this.buttonOpenInExport.IsEnabled = canExport;
				this.buttonSend.IsEnabled = canExport;
			}
			else
			{
				this.buttonOpenInScan.IsEnabled = false;
				this.buttonOpenInCleanUp.IsEnabled = false;
				this.buttonOpenInExport.IsEnabled = false;
				this.buttonSend.IsEnabled = false;
			}

			bool buttonDeleteEnabled = (this.SelectedArticles.Count > 0);

			//if (this.article != null)
            if(this.batch.Articles.Count > 0)
			{
                foreach ( Article art in this.batch.Articles)
                {
                    foreach (BscanILLData.Models.DbArticle a in this.SelectedArticles)
                    {
                        //if (a.Id == this.article.Id)
                        if (a.Id == art.Id)
                        {
                            buttonDeleteEnabled = false;
                            break;
                        }
                    }
                    if( ! buttonDeleteEnabled )
                    {
                        break;
                    }
                }
			}

			this.buttonRemove.IsEnabled = buttonDeleteEnabled;
		}
		#endregion

		#region Header_Click()
		private void Header_Click(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

			if (headerClicked != null)
			{
				if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
				{
					CompareBy compareBy = CompareBy.Id;

					if (headerClicked == headerId)
						compareBy = CompareBy.Id;
					else if (headerClicked == headerTn)
						compareBy = CompareBy.Tn;
					else if (headerClicked == headerIllNumber)
						compareBy = CompareBy.IllNumber;
					else if (headerClicked == headerCreated)
						compareBy = CompareBy.Created;
					else if (headerClicked == headerSent)
						compareBy = CompareBy.LastSent;
					else if (headerClicked == headerPatron)
						compareBy = CompareBy.Patron;
                    else if (headerClicked == headerAddress)
						compareBy = CompareBy.Address;
					else if (headerClicked == headerDelivery)
						compareBy = CompareBy.DeliveryMethod;

					if (this.lastCompareBy != compareBy)
					{
						this.lastCompareBy = compareBy;

						if (this.lastCompareBy == CompareBy.Created || this.lastCompareBy == CompareBy.LastSent)
							lastHeaderDirection = ListSortDirection.Descending;
						else
							lastHeaderDirection = ListSortDirection.Ascending;
					}
					else
						lastHeaderDirection = (lastHeaderDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;


					string header = "";

					if (headerClicked.Column.DisplayMemberBinding != null)
						header = ((System.Windows.Data.Binding)(headerClicked.Column.DisplayMemberBinding)).Path.Path;

					if (header == "IllNumber")
					{
						ListCollectionView view = CollectionViewSource.GetDefaultView(this.listView.ItemsSource) as ListCollectionView;
						view.CustomSort = new ArticlesLocalComparer(CompareBy.IllNumber, lastHeaderDirection);
					}
					else
					{
						ICollectionView dataView = CollectionViewSource.GetDefaultView(this.listView.ItemsSource);

						dataView.SortDescriptions.Clear();
						SortDescription sd = new SortDescription(header, lastHeaderDirection);
						dataView.SortDescriptions.Add(sd);
						dataView.Refresh();
					}
				}
			}
		}
		
		/*private void Header_Click(object sender, RoutedEventArgs e)
{
	GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

	if (headerClicked != null)
	{
		if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
		{
			CompareBy compareBy = CompareBy.Id;

			if (headerClicked == headerId)
				compareBy = CompareBy.Id;
			else if (headerClicked == headerTn)
				compareBy = CompareBy.Tn;
			else if (headerClicked == headerIllNumber)
				compareBy = CompareBy.IllNumber;
			else if (headerClicked == headerCreated)
				compareBy = CompareBy.Created;
			else if (headerClicked == headerSent)
				compareBy = CompareBy.LastSent;
			else if (headerClicked == headerPatron)
				compareBy = CompareBy.Patron;
			else if (headerClicked == headerDelivery)
				compareBy = CompareBy.DeliveryMethod;

			if (this.lastCompareBy != compareBy)
			{
				this.lastCompareBy = compareBy;

				if (this.lastCompareBy == CompareBy.Created || this.lastCompareBy == CompareBy.LastSent)
					lastHeaderDirection = ListSortDirection.Descending;
				else
					lastHeaderDirection = ListSortDirection.Ascending;
			}
			else
				lastHeaderDirection = (lastHeaderDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;

			ShowItems();
		}
	}
}*/
		#endregion
	
		#region ShowItems()
		private void ShowItems()
		{
			try
			{	
				ArticlesLocal list = (ArticlesLocal)this.listView.DataContext;

				ArticlesLocalComparer comparer = new ArticlesLocalComparer(this.lastCompareBy, this.lastHeaderDirection);

				//list.Sort(comparer);

				ArticlesLocal sortedItems = new ArticlesLocal();
				foreach (ArticleLocal i in list)
					sortedItems.Add(i);

				this.listView.DataContext = sortedItems;
				this.listView.GetBindingExpression(ListView.ItemsSourceProperty).UpdateTarget();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region StartCountsThread()
		public void StartCountsThread(ArticlesLocal articles)
		{
			StopCountsThread();
			
			countsThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(DB.Database.Instance.AddArticleCountsThread));
			countsThread.SetApartmentState(System.Threading.ApartmentState.STA);
			countsThread.Start(articles);
		}
		#endregion

		#region StopCountsThread()
		public void StopCountsThread()
		{
			if (countsThread != null)
			{
				countsThread.Abort();
				countsThread = null;
			}
		}
		#endregion

		#endregion

	}
}
