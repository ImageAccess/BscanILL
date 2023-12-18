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
using System.Collections.ObjectModel;
using System.Reflection;

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for CloudStorage.xaml
	/// </summary>
	public partial class CloudAndSmartDock : PanelBase
	{


		#region constructor
		public CloudAndSmartDock()
		{
			InitializeComponent();

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region GoogleDocsEnabled
		public bool GoogleDocsEnabled
		{
			get { return _settings.Export.Cloud.GoogleDocsEnabled; }
			set
			{
				_settings.Export.Cloud.GoogleDocsEnabled = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region QrEnabled
		public bool QrEnabled
		{
			get { return _settings.Export.Cloud.QrCodeEnabled; }
			set
			{
				_settings.Export.Cloud.QrCodeEnabled = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods	
		#endregion


		//PRIVATE METHODS
		#region private methods

		#endregion

	}
}
