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
	/// Interaction logic for ScannerAddOn.xaml
	/// </summary>
	public partial class ScannerAddOn : PanelBase
	{
		List<BscanILL.UI.Settings.Panels.Scanner.ComboScannerTypeItem>	scannerTypes = BscanILL.UI.Settings.Panels.Scanner.AdfScannerTypes;
		List<BscanILL.UI.Settings.Panels.Scanner.ComboScanModeItem>		scanModes = BscanILL.UI.Settings.Panels.Scanner.AdfScanModes;

		public event EventHandler ScanClick;
		public event EventHandler OpenSettingsClick;


		#region constructor
		public ScannerAddOn()
		{
			InitializeComponent();

			if(_settings != null)
				_settings.Scanner.AdfAddOnScanner.PropertyChanged += new PropertyChangedEventHandler(AdfAddOnScanner_PropertyChanged);

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public List<BscanILL.UI.Settings.Panels.Scanner.ComboScannerTypeItem> ScannerTypes { get { return scannerTypes; } /*set { scannerTypes = value; }*/ }
		public List<BscanILL.UI.Settings.Panels.Scanner.ComboScanModeItem>	ScanModes { get { return scanModes; } /*set { scannerTypes = value; }*/ }

		#region SelectedScannerType
		public BscanILL.UI.Settings.Panels.Scanner.ComboScannerTypeItem SelectedScannerType
		{
			get
			{
				foreach (BscanILL.UI.Settings.Panels.Scanner.ComboScannerTypeItem scannerTypeItem in scannerTypes)
					if (scannerTypeItem.ScannerType == _settings.Scanner.AdfAddOnScanner.ScannerType)
						return scannerTypeItem;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Scanner.AdfAddOnScanner.ScannerType = value.ScannerType;
					RaisePropertyChanged("SelectedScannerType");
				}
			}
		}
		#endregion

		#region SelectedScanMode
		public BscanILL.UI.Settings.Panels.Scanner.ComboScanModeItem SelectedScanMode
		{
			get
			{
				foreach (BscanILL.UI.Settings.Panels.Scanner.ComboScanModeItem item in this.scanModes)
					if (item.ScanMode == _settings.Scanner.AdfAddOnScanner.DefaultScanMode)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Scanner.AdfAddOnScanner.DefaultScanMode = value.ScanMode;
					RaisePropertyChanged("SelectedScanMode");
				}
			}
		}
		#endregion

		#region ScannerEnabled
		public bool ScannerEnabled
		{
			get { return _settings.Scanner.AdfAddOnScanner.IsEnabled; }
			set
			{
				_settings.Scanner.AdfAddOnScanner.IsEnabled = value;
				RaisePropertyChanged("ScannerAddOnEnabled");
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region TestScanAdf_Click()
		private void TestScanAdf_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (ScanClick != null)
					ScanClick(this, null);
				else
					throw new Exception("ScanClick event is not hooked up!");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region SettingsAdf_Click()
		private void SettingsAdf_Click(object sender, RoutedEventArgs e)
		{
			try
			{
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

		#region CheckScannerAddOn_CheckedChanged()
		private void CheckScannerAddOn_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.stackPanel.IsEnabled = this.checkEnabled.IsChecked.Value;
		}
		#endregion

		#region AdfAddOnScanner_PropertyChanged()
		void AdfAddOnScanner_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			BindingOperations.GetBindingExpression(checkEnabled, CheckBox.IsCheckedProperty).UpdateTarget();
			BindingOperations.GetBindingExpression(comboScannerType, ComboBox.SelectedItemProperty).UpdateTarget();
			BindingOperations.GetBindingExpression(comboDefaultScanMode, ComboBox.SelectedItemProperty).UpdateTarget();
		}
		#endregion

		#endregion


	}
}
