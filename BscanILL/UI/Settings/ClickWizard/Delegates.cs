using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.UI.Settings.ClickWizard
{
	public delegate void LoadScannerHnd(string comPort);
	public delegate void StatusHnd(string status);
	public delegate void ClickHnd();
	public delegate void ScanHnd(bool distortion, bool whiteBalance, bool crop, bool bookfold, string saveTo);
	public delegate void WhiteBalanceHnd(CanonCamera.CameraProperties.Av av);

}
