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


namespace ViewPane.ImageTreatmentUi
{
	/// <summary>
	/// Interaction logic for BfPointUi.xaml
	/// </summary>
	internal partial class BfPointUi : UserControl
	{
		CurveUi curveUi;
		ImageProcessing.IpSettings.BfPoint bfPoint;

		ImageProcessing.IpSettings.BfPoint.BfPointHnd dlgBfPointChanged;

		public event EventHandler Changed;

		#region constructor
		private BfPointUi()
		{
			InitializeComponent();

			dlgBfPointChanged = new ImageProcessing.IpSettings.BfPoint.BfPointHnd(BfPoint_ChangedTU);
		}

		public BfPointUi(CurveUi curveUi, ImageProcessing.IpSettings.BfPoint bfPoint)
			:this()
		{
			this.curveUi = curveUi;
			this.bfPoint = bfPoint;

			//this.bfPoint.Changed += new ImageProcessing.IpSettings.BfPoint.BfPointHnd(BfPoint_Changed);
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties

		public ImageProcessing.IpSettings.BfPoint BfPoint { get { return this.bfPoint; } }

		#endregion

		//PRIVATE PROPERTIES
		#region private properties

		private double					Zoom { get { return this.curveUi.Zoom; } }
		private ViewPane.ToolbarSelection	ToolbarSelection { get { return this.curveUi.ToolbarSelection; } }
		private ImageProcessing.IpSettings.ItPage	Page { get { return this.curveUi.Page; } }

		#endregion

		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			//this.bfPoint.Changed -= new ImageProcessing.IpSettings.BfPoint.BfPointHnd(BfPoint_Changed);
		}
		#endregion
	
		#region SetButtonLocation()
		/*public void SetButtonLocation()
		{
			RatioPoint p = this.curveUi.Curve.GetPagePoint(bfPoint);
			double width = this.curveUi.CurrentWidth;
			double height = this.curveUi.CurrentHeight;

			double xPanel = p.X * width;
			double yPanel = p.Y * height;

			if (this.Margin == null || this.Margin.Left != xPanel - 5 || this.Margin.Top != yPanel - 5)
			{
				this.Margin = new Thickness(xPanel - 5, yPanel - 5, 0,0);
				
				if (Changed != null)
					Changed(this, null);
			}
		}*/
		#endregion

		#region SetButtonLocation()
		public bool SetButtonLocation()
		{
			RatioPoint p = this.curveUi.Curve.GetPagePoint(bfPoint);
			double width = this.curveUi.CurrentWidth;
			double height = this.curveUi.CurrentHeight;

			double xPanel = p.X * width;
			double yPanel = p.Y * height;

			if (this.Margin == null || this.Margin.Left != xPanel - 5 || this.Margin.Top != yPanel - 5)
			{
				Thickness oldMargin = this.Margin;
				this.Margin = new Thickness(xPanel - 5, yPanel - 5, 0, 0);

				if (oldMargin != this.Margin)
					return true;
			}

			return false;
		}
		#endregion
	
		#endregion

		//PRIVATE METHODS
		#region private methods

		#region Mouse_LeftDown()
		private void Mouse_LeftDown(object sender, MouseButtonEventArgs e)
		{
			if (this.ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
			{
				CaptureMouse();
				e.Handled = true;
			}
		}
		#endregion

		#region Mouse_Move()
		private void Mouse_Move(object sender, MouseEventArgs e)
		{
			if (ToolbarSelection == ViewPane.ToolbarSelection.Bookfold && e.LeftButton == MouseButtonState.Pressed)
			{
				RatioPoint p = this.curveUi.PanelToImage(e);

				this.curveUi.Curve.SetPoint(this.bfPoint, p);

				e.Handled = true;
				CaptureMouse();
			}
		}
		#endregion

		#region Mouse_LeftUp()
		private void Mouse_LeftUp(object sender, MouseButtonEventArgs e)
		{
			if (this.ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
			{
				Mouse.Capture(null);
				e.Handled = true;
			}				
		}
		#endregion

		#region Mouse_RightDown()
		private void Mouse_RightDown(object sender, MouseButtonEventArgs e)
		{
			if (ToolbarSelection == ViewPane.ToolbarSelection.Bookfold)
			{
				this.curveUi.Curve.RemovePoint(this.bfPoint);

				e.Handled = true;
			}
		}
		#endregion

		#region Mouse_RightUp()
		private void Mouse_RightUp(object sender, MouseButtonEventArgs e)
		{

		}
		#endregion

		#region BfPoint_Changed()
		void BfPoint_Changed(ImageProcessing.IpSettings.BfPoint bfPoint)
		{
			this.Dispatcher.Invoke(dlgBfPointChanged, bfPoint);
		}
		#endregion

		#region BfPoint_ChangedTU()
		void BfPoint_ChangedTU(ImageProcessing.IpSettings.BfPoint bfPoint)
		{
			if (SetButtonLocation() && Changed != null)
			{
				Changed(this, null);
			}
		}
		#endregion

		#region GetMousePoint()
		/*public Point GetMousePoint(MouseEventArgs e)
		{
			return this.curveUi.GetMousePoint(e);
		}*/
		#endregion

		#endregion

	}
}
