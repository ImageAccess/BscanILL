using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N
{

	#region enum Autofocus
	[XmlTypeAttribute(TypeName = "Scanners.S2N.Autofocus")]
	public enum Autofocus
	{
		On,
		Off,
		Skip
	}
	#endregion

	#region enum BitonalThreshold
	public enum BitonalThreshold
	{
		Dynamic,
		Fixed
	}
	#endregion

	#region enum ColorMode
	public enum ColorMode
	{
		Color,
		Grayscale,
		Lineart,
		Photo
	}
	#endregion

	#region enum Despeckle
	public enum Despeckle
	{
		Off,
		Despeckle4x4
	}
	#endregion

	#region enum DocumentMode
	public enum DocumentMode
	{
		Flat = 0,
		Book = 1,
		Folder = 2,
		FixedFocus = 3,
		GlassPlate = 4,

		/// <summary>
		///  auto detection to decide, if cradle is in flat or book mode - for scanners with adjustable flat-V cradle
		/// </summary>
		Auto = 5,
		V = 6,
		BookGlassPlate = 7
	}
	#endregion

	#region enum DpiMode
	public enum DpiMode
	{
		Flexible,
		Fixed
	}
	#endregion

	#region enum FileFormat
	[XmlTypeAttribute(TypeName = "Scanners.S2N.FileFormat")]
	public enum FileFormat
	{
		Jpeg,
		Tiff,
		Pnm,
		Pdf,
		Unknown
	}
	#endregion

	#region enum LightSwitch
	public enum LightSwitch
	{
		On,
		Off
	}
	#endregion

	#region enum Rotation
	public enum Rotation
	{
		None = 0,
		CV90 = 90,
		CV180 = 180,
		CV270 = 270
	}
	#endregion

	#region enum ScanMode
	public enum ScanMode
	{
		Direct,
		Wait
	}
	#endregion

	#region enum Speed
	public enum Speed
	{
		Quality,
		Fast
	}
	#endregion

	#region enum Splitting
	public enum Splitting
	{		
		Left,
		Right,
        Auto,
        Off
	}
	#endregion

    #region enum SplittingStartPage
    public enum SplittingStartPage
    {        
        Left,
        Right        
    }
    #endregion

	#region enum TiffCompression
	public enum TiffCompression
	{
		None,
		G4
	}
	#endregion

	#region enum UserUnit
	public enum UserUnit
	{
		mm,
		mil
	}
	#endregion

	#region enum DocumentSize
	public enum DocumentSize
	{
		MaximumLandscape,
		MaximumPortraitLeft,
		MaximumPortraitRight,
		LegalLandscape,
		LetterLandscape,
		LetterPortraitLeft,
		LetterPortraitRight,
		HalfLetterProtraitLeft,
		HalfLetterProtraitRight,
		X11x17Landscape,
		X11x17PortraitLeft,
		X11x17PortraitRight,
		User,
		Auto
	}
	#endregion
	
	#region enum DocSizeType
	/*[Serializable]
	public enum DocSizeType
	{
		LetterLeft = 2,
		LetterRight = 3,
		Legal = 4,
		//DoubleLetter = 5,
		Max = 6,
		MaxLeft = 7,
		MaxRight = 8,
		Auto = 9,
		LetterLandscape = 17
	}*/
	#endregion

	#region enum ScannerScanAreaSelection
	public enum ScannerScanAreaSelection
	{
		Left,
		Right,
		Both,
		Flat
	}
	#endregion

	#region enum TouchScreenButton
	public enum TouchScreenButton
	{
		None,
		Scan,
		ScanPullslip
	}
	#endregion

}
