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
	public partial class PanelFocusResult : UserControl
	{
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd BackClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd ContinueClick;


		#region constructor()
		public PanelFocusResult()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties
		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Apply()
		public void Apply(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR)
		{
			int width = (int)imageGrid.Width;
			int height = (int)imageGrid.Height;

			this.imageL.Source = MainPanel.GetClip(bitmapL, new System.Drawing.Rectangle((bitmapL.Width - width) / 2, (bitmapL.Height - height) / 2, width, height));
			this.imageR.Source = MainPanel.GetClip(bitmapR, new System.Drawing.Rectangle((bitmapR.Width - width) / 2, (bitmapR.Height - height) / 2, width, height));
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Back_Click()
		private void Back_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BackClick();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Continue_Click()
		private void Continue_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ContinueClick();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#endregion

	}
}
