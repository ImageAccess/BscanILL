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

namespace BscanILL.UI.ItSettings.Panels
{
	/// <summary>
	/// Interaction logic for FingerRemoval.xaml
	/// </summary>
	public partial class FingerRemoval : PanelBase
	{

		#region FingerRemoval()
		public FingerRemoval()
		{
			InitializeComponent();
			
			this.DataContext = this;
		}
		#endregion



		//PUBLIC PROPERTIES
		#region public properties

		#region SmtpServer
		/*public string SmtpServer
		{
			get { return _settings.Export.Email.SmtpServer; }
			set
			{
				_settings.Export.Email.SmtpServer = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}*/
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region ApplySettings()
		public override void ApplySettings()
		{
		}
		#endregion

		#endregion

	}
}
