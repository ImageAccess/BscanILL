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
using ViewPane.ImagePanel;

namespace ViewPane.Toolbar
{
	/// <summary>
	/// Interaction logic for ToolbarZoomMode.xaml
	/// </summary>
	internal partial class ToolbarZoomMode : ToolbarBase
	{
		public ImagePane.ZoomModeChangedHnd ZoomModeChanged;


		#region constructor
		public ToolbarZoomMode()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region ToolbarSelection
		public ViewPane.ToolbarSelection ToolbarSelection
		{
			set
			{
				this.radioMove.IsChecked = (value == ViewPane.ToolbarSelection.Move);
				this.radioZoomIn.IsChecked = (value == ViewPane.ToolbarSelection.ZoomIn);
				this.radioZoomOut.IsChecked = (value == ViewPane.ToolbarSelection.ZoomOut);
				this.radioDynamicZoom.IsChecked = (value == ViewPane.ToolbarSelection.ZoomDynamic);
				this.radioSelectArea.IsChecked = (value == ViewPane.ToolbarSelection.SelectRegion);
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region ZoomMode_Changed()
		void ZoomMode_Changed(object sender, RoutedEventArgs e)
		{
			if (ZoomModeChanged != null)
			{
				if(sender == this.radioMove)
					ZoomModeChanged(ViewPane.ToolbarSelection.Move, ViewPane.ToolbarSelection.Move);
				else if (sender == this.radioZoomIn)
					ZoomModeChanged(ViewPane.ToolbarSelection.Move, ViewPane.ToolbarSelection.ZoomIn);
				else if (sender == this.radioZoomOut)
					ZoomModeChanged(ViewPane.ToolbarSelection.Move, ViewPane.ToolbarSelection.ZoomOut);
				else if (sender == this.radioDynamicZoom)
					ZoomModeChanged(ViewPane.ToolbarSelection.Move, ViewPane.ToolbarSelection.ZoomDynamic);
				else if (sender == this.radioSelectArea)
					ZoomModeChanged(ViewPane.ToolbarSelection.Move, ViewPane.ToolbarSelection.SelectRegion);
			}
		}
		#endregion

		#endregion
	
	}
}
