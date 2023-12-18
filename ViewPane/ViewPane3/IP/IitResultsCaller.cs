using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace ViewPane.IP
{
	internal interface IitResultsCaller
	{
		void ThumbnailCreatedDelegate(Bitmap bitmap, int renderingId);
		void ThumbnailCanceledDelegate();
		void ThumbnailErrorDelegate(Exception ex);
	}
}
