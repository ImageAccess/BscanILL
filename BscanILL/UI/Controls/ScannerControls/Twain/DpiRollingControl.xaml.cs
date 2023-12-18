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

namespace BscanILL.UI.Controls.ScannerControls.Twain
{
	/// <summary>
	/// Interaction logic for DpiRollingControl.xaml
	/// </summary>
	public partial class DpiRollingControl : ScannerControlBase
	{
		static DependencyProperty	dpiProperty = DependencyProperty.Register("Value", typeof(ushort), typeof(BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl), new FrameworkPropertyMetadata((ushort)300, new PropertyChangedCallback(OnDpiChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl));

		GridLengthAnimation row2Animation = new GridLengthAnimation();
		Storyboard			storyboard = new Storyboard();

		ThicknessAnimation	valueChangeAnimation = new ThicknessAnimation();
		Storyboard			valueChangeStoryboard = new Storyboard();

		ushort? scanDpi = null;


		#region constructor
		public DpiRollingControl()
		{
			InitializeComponent();

			row2Animation.Duration = new Duration(TimeSpan.Parse("00:00:00.3"));
			row2Animation.FillBehavior = FillBehavior.Stop;
			Storyboard.SetTarget(row2Animation, scrollViewer);
			Storyboard.SetTargetProperty(row2Animation, new PropertyPath(RowDefinition.HeightProperty));

			//storyboard.Children.Add(row0Animation);
			storyboard.Children.Add(row2Animation);
			storyboard.Completed += new EventHandler(Storyboard_Completed);

			valueChangeAnimation.Duration = new Duration(TimeSpan.Parse("00:00:00.3"));
			valueChangeAnimation.FillBehavior = FillBehavior.Stop;
			Storyboard.SetTarget(valueChangeAnimation, stackPanel);
			Storyboard.SetTargetProperty(valueChangeAnimation, new PropertyPath(Grid.MarginProperty));

			valueChangeStoryboard.Children.Add(valueChangeAnimation);
			valueChangeStoryboard.Completed += new EventHandler(ValueChangeStoryboard_Completed);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public ushort Value
		{
			get { return (ushort)GetValue(BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl.dpiProperty); }
			set { SetValue(BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl.dpiProperty, value); }
		}
		#endregion

		#region ValueChanged
		public event RoutedEventHandler ValueChanged
		{
			add { AddHandler(BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl.valueChangedEvent, value); }
			remove { RemoveHandler(BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl.valueChangedByUserEvent, value); }
		}
		#endregion

		#region ScanSettings
		public Scanners.Twain.TwainSettings ScanSettings
		{
			get { return this.scanSettings; }
			set
			{
				if (this.scanSettings != value)
				{
					if (this.scanSettings != null)
						this.scanSettings.Dpi.Changed -= new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.Dpi.Changed += new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
						ApplyFromSettings();
					}
				}
			}
		}
		#endregion
	
		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		#region ScanDpi
		private ushort? ScanDpi
		{
			set
			{
				if (this.scanDpi != value)
				{
					this.scanDpi = value;

					this.buttonScrollUp.IsEnabled = (this.scanDpi != null && this.Value < this.scanDpi.Value) || (this.scanDpi == null && this.Value < 600); ;
					this.buttonScrollDown.IsEnabled = (this.Value > 150);
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
			this.ScanDpi = null; 
			Value = (ushort)dpiProperty.DefaultMetadata.DefaultValue;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region OnInteractiveChanged()
		private static void OnInteractiveChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl instance = o as BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl;

			if ((bool)args.NewValue)
			{
				instance.row2Animation.From = instance.scrollViewer.ActualHeight;
				instance.row2Animation.To = instance.contentGrid.ActualHeight / 2.0;
			}
			else
			{
				instance.row2Animation.From = instance.scrollViewer.ActualHeight;
				instance.row2Animation.To = 0;
			}

			instance.storyboard.Begin(instance);
		}
		#endregion
	
		#region OnDpiChanged()
		private static void OnDpiChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl instance = o as BscanILL.UI.Controls.ScannerControls.Twain.DpiRollingControl;

			switch (instance.Value)
			{
				case 600:
					instance.valueChangeAnimation.From = instance.stackPanel.Margin;
					instance.valueChangeAnimation.To = new Thickness(0, 0, 0, 0);
					break;
				case 400:
					instance.valueChangeAnimation.From = instance.stackPanel.Margin;
					instance.valueChangeAnimation.To = new Thickness(0, -instance.stackPanel.Height * 1 / 5, 0, 0);
					break;
				case 300:
					instance.valueChangeAnimation.From = instance.stackPanel.Margin;
					instance.valueChangeAnimation.To = new Thickness(0, -instance.stackPanel.Height * 2 / 5, 0, 0);
					break;
				case 200:
					instance.valueChangeAnimation.From = instance.stackPanel.Margin;
					instance.valueChangeAnimation.To = new Thickness(0, -instance.stackPanel.Height * 3 / 5, 0, 0);
					break;
				default:
					instance.valueChangeAnimation.From = instance.stackPanel.Margin;
					instance.valueChangeAnimation.To = new Thickness(0, -instance.stackPanel.Height * 4 / 5, 0, 0);
					break;
			}

			instance.buttonScrollUp.IsEnabled = (instance.scanDpi != null && instance.Value < instance.scanDpi.Value) || (instance.scanDpi == null && instance.Value < 600);
			instance.buttonScrollDown.IsEnabled = (instance.Value > 150);
			instance.valueChangeStoryboard.Begin();

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(valueChangedEvent, instance));
		}
		#endregion

		#region ScrollViewer_SizeChanged
		private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			AdjustPosition();
		}
		#endregion

		#region AdjustPosition
		private void AdjustPosition()
		{
			this.stackPanel.Height = this.scrollViewer.ActualHeight * 5;

			switch (this.Value)
			{
				case 75:
				case 150:
					this.stackPanel.Margin = new Thickness(0, -this.stackPanel.Height * 4 / 5, 0, 0);
					break;
				case 200:
					this.stackPanel.Margin = new Thickness(0, -this.stackPanel.Height * 3 / 5, 0, 0);
					break;
				case 400:
					this.stackPanel.Margin = new Thickness(0, -this.stackPanel.Height * 1 / 5, 0, 0);
					break;
				case 600:
					this.stackPanel.Margin = new Thickness(0, 0, 0, 0);
					break;
				default:
					this.stackPanel.Margin = new Thickness(0, -this.stackPanel.Height * 2 / 5, 0, 0);
					break;
			}
		}
		#endregion

		#region Storyboard_Completed()
		void Storyboard_Completed(object sender, EventArgs e)
		{
		}
		#endregion

		#region ValueChangeStoryboard_Completed()
		void ValueChangeStoryboard_Completed(object sender, EventArgs e)
		{
			AdjustPosition();
		}
		#endregion

		#region ScrollUp_Click()
		private void ScrollUp_Click(object sender, RoutedEventArgs e)
		{
			ushort dpi = this.Value;
			
			switch (this.Value)
			{
				case 75:
				case 150: 
				this.Value = 200; break;
				case 200: this.Value = 300; break;
				case 300: this.Value = 400; break;
				case 400: this.Value = 600; break;
				case 600: this.Value = 600; break;
			}

			if(dpi != this.Value && valueChangedByUserEvent != null)
				RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region ScrollDown_Click()
		private void ScrollDown_Click(object sender, RoutedEventArgs e)
		{
			ushort dpi = this.Value;

			switch (this.Value)
			{
				case 75:
				case 150:
					break;
				case 200: this.Value = 150; break;
				case 300: this.Value = 200; break;
				case 400: this.Value = 300; break;
				case 600: this.Value = 400; break;
			}

			if (dpi != this.Value && valueChangedByUserEvent != null)
				RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region ButtonDpi_SizeChanged()
		private void ButtonDpi_SizeChanged(object sender, SizeChangedEventArgs e)
		{
		}
		#endregion

		#region UserControl_PreviewMouseWheel()
		private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				ScrollDown_Click(null, null);
			}
			else if (e.Delta < 0)
			{
				ScrollUp_Click(null, null);
			}
		}
		#endregion

		#region ApplyFromSettings()
		protected override void ApplyFromSettings()
		{
			if (this.ScanSettings != null && (this.Value != this.ScanSettings.Dpi.Value))
				this.Value = (ushort)this.ScanSettings.Dpi.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null && (this.ScanSettings.Dpi.Value != this.Value))
				this.ScanSettings.Dpi.Value = this.Value;
		}
		#endregion

		#endregion

	}
}
