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
	public class Bookeye4Wrapper : WrapperBase, IScanner, IS2NWrapper
	{
		Scanners.S2N.Bookeye4		scanner = null;	

		List<ScannerOperation>	scannerQueue = new List<ScannerOperation>();
		object					scannerQueueLocker = new object();
		bool					stopMessagePump = false;
		AutoResetEvent			messagePumpLocker = new AutoResetEvent(false);


		#region constructor
		public Bookeye4Wrapper(string ip , bool wakeOnLANExecuted)
			: base(Scanners.S2N.Bookeye4.GetInstance(ip))
		{			
			//Thread.Sleep(500);
			scanner = (Scanners.S2N.Bookeye4)this.Scanner;

            scanner.ResetTouchScreenEvents();
            if(scanner.IsTouchScreenAvailable)
            {                
                scanner.RequestTouchScreenVersion();
                Thread.Sleep(200);

                scanner.GetTouchScreenVersion(wakeOnLANExecuted);                
            }			

			Thread thread = new Thread(new ThreadStart(MessagePump));
			thread.Name = "ThreadBookeye4Shared_MessagePump";
			thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region IsPedalAndButtonsActive
		public bool IsPedalAndButtonsActive
		{
			get { return false; }
			set { }
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
		/// <param name="settings"></param>
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
					notifications.Notify(this, Notifications.Type.Error, "Bookeye4Shared Scan(): " + ex.Message, ex);
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
            //no need to implement in BE4 wrapper - this is just for BE3, BE5 to update scanner settings if modified via GUI for scan Wait mode (foot pedal, hardware scan button)
        }
        #endregion

		#region SetDeviceTurbo()
		public void SetDeviceTurbo(bool userDocSize, Rectangle rectInMil)
		{
			if (userDocSize)
			{
				this.scanner.Settings.DocSize.Value = Scanners.S2N.DocumentSize.User;
				this.scanner.Settings.UserX.Value = Math.Max(this.scanner.Settings.UserX.Minimum, Math.Min(this.scanner.Settings.UserX.Maximum, rectInMil.X));
				this.scanner.Settings.UserY.Value = Math.Max(this.scanner.Settings.UserY.Minimum, Math.Min(this.scanner.Settings.UserY.Maximum, rectInMil.Y));
				this.scanner.Settings.UserW.Value = Math.Max(this.scanner.Settings.UserW.Minimum, Math.Min(this.scanner.Settings.UserW.Maximum, rectInMil.Width));
				this.scanner.Settings.UserH.Value = Math.Max(this.scanner.Settings.UserH.Minimum, Math.Min(this.scanner.Settings.UserH.Maximum, rectInMil.Height));
			}
			else
				this.scanner.Settings.DocSize.Value = Scanners.S2N.DocumentSize.Auto;
		}
		#endregion

		#region Reset()
		public void Reset(int operationId)
		{
			StopTouchScreenMonitoring();
			
			try
			{
				this.scanner.LockUi( false );
			}
			catch { }

			this.scanner.Settings.SettingsChanged = true;
		}
		#endregion
	
		#region GetSettingsText()
		public string GetSettingsText()
		{
			if (this.scanner != null)
				return this.scanner.Settings.ToString();
			else
				return "";
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
						}
					}

					if (operation != null)
					{
						switch (operation.OperationType)
						{
							//case ScannerOperationType.Ping: Ping_Thread(operation.OperationId); break;
							//case ScannerOperationType.SetDevice: SetDevice_Thread(operation.OperationId, (string)operation.Parameters); break;
							//case ScannerOperationType.Scan: Scan_Thread(operation.OperationId); break;
                            case ScannerOperationType.Scan: Scan_Thread(operation.OperationId, (Scanners.S2N.S2NSettings)operation.Parameters); break;
						}
					}
					else
					{
						/*if (this.scannerButtonsActive)
							ScanWait_Thread();*/

						Thread.Sleep(100);
					}

				}
				catch (Exception ex)
				{
					/*lock (scannerButtonsActivationLocker)
					{
						this.scannerButtonsActive = false;
					}*/

					RaiseOperationError(-1, ex);
				}
			}

			this.messagePumpLocker.Set();
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
