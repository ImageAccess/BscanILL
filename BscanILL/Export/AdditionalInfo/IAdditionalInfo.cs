using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.AdditionalInfo
{
	public interface IAdditionalInfo
	{
		bool IncludePullslip { get; }
        BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get; }
        int FileQuality { get; }		
	}
}
