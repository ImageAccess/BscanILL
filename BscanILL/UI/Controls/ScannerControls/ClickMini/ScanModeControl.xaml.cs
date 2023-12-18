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
using System.Windows.Media.Animation;
using System.Windows.Automation.Peers;

namespace BscanILL.UI.Controls.ScannerControls.ClickMini
{
	/// <summary>
	/// Interaction logic for ScanModeControl.xaml
	/// </summary>
	public partial class ScanModeControl : BscanILL.UI.Controls.ScannerControls.ClickMini.ScannerControlBase
	{
		static DependencyProperty scanModeProperty = DependencyProperty.Register("Value", typeof(Scanners.Click.ClickMiniScanMode), typeof(ScanModeControl),
					new FrameworkPropertyMetadata(Scanners.Click.ClickMiniScanMode.SplitImage, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanModeControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanModeControl));
		

		#region constructor
		public ScanModeControl()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.Click.ClickMiniScanMode Value
		{
			get { return (Scanners.Click.ClickMiniScanMode)GetValue(ScanModeControl.scanModeProperty); }
			set { SetValue(ScanModeControl.scanModeProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(ScanModeControl.valueChangedEvent, value); }
			remove { RemoveHandler(ScanModeControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(ScanModeControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(ScanModeControl.valueChangedByUserEvent, value); }
		}
		#endregion

		#region ScanSettings
		public Scanners.Click.ClickMiniSettings ScanSettings
		{
			get { return this.scanSettings; }
			set
			{
				if (this.scanSettings != value)
				{
					if (this.scanSettings != null)
						this.scanSettings.ScanMode.Changed -= new Scanners.Click.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.ScanMode.Changed += new Scanners.Click.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
						ApplyFromSettings();
					}
				}
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Reset()
		/*public void Reset()
		{
			this.Value = Scanners.Click.ClickMiniScanMode.FlatMode;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Radio_Checked()
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.Click.ClickMiniScanMode scanMode = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioBook)
					this.Value = Scanners.Click.ClickMiniScanMode.BookMode;
				else
					this.Value = Scanners.Click.ClickMiniScanMode.SplitImage;
			}

			e.Handled = true;

			if (this.Value != scanMode)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			ScanModeControl instance = o as ScanModeControl;

			switch (instance.Value)
			{
				case Scanners.Click.ClickMiniScanMode.BookMode:
					instance.radioBook.IsChecked = true;
					break;
				default:
					instance.radioFlat.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(ScanModeControl.valueChangedEvent, instance));
		}
		#endregion

		#region Radio_PreviewMouseDown()
		private void Radio_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
		}
		#endregion

		#region ApplyFromSettings()
		protected override void ApplyFromSettings()
		{
			if (this.ScanSettings != null)
				this.Value = this.ScanSettings.ScanMode.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.ScanMode.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
