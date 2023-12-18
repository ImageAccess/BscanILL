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
using System.ComponentModel;
using BscanILL.Export.Printing;


namespace BscanILL.UI.Settings.Dialogs
{
	/// <summary>
	/// Interaction logic for PrinterDlg.xaml
	/// </summary>
	public partial class PrinterDlg : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public delegate void PrintTestSheetHnd(BscanILL.Export.Printing.PrinterProfile printerProfile);
		public event PrintTestSheetHnd PrintTestSheet;


		#region constructor
		public PrinterDlg()
		{
			InitializeComponent();

			this.DataContext = this;

			foreach (IIllPrinter printer in BscanILL.Export.Printing.IllPrinters.GetPrinters(BscanILL.SETTINGS.Settings.Instance.Export.Printer.PrintFunctionality))
			{
				comboPrinter.Items.Add(printer);
			}
		}
		#endregion
		

		//PUBLIC PROPERTIES
		#region public properties
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Open()
		public BscanILL.Export.Printing.PrinterProfile Open()
		{
			IIllPrinter printer = IllPrinters.GetDefaultPrinter(BscanILL.SETTINGS.Settings.Instance.Export.Printer.PrintFunctionality);

			foreach (IIllPrinter queue in comboPrinter.Items)
				if (queue.Name == printer.Name)
					comboPrinter.SelectedItem = queue;

			/*foreach (PrintSizeComboItem comboSizeItem in this.comboSize.Items)
				if (comboSizeItem.Value.PageMediaSizeName == PageMediaSizeName.NorthAmericaLetter)
					this.comboSize.SelectedItem = comboSizeItem;

			foreach (InputBin inputBin in this.comboTray.Items)
				if (inputBin.ToString() == InputBin.AutoSelect.ToString())
					this.comboTray.SelectedItem = inputBin;*/

			/*if (this.comboSize.SelectedItem == null && this.comboSize.Items.Count > 0)
				this.comboSize.SelectedIndex = 0;

			if (this.comboTray.SelectedItem == null && this.comboTray.Items.Count > 0)
				this.comboTray.SelectedIndex = 0;*/

			if (this.ShowDialog() == true)
				return GetPrinterProfile();
			else
				return null;
		}

		public bool Open(BscanILL.Export.Printing.PrinterProfile printerProfile)
		{
			IIllPrinter printer = printerProfile.GetPrinter();

			if (printer == null)
				printer = IllPrinters.GetDefaultPrinter(BscanILL.SETTINGS.Settings.Instance.Export.Printer.PrintFunctionality);

			if (printer != null)
			{
				foreach (IIllPrinter queue in comboPrinter.Items)
					if (queue.Name == printer.Name)
					{
						comboPrinter.SelectedItem = queue;
						break;
					}

				textBoxName.Text = printerProfile.Description;
			}

			if (this.ShowDialog() == true)
			{
				printerProfile.Set(textBoxName.Text, ((IIllPrinter)comboPrinter.SelectedItem).Name, 
					(comboSize.SelectedItem != null) ? ((BscanILL.Export.Printing.IPaperSize)comboSize.SelectedItem).Key : "",
					(comboTray.SelectedItem != null) ? ((BscanILL.Export.Printing.IInputBin)comboTray.SelectedItem).Key : "",
					BscanILL.SETTINGS.Settings.Instance.Export.Printer.PrintFunctionality);

				printerProfile.InvalidatePrinterQueue();
				return true;
			}
			else
				return false;
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region PrintTestSheet_Click()
		private void PrintTestSheet_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				BscanILL.Export.Printing.PrinterProfile printerProfile = GetPrinterProfile();

				if (PrintTestSheet != null)
					PrintTestSheet(printerProfile);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region ComboPrinter_SelectionChanged()
		private void ComboPrinter_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.comboSize.Items.Clear();
			this.comboTray.Items.Clear();

			if (this.comboPrinter.SelectedItem != null && this.comboPrinter.SelectedItem is IIllPrinter)
			{
				IIllPrinter printer = (IIllPrinter)this.comboPrinter.SelectedItem;

				textBoxName.Text = printer.Name;

				foreach (BscanILL.Export.Printing.IPaperSize size in printer.PaperSizes)
					this.comboSize.Items.Add(size);

				foreach (BscanILL.Export.Printing.IInputBin tray in printer.InputBins)
					this.comboTray.Items.Add(tray);

				if (this.comboSize.SelectedItem == null && this.comboSize.Items.Count > 0)
					this.comboSize.SelectedIndex = 0;

				if (this.comboTray.SelectedItem == null && this.comboTray.Items.Count > 0)
					this.comboTray.SelectedIndex = 0;

				if (printer.DuplexAvailable)
				{
					//checkDuplex.IsEnabled = true;
					//checkDuplex.IsChecked = true;
				}
				else
				{
					//checkDuplex.IsEnabled = false;
					//checkDuplex.IsChecked = false;
				}
			}

			Evaluate();
		}
		#endregion

		#region RaisePropertyChanged()
		private void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		#region Ok_Click()
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
		#endregion

		#region GetPrinterProfile()
		private BscanILL.Export.Printing.PrinterProfile GetPrinterProfile()
		{
			if (comboPrinter.SelectedItem != null && comboSize.SelectedItem != null/* && comboTray.SelectedItem != null*/)
			{
				if (comboTray.SelectedItem != null)
				{
					return new BscanILL.Export.Printing.PrinterProfile(
					textBoxName.Text,
					((IIllPrinter)comboPrinter.SelectedItem).Name,
					((BscanILL.Export.Printing.IPaperSize)comboSize.SelectedItem).Key,
					((BscanILL.Export.Printing.IInputBin)comboTray.SelectedItem).Key,
					BscanILL.SETTINGS.Settings.Instance.Export.Printer.PrintFunctionality);
				}
				else
				{
					return new BscanILL.Export.Printing.PrinterProfile(
					textBoxName.Text,
					((IIllPrinter)comboPrinter.SelectedItem).Name,
					((BscanILL.Export.Printing.IPaperSize)comboSize.SelectedItem).Key,
					"",
					BscanILL.SETTINGS.Settings.Instance.Export.Printer.PrintFunctionality);
				}
			}
			else
				throw new Exception("Select printer, paper tray and paper size first!");
		}
		#endregion

		#region TextBoxDescription_TextChanged()
		private void TextBoxDescription_TextChanged(object sender, TextChangedEventArgs e)
		{
			Evaluate();
		}
		#endregion

		#region Combo_SelectionChanged()
		private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Evaluate();
		}
		#endregion

		#region Evaluate()
		private void Evaluate()
		{
			if (this.buttonOk != null && this.buttonPrintTestSheet != null)
			{
				if (textBoxName != null && comboPrinter != null)
				{
					this.buttonOk.IsEnabled = (textBoxName.Text.Trim().Length > 0) && (comboPrinter.SelectedItem != null) && (comboSize.SelectedItem != null)/* && (comboTray.SelectedItem != null)*/;
					this.buttonPrintTestSheet.IsEnabled = this.buttonOk.IsEnabled;
				}
				else
				{
					this.buttonOk.IsEnabled = false;
					this.buttonPrintTestSheet.IsEnabled = false;
				}
			}
		}
		#endregion

		#region Form_Loaded()
		private void Form_Loaded(object sender, RoutedEventArgs e)
		{
			Evaluate();
		}
		#endregion

		#endregion

	}
}
