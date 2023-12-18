using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.MODELS
{

	#region enum ScanerModel
	public enum ScanerModel
	{
		//S2N

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
		BE3_SGS_R1 = 26, // DeviceClass=6
		BE3_SGS_R2 = 27,

		//Bookeye 4
		BE4_SGS_V2 = 52,
		BE4_BDLK1_V3 = 71,
		BE4_BDL_V1A = 64,

        //Bookeye 5
        BE5_V3 = 92,
        BE5_V2 = 93,
		// these numeric values must be retrieved from DeviceType field in BE device S2Ninfo
		BE5_V1A = 94,  
		BE5_V2A = 99,

		// WideTEK Large Format
		WT36_200 = 32,
		WT42_200 = 34,
		//WT42_200 = ,
		//WT42-200-BDL = ,
		WT48 = 18,

		//WideTek Flat Bed
		WT25_200 = 38,
		WT25_600 = 62,
		WT25_650 = 86,

		// CLICK FAMILY
		ClickV1 = 1024,
		ClickMiniV1 = 1050,

		// ADF
		KodakI1405 = 2048,
		KodakI1120 = 2049,
		KodakI1150 = 2050,
		KodakI1150New = 2051,   //i1150 with new driver for i11XX series (for Win10)
		KodakE1035 = 2052,
		KodakE1040 = 2053,

		// BOOKEDGE
		iVinaFB6280E = 3072,
		iVinaFB6080E = 3073,

		// TWAIN OVERHEAD
		Fujitsu_SV600 = 4096
	}
	#endregion

	#region enum ScannerGroup
	public enum ScannerGroup
	{
		S2N,
		Click,
		Twain
	}
	#endregion

	#region enum ScannerSubGroup
	public enum ScannerSubGroup
	{
		BE2_N2,
		BE2_N3,
		BE3,
		BE4,
        BE5,
		WideTekFlat,
		WideTekThru,
		Click,
		ClickMini,
		BookEdge,
		TwainAdf,
		TwainOverhead
	}
	#endregion

}
