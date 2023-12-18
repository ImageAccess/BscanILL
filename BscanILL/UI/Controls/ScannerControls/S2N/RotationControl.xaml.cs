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

namespace BscanILL.UI.Controls.ScannerControls.S2N
{
	/// <summary>
	/// Interaction logic for RotationControl.xaml
	/// </summary>
	public partial class RotationControl : ScannerControlBase
	{		
		static DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(Scanners.S2N.Rotation), typeof(RotationControl),
					new FrameworkPropertyMetadata(Scanners.S2N.Rotation.None, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RotationControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RotationControl));
		
		Brush nonInteractiveBrush;


		#region constructor
		public RotationControl()
		{
			InitializeComponent();

			this.nonInteractiveBrush = this.groupBox.BorderBrush;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.Rotation Value
		{
			get { return (Scanners.S2N.Rotation)GetValue(RotationControl.valueProperty); }
			set { SetValue(RotationControl.valueProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(RotationControl.valueChangedEvent, value); }
			remove { RemoveHandler(RotationControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(RotationControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(RotationControl.valueChangedByUserEvent, value); }
		}
		#endregion

		#region ScanSettings
		public Scanners.S2N.S2NSettings ScanSettings
		{
			get { return this.scanSettings; }
			set
			{
				if (this.scanSettings != value)
				{
					if (this.scanSettings != null)
						this.scanSettings.Rotation.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.Rotation.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Value = Scanners.S2N.Rotation.None;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Radio_Checked()
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.S2N.Rotation colorMode = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioNone)
					this.Value = Scanners.S2N.Rotation.None;
				else if (sender == this.radio90)
					this.Value = Scanners.S2N.Rotation.CV90;
				else if (sender == this.radio180)
					this.Value = Scanners.S2N.Rotation.CV180;
				else if (sender == this.radio270)
					this.Value = Scanners.S2N.Rotation.CV270;
			}

			e.Handled = true;

			if (this.Value != colorMode)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			RotationControl instance = o as RotationControl;

			switch (instance.Value)
			{
				case Scanners.S2N.Rotation.CV90:
					instance.radio90.IsChecked = true;
					break;
				case Scanners.S2N.Rotation.CV180:
					instance.radio180.IsChecked = true;
					break;
				case Scanners.S2N.Rotation.CV270:
					instance.radio270.IsChecked = true;
					break;
				default:
					instance.radioNone.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(RotationControl.valueChangedEvent, instance));
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
				this.Value = this.ScanSettings.Rotation.Value; 
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.Rotation.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
