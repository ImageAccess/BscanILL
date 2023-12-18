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

namespace BscanILL.UI.Controls.ScannerControls.Click
{
	/// <summary>
	/// Interaction logic for ScanPageControl.xaml
	/// </summary>
	public partial class ScanPageControl : BscanILL.UI.Controls.ScannerControls.Click.ScannerControlBase
	{
		static DependencyProperty scanModeProperty = DependencyProperty.Register("Value", typeof(Scanners.Click.ClickScanPage), typeof(ScanPageControl),
					new FrameworkPropertyMetadata(Scanners.Click.ClickScanPage.Both, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanPageControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanPageControl));
		

		#region constructor
		public ScanPageControl()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.Click.ClickScanPage Value
		{
			get { return (Scanners.Click.ClickScanPage)GetValue(ScanPageControl.scanModeProperty); }
			set { SetValue(ScanPageControl.scanModeProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(ScanPageControl.valueChangedEvent, value); }
			remove { RemoveHandler(ScanPageControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(ScanPageControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(ScanPageControl.valueChangedByUserEvent, value); }
		}
		#endregion

		#region ScanSettings
		public Scanners.Click.ClickSettings ScanSettings
		{
			get { return this.scanSettings; }
			set
			{
				if (this.scanSettings != value)
				{
					if (this.scanSettings != null)
						this.scanSettings.ScanPage.Changed -= new Scanners.Click.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.ScanPage.Changed += new Scanners.Click.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Value = Scanners.Click.ClickScanPage.FlatMode;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Radio_Checked()
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.Click.ClickScanPage scanMode = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioLeft)
					this.Value = Scanners.Click.ClickScanPage.Left;
				else if (sender == this.radioRight)
					this.Value = Scanners.Click.ClickScanPage.Right;
				else
					this.Value = Scanners.Click.ClickScanPage.Both;
			}

			e.Handled = true;

			if (this.Value != scanMode)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			ScanPageControl instance = o as ScanPageControl;

			switch (instance.Value)
			{
				case Scanners.Click.ClickScanPage.Left:
					instance.radioLeft.IsChecked = true;
					break;
				case Scanners.Click.ClickScanPage.Right:
					instance.radioRight.IsChecked = true;
					break;
				default:
					instance.radioBoth.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(ScanPageControl.valueChangedEvent, instance));
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
				this.Value = this.ScanSettings.ScanPage.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.ScanPage.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
