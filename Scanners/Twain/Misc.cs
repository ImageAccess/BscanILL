using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.Twain
{
	class Misc
	{

		public static Scanners.Twain.FileFormat TwainFileFormatToScannerFileFormat(TwainApp.FileFormat twainFileFormat)
		{
			switch (twainFileFormat)
			{
				case TwainApp.FileFormat.TIFF:
				case TwainApp.FileFormat.TIFFMULTI:
					return Scanners.Twain.FileFormat.Tiff;
				case TwainApp.FileFormat.PNG:
					return Scanners.Twain.FileFormat.Png;
				default:
					return Scanners.Twain.FileFormat.Jpeg;
			}
		}
	}
}
