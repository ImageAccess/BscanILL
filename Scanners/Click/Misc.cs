using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanners.Click
{
	public delegate void ConnectionSuccessHnd(bool settingsChanged);
	public delegate void ScanBitmapsCompleteHnd(Bitmap bitmap1, Bitmap bitmap2);
    public delegate void ScanBitmapCompleteHnd(Bitmap bitmap, Scanners.Click.ScanPage scanPage);          
    //public delegate void CameraBitmapCompleteHnd(Bitmap bitmap, CameraScanPage scanPage);
    public delegate void ScanErrorHnd(Exception ex);
    //public delegate void ScanCanceledByUserHnd();    
    public delegate void LiveImageCapturedClickHnd(Bitmap bitmap, ClickCommon.CameraScanPage scanPage);
    public delegate void LiveImageCapturedHnd(Bitmap bitmap);
    public delegate void CameraInternalErrorClickHnd(bool leftCamera);
    public delegate void CameraInternalErrorHnd();
    public delegate void CameraShutDownClickHnd(bool leftCamera);
    public delegate void CameraShutDownHnd();
    //public delegate void BookfoldImageCompleteHnd(CameraScanPage scanPage);
    public delegate void ScanButtonPressedHnd();
	public delegate void ErrorHnd(Exception ex);
	public delegate void BitmapCompleteHnd(Bitmap bitmap);
	public delegate void PowerUpSuccessHnd();

	public static class Misc
	{
		//List<ClickDLL.UI.Dialogs.ClickWizard.MainPanel.ComboApertureItem> ApertureItems { get { return ClickDLL.UI.Dialogs.ClickWizard.MainPanel.GetApertureItems(); } }
		//List<ClickDLL.UI.Dialogs.ClickWizard.MainPanel.ComboSpeedItem> SpeedItems { get { return ClickDLL.UI.Dialogs.ClickWizard.MainPanel.GetSpeedItems(); } }

	}

}
