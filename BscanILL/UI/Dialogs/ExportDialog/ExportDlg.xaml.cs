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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Reflection;

namespace BscanILL.UI.Dialogs.ExportDialog
{
	/// <summary>
	/// Interaction logic for ExportDlg.xaml
	/// </summary>
	public partial class ExportDlg : DialogBase
	{
		BscanILL.Hierarchy.Article		article;
		BscanILL.Export.ExportType		selectedExport = BscanILL.Export.ExportType.None;
    bool exportDialogEnabled = true;		
        bool yesAllWasPressed = false;
        bool previousWasPressed = false;

		#region constructor
        public ExportDlg(BscanILL.UI.Frames.Export.FrameExport frameExport)
		{
			InitializeComponent();

			foreach (ComboItemExportType item in ComboItemsExportType.GetList())
				this.comboOutputMethod.Items.Add(item);

			this.DataContext = this;

            if (frameExport != null)
            {
                frameExport.SendAll_ExportClick += new BscanILL.UI.Frames.Export.FrameExport.BoolHnd(SendALL_Pressed);
                frameExport.Previous_ExportClick += new BscanILL.UI.Frames.Export.FrameExport.BoolHnd(Previous_Pressed);                
            }
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
	
		#region ExportType
		public BscanILL.Export.ExportType ExportType
		{
			get 
			{
				return selectedExport; 
			}
			set
			{
				if (this.selectedExport != value)
				{
					this.selectedExport = value;

                    if (this.exportDialogEnabled)
                    {
                        foreach (ComboItemExportType item in comboOutputMethod.Items)
                        {
                            if (item.Value == this.selectedExport)
                            {
                                comboOutputMethod.SelectedItem = item;
                                break;
                            }
                        }

                        panelAriel.Visibility = (this.selectedExport == Export.ExportType.Ariel) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelILLiad.Visibility = (this.selectedExport == Export.ExportType.ILLiad) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelOdyssey.Visibility = (this.selectedExport == Export.ExportType.Odyssey) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelEmail.Visibility = (this.selectedExport == Export.ExportType.Email) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelFtp.Visibility = (this.selectedExport == Export.ExportType.Ftp) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelFtpDirectory.Visibility = (this.selectedExport == Export.ExportType.FtpDir) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelPrinter.Visibility = (this.selectedExport == Export.ExportType.Print) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelSaveOnDisk.Visibility = (this.selectedExport == Export.ExportType.SaveOnDisk) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelArticleExchange.Visibility = (this.selectedExport == Export.ExportType.ArticleExchange) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelTipasa.Visibility = (this.selectedExport == Export.ExportType.Tipasa) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelWorldShareILL.Visibility = (this.selectedExport == Export.ExportType.WorldShareILL) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        panelRapido.Visibility = (this.selectedExport == Export.ExportType.Rapido) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                        //  }

                        if (panelSaveOnDisk.Visibility == Visibility.Visible)
                        {
                            panelSaveOnDisk.BrowseOutputPath(this.exportDialogEnabled, this.TNValue, this.ILLValue);
                        }
                        if (panelFtpDirectory.Visibility == Visibility.Visible)
                        {
                            panelFtpDirectory.BrowseOutputPath(this.exportDialogEnabled, this.TNValue, this.ILLValue);
                        }

                    }
                    else
                    {
                        if (this.selectedExport == Export.ExportType.SaveOnDisk)
                        {
                            panelSaveOnDisk.BrowseOutputPath(this.exportDialogEnabled, this.TNValue, this.ILLValue);
                        }
                        if (this.selectedExport == Export.ExportType.FtpDir)
                        {
                            panelFtpDirectory.BrowseOutputPath(this.exportDialogEnabled, this.TNValue, this.ILLValue);
                        }
                    }

					       RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
					       
                    if (this.exportDialogEnabled)
                    {
                        this.InvalidateVisual();
                    }										
				}
			}
		}
		#endregion

		#region ExportTypeItem
		public ComboItemExportType ExportTypeItem
		{
			get 
			{
				foreach (ComboItemExportType item in comboOutputMethod.Items)
					if (item.Value == this.ExportType)
						return item;
			
				return null; 
			}
			set
			{
				if (value != null)
				{
					this.ExportType = value.Value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#region IPanel
		IPanel IPanel
		{
			get
			{
				switch (this.ExportType)
				{
					case Export.ExportType.Ariel: return panelAriel;
					case Export.ExportType.ILLiad: return panelILLiad;
					case Export.ExportType.Odyssey: return panelOdyssey;
					case Export.ExportType.Email: return panelEmail;
					case Export.ExportType.Ftp: return panelFtp;
					case Export.ExportType.FtpDir: return panelFtpDirectory;
					case Export.ExportType.SaveOnDisk: return panelSaveOnDisk;
					case Export.ExportType.ArticleExchange: return panelArticleExchange;
                    case Export.ExportType.Tipasa: return panelTipasa;
                    case Export.ExportType.WorldShareILL: return panelWorldShareILL;
                    case Export.ExportType.Rapido: return panelRapido;
                    case Export.ExportType.Print: return panelPrinter;
					default: throw new Exception("Unsupported IPanel!");
				}
			}
		}
		#endregion

        #region IPanelNoDialog
        IPanel IPanelNoDialog
        {
            get
            {
                switch (this.article.ExportType)
                {
                    case Export.ExportType.Ariel: return panelAriel;
                    case Export.ExportType.ILLiad: return panelILLiad;
                    case Export.ExportType.Odyssey: return panelOdyssey;
                    case Export.ExportType.Email: return panelEmail;
                    case Export.ExportType.Ftp: return panelFtp;
                    case Export.ExportType.FtpDir: return panelFtpDirectory;
                    case Export.ExportType.SaveOnDisk: return panelSaveOnDisk;
                    case Export.ExportType.ArticleExchange: return panelArticleExchange;
                    case Export.ExportType.Tipasa: return panelTipasa;
                    case Export.ExportType.WorldShareILL: return panelWorldShareILL;
                    case Export.ExportType.Rapido: return panelRapido;
                    case Export.ExportType.Print: return panelPrinter;
                    default: throw new Exception("Unsupported IPanel!");
                }
            }
        }
        #endregion

        #region TNValue
        public string TNValue
        {
            get { return this.article.TransactionId.ToString(); }            
        }
        #endregion

        #region ILLValue
        public string ILLValue
        {
            get { return this.article.IllNumber; }
        }
        #endregion

        #region PatronValue
        public string PatronValue
        {
            get { return this.article.Patron; }
        }
        #endregion

        #region AddressValue
        public string AddressValue
        {
            get { return this.article.Address; }
        }
        #endregion

		#endregion


		//PUBLIC METHODS
		#region public methods


        public bool Previous_Pressed()
        {
            return previousWasPressed;
        }

        public bool SendALL_Pressed()
        {
            return yesAllWasPressed;
        }

		#region Open()
        public BscanILL.Export.ExportTempUnit OpenTemp(BscanILL.Hierarchy.Article article, int articleOrder, int articlesTotalCounter, bool ExportDialogEnabled )
		{
			this.article = article;

			this.panelAriel.LoadArticle(this.article);
			this.panelILLiad.LoadArticle(this.article);
			this.panelOdyssey.LoadArticle(this.article);
			this.panelEmail.LoadArticle(this.article);
			this.panelFtp.LoadArticle(this.article);
			this.panelFtpDirectory.LoadArticle(this.article);
			this.panelPrinter.LoadArticle(this.article);
			this.panelSaveOnDisk.LoadArticle(this.article);
			this.panelArticleExchange.LoadArticle(this.article);
            this.panelTipasa.LoadArticle(this.article);
            this.panelWorldShareILL.LoadArticle(this.article);
            this.panelRapido.LoadArticle(this.article);

            this.exportDialogEnabled = ExportDialogEnabled;

            if( ExportDialogEnabled )
            {
                if (articlesTotalCounter > 1)
                {
                    this.articleNumber.Text = "Article:  " + articleOrder + " of " + articlesTotalCounter;
                    this.articleNumber.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.articleNumber.Visibility = System.Windows.Visibility.Collapsed;
                }

                if ((articleOrder > 1) && (articlesTotalCounter > 1))
                {
                    this.buttonGoToPrevious.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.buttonGoToPrevious.Visibility = System.Windows.Visibility.Collapsed;
                }

                if (articleOrder != articlesTotalCounter)
                {
                    this.buttonSendCurrent.Visibility = System.Windows.Visibility.Visible;
                    this.buttonSendAll.Content = "Finish";
                }
                else
                {                    
                    this.buttonSendCurrent.Visibility = System.Windows.Visibility.Collapsed;                    
                    //this.buttonSendAll.Text = "Send";
                    this.buttonSendAll.Content = "Send";
                }

                if( this.article.TransactionId == null)
                {
                   this.boxTN.Visibility = System.Windows.Visibility.Collapsed;
                }
                if( this.article.IllNumber.Length == 0)
                {
                   this.boxILL.Visibility = System.Windows.Visibility.Collapsed;
                }                    
                if( this.article.Patron.Length == 0)
                {
                   this.boxPatron.Visibility = System.Windows.Visibility.Collapsed;
                }
                if( this.article.Address.Length == 0)
                {
                    this.boxAddress.Visibility = System.Windows.Visibility.Collapsed;
                }                    
                                       
			    if (this.ShowDialog() == true)
			    {
				    IPanel iPanel = this.IPanel;

				    BscanILLData.Models.Helpers.NewDbExport newDbExport = new BscanILLData.Models.Helpers.NewDbExport()
				    {
					    ExportType = (BscanILLData.Models.ExportType)this.ExportType,
					    fArticleId = (int)this.article.Id,
					    FileFormat = (BscanILLData.Models.ExportFileFormat)iPanel.FileFormat,
					    FileNamePrefix = iPanel.FileNamePrefix,
					    FolderName = this.article.GetNewExportFolderName().Name,
					    MultiImage = iPanel.MultiImage,
					    PdfA = iPanel.PdfA,
					    Status = BscanILLData.Models.ExportStatus.Created
				    };

                    //per Diana - overwrite ExportType field  (read of the pull slip) with modified value from export dialog so it is shown updated in the resend table
                    // maybe in the future, add LastExportType field into dbArticle or just when Refresh() method called in REsend dialog, just lookup latest export type in ExprotUnit list and this value use in ExportType column
                    this.article.SetExportType(this.ExportType);

				    //BscanILLData.Models.DbExport dbExort = BscanILL.DB.Database.Instance.InsertExport(newDbExport);
				    //BscanILL.Export.ExportUnit exportUnit = new BscanILL.Export.ExportUnit(this.article, dbExort);
                    BscanILL.Export.ExportTempUnit exportTempUnit = new BscanILL.Export.ExportTempUnit(this.article, newDbExport, iPanel.GetAdditionalInfo());
				    //this.article.ExportUnits.Add(exportUnit);
				
				    //exportUnit.AdditionalInfo = iPanel.GetAdditionalInfo();

                    return exportTempUnit;
			    }
			    else
				    return null;
            }
            else
            {
                IPanel iPanel = this.IPanelNoDialog;
                this.ExportType = article.ExportType;  //in case FtpDir or SaveOnDisk ->make sure destination path is not blank                
                BscanILLData.Models.Helpers.NewDbExport newDbExport = new BscanILLData.Models.Helpers.NewDbExport()
                {
                    ExportType = (BscanILLData.Models.ExportType)this.article.ExportType,
                    fArticleId = (int)this.article.Id,
                    FileFormat = (BscanILLData.Models.ExportFileFormat)iPanel.FileFormat,
                    FileNamePrefix = iPanel.FileNamePrefix,
                    FolderName = this.article.GetNewExportFolderName().Name,
                    MultiImage = iPanel.MultiImage,
                    PdfA = iPanel.PdfA,
                    Status = BscanILLData.Models.ExportStatus.Created
                };

                //per Diana - overwrite ExportType field  (read of the pull slip) with modified value from export dialog so it is shown updated in the resend table
                // maybe in the future, add LastExportType field into dbArticle or just when Refresh() method called in REsend dialog, just lookup latest export type in ExprotUnit list and this value use in ExportType column
                //this.article.SetExportType(this.article.ExportType);

                //BscanILLData.Models.DbExport dbExort = BscanILL.DB.Database.Instance.InsertExport(newDbExport);
                //BscanILL.Export.ExportUnit exportUnit = new BscanILL.Export.ExportUnit(this.article, dbExort);
                BscanILL.Export.ExportTempUnit exportTempUnit = new BscanILL.Export.ExportTempUnit(this.article, newDbExport, iPanel.GetAdditionalInfo());
                //this.article.ExportUnits.Add(exportUnit);

                //exportUnit.AdditionalInfo = iPanel.GetAdditionalInfo();
                this.Close();   //with this line, code does not freezes                
                return exportTempUnit;
            }

		}
				
        public BscanILL.Export.ExportUnit Open(BscanILL.Hierarchy.Article article, int articleOrder, int articlesTotalCounter, bool ExportDialogEnabled)
        {
            this.article = article;

            this.panelAriel.LoadArticle(this.article);
            this.panelILLiad.LoadArticle(this.article);
            this.panelOdyssey.LoadArticle(this.article);
            this.panelEmail.LoadArticle(this.article);
            this.panelFtp.LoadArticle(this.article);
            this.panelFtpDirectory.LoadArticle(this.article);
            this.panelPrinter.LoadArticle(this.article);
            this.panelSaveOnDisk.LoadArticle(this.article);
            this.panelArticleExchange.LoadArticle(this.article);
            this.panelTipasa.LoadArticle(this.article);
            this.panelWorldShareILL.LoadArticle(this.article);
            this.panelRapido.LoadArticle(this.article);

            if (ExportDialogEnabled)
            {
                if (articlesTotalCounter > 1)
                {
                    this.articleNumber.Text = "Article:  " + articleOrder + " of " + articlesTotalCounter;
                    this.articleNumber.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.articleNumber.Visibility = System.Windows.Visibility.Collapsed;
                }

                if ((articleOrder > 1) && (articlesTotalCounter > 1))
                {
                    this.buttonGoToPrevious.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.buttonGoToPrevious.Visibility = System.Windows.Visibility.Collapsed;
                }

                if (articleOrder != articlesTotalCounter)
                {
                    this.buttonSendCurrent.Visibility = System.Windows.Visibility.Visible;
                    this.buttonSendAll.Content = "Finish";
                }
                else
                {
                    this.buttonSendCurrent.Visibility = System.Windows.Visibility.Collapsed;
                    //this.buttonSendAll.Text = "Send";
                    this.buttonSendAll.Content = "Send";
                }

                if (this.article.TransactionId == null)
                {
                    this.boxTN.Visibility = System.Windows.Visibility.Collapsed;
                }
                if (this.article.IllNumber.Length == 0)
                {
                    this.boxILL.Visibility = System.Windows.Visibility.Collapsed;
                }
                if (this.article.Patron.Length == 0)
                {
                    this.boxPatron.Visibility = System.Windows.Visibility.Collapsed;
                }
                if (this.article.Address.Length == 0)
                {
                    this.boxAddress.Visibility = System.Windows.Visibility.Collapsed;
                }

                if (this.ShowDialog() == true)
                {
                    IPanel iPanel = this.IPanel;

                    BscanILLData.Models.Helpers.NewDbExport newDbExport = new BscanILLData.Models.Helpers.NewDbExport()
                    {
                        ExportType = (BscanILLData.Models.ExportType)this.ExportType,
                        fArticleId = (int)this.article.Id,
                        FileFormat = (BscanILLData.Models.ExportFileFormat)iPanel.FileFormat,
                        FileNamePrefix = iPanel.FileNamePrefix,
                        FolderName = this.article.GetNewExportFolderName().Name,
                        MultiImage = iPanel.MultiImage,
                        PdfA = iPanel.PdfA,
                        Status = BscanILLData.Models.ExportStatus.Created
                    };

                    //per Diana - overwrite ExportType field  (read of the pull slip) with modified value from export dialog so it is shown updated in the resend table
                    // maybe in the future, add LastExportType field into dbArticle or just when Refresh() method called in REsend dialog, just lookup latest export type in ExprotUnit list and this value use in ExportType column
                    this.article.SetExportType(this.ExportType);

                    BscanILLData.Models.DbExport dbExort = BscanILL.DB.Database.Instance.InsertExport(newDbExport);
                    BscanILL.Export.ExportUnit exportUnit = new BscanILL.Export.ExportUnit(this.article, dbExort);
                    this.article.ExportUnits.Add(exportUnit);

                    exportUnit.AdditionalInfo = iPanel.GetAdditionalInfo();

                    return exportUnit;
                }
                else
                    return null;
            }
            else
            {
                IPanel iPanel = this.IPanelNoDialog;
                BscanILLData.Models.Helpers.NewDbExport newDbExport = new BscanILLData.Models.Helpers.NewDbExport()
                {
                    ExportType = (BscanILLData.Models.ExportType)this.article.ExportType,
                    fArticleId = (int)this.article.Id,
                    FileFormat = (BscanILLData.Models.ExportFileFormat)iPanel.FileFormat,
                    FileNamePrefix = iPanel.FileNamePrefix,
                    FolderName = this.article.GetNewExportFolderName().Name,
                    MultiImage = iPanel.MultiImage,
                    PdfA = iPanel.PdfA,
                    Status = BscanILLData.Models.ExportStatus.Created
                };

                //per Diana - overwrite ExportType field  (read of the pull slip) with modified value from export dialog so it is shown updated in the resend table
                // maybe in the future, add LastExportType field into dbArticle or just when Refresh() method called in REsend dialog, just lookup latest export type in ExprotUnit list and this value use in ExportType column
                //this.article.SetExportType(this.article.ExportType);

                BscanILLData.Models.DbExport dbExort = BscanILL.DB.Database.Instance.InsertExport(newDbExport);
                BscanILL.Export.ExportUnit exportUnit = new BscanILL.Export.ExportUnit(this.article, dbExort);
                this.article.ExportUnits.Add(exportUnit);

                exportUnit.AdditionalInfo = iPanel.GetAdditionalInfo();
                return exportUnit;
            }

        }
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

        #region Prev_Click()
        private void Prev_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

                previousWasPressed = true;
				//if (SaveSettings())
					this.DialogResult = false;
			}
			catch (Exception ex)
			{
				AlertDlg.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion
	
		#region Ok_Click()
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

                bool addressCorrect = true;
                if (_settings.Export.Email.EmailValidation)
                {
                    string address = "";
                    bool confirmEmailFtpServer = this.panelFtp.SendConfirmEmail;
                    bool confirmEmailFtpDir = this.panelFtpDirectory.SendConfirmEmail;

                    if(this.boxAddress.Text.Length > 0)
                    {
                      address = this.boxAddress.Text;
                    }

                    //use email address field from export dialog in case user modified email address before exporting
                    if (((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value == Export.ExportType.Email)
                    {
                        address = this.panelEmail.Recipient;  
                    }
                    else
                    if (((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value == Export.ExportType.ArticleExchange)
                    {
                        address = this.panelArticleExchange.RecipientEmail;
                    }                    

                    if ((address.Length > 0) && (string.Compare(address.Trim(), "N/A") != 0))
                    {
                        addressCorrect = BscanILL.Misc.EmailValidator.IsAddressValid(address.Trim(), ((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value, _settings.Export, confirmEmailFtpServer, confirmEmailFtpDir);
                    }

                    if (addressCorrect)
                    {
                        //check confirmation address as well when Article Exchange
                        if (((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value == Export.ExportType.ArticleExchange)
                        {
                            if (this.panelArticleExchange.ConfirmEmailAddress != null)
                            {
                                //foreach( string confirmEmail in this.panelArticleExchange.ConfirmationEmails)
                                string confirmEmail = this.panelArticleExchange.ConfirmEmailAddress;
                                //{
                                if ((confirmEmail.Length > 0) && (string.Compare(confirmEmail.Trim(), "N/A") != 0))
                                {
                                    addressCorrect = BscanILL.Misc.EmailValidator.IsAddressValid(confirmEmail.Trim(), ((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value, _settings.Export, confirmEmailFtpServer, confirmEmailFtpDir);
                                    if (!addressCorrect)
                                    {
                                        AlertDlg.Show("Invalid Confirmation Email Address Value!", AlertDlg.AlertDlgType.Error);
                                        //        break;
                                    }
                                }
                                //}
                            }
                        } 
                    }
                    else
                    {
                        //MessageBox.Show("Invalid Email Address Value!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        AlertDlg.Show("Invalid Email Address Value!", AlertDlg.AlertDlgType.Error);
                    }
                }

                if (addressCorrect)
                {                    
                    //if (SaveSettings())
                    this.DialogResult = true;
                }
			}
			catch (Exception ex)
			{
				AlertDlg.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

        #region YesAll_Click()
        private void YesAll_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

                bool addressCorrect = true;
                if (_settings.Export.Email.EmailValidation)
                {
                    string address = "";
                    bool confirmEmailFtpServer = this.panelFtp.SendConfirmEmail;
                    bool confirmEmailFtpDir = this.panelFtpDirectory.SendConfirmEmail;

                    if (this.boxAddress.Text.Length > 0)
                    {
                        address = this.boxAddress.Text;
                    }

                    //use email address field from export dialog in case user modified email address before exporting
                    if (((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value == Export.ExportType.Email)
                    {
                        address = this.panelEmail.Recipient;
                    }
                    else
                        if (((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value == Export.ExportType.ArticleExchange)
                        {
                            address = this.panelArticleExchange.RecipientEmail;
                        }

                    if ((address.Length > 0) && (string.Compare(address.Trim(), "N/A") != 0))
                    {
                        addressCorrect = BscanILL.Misc.EmailValidator.IsAddressValid(address.Trim(), ((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value, _settings.Export, confirmEmailFtpServer, confirmEmailFtpDir);
                    }

                    if (addressCorrect)
                    {
                        //check confirmation address as well when Article Exchange
                        if (((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value == Export.ExportType.ArticleExchange)
                        {
                            if (this.panelArticleExchange.ConfirmEmailAddress != null)
                            {
                                //foreach (string confirmEmail in this.panelArticleExchange.ConfirmationEmails)
                                string confirmEmail = this.panelArticleExchange.ConfirmEmailAddress;
                                // {
                                if ((confirmEmail.Length > 0) && (string.Compare(confirmEmail.Trim(), "N/A") != 0))
                                {
                                    addressCorrect = BscanILL.Misc.EmailValidator.IsAddressValid(confirmEmail.Trim(), ((ComboItemExportType)this.comboOutputMethod.SelectedItem).Value, _settings.Export, confirmEmailFtpServer, confirmEmailFtpDir);
                                    if (!addressCorrect)
                                    {
                                        AlertDlg.Show("Invalid Confirmation Email Address Value!", AlertDlg.AlertDlgType.Error);
                                        //  break;
                                    }
                                }
                                // }
                            }
                        }
                    }
                    else
                    {
                        //MessageBox.Show("Invalid Email Address Value!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        AlertDlg.Show("Invalid Email Address Value!", AlertDlg.AlertDlgType.Error);
                    }
                }
                
                if (addressCorrect)
                {
                    yesAllWasPressed = true;

                    //if (SaveSettings())
                    this.DialogResult = true;
                }
			}
			catch (Exception ex)
			{
				AlertDlg.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region OnSourceInitialized()
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
		}
		#endregion

		#region Lock()
		public void Lock()
		{
			this.IsEnabled = false;
			this.Cursor = Cursors.Wait;
		}
		#endregion

		#region UnLock()
		public void UnLock()
		{
			this.IsEnabled = true;
			this.Cursor = null;
		}
		#endregion

		#region Form_Loaded()
		private void Form_Loaded(object sender, RoutedEventArgs e)
		{
			this.ExportType = article.ExportType;
		}
		#endregion

		#endregion

	}
}
