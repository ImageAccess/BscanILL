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
	/// Interaction logic for DocModeControl.xaml
	/// </summary>
	public partial class DocModeControl : ScannerControlBase
	{		
		static DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(Scanners.S2N.DocumentMode), typeof(DocModeControl),
					new FrameworkPropertyMetadata(Scanners.S2N.DocumentMode.Flat, new PropertyChangedCallback(OnValueChanged)));

		static RoutedEvent valueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DocModeControl));
		static RoutedEvent valueChangedByUserEvent = EventManager.RegisterRoutedEvent("ValueChangedByUser", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DocModeControl));

        public event Scanners.S2N.DocModeSettingHnd DocModeSettingsChanged;        

		#region constructor
		public DocModeControl()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.DocumentMode Value
		{
			get { return (Scanners.S2N.DocumentMode)GetValue(DocModeControl.valueProperty); }
			set { SetValue(DocModeControl.valueProperty, value); }
		}
		#endregion

		#region ValueChanged
		private event RoutedEventHandler ValueChanged
		{
			add { AddHandler(DocModeControl.valueChangedEvent, value); }
			remove { RemoveHandler(DocModeControl.valueChangedEvent, value); }
		}
		#endregion

		#region ValueChangedByUser
		public event RoutedEventHandler ValueChangedByUser
		{
			add { AddHandler(DocModeControl.valueChangedByUserEvent, value); }
			remove { RemoveHandler(DocModeControl.valueChangedByUserEvent, value); }
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
						this.scanSettings.DocMode.Changed -= new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);

					this.scanSettings = value;

					if (this.scanSettings != null)
					{
						this.scanSettings.DocMode.Changed += new Scanners.S2N.Settings.SettingChangedHnd(ScanSettings_ValueChanged);
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
			this.Value = Scanners.S2N.DocumentMode.G4;
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Radio_Checked()
		private void Radio_Checked(object sender, RoutedEventArgs e)
		{
			Scanners.S2N.DocumentMode previous = this.Value;

			if (this.IsLoaded)
			{
				if (sender == this.radioFlat)
					this.Value = Scanners.S2N.DocumentMode.Flat;
				else if (sender == this.radioBook)
					this.Value = Scanners.S2N.DocumentMode.Book;
				else if (sender == this.radioFolder)
					this.Value = Scanners.S2N.DocumentMode.Folder;
				else if (sender == this.radioFixedFocus)
					this.Value = Scanners.S2N.DocumentMode.FixedFocus;
				else if (sender == this.radioGlassPlate)
					this.Value = Scanners.S2N.DocumentMode.GlassPlate;
				else if (sender == this.radioAuto)
					this.Value = Scanners.S2N.DocumentMode.Auto;
				else if (sender == this.radioV)
					this.Value = Scanners.S2N.DocumentMode.V;
				else if (sender == this.radioBookGlass)
					this.Value = Scanners.S2N.DocumentMode.BookGlassPlate;
			}

			e.Handled = true;

            if (this.Value != previous)
            {
                this.RaiseEvent(new RoutedEventArgs(valueChangedByUserEvent, this));
                if (DocModeSettingsChanged != null)
                {
                    DocModeSettingsChanged(this.Value);
                }
            }
		}
		#endregion

		#region OnValueChanged()
		private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			DocModeControl instance = o as DocModeControl;

			switch (instance.Value)
			{
				case Scanners.S2N.DocumentMode.Flat: instance.radioFlat.IsChecked = true; break;
				case Scanners.S2N.DocumentMode.Book: instance.radioBook.IsChecked = true; break;
				case Scanners.S2N.DocumentMode.Folder: instance.radioFolder.IsChecked = true; break;
				case Scanners.S2N.DocumentMode.FixedFocus: instance.radioFixedFocus.IsChecked = true; break;
				case Scanners.S2N.DocumentMode.GlassPlate: instance.radioGlassPlate.IsChecked = true; break;
				case Scanners.S2N.DocumentMode.Auto: instance.radioAuto.IsChecked = true; break;
				case Scanners.S2N.DocumentMode.V: instance.radioV.IsChecked = true; break;
				case Scanners.S2N.DocumentMode.BookGlassPlate: instance.radioBookGlass.IsChecked = true; break;
			}

			instance.ApplyToSettings();
			instance.RaiseEvent(new RoutedEventArgs(DocModeControl.valueChangedEvent, instance));
		}
		#endregion

		#region Radio_PreviewMouseDown()
		private void Radio_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
		}
		#endregion

		#region ApplyFromSettings()
		/*
		Flat = 0,
		Book = 1,
		Folder = 2,
		FixedFocus = 3,
		GlassPlate = 4,

		/// <summary>
		///  auto detection to decide, if cradle is in flat or book mode - for scanners with adjustable flat-V cradle
		/// </summary>
		Auto = 5,
		V = 6
		BookGlassPlate = 7*/
		protected override void ApplyFromSettings()
		{
			if (this.ScanSettings != null)
			{
				List<Scanners.S2N.DocumentMode> list = this.ScanSettings.DocMode.AvailableValues;
				int availableButtons = 0;

				if (list.Contains(Scanners.S2N.DocumentMode.Flat))
				{
					radioFlat.Visibility = System.Windows.Visibility.Visible;
					radioFlat.SetValue(Grid.RowProperty, availableButtons / 2);
					radioFlat.SetValue(Grid.ColumnProperty, availableButtons % 2);
					
					availableButtons++;
				}
				else
					radioFlat.Visibility = System.Windows.Visibility.Collapsed;

				if (list.Contains(Scanners.S2N.DocumentMode.Book))
				{
					radioBook.Visibility = System.Windows.Visibility.Visible;
					radioBook.SetValue(Grid.RowProperty, availableButtons / 2);
					radioBook.SetValue(Grid.ColumnProperty, availableButtons % 2);

					availableButtons++;
				}
				else
					radioBook.Visibility = System.Windows.Visibility.Collapsed;

				if (list.Contains(Scanners.S2N.DocumentMode.Folder))
				{
					radioFolder.Visibility = System.Windows.Visibility.Visible;
					radioFolder.SetValue(Grid.RowProperty, availableButtons / 2);
					radioFolder.SetValue(Grid.ColumnProperty, availableButtons % 2);

					availableButtons++;
				}
				else
					radioFolder.Visibility = System.Windows.Visibility.Collapsed;

				if (list.Contains(Scanners.S2N.DocumentMode.FixedFocus))
				{
					radioFixedFocus.Visibility = System.Windows.Visibility.Visible;
					radioFixedFocus.SetValue(Grid.RowProperty, availableButtons / 2);
					radioFixedFocus.SetValue(Grid.ColumnProperty, availableButtons % 2);

					availableButtons++;
				}
				else
					radioFixedFocus.Visibility = System.Windows.Visibility.Collapsed;

				if (list.Contains(Scanners.S2N.DocumentMode.GlassPlate))
				{
					radioGlassPlate.Visibility = System.Windows.Visibility.Visible;
					radioGlassPlate.SetValue(Grid.RowProperty, availableButtons / 2);
					radioGlassPlate.SetValue(Grid.ColumnProperty, availableButtons % 2);

					availableButtons++;
				}
				else
					radioGlassPlate.Visibility = System.Windows.Visibility.Collapsed;

				if (list.Contains(Scanners.S2N.DocumentMode.Auto))
				{
					radioAuto.Visibility = System.Windows.Visibility.Visible;
					radioAuto.SetValue(Grid.RowProperty, availableButtons / 2);
					radioAuto.SetValue(Grid.ColumnProperty, availableButtons % 2);

					availableButtons++;
				}
				else
					radioAuto.Visibility = System.Windows.Visibility.Collapsed;

				if (list.Contains(Scanners.S2N.DocumentMode.V))
				{
					radioV.Visibility = System.Windows.Visibility.Visible;
					radioV.SetValue(Grid.RowProperty, availableButtons / 2);
					radioV.SetValue(Grid.ColumnProperty, availableButtons % 2);

					availableButtons++;
				}
				else
					radioV.Visibility = System.Windows.Visibility.Collapsed;

				if (list.Contains(Scanners.S2N.DocumentMode.BookGlassPlate))
				{
					radioBookGlass.Visibility = System.Windows.Visibility.Visible;
					radioBookGlass.SetValue(Grid.RowProperty, availableButtons / 2);
					radioBookGlass.SetValue(Grid.ColumnProperty, availableButtons % 2);

					availableButtons++;
				}
				else
					radioBookGlass.Visibility = System.Windows.Visibility.Collapsed;


				row1.Height = (availableButtons > 2) ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
				row2.Height = (availableButtons > 4) ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
				row3.Height = (availableButtons > 6) ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

				this.Height = 20 + ((availableButtons + 1) / 2) * 31;
				this.Value = this.ScanSettings.DocMode.Value;

                if (DocModeSettingsChanged != null)
                {
                    DocModeSettingsChanged(this.Value);
                }
			}
		}
		#endregion

		#region ApplyToSettings()
		protected override void ApplyToSettings()
		{
			if (this.ScanSettings != null)
				this.ScanSettings.DocMode.Value = this.Value;
		}
		#endregion

		#endregion
	}
}
