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
	/// Interaction logic for PanelScanner.xaml
	/// </summary>
	public partial class ContentLocation : PanelBase
	{
		double offset ;
	
		#region constructor
		public ContentLocation()
		{
			InitializeComponent();

			this.Offset = _settings.ImageTreatment.AutoImageTreatment.ContentLocation.ContentOffsetInches;

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Offset
		public double Offset
		{
			get { return offset; }
			set
			{
				offset = Math.Max(0, Math.Min(2, value));
				textLabel.Text = string.Format("{0:0.00} inches", this.offset);
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			} 
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region ApplySettings()
		public override void ApplySettings()
		{
			_settings.ImageTreatment.AutoImageTreatment.ContentLocation.ContentOffsetInches = this.Offset;
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods
		#endregion

	}
}
