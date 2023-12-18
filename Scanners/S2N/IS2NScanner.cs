using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.S2N
{
	public delegate void ScanRequestHnd(Scanners.S2N.ScannerScanAreaSelection scanArea);
    public delegate void DocModeSettingHnd(Scanners.S2N.DocumentMode docMode);

    

	
	public interface IS2NScanner
	{
		Scanners.DeviceInfo		DeviceInfo { get; }
		S2NSettings				Settings { get; }
		short					MaxColorDpi { get; }
		bool					IsColorScanner { get; }

		bool					IsTouchScreenMonitoringRunning { get; }

		void Scan(string filePath);
		void StartTouchScreenMonitoring();
		void StopTouchScreenMonitoring();
    void StartTimerPing();
    void StopTimerPing();		
		void ResetTouchScreenEvents();
        void UpdateTouchScreenExternal(Scanners.S2N.ScannerScanAreaSelection areaSelection);

		void LockUi( bool forceLock );
        void UnlockUi(bool singleArticleMode);

		
		event Scanners.PreviewScannedHnd	PreviewScanned;
		event Scanners.ProgressChangedHnd	ProgressChanged;
		event Scanners.S2N.ScanRequestHnd	ScanRequest;
		event Scanners.S2N.ScanRequestHnd	ScanPullslipRequest;
        event Scanners.S2N.ScanRequestHnd   TouchScreenScanAreaButtonPressed;
	}
}
