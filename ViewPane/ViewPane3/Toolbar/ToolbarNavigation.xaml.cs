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
	/// Interaction logic for ToolbarNavigation.xaml
	/// </summary>
	internal partial class ToolbarNavigation : ToolbarBase
	{
		public event EventHandler FirstClick;
		public event EventHandler PreviousClick;
		public event EventHandler NextClick;
		public event EventHandler LastClick;


		#region constructor
		public ToolbarNavigation()
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
			if (sender == buttonFirst && FirstClick != null)
				FirstClick(this, null);
			else if (sender == buttonPrevious && PreviousClick != null)
				PreviousClick(this, null);
			else if (sender == buttonNext && NextClick != null)
				NextClick(this, null);
			else if (sender == buttonLast && LastClick != null)
				LastClick(this, null);
		}
		#endregion

		#endregion


	}
}
