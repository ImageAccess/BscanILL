using System;
using System.Collections;
using System.Collections.Specialized ;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging ;
using System.Data;
using System.IO ;
using System.Net ;
using System.Text ;
using System.Diagnostics ;
using System.Configuration ;
using System.Threading;
using System.Collections.Generic;


namespace Scanners.S2N
{
	public class ScannerS2NWrapper : WrapperBase, IScanner, IS2NWrapper
	{
		Scanners.S2N.ScannerS2N		scanner;

		bool						scannerButtonsActive = false;		
		bool						isAutoFocusOn = true;

		List<ScannerOperation>	scannerQueue = new List<ScannerOperation>();
		object					scannerQueueLocker = new object();
		bool					stopMessagePump = false;
		AutoResetEvent			messagePumpLocker = new AutoResetEvent(false);


		#region constructor
		public ScannerS2NWrapper(string ip)
			: base(Scanners.S2N.ScannerS2N.GetInstance(ip))
		{
			scanner = (Scanners.S2N.ScannerS2N)this.Scanner;

			scanner.ResetTouchScreenEvents();

			Thread thread = new Thread(new ThreadStart(MessagePump));
			thread.Name = "ThreadS2NShared_MessagePump";
			thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		
		public bool							CanTurnOffAutoFocus { get { return this.scanner.CanTurnOffAutoFocus; } }
		public bool							CanTurnOffLights { get { return this.scanner.CanTurnOffLights; } }
		public bool							CanSetDocMode { get { return this.scanner.CanSetDocMode; } }
		public bool							IsLightOn { get { return this.scanner.IsLightOn; } }

		#region IsPedalAndButtonsActive
		public bool IsPedalAndButtonsActive
		{
			get { return this.scannerButtonsActive; }
			set
			{
				lock (scannerButtonsActivationLocker)
				{
					this.scannerButtonsActive = (value && (scanner.IsTouchScreenAvailable == false));
				}
			}
		}
		#endregion

		#region IsAutoFocusOn
		public bool IsAutoFocusOn
		{
			get { return this.isAutoFocusOn; }
			protected set { this.isAutoFocusOn = value; }
		}
		#endregion

    #region HtmlTouchScreenAppVersion
    public int HtmlTouchScreenAppVersion
    {
      get { return scanner.HtmlTouchScreenAppVersion; }
      set { scanner.HtmlTouchScreenAppVersion = value; }
    }
    #endregion
    
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			this.IsPedalAndButtonsActive = false;
			StopTouchScreenMonitoring();
			this.stopMessagePump = true;
			messagePumpLocker.WaitOne(30000);
			scanner.Dispose();
		}
		#endregion

		#region Scan()
		/// <summary>
		/// 
		/// </summary>
		/// <param name="operationId"></param>
		/// <param name="docSize"></param>
		/// <param name="colorMode"></param>
		/// <param name="fileFormat"></param>
		/// <param name="dpi"></param>
		/// <param name="brightness">in interval -1, 1</param>
		/// <param name="contrast">in interval -1, 1</param>
		/// <param name="autoFocus"></param>
		public void Scan(int operationId, Scanners.S2N.S2NSettings settings)
		{
			lock (this)
			{
				//SetScanner(settings);

				try
				{
					lock (this.scannerQueueLocker)
					{						
						//scannerQueue.Add(new ScannerOperation(operationId, ScannerOperationType.Scan, ""));
                        scannerQueue.Add(new ScannerOperation(operationId, ScannerOperationType.Scan, settings));
					}
				}
				catch (ScannersEx ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					notifications.Notify(this, Notifications.Type.Error, "ScannerS2NWrapper Scan(): " + ex.Message, ex);
					throw new ScannersEx("Scanner exception while scanning:" + " " + ex.Message, ScannersEx.AlertType.Error);
				}
				finally
				{
				}
			}
		}
		#endregion

        #region SetScannerSettings()
        public void SetScannerSettings(int operationId, Scanners.S2N.S2NSettings settings)
        {
            lock (this)
            {
                //SetScanner(settings);

                try
                {
                    lock (this.scannerQueueLocker)
                    {
                        
                        scannerQueue.Add(new ScannerOperation(operationId, ScannerOperationType.SetDevice, settings));
                    }
                }
                catch (ScannersEx ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    notifications.Notify(this, Notifications.Type.Error, "ScannerS2NWrapper SetScannerSettings(): " + ex.Message, ex);
                    throw new ScannersEx("Scanner exception while setting scanner settings:" + " " + ex.Message, ScannersEx.AlertType.Error);
                }
                finally
                {
                }
            }
        }
        #endregion

        #region Reset()
        public void Reset(int operationId)
		{
			this.IsPedalAndButtonsActive = false;
			
			StopTouchScreenMonitoring();
			
			try
			{
				this.scanner.LockUi( false );
			}
			catch { }

			this.scanner.Settings.SettingsChanged = true;
		}
		#endregion

		#region ResetTouchScreenEvents()
		/// <summary>
		/// Call when the application is launched and before UI is open.
		/// </summary>
		public void ResetTouchScreenEvents()
		{
			scanner.ResetTouchScreenEvents();
		}
		#endregion

    #region RequestTouchScreenVersion()
    public void RequestTouchScreenVersion()
    {
      scanner.RequestTouchScreenVersion();
    }
    #endregion

    #region GetTouchScreenVersion()
    public void GetTouchScreenVersion(bool wakeOnLANExecuted)
    {
      scanner.GetTouchScreenVersion(wakeOnLANExecuted);
    }
    #endregion
    
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region MessagePump()
		private void MessagePump()
		{
			while (this.stopMessagePump == false)
			{
				try
				{
					ScannerOperation operation = null;

					lock (this.scannerQueueLocker)
					{
						if (scannerQueue.Count > 0)
						{
							operation = scannerQueue[0];
							scannerQueue.RemoveAt(0);

                            if (operation != null)
                            {
                                if (operation.OperationType == ScannerOperationType.SetDevice)   //if set settings request
                                {
                                    if (this.IsPedalAndButtonsActive)   //just if foot pedal turn on we have to make sure settings is up to date. When just direct mode, we set Settings before scan command
                                    {
                                        // in case of using GUI controls, we trigger this request on every value change so now execute just last straight SetDevice command with final setting values                                    
                                        ScannerOperation operationTemp = null;

                                        while (scannerQueue.Count > 0)
                                        {
                                            if (scannerQueue[0].OperationType == ScannerOperationType.SetDevice)
                                            {
                                                operationTemp = scannerQueue[0];
                                                scannerQueue.RemoveAt(0);
                                                if (operationTemp != null)
                                                {
                                                    operation = operationTemp;
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        operation = null;
                                    }
                                }
                            }
						}
					}

					if (operation != null)
					{
						switch (operation.OperationType)
						{
							//case ScannerOperationType.SetDevice: SetDevice_Thread(operation.OperationId); break;
                            case ScannerOperationType.SetDevice: SetDevice_Thread(operation.OperationId, (Scanners.S2N.S2NSettings)operation.Parameters); break;                                                                                                
							//case ScannerOperationType.Scan: Scan_Thread(operation.OperationId); break;
                            case ScannerOperationType.Scan: Scan_Thread(operation.OperationId, (Scanners.S2N.S2NSettings)operation.Parameters); break;
						}
					}
					else
					{
						if (this.IsPedalAndButtonsActive)
							ScanWait_Thread();

						Thread.Sleep(100);
					}

				}
				catch (Exception ex)
				{
					this.IsPedalAndButtonsActive = false;   //not sure why we turn off foot pedal support when exception...

					scanner.Logout();
					RaiseOperationError(-1, ex);
				}
			}

			this.messagePumpLocker.Set();
		}
		#endregion

		#region ScanWait_Thread()
		private void ScanWait_Thread()
		{
			try
			{
				//if (scanner.IsTouchScreenAvailable)
                if (this.IsPedalAndButtonsActive )
				{
					string filePath = string.Format("{0}\\{1}.jpg", imageDir.FullName, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff"));
					string fileScanned = scanner.ScanWait(filePath);

					if (fileScanned != null)
						RaiseImageScanned(-1, fileScanned);
				}
			}
			catch (Exception ex)
			{
				this.IsPedalAndButtonsActive = false;

				scanner.Logout();
				RaiseOperationError(-1, ex);
			}
		}
		#endregion

		#region SetDevice_Thread()        
		private void SetDevice_Thread(int operationId)
		{
			try
			{
				Thread.Sleep(100);
				scanner.SetDevice(this);
				RaiseOperationSuccessfull(operationId);
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't setup scanner!" + " " + ex.Message, ex);
				RaiseOperationError(operationId, ex);
			}
		}
		#endregion

		#region Scan_Thread()
		/*private void Scan_Thread(int operationId)
		{
			try
			{
				Thread.Sleep(100);
				string filePath = string.Format("{0}\\{1}.jpg", imageDir.FullName, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-ff"));
				scanner.Scan(filePath);

				ImageScanned(operationId, filePath);
			}
			catch (ScannersEx ex)
			{
				RaiseOperationError(operationId, ex);
			}
			catch (Exception ex)
			{
				Notifications.Instance.Notify(this, Notifications.Type.Warning, "Can't scan image!" + " " + ex.Message, ex);
				RaiseOperationError(operationId, ex);
			}
		}*/
		#endregion

		#endregion

	}
}
