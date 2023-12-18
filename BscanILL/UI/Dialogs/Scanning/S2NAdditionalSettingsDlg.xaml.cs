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

namespace BscanILL.UI.Dialogs.Scanning
{
	/// <summary>
	/// Interaction logic for S2NAdditionalSettingsDlg.xaml
	/// </summary>
	public partial class S2NAdditionalSettingsDlg : Window
	{
		Scanners.S2N.S2NSettings bookeye4Settings;
        
		#region constructor
		public S2NAdditionalSettingsDlg()
		{
			InitializeComponent();

			bookeye4Settings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;

            this.docModeControl.DocModeSettingsChanged += delegate(Scanners.S2N.DocumentMode docMode)
            {
                S2NAdditionalSettingDocModeSettingsChanged(docMode);
            };

            this.docSizeControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.docModeControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.dpiModeControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.gammaControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;			
			this.rotationControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.jpegQualityControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.tiffCompressionControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.despeckleControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.sharpenControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
			this.bitonalThresholdControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;
            this.pageSplittingFirstPageControl.ScanSettings = BscanILL.SETTINGS.ScanSettings.Instance.S2N;

		}
		#endregion


		// PRIVATE METHODS
		#region private methods

        private void S2NAdditionalSettingDocModeSettingsChanged(Scanners.S2N.DocumentMode docMode)
        {
            this.docSizeControl.AdjustDocSizeOptionOnDocModeChange(docMode);
        }

		#region Ok_Click()
		/*private void Ok_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (this.bookeye4Settings.DpiMode.IsDefined)
					this.bookeye4Settings.DpiMode.Value = this.dpiModeControl.Value;
				if (this.bookeye4Settings.Gamma.IsDefined)
					this.bookeye4Settings.Gamma.Value = this.gammaControl.Value;
				if (this.bookeye4Settings.DocSize.IsDefined)
					this.bookeye4Settings.DocSize.Value = this.docSizeControl.Value;
				if (this.bookeye4Settings.Rotation.IsDefined)
					this.bookeye4Settings.Rotation.Value = this.rotationControl.Value;
				if (this.bookeye4Settings.JpegQuality.IsDefined)
					this.bookeye4Settings.JpegQuality.Value = this.jpegQualityControl.Value;
				if (this.bookeye4Settings.TiffCompression.IsDefined)
					this.bookeye4Settings.TiffCompression.Value = this.tiffCompressionControl.Value;
				if (this.bookeye4Settings.Despeckle.IsDefined)
					this.bookeye4Settings.Despeckle.Value = this.despeckleControl.Value;
				if (this.bookeye4Settings.Sharpening.IsDefined)
					this.bookeye4Settings.Sharpening.Value = this.sharpenControl.Value;
				if (this.bookeye4Settings.BitonalThreshold.IsDefined)
					this.bookeye4Settings.BitonalThreshold.Value = this.bitonalThresholdControl.Value;

				this.DialogResult = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}*/
		#endregion

		#region Form_Loaded()
		private void Form_Loaded(object sender, RoutedEventArgs e)
		{
			double height = 0;

			if (this.bookeye4Settings.DocMode.IsDefined)
				height += this.docModeControl.Height;
			else
				this.docModeControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.DpiMode.IsDefined)
				height += this.dpiModeControl.Height;
			else
				this.dpiModeControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.Gamma.IsDefined)
				height += this.gammaControl.Height;
			else
				this.gammaControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.DocSize.IsDefined)
				height += this.docSizeControl.Height;
			else
				this.docSizeControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.Rotation.IsDefined)
				height += this.rotationControl.Height;
			else
				this.rotationControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.JpegQuality.IsDefined)
				height += this.jpegQualityControl.Height;
			else
				this.jpegQualityControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.TiffCompression.IsDefined)
				height += this.tiffCompressionControl.Height;
			else
				this.tiffCompressionControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.Despeckle.IsDefined)
				height += this.despeckleControl.Height;
			else
				this.despeckleControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.Sharpening.IsDefined)
				height += this.sharpenControl.Height;
			else
				this.sharpenControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.BitonalThreshold.IsDefined)
				height += this.bitonalThresholdControl.Height;
			else
				this.bitonalThresholdControl.Visibility = System.Windows.Visibility.Collapsed;

			if (this.bookeye4Settings.Splitting_StartPage.IsDefined)
				height += this.pageSplittingFirstPageControl.Height;
			else
				this.pageSplittingFirstPageControl.Visibility = System.Windows.Visibility.Collapsed;            

			this.Height = height + 88;
		}
		#endregion

		#endregion

	}
}
