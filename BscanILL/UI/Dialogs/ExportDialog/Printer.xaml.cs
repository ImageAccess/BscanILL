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
using System.Runtime.InteropServices;
using System.Reflection;


namespace BscanILL.UI.Dialogs.ExportDialog
{
	/// <summary>
	/// Interaction logic for Printer.xaml
	/// </summary>
	public partial class Printer : PanelBase, IPanel
	{
		List<BscanILL.Export.Printing.IIllPrinter>	printers = null;
		BscanILL.Export.Printing.IIllPrinter		printer = null;
		List<Export.Printing.IPaperSize>			sizes = new List<Export.Printing.IPaperSize>();
		Export.Printing.IPaperSize					selectedSize = null;
		List<Export.Printing.IInputBin>				trays = new List<Export.Printing.IInputBin>();
		Export.Printing.IInputBin					selectedTray = null;
		
		bool										includePullslip = true;

		public delegate void DeleteRequestHnd(Printer printerPanel);
		
		//public event BscanILL.UI.Settings.Dialogs.PrinterDlg.PrintTestSheetHnd	PrintTestSheet;


		#region constructor
		public Printer()
		{
			InitializeComponent();

            if (_settings.Export.Printer.Enabled)   //run code below just print export enable because command 'this.SelectedPrinter = ..' executes for XPS type very slowly and it slows down launching of Export Dialog
            {

                if (_settings.Export.Printer.PrintFunctionality == Export.Printing.Functionality.Win32)
                {
                    this.Printers = BscanILL.Export.Printing.Win32.IllPrinters.GetPrinters();
                    this.SelectedPrinter = BscanILL.Export.Printing.Win32.IllPrinters.GetDefaultPrinter();
                }
                else
                {
                    this.Printers = BscanILL.Export.Printing.Xps.IllPrinters.GetPrinters();
                    this.SelectedPrinter = BscanILL.Export.Printing.Xps.IllPrinters.GetDefaultPrinter();
                }
            }

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public bool							IsExportSetup { get { return this.SelectedPrinter != null; } }

		public BscanILL.Scan.FileFormat		FileFormat	{ get { return Scan.FileFormat.Png; } }
		public bool							MultiImage	{ get { return true; } }
		public bool							PdfA		{ get { return false; } }

		#region Printers
		public List<BscanILL.Export.Printing.IIllPrinter> Printers
		{
			get { return this.printers; }
			set
			{
				if(this.printers != value)
				{
					this.printers = value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#region SelectedPrinter
		public BscanILL.Export.Printing.IIllPrinter SelectedPrinter
		{
			get { return this.printer; }
			set
			{
				if (this.printer != value)
				{					
					foreach (BscanILL.Export.Printing.IIllPrinter p in this.Printers)
					{
						if (p.Name.ToLower() == value.Name.ToLower())
						{
							this.printer = p;
							this.Sizes = this.printer.PaperSizes;
							this.Trays = this.printer.InputBins;
							RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
							RaisePropertyChanged("set_Trays");
							RaisePropertyChanged("set_Sizes");

							this.SelectedTray = this.printer.InputBin;
							this.SelectedSize = this.printer.PaperSize;
						}
					}
				}
			}
		}
		#endregion

		#region Trays
		public List<Export.Printing.IInputBin> Trays
		{
			get { return this.trays; }
			set
			{
				this.trays = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SelectedTray
		public Export.Printing.IInputBin SelectedTray
		{
			get { return this.selectedTray; }
			set
			{
				if (this.SelectedPrinter != null && value != null)
				{
					foreach(Export.Printing.IInputBin bin in this.Trays)
						if (bin.DisplayName == value.DisplayName)
						{
							this.selectedTray = bin;

							if (this.SelectedPrinter != null)
								this.SelectedPrinter.InputBin = bin;

							RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
							break;
						}
				}
			}
		}
		#endregion

		#region Sizes
		public List<Export.Printing.IPaperSize> Sizes
		{
			get { return this.sizes; }
			set
			{
				this.sizes = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SelectedSize
		public Export.Printing.IPaperSize SelectedSize
		{
			get { return this.selectedSize; }
			set
			{
				if (this.SelectedPrinter != null && value != null)
				{
					foreach(Export.Printing.IPaperSize paperSize in this.Sizes)
						if (paperSize.DisplayName == value.DisplayName)
						{
							this.selectedSize = paperSize;

							if (this.SelectedPrinter != null)
								this.SelectedPrinter.PaperSize = paperSize;
							
							RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
							break;
						}
				}
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

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region GetAdditionalInfo()
		public BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo()
		{
			return new BscanILL.Export.AdditionalInfo.AdditionalPrinter(this.SelectedPrinter, this.IncludePullslip, false, false);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region PrinterProperties_Click()
		private void PrinterProperties_Click(object sender, RoutedEventArgs e)
		{
			IntPtr handle = new System.Windows.Interop.WindowInteropHelper(Window.GetWindow(this)).Handle;
			PrintDocument printDoc = new PrintDocument();
			PrinterSettings _settings = printDoc.PrinterSettings;

			OpenPrinterPropertiesDialog(_settings, handle);
		}
		#endregion

		#region OpenPrinterPropertiesDialog()
		[DllImport("kernel32.dll")]
		static extern IntPtr GlobalLock(IntPtr hMem);

		[DllImport("kernel32.dll")]
		static extern bool GlobalUnlock(IntPtr hMem);

		[DllImport("kernel32.dll")]
		static extern bool GlobalFree(IntPtr hMem);

		[DllImport("winspool.Drv", EntryPoint = "DocumentPropertiesW", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		static extern int DocumentProperties(IntPtr hwnd, IntPtr hPrinter, [MarshalAs(UnmanagedType.LPWStr)] string pDeviceName, IntPtr pDevModeOutput, IntPtr pDevModeInput, int fMode);

		private const Int32 DM_OUT_BUFFER = 14;
		
		public void OpenPrinterPropertiesDialog(PrinterSettings printerSettings, System.IntPtr pHandle)
		{
			IntPtr hDevMode = printerSettings.GetHdevmode();
			IntPtr pDevMode = GlobalLock(hDevMode);
			Int32 fMode = 0;
			int sizeNeeded = DocumentProperties(pHandle, IntPtr.Zero, printerSettings.PrinterName, IntPtr.Zero, pDevMode, fMode);
			IntPtr devModeData = Marshal.AllocHGlobal(sizeNeeded);

			fMode = DM_OUT_BUFFER;

			DocumentProperties(pHandle, IntPtr.Zero, printerSettings.PrinterName, devModeData, pDevMode, fMode);
			GlobalUnlock(hDevMode);
			printerSettings.SetHdevmode(devModeData);
			printerSettings.DefaultPageSettings.SetHdevmode(devModeData);
			GlobalFree(hDevMode);
			Marshal.FreeHGlobal(devModeData);
		}
		#endregion

		#region ComboPrinters_SelectionChanged()
		private void ComboPrinters_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
		#endregion

		#endregion

	}
}
