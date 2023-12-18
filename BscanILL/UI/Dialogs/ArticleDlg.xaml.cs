#define TransNumber_LONG

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
using System.IO;

namespace BscanILL.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for ArticleDlg.xaml
	/// </summary>
	public partial class ArticleDlg : Window
	{
		//BscanILL.Hierarchy.Article article = null;
		BscanILL.SETTINGS.Settings _settings = BscanILL.SETTINGS.Settings.Instance;

		#region constructor
		public ArticleDlg()
		{
			InitializeComponent();

			foreach (ComboItemExportType item in ComboItemsExportType.GetList())
				this.comboDeliveryMethod.Items.Add(item);
		}
		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Open()
		public void Open(BscanILL.Hierarchy.Article article)
		{
			this.flagAddressErr.Visibility = System.Windows.Visibility.Hidden;
			
			if (article != null)
			{
				this.textId.Text = article.Id.ToString();
				this.textPatron.Text = article.Patron;
				this.textAddress.Text = article.Address;
				this.textIllNumber.Text = article.IllNumber;
				this.textTransactionNumber.Text = article.TransactionId.HasValue ? article.TransactionId.Value.ToString() : "";
				SelectComboItemExportType(article.ExportType);
			}
			else
			{
				this.textId.Text = "Undefined";
				this.textPatron.Text = "";
				this.textAddress.Text = "";
				this.textIllNumber.Text = "";
				this.textTransactionNumber.Text = "";
				SelectComboItemExportType(BscanILL.Export.ExportType.SaveOnDisk);
			}

			if (this.ShowDialog().Value)
			{
#if TransNumber_LONG
                long? transactionId = null;
#else
				int? transactionId = null;
#endif
                

				if (this.textTransactionNumber.Text.Trim().Length > 0)
				{
#if TransNumber_LONG
                    long temp;
                    if (long.TryParse(this.textTransactionNumber.Text, out temp))
#else
                    int temp;
					if (int.TryParse(this.textTransactionNumber.Text, out temp))
#endif
						transactionId = temp;
				}

				article.Set(transactionId, this.textIllNumber.Text.Trim(), this.textPatron.Text.Trim(), this.textAddress.Text.Trim(),
					((ComboItemExportType)this.comboDeliveryMethod.SelectedItem).Value);
			}
		}
		#endregion

		#region Open()
		public bool Open(BscanILL.FP.FormsProcessingResult fpResult)
		{
            if( fpResult.AddressFlag) 
            {
                this.flagAddressErr.Visibility = System.Windows.Visibility.Visible; 
            }
            else
            {
                this.flagAddressErr.Visibility = System.Windows.Visibility.Hidden;
            }
            			
			this.textBlockId.Visibility = System.Windows.Visibility.Hidden;
			this.textId.Text = "";
			this.textPatron.Text = fpResult.PatronName;
			this.textAddress.Text = fpResult.Address;
			this.textIllNumber.Text = fpResult.IllNumber;
			this.textTransactionNumber.Text = fpResult.TN.HasValue ? fpResult.TN.Value.ToString() : "";
			SelectComboItemExportType(fpResult.DeliveryMethod);

			if (this.ShowDialog().Value)
			{
#if TransNumber_LONG
                long? transactionId = null;
#else
				int? transactionId = null;
#endif
                

				if (this.textTransactionNumber.Text.Trim().Length > 0)
				{
#if TransNumber_LONG
                    long temp;
					if (long.TryParse(this.textTransactionNumber.Text, out temp))
#else
					int temp;
					if (int.TryParse(this.textTransactionNumber.Text, out temp))
#endif
						transactionId = temp;
				}

				fpResult.TN = transactionId;
				fpResult.IllNumber = this.textIllNumber.Text.Trim();
				fpResult.PatronName = this.textPatron.Text.Trim();
				fpResult.Address = this.textAddress.Text.Trim();
				fpResult.DeliveryMethod = ((ComboItemExportType)this.comboDeliveryMethod.SelectedItem).Value;
				return true;
			}

			return false;
		}
		#endregion

		#region Open()
		public BscanILL.Hierarchy.Article Open()
		{
			this.textId.Text = "Undefined";
			this.textPatron.Text = "";
			this.textAddress.Text = "";
			this.textIllNumber.Text = "";
			this.textTransactionNumber.Text = "";
			SelectComboItemExportType(BscanILL.Export.ExportType.ILLiad);

			if (this.ShowDialog().Value)
			{
#if TransNumber_LONG
                long? transactionId = null;
#else
				int? transactionId = null;
#endif                

				if (this.textTransactionNumber.Text.Trim().Length > 0)
				{
#if TransNumber_LONG
					long temp;
					if (long.TryParse(this.textTransactionNumber.Text, out temp))
#else
					int temp;
					if (int.TryParse(this.textTransactionNumber.Text, out temp))
#endif
						transactionId = temp;
				}

				BscanILLData.Models.Helpers.NewDbArticle newDbArticle = new BscanILLData.Models.Helpers.NewDbArticle
				{
#if TransNumber_LONG                    
                    TransactionNumberBig = transactionId,
#else
					TransactionNumber = transactionId,
#endif
					IllNumber = this.textIllNumber.Text.Trim(),
					Patron = this.textPatron.Text.Trim(),
					Address = this.textAddress.Text.Trim(),
					ExportType = (BscanILLData.Models.ExportType)((ComboItemExportType)this.comboDeliveryMethod.SelectedItem).Value,
					Status = (BscanILLData.Models.ArticleStatus)BscanILL.Hierarchy.ArticleStatus.Creating
				};

				BscanILLData.Models.DbArticle dbArticle = BscanILL.DB.Database.Instance.InsertArticle(newDbArticle);
				string parentDir = _settings.General.ArticlesDir;
				string dirNameBase = dbArticle.Id.ToString("00000000");
				string dirName = dirNameBase;
				int index = 1;

				while (BscanILL.Misc.Io.DirectoryExists(parentDir + @"\" + dirName))
				{
					dirName = dirNameBase + "_" + index.ToString();
					index++;
				}

				Directory.CreateDirectory(parentDir + @"\" + dirName);
				dbArticle.FolderName = dirName;

				BscanILL.Hierarchy.Article article = new BscanILL.Hierarchy.Article(dbArticle);
				article.Status = Hierarchy.ArticleStatus.Active;
				return article;
			}
			else
			{
				return null;
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region OK_Click()
		private void OK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
                this.flagAddressErr.Visibility = System.Windows.Visibility.Hidden;

                bool addressCorrect = true;
                if (_settings.Export.Email.EmailValidation)
                {
                    if ((this.textAddress.Text.Length > 0) && (string.Compare(this.textAddress.Text.Trim(), "N/A") != 0))
                    {
                      addressCorrect = BscanILL.Misc.EmailValidator.IsAddressValid(this.textAddress.Text.Trim(), ((ComboItemExportType)this.comboDeliveryMethod.SelectedItem).Value,
                                       _settings.Export, _settings.Export.FtpServer.SendConfirmationEmail, _settings.Export.FtpDirectory.SendConfirmationEmail);
                    }
                }
                
                if (addressCorrect)
                {
                    if (this.textTransactionNumber.Text.Trim().Length > 0)
#if TransNumber_LONG
                        long.Parse(this.textTransactionNumber.Text);
#else
                        int.Parse(this.textTransactionNumber.Text);
#endif                        
                    this.DialogResult = true;
                }
                else
                {
                    this.flagAddressErr.Visibility = System.Windows.Visibility.Visible;
                    //MessageBox.Show("Invalid Email Address Value!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AlertDlg.Show("Invalid Email Address Value!", AlertDlg.AlertDlgType.Error);
                }
			}
			catch (Exception)
			{
				//MessageBox.Show("Can't parse Transaction Number into number!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                AlertDlg.Show("Can't parse Transaction Number into number!", AlertDlg.AlertDlgType.Error);
			}
		}
		#endregion

		#region SelectComboItemExportType()
		private void SelectComboItemExportType(BscanILL.Export.ExportType exportType)
		{
			foreach (ComboItemExportType item in this.comboDeliveryMethod.Items)
				if (item.Value == exportType)
				{
					this.comboDeliveryMethod.SelectedItem = item;
					break;
				}	
		}
		#endregion

		#endregion

	}
}
