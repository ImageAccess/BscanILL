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
	/// Interaction logic for ColorModeControl.xaml
	/// </summary>
	public partial class ColorModeControl : ScannerControlBase
	{
		static DependencyProperty colorModeProperty = DependencyProperty.Register("Value", typeof(Scanners.S2N.ColorMode), typeof(ColorModeControl),
					new FrameworkPropertyMetadata(Scanners.S2N.ColorMode.Color, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorModeControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorModeControl));
		
		Brush nonInteractiveBrush;

        public event BscanILL.Misc.VoidHnd SettingsChanged;

		#region constructor
		public ColorModeControl()
		{
			InitializeComponent();

			this.nonInteractiveBrush = this.groupBox.BorderBrush;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		
		#region Value
		public Scanners.S2N.ColorMode Value
		{
			get { return (Scanners.S2N.ColorMode)GetValue(ColorModeControl.colorModeProperty); }
			set { SetValue(ColorModeControl.colorModeProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(ColorModeControl.valueChangedEvent, value); }
			remove { RemoveHandler(ColorModeControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(ColorModeControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(ColorModeControl.valueChangedByUserEvent, value); }
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
						this.scanSettings.ColorMode.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.ColorMode.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Value = BscanILL.SETTINGS.Settings.Instance.Scanner.General.DefaultColorMode;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region RadioButtonColor_Checked()
		private void RadioButtonColor_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.S2N.ColorMode colorMode = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioColor)
				{
					this.Value = Scanners.S2N.ColorMode.Color;
					//this.radioButtonColorMode.Background = this.radioColor.Background;
				}
				else if (sender == this.radioGrayscale)
				{
					this.Value = Scanners.S2N.ColorMode.Grayscale;
					//this.radioButtonColorMode.Background = this.radioGrayscale.Background;
				}
				else
				{
					this.Value = Scanners.S2N.ColorMode.Lineart;
					//this.radioButtonColorMode.Background = this.radioBw.Background;
				}
			}

			e.Handled = true;

            if (this.Value != colorMode)
            {
                this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));

                if( SettingsChanged != null )
                {
                    SettingsChanged();
                }
            }
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			ColorModeControl instance = o as ColorModeControl;

			switch (instance.Value)
			{
				case Scanners.S2N.ColorMode.Grayscale:
					instance.radioGrayscale.IsChecked = true;
					break;
				case Scanners.S2N.ColorMode.Lineart:
					instance.radioBw.IsChecked = true;
					break;
				default:
					instance.radioColor.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(ColorModeControl.valueChangedEvent, instance));
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
				this.Value = this.ScanSettings.ColorMode.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
            if (this.ScanSettings != null)
            {
                this.ScanSettings.ColorMode.Value = this.Value;
                if( ( this.ScanSettings.ColorMode.Value == Scanners.S2N.ColorMode.Lineart ) ||  ( this.ScanSettings.ColorMode.Value == Scanners.S2N.ColorMode.Photo ) )
                {
                    // in case of B&W color mode - force G4 compression
                    this.ScanSettings.TiffCompression.Value = Scanners.S2N.TiffCompression.G4;
                }
            }
		}
		#endregion

		#endregion
	}
}
