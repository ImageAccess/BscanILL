using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewPane.IT
{
	/*public enum DespeckleMode
	{
		BlackSpecklesOnly = 0x01,
		WhiteSpecklesOnly = 0x02,
		BothColors = BlackSpecklesOnly | WhiteSpecklesOnly
	}*/

	/*public enum DespeckleSize
	{
		Size1x1 = 0x01,
		Size2x2 = 0x02,
		Size3x3 = 0x03,
		Size4x4 = 0x04,
		Size5x5 = 0x05,
		Size6x6 = 0x06,
	}*/

	/*public enum RotationMode
	{
		NoRotation = 0x01,
		Rotation90 = 0x02,
		Rotation180 = 0x03,
		Rotation270 = 0x04,
	}*/

	public enum BookPartType
	{
		None = 0,

		FrontCover = 100,
		SpineCover,
		RearCover,

		HeadEdge = 200,
		TailEdge,
		ForeEdge,

		FrontEndPapers = 300,
		FrontPastdownEndPaper,
		FrontFreeEndPaper,
		RearEndPapers,
		RearFreeEndPaper,
		RearPastdownEndPaper,

		Normal = 400,

		Other = 500,

		Unknown = 9999
	}
}
