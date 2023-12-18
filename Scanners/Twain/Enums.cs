using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Twain
{
	#region enum ColorMode
	[XmlTypeAttribute(TypeName = "Scanners.Twain.ColorMode")]
	public enum ColorMode
	{
		Color = Scanners.ColorMode.Color,
		Grayscale = Scanners.ColorMode.Grayscale,
		Bitonal = Scanners.ColorMode.Bitonal
	}
	#endregion

	#region enum FileFormat
	[XmlTypeAttribute(TypeName = "Scanners.Twain.FileFormat")]
	public enum FileFormat
	{
		Jpeg,
		Png,
		Tiff
	}
	#endregion
	
	#region enum DocSize
	[XmlTypeAttribute(TypeName = "Scanners.Twain.DocSize")]
	public enum DocSize
	{
		Max = 6,
		Auto = 9,
	}
	#endregion

	#region enum BookedgePage
	[XmlTypeAttribute(TypeName = "Scanners.Twain.BookedgePage")]
	public enum BookedgePage
	{
		FlatMode,
		LeftPage,
		RightPage,
		Automatic
	}
	#endregion
}
