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
using System.ComponentModel;

namespace BscanILL.UI.Settings.ClickWizard
{
	/// <summary>
	/// Interaction logic for PanelInit.xaml
	/// </summary>
	public partial class PanelWhiteBalanceProgress : UserControl
	{

		#region constructor()
		public PanelWhiteBalanceProgress()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Apply()
		public void Apply(BitmapSource bitmapL, BitmapSource bitmapR, CanonCamera.CameraProperties.Tv tv, double shade)
		{
			this.imageLTop.Source = bitmapL;
			this.imageRTop.Source = bitmapR;

			this.textSpeed.Text = CanonCamera.CameraProperties.TvProperty.GetName(tv);
			this.textShade.Text = shade.ToString("0");
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#endregion

	}
}
