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
	public partial class PanelRegPoints : BscanILL.UI.Settings.Panels.PanelBase
	{
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd BackClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd FindClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd SkipClick;


		#region constructor
		public PanelRegPoints()
		{
			InitializeComponent();

			ClickDLL.Settings.Settings.Instance.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				//if (PropertyChanged != null)
				//	PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("RegistrationPointsBrush"));
				RaisePropertyChanged("get_RegistrationPointsBrush");
			};

			this.DataContext = this;
		}
		#endregion


		#region RegistrationPointsBrush
		public Brush RegistrationPointsBrush
		{
			get
			{
				if (this.settings.Scanner.ClickScanner == null)
					return new SolidColorBrush(Colors.White);
				else if (this.settings.Scanner.ClickScanner.IT.ItPageL.IsRegistrationPointsDefined == false)
					return new SolidColorBrush(Colors.Red);
				else
					return new SolidColorBrush(Colors.LightGreen);
			}
			set
			{
				//RaisePropertyChanged(System.Reflection.MethodInfo.GetCurrentMethod().Name.Substring(4));
			}
		}
		#endregion


		// PRIVATE METHODS
		#region private methods

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

		#endregion

	}
}
