using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;

namespace Scanners
{
	public class Misc
	{
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

		#region GetColorMode()
		public static Scanners.ColorMode GetColorMode(ClickCommon.ColorMode colorMode)
		{
			switch (colorMode)
			{
				case ClickCommon.ColorMode.Bitonal: return ColorMode.Bitonal;
				case ClickCommon.ColorMode.Grayscale: return ColorMode.Grayscale;
				default: return ColorMode.Color;
			}
		}

		public static Scanners.ColorMode GetColorMode(Scanners.S2N.ColorMode colorMode)
		{
			switch (colorMode)
			{
				case Scanners.S2N.ColorMode.Lineart:
				case Scanners.S2N.ColorMode.Photo:
					return ColorMode.Bitonal;
				case Scanners.S2N.ColorMode.Grayscale: return ColorMode.Grayscale;
				default: return ColorMode.Color;
			}
		}

		public static Scanners.ColorMode GetColorMode(Scanners.Twain.ColorMode colorMode)
		{
			switch (colorMode)
			{
				case Scanners.Twain.ColorMode.Bitonal: return ColorMode.Bitonal;
				case Scanners.Twain.ColorMode.Grayscale: return ColorMode.Grayscale;
				default: return ColorMode.Color;
			}
		}

		public static ClickCommon.ColorMode GetColorMode(Scanners.ColorMode colorMode)
		{
			switch (colorMode)
			{
				case Scanners.ColorMode.Bitonal: return ClickCommon.ColorMode.Bitonal;
				case Scanners.ColorMode.Grayscale: return ClickCommon.ColorMode.Grayscale;
				default: return ClickCommon.ColorMode.Color;
			}
		}
		#endregion

		#region GetS2NColorMode()
		public static Scanners.S2N.ColorMode GetS2NColorMode(Scanners.ColorMode colorMode)
		{
			switch (colorMode)
			{
				case ColorMode.Bitonal: return Scanners.S2N.ColorMode.Lineart;
				case ColorMode.Grayscale: return Scanners.S2N.ColorMode.Grayscale;
				default: return Scanners.S2N.ColorMode.Color;
			}
		}
		#endregion

		#region GetErrorMessage()
		/// <summary>
		/// returns error message lines including inner exceptions devided by new line characters, most significant error on the top
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		internal static string GetErrorMessage(Exception ex)
		{
			string message = ex.Message;

			while ((ex = ex.InnerException) != null)
			{
				message += Environment.NewLine + ex.Message;
			}

			return message;
		}
		#endregion

		#region GetImageFormat()
		/*public static System.Drawing.Imaging.ImageFormat GetImageFormat(Image i)
		{
			foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
			{
				if (codec.FormatID == i.RawFormat.Guid)
				{
					if (codec.FilenameExtension.ToLower().Contains("bmp"))
						return ImageFormat.Bmp;
					if (codec.FilenameExtension.ToLower().Contains("png"))
						return ImageFormat.Png;
					if (codec.FilenameExtension.ToLower().Contains("tif"))
						return ImageFormat.Tiff;
					if (codec.FilenameExtension.ToLower().Contains("gif"))
						return ImageFormat.Gif;

					return ImageFormat.Jpeg;
				}
			}

			return ImageFormat.Jpeg;
		}*/

		public static FileFormat GetImageFormat(Guid formatId)
		{
			foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
			{
				if (codec.FormatID == formatId)
				{
					if (codec.FilenameExtension.ToLower().Contains("png"))
						return FileFormat.Png;
					if (codec.FilenameExtension.ToLower().Contains("tif"))
						return FileFormat.Tiff;

					return FileFormat.Jpeg;
				}
			}

			return FileFormat.Jpeg;
		}
		#endregion

		#region GetFileFormat()
		public static Scanners.FileFormat GetFileFormat(Scanners.S2N.FileFormat fileFormat)
		{
			switch (fileFormat)
			{
				case Scanners.S2N.FileFormat.Tiff: return FileFormat.Tiff;
				default: return FileFormat.Jpeg;
			}
		}

		public static Scanners.S2N.FileFormat GetFileFormat(Scanners.FileFormat fileFormat)
		{
			switch (fileFormat)
			{
				case FileFormat.Tiff: return Scanners.S2N.FileFormat.Tiff;
				default: return Scanners.S2N.FileFormat.Jpeg;
			}
		}
		#endregion

	}
}
