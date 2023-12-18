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
	/// Interaction logic for DespeckleControl.xaml
	/// </summary>
	public partial class DespeckleControl : ScannerControlBase
	{		
		static DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(Scanners.S2N.Despeckle), typeof(DespeckleControl),
					new FrameworkPropertyMetadata(Scanners.S2N.Despeckle.Off, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DespeckleControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DespeckleControl));
		
		Brush nonInteractiveBrush;


		#region constructor
		public DespeckleControl()
		{
			InitializeComponent();

			this.nonInteractiveBrush = this.groupBox.BorderBrush;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.Despeckle Value
		{
			get { return (Scanners.S2N.Despeckle)GetValue(DespeckleControl.valueProperty); }
			set { SetValue(DespeckleControl.valueProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(DespeckleControl.valueChangedEvent, value); }
			remove { RemoveHandler(DespeckleControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(DespeckleControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(DespeckleControl.valueChangedByUserEvent, value); }
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
						this.scanSettings.Despeckle.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.Despeckle.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Value = (Scanners.S2N.Despeckle)valueProperty.DefaultMetadata.DefaultValue;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Radio_Checked()
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.S2N.Despeckle colorMode = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioNone)
					this.Value = Scanners.S2N.Despeckle.Off;
				else if (sender == this.radio4x4)
					this.Value = Scanners.S2N.Despeckle.Despeckle4x4;
			}

			e.Handled = true;

			if (this.Value != colorMode)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			DespeckleControl instance = o as DespeckleControl;

			switch (instance.Value)
			{
				case Scanners.S2N.Despeckle.Off:
					instance.radioNone.IsChecked = true;
					break;
				case Scanners.S2N.Despeckle.Despeckle4x4:
					instance.radio4x4.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(DespeckleControl.valueChangedEvent, instance));
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
				this.Value = this.ScanSettings.Despeckle.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.Despeckle.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
