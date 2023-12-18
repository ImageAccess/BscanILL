using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ViewPane.Misc
{
	internal class Misc
	{
		static object locker = new object();

		[System.Runtime.InteropServices.DllImport ("gdi32.dll")]
		private static extern bool DeleteObject (IntPtr hObject);

		#region GetBitmapSource()
		public static BitmapSource GetBitmapSource (System.Drawing.Bitmap bitmap)
		{
			lock (locker)
			{
				int width = 666;
				try
				{
					width = bitmap.Width;

					IntPtr hBitmap = bitmap.GetHbitmap();
					BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					DeleteObject(hBitmap);
					bitmap.Dispose();

					return bitmapSource;
				}
				catch (Exception ex)
				{
#if DEBUG
					Console.WriteLine(ex.Message + ". " + width);
					return null;
#else
					throw ex;
#endif
				}
			}
		}
		#endregion GetBitmapSource()

		#region IsFileAvailable()
		public static bool IsFileAvailable (string path)
		{
			// try and read the file...
			int count = 0;

			while (count <= 400)
			{
				count = count + 10;
				try
				{
					using (FileStream fs = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						fs.Close ();
					}

					return true;
				}
				catch (IOException)// ex)
				{
					//ex = ex;
					System.Threading.Thread.Sleep (count);
				}
				catch (UnauthorizedAccessException)
				{
					System.Threading.Thread.Sleep (count);
				}
			}

			return false;
		}
		#endregion IsFileAvailable()

		#region GetErrorMessage()
		/// <summary>
		/// returns error message lines including inner exceptions devided by new line characters, most significant error on the top
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		public static string GetErrorMessage(Exception ex)
		{
			string message = ex.Message;

			while ((ex = ex.InnerException) != null)
			{
				message += Environment.NewLine + ex.Message;
			}

			return message;
		}
		#endregion
	}
}