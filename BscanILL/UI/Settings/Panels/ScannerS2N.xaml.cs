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
	/// Interaction logic for ScannerS2N.xaml
	/// </summary>
	public partial class ScannerS2N : PanelBase
	{
		List<BscanILL.UI.Settings.Panels.Scanner.ComboScannerTypeItem>	scannerTypes = BscanILL.UI.Settings.Panels.Scanner.AdfScannerTypes;
		List<BscanILL.UI.Settings.Panels.Scanner.ComboScanModeItem>		scanModes = BscanILL.UI.Settings.Panels.Scanner.AdfScanModes;

		public event EventHandler TestConnection;
		public event EventHandler TestTouchscreen;        
		public event EventHandler ScanAdfClick;
		public event EventHandler ScannerSettingsAdfClick;


		#region constructor
		public ScannerS2N()
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

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region ScannerIp
		public string ScannerIp
		{
			get { return _settings.Scanner.S2NScanner.Ip; }
			set
			{
				_settings.Scanner.S2NScanner.Ip = value;
				RaisePropertyChanged("ScannerIp");
			}
		}
		#endregion

		#region ScannerUsePedal
		public bool ScannerUsePedal
		{
			get { return _settings.Scanner.S2NScanner.FootPedal; }
			set
			{
				_settings.Scanner.S2NScanner.FootPedal = value;
				RaisePropertyChanged("ScannerUsePedal");
			}
		}
		#endregion

		#region Timeout
		public uint Timeout
		{
			get { return (uint)_settings.Scanner.General.SaveModeTimeoutInMins; }
			set
			{
				_settings.Scanner.General.SaveModeTimeoutInMins = (int)value;
				RaisePropertyChanged("Timeout");
			}
		}
		#endregion

		#region WakeOnLan
		public bool WakeOnLan
		{
			get { return _settings.Scanner.S2NScanner.WakeOnLAN; }
			set
			{
				_settings.Scanner.S2NScanner.WakeOnLAN = value;
				RaisePropertyChanged("WakeOnLan");
			}
		}
		#endregion

		#region WakeOnLanMacAddress
		public string WakeOnLanMacAddress
		{
			get { return _settings.Scanner.S2NScanner.WakeOnLanMacAddress; }
			set
			{
				_settings.Scanner.S2NScanner.WakeOnLanMacAddress = value;
				RaisePropertyChanged("WakeOnLanMacAddress");
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods
		
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region TestConnection_Click()
		private void TestConnection_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (TestConnection != null)
					TestConnection(this, null);
				else
					throw new Exception("TestConnection_Click event is not hooked up!");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion
		
        #region TestTouchScreen_Click()
        private void TestTouchScreen_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (TestTouchscreen != null)
                    TestTouchscreen(this, null);
				else
					throw new Exception("TestConnection_Click event is not hooked up!");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion		

		#endregion


	}
}
