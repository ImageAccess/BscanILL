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

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for SharedDisk.xaml
	/// </summary>
	public partial class SharedDisk : PanelBase
	{

		#region constructor
		public SharedDisk()
		{
			InitializeComponent();

			this.textPath.IsEnabled = _settings.Export.SharedDisk.Enabled;
			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region SharedDiskEnabled
		public bool SharedDiskEnabled
		{
			get { return _settings.Export.SharedDisk.Enabled; }
			set
			{
				_settings.Export.SharedDisk.Enabled = value;
				this.textPath.IsEnabled = _settings.Export.SharedDisk.Enabled;
				RaisePropertyChanged("SharedDiskEnabled");
			}
		}
		#endregion

		#region SharedDiskPath
		public string SharedDiskPath
		{
			get { return _settings.Export.SharedDisk.Path; }
			set
			{
				_settings.Export.SharedDisk.Path = value;
				RaisePropertyChanged("SharedDiskPath");
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
