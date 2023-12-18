using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners
{
	
	#region enum ColorMode
	[Serializable]
	public enum ColorMode
	{
		Color = 1,
		Grayscale = 2,
		Bitonal = 3,
		Unknown = 5
	}
	#endregion

	#region enum ScanMode
	[Serializable]
	[Flags]
	public enum ScanMode
	{
		SingleScan = 0x01,
		SplitImage = 0x02,
		BookMode = 0x04,
		TurboScan = 0x08,
		AutoSplitImage = 0x10,
		AdfDuplexMulti = 0x20,
		AdfSimplexMulti = 0x40
	}
	#endregion

	#region enum RebelScanMode
	[Serializable]
	[Flags]
	public enum RebelScanMode
	{
		SplitImage = ScanMode.SplitImage,
		BookMode = ScanMode.BookMode,
	}
	#endregion

	#region enum FileFormat
	public enum FileFormat : byte
	{
		Jpeg = 1,
		Tiff = 2,
		Png = 4
	}
	#endregion

	#region enum SettingsScannerType
	[Serializable]
	public enum SettingsScannerType : int
	{
		S2N = 0,
		FolderScanner = 1,
		Click = 2,
		ClickMini = 3,
		iVinaFB6280E = 5,
		iVinaFB6080E = 6,
		KodakI1405 = 256,
		KodakI1120 = 257,
		KodakI1150 = 258,
		KodakI1150New = 259,
		KodakE1035 = 260,
		KodakE1040 = 261
	}
	#endregion

	#region enum AdfScannerType
	/*[Serializable]
	public enum AdfScannerType : int
	{
		KodakI1405 = 256,
		KodakI1120 = 257
	}*/
	#endregion

	#region enum MultiScannerMode
	public enum MultiScannerMode
	{
		Off,
		BookEdgeAdf,
		Bookeye4Adf,
		Bookeye2Adf
	}
	#endregion

	#region enum AdfFeederState
	[Serializable]
	public enum AdfFeederState
	{
		Loaded,
		Unloaded
	}
	#endregion

	#region enum S2nType
	/*[Serializable]
	public enum S2nType : byte
	{
		//Bookeye 2
		BE2_SCL_N2 = 0,
		BE2_SGS_N2 = 2,
		BE2_SCL_R1 = 3,
		BE2_SCL_R2 = 4,
		BE2_SGS_R2 = 5,
		BE2_SGS_R1 = 6,
		BE2_SCL_R1_PLUS = 11,
		BE2_SGS_R1_PLUS = 12,
		BE2_SCL_R2_PLUS = 13,
		BE2_SGS_R2_PLUS = 14,
		BE2_CSL_N2_PLUS = 16,
		BE2_SGS_N2_PLUS = 17,
		BE2_CGS_N2 = 9,
		BE2_SGS_N3 = 28,
		BE2_SCL_N2_R = 41,

		//WideTek A2
		FBA2_100 = 1,
		FBA2_110 = 15,
		WTA2_110 = 15,
		FBA2_80 = 8,
		FBA2_90 = 22,
		WTA2_90 = 22,

		//WideTek A3
		WTA3_100 = 40,
		WTA3_110 = 42,

		//Bookeye 3
		BE3_SCL_R1 = 23,
		BE3_SGS_R2 = 27,

		//Bookeye 4
		BE4_SGS_V2 = 52,
		BE4_BDLK1_V3 = 71,

		WT48 = 18,
		//WT_A3 = 40,
		//WTA3_100 = 42,
		//Unknown = 255
	}*/
	#endregion

	#region enum ScannedPages
	[Serializable]
	[Flags]
	public enum ScannedPages : short
	{
		Left = 1,
		Right = 2,
		Both = 4
	}
	#endregion
	
	#region enum Sv600ScanMode
	public enum Sv600ScanMode
	{
		Flat = 0,
		Book = 1
	}
	#endregion

}
