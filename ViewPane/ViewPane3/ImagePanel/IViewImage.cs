using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
//using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Text;

//using ImageProcessing;
//using ImageProcessing.ImageFile;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows;
using ViewPane.Hierarchy;



namespace ViewPane.ImagePanel
{
	internal interface IViewImage
	{
		//PUBLIC PROPERTIES
		bool								TwoPages { get; }

		//VpImage								VpImage { get; }
		ImageProcessing.IpSettings.ItImage	ItImage { get; }

		bool					IsFixed { get; set; }
		bool					IsIndependent { get; set; }

		ImageProcessing.ImageFile.ImageInfo FullImageInfo { get; }
		System.Windows.Size		FullImageSize { get; }

		float					Confidence { get; }


		//PUBLIC METHODS
		void			ReleaseBitmaps();
		//BitmapSource	GetBitmap(Rectangle clip, double zoom);
		void			Dispose();
		void			GetBitmapAsync(ViewPane.IP.IPreviewCaller caller, int renderingId, bool highPriority, int width, int height);
		void			GetBitmapAsync(ViewPane.IP.IPreviewCaller caller, int renderingId, bool highPriority, BIP.Geometry.RatioRect imageRect, int width, int height);

	}
}
