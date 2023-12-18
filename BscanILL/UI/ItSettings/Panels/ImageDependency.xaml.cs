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
	/// Interaction logic for ImageDependency.xaml
	/// </summary>
	public partial class ImageDependency : PanelBase
	{
		double offsetHorizontal;
		double offsetVertical;
		bool autoImageDependency;

		#region constructor
		public ImageDependency()
		{
			InitializeComponent();

			this.OffsetHorizontal = _settings.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyHorizontalToleranceInches;
			this.OffsetVertical = _settings.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyVerticalToleranceInches;
			this.AutoImageDependency = _settings.ImageTreatment.AutoImageTreatment.ImageDependency.SetDependencyAutomatically;

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region AutoImageDependency
		public bool AutoImageDependency
		{
			get { return autoImageDependency; }
			set
			{
				autoImageDependency = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region OffsetHorizontal
		public double OffsetHorizontal
		{
			get { return offsetHorizontal; }
			set
			{
				offsetHorizontal = Math.Max(0, Math.Min(2, value));
				textLabelHorizontal.Text = string.Format("{0:0.00} inches", this.offsetHorizontal);
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region OffsetVertical
		public double OffsetVertical
		{
			get { return offsetVertical; }
			set
			{
				offsetVertical = Math.Max(0, Math.Min(2, value));
				textLabelVertical.Text = string.Format("{0:0.00} inches", this.offsetVertical);
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
			_settings.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyHorizontalToleranceInches = this.OffsetHorizontal;
			_settings.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyVerticalToleranceInches = this.OffsetVertical;
			_settings.ImageTreatment.AutoImageTreatment.ImageDependency.SetDependencyAutomatically = this.AutoImageDependency;
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods
		#endregion

	}
}

