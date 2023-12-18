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

namespace BscanILL.UI.Settings.Dialogs
{
	/// <summary>
	/// Interaction logic for ArticleExchangeRecipientsDlg.xaml
	/// </summary>
	public partial class ArticleExchangeRecipientsDlg : Window
	{

		#region constructor
		public ArticleExchangeRecipientsDlg()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#region Items
		public List<string> Items
		{
			get
			{
				List<string> items = new List<string>();

				foreach (string item in listBoxRecipients.Items)
					items.Add(item);

				return items;
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Open()
		public bool Open(ObservableCollection<string> list)
		{
			try
			{
				foreach (string item in list)
					this.listBoxRecipients.Items.Add(item);

				return this.ShowDialog().Value;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region TextBox_TextChanged()
		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				buttonAdd.IsEnabled = BscanILL.Export.Email.Email.IsValidEmail(textBox.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Ok_Click()
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.DialogResult = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Add_Click()
		private void Add_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				listBoxRecipients.Items.Add(this.textBox.Text);
				this.textBox.Text = "";
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Remove_Click()
		private void Remove_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (listBoxRecipients.SelectedIndex >= 0)
					listBoxRecipients.Items.RemoveAt(listBoxRecipients.SelectedIndex);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region ListBox_SelectionChanged()
		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				buttonRemove.IsEnabled = (listBoxRecipients.SelectedItems.Count > 0);

				if (listBoxRecipients.SelectedItem != null)
					this.textBox.Text = (string)listBoxRecipients.SelectedItem;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#endregion

	}
}
