using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.Printing
{
	public interface IInputBin
	{
		string		DisplayName { get; }
		string		Key { get; }
	}
}
