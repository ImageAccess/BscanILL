using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.UI.Controls.ScannerControls
{
	public interface IScannerControl
	{
		double					Brightness	{ get; }
		double					Contrast	{ get; }
		ushort					Dpi			{ get; }

		Scanners.ColorMode		ColorMode	{ get; }
		Scanners.FileFormat		FileFormat	{ get; }
	}
}
