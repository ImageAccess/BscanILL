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
	public partial class PanelInitResult : UserControl, INotifyPropertyChanged
	{
		BscanILL.Settings.Settings settings = BscanILL.Settings.Settings.Instance;
		public event PropertyChangedEventHandler PropertyChanged;

		public event BscanILL.UI.Settings.ClickWizard.ClickHnd LightsOnClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd LightsOffClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd LaserOnClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd LaserOffClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd CamerasSettingsClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd CamerasDefaultSettingsClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd SwapCamerasClick;

		public event BscanILL.UI.Settings.ClickWizard.ClickHnd ContinueClick;
		public event BscanILL.UI.Settings.ClickWizard.ScanHnd TestScanClick;


		#region constructor()
		public PanelInitResult()
		{
			InitializeComponent();

			this.DataContext = this;
		}
		#endregion


		// PRIVATE METHODS
		#region private methods

		#region LightsOn_Click()
		private void LightsOn_Click(object sender, RoutedEventArgs e)
		{
			if (LightsOnClick != null)
				LightsOnClick();
		}
		#endregion

		#region LightsOff_Click()
		private void LightsOff_Click(object sender, RoutedEventArgs e)
		{
			if (LightsOffClick != null)
				LightsOffClick();
		}
		#endregion

		#region LaserOn_Click()
		private void LaserOn_Click(object sender, RoutedEventArgs e)
		{
			if (LaserOnClick != null)
				LaserOnClick();
		}
		#endregion

		#region LaserOff_Click()
		private void LaserOff_Click(object sender, RoutedEventArgs e)
		{
			if (LaserOffClick != null)
				LaserOffClick();
		}
		#endregion

		#region CamerasSettings_Click()
		private void CamerasSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (CamerasSettingsClick != null)
					CamerasSettingsClick();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion		
		
		#region CamerasDefaultSettings_Click()
		private void CamerasDefaultSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (CamerasDefaultSettingsClick != null)
					CamerasDefaultSettingsClick();
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

		#region RaisePropertyChanged()
		public void RaisePropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
		#endregion

		#region TestScan_Click()
		private void TestScan_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				TestScanClick(true, false, false, false, this.textSave.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region SwapCameras_Click()
		private void SwapCameras_Click(object sender, RoutedEventArgs e)
		{
			if (SwapCamerasClick != null)
				SwapCamerasClick();
		}
		#endregion

		#endregion

	}
}
