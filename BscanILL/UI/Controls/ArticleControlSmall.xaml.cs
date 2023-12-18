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
using System.ComponentModel;
using System.Reflection;

namespace BscanILL.UI.Controls
{
    /// <summary>
    /// Interaction logic for ArticleControlSmall.xaml
    /// </summary>
    public partial class ArticleControlSmall : UserControl, INotifyPropertyChanged
    {
        BscanILL.Hierarchy.Article article = null;

        public event PropertyChangedEventHandler PropertyChanged;

        #region constructor
        public ArticleControlSmall()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        // PUBLIC PROPERTIES
        #region public properties

        public BscanILL.Hierarchy.Article Article
        {
            get { return this.article; }
            set
            {
                this.article = value;                
                RefreshBinding();
            }
        }

        public string Patron { get { return (this.article != null) ? article.Patron : ""; } }
        public string Address { get { return (this.article != null) ? article.Address : ""; } }
        public string IllNumber { get { return (this.article != null) ? article.IllNumber : ""; } }
        public string TransactionNumber { get { return (this.article != null && article.TransactionId.HasValue) ? article.TransactionId.Value.ToString() : ""; } }

        public string DeliveryMethod { get { return (this.article != null) ? BscanILL.Export.Misc.GetExportTypeCaption(article.ExportType) : ""; } }

        #endregion

        // PUBLIC METHODS
        #region public methods

        #region LoadArticle()
        public void LoadArticle(BscanILL.Hierarchy.Article article)
        {
            if (this.Article != null)
                this.article.PropertyChanged -= new PropertyChangedEventHandler(Article_PropertyChanged);

            this.Article = article;

            if (this.Article != null)
                this.article.PropertyChanged += new PropertyChangedEventHandler(Article_PropertyChanged);
        }
        #endregion

        #endregion

        // PRIVATE METHODS
        #region private methods

        #region Article_PropertyChanged()
        void Article_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate() { RefreshBinding(); });
        }
        #endregion

        #region RefreshBinding
        private void RefreshBinding()
        {
            try
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Patron"));
                PropertyChanged(this, new PropertyChangedEventArgs("Address"));
                PropertyChanged(this, new PropertyChangedEventArgs("IllNumber"));
                PropertyChanged(this, new PropertyChangedEventArgs("TransactionNumber"));
                PropertyChanged(this, new PropertyChangedEventArgs("DeliveryMethod"));
            }
            catch { }
        }
        #endregion

        #endregion


    }
}
