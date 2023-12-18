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
	/// Interaction logic for ClickControl.xaml
	/// </summary>
	public partial class ClickControl : UserControl, IScannerControl
	{
		public event BscanILL.Misc.VoidHnd MoreSettingsClick;


		#region constructor
		public ClickControl()
		{
			InitializeComponent();

			this.scanPageControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.Click;
			this.scanModeControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.Click;
			this.colorModeControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.Click;
			this.fileFormatControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.Click;
			this.dpiControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.Click;
			this.brightContrControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.Click;
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#region ScanPage
		public Scanners.Click.ClickScanPage ScanPage
		{
			get
			{
				return this.scanPageControl.Value;
			}
		}
		#endregion

		#region ScanMode
		public Scanners.Click.ClickScanMode ScanMode
		{
			get
			{
				return this.scanModeControl.Value;
			}
		}
		#endregion

		#region ColorMode
		public Scanners.ColorMode ColorMode
		{
			get
			{
				switch (this.colorModeControl.Value)
				{
					case Scanners.Click.ClickColorMode.Bitonal: return Scanners.ColorMode.Bitonal;
					case Scanners.Click.ClickColorMode.Grayscale: return Scanners.ColorMode.Grayscale;
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
					case Scanners.FileFormat.Tiff: return Scanners.FileFormat.Tiff;
					case Scanners.FileFormat.Png: return Scanners.FileFormat.Png;
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
