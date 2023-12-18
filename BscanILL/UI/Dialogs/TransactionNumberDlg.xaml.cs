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

namespace BscanILL.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for TransactionNumberDlg.xaml
	/// </summary>
	public partial class TransactionNumberDlg : Window
	{
		public TransactionNumberDlg()
		{
			InitializeComponent();
		}

		//PUBLIC METHODS
		#region public methods

		public int? Open(BscanILL.Hierarchy.Article article)
		{
			this.textIllNumber.Text = article.IllNumber;
			this.textPatron.Text = article.Patron;
			this.textAddress.Text = article.Address;
			this.textDeliveryMethod.Text = article.ExportType.ToString();

			if (this.ShowDialog() == true)
				return int.Parse(textTransactionNumber.Text);

			return null;
		}

		#endregion

		// PRIVATE METHODS
		#region private methods

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.buttonOk.IsEnabled = textTransactionNumber.Text.Length > 0;

		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int.Parse(textTransactionNumber.Text);
				this.DialogResult = true;
			}
			catch
			{
				MessageBox.Show("Enter valid Transaction number!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

		}

		#endregion

	}
}
