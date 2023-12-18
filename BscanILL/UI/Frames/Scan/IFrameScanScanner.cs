using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.UI.Frames.Scan
{
	public interface IFrameScanScanner
	{
		void Dispose();
		void Activate();
		void Deactivate();
		//void Scan();
		void Reset();
	}
}
