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
using System.Windows.Shapes;
using System.ComponentModel;
using ViewPane.Hierarchy;

namespace ViewPane.Dialogs
{
	/// <summary>
	/// Interaction logic for IpDlg.xaml
	/// </summary>
	public partial class IpDlg : Window, INotifyPropertyChanged
	{
		List<VpImage> vpImages;
		int currentImageIndex;

		List<DespeckleDlg.ComboDespeckleModeItem> despeckleModes = new List<DespeckleDlg.ComboDespeckleModeItem>();
		ImageProcessing.NoiseReduction.DespeckleMode despeckleMode = ImageProcessing.NoiseReduction.DespeckleMode.WhiteSpecklesOnly;

		public event PropertyChangedEventHandler PropertyChanged;


		#region constructor
		public IpDlg()
		{
			InitializeComponent();

			applyToCombo.Items.Add("Current Image");
			applyToCombo.Items.Add("Current and All Subsequent Dependent Images");
			applyToCombo.Items.Add("Current and All Subsequent Images");
			applyToCombo.Items.Add("Current and All Dependent Images");
			applyToCombo.Items.Add("All Images");

			rotationCombo.Items.Add("No Rotation");
			rotationCombo.Items.Add("90° Rotation");
			rotationCombo.Items.Add("180° Rotation");
			rotationCombo.Items.Add("270° Rotation");

			despeckleModes.Add(new DespeckleDlg.ComboDespeckleModeItem(ViewPane.Languages.UiStrings.BothBlackAndWhiteSpeckles_STR, ImageProcessing.NoiseReduction.DespeckleMode.BothColors));
			despeckleModes.Add(new DespeckleDlg.ComboDespeckleModeItem(ViewPane.Languages.UiStrings.BlackSpecklesOnly_STR, ImageProcessing.NoiseReduction.DespeckleMode.BlackSpecklesOnly));
			despeckleModes.Add(new DespeckleDlg.ComboDespeckleModeItem(ViewPane.Languages.UiStrings.WhiteSpecklesOnly_STR, ImageProcessing.NoiseReduction.DespeckleMode.WhiteSpecklesOnly));

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public List<DespeckleDlg.ComboDespeckleModeItem> DespeckleModes { get { return this.despeckleModes; } set { this.despeckleModes = value; } }

		#region SelectedDespeckleMode
		public DespeckleDlg.ComboDespeckleModeItem SelectedDespeckleMode
		{
			get
			{
				foreach (DespeckleDlg.ComboDespeckleModeItem despeckleMode in despeckleModes)
					if (despeckleMode.DespeckleMode == this.despeckleMode)
						return despeckleMode;

				return null;
			}
			set
			{
				if (value != null)
				{
					this.despeckleMode = value.DespeckleMode;
					RaisePropertyChanged("SelectedDespeckleMode");
				}
			}
		}
		#endregion

		#region DespeckleMode
		public ImageProcessing.NoiseReduction.DespeckleMode DespeckleMode
		{
			get
			{
				return this.despeckleMode;
			}
			set
			{
				this.despeckleMode = value;
				RaisePropertyChanged("DespeckleMode");
				RaisePropertyChanged("SelectedDespeckleMode");
			}
		}
		#endregion
		
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Open()
		public void Open(bool advanced, List<VpImage> vpImages, int currentImageIndex)
		{
			this.vpImages = vpImages;
			this.currentImageIndex = currentImageIndex;

			if (advanced)
			{
				this.groupBoxRotation.Visibility = Visibility.Visible;
				this.groupBoxBackgroundRemoval.Visibility = Visibility.Visible;
				this.groupBoxBlackBorderRemoval.Visibility = Visibility.Visible;
				this.groupBoxInvert.Visibility = Visibility.Visible;
				this.Height = 460;
			}
			else
			{
				this.groupBoxRotation.Visibility = Visibility.Collapsed;
				this.groupBoxBackgroundRemoval.Visibility = Visibility.Collapsed;
				this.groupBoxBlackBorderRemoval.Visibility = Visibility.Collapsed;
				this.groupBoxInvert.Visibility = Visibility.Collapsed;
				this.Height = 220;
			}

			if (currentImageIndex >= 0)
			{
				VpImage vpImage = vpImages[currentImageIndex];

				this.despeckleEnabled.IsChecked = vpImage.PostProcessing.ItDespeckle.IsEnabled;
				this.despeckleMask.SelectedIndex = ((int)vpImage.PostProcessing.ItDespeckle.MaskSize) - 1;
				this.DespeckleMode = vpImage.PostProcessing.ItDespeckle.DespeckleMode;

				switch (vpImage.PostProcessing.ItRotation.Angle)
				{
					case ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation90: rotationCombo.SelectedIndex = 1; break;
					case ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation180: rotationCombo.SelectedIndex = 2; break;
					case ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation270: rotationCombo.SelectedIndex = 3; break;
					default: rotationCombo.SelectedIndex = 0; break;
				}

				this.blackBorderEnabled.IsChecked = vpImage.PostProcessing.ItBlackBorderRemoval.IsEnabled;
				this.backgroundEnabled.IsChecked = vpImage.PostProcessing.ItBackgroundRemoval.IsEnabled;
				this.invertEnabled.IsChecked = vpImage.PostProcessing.ItInvertion.IsEnabled;
			}

			this.ShowDialog();
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region DespeckleChecked_Changed()
		private void DespeckleChecked_Changed(object sender, RoutedEventArgs e)
		{
			this.despeckleMask.IsEnabled = this.despeckleEnabled.IsChecked.Value;
			this.comboDespeckleOptions.IsEnabled = this.despeckleEnabled.IsChecked.Value;
		}
		#endregion

		#region Apply_Click()
		private void Apply_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				List<VpImage> images = GetSelectedImages();

				foreach (VpImage vpImage in images)
				{
					vpImage.PostProcessing.ItDespeckle.IsEnabled = this.despeckleEnabled.IsChecked.Value;
					vpImage.PostProcessing.ItDespeckle.MaskSize = (ImageProcessing.NoiseReduction.DespeckleSize)(this.despeckleMask.SelectedIndex + 1);
					vpImage.PostProcessing.ItDespeckle.DespeckleMode = this.DespeckleMode;

					switch (rotationCombo.SelectedIndex)
					{
						case 1: vpImage.PostProcessing.ItRotation.Angle = ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation90; break;
						case 2: vpImage.PostProcessing.ItRotation.Angle = ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation180; break;
						case 3: vpImage.PostProcessing.ItRotation.Angle = ImageProcessing.PostProcessing.Rotation.RotationMode.Rotation270; break;
						default: vpImage.PostProcessing.ItRotation.Angle = ImageProcessing.PostProcessing.Rotation.RotationMode.NoRotation; break;
					}

					vpImage.PostProcessing.ItBlackBorderRemoval.IsEnabled = this.blackBorderEnabled.IsChecked.Value;
					vpImage.PostProcessing.ItBackgroundRemoval.IsEnabled = this.backgroundEnabled.IsChecked.Value;
					vpImage.PostProcessing.ItInvertion.IsEnabled = this.invertEnabled.IsChecked.Value;
				}

				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Close_Click()
		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		#endregion

		#region GetSelectedImages()
		private List<VpImage> GetSelectedImages()
		{
			List<VpImage> images = new List<VpImage>();
			
			switch (this.applyToCombo.SelectedIndex)
			{
				//<TextBlock Text="Current Image" />
				//<TextBlock Text="Current and All Subsequent Dependent Images" />
				//<TextBlock Text="Current and All Subsequent Images" />
				//<TextBlock Text="Current and All Dependent Images" />
				//<TextBlock Text="All Images" />
				case 1:
					{
						images.Add(vpImages[currentImageIndex]);

						for (int i = currentImageIndex + 1; i < vpImages.Count; i++)
						{
							VpImage vpImage = vpImages[i];

							if (vpImage.IsIndependent == false)
								images.Add(vpImage);
						}
					} break;
				case 2:
					{
						for (int i = currentImageIndex ; i < vpImages.Count; i++)
							images.Add(vpImages[i]);
					} break;
				case 3:
					{
						images.Add(vpImages[currentImageIndex]);

						for (int i = 0; i < vpImages.Count; i++)
						{
							VpImage vpImage = vpImages[i];

							if (vpImage.IsIndependent == false)
								images.Add(vpImage);
						}
					} break;
				case 4:
					{
						for (int i = 0; i < vpImages.Count; i++)
							images.Add(vpImages[i]);
					} break;
				default:
					{
						images.Add(vpImages[currentImageIndex]);
					} break;
			}

			return images;
		}
		#endregion

		#region RaisePropertyChanged()
		private void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		#endregion

	}
}
