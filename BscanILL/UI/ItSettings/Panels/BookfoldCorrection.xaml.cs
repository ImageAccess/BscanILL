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
using System.IO;
using System.Security;

namespace BscanILL.UI.ItSettings.Panels
{
	public partial class BookfoldCorrection : PanelBase
	{

		
		#region constructor
		public BookfoldCorrection()
		{
			InitializeComponent();

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region ArticleExchangeEnabled
		/*public bool ArticleExchangeEnabled
		{
			get { return _settings.Export.ArticleExchange.Enabled; }
			set
			{
				_settings.Export.ArticleExchange.Enabled = value;
				this.groupBox.Visibility = (value) ? Visibility.Visible : Visibility.Hidden;
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
