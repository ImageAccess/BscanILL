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

namespace ViewPane.Toolbar
{
	/// <summary>
	/// Interaction logic for ToolbarScan.xaml
	/// </summary>
	internal partial class ToolbarScan : ToolbarBase
	{
		bool scanEnabled = true;

		public event EventHandler ScanClick;


		#region constructor
		public ToolbarScan()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public bool ScanEnabled
		{
			get { return this.scanEnabled; }
			set
			{
				this.scanEnabled = value;
				this.buttonScan.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		#endregion


		//PUBLIC METHODS
		#region public methods

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Scan_Click()
		private void Scan_Click(object sender, RoutedEventArgs e)
		{
			if (ScanClick != null)
				ScanClick(sender, e);
		}
		#endregion

		#endregion

	}
}
