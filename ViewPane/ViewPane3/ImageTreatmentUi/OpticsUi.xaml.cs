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
using BIP.Geometry;
using ViewPane.ImagePanel;


namespace ViewPane.ImageTreatmentUi
{
	/// <summary>
	/// Interaction logic for OpticsUi.xaml
	/// </summary>
	internal partial class OpticsUi : UserControl
	{
		ImageUi										imageUi;
		ImageProcessing.IpSettings.ItImage			itImage = null;
		List<ImageProcessing.IpSettings.ItImage>	affectedImages = new List<ImageProcessing.IpSettings.ItImage>();


		public OpticsUi()
		{
			InitializeComponent();
		}

		//PUBLIC PROPERTIES
		#region internal properties
		ImageUi		ImageUi { get { return this.imageUi; } }
		ImagePane	ImagePane { get { return this.imageUi.ImagePane; } }
		#endregion

		//PRIVATE PROPERTIES
		#region private properties
		private ViewPane.ToolbarSelection ToolbarSelection { get { return this.ImagePane.ToolbarSelection; } }
		#endregion

		//PUBLIC METHODS
		#region internal methods

		#region Init()
		public void Init(ImageUi imageUi)
		{
			this.imageUi = imageUi;
			this.imageUi.ImagePane.ZoomModeChanged += new ImagePane.ZoomModeChangedHnd(ImagePane_ZoomModeChanged);
			this.imageUi.ImagePane.ZoomChanged += new ImagePane.ZoomChangedHnd(ImagePane_ZoomChanged);
		}
		#endregion

		#region Synchronize()
		internal void Synchronize(ImageProcessing.IpSettings.ItImage itImage)
		{
			if (this.itImage != null && this.itImage != itImage)
				this.itImage.Changed -= new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);

			this.itImage = itImage;

			if (this.itImage != null)
				this.itImage.Changed += new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);

			RefreshUi();
		}

		void itImage_Changed(ImageProcessing.IpSettings.ItImage itImage, ImageProcessing.IpSettings.ItProperty type)
		{
			throw new NotImplementedException();
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region ImagePane_ZoomModeChanged()
		void ImagePane_ZoomModeChanged(ViewPane.ToolbarSelection oldZoomMode, ViewPane.ToolbarSelection newZoomMode)
		{
			RefreshUi();
		}
		#endregion

		#region ImagePane_ZoomChanged()
		void ImagePane_ZoomChanged(double newZoom)
		{
			RefreshUi();
		}
		#endregion

		#region Mouse_LeftButtonDown()
		private void Mouse_LeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
			{
				RatioPoint mousePoint = this.ImageUi.PanelToImage(e);

				SelectAffectedImages();
				((UIElement)sender).CaptureMouse();
				e.Handled = true;
			}
		}
		#endregion

		#region Mouse_Move()
		private void Mouse_Move(object sender, MouseEventArgs e)
		{
			if (ToolbarSelection == ViewPane.ToolbarSelection.Bookfold && e.LeftButton == MouseButtonState.Pressed)
			{
				if (this.itImage != null)
				{
					RatioPoint p = this.ImageUi.PanelToImage(e);

					foreach(ImageProcessing.IpSettings.ItImage affectedImage in this.affectedImages)
						affectedImage.OpticsCenter = Math.Max(0, Math.Min(1, p.Y));

					this.ImagePane.InvalidateVisual();
				}
					
				e.Handled = true;
			}
		}
		#endregion

		#region Mouse_LeftButtonUp()
		private void Mouse_LeftButtonUp(object sender, MouseButtonEventArgs e)
		{	
			if (ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
			{
				ReleaseAffectedImages();
				Mouse.Capture(null);
				e.Handled = true;
			}
		}
		#endregion

		#region ItImage_Changed()
		private void ItImage_Changed(ImageProcessing.IpSettings.ItImage itImage, ImageProcessing.IpSettings.ItProperty type)
		{
			if ((type & ImageProcessing.IpSettings.ItProperty.ImageSettings) > 0)
			{
				this.Dispatcher.Invoke((Action)delegate() { ItImage_ChangedTU(itImage); });
			}
		}
		#endregion

		#region ItImage_ChangedTU()
		private void ItImage_ChangedTU(ImageProcessing.IpSettings.ItImage itImage)
		{
			RefreshUi();
		}
		#endregion

		#region RefreshUi()
		internal void RefreshUi()
		{
			if (this.itImage != null && this.itImage.IsFixed == false && this.ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
			{
				this.Visibility = Visibility.Visible;
				this.Margin = new Thickness(0, this.itImage.OpticsCenter * this.imageUi.CurrentHeight - 9, 0, 0);
			}
			else
			{
				this.Visibility = Visibility.Hidden;
			}
		}
		#endregion

		#region SelectAffectedImages()
		void SelectAffectedImages()
		{
			ReleaseAffectedImages();

			if (this.ImagePane.ItImages != null && this.itImage != null && this.itImage.IsFixed == false)
			{
				this.affectedImages.Add(this.itImage);

				if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
				{
					ImageProcessing.IpSettings.ItImages itImages = this.ImagePane.ItImages;
					int imageIndex = itImages.IndexOf(this.itImage);

					for (int i = imageIndex + 1; i < itImages.Count; i++)
						affectedImages.Add(itImages[i]);
				}
			}
		}
		#endregion

		#region ReleaseAffectedImages()
		void ReleaseAffectedImages()
		{
			affectedImages.Clear();
		}
		#endregion
	
		#endregion

	}
}
