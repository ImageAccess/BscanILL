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
	/// Interaction logic for FileFormatControl.xaml
	/// </summary>
	public partial class FileFormatControl : ScannerControlBase
	{
		static DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(Scanners.FileFormat), typeof(FileFormatControl),
					new FrameworkPropertyMetadata(Scanners.FileFormat.Tiff, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileFormatControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileFormatControl));
		
		Brush	nonInteractiveBrush;
		bool	isPngVisible = true;


		#region constructor
		public FileFormatControl()
		{
			InitializeComponent();

			this.nonInteractiveBrush = this.groupBox.BorderBrush;
			this.IsPngVisible = false;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.FileFormat Value
		{
			get { return (Scanners.FileFormat)GetValue(FileFormatControl.valueProperty); }
			set { SetValue(FileFormatControl.valueProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(FileFormatControl.valueChangedEvent, value); }
			remove { RemoveHandler(FileFormatControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(FileFormatControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(FileFormatControl.valueChangedByUserEvent, value); }
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
						this.scanSettings.FileFormat.Changed -= new Scanners.Click.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.FileFormat.Changed += new Scanners.Click.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
						ApplyFromSettings();
					}
				}
			}
		}
		#endregion
	
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		#region IsPngVisible
		private bool IsPngVisible
		{
			get{return this.isPngVisible;}
			set
			{
				if (this.isPngVisible != value)
				{
					this.isPngVisible = value;
					columnPng.Width = (this.isPngVisible) ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
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
			if (valueProperty != null && valueProperty.DefaultMetadata != null)
				this.Value = (Scanners.FileFormat)valueProperty.DefaultMetadata.DefaultValue;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region RadioButton_Checked()
		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.FileFormat colorMode = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioTiff)
				{
					this.Value = Scanners.FileFormat.Tiff;
				}
				else if (sender == this.radioPng)
				{
					this.Value = Scanners.FileFormat.Png;
				}
				else
				{
					this.Value = Scanners.FileFormat.Jpeg;
				}
			}

			e.Handled = true;

			if (this.Value != colorMode)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			FileFormatControl instance = o as FileFormatControl;

			switch (instance.Value)
			{
				case Scanners.FileFormat.Tiff:
					instance.radioTiff.IsChecked = true;
					break;
				case Scanners.FileFormat.Png:
					instance.radioPng.IsChecked = true;
					break;
				default:
					instance.radioJpeg.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(FileFormatControl.valueChangedEvent, instance));
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
			if (this.ScanSettings != null && this.Value != this.ScanSettings.FileFormat.Value)
			{
				this.Value = this.ScanSettings.FileFormat.Value;
				//this.IsPngVisible = true;
			}
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.FileFormat.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
