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

namespace BscanILL.UI.Settings
{
	/// <summary>
	/// Interaction logic for AddIllUsbDriveDlg.xaml
	/// </summary>
	public partial class AddIllUsbDriveDlg : Window
	{
		List<string> arrayOfAlreadyUsedNames;
		
		private AddIllUsbDriveDlg(List<string> arrayOfAlreadyUsedNames)
		{
			InitializeComponent();

			this.arrayOfAlreadyUsedNames = arrayOfAlreadyUsedNames;
		}

		//PUBLIC METHODS
		#region public methods

		/// <summary>
		/// if the dialog is canceled, it returns null;
		/// </summary>
		/// <returns></returns>
		public static string GetNewUser(List<string> arrayOfAlreadyUsedNames)
		{
			AddIllUsbDriveDlg dlg = new AddIllUsbDriveDlg(arrayOfAlreadyUsedNames);
			
			if (dlg.ShowDialog() == true)
				return dlg.textBoxName.Text;
			else
				return null;
		}


		#endregion

		//PRIVATE METHODS
		#region private methods

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			foreach (string alreadyUsedName in arrayOfAlreadyUsedNames)
				if (alreadyUsedName.ToLower() == this.textBoxName.Text.ToLower())
				{
					MessageBox.Show("The name '" + this.textBoxName.Text + "' is already used!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

			this.DialogResult = true;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.buttonOk.IsEnabled = (this.textBoxName.Text.Trim().Length >= 5);
		}

		#endregion

	}
}
