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
	/// Interaction logic for BookfoldUi.xaml
	/// </summary>
	internal partial class BookfoldUi : UserControl
	{
		PageUi pageUi;
		Point? startPoint = null;



		#region constructor
		public BookfoldUi()
		{
			InitializeComponent();
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties

		public double								Zoom		{ get { return this.ImagePane.Zoom; } }
		public ViewPane.ToolbarSelection		ToolbarSelection { get { return this.ImagePane.ToolbarSelection; } }
		public ImageProcessing.IpSettings.ItPage	Page		{ get { return this.pageUi.Page; } }
		public ImagePane							ImagePane	{ get { return this.pageUi.ImagePane; } }
		internal double								CurrentWidth { get { return pageUi.CurrentWidth; } }
		internal double								CurrentHeight { get { return pageUi.CurrentHeight; } }

		#endregion

		//PRIVATE PROPERTIES
		#region private properties
		Point					StartPoint { set { this.startPoint = value; } }
		ImageUi					ImageUi { get { return this.pageUi.ImageUi; } }

		#endregion

		//PUBLIC METHODS
		#region internal methods

		#region Init()
		internal void Init(PageUi pageUi)
		{
			this.pageUi = pageUi;

			this.curveUiT.Init(this);
			this.curveUiB.Init(this);
		}
		#endregion

		#region Synchronize()
		internal void Synchronize()
		{
			if (Page != null && Page.Bookfolding != null)
			{
				this.curveUiT.Synchronize(Page.Bookfolding.TopCurve);		
				this.curveUiB.Synchronize(Page.Bookfolding.BottomCurve);
			}
			else
			{				
				this.curveUiT.Synchronize(null);
				this.curveUiB.Synchronize(null);
			}
	
			RefreshUi();
		}
		#endregion

		#region PanelToImage()
		public RatioPoint PanelToImage(MouseEventArgs e)
		{
			return pageUi.PanelToImage(e);
		}
		#endregion

		#region GetMousePoint()
		/*public Point GetMousePoint(MouseEventArgs e)
		{
			return pageUi.GetMousePoint(e);
		}*/
		#endregion

		#region MainGrid_MouseMove()
		public void MainGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (Page != null && Page.Bookfolding != null)
			{
				this.Cursor = Cursors.Cross;
			}
			else
			{
				this.Cursor = null;
			}
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region RefreshUi()
		private void RefreshUi()
		{
			if (Page != null && Page.Bookfolding != null)
			{
				this.Visibility = Visibility.Visible;
			}
			else
			{
				this.Visibility = Visibility.Hidden;
			}
		}
		#endregion

		#endregion
	}
}
