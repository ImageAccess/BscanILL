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
	/// Interaction logic for PanelInit.xaml
	/// </summary>
	public partial class PanelRegPointsResult : BscanILL.UI.Settings.Panels.PanelBase
	{
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd BackClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd ContinueClick;


		#region constructor()
		public PanelRegPointsResult()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#region Deskew
		public bool Deskew
		{
			get { return settings.Scanner.ClickScanner.IT.Deskew; }
			set
			{
				settings.Scanner.ClickScanner.IT.Deskew = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Apply()
		public void Apply(BitmapSource bitmapLTop, BitmapSource bitmapLBottom, BitmapSource bitmapRTop, BitmapSource bitmapRBottom, double skewL, double skewR)
		{
			this.imageLTop.Source = bitmapLTop;
			this.imageRTop.Source = bitmapRTop;
			this.imageLBottom.Source = bitmapLBottom;
			this.imageRBottom.Source = bitmapRBottom;

			this.textSkewL.Text = skewL.ToString("0.0") + " °";
			this.textSkewL.Foreground = (skewL > -1 && skewL < 1) ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Red);
			this.textSkewR.Text = skewR.ToString("0.0") + " °";
			this.textSkewR.Foreground = (skewR > -1 && skewR < 1) ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Red);
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
