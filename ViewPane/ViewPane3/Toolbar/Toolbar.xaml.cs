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
using ViewPane.ImagePanel;

namespace ViewPane.Toolbar
{
	/// <summary>
	/// Interaction logic for Toolbar.xaml
	/// </summary>
	public partial class Toolbar : UserControl
	{
		public static DependencyProperty IsExpandedProperty;
		public static DependencyProperty IsPinnedProperty;

		ViewPane.ToolbarSelection toolbarSelection = ViewPane.ToolbarSelection.Move;

		internal event ViewPane.ImagePanel.ImagePane.ZoomModeChangedHnd ZoomModeChanged;


		#region constructor
		public Toolbar()
		{
			InitializeComponent();

			if (IsExpandedProperty == null)
			{
				IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Toolbar),
					new PropertyMetadata(true, new PropertyChangedCallback(OnExpanded_Changed)));
				IsPinnedProperty = DependencyProperty.Register("IsPinned", typeof(bool), typeof(Toolbar),
					new PropertyMetadata(true, new PropertyChangedCallback(OnPinned_Changed)));
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		internal ImagePane ImagePane { get { return (ImagePane)this.Parent; } }

		internal ViewPane.Toolbar.ToolbarZoomMode ToolbarZoomMode { get { return this.toolbarZoomMode; } }
		internal ViewPane.Toolbar.ToolbarZoomSize ToolbarZoomSize { get { return this.toolbarZoomSize; } }
		internal ViewPane.Toolbar.ToolbarItSettings ToolbarItSettings { get { return this.toolbarItSettings; } }
		internal ViewPane.Toolbar.ToolbarItTransforms ToolbarItTransforms { get { return this.toolbarItTransforms; } }
		internal ViewPane.Toolbar.ToolbarNavigation ToolbarNavigation { get { return this.toolbarNavigation; } }
		internal ViewPane.Toolbar.ToolbarPages ToolbarPages { get { return this.toolbarPages; } }
		internal ViewPane.Toolbar.ToolbarScan ToolbarScan { get { return this.toolbarScan; } }
		internal ViewPane.Toolbar.ToolbarTreatment ToolbarTreatment { get { return this.toolbarTreatment; } }
		internal ViewPane.Toolbar.ToolbarBookInfo ToolbarBookInfo { get { return this.toolbarBookInfo; } }

		#region Visible
		public bool Visible
		{
			get { return this.Visibility == Visibility.Visible; }
			set { if (value) this.Visibility = Visibility.Visible; else this.Visibility = Visibility.Collapsed; }
		}
		#endregion

		#region IsExpanded
		public bool IsExpanded
		{
			get { return (bool)GetValue(IsExpandedProperty); }
			set { SetValue(IsExpandedProperty, value); }
		}
		#endregion

		#region IsPinned
		public bool IsPinned
		{
			get { return (bool)GetValue(IsPinnedProperty); }
			set { SetValue(IsPinnedProperty, value); }
		}
		#endregion

		#region ToolbarSelection
		internal ViewPane.ToolbarSelection ToolbarSelection
		{
			get { return this.toolbarSelection; }
			set
			{
				if (this.toolbarSelection != value)
				{
					ViewPane.ToolbarSelection oldMode = this.toolbarSelection;

					this.toolbarSelection = value;
					this.toolbarZoomMode.ToolbarSelection = value;
					this.toolbarItTransforms.ToolbarSelection = value;

					if (ZoomModeChanged != null)
						ZoomModeChanged(oldMode, this.toolbarSelection);
				}
			}
		}
		#endregion

		#region ZoomType
		internal ViewPane.ZoomType ZoomType
		{
			get { return this.toolbarZoomSize.ZoomType; }
			set { this.toolbarZoomSize.ZoomType = value; }
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Init()
		internal void Init(IImagePane imagePane, bool zoomModeVisible, bool zoomSizeVisible, bool itSettingsVisible, bool itTransformsVisible,
			bool scanVisible, bool navigationVisible, bool pagesVisible, bool treatmentVisible, bool bookInfoVisible)
		{
			this.toolbarZoomMode.Init(imagePane, zoomModeVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
			this.toolbarZoomSize.Init(imagePane, zoomSizeVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
			this.toolbarItSettings.Init(imagePane, itSettingsVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
			this.toolbarItTransforms.Init(imagePane, itTransformsVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
			this.toolbarScan.Init(imagePane, scanVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
			this.toolbarNavigation.Init(imagePane, navigationVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
			this.toolbarPages.Init(imagePane, pagesVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
			this.toolbarTreatment.Init(imagePane, treatmentVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
			this.toolbarBookInfo.Init(imagePane, bookInfoVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region OnExpanded_Changed()
		private void OnExpanded_Changed(object sender, DependencyPropertyChangedEventArgs args)
		{
			Toolbar instance = sender as Toolbar;

			DoubleAnimation animation = new DoubleAnimation();
			animation.Duration = new Duration(TimeSpan.Parse("00:00:00.3"));
			animation.AutoReverse = false;
			animation.FillBehavior = FillBehavior.HoldEnd;

			if (instance.IsExpanded)
				animation.To = 31 + 7;
			else
				animation.To = 7;

			instance.BeginAnimation(Toolbar.HeightProperty, animation);
		}
		#endregion

		#region OnPinned_Changed()
		private void OnPinned_Changed(object sender, DependencyPropertyChangedEventArgs args)
		{
			Toolbar instance = sender as Toolbar;

			if (instance.IsPinned)
			{
				instance.IsExpanded = true;
				instance.pinRotation.Angle = 0;
			}
			else
			{
				instance.IsExpanded = instance.IsMouseOver;
				instance.pinRotation.Angle = 90;
			}
		}
		#endregion

		#region Pin_Click()
		private void Pin_Click(object sender, RoutedEventArgs e)
		{
			this.IsPinned = !this.IsPinned;
		}
		#endregion

		#region Form_PreviewMouseMove()
		private void Form_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			this.IsExpanded = true;
		}
		#endregion

		#region Form_MouseLeave()
		private void Form_MouseLeave(object sender, MouseEventArgs e)
		{
			this.IsExpanded = this.IsPinned;
		}
		#endregion

		#region StackToolbars_SizeChanged()
		private void StackToolbars_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.Width = this.stackToolbars.ActualWidth + 18;
		}
		#endregion

		#endregion

	}
}
