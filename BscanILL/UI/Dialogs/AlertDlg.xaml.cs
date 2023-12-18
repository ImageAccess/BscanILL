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
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace BscanILL.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for AlertDlg.xaml
	/// </summary>
	public partial class AlertDlg : Window
	{
		AlertDlgType alertType = AlertDlgType.Information;


		#region constructor
		private AlertDlg()
		{
			InitializeComponent();

			this.MaxHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - 50;
		}
		#endregion


		#region AlertDlgType
		public enum AlertDlgType
		{
			Information,
			Question,
			Warning,
			Error
		}
		#endregion

		#region AlertText
		internal string AlertText
		{
			get { return this.textBlock.Text; }
			set { this.textBlock.Text = value; }
		}
		#endregion

		#region AlertType
		internal AlertDlgType AlertType
		{
			get { return this.alertType; }
			set
			{
				this.alertType = value;

				switch (this.alertType)
				{
					case AlertDlgType.Information:
						this.iconInfo.Visibility = Visibility.Visible;
						this.iconQuestion.Visibility = Visibility.Hidden;
						this.iconWarning.Visibility = Visibility.Hidden;
						this.iconError.Visibility = Visibility.Hidden;

						this.buttonOk.Visibility = Visibility.Visible;
						this.buttonYes.Visibility = Visibility.Hidden;
						this.buttonNo.Visibility = Visibility.Hidden;
						break;
					case AlertDlgType.Question:
						this.iconInfo.Visibility = Visibility.Hidden;
						this.iconQuestion.Visibility = Visibility.Visible;
						this.iconWarning.Visibility = Visibility.Hidden;
						this.iconError.Visibility = Visibility.Hidden;

						this.buttonOk.Visibility = Visibility.Hidden;
						this.buttonYes.Visibility = Visibility.Visible;
						this.buttonNo.Visibility = Visibility.Visible;
						break;
					case AlertDlgType.Warning:
						this.iconInfo.Visibility = Visibility.Hidden;
						this.iconQuestion.Visibility = Visibility.Hidden;
						this.iconWarning.Visibility = Visibility.Visible;
						this.iconError.Visibility = Visibility.Hidden;

						this.buttonOk.Visibility = Visibility.Visible;
						this.buttonYes.Visibility = Visibility.Hidden;
						this.buttonNo.Visibility = Visibility.Hidden;
						break;
					case AlertDlgType.Error:
						this.iconInfo.Visibility = Visibility.Hidden;
						this.iconQuestion.Visibility = Visibility.Hidden;
						this.iconWarning.Visibility = Visibility.Hidden;
						this.iconError.Visibility = Visibility.Visible;

						this.buttonOk.Visibility = Visibility.Visible;
						this.buttonYes.Visibility = Visibility.Hidden;
						this.buttonNo.Visibility = Visibility.Hidden;
						break;
				}
			}
		}
		#endregion

		// PUBLIC METHODS
		#region public methods

		#region Show()
		public static bool Show(string message, AlertDlgType alertType)
		{
			return Show(null, message, alertType);
		}

		public static bool Show(System.Windows.Window mainForm, string message, AlertDlgType alertType, bool getTimeoutAsYes)
		{
			return Show(mainForm, message, alertType);
		}

		public static bool Show(System.Windows.Window mainForm, string message, AlertDlgType alertType)
		{
			AlertDlg dlg = new AlertDlg();

			dlg.AlertText = message;
			dlg.AlertType = alertType;
			dlg.Owner = mainForm;

			bool? result = dlg.ShowDialog();

			if (result.HasValue)
				return result.Value;
			else
				return false;
		}
		#endregion


		#endregion



		//PRIVATE METHODS
		#region private methods

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (sender == this.buttonNo)
				this.DialogResult = false;
			else
				this.DialogResult = true;
		}

		private void Form_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.Left = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - this.ActualWidth) / 2;
			this.Top = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - this.ActualHeight) / 2;
		}

		private void Form_Loaded(object sender, RoutedEventArgs e)
		{
			this.Activate();

			this.Left = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - this.ActualWidth) / 2;
			this.Top = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - this.ActualHeight) / 2;
		}

		private void Form_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if(this.IsVisible)
				this.Activate();
		}

		private void Form_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter || e.Key == Key.Return)
				this.DialogResult = true;
			else if (e.Key == Key.Escape)
				this.DialogResult = true;
		}

		#endregion

	}
}
