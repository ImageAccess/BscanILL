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
	/// Interaction logic for ToolbarPostProcessing.xaml
	/// </summary>
	public partial class ToolbarPostProcessing : UserControl
	{
		VpImage vpImage = null;
		ImageProcessing.PostProcessing itProcessing = null;


		#region constructor
		public ToolbarPostProcessing()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		#region IsDespeckleChecked
		private bool IsDespeckleChecked
		{
			get { return this.checkDespeckle.IsChecked.Value; }
			set 
			{ 
				if (this.itProcessing != null && this.itProcessing.ItDespeckle.IsEnabled != value)
				{
					if (value == true)
					{
						ViewPane.Dialogs.DespeckleDlg dlg = new ViewPane.Dialogs.DespeckleDlg(this.itProcessing.ItDespeckle.MaskSize, this.itProcessing.ItDespeckle.DespeckleMode);
						dlg.MaskSize = this.itProcessing.ItDespeckle.MaskSize;
						dlg.DespeckleMode = this.itProcessing.ItDespeckle.DespeckleMode;

						if (dlg.ShowDialog() == true)
						{
							this.itProcessing.ItDespeckle.IsEnabled = true;
							this.itProcessing.ItDespeckle.MaskSize = dlg.MaskSize;
							this.itProcessing.ItDespeckle.DespeckleMode = dlg.DespeckleMode;
						}
					}
					else
						this.itProcessing.ItDespeckle.IsEnabled = value;
				}

				this.checkDespeckle.IsChecked = this.itProcessing.ItDespeckle.IsEnabled;
			}
		}
		#endregion

		#region IsDespeckleEnabled
		private bool IsDespeckleEnabled
		{
			set { this.checkDespeckle.IsEnabled = value; }
		}
		#endregion

		#region Rotation
		private System.Windows.Media.Imaging.Rotation Rotation
		{
			get 
			{
				if (this.radioRotation90.IsChecked.Value)
					return System.Windows.Media.Imaging.Rotation.Rotate90;
				else if (this.radioRotation180.IsChecked.Value)
					return System.Windows.Media.Imaging.Rotation.Rotate180;
				else if (this.radioRotation270.IsChecked.Value)
					return System.Windows.Media.Imaging.Rotation.Rotate270;
				else
					return System.Windows.Media.Imaging.Rotation.Rotate0;
			}
			set
			{
				if (this.itProcessing != null)
				{
					switch (value)
					{
						case Rotation.Rotate90: 
							this.radioRotation90.IsChecked = true;
							this.itProcessing.ItRotation.Angle = ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation90;
							break;
						case Rotation.Rotate180:
							this.radioRotation180.IsChecked = true;
							this.itProcessing.ItRotation.Angle = ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation180;
							break;
						case Rotation.Rotate270:
							this.radioRotation270.IsChecked = true;
							this.itProcessing.ItRotation.Angle = ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation270;
							break;
						default: 
							this.radioNoRotation.IsChecked = true;
							this.itProcessing.ItRotation.Angle = ImageProcessing.PostProcessing.Rotation.RotationMode.NoRotation;
							break;
					}
				}
			}
		}
		#endregion

		#region IsRotationEnabled
		private bool IsRotationEnabled
		{
			set 
			{
				this.radioRotation90.IsEnabled = value;
				this.radioRotation180.IsEnabled = value;
				this.radioRotation270.IsEnabled = value;
				this.radioNoRotation.IsEnabled = value;
			}
		}
		#endregion

		#region IsBlackBoardRemovalChecked
		private bool IsBlackBoardRemovalChecked
		{
			get { return this.checkBlackBorderRemoval.IsChecked.Value; }
			set 
			{ 
				this.checkBlackBorderRemoval.IsChecked = value;

				if (this.itProcessing != null && this.itProcessing.ItBlackBorderRemoval.IsEnabled != value)
					this.itProcessing.ItBlackBorderRemoval.IsEnabled = value;
			}
		}
		#endregion

		#region IsBlackBoardRemovalEnabled
		private bool IsBlackBoardRemovalEnabled
		{
			set { this.checkBlackBorderRemoval.IsEnabled = value; }
		}
		#endregion

		#region IsBackgroundRemovalChecked
		private bool IsBackgroundRemovalChecked
		{
			get { return this.checkBackgroundRemoval.IsChecked.Value; }
			set
			{
				this.checkBackgroundRemoval.IsChecked = value;

				if (this.itProcessing != null && this.itProcessing.ItBackgroundRemoval.IsEnabled != value)
					this.itProcessing.ItBackgroundRemoval.IsEnabled = value;
			}
		}
		#endregion

		#region IsBackgroundRemovalEnabled
		private bool IsBackgroundRemovalEnabled
		{
			set { this.checkBackgroundRemoval.IsEnabled = value; }
		}
		#endregion

		#region IsInvertChecked
		private bool IsInvertChecked
		{
			get { return this.checkInvert.IsChecked.Value; }
			set
			{
				this.checkInvert.IsChecked = value;

				if (this.itProcessing != null && this.itProcessing.ItInvertion.IsEnabled != value)
					this.itProcessing.ItInvertion.IsEnabled = value;
			}
		}
		#endregion

		#region IsInvertEnabled
		private bool IsInvertEnabled
		{
			set { this.checkInvert.IsEnabled = value; }
		}
		#endregion


		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Set()
		internal void Set(ImagePane imagePane)
		{
			if (imagePane.AllowTransforms && imagePane.IImage != null && imagePane.IImage.IsFixed == false && imagePane.IImage is ViewPane.Hierarchy.VpImage)
			{
				this.Width = 34;

				if (imagePane.Licensing.PostProcessing == PostProcessingMode.Advanced)
					this.gridAdvanced.Visibility = System.Windows.Visibility.Visible;
				else
					this.gridAdvanced.Visibility = System.Windows.Visibility.Collapsed;

				this.vpImage = (VpImage)imagePane.IImage;
				this.itProcessing = this.vpImage.PostProcessing;

				if (imagePane.IImage.FullImageInfo.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite)
				{
					this.IsDespeckleEnabled = true;
					this.IsDespeckleChecked = itProcessing.ItDespeckle.IsEnabled;
					this.IsBlackBoardRemovalEnabled = true;
					this.IsBlackBoardRemovalChecked = itProcessing.ItBlackBorderRemoval.IsEnabled;
					this.IsBackgroundRemovalEnabled = false;
				}
				else
				{
					this.IsDespeckleEnabled = false;
					this.IsBlackBoardRemovalEnabled = false;
					this.IsBackgroundRemovalChecked = itProcessing.ItBackgroundRemoval.IsEnabled;
					this.IsBackgroundRemovalEnabled = true;
				}

				switch (itProcessing.ItRotation.Angle)
				{
					case ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation90: this.Rotation = Rotation.Rotate90; break;
					case ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation180: this.Rotation = Rotation.Rotate180; break;
					case ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation270: this.Rotation = Rotation.Rotate270; break;
					default: this.Rotation = Rotation.Rotate0; break;
				}

				this.IsInvertChecked = itProcessing.ItInvertion.IsEnabled;
			}
			else
			{
				this.Width = 0;
				this.vpImage = null;
				this.itProcessing = null;
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Despeckle_CheckedChanged()
		private void Despeckle_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.IsDespeckleChecked = this.checkDespeckle.IsChecked.Value;
		}
		#endregion

		#region Rotation_Checked()
		private void Rotation_Checked(object sender, RoutedEventArgs e)
		{
			if (this.radioRotation90.IsChecked.Value)
			{
				this.Rotation = Rotation.Rotate90;
			}
			else if (this.radioRotation180.IsChecked.Value)
			{
				this.Rotation = Rotation.Rotate180;
			}
			else if (this.radioRotation270.IsChecked.Value)
			{
				this.Rotation = Rotation.Rotate270;
			}
			else if (this.radioNoRotation.IsChecked.Value)
			{
				this.Rotation = Rotation.Rotate0;
			}
		}
		#endregion

		#region BlackBorder_CheckedChanged()
		private void BlackBorder_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.IsBlackBoardRemovalChecked = this.checkBlackBorderRemoval.IsChecked.Value;
		}
		#endregion

		#region Background_CheckedChanged()
		private void Background_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.IsBackgroundRemovalChecked = this.checkBackgroundRemoval.IsChecked.Value;
		}
		#endregion

		#region Invert_CheckedChanged()
		private void Invert_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.IsInvertChecked = this.checkInvert.IsChecked.Value;
		}
		#endregion

		#endregion


	}
}
