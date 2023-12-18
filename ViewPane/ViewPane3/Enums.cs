using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewPane
{
	#region enum ToolbarSelection
	[Flags]
	public enum ToolbarSelection : int
	{
		None = 0x00,
		Move = 0x01,
		ZoomIn = 0x02,
		ZoomOut = 0x04,
		ZoomDynamic = 0x08,
		SelectRegion = 0x10,
		Pages = 0x20,
		FingerRemoval = 0x40,
		//Rotation = 0x80,
		Bookfold = 0x100,
		Transforms = Pages + FingerRemoval + Bookfold
	}
	#endregion

	#region enum ZoomType
	public enum ZoomType : int
	{
		ActualSize,
		FitImage,
		FitWidth,
		Value
	}
	#endregion

}
