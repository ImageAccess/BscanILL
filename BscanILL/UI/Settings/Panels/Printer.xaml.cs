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


namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for Printer.xaml
	/// </summary>
	public partial class Printer : UserControl
	{
		BscanILL.Export.Printing.PrinterProfile printerProfile;

		public delegate void DeleteRequestHnd(Printer printerPanel);
		
		public event DeleteRequestHnd											DeleteRequest;
		public event BscanILL.UI.Settings.Dialogs.PrinterDlg.PrintTestSheetHnd	PrintTestSheet;


		#region constructor
		private Printer()
		{
			InitializeComponent();
		}

		public Printer(BscanILL.Export.Printing.PrinterProfile printerProfile)
			: this()
		{
			this.printerProfile = printerProfile;
			this.DataContext = printerProfile;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Export.Printing.PrinterProfile PrinterProfile { get { return this.printerProfile; } set { this.printerProfile = value; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region GetPrinterPanel()
		public static BscanILL.UI.Settings.Panels.Printer GetPrinterPanel(UIElementCollection collection, BscanILL.Export.Printing.PrinterProfile printerProfile)
		{
			foreach (UIElement uiElement in collection)
				if ((uiElement is BscanILL.UI.Settings.Panels.Printer) && (((BscanILL.UI.Settings.Panels.Printer)uiElement).PrinterProfile == printerProfile))
					return (BscanILL.UI.Settings.Panels.Printer)uiElement;

			return null;
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Edit_Click()
		private void Edit_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				BscanILL.UI.Settings.Dialogs.PrinterDlg dlg = new BscanILL.UI.Settings.Dialogs.PrinterDlg();

				dlg.PrintTestSheet += delegate(BscanILL.Export.Printing.PrinterProfile printerProfile)
				{
					if (this.PrintTestSheet != null)
						this.PrintTestSheet(printerProfile);
				}; 

				if (dlg.Open(this.printerProfile))
				{
					this.textBlockName.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
					this.textBlockPrinter.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
					this.textBlockTray.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
					this.textBlockSize.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
					//this.textDuplex.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
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

		#region Delete_Click()
		private void Delete_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				if (DeleteRequest != null)
					DeleteRequest(this);
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

		#endregion

	}
}
