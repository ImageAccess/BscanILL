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

namespace BscanILL.UI.Controls
{
	/// <summary>
	/// Interaction logic for FastKeysControl.xaml
	/// </summary>
	public partial class FastKeysControl : UserControl
	{
		public event BscanILL.Misc.VoidHnd HelpClick;
		public event BscanILL.Misc.VoidHnd KicImportClick;
		public event BscanILL.Misc.VoidHnd DiskImportClick;


		#region FastKeysControl()
		public FastKeysControl()
		{
			InitializeComponent();
		}
		#endregion


		#region Help_Click()
		private void Help_Click(object sender, EventArgs e)
		{
			if (HelpClick != null)
				HelpClick();
		}
		#endregion


		#region KICImport_Click()
		private void KICImport_Click(object sender, RoutedEventArgs e)
		{
			if (KicImportClick != null)
				KicImportClick();
		}
		#endregion

		#region DiskImport_Click()
		private void DiskImport_Click(object sender, RoutedEventArgs e)
		{
			if (DiskImportClick != null)
				DiskImportClick();
		}
		#endregion

	}
}
