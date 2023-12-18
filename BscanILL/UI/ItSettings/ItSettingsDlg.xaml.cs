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
using System.IO;

namespace BscanILL.UI.ItSettings
{
	/// <summary>
	/// Interaction logic for ItSettingsDlg.xaml
	/// </summary>
	public partial class ItSettingsDlg : Window
	{

		#region constructor
		public ItSettingsDlg()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public IntPtr Handle { get { return new System.Windows.Interop.WindowInteropHelper(this).Handle; } }

		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		BscanILL.SETTINGS.Settings _settings { get { return BscanILL.SETTINGS.Settings.Instance; } }

		#endregion



		//PRIVATE METHODS
		#region private methods

		#region SaveSettings()
		private bool SaveSettings()
		{
			try
			{
				Lock();

				this.panelContentLocator.ApplySettings();
				this.panelImageDependency.ApplySettings();
				this.panelSkew.ApplySettings();
				this.panelBookfold.ApplySettings();
				this.panelFingerRemoval.ApplySettings();

				_settings.Save();
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL Settings", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region Ok_Click()
		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			if (SaveSettings())
				this.DialogResult = true;
		}
		#endregion

		#region OnSourceInitialized()
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
		}
		#endregion

		#region Lock()
		public void Lock()
		{
			this.IsEnabled = false;
			this.Cursor = Cursors.Wait;
		}
		#endregion

		#region UnLock()
		public void UnLock()
		{
			this.IsEnabled = true;
			this.Cursor = null;
		}
		#endregion

		#endregion

	}
}
