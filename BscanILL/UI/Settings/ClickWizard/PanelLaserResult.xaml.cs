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
	public partial class PanelLaserResult : UserControl
	{
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd BackClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd ContinueClick;


		#region constructor()
		public PanelLaserResult()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Apply()
		public void Apply(BitmapSource bitmapLLeft, BitmapSource bitmapLMiddle, BitmapSource bitmapLRight, 
			BitmapSource bitmapRLeft, BitmapSource bitmapRMiddle, BitmapSource bitmapRRight)
		{
			this.imageLLeft.Source = bitmapLLeft;
			this.imageLMiddle.Source = bitmapLMiddle;
			this.imageLRight.Source = bitmapLRight;
			this.imageRLeft.Source = bitmapRLeft;
			this.imageRMiddle.Source = bitmapRMiddle;
			this.imageRRight.Source = bitmapRRight;
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
