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

namespace BscanILL.UI.Frames.ItResults
{
	/// <summary>
	/// Interaction logic for ControlPanel.xaml
	/// </summary>
	public partial class ControlPanel : UserControl
	{
		public event BscanILL.Misc.VoidHnd		DoneClick;

		public event BscanILL.Misc.VoidHnd		ItSettingsClick;
		public event BscanILL.Misc.VoidHnd		ChangeDependencyClick;
		public event BscanILL.Misc.VoidHnd		ResetCurrentClick;
		public event BscanILL.Misc.VoidHnd		ResetAllClick;
		public event BscanILL.Misc.VoidHnd		HelpClick;

		public event BscanILL.Misc.VoidHnd		SkipItClick;


		#region ControlPanel()
		public ControlPanel()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties


		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Reset()
		public void Reset()
		{
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Settings_Click()
		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			if (ItSettingsClick != null)
				ItSettingsClick();
		}
		#endregion

		#region ChangeDependency_Click()
		private void ChangeDependency_Click(object sender, RoutedEventArgs e)
		{
			if (ChangeDependencyClick != null)
				ChangeDependencyClick();
		}
		#endregion

		#region ResetAllSettings_Click()
		private void ResetAllSettings_Click(object sender, RoutedEventArgs e)
		{
			if (this.ResetAllClick != null)
				this.ResetAllClick();
		}
		#endregion

		#region ResetCurrentSettings_Click()
		private void ResetCurrentSettings_Click(object sender, RoutedEventArgs e)
		{
			if (this.ResetCurrentClick != null)
				this.ResetCurrentClick();
		}
		#endregion

		#region Help_Click()
		private void Help_Click(object sender, RoutedEventArgs e)
		{
			if (HelpClick != null)
				HelpClick();
		}
		#endregion

		private void UndoImageChange_Click(object sender, RoutedEventArgs e)
		{

		}

		#region Done_Click()
		private void Done_Click(object sender, RoutedEventArgs e)
		{
			if (this.DoneClick != null)
				this.DoneClick();
		}
		#endregion

		#region SkipIt_Click()
		private void SkipIt_Click(object sender, RoutedEventArgs e)
		{
			if (this.SkipItClick != null)
				this.SkipItClick();
		}
		#endregion

		#endregion

	}
}
