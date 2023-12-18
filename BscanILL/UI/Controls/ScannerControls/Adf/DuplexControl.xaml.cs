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

namespace BscanILL.UI.Controls.ScannerControls.Adf
{
	/// <summary>
	/// Interaction logic for DuplexControl.xaml
	/// </summary>
	public partial class DuplexControl : BscanILL.UI.Controls.ScannerControls.Adf.ScannerControlBase
	{
		static DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(bool), typeof(DuplexControl),
					new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DuplexControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DuplexControl));
		
		Brush	nonInteractiveBrush;


		#region constructor
		public DuplexControl()
		{
			InitializeComponent();

			this.nonInteractiveBrush = this.groupBox.BorderBrush;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public bool Value
		{
			get { return (bool)GetValue(DuplexControl.valueProperty); }
			set { SetValue(DuplexControl.valueProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(DuplexControl.valueChangedEvent, value); }
			remove { RemoveHandler(DuplexControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(DuplexControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(DuplexControl.valueChangedByUserEvent, value); }
		}
		#endregion

		#region ScanSettings
		public Scanners.Twain.AdfSettings ScanSettings
		{
			get { return this.scanSettings; }
			set
			{
				if (this.scanSettings != value)
				{
					if (this.scanSettings != null)
						this.scanSettings.Duplex.Changed -= new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.Duplex.Changed += new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
						ApplyFromSettings();
					}
				}
			}
		}
		#endregion
	
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region RadioButton_Checked()
		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			bool previous = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioDuplex)
					this.Value = true;
				else if (sender == this.radioSimplex)
					this.Value = false;
			}

			e.Handled = true;

			if (this.Value != previous)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			DuplexControl instance = o as DuplexControl;

			if (instance.Value)
				instance.radioDuplex.IsChecked = true;
			else
				instance.radioSimplex.IsChecked = true;

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(DuplexControl.valueChangedEvent, instance));
		}
		#endregion

		#region ApplyFromSettings()
		protected override void ApplyFromSettings()
		{
			if (this.ScanSettings != null)
				this.Value = this.ScanSettings.Duplex.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.Duplex.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
