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
using System.ComponentModel;

namespace BscanILL.UI.Settings.ClickWizard
{
	/// <summary>
	/// Interaction logic for ClickDlg.xaml
	/// </summary>
	public partial class ClickDlg : Window
	{


		#region constructor
		public ClickDlg()
		{
			InitializeComponent();

			this.mainPanel.Init(Scanners.Clicks.ClickWrapper.AvailableSerialPorts, BscanILL.Settings.Settings.Instance.Scanner.ClickScanner.General.ComPort);

			Init();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public bool			CanInvoke	{ get { return true; } }
		public MainPanel	Control		{ get { return this.mainPanel; } }
	
		#endregion

		// PRIVATE PROPERTIES
		#region private properties

		BscanILL.Settings.Settings settings = BscanILL.Settings.Settings.Instance;

		#endregion




		// PUBLIC METHODS
		#region public methods

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


		// PRIVATE METHODS
		#region private methods

		#region Form_Load()
		private void Form_Loaded(object sender, RoutedEventArgs e)
		{
			//CanInvoke = true;
			//this.rebelScanner.RawImage = true;
		}
		#endregion

		#region Form_Closing()
		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{		
			//CanInvoke = false;
			
			//RebelV.Properties.LightsAdjuster.Default.Save();
			//Camera_UnRegisterEvents();
			//this.rebelScanner.RawImage = false;
			Dispose();
		}
		#endregion

		#endregion

	}
}
