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
	/// Interaction logic for ToolbarPages.xaml
	/// </summary>
	internal partial class ToolbarPages : ToolbarBase
	{
		VisiblePage pages = VisiblePage.None;

		public event EventHandler ShowPageL;
		public event EventHandler ShowPageR;


		#region constructor
		public ToolbarPages()
		{
			InitializeComponent();

			this.buttonPage1.IsChecked = true;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public VisiblePage Pages
		{
			get { return pages; }
			set
			{
				this.pages = value;

				switch (value)
				{
					case VisiblePage.Left:
						buttonPage1.IsEnabled = true;
						buttonPage2.IsEnabled = true;
						buttonPage1.IsChecked = true;
						buttonPage2.IsChecked = false;
						break;
					case VisiblePage.Right:
						buttonPage1.IsEnabled = true;
						buttonPage2.IsEnabled = true;
						buttonPage1.IsChecked = false;
						buttonPage2.IsChecked = true;
						break;
					case VisiblePage.None:
						buttonPage1.IsEnabled = false;
						buttonPage2.IsEnabled = false;
						buttonPage1.IsChecked = false;
						buttonPage2.IsChecked = false;
						break;
				}
			}
		}
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Button_Click()
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (sender == buttonPage1)
			{
				this.Pages = VisiblePage.Left;

				if (this.ShowPageL != null)
					this.ShowPageL(this, null);
			}
			else
			{
				this.Pages = VisiblePage.Right;

				if (this.ShowPageR != null)
					this.ShowPageR(this, null);
			}
		}
		#endregion

		#endregion
	}
}
