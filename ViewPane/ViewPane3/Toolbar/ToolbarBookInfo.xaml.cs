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
	/// Interaction logic for ToolbarBookInfo.xaml
	/// </summary>
	internal partial class ToolbarBookInfo : ToolbarBase
	{
		public delegate void BookPartTypeHnd(ViewPane.IT.BookPartType bookPartType);
		public event BookPartTypeHnd BookPartTypeChanged;


		#region constructor
		public ToolbarBookInfo()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public ViewPane.IT.BookPartType BookPart
		{
			get
			{
				if ((comboBookPage.SelectedItem != null) && (comboBookPage.SelectedItem is ComboBoxItem))
					return (ViewPane.IT.BookPartType)((ComboBoxItem)comboBookPage.SelectedItem).Tag;
				else
					return ViewPane.IT.BookPartType.None;
			}
			set
			{
				foreach (object item in this.comboBookPage.Items)
				{
					if (item is ComboBoxItem)
					{
						ComboBoxItem comboItem = (ComboBoxItem)item;

						if ((ViewPane.IT.BookPartType)comboItem.Tag == value)
						{
							comboBookPage.SelectionChanged -= new SelectionChangedEventHandler(ComboBookPage_SelectionChanged);
							this.comboBookPage.SelectedItem = comboItem;
							comboBookPage.SelectionChanged += new SelectionChangedEventHandler(ComboBookPage_SelectionChanged);
							break;
						}
					}
				}
			}
		}

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region AdjustUi()
		public void AdjustUi(ViewPane.ZoomType type, double zoomValue)
		{
		}
		#endregion

		#region GetCaption()
		public static string GetCaption(ViewPane.IT.BookPartType bookPart)
		{
			switch (bookPart)
			{
				case ViewPane.IT.BookPartType.FrontCover: return ViewPane.Languages.UiStrings.FrontCover_STR;
				case ViewPane.IT.BookPartType.SpineCover: return ViewPane.Languages.UiStrings.SpineCover_STR;
				case ViewPane.IT.BookPartType.RearCover: return ViewPane.Languages.UiStrings.RearCover_STR;

				case ViewPane.IT.BookPartType.HeadEdge: return ViewPane.Languages.UiStrings.HeadEdge_STR;
				case ViewPane.IT.BookPartType.TailEdge: return ViewPane.Languages.UiStrings.TailEdge_STR;
				case ViewPane.IT.BookPartType.ForeEdge: return ViewPane.Languages.UiStrings.ForeEdge_STR;

				case ViewPane.IT.BookPartType.FrontEndPapers: return ViewPane.Languages.UiStrings.FrontEndPapers_STR;
				case ViewPane.IT.BookPartType.FrontPastdownEndPaper: return ViewPane.Languages.UiStrings.FrontPastdownEndPaper_STR;
				case ViewPane.IT.BookPartType.FrontFreeEndPaper: return ViewPane.Languages.UiStrings.FrontFreeEndPaper_STR;
				case ViewPane.IT.BookPartType.RearEndPapers: return ViewPane.Languages.UiStrings.RearEndPapers_STR;
				case ViewPane.IT.BookPartType.RearFreeEndPaper: return ViewPane.Languages.UiStrings.RearFreeEndPaper_STR;
				case ViewPane.IT.BookPartType.RearPastdownEndPaper: return ViewPane.Languages.UiStrings.RearPastdownEndPaper_STR;

				case ViewPane.IT.BookPartType.Normal: return ViewPane.Languages.UiStrings.Normal_STR;
				case ViewPane.IT.BookPartType.Other: return ViewPane.Languages.UiStrings.Other_STR;
				case ViewPane.IT.BookPartType.None: return ViewPane.Languages.UiStrings.None_STR;
				case ViewPane.IT.BookPartType.Unknown: return ViewPane.Languages.UiStrings.Unknown_STR;

				default: return "";
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region RadioSize_Checked
		void ComboBookPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if ((BookPartTypeChanged != null) && (comboBookPage.SelectedItem is ComboBoxItem))
					BookPartTypeChanged((ViewPane.IT.BookPartType)((ComboBoxItem)comboBookPage.SelectedItem).Tag);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#endregion

	}

}
