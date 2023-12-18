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
using System.Drawing.Printing;
using System.Reflection;


namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for Printers.xaml
	/// </summary>
	public partial class Printers : PanelBase
	{
		public event	BscanILL.UI.Settings.Dialogs.PrinterDlg.PrintTestSheetHnd		PrintTestSheet;

		#region constructor
		public Printers()
		{
			InitializeComponent();
			this.DataContext = this;

			if (_settings != null)
			{
				_settings.Export.Printer.PrinterProfiles.Validate();

				foreach (BscanILL.Export.Printing.PrinterProfile printerProfile in _settings.Export.Printer.PrinterProfiles)
					AddPrinter(printerProfile);

				this.PrintingEnabled = _settings.Export.Printer.PrinterProfiles.Count > 0;
				_settings.Export.Printer.PrinterProfiles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(PrinterProfiles_CollectionChanged);
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region PrintingEnabled
		public bool PrintingEnabled
		{
			set
			{
				this.checkBoxEnabled.IsChecked = value;
				this.groupBox.Visibility = (value) ? Visibility.Visible : Visibility.Hidden;
			}
		}
		#endregion

		#region SplitCopies
		public bool SplitCopies
		{
			get { return true;}// _settings.Export.Printer.SplitCopiesIntoSeparatePrintJobs; }
			set
			{
				//_settings.Export.Printer.SplitCopiesIntoSeparatePrintJobs = value;
				//RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Enabled_CheckedChanged()
		private void Enabled_CheckedChanged(object sender, RoutedEventArgs e)
		{
			try
			{
				if (checkBoxEnabled.IsChecked.Value)
				{
					if (_settings.Export.Printer.PrinterProfiles.Count == 0)
					{
						BscanILL.UI.Settings.Dialogs.PrinterDlg dlg = new BscanILL.UI.Settings.Dialogs.PrinterDlg();
						dlg.PrintTestSheet += new BscanILL.UI.Settings.Dialogs.PrinterDlg.PrintTestSheetHnd(PrintTestSheet_Request);
						BscanILL.Export.Printing.PrinterProfile printerProfile = dlg.Open();

						if (printerProfile != null)
						{
							_settings.Export.Printer.PrinterProfiles.Add(printerProfile);
							//AddPrinter(printerProfile);
						}
					}
				}
				else
				{
					if (_settings.Export.Printer.PrinterProfiles.Count > 0)
						_settings.Export.Printer.PrinterProfiles.Clear();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				if (_settings.Export.Printer.PrinterProfiles.Count == 0)
					this.PrintingEnabled = false;
				
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region PrinterPanel_DeleteRequest()
		void PrinterPanel_DeleteRequest(BscanILL.UI.Settings.Panels.Printer printerPanel)
		{
			if (_settings.Export.Printer.PrinterProfiles.Contains(printerPanel.PrinterProfile))
				_settings.Export.Printer.PrinterProfiles.Remove(printerPanel.PrinterProfile);

			if (scrollViewerStackPanel.Children.Contains(printerPanel))
			{
				printerPanel.DeleteRequest -= new BscanILL.UI.Settings.Panels.Printer.DeleteRequestHnd(PrinterPanel_DeleteRequest);
				scrollViewerStackPanel.Children.Remove(printerPanel);
			}
		}
		#endregion

		#region AddPrinter_Click()
		private void AddPrinter_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				BscanILL.UI.Settings.Dialogs.PrinterDlg dlg = new BscanILL.UI.Settings.Dialogs.PrinterDlg();
				dlg.PrintTestSheet += new BscanILL.UI.Settings.Dialogs.PrinterDlg.PrintTestSheetHnd(PrintTestSheet_Request); 				
				BscanILL.Export.Printing.PrinterProfile printerProfile = dlg.Open();

				if (printerProfile != null)
				{
					_settings.Export.Printer.PrinterProfiles.Add(printerProfile);
					//AddPrinter(printerProfile);
				}
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

		#region AddPrinter()
		private void AddPrinter(BscanILL.Export.Printing.PrinterProfile printerProfile)
		{
			BscanILL.UI.Settings.Panels.Printer printerPanel = new BscanILL.UI.Settings.Panels.Printer(printerProfile);
			printerPanel.DeleteRequest += new BscanILL.UI.Settings.Panels.Printer.DeleteRequestHnd(PrinterPanel_DeleteRequest);
			printerPanel.PrintTestSheet += new BscanILL.UI.Settings.Dialogs.PrinterDlg.PrintTestSheetHnd(PrintTestSheet_Request);
			printerPanel.Width = double.NaN;
			printerPanel.Margin = new Thickness(2, 2, 2, 8);
			scrollViewerStackPanel.Children.Add(printerPanel);
		}
		#endregion

		#region PrintTestSheet_Request()
		void PrintTestSheet_Request(BscanILL.Export.Printing.PrinterProfile printerProfile)
		{
			if (this.PrintTestSheet != null)
				this.PrintTestSheet(printerProfile);
		}
		#endregion

		#region PrinterProfiles_CollectionChanged()
		void PrinterProfiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			for (int i = scrollViewerStackPanel.Children.Count - 1; i >= 0; i--)
			{
				BscanILL.UI.Settings.Panels.Printer printerPanel = (BscanILL.UI.Settings.Panels.Printer)scrollViewerStackPanel.Children[i];

				if (_settings.Export.Printer.PrinterProfiles.Contains(printerPanel.PrinterProfile) == false)
					scrollViewerStackPanel.Children.Remove(printerPanel);
			}

			foreach (BscanILL.Export.Printing.PrinterProfile printerProfile in _settings.Export.Printer.PrinterProfiles)
			{
				BscanILL.UI.Settings.Panels.Printer printerPanel = BscanILL.UI.Settings.Panels.Printer.GetPrinterPanel(scrollViewerStackPanel.Children, printerProfile);

				if (printerPanel == null)
					AddPrinter(printerProfile);
			}
			
			this.PrintingEnabled = (_settings.Export.Printer.PrinterProfiles.Count > 0);
		}
		#endregion

		#endregion

	}
}
