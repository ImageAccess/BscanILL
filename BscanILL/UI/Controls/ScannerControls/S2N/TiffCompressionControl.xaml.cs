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
	/// Interaction logic for TiffCompressionControl.xaml
	/// </summary>
	public partial class TiffCompressionControl : ScannerControlBase
	{		
		static DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(Scanners.S2N.TiffCompression), typeof(TiffCompressionControl),
					new FrameworkPropertyMetadata(Scanners.S2N.TiffCompression.G4, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TiffCompressionControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TiffCompressionControl));
		

		#region constructor
		public TiffCompressionControl()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.TiffCompression Value
		{
			get { return (Scanners.S2N.TiffCompression)GetValue(TiffCompressionControl.valueProperty); }
			set { SetValue(TiffCompressionControl.valueProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(TiffCompressionControl.valueChangedEvent, value); }
			remove { RemoveHandler(TiffCompressionControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(TiffCompressionControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(TiffCompressionControl.valueChangedByUserEvent, value); }
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
						this.scanSettings.TiffCompression.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.TiffCompression.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Value = Scanners.S2N.TiffCompression.G4;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Radio_Checked()
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.S2N.TiffCompression tmp = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioNone)
					this.Value = Scanners.S2N.TiffCompression.None;
				else if (sender == this.radioG4)
					this.Value = Scanners.S2N.TiffCompression.G4;
			}

			e.Handled = true;

			if (this.Value != tmp)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			TiffCompressionControl instance = o as TiffCompressionControl;

			switch (instance.Value)
			{
				case Scanners.S2N.TiffCompression.None:
					instance.radioNone.IsChecked = true;
					break;
				default:
					instance.radioG4.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(TiffCompressionControl.valueChangedEvent, instance));
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
				this.Value = this.ScanSettings.TiffCompression.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.TiffCompression.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
