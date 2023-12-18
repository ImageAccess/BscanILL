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
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace BscanILL.UI.Settings.Controls
{
	/// <summary>
	/// Interaction logic for General.xaml
	/// </summary>
	public partial class PrintFunctionalityPanel : BscanILL.UI.Settings.Panels.PanelBase
	{

		public PrintFunctionalityPanel()
		{
			InitializeComponent();

			if (_settings != null)
			{
				_settings.Export.Printer.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
				{
					this.textFunctionalityCaption.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
				};
			}

			this.DataContext = this;
		}

		//PUBLIC PROPERTIES
		#region public properties

		#region FunctionalityCaption
		public string FunctionalityCaption
		{
			get
			{
				if (_settings.Export.Printer.PrintFunctionality == BscanILL.Export.Printing.Functionality.Xps)
					return "XPS (Windows Vista, 7, 8)";
				else
					return "Win32 (XP)";
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#endregion
	
		
		//PRIVATE METHODS
		#region private methods

		#region Change_Click()
		private void Change_Click(object sender, MouseButtonEventArgs e)
		{
			try
			{
				BscanILL.UI.Settings.Dialogs.PrintFunctionalityDlg dlg = new BscanILL.UI.Settings.Dialogs.PrintFunctionalityDlg();

				dlg.Open();
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
