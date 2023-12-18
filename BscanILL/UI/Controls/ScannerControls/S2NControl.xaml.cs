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
	/// Interaction logic for S2NControl.xaml
	/// </summary>
	public partial class S2NControl : UserControl, IScannerControl
	{
		public event BscanILL.Misc.VoidHnd		MoreSettingsClick;
        public event BscanILL.Misc.VoidHnd      ScannerSettingsChanged;
        public event Scanners.S2N.ScanRequestHnd   ScannerSplittingSettingsChanged;        

		#region constructor
		public S2NControl()
		{
			InitializeComponent();

			this.colorModeControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
            this.pageSplittingControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.fileFormatControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.dpiControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.brightContrControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;

            this.colorModeControl.SettingsChanged += delegate()
            {
                if (ScannerSettingsChanged != null)
                    ScannerSettingsChanged();
            };

            this.pageSplittingControl.SettingsChanged += delegate()
            {
                if (ScannerSettingsChanged != null)
                    ScannerSettingsChanged();
            };

            this.fileFormatControl.SettingsChanged += delegate()
            {
                if (ScannerSettingsChanged != null)
                    ScannerSettingsChanged();
            };

            this.dpiControl.SettingsChanged += delegate()
            {
                if (ScannerSettingsChanged != null)
                    ScannerSettingsChanged();
            };

            this.brightContrControl.SettingsChanged += delegate()
            {
                if (ScannerSettingsChanged != null)
                    ScannerSettingsChanged();
            };

            this.pageSplittingControl.SplittingSettingsChanged += delegate( Scanners.S2N.ScannerScanAreaSelection scanArea )
            {
                if (ScannerSplittingSettingsChanged != null)
                    ScannerSplittingSettingsChanged(scanArea);
            };
            
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
					case Scanners.S2N.ColorMode.Lineart:
					case Scanners.S2N.ColorMode.Photo:
						return Scanners.ColorMode.Bitonal;
					case Scanners.S2N.ColorMode.Grayscale: return Scanners.ColorMode.Grayscale;
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
					case Scanners.S2N.FileFormat.Tiff: return Scanners.FileFormat.Tiff;
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
		/// <summary>
		/// value is in interval <-1,1>
		/// </summary>
		public double Brightness
		{
			get { return this.brightContrControl.Brightness; }
		}
		#endregion

		#region Contrast
		/// <summary>
		/// value is in interval <-1,1>
		/// </summary>
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
			/*try
			{
				if (sm.PrimaryScanner is Scanners.S2N.Bookeye4Wrapper || sm.PrimaryScanner is Scanners.S2N.ScannerS2NWrapper)
				{
					Window w = Window.GetWindow(this);

					if (w != null)
					{
						w.IsEnabled = false;
					}
					
					BscanILL.UI.Dialogs.Scanning.S2NAdditionalSettingsDlg dlg = new BscanILL.UI.Dialogs.Scanning.S2NAdditionalSettingsDlg();

					dlg.ShowDialog();

					if (w != null)
					{
						w.IsEnabled = true;
					}
				}
			}
			catch (Exception ex)
			{
				notifications.Notify(this, BscanILL.Misc.Notifications.Type.Error, "S2NControl, MoreSettings_Click(): " + ex.Message, ex);
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			} */
			
			if (MoreSettingsClick != null)
				MoreSettingsClick();

			e.Handled = true;
		}
		#endregion

		#endregion

	}
}
