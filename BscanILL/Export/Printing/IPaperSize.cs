using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.Printing
{
	public interface IPaperSize
	{
		// PUBLIC PROPERTIES
		//double? Width { get; }
		//double? Height { get; }
		double? WidthInMM { get; }
		double? HeightInMM { get; }
		double? WidthInInches { get; }
		double? HeightInInches { get; }

		string Key { get; }
		string DisplayName { get; }


		// PUBLIC METHODS
		string ToString();

	}
}
