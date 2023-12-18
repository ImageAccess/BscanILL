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

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for ScannerTwain.xaml
	/// </summary>
	public partial class ScannerTwain : PanelBase
	{
		public event EventHandler ScanClick;
		public event EventHandler ScannerSettingsClick;
		public event EventHandler ScanAdfClick;
		public event EventHandler ScannerSettingsAdfClick;


		#region constructor
		public ScannerTwain()
		{
			InitializeComponent();

			this.scannerAddOn.ScanClick += delegate(object sender, EventArgs e)
			{
				if (ScanAdfClick != null)
					ScanAdfClick(this, null);
				else
					throw new Exception("TestScan_Click event is not hooked up!");
			};

			this.scannerAddOn.OpenSettingsClick += delegate(object sender, EventArgs e)
			{
				if (ScannerSettingsAdfClick != null)
					ScannerSettingsAdfClick(this, null);
				else
					throw new Exception("Settings_Click event is not hooked up!");
			};

			if (_settings != null)
				_settings.Scanner.PropertyChanged += new PropertyChangedEventHandler(Scanner_PropertyChanged);
			
			this.DataContext = this;
		}
		#endregion



		//PUBLIC PROPERTIES
		#region public properties

		#region EnergySavings
		public bool EnergySavings
		{
			get
			{
				return _settings.Scanner.TwainScanner.IsEnergyStarOn;
			}
			set
			{
				_settings.Scanner.TwainScanner.IsEnergyStarOn = value;
				RaisePropertyChanged("EnergySavings");
			}
		}
		#endregion

		#region EnergySavingsTimeout
		public int EnergySavingsTimeout
		{
			get
			{
				return _settings.Scanner.TwainScanner.EnergyStarTimeout;
			}
			set
			{
				_settings.Scanner.TwainScanner.EnergyStarTimeout = value;
				RaisePropertyChanged("EnergySavingsTimeout");
			}
		}
		#endregion

		#region IsAdjustingVisible
		public bool IsAdjustingVisible
		{
			get
			{
/*
				Scanners.SettingsScannerType scannerType = _settings.Scanner.General.ScannerType;

				return ((scannerType == Scanners.SettingsScannerType.iVinaFB6080E) ||
					(scannerType == Scanners.SettingsScannerType.iVinaFB6280E) ||
					(scannerType == Scanners.SettingsScannerType.KodakI1405) ||
					(scannerType == Scanners.SettingsScannerType.KodakI1120) ||
					(scannerType == Scanners.SettingsScannerType.KodakI1150)
					);
*/
                return false;       //do not show the brightness delta and contrast delta controls at any time for Bookedge/Kodak Adf - not sure what was it for
			}
			set
			{
				//_settings.Scanner.TwainScanner.IsEnergyStarOn = value;
				//RaisePropertyChanged("IsAdjustingVisible");
			}
		}
		#endregion

		#region BrightnessDelta
		public int BrightnessDelta
		{
			get { return Convert.ToInt32(_settings.Scanner.TwainScanner.BrightnessDelta * 100.0); }
			set
			{
				_settings.Scanner.TwainScanner.BrightnessDelta = value / 100.0 ;
				RaisePropertyChanged("BrightnessDelta");
			}
		}
		#endregion

		#region ContrastDelta
		public int ContrastDelta
		{
			get { return Convert.ToInt32(_settings.Scanner.TwainScanner.ContrastDelta * 100.0); }
			set
			{
				_settings.Scanner.TwainScanner.ContrastDelta = value / 100.0;
				RaisePropertyChanged("ContrastDelta");
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region TestScan_Click()
		private void TestScan_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (ScanClick != null)
					ScanClick(this, null);
				else
					throw new Exception("TestScan_Click event is not hooked up!");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Settings_Click()
		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (ScannerSettingsClick != null)
					ScannerSettingsClick(this, null);
				else
					throw new Exception("Settings_Click event is not hooked up!");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region ResetBrighContr_Click()
		private void ResetBrighContr_Click(object sender, RoutedEventArgs e)
		{
			BrightnessDelta = 0;
			ContrastDelta = 0;
		}
		#endregion

		#region Scanner_PropertyChanged()
		void Scanner_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ScannerType")
				this.gridAdjusting.GetBindingExpression(VisibilityProperty).UpdateTarget();
		}
		#endregion

		#endregion

	}
}
