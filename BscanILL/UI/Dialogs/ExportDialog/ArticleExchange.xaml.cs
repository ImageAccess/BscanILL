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
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;

namespace BscanILL.UI.Dialogs.ExportDialog
{
	/// <summary>
	/// Interaction logic for Ftp.xaml
	/// </summary>
	public partial class ArticleExchange : PanelBase, IPanel
	{
		string		recipientEmail = "";
		string		subject = "";
		string		body = "";

        static bool confirmEmailAddressInit = false;
        static string confirmEmailAddress = "";
		

		#region constructor
		public ArticleExchange()
		{
			InitializeComponent();

			ComboItemFileFormat comboItemPdf = new ComboItemFileFormat(Scan.FileFormat.Pdf);

            //fileformatCtrl.Init(this.Name);            
            fileformatCtrl.Reload(BscanILL.Export.ExportType.ArticleExchange);
			
			//comboConfirmationRecipients.Text = _settings.Export.ArticleExchange.ConfirmationEmailAddress;            
            if( ( ! confirmEmailAddressInit ) || (_settings.Export.ArticleExchange.ResetConfirmEmailAddress) )
            {
                ConfirmEmailAddress = _settings.Export.ArticleExchange.ConfirmationEmailAddress;
                confirmEmailAddressInit = true;
            }
            comboConfirmationRecipients.Text = ConfirmEmailAddress; 

			this.FileNamePrefix = BscanILL.Export.Misc.GetUniqueExportFilePrefix(Scan.FileFormat.Pdf, true);

            this.fileformatCtrl.UpdateFileParamControl += delegate()
            {
                FileFormat_SelectionChanged();
            };
            
			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Scan.FileFormat FileFormat { get { return this.fileformatCtrl.FileFormat; } }
		public bool						MultiImage	{ get { return this.fileformatCtrl.MultiImage; } }
		public bool						PdfA		{ get { return this.fileformatCtrl.PdfA; } }
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get { return this.fileparamCtrl.FileColor; } }
        public int FileQuality { get { return this.fileparamCtrl.FileQuality; } } 

		#region RecipientEmail
		public string RecipientEmail
		{
			get { return this.recipientEmail; }
			set
			{
				this.recipientEmail = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ConfirmationEmail
		public string ConfirmationEmail
		{
			get { return _settings.Export.ArticleExchange.ConfirmationEmailAddress; }
			/*set
			{
				_settings.Export.ArticleExchange.ConfirmationEmailAddress = comboConfirmationRecipients.Text;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}*/
		}
		#endregion

        #region ConfirmEmailAddress
        public string ConfirmEmailAddress
        {
            get { return confirmEmailAddress; }
            set
            {
                confirmEmailAddress = value;
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

		#region ConfirmationEmails
		public ObservableCollection<string> ConfirmationEmails
		{
			get { return _settings.Export.ArticleExchange.ConfirmationRecipients; }
		}
		#endregion

		#region MessageSubject
		public string MessageSubject
		{
			get { return this.subject; }
			set
			{
				this.subject = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region MessageBody
		public string MessageBody
		{
			get { return this.body; }
			set
			{
				this.body = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region LoadArticle()
		public override void LoadArticle(BscanILL.Hierarchy.Article article)
		{
			base.LoadArticle(article);

			this.RecipientEmail = this.article.Address;

            //for safety.. the confirmation email address should be initialized in constructor for each article            
            if ((!confirmEmailAddressInit) || (_settings.Export.ArticleExchange.ResetConfirmEmailAddress))
            {
                ConfirmEmailAddress = _settings.Export.ArticleExchange.ConfirmationEmailAddress;
                confirmEmailAddressInit = true;
            }

			this.FileNamePrefix = GetExportFileNamePrefixPreferringIllNumber(article);

			string subject = _settings.Export.ArticleExchange.Subject;
			string body = _settings.Export.ArticleExchange.Body;

			subject = subject.Replace("{ID}", this.FileNamePrefix);
			body = body.Replace("{ID}", this.FileNamePrefix);
			
			if (article.Patron != null)
			{
				subject = subject.Replace("{PA}", article.Patron);
				body = body.Replace("{PA}", article.Patron);
			}

			this.MessageSubject = subject;
			this.MessageBody = body;
            //fileformatCtrl.PdfA
            fileformatCtrl.FileFormat = _settings.Export.ArticleExchange.ExportFileFormat;
            this.fileparamCtrl.FileColor = _settings.Export.ArticleExchange.FileExportColorMode;
            this.fileparamCtrl.FileQuality = _settings.Export.ArticleExchange.FileExportQuality;
		}
		#endregion

		#region GetAdditionalInfo()
		public BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo()
		{			
			//return new BscanILL.Export.AdditionalInfo.AdditionalArticleExchange(this.RecipientEmail, this.ConfirmationEmail, this.FileNamePrefix, this.FileFormat,
            return new BscanILL.Export.AdditionalInfo.AdditionalArticleExchange(this.RecipientEmail, this.ConfirmEmailAddress, this.FileNamePrefix, this.FileFormat,                
                this.MessageSubject, this.MessageBody, this.MultiImage, this.PdfA, this.FileColor, this.FileQuality);
		}
		#endregion

		#region Combo_SelectionChanged()
		private void Combo_SelectionChanged(object sender, TextChangedEventArgs e)
		{
			//_settings.Export.ArticleExchange.ConfirmationEmailAddress = comboConfirmationRecipients.Text;
            ConfirmEmailAddress = comboConfirmationRecipients.Text;
		}
		#endregion

        #region FileFormat_SelectionChanged
        public void FileFormat_SelectionChanged()
        {
            if (fileformatCtrl.FileFormat == Scan.FileFormat.Pdf || fileformatCtrl.FileFormat == Scan.FileFormat.SPdf)
            {
                this.fileparamCtrl.comboFileColor.Visibility = System.Windows.Visibility.Visible;
                this.fileparamCtrl.textFileColor.Visibility = System.Windows.Visibility.Visible;

                FileColor_SelectionChanged();
            }
            else
            {
                this.fileparamCtrl.comboFileColor.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.textFileColor.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.comboFileQuality.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.textFileQuality.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        #endregion

        #region FileColor_SelectionChanged
        public void FileColor_SelectionChanged()
        {
            if (this.fileparamCtrl.FileColor != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
            {
                this.fileparamCtrl.comboFileQuality.Visibility = System.Windows.Visibility.Visible;
                this.fileparamCtrl.textFileQuality.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.fileparamCtrl.comboFileQuality.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.textFileQuality.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        #endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#endregion

	}
}
