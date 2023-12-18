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
	public partial class PanelInit : UserControl
	{
		public event LoadScannerHnd LoadScannerClick;
		
		
		public PanelInit()
		{
			InitializeComponent();
		}

		public void Init(string[] comPorts, string selectedComPort)
		{
			foreach (string comPort in comPorts)
				this.comboSerialPorts.Items.Add(comPort);

			if (this.comboSerialPorts.Items.Contains(selectedComPort))
				this.comboSerialPorts.SelectedItem = selectedComPort;
		}

		private void LoadScanner_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (LoadScannerClick != null)
					LoadScannerClick((string)this.comboSerialPorts.SelectedItem);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
