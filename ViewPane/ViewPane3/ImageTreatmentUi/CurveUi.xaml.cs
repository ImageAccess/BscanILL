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
	/// Interaction logic for CurveUi.xaml
	/// </summary>
	internal partial class CurveUi : UserControl
	{
		BookfoldUi bookfoldUi;
		ImageProcessing.IpSettings.Curve curve;

		ImageProcessing.IpSettings.BfPoint.BfPointHnd dlgCurve_PointRemoving;
		ImageProcessing.IpSettings.BfPoint.BfPointHnd dlgCurve_PointAdded;
		ImageProcessing.IpSettings.BfPoint.VoidHnd dlgCurve_Clearing;

		RatioPoint? startPoint = null;

		object locker = new object();


		#region constructor
		public CurveUi()
		{
			InitializeComponent();

			dlgCurve_PointRemoving = new ImageProcessing.IpSettings.BfPoint.BfPointHnd(Curve_PointRemovingTU);
			dlgCurve_PointAdded = new ImageProcessing.IpSettings.BfPoint.BfPointHnd(Curve_PointAddedTU);
			dlgCurve_Clearing = new ImageProcessing.IpSettings.BfPoint.VoidHnd(Curve_ClearingTU);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public double								Zoom		{ get { return this.bookfoldUi.Zoom; } }
		public ViewPane.ToolbarSelection			ToolbarSelection	{ get { return this.bookfoldUi.ToolbarSelection; } }
		public ImageProcessing.IpSettings.ItPage	Page		{ get { return this.bookfoldUi.Page; } }
		public ImageProcessing.IpSettings.Curve		Curve		{ get { return this.curve; } }
		internal double								CurrentWidth { get { return bookfoldUi.CurrentWidth; } }
		internal double								CurrentHeight { get { return bookfoldUi.CurrentHeight; } }

		#endregion

		//PUBLIC METHODS
		#region public methods

		#region Init()
		internal void Init(BookfoldUi bookfoldUi)
		{
			this.bookfoldUi = bookfoldUi;
			
			this.bookfoldUi.ImagePane.ZoomChanged += new ImagePane.ZoomChangedHnd(ImagePane_ZoomChanged);
			this.bookfoldUi.ImagePane.ZoomModeChanged += new ImagePane.ZoomModeChangedHnd(ImagePane_ZoomModeChanged);
		}
		#endregion
	
		#region Synchronize()
		internal void Synchronize(ImageProcessing.IpSettings.Curve curve)
		{
			if (this.curve != curve)
			{
				if (this.curve != null)
				{
					this.curve.PointRemoving -= new ImageProcessing.IpSettings.BfPoint.BfPointHnd(Curve_PointRemoving);
					this.curve.PointAdded -= new ImageProcessing.IpSettings.BfPoint.BfPointHnd(Curve_PointAdded);
					this.curve.Clearing -= new ImageProcessing.IpSettings.BfPoint.VoidHnd(Curve_Clearing);
					this.curve.Changed -= new ImageProcessing.IpSettings.ItImage.VoidHnd(Curve_Changed);
				}

				foreach (BfPointUi bfPointUi in this.gridPoints.Children)
				{
					bfPointUi.Changed -= new EventHandler(BfPointUi_Changed);
					bfPointUi.Dispose();
				}

				this.gridPoints.Children.Clear();

				this.curve = curve;

				if (this.curve != null)
				{
					this.curve.PointRemoving += new ImageProcessing.IpSettings.BfPoint.BfPointHnd(Curve_PointRemoving);
					this.curve.PointAdded += new ImageProcessing.IpSettings.BfPoint.BfPointHnd(Curve_PointAdded);
					this.curve.Clearing += new ImageProcessing.IpSettings.BfPoint.VoidHnd(Curve_Clearing);
					this.curve.Changed += new ImageProcessing.IpSettings.ItImage.VoidHnd(Curve_Changed);
				}

				if (this.curve != null)
				{
					//top curve
					ImageProcessing.IpSettings.BfPoints bfPoints = this.curve.BfPoints;

					for (int i = this.gridPoints.Children.Count - 1; i >= 0; i--)
					{
						if (bfPoints.Contains(((BfPointUi)this.gridPoints.Children[i]).BfPoint) == false)
						{
							((BfPointUi)this.gridPoints.Children[i]).Changed -= new EventHandler(BfPointUi_Changed);
							this.gridPoints.Children.RemoveAt(i);
						}
					}

					foreach (ImageProcessing.IpSettings.BfPoint bfPoint in bfPoints)
					{
						BfPointUi bfPointUi = GetBfPointUi(bfPoint);

						if (bfPointUi == null)
						{
							bfPointUi = new BfPointUi(this, bfPoint);
							bfPointUi.Changed += new EventHandler(BfPointUi_Changed);
							this.gridPoints.Children.Add(bfPointUi);
						}
					}

				}
				else
				{
					foreach (BfPointUi bfPointUi in this.gridPoints.Children)
					{
						bfPointUi.Changed -= new EventHandler(BfPointUi_Changed);
						bfPointUi.Dispose();
					}

					this.gridPoints.Children.Clear();
				}
			}
				
			RefreshUi();	
		}
		#endregion

		#region PanelToImage()
		public RatioPoint PanelToImage(MouseEventArgs e)
		{
			return this.bookfoldUi.PanelToImage(e);
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region DrawCurve()
		private void DrawCurve()
		{
#if DEBUG
			Console.WriteLine("CurveUi, DrawCurve()");
#endif
	
			if (this.curve != null)
			{
				this.Visibility = Visibility.Visible;

				List<System.Drawing.Point> pts = new List<System.Drawing.Point>();

				foreach (BfPointUi bfPointUi in gridPoints.Children)
					pts.Add(new System.Drawing.Point(Convert.ToInt32(bfPointUi.Margin.Left + 5), Convert.ToInt32(bfPointUi.Margin.Top + 5)));

				if (pts.Count >= 2)
				{
					var sortedPoints = from p in pts orderby p.X ascending select p;

					double[] curveArray = ImageProcessing.IpSettings.Curve.GetArray(sortedPoints.ToArray());
					double yFrom = sortedPoints.ToArray()[0].Y;

					polygon.Points.Clear();
					polygon.Visibility = Visibility.Visible;

					for (int x = 0; x < curveArray.Length; x++)
						if (x >= 0 && x < curveArray.Length)
							polygon.Points.Add(new Point(x, curveArray[x]));
				}
				else
				{
					polygon.Points.Clear();
					polygon.Visibility = Visibility.Hidden;
				}
			}
			else
			{
				this.Visibility = Visibility.Hidden;
			}
		}
		#endregion
	
		#region GetBfPointUi()
		private BfPointUi GetBfPointUi(ImageProcessing.IpSettings.BfPoint bfPoint)
		{
			foreach (BfPointUi bfPointUi in this.gridPoints.Children)
				if (bfPointUi.BfPoint == bfPoint)
					return bfPointUi;

			return null;
		}
		#endregion

		#region Mouse_LeftButtonDown()
		private void Mouse_LeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (this.ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
			{
				this.startPoint = PanelToImage(e);
				((UIElement)sender).CaptureMouse();
				e.Handled = true;
			}
		}
		#endregion

		#region Mouse_Move()
		private void Mouse_Move(object sender, MouseEventArgs e)
		{
			if (ToolbarSelection == ViewPane.ToolbarSelection.Bookfold && e.LeftButton == MouseButtonState.Pressed && startPoint.HasValue)
			{
				RatioPoint p = PanelToImage(e);

				if (sender == this.polygon)
				{
					double dy = p.Y - startPoint.Value.Y;

					this.curve.Shift(dy);
				}

				startPoint = p;
				e.Handled = true;
				((UIElement)sender).CaptureMouse();
			}
		}
		#endregion

		#region Mouse_LeftButtonUp()
		private void Mouse_LeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (this.ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
			{
				startPoint = null;
				Mouse.Capture(null);
				e.Handled = true;
			}
		}
		#endregion

		#region Mouse_RightButtonDown()
		private void Mouse_RightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
			{
				if (sender == this.polygon)
				{
					RatioPoint p = PanelToImage(e);

					this.curve.AddPoint(p);
				}

				e.Handled = true;
			}
		}
		#endregion

		#region Curve_PointAdded()
		void Curve_PointAdded(ImageProcessing.IpSettings.BfPoint bfPoint)
		{
			this.Dispatcher.Invoke(dlgCurve_PointAdded, bfPoint);
		}
		#endregion

		#region Curve_PointRemoving()
		void Curve_PointRemoving(ImageProcessing.IpSettings.BfPoint bfPoint)
		{
			this.Dispatcher.Invoke(dlgCurve_PointRemoving, bfPoint);
		}
		#endregion

		#region Curve_Clearing()
		void Curve_Clearing()
		{
			this.Dispatcher.Invoke(dlgCurve_Clearing);
		}
		#endregion

		#region Curve_PointAddedTU()
		void Curve_PointAddedTU(ImageProcessing.IpSettings.BfPoint bfPoint)
		{
			BfPointUi bfPointUi = new BfPointUi(this, bfPoint);
			bfPointUi.Changed += new EventHandler(BfPointUi_Changed);
			this.gridPoints.Children.Add(bfPointUi);

			RefreshUi();
		}
		#endregion

		#region Curve_PointRemovingTU()
		void Curve_PointRemovingTU(ImageProcessing.IpSettings.BfPoint bfPoint)
		{
			BfPointUi bfPointUi = GetBfPointUi(bfPoint);

			if (bfPointUi != null)
			{
				bfPointUi.Changed -= new EventHandler(BfPointUi_Changed);
				this.gridPoints.Children.Remove(bfPointUi);
				bfPointUi.Dispose();
				RefreshUi();
			}
		}
		#endregion

		#region Curve_ClearingTU()
		void Curve_ClearingTU()
		{
			foreach (BfPointUi bfPointUi in this.gridPoints.Children)
			{
				bfPointUi.Changed -= new EventHandler(BfPointUi_Changed);
				bfPointUi.Dispose();
			}

			this.gridPoints.Children.Clear();
			RefreshUi();
		}
		#endregion

		#region RefreshUi()
		void RefreshUi()
		{
#if DEBUG
			Console.WriteLine("CurveUi, RefreshUi()");
#endif


			lock (locker)
			{
#if DEBUG
				//MessageBox.Show("CurveUi, RefreshUi() 01"); 
#endif
				bool changed = false;

				foreach (BfPointUi bfPointUi in this.gridPoints.Children)
				{
					changed |= bfPointUi.SetButtonLocation();
				}

#if DEBUG
				//MessageBox.Show("CurveUi, RefreshUi() 02"); 
#endif
				//if (changed)
				{	
					if (this.curve != null && ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
					{
#if DEBUG
						//MessageBox.Show("CurveUi, RefreshUi() 03");
#endif

						if (changed)
							DrawCurve();


#if DEBUG
						//MessageBox.Show("CurveUi, RefreshUi() 04");
#endif

						this.gridPoints.Visibility = Visibility.Visible;
						//this.polygon.SetValue(Panel.ZIndexProperty,666);
						this.polygon.Cursor = Cursors.SizeNS;
					}
					else if (this.curve != null && curve.IsCurved)
					{
#if DEBUG
						//MessageBox.Show("CurveUi, RefreshUi() 05");
#endif

						if (changed)
							DrawCurve();
#if DEBUG
						//MessageBox.Show("CurveUi, RefreshUi() 06");
#endif

						this.gridPoints.Visibility = Visibility.Hidden;
						this.polygon.Cursor = null;
					}
					else
					{
#if DEBUG
						//MessageBox.Show("CurveUi, RefreshUi() 07");
#endif

						polygon.Points.Clear();
						polygon.Visibility = Visibility.Hidden;
						this.gridPoints.Visibility = Visibility.Hidden;
						this.polygon.Cursor = null;
#if DEBUG
						//MessageBox.Show("CurveUi, RefreshUi() 08");
#endif

					}
				}
			}
		}
		#endregion

		#region BfPointUi_Changed()
		void BfPointUi_Changed(object sender, EventArgs e)
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

		#region ImagePane_ZoomModeChanged()
		void ImagePane_ZoomModeChanged(ViewPane.ToolbarSelection oldZoomMode, ViewPane.ToolbarSelection newZoomMode)
		{
			RefreshUi();
		}
		#endregion

		#region Curve_Changed()
		void Curve_Changed()
		{
			if (this.Dispatcher.CheckAccess())
				RefreshUi();
			else
				this.Dispatcher.Invoke((Action)delegate() { RefreshUi(); });
		}
		#endregion

		#endregion

	}
}
