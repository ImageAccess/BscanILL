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

namespace ViewPane.Toolbar
{
	/// <summary>
	/// Interaction logic for ToolbarTreatment.xaml
	/// </summary>
	internal partial class ToolbarTreatment : ToolbarBase
	{
		public event EventHandler Rotate90Request;
		public event EventHandler Rotate180Request;
		public event EventHandler RotateCCWRequest;
		public event EventHandler RotateCWRequest;


		#region constructor
		public ToolbarTreatment()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#endregion


		//PUBLIC METHODS
		#region public methods

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Button_Click()
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (sender == buttonRotate90 && Rotate90Request != null)
				Rotate90Request(this, null);
			else if (sender == buttonRotate180 && Rotate180Request != null)
				Rotate180Request(this, null);
			else if (sender == buttonRotateCCW && RotateCCWRequest != null)
				RotateCCWRequest(this, null);
			else if (sender == buttonRotateCW && RotateCWRequest != null)
				RotateCWRequest(this, null);
		}
		#endregion

		#endregion

	}
}
