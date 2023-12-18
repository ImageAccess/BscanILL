using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.S2N
{
	public interface IS2NWrapper
	{
		//properties
		bool			IsPedalAndButtonsActive { get; set; }
		S2NSettings		ScannerSettings { get; }
    int HtmlTouchScreenAppVersion { get; set; }
		
		//methods
		void Scan(int operationId, Scanners.S2N.S2NSettings settings);
        void SetScannerSettings(int operationId, Scanners.S2N.S2NSettings settings);
		void Reset(int operationId);

		void LockScannerUi( bool forceLock );
    void UnlockScannerUi(bool singleArticleMode); 
		void StartTouchScreenMonitoring();
		void StopTouchScreenMonitoring();
    void StartTimerPing();
    void StopTimerPing();		
		void ResetTouchScreenEvents();
        void UpdateTouchScreenExternal(Scanners.S2N.ScannerScanAreaSelection areaSelection);    

    void RequestTouchScreenVersion();
    void GetTouchScreenVersion(bool wakeOnLANExecuted);		

		//events
		event Scanners.ImageScannedHnd PreviewScanned;
		event Scanners.FileScannedHnd ImageScanned;
		event Scanners.S2N.ScanRequestHnd ScanRequest;
		event Scanners.S2N.ScanRequestHnd ScanPullslipRequest;
		event Scanners.OperationSuccessfullHnd OperationSuccessfull;
		event Scanners.OperationErrorHnd OperationError;
		event Scanners.ProgressChangedHnd ProgressChanged;
        event Scanners.S2N.ScanRequestHnd UpdateSplittingButtonsRequest;
        event Scanners.S2N.ScanRequestHnd OverrideScanArea;
	}
}
