using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Printing;
using BscanILL.Misc;

namespace BscanILL.Export.Printing
{
	public class PrintOptions 
	{
		static PrintOptions			instance = new PrintOptions();
		List<PrintOption>			printers = new List<PrintOption>();
		BscanILL.SETTINGS.Settings	_settings = BscanILL.SETTINGS.Settings.Instance;


		#region constructor
		private PrintOptions()
		{
			if (_settings.Export.Printer.Enabled)
			{
				_settings.Export.Printer.PrinterProfiles.Validate();

				if (_settings.Export.Printer.Enabled)
				{
					System.Drawing.Printing.PrintDocument printDocument = new System.Drawing.Printing.PrintDocument();

					foreach (BscanILL.Export.Printing.PrinterProfile printerProfile in _settings.Export.Printer.PrinterProfiles)
					{
						try
						{
							PrintOption printOption = new PrintOption(printerProfile);
							this.printers.Add(printOption);
						}
						catch
						{
						}
					}
				}
			}
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties

		#region Instance
		public static BscanILL.Export.Printing.PrintOptions Instance
		{
			get
			{
				try
				{
					return PrintOptions.instance;
				}
				catch (Exception ex)
				{
					string message = "";
					Exception e = ex;

					while (e != null)
					{
						message += e.Message + Environment.NewLine;
						e = e.InnerException;
					}

					Notifications.Instance.Notify(null, Notifications.Type.Error, "Can't initialize printer!" + " " + message, ex);
					throw new IllException("Can't initialize printer!");
				}
			}
		}
		#endregion

		#endregion

		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			Reset();
		}
		#endregion

		#region OpenSimple()
		/*public static BscanILL.Export.Printing.PrintingOptions OpenSimple(BscanILL.Hierarchy.DlsgImages dlsgImages, System.Windows.Window parentForm)
		{
			BscanILL.Export.Printing.PrintingOptions printingOptions;

			using (BscanILL.Dialogs.PrintDlg dialog = new BscanILL.Dialogs.PrintDlg(dlsgImages))
			{
				printingOptions = dialog.Open(parentForm);
			}

			return printingOptions;
		}

		public static BscanILL.Export.Printing.PrintingOptions OpenSimpleArea(BscanILL.Hierarchy.DlsgImage dlsgImage, System.Windows.Window parentForm, IImageWindow imageWindow)
		{
			BscanILL.Dialogs.PrintImageAreaDlg dialog = new BscanILL.Dialogs.PrintImageAreaDlg(imageWindow);

			BscanILL.Export.Printing.PrintingOptions printingOptions = dialog.Open(parentForm, dlsgImage);
			return printingOptions;
		}*/
		#endregion

		#region OpenAdvanced()
		/*public static ExportDlg.WorkUnit OpenAdvanced(BscanILL.Hierarchy.Article article, System.Windows.Window parentForm)
		{
			List<BscanILL.Hierarchy.IImage> iIllImages = new List<BscanILL.Hierarchy.IImage>();
			BscanILL.Export.Printing.PrintingOptions printingOptions;

			foreach (BscanILL.Hierarchy.IImage dlsgImage in article.Images)
				iIllImages.Add(dlsgImage);

			using (BscanILL.Dialogs.PrintDlg dialog = new BscanILL.Dialogs.PrintDlg(article.Images))
			{
				printingOptions = dialog.Open(parentForm);
			}

			if (printingOptions != null)
			{
				ExportDlg.WorkUnit exportUnit = new ExportDlg.WorkUnit(article.Images, 
					new BscanILL.ExportSettings(BscanILL.Scan.ExportMedium.Print, BscanILL.Scan.FileFormat.Auto, BscanILL.ImagesSelection.All, "Printed Images", false, null, Settings.Instance.Export.IsPdfaDefault));

				exportUnit.Tag = printingOptions;

				return exportUnit;
			}

			return null;
		}*/
		#endregion

		#region GetPrinters()
		public static System.Printing.PrintQueueCollection GetPrinters()
		{
			System.Printing.PrintServer printServer = new System.Printing.PrintServer();
			System.Printing.EnumeratedPrintQueueTypes[] printTypes = new System.Printing.EnumeratedPrintQueueTypes[] { System.Printing.EnumeratedPrintQueueTypes.Local, System.Printing.EnumeratedPrintQueueTypes.Connections };
			System.Printing.PrintQueueCollection printQueues = printServer.GetPrintQueues(printTypes);

			return printQueues;
		}
		#endregion

		#region GetDefaultPrinter()
		public static System.Printing.PrintQueue GetDefaultPrinter()
		{
			return LocalPrintServer.GetDefaultPrintQueue();
		}
		#endregion

		#region GetInputBins()
		public static System.Collections.ObjectModel.ReadOnlyCollection<System.Printing.InputBin> GetInputBins(System.Printing.PrintQueue printer)
		{
			return printer.GetPrintCapabilities().InputBinCapability;
		}
		#endregion

		#region GetPaperSizeCollection()
		public static System.Collections.ObjectModel.ReadOnlyCollection<System.Printing.PageMediaSize> GetPaperSizeCollection(System.Printing.PrintQueue printer)
		{
			return printer.GetPrintCapabilities().PageMediaSizeCapability;
		}
		#endregion

		#region GetPrintOption()
		public PrintOption GetPrintOption(BscanILL.Export.Printing.PrinterProfile printerProfile)
		{
			foreach (PrintOption printOption in this.printers)
			{
				if (printOption.PrinterProfile == printerProfile)
				{
					return printOption;
				}
			}

			return null;
		}
		#endregion

		#region Reset()
		public void Reset()
		{
		}
		#endregion
	
		#endregion

	}
}
