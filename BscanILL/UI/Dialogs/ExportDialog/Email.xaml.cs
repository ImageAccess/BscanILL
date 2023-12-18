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

namespace BscanILL.UI.Dialogs.ExportDialog
{
	/// <summary>
	/// Interaction logic for Email.xaml
	/// </summary>
	public partial class Email : PanelBase, IPanel
	{
		string recipient = "";
		string subject = "";
		string body = "";
		bool updateIlliad;
		bool changeStatusToRequestFinished;
		bool includePullslip = true;
		
		public delegate void	SendTestEmailRequestHnd(bool sendDataAttachment);


		#region Email()
		public Email()
		{
			InitializeComponent();
			
			this.updateIlliad = _settings.Export.Email.UpdateILLiad;
			this.changeStatusToRequestFinished = _settings.Export.Email.ChangeRequestToFinished;

            this.FileNamePrefix = BscanILL.Export.Misc.GetUniqueExportFilePrefix(Scan.FileFormat.Pdf, true);

			this.DataContext = this;

            this.fileformatCtrl.UpdateFileParamControl += delegate()
            {
                FileFormat_SelectionChanged();
            };
		}
		#endregion



		//PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Scan.FileFormat FileFormat { get { return this.fileformatCtrl.FileFormat; } }
		public bool MultiImage { get { return this.fileformatCtrl.MultiImage; } }
		public bool PdfA { get { return this.fileformatCtrl.PdfA; } }
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get { return this.fileparamCtrl.FileColor; } }
        public int FileQuality { get { return this.fileparamCtrl.FileQuality; } } 


		#region FileFormatSelectedItem
		/*public ComboItemFileFormat FileFormatSelectedItem
		{
			get 
			{
				foreach (ComboItemFileFormat item in comboFileFormat.Items)
					if (item.Value == _settings.Export.Email.FileFormat)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Export.Email.FileFormat = value.Value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}*/
		#endregion

		#region Recipient
		public string Recipient
		{
			get { return this.recipient; }
			set
			{
				this.recipient = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
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

		#region IncludePullslip
		public bool IncludePullslip
		{
			get { return this.includePullslip; }
			set
			{
				this.includePullslip = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region UpdateILLiad
		public bool UpdateILLiad
		{
			get { return this.updateIlliad; }
			set
			{
				this.updateIlliad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ChangeStatusToRequestFinished
		public bool ChangeStatusToRequestFinished
		{
			get { return this.changeStatusToRequestFinished; }
			set
			{
				this.changeStatusToRequestFinished = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods
		
		#region LoadArticle()
		public override void LoadArticle(BscanILL.Hierarchy.Article article)
		{
			base.LoadArticle(article);

			string id = GetExportFileNamePrefixPreferringIllNumber(article);
			string subject = _settings.Export.Email.Subject;
			string body = _settings.Export.Email.Body;

            this.FileNamePrefix = GetExportFileNamePrefixPreferringIllNumber(article);  

			if (article.TransactionId != null)
				subject = subject.Replace("{TN}", article.TransactionId.ToString());
			if (article.IllNumber != null)
				subject = subject.Replace("{IN}", article.IllNumber);
			if (article.Patron != null)
				subject = subject.Replace("{PA}", article.Patron);
			if (article.Address != null)
				subject = subject.Replace("{AD}", article.Address);

			if (article.TransactionId != null)
				body = body.Replace("{TN}", article.TransactionId.ToString());
			if (article.IllNumber != null)
				body = body.Replace("{IN}", article.IllNumber);
			if (article.Patron != null)
				body = body.Replace("{PA}", article.Patron);
			if (article.Address != null)
				body = body.Replace("{AD}", article.Address);

			this.Recipient = article.Address;
			this.MessageSubject = subject;
			this.MessageBody = body;
            fileformatCtrl.FileFormat = _settings.Export.Email.FileFormat;

            this.fileparamCtrl.FileColor = _settings.Export.Email.FileExportColorMode;
            this.fileparamCtrl.FileQuality = _settings.Export.Email.FileExportQuality;

		}
		#endregion

		#region GetAdditionalInfo()
		public BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo()
		{
			return new BscanILL.Export.AdditionalInfo.AdditionalEmail(this.Recipient, this.MessageSubject, this.MessageBody, this.FileNamePrefix, this.FileFormat,
                this.MultiImage, this.PdfA, this.IncludePullslip, this.UpdateILLiad, this.ChangeStatusToRequestFinished, this.FileColor, this.FileQuality);
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

		#region CheckUpdateILLiad_CheckedChanged()
		private void CheckUpdateILLiad_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.checkChangeStatusToRequestFinished.IsEnabled = checkUpdateILLiad.IsChecked.Value;
		}
		#endregion

		#endregion

	}
}
