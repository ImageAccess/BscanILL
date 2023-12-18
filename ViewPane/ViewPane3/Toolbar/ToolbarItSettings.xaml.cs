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
	/// Interaction logic for ToolbarItSettings.xaml
	/// </summary>
	internal partial class ToolbarItSettings : ToolbarBase
	{
		public delegate void					IndependentImageClickHnd(bool independent);
		public event IndependentImageClickHnd	IndependentImageClick;
		public event EventHandler				ClipsSameSizeClick;

		bool fireEvents = true;


		#region constructor
		public ToolbarItSettings()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#endregion


		//PUBLIC METHODS
		#region public methods

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region ImagePane_ImageChanged()
		protected override void ImagePane_ImageChanged(VpImage vpImage)
		{
			if (this.ImagePane is ImagePane)
			{
				IViewImage iImage = ((ImagePane)this.ImagePane).IImage;
				this.Visibility = (ImagePane != null && ((ImagePane)this.ImagePane).AllowTransforms && iImage != null && iImage.IsFixed == false) ? Visibility.Visible : Visibility.Collapsed;

				if (this.Visibility == Visibility.Visible)
				{
					bool independent = (iImage != null && iImage.IsFixed == false && iImage.IsIndependent == true);

					if (this.checkIndependentImage.IsChecked != independent)
					{
						this.fireEvents = false;
						this.checkIndependentImage.IsChecked = independent;
						this.buttonClipsSameSize.Visibility = independent ? Visibility.Visible : Visibility.Collapsed;
						this.fireEvents = true;
					}
				}
			}
		}
		#endregion

		#region IndependentImage_CheckedChanged()
		private void IndependentImage_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (this.ImagePane is ImagePane)
			{
				if (this.ImagePane != null && ((ImagePane)this.ImagePane).IImage != null && ((ImagePane)this.ImagePane).IImage.IsFixed == false)
				{
					if (IndependentImageClick != null && this.fireEvents)
						IndependentImageClick(checkIndependentImage.IsChecked.Value);
				}
			}
					
			this.buttonClipsSameSize.Visibility = (checkIndependentImage.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
		}
		#endregion
	
		#region SameSize_Click()
		private void SameSize_Click(object sender, RoutedEventArgs e)
		{
			if (ClipsSameSizeClick != null && this.fireEvents)
				ClipsSameSizeClick(this, e);
		}
		#endregion

		#endregion


	}
}
