using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewPane.IP
{
	
	public interface IPreviewCaller
	{
		bool IsDisplayed { get; }

		void ThumbnailCreatedDelegate(System.Drawing.Bitmap bitmap, int renderingId, TimeSpan timeSpan);
		void ThumbnailCanceledDelegate();
		void ThumbnailErrorDelegate(Exception ex);
	}

}
