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
	/// Interaction logic for DocSizeControl.xaml
	/// </summary>
	public partial class DocSizeControl : ScannerControlBase
	{
		static DependencyProperty colorModeProperty = DependencyProperty.Register("Value", typeof(Scanners.S2N.DocumentSize), typeof(DocSizeControl),
					new FrameworkPropertyMetadata(Scanners.S2N.DocumentSize.Auto, new PropertyChangedCallback(OnDocSizeChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DocSizeControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DocSizeControl));
		
		Brush nonInteractiveBrush;


		#region constructor
		public DocSizeControl()
		{
			InitializeComponent();

			this.nonInteractiveBrush = this.groupBox.BorderBrush;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.DocumentSize Value
		{
			get { return (Scanners.S2N.DocumentSize)GetValue(DocSizeControl.colorModeProperty); }
			set { SetValue(DocSizeControl.colorModeProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(DocSizeControl.valueChangedEvent, value); }
			remove { RemoveHandler(DocSizeControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(DocSizeControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(DocSizeControl.valueChangedByUserEvent, value); }
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
						this.scanSettings.DocSize.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.DocSize.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Value = Scanners.S2N.DocumentSize.Auto;
		}*/
		#endregion

        #region AdjustDocSizeOptionOnDocModeChange()
        public void AdjustDocSizeOptionOnDocModeChange(Scanners.S2N.DocumentMode newDocMode)
        {
          if (newDocMode == Scanners.S2N.DocumentMode.Book)
          {
              // disable Max page size control and force auto size in Book mode
              if(this.Value == Scanners.S2N.DocumentSize.MaximumLandscape)
              {
                  this.Value = Scanners.S2N.DocumentSize.Auto;                  

                  this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
              }
              this.radioMaximum.IsEnabled = false;
          }
          else
          {
              this.radioMaximum.IsEnabled = true;
          }
        }
        #endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Radio_Checked()
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.S2N.DocumentSize colorMode = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioAuto)
					this.Value = Scanners.S2N.DocumentSize.Auto;
				else if (sender == this.radioMaximum)
					this.Value = Scanners.S2N.DocumentSize.MaximumLandscape;
			}

			e.Handled = true;

			if (this.Value != colorMode)
				this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region OnDocSizeChanged()
		private static void OnDocSizeChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			DocSizeControl instance = o as DocSizeControl;

			switch (instance.Value)
			{
				case Scanners.S2N.DocumentSize.MaximumLandscape:
					instance.radioMaximum.IsChecked = true;
					break;
				default:
					instance.radioAuto.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(DocSizeControl.valueChangedEvent, instance));
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
				this.Value = this.ScanSettings.DocSize.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.DocSize.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
