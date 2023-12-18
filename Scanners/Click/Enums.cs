using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Click
{

	#region enum ClickColorMode
	[XmlTypeAttribute(TypeName = "Scanners.Click.ColorMode")]
	public enum ClickColorMode
	{
		Color = Scanners.ColorMode.Color,
		Grayscale = Scanners.ColorMode.Grayscale,
		Bitonal = Scanners.ColorMode.Bitonal
	}
	#endregion

	#region enum ScanPage
	[Serializable]
	[XmlTypeAttribute(TypeName = "Scanners.Click.ScanPage")]
	public enum ScanPage
	{
        Both = ClickCommon.ScanPage.FlatBoth,   
        Left = ClickCommon.ScanPage.FlatLeft,
        Right = ClickCommon.ScanPage.FlatRight,
        Single = ClickCommon.ScanPage.Stitch,        
	}
	#endregion

	#region enum ClickScanPage
	[Serializable]
	[XmlTypeAttribute(TypeName = "Scanners.Click.ClickScanPage")]
	public enum ClickScanPage
	{
        Both = ClickCommon.ScanPage.FlatBoth,
        Left = ClickCommon.ScanPage.FlatLeft,
        Right = ClickCommon.ScanPage.FlatRight,
	}
	#endregion

	#region enum ClickScanMode
	[Serializable]
	[XmlTypeAttribute(TypeName = "Scanners.Click.ClickScanMode")]
	public enum ClickScanMode
	{
		SplitImage = Scanners.ScanMode.SplitImage,
		BookMode = Scanners.ScanMode.BookMode,
	}
	#endregion

	#region enum ClickMiniScanMode
	[Serializable]
	[XmlTypeAttribute(TypeName = "Scanners.Click.ClickMiniScanMode")]
	public enum ClickMiniScanMode
	{
		SingleScan = Scanners.ScanMode.SingleScan,
		SplitImage = Scanners.ScanMode.SplitImage,
		BookMode = Scanners.ScanMode.BookMode,
		AutoSplitImage = Scanners.ScanMode.AutoSplitImage
	}
	#endregion

}
