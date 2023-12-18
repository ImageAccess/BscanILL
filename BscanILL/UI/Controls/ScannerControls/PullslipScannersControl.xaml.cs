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
	/// Interaction logic for PullslipScannersControl.xaml
	/// </summary>
	public partial class PullslipScannersControl : UserControl
	{

		#region constructor
		public PullslipScannersControl()
		{
			InitializeComponent();
		}
		#endregion


		#region Control_Loaded()
		private void Control_Loaded(object sender, RoutedEventArgs e)
		{
			sm.PrimaryScannerChanged += new Scan.ScannersManager.ScannersChangedHnd(ScannersManager_Changed);
			sm.SecondaryScannerChanged += new Scan.ScannersManager.ScannersChangedHnd(ScannersManager_Changed);

			Update();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public bool PrimaryScannerSelected
		{
			get { return radioPrimary.IsChecked.Value; }
			set
			{
				if (value)
					radioPrimary.IsChecked = true;
				else
					radioSecondary.IsChecked = true;
			}
		}

		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		BscanILL.Scan.ScannersManager sm { get { return BscanILL.Scan.ScannersManager.Instance; } }

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Update()
		private void Update()
		{            
			if (sm.MultiScannerMode)                        
			{
				imagePrimary.Source = new BitmapImage(new Uri("pack://application:,,,/BscanILL;component/Images/Scanner/Height60/Bookeye4.png", UriKind.Absolute));
				
				if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
					imagePrimary.Source = new BitmapImage(new Uri("pack://application:,,,/BscanILL;component/Images/Scanner/Height60/Bookeye4.png", UriKind.Absolute));
				else if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
					imagePrimary.Source = new BitmapImage(new Uri("pack://application:,,,/BscanILL;component/Images/Scanner/Height60/FB6280E.png", UriKind.Absolute));
				else if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
					imagePrimary.Source = new BitmapImage(new Uri("pack://application:,,,/BscanILL;component/Images/Scanner/Height60/KodakI1120.png", UriKind.Absolute));
                else if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click)
                    imagePrimary.Source = new BitmapImage(new Uri("pack://application:,,,/BscanILL;component/Images/Scanner/Height60/Click.png", UriKind.Absolute));
                else if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini)
                    imagePrimary.Source = new BitmapImage(new Uri("pack://application:,,,/BscanILL;component/Images/Scanner/Height60/ClickMini2.png", UriKind.Absolute));

				if (sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
					imageSecondary.Source = new BitmapImage(new Uri("pack://application:,,,/BscanILL;component/Images/Scanner/Height60/Bookeye4.png", UriKind.Absolute));
				else if (sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
					imageSecondary.Source = new BitmapImage(new Uri("pack://application:,,,/BscanILL;component/Images/Scanner/Height60/FB6280E.png", UriKind.Absolute));
				else if (sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
					imageSecondary.Source = new BitmapImage(new Uri("pack://application:,,,/BscanILL;component/Images/Scanner/Height60/KodakI1120.png", UriKind.Absolute));

				mainGrid.Visibility = System.Windows.Visibility.Visible;
                radioPrimary.Visibility = System.Windows.Visibility.Visible;
                radioSecondary.Visibility = System.Windows.Visibility.Visible;

                HideBrightnessContrastControls();
                if (this.PrimaryScannerSelected == true)
                {
                    ShowBrightContrControlsByPrimaryScanner();
                }
                else
                {
                    ShowBrightContrControlsBySecondaryScanner();
                }
			}
			else
			{
				this.PrimaryScannerSelected = true;
				//this.mainGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.mainGrid.Visibility = System.Windows.Visibility.Visible;
                radioPrimary.Visibility = System.Windows.Visibility.Collapsed;
                radioSecondary.Visibility = System.Windows.Visibility.Collapsed;

                HideBrightnessContrastControls();
                ShowBrightContrControlsByPrimaryScanner(); 
			}
		}
		#endregion

        #region HideBrightnessContrastControls()
        private void HideBrightnessContrastControls()
        {
            if (s2nControlPullSlip != null)            
                s2nControlPullSlip.Visibility = System.Windows.Visibility.Collapsed;

            if (clickMiniControlPullSlip != null)            
                clickMiniControlPullSlip.Visibility = System.Windows.Visibility.Collapsed;

            if (clickControlPullSlip != null)            
               clickControlPullSlip.Visibility = System.Windows.Visibility.Collapsed;

            if (bookedgeControlPullSlip != null)            
               bookedgeControlPullSlip.Visibility = System.Windows.Visibility.Collapsed;

            if (adfControlPullSlip != null)            
               adfControlPullSlip.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion

        #region ShowBrightContrControlsByPrimaryScanner()
        private void ShowBrightContrControlsByPrimaryScanner()
        {
            if (sm.PrimaryScanner != null)
            {
                if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
                {
                    if (s2nControlPullSlip != null)            
                       s2nControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
                else if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini)
                {
                    if (clickMiniControlPullSlip != null)  
                      clickMiniControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
                else if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click)
                {
                    if (clickControlPullSlip != null)            
                      clickControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
                else if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
                {
                    if (bookedgeControlPullSlip != null) 
                      bookedgeControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
                else if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
                {
                    if (adfControlPullSlip != null) 
                    adfControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
        #endregion

        #region ShowBrightContrControlsBySecondaryScanner()
        private void ShowBrightContrControlsBySecondaryScanner()
        {
            if (sm.SecondaryScanner != null)
            {
                if (sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
                {
                    if (s2nControlPullSlip != null)            
                      s2nControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
                else if (sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini)
                {
                    if (clickMiniControlPullSlip != null)  
                      clickMiniControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
                else if (sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click)
                {
                    if (clickControlPullSlip != null)            
                      clickControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
                else if (sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
                {
                    if (bookedgeControlPullSlip != null) 
                       bookedgeControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
                else if (sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
                {
                    if (adfControlPullSlip != null) 
                      adfControlPullSlip.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
        #endregion

        #region ScannerRadio_Checked()
        private void ScannerRadio_Checked(object sender, RoutedEventArgs e)
        {
            HideBrightnessContrastControls();

            if ((e.OriginalSource as RadioButton).Name == "radioPrimary")
            {
                ShowBrightContrControlsByPrimaryScanner();
            }
            else
                if ((e.OriginalSource as RadioButton).Name == "radioSecondary")
                {
                    ShowBrightContrControlsBySecondaryScanner(); 
                }            
        }
        #endregion

		#region ScannersManager_Changed()
		void ScannersManager_Changed()
		{
			if (this.Dispatcher.CheckAccess())
				Update();
			else
				this.Dispatcher.BeginInvoke((Action)delegate() { Update(); });
		}
		#endregion

		#endregion
	}
}
