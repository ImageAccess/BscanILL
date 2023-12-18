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
	/// Interaction logic for PageSplittingStartPageControl.xaml
	/// </summary>
	public partial class PageSplittingStartPageControl : ScannerControlBase
	{
        static DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(Scanners.S2N.SplittingStartPage), typeof(PageSplittingStartPageControl),
                    new FrameworkPropertyMetadata(Scanners.S2N.SplittingStartPage.Left, new PropertyChangedCallback(OnValueChanged)));

        static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageSplittingStartPageControl));
        static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageSplittingStartPageControl));
		
		Brush nonInteractiveBrush;        

		#region constructor
        public PageSplittingStartPageControl()
		{
			InitializeComponent();

			this.nonInteractiveBrush = this.groupBox.BorderBrush;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
        public Scanners.S2N.SplittingStartPage Value
		{
            get { return (Scanners.S2N.SplittingStartPage)GetValue(PageSplittingStartPageControl.valueProperty); }
            set { SetValue(PageSplittingStartPageControl.valueProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
            add { AddHandler(PageSplittingStartPageControl.valueChangedEvent, value); }
            remove { RemoveHandler(PageSplittingStartPageControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
            add { AddHandler(PageSplittingStartPageControl.valueChangedByUserEvent, value); }
            remove { RemoveHandler(PageSplittingStartPageControl.valueChangedByUserEvent, value); }
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
						this.scanSettings.Splitting_StartPage.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
                        this.scanSettings.Splitting_StartPage.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
						ApplyFromSettings();
					}
				}
			}
		}

        void PageSplittingStartPage_Changed()
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
			this.Value = (Scanners.S2N.SplittingStartPage)valueProperty.DefaultMetadata.DefaultValue; ;
		}*/
        #endregion

        #endregion


        //PRIVATE METHODS
		#region private methods

        #region RadioButtonSplittingStartPage_Checked()
        private void RadioButtonSplittingStartPage_Checked(object sender, RoutedEventArgs e)
		{
            Scanners.S2N.SplittingStartPage splittingFirstPage = this.Value;

			if (this.IsLoaded)
			{
                if (sender == this.radioLeftPage)
					this.Value = Scanners.S2N.SplittingStartPage.Left;
                else if (sender == this.radioRightPage)
                    this.Value = Scanners.S2N.SplittingStartPage.Right;
			}

			e.Handled = true;

            if (this.Value != splittingFirstPage)
            {
                this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
            }
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
            PageSplittingStartPageControl instance = o as PageSplittingStartPageControl;

			switch (instance.Value)
			{
				case Scanners.S2N.SplittingStartPage.Right:
					instance.radioRightPage.IsChecked = true;
					break;
				default:
                    instance.radioLeftPage.IsChecked = true;
					break;
			}

			instance.ApplyToSettings();
            instance.RaiseEvent(new RoutedEventArgs(PageSplittingStartPageControl.valueChangedEvent, instance));
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
				this.Value = this.ScanSettings.Splitting_StartPage.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
                this.ScanSettings.Splitting_StartPage.Value = this.Value;
		}
		#endregion

		#endregion

	}
}
