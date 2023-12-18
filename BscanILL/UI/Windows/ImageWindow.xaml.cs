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
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Threading;
using BscanILL.Hierarchy;

namespace BscanILL.UI.Windows
{
	/// <summary>
	/// Interaction logic for ImageWindow.xaml
	/// </summary>
	public partial class ImageWindow : Window
	{
		/*Bitmap		defaultImage;
		bool		closing = false;

		bool isUserGuideVideoActivated = false;

		delegate void InitializeRedraingHnd(System.Drawing.Bitmap bitmap);
		InitializeRedraingHnd dlgInitializeRedraing;
		BscanILL.Misc.VoidHnd dlgShowDefaultImage;*/


		#region constructor
		public ImageWindow()
		{
			InitializeComponent();

			/*this.dlgInitializeRedraing = delegate(System.Drawing.Bitmap bitmap)
			{
				DrawBitmapSource(bitmap);
			};

			defaultImage = BscanILL.Properties.Resources.BscanILLBigLogo;
			this.dlgShowDefaultImage += new BscanILL.Misc.VoidHnd(ShowDefaultImage);*/
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Visible
		public bool Visible
		{
			get { return this.Visibility == System.Windows.Visibility.Visible; }
			set { this.Visibility = (value) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden; }
		}
		#endregion

		#region Bounds
		/*public System.Drawing.Rectangle Bounds
		{
			set
			{
				this.Left = value.Left;
				this.Top = value.Top;
				this.Width = value.Width;
				this.Height = value.Height;
			}
		}*/
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region DrawBitmapSource()
		public void DrawBitmapSource(System.Drawing.Bitmap bitmap)
		{
			/*try
			{
				if (bitmap != null)
					imagePanel.ShowImage(bitmap);
				else
					imagePanel.ShowImage(this.defaultImage);
			}
			catch
			{
			}*/
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			/*this.closing = true;
			this.imagePanel.Dispose();
			Close();*/
		}
		#endregion

		#region ShowVpImage()
		public void ShowVpImage(ViewPane.Hierarchy.VpImage vpImage)
		{
			//this.imagePanel.ShowImage(vpImage);
		}
		#endregion

		#region ShowVpImageThreadSafe()
		public void ShowVpImageThreadSafe(ViewPane.Hierarchy.VpImage vpImage)
		{
			//this.Dispatcher.Invoke((Action)delegate() { ShowVpImage(vpImage); });
		}
		#endregion

		#region ShowDefaultImage()
		public void ShowDefaultImage()
		{
			//this.imagePanel.ShowImage(this.defaultImage);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Form_PreviewKeyDown()
		private void Form_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			/*if ((System.Windows.Input.Keyboard.Modifiers & ModifierKeys.Control) > 0 && e.Key == Key.T)
			{
				this.Visible = !this.Visible;
			}*/
		}
		#endregion

		#region Form_Loaded()
		private void Form_Loaded(object sender, RoutedEventArgs e)
		{
			/*if (BscanILL.UI.Misc.ScreensCount > 1)
			{
				System.Windows.Rect rect = BscanILL.UI.Misc.SecondaryScreenRect;

				this.Left = rect.Left;
				this.Top = rect.Top;
				this.Width = rect.Width;
				this.Height = rect.Height;

				this.WindowState = WindowState.Maximized;
			}
			else
			{
				System.Windows.Rect rect = BscanILL.UI.Misc.PrimaryScreenRect;

				this.Left = rect.Left;
				this.Top = rect.Top;
				this.Width = rect.Width;
				this.Height = rect.Height;

				this.Visibility = Visibility.Hidden;
			}

#if DEBUG
			this.Topmost = false;
#endif*/
		}
		#endregion

		#region Form_Closing()
		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			/*if (this.closing == false)
			{
				this.Hide();
				e.Cancel = true;
			}*/
		}
		#endregion

		#endregion
		
	}
}
