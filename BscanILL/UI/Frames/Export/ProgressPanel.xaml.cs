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

namespace BscanILL.UI.Frames.Export
{
	/// <summary>
	/// Interaction logic for ProgressPanel.xaml
	/// </summary>
	public partial class ProgressPanel : UserControl
	{
		public event BscanILL.Misc.VoidHnd OkClick;
		public event BscanILL.Misc.VoidHnd CloseArticleClick;
		public event BscanILL.Misc.VoidHnd KeepArticleOpenClick;


		#region constructor
		public ProgressPanel()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#region Progress
		internal double Progress
		{
			get { return (this.progressBar.Value / 100.0); }
			set { this.progressBar.Value = value * 100.0; }
		}
		#endregion

		#region CurrentAction
		internal string CurrentAction
		{
			get { return this.textBlockAction.Text; }
			set { this.textBlockAction.Text = value; }
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Reset()
		public void Reset()
		{
			this.gridOkButton.Visibility = System.Windows.Visibility.Collapsed;
			this.gridOptionsButtons.Visibility = System.Windows.Visibility.Collapsed;
			textBoxComments.Text = "";
			this.Progress = 0;
			this.CurrentAction = "";
		}
		#endregion

		#region AddComment()
		public void AddComment(string comment)
		{
			if(comment != null && comment.Length > 0)
				textBoxComments.Text += comment + Environment.NewLine;
		}
		#endregion

        #region ExportArticleFinishedSuccessfully()
        public void ExportArticleFinishedSuccessfully()
		{
			this.Progress = 0;
			this.gridOptionsButtons.Visibility = System.Windows.Visibility.Visible;
		}
		#endregion

        #region ExportBatchFinishedSuccessfully()
        public void ExportBatchFinishedSuccessfully( int articleCount )
        {
            this.Progress = 0;
            if (articleCount > 1)
            {
                this.keepOpenButton.Content = "Keep Current\r\nArticles Open"; ;
                
                //this.keepButtonText.Text = "Keep Current<LineBreak/>Articles Open";
                this.successText.Text = "Articles were exported successfully. Would you like to close current articles and start working on another one, or would you like to export current articles to another medium?";
            }
            else
            {
                this.keepOpenButton.Content = "Keep Current\r\nArticle Open"; ;
               // this.keepButtonText.Text = "Keep Current<LineBreak/>Article Open";
                this.successText.Text = "Article was exported successfully. Would you like to close current article and start working on another one, or would you like to export current article to another medium?";
            }
            this.gridOptionsButtons.Visibility = System.Windows.Visibility.Visible;
        }
        #endregion

		#region ExportFinishedWithError()
		public void ExportFinishedWithError()
		{
			this.gridOkButton.Visibility = System.Windows.Visibility.Visible;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Ok_Click()
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			if (this.OkClick != null)
				this.OkClick();
		}
		#endregion

		#region CloseArticle_Click()
		private void CloseArticle_Click(object sender, RoutedEventArgs e)
		{
			if (this.CloseArticleClick != null)
				this.CloseArticleClick();
		}
		#endregion

		#region KeepArticleOpen_Click()
		private void KeepArticleOpen_Click(object sender, RoutedEventArgs e)
		{
			if (this.KeepArticleOpenClick != null)
				this.KeepArticleOpenClick();
		}
		#endregion

		#endregion

	}
}
