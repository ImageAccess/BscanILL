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
using ViewPane.Hierarchy;

namespace ViewPane.Toolbar
{
	/// <summary>
	/// Interaction logic for ToolbarItTransforms.xaml
	/// </summary>
	internal partial class ToolbarItTransforms : ToolbarBase
	{
		bool rotationEnabled = true;
		bool bfEnabled = true;
		bool fingersEnabled = true;

		public event ImagePane.ZoomModeChangedHnd ZoomModeChanged;


		#region constructor
		public ToolbarItTransforms()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public bool SkewEnabled
		{
			get { return this.rotationEnabled; }
			set
			{
				this.rotationEnabled = value;
			}
		}

		public bool BfEnabled
		{
			get { return this.bfEnabled; }
			set
			{
				this.bfEnabled = value;
				this.buttonBookfold.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public bool FingersEnabled
		{
			get { return this.fingersEnabled; }
			set
			{
				this.fingersEnabled = value;
				this.buttonFingerRemoval.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public ViewPane.ToolbarSelection ToolbarSelection
		{
			set
			{
				this.buttonBookfold.IsChecked = (value == ViewPane.ToolbarSelection.Bookfold);
				this.buttonPages.IsChecked = (value == ViewPane.ToolbarSelection.Pages);
				this.buttonFingerRemoval.IsChecked = (value == ViewPane.ToolbarSelection.FingerRemoval);
			}
		}
	
		#endregion


		//PUBLIC METHODS
		#region public methods

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Radio_Checked()
		void Radio_Checked(object sender, RoutedEventArgs e)
		{
			if (ZoomModeChanged != null)
			{
				if(sender == buttonBookfold)
					ZoomModeChanged(ViewPane.ToolbarSelection.Move, ViewPane.ToolbarSelection.Bookfold);
				else if(sender == buttonPages)
					ZoomModeChanged(ViewPane.ToolbarSelection.Move, ViewPane.ToolbarSelection.Pages);
				else if(sender == buttonFingerRemoval)
					ZoomModeChanged(ViewPane.ToolbarSelection.Move, ViewPane.ToolbarSelection.FingerRemoval);
			}
		}
		#endregion

		#region Settings_Click()
		private void Settings_Click(object sender, RoutedEventArgs e)
		{
		}
		#endregion

		#region ImagePane_ImageChanged()
		protected override void ImagePane_ImageChanged(VpImage vpImage)
		{
			if (this.ImagePane is ImagePane)
			{
				IViewImage iImage = ((ImagePane)this.ImagePane).IImage;
				this.Visibility = (ImagePane != null && ((ImagePane)this.ImagePane).AllowTransforms && iImage != null && iImage.IsFixed == false) ? Visibility.Visible : Visibility.Collapsed;
			}
		}
		#endregion

		#endregion


	}
}
