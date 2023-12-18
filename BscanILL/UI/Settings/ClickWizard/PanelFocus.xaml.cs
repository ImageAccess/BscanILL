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

namespace BscanILL.UI.Settings.ClickWizard
{
	/// <summary>
	/// Interaction logic for PanelInitResult.xaml
	/// </summary>
	public partial class PanelFocus : UserControl
	{
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd BackClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd FindClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd SkipClick;


		public PanelFocus()
		{
			InitializeComponent();
		}

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

		private void Find_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				FindClick();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Skip_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SkipClick();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

	}
}
