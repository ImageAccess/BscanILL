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
	/// Interaction logic for PageSplittingControl.xaml
	/// </summary>
	public partial class PageSplittingControl : ScannerControlBase
	{
        static DependencyProperty scanModeProperty = DependencyProperty.Register("Value", typeof(Scanners.S2N.Splitting), typeof(PageSplittingControl),
                    new FrameworkPropertyMetadata(Scanners.S2N.Splitting.Off, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageSplittingControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageSplittingControl));

        public event BscanILL.Misc.VoidHnd SettingsChanged;
        public event Scanners.S2N.ScanRequestHnd SplittingSettingsChanged;        

		#region constructor
		public PageSplittingControl()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.Splitting Value
		{
            get { return (Scanners.S2N.Splitting)GetValue(PageSplittingControl.scanModeProperty); }
			set { SetValue(PageSplittingControl.scanModeProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(PageSplittingControl.valueChangedEvent, value); }
			remove { RemoveHandler(PageSplittingControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(PageSplittingControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(PageSplittingControl.valueChangedByUserEvent, value); }
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
						this.scanSettings.Splitting.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.Splitting.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Value = Scanners.Twain.BookedgePage.FlatMode;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Radio_Checked()
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.S2N.Splitting pageSplittingMode = this.Value;

			if (this.IsLoaded)
			{
                if (sender == this.radioLeft)
                    this.Value = Scanners.S2N.Splitting.Left;
                else if (sender == this.radioRight)
                    this.Value = Scanners.S2N.Splitting.Right;
                else if (sender == this.radioAuto)
                    this.Value = Scanners.S2N.Splitting.Auto;
                else
                    this.Value = Scanners.S2N.Splitting.Off;
			}

			e.Handled = true;

            if (this.Value != pageSplittingMode)
            {
                this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
                if (SettingsChanged != null)
                {
                    SettingsChanged();
                }
                if (SplittingSettingsChanged != null)
                {
                    SplittingSettingsChanged((Scanners.S2N.ScannerScanAreaSelection)this.Value);
                }
            }
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			PageSplittingControl instance = o as PageSplittingControl;

			switch (instance.Value)
			{
                case Scanners.S2N.Splitting.Left:
					instance.radioLeft.IsChecked = true;
					break;
                case Scanners.S2N.Splitting.Right:
					instance.radioRight.IsChecked = true;
					break;
                case Scanners.S2N.Splitting.Auto:
			   		instance.radioAuto.IsChecked = true;
					break;
				default:
					instance.radioBoth.IsChecked = true;  //full bed
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(PageSplittingControl.valueChangedEvent, instance));
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
				this.Value = this.ScanSettings.Splitting.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.Splitting.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
