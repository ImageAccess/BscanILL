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

namespace BscanILL.UI.Controls.ScannerControls.S2N
{
	/// <summary>
	/// Interaction logic for SharpenControl.xaml
	/// </summary>
	public partial class SharpenControl : ScannerControlBase
	{
		static DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(short), typeof(SharpenControl), new FrameworkPropertyMetadata((short)0, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SharpenControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SharpenControl));

		GridLengthAnimation row2Animation = new GridLengthAnimation();
		Storyboard			storyboard = new Storyboard();

		ThicknessAnimation	valueChangeAnimation = new ThicknessAnimation();
		Storyboard			valueChangeStoryboard = new Storyboard();

		short min = -7;
		short max = 7;
		short itemsCount = 15;


		#region constructor
		public SharpenControl()
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
		public short Value
		{
			get { return (short)GetValue(SharpenControl.valueProperty); }
			set 
			{
				value = Math.Max(this.min, Math.Min(this.max, value));

				if (this.Value != value)
				{
					SetValue(SharpenControl.valueProperty, value);
				}
			}
		}
		#endregion

		#region ValueChanged
		public event RoutedEventHandler ValueChanged
		{
			add { AddHandler(SharpenControl.valueChangedEvent, value); }
			remove { RemoveHandler(SharpenControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(SharpenControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(SharpenControl.valueChangedByUserEvent, value); }
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
						this.scanSettings.Sharpening.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.Sharpening.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Value = (short)valueProperty.DefaultMetadata.DefaultValue;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region OnInteractiveChanged()
		private static void OnInteractiveChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			SharpenControl instance = o as SharpenControl;

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
	
		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			SharpenControl instance = o as SharpenControl;

			instance.valueChangeAnimation.From = instance.stackPanel.Margin;
			instance.valueChangeAnimation.To = new Thickness(0, -instance.stackPanel.Height * ((instance.max - instance.Value) / (double)instance.itemsCount), 0, 0);

			instance.buttonScrollUp.IsEnabled = (instance.Value < instance.max );
			instance.buttonScrollDown.IsEnabled = (instance.Value > instance.min);
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
			this.stackPanel.Height = this.scrollViewer.ActualHeight * this.itemsCount;
			this.stackPanel.Margin = new Thickness(0, -this.stackPanel.Height * ((this.max - this.Value) / (double)this.itemsCount), 0, 0);
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
			short current = this.Value;
			this.Value = (short)(this.Value + 1);
			
			if(current != this.Value && valueChangedByUserEvent != null)
				RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region ScrollDown_Click()
		private void ScrollDown_Click(object sender, RoutedEventArgs e)
		{
			short current = this.Value;
			this.Value = (short)(this.Value - 1);

			if (current != this.Value && valueChangedByUserEvent != null)
				RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
		}
		#endregion

		#region Item_SizeChanged()
		private void Item_SizeChanged(object sender, SizeChangedEventArgs e)
		{
		}
		#endregion

		#region UserControl_PreviewMouseWheel()
		private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
				ScrollUp_Click(null, null);
			else if(e.Delta < 0)
				ScrollDown_Click(null, null);
		}
		#endregion

		#region ApplyFromSettings()
		protected override void ApplyFromSettings()
		{
			if (this.ScanSettings != null)
				this.Value = (short)this.ScanSettings.Sharpening.Value;
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.Sharpening.Value = this.Value;
		}
		#endregion

		#endregion

	}
}
