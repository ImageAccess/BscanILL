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
using System.Security;
using System.Runtime.InteropServices;

namespace BscanILL.UI.Settings.Dialogs
{
	/// <summary>
	/// Interaction logic for PrintFunctionalityDlg.xaml
	/// </summary>
	public partial class PrintFunctionalityDlg : Window
	{
		BscanILL.SETTINGS.Settings _settings = BscanILL.SETTINGS.Settings.Instance;
		int			selectedIndex = 0;


		#region constructor
		public PrintFunctionalityDlg()
		{
			InitializeComponent();

			this.DataContext = this;
		}
		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Open()
		public void Open()
		{
			this.selectedIndex = (_settings.Export.Printer.PrintFunctionality == BscanILL.Export.Printing.Functionality.Xps) ? 0 : 1;
			this.combo.SelectedIndex = selectedIndex;
			
			if (ShowDialog() == true)
			{
				if (this.combo.SelectedIndex != selectedIndex)
				{
					_settings.Export.Printer.PrintFunctionality = (this.combo.SelectedIndex == 0) ? BscanILL.Export.Printing.Functionality.Xps : BscanILL.Export.Printing.Functionality.Win32;

					_settings.Export.Printer.PrinterProfiles.Clear();
				}
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Ok_Click()
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
		#endregion

		#region Combo_SelectionChanged()
		private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetOkButton();
		}
		#endregion

		#region SetOkButton()
		private void SetOkButton()
		{
			if (this.buttonOk != null && this.combo != null)
				this.buttonOk.IsEnabled = (this.combo.SelectedIndex != this.selectedIndex);
		}
		#endregion

		#region Window_Loaded()
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SetOkButton();
		}
		#endregion

		#endregion

	}
}
