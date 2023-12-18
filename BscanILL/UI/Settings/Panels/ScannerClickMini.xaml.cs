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
	/// Interaction logic for ScannerClickMini.xaml
	/// </summary>
	public partial class ScannerClickMini : PanelBase
	{		
		public event EventHandler					OpenSettingsClick;
        public event EventHandler                   ScanAdfClick;
        public event EventHandler                   ScannerSettingsAdfClick;

		#region constructor
		public ScannerClickMini()
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

			List<string> ports = new List<string>(System.IO.Ports.SerialPort.GetPortNames());
			ports.Sort();

			foreach (string serialPort in ports)
			{
				comboSerialPort.Items.Add(serialPort);

				if (serialPort.ToLower() == _settings.Scanner.ClickMini.ComPort.ToLower())
					comboSerialPort.SelectedItem = serialPort;
			}

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region ComPort
		public string ComPort
		{
			get { return _settings.Scanner.ClickMini.ComPort; }
			set
			{
				_settings.Scanner.ClickMini.ComPort = value;
				RaisePropertyChanged("ComPort");
			}
		}
		#endregion

		#endregion

		
		//PRIVATE METHODS
		#region private methods

		#region OpenSettings_Click()
		private void OpenSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (comboSerialPort.SelectedItem != null && comboSerialPort.SelectedItem is string)
					_settings.Scanner.ClickMini.ComPort = (string)comboSerialPort.SelectedItem;

				if (OpenSettingsClick != null)
					OpenSettingsClick(this, null);
				else
					throw new Exception("OpenSettingsClick event is not hooked up!");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Form_IsVisibleChanged()
		private void Form_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
		}
		#endregion

		#endregion


	}
}
