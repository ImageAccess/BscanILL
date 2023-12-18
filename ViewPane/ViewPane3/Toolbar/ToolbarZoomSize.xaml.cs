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

namespace ViewPane.Toolbar
{
	/// <summary>
	/// Interaction logic for ToolbarZoomSize.xaml
	/// </summary>
	internal partial class ToolbarZoomSize : ToolbarBase
	{
		public delegate void ZoomSizeEventHandler(object sender, ZoomSizeEventArgs args);
		public event ZoomSizeEventHandler ZoomTypeChanged;
		public event EventHandler ZoomInRequest;
		public event EventHandler ZoomOutRequest;

		ViewPane.ZoomType zoomType = ViewPane.ZoomType.FitImage;


		#region constructor
		public ToolbarZoomSize()
		{
			InitializeComponent();
		}
		#endregion


		#region class ZoomSizeEventArgs
		internal class ZoomSizeEventArgs : EventArgs
		{
			ViewPane.ZoomType zoomType;
			float zoomValue;

			public ZoomSizeEventArgs(ViewPane.ZoomType zoomType, float zoomValue)
			{
				this.zoomType = zoomType;
				this.zoomValue = zoomValue;
			}

			public ViewPane.ZoomType ZoomType { get { return zoomType; } }
			public float ZoomValue { get { return zoomValue; } }
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties

		public ViewPane.ZoomType ZoomType
		{
			get { return zoomType; }
			set
			{
				if (zoomType != value)
				{
					zoomType = value;
					AdjustUi(zoomType);
				}
			}
		}

		#endregion

		//PUBLIC METHODS
		#region public methods

		#region AdjustUi()
		public void AdjustUi(ViewPane.ZoomType type, double zoomValue)
		{
			this.zoomType = type;
			this.buttonActualSize.IsChecked = (type == ViewPane.ZoomType.ActualSize);
			this.buttonFitImage.IsChecked = (type == ViewPane.ZoomType.FitImage);
			this.buttonFitWidth.IsChecked = (type == ViewPane.ZoomType.FitWidth);

			textZoom.Text = string.Format("{0}%", Math.Round(zoomValue * 100, 2));

			menu1600.IsChecked = (zoomValue == 16F);
			menu800.IsChecked = (zoomValue == 8F);
			menu400.IsChecked = (zoomValue == 4F);
			menu200.IsChecked = (zoomValue == 2F);
			menu150.IsChecked = (zoomValue == 1.5F);
			menu100.IsChecked = (zoomValue == 1F);
			menu50.IsChecked = (zoomValue == .5F);
			menu25.IsChecked = (zoomValue == .25F);
			menu12.IsChecked = (zoomValue == .125F);
			menuActSize.IsChecked = (type == ViewPane.ZoomType.ActualSize);
			menuFitImage.IsChecked = (type == ViewPane.ZoomType.FitImage);
			menuFitWidth.IsChecked = (type == ViewPane.ZoomType.FitWidth);
		}

		public void AdjustUi(ViewPane.ZoomType type)
		{
			this.zoomType = type;
			this.buttonActualSize.IsChecked = (type == ViewPane.ZoomType.ActualSize);
			this.buttonFitImage.IsChecked = (type == ViewPane.ZoomType.FitImage);
			this.buttonFitWidth.IsChecked = (type == ViewPane.ZoomType.FitWidth);

			menuActSize.IsChecked = (type == ViewPane.ZoomType.ActualSize);
			menuFitImage.IsChecked = (type == ViewPane.ZoomType.FitImage);
			menuFitWidth.IsChecked = (type == ViewPane.ZoomType.FitWidth);
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region RadioSize_Checked
		void RadioSize_Checked(object sender, RoutedEventArgs e)
		{
			ViewPane.ZoomType zt = this.zoomType;
			
			if(sender == buttonActualSize)
				zt = ViewPane.ZoomType.ActualSize;
			else if(sender == buttonFitImage)
				zt = ViewPane.ZoomType.FitImage;
			else if(sender == buttonFitWidth)
				zt = ViewPane.ZoomType.FitWidth;

			if (zoomType != zt)
			{
				zoomType = zt;
				AdjustUi(zoomType);

				if (ZoomTypeChanged != null)
					ZoomTypeChanged(this, new ZoomSizeEventArgs(zoomType, 1.0F));
			}
		}
		#endregion

		#region ZoomButton_Click()
		private void ZoomButton_Click(object sender, RoutedEventArgs e)
		{
			this.ZoomType = ViewPane.ZoomType.Value;

			if (sender == buttonZoomIn && this.ZoomInRequest != null)
				this.ZoomInRequest(this, null);
			else if (sender == buttonZoomOut && this.ZoomOutRequest != null)
				this.ZoomOutRequest(this, null);
		}
		#endregion

		#region Menu_Click()
		private void Menu_Click(object sender, RoutedEventArgs e)
		{
			float newZoom = 0F;
			ViewPane.ZoomType type = ViewPane.ZoomType.Value;

			switch (contextMenu.Items.IndexOf(sender))
			{
				case 0: newZoom = 16F; break;
				case 1: newZoom = 8F; break;
				case 2: newZoom = 4F; break;
				case 3: newZoom = 2F; break;
				case 4: newZoom = 1.5F; break;
				case 5: newZoom = 1F; break;
				case 6: newZoom = .5F; break;
				case 7: newZoom = .25F; break;
				case 8: newZoom = .125F; break;

				case 10: type = ViewPane.ZoomType.ActualSize; break;
				case 11: type = ViewPane.ZoomType.FitImage; break;
				case 12: type = ViewPane.ZoomType.FitWidth; break;
			}

			if ((newZoom != 0) || (type != ViewPane.ZoomType.Value))
			{
				zoomType = type;
				AdjustUi(zoomType, newZoom);

				if (ZoomTypeChanged != null)
					ZoomTypeChanged(this, new ZoomSizeEventArgs(zoomType, newZoom));
			}
		}
		#endregion

		#region ToolbarZoomSize_Click()
		void ToolbarZoomSize_Click(object sender, RoutedEventArgs e)
		{
			contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
			contextMenu.PlacementTarget = (ToolbarButton)buttonZoomMenu;
			contextMenu.PlacementRectangle = new Rect(0, 0, 0, 0);
			contextMenu.IsOpen = true;
		}
		#endregion

		#endregion


	}
}
