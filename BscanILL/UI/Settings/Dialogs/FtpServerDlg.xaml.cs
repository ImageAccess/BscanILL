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

namespace BscanILL.UI.Settings.Dialogs
{
	/// <summary>
	/// Interaction logic for FtpServerDlg.xaml
	/// </summary>
	public partial class FtpServerDlg : Window
	{
		
		public FtpServerDlg()
		{
			InitializeComponent();

			this.DataContext = this;
			this.ftpLoginControl.TextChanged += new BscanILL.Misc.VoidHnd(FtpLoginControl_TextChanged);
		}

		public BscanILL.Export.FTP.FtpLogin Open()
		{
			this.ftpLoginControl.Clear();
			
			if (ShowDialog() == true)
			{
				return ftpLoginControl.FtpLogin;;
			}

			else return null;
		}

		public bool Open(BscanILL.Export.FTP.FtpLogin ftpProfile)
		{
			this.ftpLoginControl.Load(ftpProfile);

			if (ShowDialog() == true)
			{
				this.ftpLoginControl.CopyTo(ftpProfile);
				return true;
			}

			return false;
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		void FtpLoginControl_TextChanged()
		{
			this.buttonOk.IsEnabled = this.ftpLoginControl.IsValidationOk;
		}

	}
}
