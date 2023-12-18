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
	/// Interaction logic for PanelScanner.xaml
	/// </summary>
	public partial class Scanner : PanelBase
	{
		List<ComboColorModeItem>	colorModes = new List<ComboColorModeItem>();
		List<ComboScanModeItem>		scanModes = new List<ComboScanModeItem>();
		List<ComboScannerTypeItem>	scannerTypes = new List<ComboScannerTypeItem>();

		public event EventHandler S2N_TestConnection;
		public event EventHandler S2N_TestTouchscreen;
		public event EventHandler Twain_ScanClick;
		public event EventHandler Twain_SettingsClick;
		public event EventHandler Rebel_OpenSettingsClick;
		public event EventHandler ClickMini_OpenSettingsClick;
		public event EventHandler AddOnScanner_ScanClick;
		public event EventHandler AddOnScanner_SettingsClick;
		public event EventHandler CheckLicenseFile_Request;
		public event EventHandler DownloadLicenseFile_Request;

	
		#region constructor
		public Scanner()
		{
			InitializeComponent();

			colorModes.Add(new ComboColorModeItem("Color", Scanners.ColorMode.Color));
			colorModes.Add(new ComboColorModeItem("Grayscale", Scanners.ColorMode.Grayscale));
			colorModes.Add(new ComboColorModeItem("Black White", Scanners.ColorMode.Bitonal));

			scanModes.Add(new ComboScanModeItem("Scan and split image into 2 pages",  Scanners.ScanMode.AutoSplitImage));
			scanModes.Add(new ComboScanModeItem("Scan single image",  Scanners.ScanMode.SingleScan));

			scannerTypes.Add(new ComboScannerTypeItem("IP Scanner (Bookeye, WideTEK, ...)", Scanners.SettingsScannerType.S2N));
			scannerTypes.Add(new ComboScannerTypeItem("Click Scanner", Scanners.SettingsScannerType.Click));
			scannerTypes.Add(new ComboScannerTypeItem("Click Mini", Scanners.SettingsScannerType.ClickMini));
			scannerTypes.Add(new ComboScannerTypeItem("BookEdge FB6280E", Scanners.SettingsScannerType.iVinaFB6280E));
			scannerTypes.Add(new ComboScannerTypeItem("BookEdge FB6080E", Scanners.SettingsScannerType.iVinaFB6080E));
			scannerTypes.Add(new ComboScannerTypeItem("Kodak i1405", Scanners.SettingsScannerType.KodakI1405));
			scannerTypes.Add(new ComboScannerTypeItem("Kodak i1120", Scanners.SettingsScannerType.KodakI1120));
			scannerTypes.Add(new ComboScannerTypeItem("Kodak i1150/i1180", Scanners.SettingsScannerType.KodakI1150));
			scannerTypes.Add(new ComboScannerTypeItem("Kodak i1150/i1180 (New)", Scanners.SettingsScannerType.KodakI1150New));
			scannerTypes.Add(new ComboScannerTypeItem("Kodak e1035", Scanners.SettingsScannerType.KodakE1035));
			scannerTypes.Add(new ComboScannerTypeItem("Kodak e1040", Scanners.SettingsScannerType.KodakE1040));

			this.panelS2N.ScanAdfClick += delegate(object sender, EventArgs e)
			{
				if (AddOnScanner_ScanClick != null)
					AddOnScanner_ScanClick(this, null);
				else
					throw new Exception("AddOnScanner_ScanClick event is not hooked up!");
			};

			this.panelS2N.ScannerSettingsAdfClick += delegate(object sender, EventArgs e)
			{
				if (AddOnScanner_SettingsClick != null)
					AddOnScanner_SettingsClick(this, null);
				else
					throw new Exception("AddOnScanner_SettingsClick event is not hooked up!");
			};

			this.panelTwain.ScanClick += delegate(object sender, EventArgs e)
			{
				if (Twain_ScanClick != null)
					Twain_ScanClick(this, null);
				else
					throw new Exception("Twain_ScanClick event is not hooked up!");
			};

			this.panelTwain.ScannerSettingsClick += delegate(object sender, EventArgs e)
			{
				if (Twain_SettingsClick != null)
					Twain_SettingsClick(this, null);
				else
					throw new Exception("Twain_ScannerSettingsClick event is not hooked up!");
			};

			this.panelTwain.ScanAdfClick += delegate(object sender, EventArgs e)
			{
				if (AddOnScanner_ScanClick != null)
					AddOnScanner_ScanClick(this, null);
				else
					throw new Exception("AddOnScanner_ScanClick event is not hooked up!");
			};

			this.panelTwain.ScannerSettingsAdfClick += delegate(object sender, EventArgs e)
			{
				if (AddOnScanner_SettingsClick != null)
					AddOnScanner_SettingsClick(this, null);
				else
					throw new Exception("AddOnScanner_SettingsClick event is not hooked up!");
			};

			this.panelRebel.OpenSettingsClick += delegate(object sender, EventArgs e)
			{
				if (Rebel_OpenSettingsClick != null)
					Rebel_OpenSettingsClick(this, null);
				else
					throw new Exception("Rebel_OpenSettingsClick event is not hooked up!");
			};

            this.panelRebel.ScanAdfClick += delegate(object sender, EventArgs e)
            {
                if (AddOnScanner_ScanClick != null)
                    AddOnScanner_ScanClick(this, null);
                else
                    throw new Exception("AddOnScanner_ScanClick event is not hooked up!");
            };

            this.panelRebel.ScannerSettingsAdfClick += delegate(object sender, EventArgs e)
            {
                if (AddOnScanner_SettingsClick != null)
                    AddOnScanner_SettingsClick(this, null);
                else
                    throw new Exception("AddOnScanner_SettingsClick event is not hooked up!");
            };

			this.panelClickMini.OpenSettingsClick += delegate(object sender, EventArgs e)
			{
				if (ClickMini_OpenSettingsClick != null)
					ClickMini_OpenSettingsClick(this, null);
				else
					throw new Exception("ClickMini_OpenSettingsClick event is not hooked up!");
			};

            this.panelClickMini.ScanAdfClick += delegate(object sender, EventArgs e)
            {
                if (AddOnScanner_ScanClick != null)
                    AddOnScanner_ScanClick(this, null);
                else
                    throw new Exception("AddOnScanner_ScanClick event is not hooked up!");
            };

            this.panelClickMini.ScannerSettingsAdfClick += delegate(object sender, EventArgs e)
            {
                if (AddOnScanner_SettingsClick != null)
                    AddOnScanner_SettingsClick(this, null);
                else
                    throw new Exception("AddOnScanner_SettingsClick event is not hooked up!");
            };

			this.panelS2N.TestConnection += delegate(object sender, EventArgs e)
			{
				if (S2N_TestConnection != null)
					S2N_TestConnection(this, null);
				else
					throw new Exception("S2N_TestConnection event is not hooked up!");
			};

      this.panelS2N.TestTouchscreen += delegate(object sender, EventArgs e)
			{
                if (S2N_TestTouchscreen != null)
                    S2N_TestTouchscreen(this, null);
				else
                    throw new Exception("S2N_TestTouchscreen event is not hooked up!");
			};
			
			this.DataContext = this;
		}
		#endregion


		#region class ComboColorModeItem
		public class ComboColorModeItem
		{
			public string				Caption		{ get; set; }
			public Scanners.ColorMode		ColorMode	{ get; set; }

			public ComboColorModeItem(string caption, Scanners.ColorMode colorMode)
			{
				this.Caption = caption;
				this.ColorMode = colorMode;
			}
		}
		#endregion

		#region class ComboScanModeItem
		public class ComboScanModeItem
		{
			public string				Caption		{ get; set; }
			public  Scanners.ScanMode		ScanMode { get; set; }

			public ComboScanModeItem(string caption,  Scanners.ScanMode scanMode)
			{
				this.Caption = caption;
				this.ScanMode = scanMode;
			}
		}
		#endregion

		#region class ComboScannerTypeItem
		public class ComboScannerTypeItem
		{
			public string Caption { get; set; }
			public Scanners.SettingsScannerType ScannerType { get; set; }

			public ComboScannerTypeItem(string caption, Scanners.SettingsScannerType scannerType)
			{
				this.Caption = caption;
				this.ScannerType = scannerType;
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public List<ComboColorModeItem>		ColorModes { get { return colorModes; } set { colorModes = value; } }
		public List<ComboScanModeItem>		ScanModes { get { return scanModes; } set { scanModes = value; } }
		public List<ComboScannerTypeItem>	ScannerTypes { get { return scannerTypes; } set { scannerTypes = value; } }

		#region SelectedScannerType
		public ComboScannerTypeItem SelectedScannerType
		{
			get
			{
				foreach (ComboScannerTypeItem scannerTypeItem in scannerTypes)
					if (scannerTypeItem.ScannerType == _settings.Scanner.General.ScannerType)
						return scannerTypeItem;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Scanner.General.ScannerType = value.ScannerType;
					RaisePropertyChanged("SelectedScannerType");
					RaisePropertyChanged("TwainPanelVisibility");
				}
			}
		}
		#endregion

		#region TwainPanelVisibility
		public Visibility TwainPanelVisibility
		{
			get
			{
				return (this.SelectedScannerType.ScannerType == Scanners.SettingsScannerType.iVinaFB6080E || 
					this.SelectedScannerType.ScannerType == Scanners.SettingsScannerType.iVinaFB6280E ||
					this.SelectedScannerType.ScannerType == Scanners.SettingsScannerType.KodakI1405 ||
					this.SelectedScannerType.ScannerType == Scanners.SettingsScannerType.KodakI1120 ||
					this.SelectedScannerType.ScannerType == Scanners.SettingsScannerType.KodakI1150 ||
					this.SelectedScannerType.ScannerType == Scanners.SettingsScannerType.KodakI1150New ||
					this.SelectedScannerType.ScannerType == Scanners.SettingsScannerType.KodakE1035 ||
					this.SelectedScannerType.ScannerType == Scanners.SettingsScannerType.KodakE1040) ?
					Visibility.Visible : Visibility.Hidden;
			}
			set
			{
			}
		}
		#endregion

		#region static AdfScannerTypes
		public static List<ComboScannerTypeItem> AdfScannerTypes 
		{
			get
			{
				List<ComboScannerTypeItem> scannerTypes = new List<ComboScannerTypeItem>();
				
				scannerTypes.Add(new ComboScannerTypeItem("Kodak i1120", Scanners.SettingsScannerType.KodakI1120));
				scannerTypes.Add(new ComboScannerTypeItem("Kodak i1150/i1180", Scanners.SettingsScannerType.KodakI1150));
				scannerTypes.Add(new ComboScannerTypeItem("Kodak i1150/i1180 (New)", Scanners.SettingsScannerType.KodakI1150New));
				scannerTypes.Add(new ComboScannerTypeItem("Kodak i1405", Scanners.SettingsScannerType.KodakI1405));
				scannerTypes.Add(new ComboScannerTypeItem("Kodak e1035", Scanners.SettingsScannerType.KodakE1035));
				scannerTypes.Add(new ComboScannerTypeItem("Kodak e1040", Scanners.SettingsScannerType.KodakE1040));

				return scannerTypes;
			}
		}
		#endregion

		#region static AdfScanModes
		public static List<ComboScanModeItem> AdfScanModes
		{
			get
			{
				List<ComboScanModeItem> scannerTypes = new List<ComboScanModeItem>();

				scannerTypes.Add(new ComboScanModeItem("Duplex",  Scanners.ScanMode.AdfDuplexMulti));
				scannerTypes.Add(new ComboScanModeItem("Simplex",  Scanners.ScanMode.AdfSimplexMulti));

				return scannerTypes;
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region ComboScannerType_SelectionChanged()
		private void ComboScannerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(comboScannerType.SelectedItem is ComboScannerTypeItem)
			{
				Scanners.SettingsScannerType scannerType = ((ComboScannerTypeItem)comboScannerType.SelectedItem).ScannerType;

				if (scannerType == Scanners.SettingsScannerType.iVinaFB6080E || scannerType == Scanners.SettingsScannerType.iVinaFB6280E ||
					scannerType == Scanners.SettingsScannerType.KodakI1405 || scannerType == Scanners.SettingsScannerType.KodakI1120 || 
					scannerType == Scanners.SettingsScannerType.KodakI1150 || scannerType == Scanners.SettingsScannerType.KodakI1150New ||
					scannerType == Scanners.SettingsScannerType.KodakE1035 || scannerType == Scanners.SettingsScannerType.KodakE1040 )
				{
					panelTwain.Visibility = Visibility.Visible;
					panelRebel.Visibility = Visibility.Hidden;
					panelClickMini.Visibility = Visibility.Hidden;
					panelS2N.Visibility = Visibility.Hidden;

                    if (scannerType == Scanners.SettingsScannerType.iVinaFB6080E || scannerType == Scanners.SettingsScannerType.iVinaFB6280E)
                    {
                        panelTwain.scannerAddOn.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //hide secondary scanner for Kodaks to avoid situation where primary and secondary scanner is same Kodak ADF
                        panelTwain.scannerAddOn.checkEnabled.IsChecked = false;
                        panelTwain.scannerAddOn.Visibility = Visibility.Hidden;
                    }
				}
				else if (scannerType == Scanners.SettingsScannerType.Click)
				{
					panelTwain.Visibility = Visibility.Hidden;
					panelRebel.Visibility = Visibility.Visible;
					panelClickMini.Visibility = Visibility.Hidden;
					panelS2N.Visibility = Visibility.Hidden;
				}
				else if (scannerType == Scanners.SettingsScannerType.ClickMini)
				{
					panelTwain.Visibility = Visibility.Hidden;
					panelRebel.Visibility = Visibility.Hidden;
					panelClickMini.Visibility = Visibility.Visible;
					panelS2N.Visibility = Visibility.Hidden;
				}
				else if(scannerType == Scanners.SettingsScannerType.S2N)
				{
					panelTwain.Visibility = Visibility.Hidden;
					panelRebel.Visibility = Visibility.Hidden;
					panelClickMini.Visibility = Visibility.Hidden;
					panelS2N.Visibility = Visibility.Visible;
				}
			}
		}
		#endregion

		#region CheckLicenseFile_Click()
		private void CheckLicenseFile_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (CheckLicenseFile_Request != null)
					CheckLicenseFile_Request(this, null);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region GetLicenseFile_Click()
		private void GetLicenseFile_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (DownloadLicenseFile_Request != null)
					DownloadLicenseFile_Request(this, null);
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
