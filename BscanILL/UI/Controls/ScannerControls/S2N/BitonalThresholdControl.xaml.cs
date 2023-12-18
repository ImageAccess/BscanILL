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
	/// Interaction logic for BitonalThreshold.xaml
	/// </summary>
	public partial class BitonalThreshold : ScannerControlBase
	{
		static DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(Scanners.S2N.BitonalThreshold), typeof(BitonalThreshold),
					new FrameworkPropertyMetadata(Scanners.S2N.BitonalThreshold.Fixed, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BitonalThreshold));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BitonalThreshold));
		
		Brush nonInteractiveBrush;


		#region constructor
		public BitonalThreshold()
		{
			InitializeComponent();

			this.nonInteractiveBrush = this.groupBox.BorderBrush;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.BitonalThreshold Value
		{
			get { return (Scanners.S2N.BitonalThreshold)GetValue(BitonalThreshold.valueProperty); }
			set { SetValue(BitonalThreshold.valueProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(BitonalThreshold.valueChangedEvent, value); }
			remove { RemoveHandler(BitonalThreshold.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(BitonalThreshold.valueChangedByUserEvent, value); }
			remove { RemoveHandler(BitonalThreshold.valueChangedByUserEvent, value); }
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
						this.scanSettings.BitonalThreshold.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.BitonalThreshold.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
						ApplyFromSettings();
					}
				}
			}
		}

		void BitonalThreshold_Changed()
		{
			throw new NotImplementedException();
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Reset()
		/*public void Reset()
		{
			this.Value = (Scanners.S2N.BitonalThreshold)valueProperty.DefaultMetadata.DefaultValue; ;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region RadioButtonColor_Checked()
		private void RadioButtonColor_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.S2N.BitonalThreshold colorMode = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioDynamic)
					this.Value = Scanners.S2N.BitonalThreshold.Dynamic;
				else if (sender == this.radioFixed)
					this.Value = Scanners.S2N.BitonalThreshold.Fixed;
			}

			e.Handled = true;

			if (this.Value != colorMode)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			BitonalThreshold instance = o as BitonalThreshold;

			switch (instance.Value)
			{
				case Scanners.S2N.BitonalThreshold.Dynamic:
					instance.radioDynamic.IsChecked = true;
					break;
				default:
					instance.radioFixed.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(BitonalThreshold.valueChangedEvent, instance));
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
				this.Value = this.ScanSettings.BitonalThreshold.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.BitonalThreshold.Value = this.Value;
		}
		#endregion

		#endregion

	}
}
