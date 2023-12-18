using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.UI.Frames.Start
{
	public interface IFrameStartScanner
	{
		void Dispose();
		void ScanPullslip();
		void Activate();
		void Deactivate();
		void ReRegisterScanner();
		void UnRegisterScanner();
		void Reset();
		int TouchApp_Request();
	}
}
