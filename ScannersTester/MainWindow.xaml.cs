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
using Scanners.S2N;
using System.IO;
using BscanILL.Misc;

namespace ScannersTester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, Scanners.PingDeviceReceiver
	{
		Scanners.IScanner scanner = null;

		Scanners.Settings settings = Scanners.Settings.Instance;

		#region constructor
		public MainWindow()
		{
			InitializeComponent();

			settings.General.ScannerType = Scanners.SettingsScannerType.S2N;
		}
		#endregion

		#region Scanner
		public Scanners.IScanner Scanner
		{
			get { return this.scanner; }
			set
			{
				this.scanner = value;
			}
		}
		#endregion

		#region LoadScanner_Click()
		private void LoadScanner_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Scanners.Settings.S2NScannerClass settings = Scanners.Settings.Instance.S2NScanner;

				settings.Ip = textIp.Text;
				settings.FootPedal = true;
				settings.WakeOnLAN = false;

				LoadScanner();

				MessageBox.Show("Connected.", "", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				string error = ex.Message;

				while ((ex = ex.InnerException) != null)
					error += Environment.NewLine + ex.Message;

				MessageBox.Show(error, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region PingDeviceProgressChanged()
		public void PingDeviceProgressChanged(string description)
		{
			Console.WriteLine(description);
		}
		#endregion

		#region LoadScanner()
		private void LoadScanner()
		{
			Scanners.IScanner scannerTemp = null;

			Scanners.Scanner.PingDevice(this, this.settings.S2NScanner.Ip, true);
			Scanners.DeviceInfo s2nInfo = Scanners.S2N.ScannerS2N.GetDeviceInfo(this.settings.S2NScanner.Ip);
			
            //if( (s2nInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE4) || (s2nInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE5) )
            if (s2nInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BE4)
			{
				scannerTemp = new Scanners.S2N.Bookeye4Wrapper(((Scanners.DeviceInfoS2N)s2nInfo).Ip, false);
				((Scanners.S2N.Bookeye4Wrapper)scannerTemp).UnlockScannerUi(false);
				((Scanners.S2N.Bookeye4Wrapper)scannerTemp).StartTouchScreenMonitoring();
				((Scanners.S2N.Bookeye4Wrapper)scannerTemp).ScanRequest += new Scanners.S2N.ScanRequestHnd(MainWindow_ScanRequest);
			}
			else
				scannerTemp = new Scanners.S2N.ScannerS2NWrapper(((Scanners.DeviceInfoS2N)s2nInfo).Ip);

			//assigning valid scanner
			if (this.Scanner != null && this.Scanner != scannerTemp)
			{
				if (this.Scanner is Scanners.Twain.TwainScanner)
					((Scanners.Twain.TwainScanner)this.Scanner).Dispose(this);
				else if (this.Scanner is Scanners.Twain.AdfScanner)
					((Scanners.Twain.AdfScanner)this.Scanner).Dispose(this);
				else
					this.Scanner.Dispose();
			}

			this.Scanner = scannerTemp;
		}
		#endregion

		#region MainWindow_ScanRequest()
		void MainWindow_ScanRequest(Scanners.S2N.ScannerScanAreaSelection scanArea)
		{
			((Scanners.S2N.Bookeye4Wrapper)scanner).LockScannerUi( false );
			System.Threading.Thread.Sleep(1000);
			((Scanners.S2N.Bookeye4Wrapper)scanner).UnlockScannerUi(false);
			MessageBox.Show("Scan Request. Scan Area: " + scanArea.ToString());
		}
		#endregion

		#region Window_Closing()
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.Scanner != null)
				this.Scanner.Dispose();
		}
		#endregion

		#region LoadClickScanner_Click()
		private void LoadClickScanner_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Scanners.Settings.ClickScannerClass settings = new Scanners.Settings.ClickScannerClass();

				settings.ComPort = "COM4";

				using (Scanners.Click.ClickWrapper clickWrapper = new Scanners.Click.ClickWrapper())
				{
					Console.WriteLine("Click serial number: " + clickWrapper.DeviceInfo.SerialNumber);
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

	}
}
