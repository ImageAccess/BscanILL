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

namespace BscanILL.UI.Controls.ScannerControls.BookEdge
{
	/// <summary>
	/// Interaction logic for PageControl.xaml
	/// </summary>
	public partial class PageControl : ScannerControlBase
	{
		static DependencyProperty scanModeProperty = DependencyProperty.Register("Value", typeof(Scanners.Twain.BookedgePage), typeof(PageControl),
					new FrameworkPropertyMetadata(Scanners.Twain.BookedgePage.FlatMode, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageControl));
		

		#region constructor
		public PageControl()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.Twain.BookedgePage Value
		{
			get { return (Scanners.Twain.BookedgePage)GetValue(PageControl.scanModeProperty); }
			set { SetValue(PageControl.scanModeProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(PageControl.valueChangedEvent, value); }
			remove { RemoveHandler(PageControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(PageControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(PageControl.valueChangedByUserEvent, value); }
		}
		#endregion

		#region ScanSettings
		public Scanners.Twain.BookedgeSettings ScanSettings
		{
			get { return this.scanSettings; }
			set
			{
				if (this.scanSettings != value)
				{
					if (this.scanSettings != null)
						this.scanSettings.ScanPage.Changed -= new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.ScanPage.Changed += new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			Scanners.Twain.BookedgePage scanMode = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioLeft)
					this.Value = Scanners.Twain.BookedgePage.LeftPage;
				else if (sender == this.radioRight)
					this.Value = Scanners.Twain.BookedgePage.RightPage;
				else if (sender == this.radioAuto)
					this.Value = Scanners.Twain.BookedgePage.Automatic;
				else
					this.Value = Scanners.Twain.BookedgePage.FlatMode;
			}

			e.Handled = true;

			if (this.Value != scanMode)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			PageControl instance = o as PageControl;

			switch (instance.Value)
			{
				case Scanners.Twain.BookedgePage.LeftPage:
					instance.radioLeft.IsChecked = true;
					break;
				case Scanners.Twain.BookedgePage.RightPage:
					instance.radioRight.IsChecked = true;
					break;
				case Scanners.Twain.BookedgePage.Automatic:
					instance.radioAuto.IsChecked = true;
					break;
				default:
					instance.radioFlat.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(PageControl.valueChangedEvent, instance));
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
