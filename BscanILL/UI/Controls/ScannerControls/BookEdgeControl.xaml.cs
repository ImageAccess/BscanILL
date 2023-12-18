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

namespace BscanILL.UI.Controls.ScannerControls
{
	/// <summary>
	/// Interaction logic for BookEdgeControl.xaml
	/// </summary>
	public partial class BookEdgeControl : UserControl, IScannerControl
	{
		public event BscanILL.Misc.VoidHnd MoreSettingsClick;


		#region constructor
		public BookEdgeControl()
		{
			InitializeComponent();

			this.bookEdgeControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.BookEdge;
			this.colorModeControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.BookEdge;
			this.fileFormatControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.BookEdge;
			this.dpiControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.BookEdge;
			this.brightContrControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.BookEdge;
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#region ColorMode
		public Scanners.ColorMode ColorMode
		{
			get
			{
				switch (this.colorModeControl.Value)
				{
					case Scanners.Twain.ColorMode.Bitonal: return Scanners.ColorMode.Bitonal;
					case Scanners.Twain.ColorMode.Grayscale: return Scanners.ColorMode.Grayscale;
					default: return Scanners.ColorMode.Color;
				}
			}
		}
		#endregion

		#region FileFormat
		public Scanners.FileFormat FileFormat
		{
			get
			{
				switch (this.fileFormatControl.Value)
				{
					case Scanners.Twain.FileFormat.Tiff: return Scanners.FileFormat.Tiff;
					case Scanners.Twain.FileFormat.Png: return Scanners.FileFormat.Png;
					default: return Scanners.FileFormat.Jpeg;
				}
			}
		}
		#endregion

		#region Dpi
		public ushort Dpi
		{
			get { return this.dpiControl.Value; }
		}
		#endregion

		#region Brightness
		public double Brightness
		{
			get { return this.brightContrControl.Brightness; }
		}
		#endregion

		#region Contrast
		public double Contrast
		{
			get { return this.brightContrControl.Contrast; }
		}
		#endregion

		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		BscanILL.Misc.Notifications		notifications { get { return BscanILL.Misc.Notifications.Instance; } }
		BscanILL.Scan.ScannersManager	sm { get { return BscanILL.Scan.ScannersManager.Instance; } }

		#endregion

	
		// PRIVATE METHODS
		#region private methods

		#region MoreSettings_Click()
		private void MoreSettings_Click(object sender, MouseButtonEventArgs e)
		{
			if (MoreSettingsClick != null)
				MoreSettingsClick();

			e.Handled = true;
		}
		#endregion

		#endregion
	
	}
}
