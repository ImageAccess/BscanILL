using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.UI
{
	class Misc
	{

		public delegate void ImageSelectedHnd(BscanILL.Hierarchy.IllImage illImage);
		public delegate void ImageEventHandler(ImageEventArgs args);

		#region class ImageEventArgs
		public class ImageEventArgs
		{
			public BscanILL.Hierarchy.IllImage Image;

			public ImageEventArgs(BscanILL.Hierarchy.IllImage image)
			{
				this.Image = image;
			}
		}
		#endregion	
	

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);


		#region GetBitmapSource()
		public static System.Windows.Media.Imaging.BitmapSource GetBitmapSource(System.Drawing.Bitmap bitmap)
		{
			IntPtr hBitmap = bitmap.GetHbitmap();

			System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					hBitmap,
					IntPtr.Zero,
					System.Windows.Int32Rect.Empty,
					System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

			DeleteObject(hBitmap);

			return bitmapSource;
		}
		#endregion

		
		public static int ScreensCount { get { return System.Windows.Forms.Screen.AllScreens.Length; } }
		public static System.Windows.Rect PrimaryScreenRect { get { return new System.Windows.Rect(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height); } }

		#region SecondaryScreenRect
		public static System.Windows.Rect SecondaryScreenRect
		{
			get
			{
				if (ScreensCount > 1)
				{
					foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
					{
						if (screen.Primary == false)
							return new System.Windows.Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
					}
				}

				return PrimaryScreenRect;
			}
		}
		#endregion

	
	}
}
