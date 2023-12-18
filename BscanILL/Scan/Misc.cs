using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Scan
{

	#region enum FileFormat
	public enum FileFormat : byte
	{
		Jpeg = Scanners.FileFormat.Jpeg,
		Tiff = Scanners.FileFormat.Tiff,
		Pdf = Scanners.FileFormat.Png,
		Png = 8,
		SPdf = 16,
		Text = 32,
		Audio = 64,
		Auto = 128,
		Unknown = 0XFF
	}
	#endregion

	#region enum CurrentState
	[Serializable]
	public enum CurrentState
	{
		Scan,
		Modify,
		Review,
		Export,
		UserGuide
	}
	#endregion

	#region enum ExportMedium
	[Serializable]
	public enum ExportMedium
	{
		//Usb,
		Email,
		Ftp,
		Print,
		SharedDisk,
		Cloud,
		//SmartDock,
		QrCode
	}
	#endregion

	#region enum ImagesSelection
	[Serializable]
	public enum ImagesSelection
	{
		Selected,
		All
	}
	#endregion

	#region class ExportFileSettings
	public class ExportFileSettings
	{
		public readonly BscanILL.Scan.FileFormat FileFormat;
		public readonly bool MultiImage = false;
		public readonly bool PdfA = BscanILL.SETTINGS.Settings.Instance.Export.IsPdfaDefault;
		public readonly short? Dpi = null;

		public ExportFileSettings(BscanILL.Scan.FileFormat fileFormat, short? dpi)
		{
			this.FileFormat = fileFormat;
			this.Dpi = dpi;
		}

		public ExportFileSettings(BscanILL.Scan.FileFormat fileFormat, short? dpi, bool multiImage)
			: this(fileFormat, dpi)
		{
			this.MultiImage = multiImage;
		}

		public ExportFileSettings(BscanILL.Scan.FileFormat fileFormat, short? dpi, bool multiImage, bool pdfA)
			: this(fileFormat, dpi, multiImage)
		{
			this.PdfA = pdfA;
		}
	}
	#endregion

	#region class ExportSettings
	public class ExportSettings : ExportFileSettings
	{
		public BscanILL.Scan.ExportMedium ExportMedium;
		public ImagesSelection Images { get; private set; }
		public string FileNamePrefix { get; private set; }

		public ExportSettings(BscanILL.Scan.ExportMedium exportMedium, BscanILL.Scan.FileFormat fileFormat, ImagesSelection images, string fileNamePrefix, bool multiImage, short? dpi, bool pdfA)
			: base(fileFormat, dpi, multiImage, pdfA)
		{
			this.ExportMedium = exportMedium;
			this.Images = images;
			this.FileNamePrefix = fileNamePrefix;
		}

		public ExportSettings(BscanILL.Scan.ExportMedium exportMedium, ExportFileSettings exportFileSettings, ImagesSelection images, string fileNamePrefix)
			: base(exportFileSettings.FileFormat, exportFileSettings.Dpi, exportFileSettings.MultiImage, exportFileSettings.PdfA)
		{
			this.ExportMedium = exportMedium;
			this.Images = images;
			this.FileNamePrefix = fileNamePrefix;
		}
	}
	#endregion

}
