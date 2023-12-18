using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.FileSystem
{
	class ImportedImage
	{
		public string				File { get; private set; }
		public Scanners.ColorMode	ColorMode { get; private set; }
		public Scanners.FileFormat	FileFormat { get; private set; }
		public ushort				Dpi { get; private set; }
		public bool					DeleteAfterImport { get; set; }

		public ImportedImage(string file, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, ushort dpi, bool deleteAfterImport)
		{
			this.File = file;
			this.ColorMode = colorMode;
			this.FileFormat = fileFormat;
			this.Dpi = dpi;
			this.DeleteAfterImport = deleteAfterImport;
		}

	}
}
