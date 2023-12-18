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

namespace BscanILL.UI.Controls.ScannerControls
{
	/// <summary>
	/// Interaction logic for ScannersControl.xaml
	/// </summary>
	public partial class ScannersControl : UserControl, IScannerControl
	{
		public event BscanILL.Misc.VoidHnd S2N_MoreSettingsClick;
		public event BscanILL.Misc.VoidHnd Bookedge_MoreSettingsClick;
		public event BscanILL.Misc.VoidHnd Click_MoreSettingsClick;
		public event BscanILL.Misc.VoidHnd ClickMini_MoreSettingsClick;
		public event BscanILL.Misc.VoidHnd Adf_MoreSettingsClick;

        public event BscanILL.Misc.VoidHnd S2N_SettingsChanged;
        public event Scanners.S2N.ScanRequestHnd S2N_SplittingSettingsChanged;

	
		
		public ScannersControl()
		{
			InitializeComponent();

			this.s2nControl.MoreSettingsClick += delegate()
			{
				if (S2N_MoreSettingsClick != null)
					S2N_MoreSettingsClick();
			};
			this.bookedgeControl.MoreSettingsClick += delegate()
			{
				if (Bookedge_MoreSettingsClick != null)
					Bookedge_MoreSettingsClick();
			};
			this.clickControl.MoreSettingsClick += delegate()
			{
				if (Click_MoreSettingsClick != null)
					Click_MoreSettingsClick();
			};
			this.clickMiniControl.MoreSettingsClick += delegate()
			{
				if (ClickMini_MoreSettingsClick != null)
					ClickMini_MoreSettingsClick();
			};
			this.adfControl.MoreSettingsClick += delegate()
			{
				if (Adf_MoreSettingsClick != null)
					Adf_MoreSettingsClick();
			};

            
            this.s2nControl.ScannerSettingsChanged += delegate()
			{
                if (S2N_SettingsChanged != null)
                    S2N_SettingsChanged();
			};

            this.s2nControl.ScannerSplittingSettingsChanged += delegate(Scanners.S2N.ScannerScanAreaSelection scanArea)
			{
                if (S2N_SplittingSettingsChanged != null)
                    S2N_SplittingSettingsChanged(scanArea);
			};
            
		}

		#region Control_Loaded()
		private void Control_Loaded(object sender, RoutedEventArgs e)
		{
			sm.PrimaryScannerChanged += new Scan.ScannersManager.ScannersChangedHnd(ScannersManager_Changed);
			sm.SecondaryScannerChanged +=new Scan.ScannersManager.ScannersChangedHnd(ScannersManager_Changed);
			sm.SelectedScannerChanged += new Scan.ScannersManager.SelectedScannerChangedHnd(ScannersManager_SelectedScannerChanged);
			
			Update();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public double Brightness
		{
			get 
			{
				IScannerControl control;

				if ((control = this.SelectedControl) != null)
					return control.Brightness;
				else
					return 0;
			}
		}

		public double Contrast
		{
			get
			{
				IScannerControl control;

				if ((control = this.SelectedControl) != null)
					return control.Contrast;
				else
					return 0;
			}
		}

		public ushort Dpi
		{
			get
			{
				IScannerControl control;

				if ((control = this.SelectedControl) != null)
					return control.Dpi;
				else
					return 300;
			}
		}

		public Scanners.ColorMode ColorMode
		{
			get
			{
				IScannerControl control;

				if ((control = this.SelectedControl) != null)
					return control.ColorMode;
				else
					return Scanners.ColorMode.Color;
			}
		}

		public Scanners.FileFormat FileFormat
		{
			get
			{
				IScannerControl control;

				if ((control = this.SelectedControl) != null)
					return control.FileFormat;
				else
					return Scanners.FileFormat.Jpeg;
			}
		}
		
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		BscanILL.Scan.ScannersManager sm { get { return BscanILL.Scan.ScannersManager.Instance; } }

		IScannerControl SelectedControl
		{
			get
			{
				if (sm.SelectedScanner != null)
				{
					if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
						return this.s2nControl;
					else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
						return this.bookedgeControl;
					else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click)
						return this.clickControl;
					else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini)
						return this.clickMiniControl;
					else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
						return this.adfControl;
				}

				return null;
			}
		}

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Update()
		private void Update()
		{			
			if (sm.MultiScannerMode)
			{
				if (mainGrid.Children.Contains(s2nControl))
					mainGrid.Children.Remove(s2nControl);
				if (gridS2N.Children.Contains(s2nControl) == false)
					gridS2N.Children.Add(s2nControl);

				if(sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N || sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
					this.tabItemS2N.Visibility = System.Windows.Visibility.Visible;
				else
					this.tabItemS2N.Visibility =  System.Windows.Visibility.Collapsed;

				if (mainGrid.Children.Contains(bookedgeControl))
					mainGrid.Children.Remove(bookedgeControl);
				if (gridBookedge.Children.Contains(bookedgeControl) == false)
					gridBookedge.Children.Add(bookedgeControl);

				if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge || sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
					this.tabItemBookedge.Visibility = System.Windows.Visibility.Visible;
				else
					this.tabItemBookedge.Visibility = System.Windows.Visibility.Collapsed;

				if (mainGrid.Children.Contains(clickControl))
					mainGrid.Children.Remove(clickControl);
                if (gridClick.Children.Contains(clickControl) == false)
                    gridClick.Children.Add(clickControl);

				if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click || sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click)
					this.tabItemClick.Visibility = System.Windows.Visibility.Visible;
				else
					this.tabItemClick.Visibility = System.Windows.Visibility.Collapsed;

				if (mainGrid.Children.Contains(clickMiniControl))
					mainGrid.Children.Remove(clickMiniControl);
                if (gridClickMini.Children.Contains(clickMiniControl) == false)
                    gridClickMini.Children.Add(clickMiniControl);

				if (sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini || sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini)
					this.tabItemClickMini.Visibility = System.Windows.Visibility.Visible;
				else
					this.tabItemClickMini.Visibility = System.Windows.Visibility.Collapsed;

				if (mainGrid.Children.Contains(adfControl))
					mainGrid.Children.Remove(adfControl);
				if (gridAdf.Children.Contains(adfControl) == false)
					gridAdf.Children.Add(adfControl);

				if(sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf || sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
					this.tabItemAdf.Visibility = System.Windows.Visibility.Visible;
				else
					this.tabItemAdf.Visibility = System.Windows.Visibility.Collapsed;

				if (sm.SelectedScanner != null)
				{
                    if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
                        this.tabItemS2N.IsSelected = true;
                    else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
                        this.tabItemBookedge.IsSelected = true;
                    else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click)
                        this.tabItemClick.IsSelected = true;
                    else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini)
                        this.tabItemClickMini.IsSelected = true;
                    else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
                        this.tabItemAdf.IsSelected = true;
				}
				
				mainGrid.Visibility = System.Windows.Visibility.Hidden;
				tabControl.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				if (sm.SelectedScanner != null)
				{
                    if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
                        ShowSingleScanner(s2nControl);
                    else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
                        ShowSingleScanner(bookedgeControl);
                    else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click)
                        ShowSingleScanner(clickControl);
                    else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini)
                        ShowSingleScanner(clickMiniControl);
                    else if (sm.SelectedScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
                        ShowSingleScanner(adfControl);
                    else
                        ShowSingleScanner(null);
				}
				else
					ShowSingleScanner(null);
			}
		}
		#endregion

		#region ScannersManager_SelectedScannerChanged()
		void ScannersManager_SelectedScannerChanged(bool isPrimaryScannerSelected)
		{
			if (this.Dispatcher.CheckAccess())
				Update();
			else
				this.Dispatcher.BeginInvoke((Action)delegate() { Update(); });
		}
		#endregion

		#region ScannersManager_Changed()
		void ScannersManager_Changed()
		{
			if (this.Dispatcher.CheckAccess())
				Update();
			else
				this.Dispatcher.BeginInvoke((Action)delegate() { Update(); });
		}
		#endregion

		#region ShowSingleScanner()
		void ShowSingleScanner(UserControl control)
		{
			for (int i = this.mainGrid.Children.Count - 1; i >= 0; i--)
				if (this.mainGrid.Children[i] != control)
					this.mainGrid.Children.RemoveAt(i);

			if (control != null && this.mainGrid.Children.Contains(control) == false)
			{
				if (control.Parent != null)
					((Grid)control.Parent).Children.Remove(control);
				
				this.mainGrid.Children.Add(control);
			}

			this.mainGrid.Visibility = System.Windows.Visibility.Visible;
			this.tabControl.Visibility = System.Windows.Visibility.Collapsed;
		}
		#endregion

		#region TabItemSelectionChanged()
		private void TabItemSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sm.MultiScannerMode)
			{
				if (tabControl.SelectedItem == tabItemS2N)
				{
					if (sm.PrimaryScanner != null && sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
						sm.IsPrimaryScannerSelected = true;
					else if (sm.SecondaryScanner != null && sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerGroup == Scanners.MODELS.ScannerGroup.S2N)
						sm.IsPrimaryScannerSelected = false;
				}
				else if (tabControl.SelectedItem == tabItemBookedge)
				{
					if (sm.PrimaryScanner != null && sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
						sm.IsPrimaryScannerSelected = true;
					else if (sm.SecondaryScanner != null && sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.BookEdge)
						sm.IsPrimaryScannerSelected = false;
				}
				else if (tabControl.SelectedItem == tabItemAdf)
				{
					if (sm.PrimaryScanner != null && sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
						sm.IsPrimaryScannerSelected = true;
					else if (sm.SecondaryScanner != null && sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.TwainAdf)
						sm.IsPrimaryScannerSelected = false;
				}
                else if (tabControl.SelectedItem == tabItemClickMini)
                {
                    if (sm.PrimaryScanner != null && sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini)
                        sm.IsPrimaryScannerSelected = true;
                    else if (sm.SecondaryScanner != null && sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.ClickMini)
                        sm.IsPrimaryScannerSelected = false;
                }
                else if (tabControl.SelectedItem == tabItemClick)
                {
                    if (sm.PrimaryScanner != null && sm.PrimaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click)
                        sm.IsPrimaryScannerSelected = true;
                    else if (sm.SecondaryScanner != null && sm.SecondaryScanner.DeviceInfo.ScannerModel.ScannerSubGroup == Scanners.MODELS.ScannerSubGroup.Click)
                        sm.IsPrimaryScannerSelected = false;
                }
			}
		}
		#endregion

		#endregion

	}
}
