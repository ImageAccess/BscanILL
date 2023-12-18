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
using System.Windows.Shapes;

namespace BscanILL.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for DependencyDlg.xaml
	/// </summary>
	public partial class DependencyDlg : Window
	{
		DependencyDlg.Result dialogResult = DependencyDlg.Result.Cancel;

		#region constructor
		public DependencyDlg()
		{
			InitializeComponent();
		}
		#endregion

		#region struct DependencyParameters
		public struct DependencyParameters
		{
			public DependencyDlg.Result DialogResult;
			public SelectionRangeEnum	SelectionRange;
			public SelectionFilterEnum	SelectionFilter;
			public int					From;
			public int					To;

			public DependencyParameters(DependencyDlg.Result dialogResult, SelectionRangeEnum selectionRange, SelectionFilterEnum filter, int from, int to)
			{
				this.DialogResult = dialogResult;
				this.SelectionRange = selectionRange;
				this.SelectionFilter = filter;
				this.From = from;
				this.To = to;
			}
		}
		#endregion

		#region enum Result
		public enum Result
		{
			Dependent,
			Independent,
			Cancel
		}
		#endregion

		#region enum SelectionRangeEnum
		public enum SelectionRangeEnum
		{
			All,
			FromCurrentOn,
			Range
		}
		#endregion

		#region enum SelectionFilterEnum
		[FlagsAttribute]
		public enum SelectionFilterEnum
		{
			AllSizes = 1,
			Landscape = 2,
			Portrait = 4
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region ResultParameters
		public DependencyParameters ResultParameters
		{
			get
			{
				return new DependencyParameters(this.dialogResult, this.SelectionRange, this.SelectionFilter, this.RangeFrom, this.RangeTo);
			}
		}
		#endregion

		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		#region SelectionRange
		private SelectionRangeEnum SelectionRange
		{
			get
			{
				if (radioRangeFromCurrent.IsChecked.Value)
					return SelectionRangeEnum.FromCurrentOn;
				else if (radioRangeRange.IsChecked.Value)
					return SelectionRangeEnum.Range;
				else
					return SelectionRangeEnum.All;
			}
		}
		#endregion

		#region RangeFrom
		private int RangeFrom
		{
			get
			{
				int from;

				if (int.TryParse(textBlockFrom.Text, out from))
					return from - 1;
				else
					return 0;
			}
		}
		#endregion

		#region RangeTo
		private int RangeTo
		{
			get
			{
				int to;

				if (int.TryParse(textBlockTo.Text, out to))
					return to - 1;
				else
					return 99999;
			}
		}
		#endregion

		#region SelectionFilter
		private SelectionFilterEnum SelectionFilter
		{
			get
			{
				if (radioFilterPortrait.IsChecked.Value)
					return SelectionFilterEnum.Portrait;
				else if (radioFilterLandscape.IsChecked.Value)
					return SelectionFilterEnum.Landscape;
				else
					return SelectionFilterEnum.AllSizes;
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Open()
		public DependencyDlg.Result Open()
		{
			this.ShowDialog();

			return this.dialogResult;
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Cancel_Click()
		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.dialogResult = DependencyDlg.Result.Cancel;
			this.Close();
		}
		#endregion

		#region Dependent_Click()
		private void Dependent_Click(object sender, RoutedEventArgs e)
		{
			this.dialogResult = DependencyDlg.Result.Dependent;
			this.Close();
		}
		#endregion

		#region Independent_Click()
		private void Independent_Click(object sender, RoutedEventArgs e)
		{
			this.dialogResult = DependencyDlg.Result.Independent;
			this.Close();
		}
		#endregion

		#region PageRangeBox_TextChanged()
		private void PageRangeBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			radioRangeRange.IsChecked = true;
		}
		#endregion

		#region Form_Loaded()
		private void Form_Loaded(object sender, RoutedEventArgs e)
		{
			radioRangeAll.IsChecked = true;
		}
		#endregion

		#endregion

	}
}
