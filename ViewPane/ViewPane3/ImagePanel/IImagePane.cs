using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewPane.ImagePanel
{
	internal interface IImagePane
	{
		//properties
		//bool AllowTransforms { get; set; }

		//events
		event ImageSelectedHnd ImageChanged;
	}
}
