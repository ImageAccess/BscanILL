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
	/// Interaction logic for FingersUi.xaml
	/// </summary>
	internal partial class FingersUi : UserControl
	{
		PageUi		pageUi;
		ImageProcessing.IpSettings.ItPage		itPage;

		ImageProcessing.IpSettings.Finger.FingerHnd	dlgFingerAdded;
		ImageProcessing.IpSettings.Finger.FingerHnd	dlgFingerRemoving;
		ImageProcessing.IpSettings.Finger.VoidHnd		dlgFingersClearing;

		public FingersUi()
		{
			InitializeComponent();

			this.dlgFingerAdded = new ImageProcessing.IpSettings.Finger.FingerHnd(FingerAddedTU);
			this.dlgFingerRemoving = new ImageProcessing.IpSettings.Finger.FingerHnd(FingerRemovingTU);
			this.dlgFingersClearing = new ImageProcessing.IpSettings.Finger.VoidHnd(FingersClearingTU);
		}
		
		//PUBLIC PROPERTIES
		#region internal properties

		internal ImageProcessing.IpSettings.ItPage		Page { get { return this.pageUi.Page; } }
		internal ImageUi	ImageUi { get { return this.pageUi.ImageUi; } }
		internal ImagePane	ImagePane { get { return this.pageUi.ImagePane; } }
		internal double		CurrentWidth { get { return pageUi.CurrentWidth; } }
		internal double		CurrentHeight { get { return pageUi.CurrentHeight; } }

		#endregion

		//PRIVATE PROPERTIES
		#region private properties
		private ViewPane.ToolbarSelection ToolbarSelection { get { return this.pageUi.ImagePane.ToolbarSelection; } }
		#endregion

		//PUBLIC METHODS
		#region internal methods

		#region Init()
		internal void Init(PageUi pageUi)
		{
			this.pageUi = pageUi;

			this.pageUi.ImagePane.ZoomChanged += new ImagePane.ZoomChangedHnd(ImagePane_ZoomChanged);
			this.pageUi.ImagePane.ZoomModeChanged += new ImagePane.ZoomModeChangedHnd(ImagePane_ZoomModeChanged);
		}
		#endregion

		#region Synchronize()
		internal void Synchronize(ImageProcessing.IpSettings.ItPage itPage)
		{
			if (this.itPage != itPage)
			{
				if (this.itPage != null)
				{
					this.itPage.Fingers.FingerAdded -= new ImageProcessing.IpSettings.Finger.FingerHnd(Fingers_FingerAdded);
					this.itPage.Fingers.FingerRemoving -= new ImageProcessing.IpSettings.Finger.FingerHnd(Fingers_FingerRemoving);
					this.itPage.Fingers.Clearing -= new ImageProcessing.IpSettings.Finger.VoidHnd(Fingers_Clearing);
				}

				this.itPage = itPage;

				if (this.itPage != null && this.itPage.ItImage.IsFixed == false)
				{
					this.itPage.Fingers.FingerAdded += new ImageProcessing.IpSettings.Finger.FingerHnd(Fingers_FingerAdded);
					this.itPage.Fingers.FingerRemoving += new ImageProcessing.IpSettings.Finger.FingerHnd(Fingers_FingerRemoving);
					this.itPage.Fingers.Clearing += new ImageProcessing.IpSettings.Finger.VoidHnd(Fingers_Clearing);
				}

				if (this.itPage != null && this.itPage.ItImage.IsFixed == false)
				{
					foreach (ImageProcessing.IpSettings.Finger finger in this.Page.Fingers)
					{
						FingerUi fingerUi = GetFingerUi(finger);

						if (fingerUi == null)
						{
							fingerUi = new FingerUi(this, finger);
							this.mainGrid.Children.Add(fingerUi);
						}
					}

					for (int i = this.mainGrid.Children.Count - 1; i >= 0; i--)
					{
						FingerUi fingerUi = this.mainGrid.Children[i] as FingerUi;

						if (this.Page.Fingers.Contains(fingerUi.Finger) == false)
							this.mainGrid.Children.Remove(fingerUi);
					}
				}
				else
				{
					this.mainGrid.Children.Clear();
				}
			}

			RefreshUi();
		}
		#endregion
	
		#region Mouse_LeftButtonDown()
		public void Mouse_LeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (ToolbarSelection == ViewPane.ToolbarSelection.FingerRemoval && pageUi != null)
			{
				RatioPoint imagePoint = pageUi.PanelToImage(e);
				RatioPoint pointNotSkewed = pageUi.Page.Clip.TransferSkewedToUnskewedPoint(imagePoint);

				ImageProcessing.IpSettings.Finger finger = ImageProcessing.IpSettings.Finger.GetFinger(this.Page, new RatioRect(pointNotSkewed.X, pointNotSkewed.Y, 0, 0), true);
				Page.Fingers.Add(finger);

				if (finger != null)
				{
					FingerUi fingerUi = GetFingerUi(finger);

					if (fingerUi != null)
					{
						//FingerUi fingerUi = new FingerUi(this, finger);
						fingerUi.CaptureMouse();
						//this.mainGrid.Children.Add(fingerUi);
						fingerUi.DragStarted(pointNotSkewed);
					}
					else
					{
					}
				}

				e.Handled = true;
			}
		}
		#endregion

		#region MainGrid_MouseMove()
		public void MainGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.itPage != null)
			{
				this.Cursor = Cursors.Cross;
			}
			else
			{
				this.Cursor = null;
			}
		}
		#endregion

		#region PanelToImage()
		internal RatioPoint PanelToImage(MouseEventArgs e)
		{
			return this.pageUi.PanelToImage(e);
		}
		#endregion
	
		#endregion

		//PRIVATE METHODS
		#region private methods

		#region GetFingerUi()
		FingerUi GetFingerUi(ImageProcessing.IpSettings.Finger finger)
		{
			foreach (FingerUi fingerUi in this.mainGrid.Children)
				if (fingerUi.Finger == finger)
					return fingerUi;
			
			return null;
		}
		#endregion

		#region Fingers_FingerAdded()
		void Fingers_FingerAdded(ImageProcessing.IpSettings.Finger finger)
		{
			this.Dispatcher.Invoke(this.dlgFingerAdded, finger);
		}
		#endregion

		#region Fingers_FingerRemoving()
		void Fingers_FingerRemoving(ImageProcessing.IpSettings.Finger finger)
		{
			this.Dispatcher.Invoke(this.dlgFingerRemoving, finger);
		}
		#endregion

		#region Fingers_Clearing()
		void Fingers_Clearing()
		{
			this.Dispatcher.Invoke(this.dlgFingersClearing);
		}
		#endregion

		#region FingerAddedTU()
		void FingerAddedTU(ImageProcessing.IpSettings.Finger finger)
		{
			FingerUi fingerUi = GetFingerUi(finger);

			if (fingerUi == null)
			{
				fingerUi = new FingerUi(this, finger);
				this.mainGrid.Children.Add(fingerUi);
			}
		}
		#endregion

		#region FingerRemovingTU()
		void FingerRemovingTU(ImageProcessing.IpSettings.Finger finger)
		{
			FingerUi fingerUi = GetFingerUi(finger);

			if (fingerUi != null)
			{
				this.mainGrid.Children.Remove(fingerUi);
				fingerUi.Dispose();
			}
		}
		#endregion

		#region FingersClearingTU()
		void FingersClearingTU()
		{
			UIElementCollection fingersUi = this.mainGrid.Children;
			this.mainGrid.Children.Clear();

			foreach (FingerUi fingerUi in fingersUi)
				fingerUi.Dispose();
		}
		#endregion

		#region RefreshUi()
		private void RefreshUi()
		{
			foreach (FingerUi fingerUi in this.mainGrid.Children)
				fingerUi.Synchronize();

			if (ToolbarSelection == ViewPane.ToolbarSelection.FingerRemoval)
				this.mainGrid.Cursor = Cursors.Cross;
			else
				this.mainGrid.Cursor = null;
		}
		#endregion

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

		#endregion


	}
}
