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

namespace BscanILL.UI.Controls.ScannerControls.Twain
{
	/// <summary>
	/// Interaction logic for BrightnessContrastControl.xaml
	/// </summary>
	public partial class BrightnessContrastControl : ScannerControlBase
	{
		bool	mouseDown = false;

		static DependencyProperty brightnessProperty = DependencyProperty.Register("Brightness", typeof(double), typeof(BrightnessContrastControl), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnBrightValueChanged)));
		static DependencyProperty contrastProperty = DependencyProperty.Register("Contrast", typeof(double), typeof(BrightnessContrastControl), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnContrValueChanged)));
		static DependencyProperty minimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(BrightnessContrastControl), new FrameworkPropertyMetadata(-1.0, new PropertyChangedCallback(OnContrValueChanged)));
		static DependencyProperty maximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(BrightnessContrastControl), new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(OnContrValueChanged)));

		static RoutedEvent brightChangedEvent = EventManager.RegisterRoutedEvent("BrightnessChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BrightnessContrastControl));
		static RoutedEvent brightChangedByUserEvent = EventManager.RegisterRoutedEvent("BrightnessChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BrightnessContrastControl));
		static RoutedEvent contrChangedEvent = EventManager.RegisterRoutedEvent("ContrastChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BrightnessContrastControl));
		static RoutedEvent contrChangedByUserEvent = EventManager.RegisterRoutedEvent("ContrastChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BrightnessContrastControl));


		#region constructor
		public BrightnessContrastControl()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Brightness
		/// <summary>
		/// value is in interval <-1,1>
		/// </summary>
		public double Brightness
		{
			get { return (double)GetValue(BrightnessContrastControl.brightnessProperty); }
			set { SetValue(BrightnessContrastControl.brightnessProperty, Math.Max(this.Minimum, Math.Min(this.Maximum, value))); }
		}
		#endregion

		#region Contrast
		/// <summary>
		/// value is in interval <-1,1>
		/// </summary>
		public double Contrast
		{
			get { return (double)GetValue(BrightnessContrastControl.contrastProperty); }
			set { SetValue(BrightnessContrastControl.contrastProperty, Math.Max(this.Minimum, Math.Min(this.Maximum, value))); }
		}
		#endregion

		#region Minimum
		public double Minimum
		{
			get { return (double)GetValue(BrightnessContrastControl.minimumProperty); }
			set { SetValue(BrightnessContrastControl.minimumProperty, value); }
		}
		#endregion

		#region Maximum
		public double Maximum
		{
			get { return (double)GetValue(BrightnessContrastControl.maximumProperty); }
			set { SetValue(BrightnessContrastControl.maximumProperty, value); }
		}
		#endregion

		#region BrightnessChanged
		public event RoutedEventHandler BrightnessChanged
		{
			add { AddHandler(BrightnessContrastControl.brightChangedEvent, value); }
			remove { RemoveHandler(BrightnessContrastControl.brightChangedEvent, value); }
		}
		#endregion

		#region BrightnessChangedByUser
		public event RoutedEventHandler BrightnessChangedByUser
		{
			add { AddHandler(BrightnessContrastControl.brightChangedByUserEvent, value); }
			remove { RemoveHandler(BrightnessContrastControl.brightChangedByUserEvent, value); }
		}
		#endregion

		#region ContrastChanged
		public event RoutedEventHandler ContrastChanged
		{
			add { AddHandler(BrightnessContrastControl.contrChangedEvent, value); }
			remove { RemoveHandler(BrightnessContrastControl.contrChangedEvent, value); }
		}
		#endregion

		#region ContrastChangedByUser
		public event RoutedEventHandler ContrastChangedByUser
		{
			add { AddHandler(BrightnessContrastControl.contrChangedByUserEvent, value); }
			remove { RemoveHandler(BrightnessContrastControl.contrChangedByUserEvent, value); }
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
					{
						this.scanSettings.Brightness.Changed -= new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
						this.scanSettings.Contrast.Changed -= new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
					}

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.Brightness.Changed += new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
						this.scanSettings.Contrast.Changed += new Scanners.Twain.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Brightness = (double)brightnessProperty.DefaultMetadata.DefaultValue;
			this.Contrast = (double)contrastProperty.DefaultMetadata.DefaultValue;
		}*/
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region UserControl_SizeChanged()
		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateControl();
			
			e.Handled = true;
		}
		#endregion

		#region UpdateControl()
		private void UpdateControl()
		{
			if (this.Maximum - this.Minimum > 0)
			{
				double rectWidth = gridBrightness.ActualWidth - 8;

				int x = Convert.ToInt32(rectWidth * ((this.Brightness - this.Minimum) / (this.Maximum - this.Minimum)));
				this.rectBrightPointer.Margin = new Thickness(x, 0, 0, 0);

				x = Convert.ToInt32(rectWidth * ((this.Contrast - this.Minimum) / (this.Maximum - this.Minimum)));
				this.rectContrPointer.Margin = new Thickness(x, 0, 0, 0);
				
				this.groupBox.Header = string.Format("Brightness {0}, Contrast: {1}", Convert.ToInt32(this.Brightness * 100.0), Convert.ToInt32(this.Contrast * 100.0));
			}
		}
		#endregion

		#region BrightRect_MouseLeftDown()
		private void BrightRect_MouseLeftDown(object sender, MouseButtonEventArgs e)
		{
			if (Maximum - Minimum > 0)
				SetUserValueBright(((e.GetPosition(imageBright).X / imageBright.ActualWidth) * (Maximum - Minimum)) - Maximum);
			else
				SetUserValueBright(Minimum);

			rectBrightPointer.CaptureMouse();
			this.mouseDown = true;
			e.Handled = true;
		}
		#endregion

		#region BrightRect_MouseMove()
		private void BrightRect_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed /*&& this.IsFocused*/ && this.mouseDown)
			{
				if (Maximum - Minimum > 0)
					SetUserValueBright(((e.GetPosition(imageBright).X / imageBright.ActualWidth) * (Maximum - Minimum)) - Maximum);
				else
					SetUserValueBright(Minimum);

				e.Handled = true;
			}
		}	
		#endregion

		#region BrightRect_MouseLeftUp()
		private void BrightRect_MouseLeftUp(object sender, MouseButtonEventArgs e)
		{
			rectBrightPointer.ReleaseMouseCapture();
			this.mouseDown = false;
			e.Handled = true;
		}
		#endregion

		#region ContrRect_MouseLeftDown()
		private void ContrRect_MouseLeftDown(object sender, MouseButtonEventArgs e)
		{
			if (Maximum - Minimum > 0)
				SetUserValueContr(((e.GetPosition(imageContr).X / imageContr.ActualWidth) * (Maximum - Minimum)) - Maximum);
			else
				SetUserValueContr(Minimum);

			rectContrPointer.CaptureMouse();
			this.mouseDown = true;
			e.Handled = true;
		}
		#endregion

		#region ContrRect_MouseMove()
		private void ContrRect_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && this.mouseDown)
			{
				if (Maximum - Minimum > 0)
					SetUserValueContr(((e.GetPosition(imageContr).X / imageContr.ActualWidth) * (Maximum - Minimum)) - Maximum);
				else
					SetUserValueContr(Minimum);

				e.Handled = true;
			}
		}
		#endregion

		#region ContrRect_MouseLeftUp()
		private void ContrRect_MouseLeftUp(object sender, MouseButtonEventArgs e)
		{
			rectContrPointer.ReleaseMouseCapture();
			this.mouseDown = false;
			e.Handled = true;
		}
		#endregion

		#region OnBrightValueChanged()
		private static void OnBrightValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			BrightnessContrastControl control = o as BrightnessContrastControl;

			control.UpdateControl();
			control.ApplyBrightnessToSettings();
			control.RaiseEvent(new RoutedEventArgs(BrightnessContrastControl.brightChangedEvent, control));
		}
		#endregion

		#region OnContrValueChanged()
		private static void OnContrValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			BrightnessContrastControl control = o as BrightnessContrastControl;

			control.UpdateControl();
			control.ApplyContrastToSettings();
			control.RaiseEvent(new RoutedEventArgs(BrightnessContrastControl.contrChangedEvent, control));
		}
		#endregion

		#region Reset_Click()
		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			SetUserValueBright(0);
			SetUserValueContr(0);

			e.Handled = true;
		}
		#endregion

		#region Form_MouseDown
		private void Form_MouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}
		#endregion

		#region SetUserValueBright()
		private void SetUserValueBright(double newValue)
		{
			double oldValue = this.Brightness;
			this.Brightness = newValue;

			if (oldValue != this.Brightness)
				this.RaiseEvent(new RoutedEventArgs(brightChangedByUserEvent, this));
		}
		#endregion

		#region SetUserValueContr()
		private void SetUserValueContr(double newValue)
		{
			double oldValue = this.Contrast;
			this.Contrast = newValue;

			if (oldValue != this.Contrast)
				this.RaiseEvent(new RoutedEventArgs(contrChangedByUserEvent, this));
		}
		#endregion

		#region ApplyFromSettings()
		protected override void ApplyFromSettings()
		{
			if (this.ScanSettings != null)
				this.Brightness = this.ScanSettings.Brightness.Value;

			if (this.ScanSettings != null)
				this.Contrast = this.ScanSettings.Contrast.Value;
		}
		#endregion

		#region ApplyBrightnessToSettings()
		private void ApplyBrightnessToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.Brightness.Value = this.Brightness;
		}
		#endregion

		#region ApplyContrastToSettings()
		private void ApplyContrastToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.Contrast.Value = this.Contrast;
		}
		#endregion

		#endregion

	}
}
