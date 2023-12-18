﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace BscanILL.IP
{
	class Resizing
	{
		//PUBLIC METHODS
		#region public methods

		#region GetBluryPreview()
		public static Bitmap GetBluryPreview(ImageProcessing.BigImages.ItDecoder itDecoder, Rectangle clip, PixelFormat resultFormat)
		{
			try
			{
				if (clip.IsEmpty)
					clip = new Rectangle(0, 0, itDecoder.Width, itDecoder.Height);
				else
					clip.Intersect(new Rectangle(0, 0, itDecoder.Width, itDecoder.Height));

				int stripHeightMax = ImageProcessing.Misc.GetStripHeightMax(itDecoder);

				if (stripHeightMax < clip.Height)
				{
					List<Bitmap> bitmapsToMerge = new List<Bitmap>();

					for (int sourceTopLine = clip.Y; sourceTopLine < clip.Bottom; sourceTopLine += stripHeightMax)
					{
						try
						{
							int stripHeight = Math.Min(stripHeightMax, clip.Bottom - sourceTopLine);

							// if little chunk is left, add it to the current strip.
							if (sourceTopLine + stripHeight > clip.Height - 20)
								stripHeight = clip.Bottom - sourceTopLine;


							Bitmap resize = null;

							using (Bitmap strip = itDecoder.GetClip(new Rectangle(clip.X, sourceTopLine, clip.Width, stripHeight)))
							{
								resize = GetBluryPreview(strip, Rectangle.Empty, resultFormat);
								itDecoder.ReleaseAllocatedMemory(strip);
							}

							bitmapsToMerge.Add(resize);
						}
						finally
						{
							itDecoder.ReleaseAllocatedMemory(null);
						}
					}

					Bitmap merge = ImageProcessing.Merging.MergeVertically(bitmapsToMerge);

					foreach (Bitmap b in bitmapsToMerge)
						b.Dispose();

					if (itDecoder.DpiX > 0)
						merge.SetResolution(itDecoder.DpiX / 8F, itDecoder.DpiY / 8F);
					return merge;
				}
				else
				{
					try
					{
						Bitmap resampled;

						using (Bitmap strip = itDecoder.GetClip(clip))
						{
							resampled = GetBluryPreview(strip, Rectangle.Empty, resultFormat);
							itDecoder.ReleaseAllocatedMemory(strip);
						}

						if (itDecoder.DpiX > 0)
							resampled.SetResolution(itDecoder.DpiX / 8F, itDecoder.DpiY / 8F);

						return resampled;
					}
					finally
					{
						itDecoder.ReleaseAllocatedMemory(null);
					}
				}
			}
			finally
			{
			}
		}
		#endregion

		#region GetBluryPreview()
		public static Bitmap GetBluryPreview(Bitmap bmpSource, Rectangle clip, PixelFormat resultFormat)
		{
			if (bmpSource == null)
				return null;

			if (clip.IsEmpty)
				clip = new Rectangle(0, 0, bmpSource.Width, bmpSource.Height);
			else
				clip = System.Drawing.Rectangle.Intersect(clip, new System.Drawing.Rectangle(0, 0, bmpSource.Width, bmpSource.Height));

			Bitmap bmpResult = ResizeInternal(bmpSource, clip, resultFormat);

			if (bmpResult != null)
			{
				bmpResult.SetResolution((float)(bmpSource.HorizontalResolution / 8), (float)(bmpSource.VerticalResolution / 8));

				if (bmpResult.PixelFormat == PixelFormat.Format8bppIndexed)
					bmpResult.Palette = ImageProcessing.Misc.GetGrayscalePalette();
			}

			return bmpResult;
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region ResizeInternal()
		private static Bitmap ResizeInternal(Bitmap source, Rectangle clip, PixelFormat resultFormat)
		{
			if (source == null)
				return null;

			Bitmap result = null;
			BitmapData sourceData = null;
			BitmapData resultData = null;

			int x, y;

			try
			{
				int sourceW = clip.Width;
				int sourceH = clip.Height;
				int resultW = clip.Width / 8;
				int resultH = clip.Height / 8;

				if (resultFormat == PixelFormat.Format24bppRgb)
					result = new Bitmap(resultW, resultH, resultFormat);
				else
					result = new Bitmap(resultW, resultH, PixelFormat.Format8bppIndexed);

				sourceData = source.LockBits(clip, ImageLockMode.ReadOnly, source.PixelFormat);
				resultData = result.LockBits(new Rectangle(0, 0, resultW, resultH), ImageLockMode.WriteOnly, result.PixelFormat);

				int strideS = sourceData.Stride;
				int strideR = resultData.Stride;
				int r, g, b;


				unsafe
				{
					byte* scan0S = (byte*)sourceData.Scan0.ToPointer();
					byte* scan0R = (byte*)resultData.Scan0.ToPointer();
					byte* currentR;

					if (resultFormat == PixelFormat.Format24bppRgb)
					{
						for (y = 0; y < resultH; y++)
						{
							currentR = scan0R + y * strideR;

							for (x = 0; x < resultW; x++)
							{
								r = scan0S[(y * 8 + 4) * strideS + (x * 8 + 4) * 3] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 2) * 3] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 6) * 3] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 2) * 3] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 6) * 3];
								g = scan0S[(y * 8 + 4) * strideS + (x * 8 + 4) * 3 + 1] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 2) * 3 + 1] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 6) * 3 + 1] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 2) * 3 + 1] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 6) * 3 + 1];
								b = scan0S[(y * 8 + 4) * strideS + (x * 8 + 4) * 3 + 2] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 2) * 3 + 2] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 6) * 3 + 2] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 2) * 3 + 2] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 6) * 3 + 2];

								*(currentR++) = (byte)(r / 5);
								*(currentR++) = (byte)(g / 5);
								*(currentR++) = (byte)(b / 5);
							}
						}
					}
					else if (resultFormat == PixelFormat.Format8bppIndexed)
					{
						for (y = 0; y < resultH; y++)
						{
							currentR = scan0R + y * strideR;

							for (x = 0; x < resultW; x++)
							{
								r = scan0S[(y * 8 + 4) * strideS + (x * 8 + 4) * 3] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 2) * 3] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 6) * 3] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 2) * 3] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 6) * 3];
								g = scan0S[(y * 8 + 4) * strideS + (x * 8 + 4) * 3 + 1] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 2) * 3 + 1] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 6) * 3 + 1] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 2) * 3 + 1] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 6) * 3 + 1];
								b = scan0S[(y * 8 + 4) * strideS + (x * 8 + 4) * 3 + 2] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 2) * 3 + 2] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 6) * 3 + 2] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 2) * 3 + 2] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 6) * 3 + 2];

								*(currentR++) = ImageProcessing.Histogram.ToGray((r / 5), (g / 5), (b / 5));
							}
						}
					}
					else
					{
						int whitePixelsCount;
						//int threshold = histogram.ThresholdR + histogram.ThresholdG + histogram.ThresholdB;
						int threshold = 128 + 128 + 128;

						for (y = 0; y < resultH; y++)
						{
							currentR = scan0R + y * strideR;

							for (x = 0; x < resultW; x++)
							{
								whitePixelsCount = 0;

								if (scan0S[(y * 8 + 4) * strideS + (x * 8 + 4) * 3] + scan0S[(y * 8 + 4) * strideS + (x * 8 + 4) * 3 + 1] + scan0S[(y * 8 + 4) * strideS + (x * 8 + 4) * 3 + 2] > threshold)
									whitePixelsCount++;
								if (scan0S[(y * 8 + 2) * strideS + (x * 8 + 2) * 3] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 2) * 3 + 1] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 4) * 3 + 2] > threshold)
									whitePixelsCount++;
								if (scan0S[(y * 8 + 2) * strideS + (x * 8 + 6) * 3] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 6) * 3 + 1] + scan0S[(y * 8 + 2) * strideS + (x * 8 + 6) * 3 + 2] > threshold)
									whitePixelsCount++;
								if (scan0S[(y * 8 + 6) * strideS + (x * 8 + 2) * 3] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 2) * 3 + 1] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 2) * 3 + 2] > threshold)
									whitePixelsCount++;
								if (scan0S[(y * 8 + 6) * strideS + (x * 8 + 6) * 3] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 6) * 3 + 1] + scan0S[(y * 8 + 6) * strideS + (x * 8 + 6) * 3 + 2] > threshold)
									whitePixelsCount++;

								*(currentR++) = (byte)((whitePixelsCount / 5.0) * 255);
							}
						}
					}
				}

				return result;
			}
			finally
			{
				if (source != null && sourceData != null)
					source.UnlockBits(sourceData);
				if (result != null && resultData != null)
					result.UnlockBits(resultData);
			}
		}
		#endregion

		#endregion
	}
}
