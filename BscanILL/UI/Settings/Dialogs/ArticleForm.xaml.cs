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
using System.Data;

namespace BscanILL.UI.Settings.Dialogs
{
	/// <summary>
	/// Interaction logic for ArticleForm.xaml
	/// </summary>
	public partial class ArticleForm : Window
	{
		public ArticleForm()
		{
			InitializeComponent();
		}

		public class ArticleListViewItem
		{
			string columnName;
			string columnValue;

			public ArticleListViewItem(string columnName, string columnValue)
			{
				this.columnName = columnName;
				this.columnValue = columnValue;
			}

			public string ColumnName	{ get { return this.columnName; } }
			public string ColumnValue	{ get { return this.columnValue; } } 
		}

		internal void Open(DataRow row1, DataRow row2)
		{
			foreach (DataColumn column in row1.Table.Columns)
				listView.Items.Add(new ArticleListViewItem(column.ColumnName, row1[column.ColumnName].ToString()));

			if (row2 != null)
			{
				foreach (DataColumn column in row2.Table.Columns)
					listView2.Items.Add(new ArticleListViewItem(column.ColumnName, row2[column.ColumnName].ToString()));
			}

			ShowDialog();
		}
	}
}
