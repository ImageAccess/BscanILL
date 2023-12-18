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
using System.Reflection;

namespace BscanILL.UI.Settings.ClickWizard
{
	/// <summary>
	/// Interaction logic for PanelInitResult.xaml
	/// </summary>
	public partial class PanelTest : BscanILL.UI.Settings.Panels.PanelBase
	{
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd	BackClick;
		public event BscanILL.UI.Settings.ClickWizard.ScanHnd		TestScanClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd	CloseClick;


		#region constructor()
		public PanelTest()
		{
			InitializeComponent();

			this.DataContext = this;
	
			distortionControlL.Init(ClickDLL.Settings.Settings.Instance.IT.ItPageL.DistortionMap);
			distortionControlR.Init(ClickDLL.Settings.Settings.Instance.IT.ItPageR.DistortionMap);
			cropControl.Init(ClickDLL.Settings.Settings.Instance);
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

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

		#region TestScan_Click()
		private void TestScan_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				TestScanClick(true, true, true, checkBookfold.IsChecked.Value, this.textSave.Text);
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
			try
			{
				CloseClick();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Advanced_CheckedChanged()
		private void Advanced_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.groupBoxDistortion.Visibility = checkAdvanced.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
		}
		#endregion

		#endregion

	}
}
