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

namespace BscanILL.UI.Settings.ClickWizard
{
	/// <summary>
	/// Interaction logic for PanelInitResult.xaml
	/// </summary>
	public partial class PanelWhiteBalance : UserControl, INotifyPropertyChanged
	{
		BscanILL.Settings.Settings									settings = BscanILL.Settings.Settings.Instance;
		public event PropertyChangedEventHandler				PropertyChanged;
		List<MainPanel.ComboApertureItem>						apertureItems;

		public event BscanILL.UI.Settings.ClickWizard.ClickHnd		BackClick;
		public event BscanILL.UI.Settings.ClickWizard.WhiteBalanceHnd FindClick;
		public event BscanILL.UI.Settings.ClickWizard.ClickHnd		SkipClick;


		#region constructor
		public PanelWhiteBalance()
		{
			InitializeComponent();

			apertureItems = MainPanel.GetApertureItems();

			foreach (MainPanel.ComboApertureItem apertureItem in apertureItems)
				this.comboAperture.Items.Add(apertureItem);

			foreach (MainPanel.ComboApertureItem apertureItem in apertureItems)
				if (apertureItem.Value == settings.Scanner.ClickScanner.Defaults.DefaultAv)
					this.comboAperture.SelectedItem = apertureItem;
		}
		#endregion

		// PUBLIC PROPERTIES
		#region public properties

		#region ApertureItems
		/*public List<MainPanel.ComboApertureItem> ApertureItems
		{
			get
			{
				return comboApertureItems;
			}
			set
			{
				this.comboApertureItems = value;
				RaisePropertyChanged(System.Reflection.MethodInfo.GetCurrentMethod().Name.Substring(4));
			}
		}*/
		#endregion

		#region SelectedApertureItem
		/*public MainPanel.ComboApertureItem SelectedApertureItem
		{
			get
			{
				foreach (MainPanel.ComboApertureItem apertureItem in this.ApertureItems)
					if (apertureItem.Value == settings.Scanner.ClickScanner.General.DefaultAv)
						return apertureItem;

				return null;
			}
			set
			{
				if (value != null)
				{
					settings.Scanner.ClickScanner.General.DefaultAv = value.Value;
					RaisePropertyChanged(System.Reflection.MethodInfo.GetCurrentMethod().Name.Substring(4));
				}
			}
		}*/
		#endregion

		#endregion

		// PUBLIC METHODS
		#region public methods


		#endregion

		// PRIVATE METHODS
		#region private methods

		#region Back_Click()
		private void Back_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BackClick();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Find_Click()
		private void Find_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if(this.comboAperture.SelectedItem != null)
					FindClick(((MainPanel.ComboApertureItem)this.comboAperture.SelectedItem).Value);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region RaisePropertyChanged()
		public void RaisePropertyChanged(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}
		#endregion

		#region Form_IsVisibleChanged()
		private void Form_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.IsVisible)
			{
				foreach (MainPanel.ComboApertureItem apertureItem in apertureItems)
					if (apertureItem.Value == settings.Scanner.ClickScanner.Defaults.DefaultAv)
						this.comboAperture.SelectedItem = apertureItem;
			}
		}
		#endregion

		private void Skip_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SkipClick();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	
		#endregion




	}
}
