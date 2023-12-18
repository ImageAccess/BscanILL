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

namespace BscanILL.UI.Frames.Help
{
	/// <summary>
	/// Interaction logic for FrameHelpUi.xaml
	/// </summary>
	public partial class FrameHelpUi : UserControl
	{
		int pageToShow = 0;
		
		public event BscanILL.Misc.VoidHnd GoToStartClick;
		public event BscanILL.Misc.VoidHnd GoToScanClick;
		public event BscanILL.Misc.VoidHnd GoToItClick;

		public event BscanILL.Misc.VoidHnd ResendClick;


		#region constructor
		public FrameHelpUi()
		{
			InitializeComponent();
		}
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		int PageToShow { get { return pageToShow; } set { this.pageToShow = Math.Max(0, Math.Min(36, value)); } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Open()
		public void Open()
		{
			this.Visibility = System.Windows.Visibility.Visible;
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			this.PageToShow = 0;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Close_Click()
		private void Close_Click(object sender, RoutedEventArgs e)
		{
			if (GoToStartClick != null)
				GoToStartClick();
		}
		#endregion

		#region First_Click()
		private void First_Click(object sender, RoutedEventArgs e)
		{
			this.PageToShow = 0;
			ShowPages();
		}
		#endregion

		#region Previous_Click()
		private void Previous_Click(object sender, RoutedEventArgs e)
		{
			this.PageToShow --;
			ShowPages();
		}
		#endregion

		#region Next_Click()
		private void Next_Click(object sender, RoutedEventArgs e)
		{
			this.PageToShow++;
			ShowPages();
		}
		#endregion

		#region Last_Click()
		private void Last_Click(object sender, RoutedEventArgs e)
		{
			this.PageToShow = 100;
			ShowPages();
		}
		#endregion

		#region ShowPages()
		private void ShowPages()
		{
			if (this.PageToShow > 0)
			{
				BitmapImage leftImage = new BitmapImage();
				leftImage.BeginInit();

				switch (this.PageToShow)
				{
					case 01: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_02.jpg"); break;
					case 02: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_04.jpg"); break;
					case 03: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_06.jpg"); break;
					case 04: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_08.jpg"); break;
					case 05: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_10.jpg"); break;
					case 06: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_12.jpg"); break;
					case 07: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_14.jpg"); break;
					case 08: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_16.jpg"); break;
					case 09: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_18.jpg"); break;
					case 10: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_20.jpg"); break;
					case 11: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_22.jpg"); break;
					case 12: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_24.jpg"); break;
					case 13: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_26.jpg"); break;
					case 14: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_28.jpg"); break;
					case 15: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_30.jpg"); break;
					case 16: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_32.jpg"); break;
					case 17: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_34.jpg"); break;
					case 18: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_36.jpg"); break;
					case 19: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_38.jpg"); break;
					case 20: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_40.jpg"); break;
					case 21: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_42.jpg"); break;
					case 22: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_44.jpg"); break;
					case 23: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_46.jpg"); break;
					case 24: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_48.jpg"); break;
					case 25: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_50.jpg"); break;
					case 26: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_52.jpg"); break;
					case 27: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_54.jpg"); break;
					case 28: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_56.jpg"); break;
					case 29: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_58.jpg"); break;
					case 30: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_60.jpg"); break;
					case 31: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_62.jpg"); break;
					case 32: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_64.jpg"); break;
					case 33: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_66.jpg"); break;
					case 34: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_68.jpg"); break;
					case 35: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_70.jpg"); break;
					case 36: leftImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_72.jpg"); break;
				}

				leftImage.EndInit();
				imageL.Source = leftImage;
			}
			else
				imageL.Source = null;

			if (this.PageToShow <= 35)
			{
				BitmapImage rightImage = new BitmapImage();
				rightImage.BeginInit();

				switch (this.PageToShow)
				{
					case 00: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_01.jpg"); break;
					case 01: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_03.jpg"); break;
					case 02: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_05.jpg"); break;
					case 03: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_07.jpg"); break;
					case 04: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_09.jpg"); break;
					case 05: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_11.jpg"); break;
					case 06: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_13.jpg"); break;
					case 07: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_15.jpg"); break;
					case 08: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_17.jpg"); break;
					case 09: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_19.jpg"); break;
					case 10: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_21.jpg"); break;
					case 11: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_23.jpg"); break;
					case 12: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_25.jpg"); break;
					case 13: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_27.jpg"); break;
					case 14: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_29.jpg"); break;
					case 15: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_31.jpg"); break;
					case 16: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_33.jpg"); break;
					case 17: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_35.jpg"); break;
					case 18: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_37.jpg"); break;
					case 19: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_39.jpg"); break;
					case 20: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_41.jpg"); break;
					case 21: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_43.jpg"); break;
					case 22: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_45.jpg"); break;
					case 23: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_47.jpg"); break;
					case 24: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_49.jpg"); break;
					case 25: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_51.jpg"); break;
					case 26: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_53.jpg"); break;
					case 27: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_55.jpg"); break;
					case 28: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_57.jpg"); break;
					case 29: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_59.jpg"); break;
					case 30: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_61.jpg"); break;
					case 31: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_63.jpg"); break;
					case 32: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_65.jpg"); break;
					case 33: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_67.jpg"); break;
					case 34: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_69.jpg"); break;
					case 35: rightImage.UriSource = new Uri("pack://application:,,,/BscanILL;component/Images/Manual/BSCAN ILL User Guide Version 3_Page_71.jpg"); break;
				}

				rightImage.EndInit();
				imageR.Source = rightImage;
			}
			else
				imageR.Source = null;
		}
		#endregion

		#region GoToStart_Click()
		private void GoToStart_Click(object sender, RoutedEventArgs e)
		{
			if (GoToStartClick != null)
				GoToStartClick();
		}
		#endregion

		#region GoToScan_Click()
		private void GoToScan_Click(object sender, RoutedEventArgs e)
		{
			if (GoToScanClick != null)
				GoToScanClick();
		}
		#endregion

		#region GoToIt_Click()
		private void GoToIt_Click(object sender, RoutedEventArgs e)
		{
			if (GoToItClick != null)
				GoToItClick();
		}
		#endregion

		#region GoToResend_Click
		private void GoToResend_Click(object sender, RoutedEventArgs e)
		{
			if (ResendClick != null)
				ResendClick();

		}
		#endregion

		#endregion

	}
}
