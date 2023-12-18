using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.UI.Dialogs.ExportDialog
{
	interface IPanel
	{
		BscanILL.Scan.FileFormat	FileFormat		{ get; }
		string						FileNamePrefix	{ get; }
		bool						MultiImage { get; }
		bool						PdfA { get; }

		BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo();
	}
}
