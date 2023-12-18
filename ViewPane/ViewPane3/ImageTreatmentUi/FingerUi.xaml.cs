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
using ImageProcessing;
using BIP.Geometry;
using ViewPane.ImagePanel;


namespace ViewPane.ImageTreatmentUi
{
	/// <summary>
	/// Interaction logic for FingerUi.xaml
	/// </summary>
	internal partial class FingerUi : UserControl
	{
		FingersUi fingersUi;
		ImageProcessing.IpSettings.Finger finger;

		RatioPoint? startPoint = null;
		bool gotMouseCapture = false;

		ImageProcessing.IpSettings.Finger.ChangedHnd dlgChanged;


		public FingerUi(FingersUi fingersUi, ImageProcessing.IpSettings.Finger finger)
		{
			InitializeComponent();

			this.fingersUi = fingersUi;
			this.finger = finger;

			dlgChanged = new ImageProcessing.IpSettings.Finger.ChangedHnd(FingerChangedTU);

			double width = this.fingersUi.CurrentWidth;
			double height = this.fingersUi.CurrentHeight;

			/*this.Margin = new Thickness(this.finger.PageRect.X * width, this.finger.PageRect.Y * height, 0, 0);
			this.Width = this.finger.PageRect.Width * width;
			this.Height = this.finger.PageRect.Height * height;*/
			this.Margin = new Thickness(this.finger.PageRect.X * width, this.finger.PageRect.Y * height, 0, 0);
			this.Width = this.finger.RectangleNotSkewed.Width * width;
			this.Height = this.finger.RectangleNotSkewed.Height * height;

			this.finger.Changed += new ImageProcessing.IpSettings.Finger.ChangedHnd(Finger_Changed);
		}

		//PUBLIC PROPERTIES
		#region internal properties
		internal ImageProcessing.IpSettings.Finger Finger { get { return finger; } }
		#endregion

		//PRIVATE PROPERTIES
		#region private properties
		ImageUi					ImageUi { get { return this.fingersUi.ImageUi; } }
		ImageProcessing.IpSettings.ItPage	Page { get { return this.fingersUi.Page; } }
		ImagePane				ImagePane { get { return this.fingersUi.ImagePane; } }
		private ViewPane.ToolbarSelection ToolbarSelection { get { return this.ImagePane.ToolbarSelection; } }
		#endregion

	
		//PUBLIC METHODS
		#region internal methods

		#region Synchronize()
		internal void Synchronize()
		{
			double width = this.fingersUi.CurrentWidth;
			double height = this.fingersUi.CurrentHeight;

			this.Margin = new Thickness(this.finger.PageRect.X * width, this.finger.PageRect.Y * height, 0, 0);
			this.Width = this.finger.PageRect.Width * width;
			this.Height = this.finger.PageRect.Height * height;

			if (this.ToolbarSelection == ViewPane.ToolbarSelection.FingerRemoval)
			{
				this.selectedGrid.Visibility = Visibility.Visible;
				this.contentRect.Cursor = Cursors.SizeAll;
			}
			else
			{
				selectedGrid.Visibility = Visibility.Hidden;
				this.contentRect.Cursor = null;
			}
		}
		#endregion

		#region DragStarted()
		public void DragStarted(RatioPoint imagePoint)
		{
			this.Visibility = Visibility.Visible;
			this.selectedGrid.Visibility = Visibility.Visible;

			this.startPoint = imagePoint;
			/*this.pSE.Visibility = Visibility.Visible;
			Mouse.Capture(null);
			Mouse.Capture(this.pSE);

			if (this.pSE.CaptureMouse())
			{
#if DEBUG
				Console.WriteLine("XXX: OK " + DateTime.Now.ToString("HH:mm:ss.ffff"));
#endif
			}
			else
			{
#if DEBUG
				Console.WriteLine("XXX: Error " + DateTime.Now.ToString("HH:mm:ss.ffff"));
#endif
			}*/
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			this.finger.Changed -= new ImageProcessing.IpSettings.Finger.ChangedHnd(Finger_Changed);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Mouse_LeftButtonDown()
		private void Mouse_LeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.FingerRemoval)
			{
				this.startPoint = this.fingersUi.PanelToImage(e);
				((UIElement)sender).CaptureMouse();
				e.Handled = true;

				this.finger.Lock();
			}
		}
		#endregion

		#region Mouse_Move()	
		private void Mouse_Move(object sender, MouseEventArgs e)
		{
			if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.FingerRemoval && e.LeftButton == MouseButtonState.Pressed && startPoint.HasValue && this.gotMouseCapture)
			{
				RatioPoint p = this.fingersUi.PanelToImage(e);
				RatioPoint pointNotSkewed = finger.Page.Clip.TransferSkewedToUnskewedPoint(p);

				MoveRegion(sender, pointNotSkewed, startPoint.Value);
				this.startPoint = pointNotSkewed;

				e.Handled = true;
			}
			else
			{
			}
		}
		#endregion

		#region Mouse_LeftButtonUp()
		private void Mouse_LeftButtonUp(object sender, MouseButtonEventArgs e)
		{
#if DEBUG
			//Console.WriteLine("ZZZ: " + DateTime.Now.ToString("HH:mm:ss.ffff"));
#endif

			if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.FingerRemoval)
			{
				startPoint = null;
				Mouse.Capture(null);
				e.Handled = true;
				this.gotMouseCapture = false;
	
				this.finger.Unlock();
			}
		}
		#endregion

		#region Content_MouseRightDown()
		private void Content_MouseRightDown(object sender, MouseButtonEventArgs e)
		{
			this.finger.RaiseRemoveRequest();
		}
		#endregion

		#region MoveRegion()
		private void MoveRegion(object sender, RatioPoint pagePoint, RatioPoint startPoint)
		{
			RatioRect r = finger.RectangleNotSkewed;
			double dx = pagePoint.X - startPoint.X;
			double dy = pagePoint.Y - startPoint.Y;

			if (dx != 0 || dy != 0)
			{
				if (sender == this.contentRect)
				{
					this.finger.Move(dx, dy);
					this.contentRect.CaptureMouse();
				}
				else if (sender == this.pW)
				{
					this.finger.Resize(dx, 0, 0, 0);
					
					if (pagePoint.X <= r.Right)
						this.pW.CaptureMouse();
					else
						this.pE.CaptureMouse();
				}
				else if (sender == this.pN)
				{
					this.finger.Resize(0, dy, 0, 0);
					
					if (pagePoint.Y <= r.Bottom)
						this.pN.CaptureMouse();
					else
						this.pS.CaptureMouse();
				}
				else if (sender == this.pE)
				{
					this.finger.Resize(0, 0, dx, 0);
					
					if (pagePoint.X >= r.Left)
						this.pE.CaptureMouse();
					else
						this.pW.CaptureMouse();
				}
				else if (sender == this.pS)
				{
					this.finger.Resize(0, 0, 0, dy);
					
					if (pagePoint.Y >= r.Top)
						this.pS.CaptureMouse();
					else
						this.pN.CaptureMouse();
				}
				else if (sender == this.pNW)
				{
					this.finger.Resize(dx, dy, 0, 0);
					
					if ((pagePoint.X <= r.Right) && (pagePoint.Y <= r.Bottom))
						this.pNW.CaptureMouse();
					else if ((pagePoint.X <= r.Right) && (pagePoint.Y > r.Bottom))
						this.pSW.CaptureMouse();
					else if ((pagePoint.X > r.Right) && (pagePoint.Y <= r.Bottom))
						this.pNE.CaptureMouse();
					else if ((pagePoint.X > r.Right) && (pagePoint.Y > r.Bottom))
						this.pSE.CaptureMouse();
				}
				else if (sender == this.pNE)
				{
					this.finger.Resize(0, dy, dx, 0);

					if ((pagePoint.X >= r.Left) && (pagePoint.Y <= r.Bottom))
						this.pNE.CaptureMouse();
					else if ((pagePoint.X < r.Left) && (pagePoint.Y <= r.Bottom))
						this.pNW.CaptureMouse();
					else if ((pagePoint.X >= r.Left) && (pagePoint.Y > r.Bottom))
						this.pSE.CaptureMouse();
					else if ((pagePoint.X < r.Left) && (pagePoint.Y > r.Bottom))
						this.pSW.CaptureMouse();
				}
				else if (sender == this.pSW)
				{
					this.finger.Resize(dx, 0, 0, dy);

					if ((pagePoint.X <= r.Right) && (pagePoint.Y >= r.Top))
						this.pSW.CaptureMouse();
					else if ((pagePoint.X > r.Right) && (pagePoint.Y >= r.Top))
						this.pSE.CaptureMouse();
					else if ((pagePoint.X <= r.Right) && (pagePoint.Y < r.Top))
						this.pNW.CaptureMouse();
					else if ((pagePoint.X > r.Right) && (pagePoint.Y < r.Top))
						this.pNE.CaptureMouse();
				}
				else if (sender == this.pSE)
				{
					this.finger.Resize(0, 0, dx, dy);

					if ((pagePoint.X >= r.Left) && (pagePoint.Y >= r.Top))
						this.pSE.CaptureMouse();
					else if ((pagePoint.X < r.Left) && (pagePoint.Y >= r.Top))
						this.pSW.CaptureMouse();
					else if ((pagePoint.X >= r.Left) && (pagePoint.Y < r.Top))
						this.pNE.CaptureMouse();
					else if ((pagePoint.X < r.Left) && (pagePoint.Y < r.Top))
						this.pNW.CaptureMouse();
				}
			}
			else
			{
				((UIElement)sender).CaptureMouse();
			}
		}
		#endregion

		#region Finger_Changed()
		void Finger_Changed(ImageProcessing.IpSettings.Finger finger, ImageProcessing.IpSettings.Finger.ChangeType type)
		{
			try
			{
				this.Dispatcher.Invoke(this.dlgChanged, finger, type);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region FingerChangedTU()
		void FingerChangedTU(ImageProcessing.IpSettings.Finger finger, ImageProcessing.IpSettings.Finger.ChangeType type)
		{
			Synchronize();
		}
		#endregion

		#region Got_MouseCapture
		private void Got_MouseCapture(object sender, MouseEventArgs e)
		{
			this.gotMouseCapture = true;
		}
		#endregion

		#region pSE_GotMouseCapture
		private void pSE_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.FingerRemoval && Mouse.LeftButton == MouseButtonState.Pressed && startPoint.HasValue)
				this.pSE.CaptureMouse();
		}
		#endregion

		#endregion

	}
}
