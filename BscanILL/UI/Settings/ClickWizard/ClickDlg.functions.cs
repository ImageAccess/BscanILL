using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

namespace BscanILL.UI.Settings.ClickWizard
{
	public partial class ClickDlg : Window
	{
		Scanners.Clicks.ClickWrapper				clickWrapper;
		AdjustmentType								adjustmentType = AdjustmentType.Scan;

		DirectoryInfo								saveTestsDir = new DirectoryInfo(@"c:\temp");
		int											saveCounter = 1;
		string										status = "ok";

		ClickDLL.IP.WhiteBalance						whiteBalance = null;


		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);


		#region Init()
		public void Init()
		{
			try
			{
				this.clickWrapper = Scanners.Clicks.ClickWrapper.GetInstance(this);
				
				Dialog_RegisterEvents();
				Camera_RegisterEvents();

				this.settings.Scanner.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Settings_PropertyChanged);
			}
			catch(Exception ex)
			{
				if (this.clickWrapper != null)
					this.clickWrapper.Dispose(this);

				throw ex;
			}
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			Camera_UnRegisterEvents();
			this.clickWrapper.Dispose(this);
		}
		#endregion

		#region enum AdjustmentType
		enum AdjustmentType
		{
			Scan,
			FindingFocus,
			FindingRegistrationPoints,
			FixingLightDistribution,
			FindingLaser,
			WhiteBalance
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public string Status { get { return this.status; } set { this.status = value; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Open()
		public void Open()
		{
			if (this.ShowDialog() == true)
			{
				//settings saved anyway
				//this.settings.Scanner.ClickScanner.Settings = settings.Scanner.ClickScanner.Settings;
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Dialog_RegisterEvents()
		void Dialog_RegisterEvents()
		{
			this.Control.LoadScannerClick += new BscanILL.UI.Settings.ClickWizard.LoadScannerHnd(LoadScanner_Click);
			this.Control.TurnVideoOnClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(StartLiveView_Click);
			this.Control.TurnVideoOffClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(StopLiveView_Click);

			this.Control.TestScanClick += new BscanILL.UI.Settings.ClickWizard.ScanHnd(TestScan_Click);
			this.Control.FindFocusClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(FindFocus_Click);
			this.Control.WhiteBalanceClick += new BscanILL.UI.Settings.ClickWizard.WhiteBalanceHnd(WhiteBalance_Click);
			this.Control.LightsDistributionClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(LightDistribution_Click);
			this.Control.ResetLightsDistributionClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(ResetLightsDistribution_Click);
			this.Control.SeekRegPointsClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(SeekRegPoints_Click);
			this.Control.FindLaserClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(FindLaser_Click);

			this.Control.LaserOnClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(LaserOn_Click);
			this.Control.LaserOffClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(LaserOff_Click);
			this.Control.LightsOnClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(LightsOn_Click);
			this.Control.LightsOffClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(LightsOff_Click);
			this.Control.SwapCamerasClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(SwapCameras_Click);

			this.Control.CamerasSettingsClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(CamerasSettings_Click);
			this.Control.CamerasDefaultSettingsClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(CamerasDefaultSettings_Click);
			this.Control.SaveAndCloseClick += new BscanILL.UI.Settings.ClickWizard.ClickHnd(SaveAndClose_Click);
		}
		#endregion

		#region Camera_RegisterEvents()
		void Camera_RegisterEvents()
		{
			clickWrapper.PowerUpSuccess += new ClickDLL.VoidHnd(Scanner_PowerUpSuccess);
			clickWrapper.PowerUpError += new ClickDLL.ErrorHnd(Scanner_PowerUpError);
			clickWrapper.ScanError += new ClickDLL.ScanErrorHnd(Scanner_ScanError);
			clickWrapper.ScanTimeout += new ClickDLL.ScanTimeoutHnd(Scanner_ScanTimeout);
			clickWrapper.LiveViewCaptured += new ClickDLL.LiveImageCapturedHnd(Scanner_LiveViewCaptured);
			clickWrapper.ScannerShutDownEvent += new ClickDLL.CameraShutDownHnd(Scanner_ShutDown);
			clickWrapper.ScannerInternalError += new ClickDLL.CameraInternalErrorHnd(Scanner_InternalError);

			clickWrapper.ImagesScanned += new ClickDLL.ScanBitmapsCompleteHnd(Scanner_ImagesComplete);
		}
		#endregion

		#region Camera_UnRegisterEvents()
		void Camera_UnRegisterEvents()
		{
			clickWrapper.PowerUpSuccess -= new ClickDLL.VoidHnd(Scanner_PowerUpSuccess);
			clickWrapper.PowerUpError -= new ClickDLL.ErrorHnd(Scanner_PowerUpError);
			clickWrapper.ScanError -= new ClickDLL.ScanErrorHnd(Scanner_ScanError);
			clickWrapper.ScanTimeout -= new ClickDLL.ScanTimeoutHnd(Scanner_ScanTimeout);
			clickWrapper.LiveViewCaptured -= new ClickDLL.LiveImageCapturedHnd(Scanner_LiveViewCaptured);
			clickWrapper.ScannerShutDownEvent -= new ClickDLL.CameraShutDownHnd(Scanner_ShutDown);
			clickWrapper.ScannerInternalError -= new ClickDLL.CameraInternalErrorHnd(Scanner_InternalError);

			clickWrapper.ImagesScanned -= new ClickDLL.ScanBitmapsCompleteHnd(Scanner_ImagesComplete);
		}
		#endregion

		#region Scanner_InternalError()
		void Scanner_InternalError(bool leftCamera)
		{
			if (leftCamera)
				ShowErrorMessage("Left camera internal error!");
			else
				ShowErrorMessage("Right camera internal error!");
		}
		#endregion

		#region Scanner_ShutDown()
		void Scanner_ShutDown(bool leftCamera)
		{
			if (leftCamera)
				ShowErrorMessage("Left camera shut down!");
			else
				ShowErrorMessage("Right camera shut down!");
		}
		#endregion

		#region Scanner_ScanTimeout()
		void Scanner_ScanTimeout()
		{
			this.Dispatcher.Invoke((Action)delegate() { Camera_ImageTimeoutTU(); });
		}
		#endregion

		#region Camera_ImageTimeoutTU()
		void Camera_ImageTimeoutTU()
		{
			ShowErrorMessage("Image Timeout!");
			UnLock();
		}
		#endregion

		#region Scanner_ScanError()
		void Scanner_ScanError(Exception ex)
		{
			this.Dispatcher.Invoke((Action)delegate() { Camera_ImageErrorTU(ex); });
		}
		#endregion

		#region Camera_ImageErrorTU()
		void Camera_ImageErrorTU(Exception ex)
		{
			ShowErrorMessage("Image Error! " + ex.Message);
			UnLock();
		}
		#endregion

		#region LoadScanner_Click()
		void LoadScanner_Click(string comPort)
		{
			try
			{
				Lock();
				BscanILL.Settings.Settings.Instance.Scanner.ClickScanner.General.ComPort = comPort;
				//this.settings.Scanner.ClickScanner.General.ComPort = comPort;
				this.clickWrapper.PowerUpAsync();
				//this.Control.InitSuccessfull();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				UnLock();
			}
		}
		#endregion

		#region Scanner_PowerUpSuccess()
		void Scanner_PowerUpSuccess()
		{
			this.Dispatcher.BeginInvoke((Action)delegate() { Scanner_PowerUpSuccessTU(); });
		}
		#endregion

		#region Scanner_PowerUpError()
		void Scanner_PowerUpError(Exception ex)
		{
			this.Dispatcher.BeginInvoke((Action)delegate() { Scanner_PowerUpErrorTU(ex); });
		}
		#endregion

		#region Scanner_PowerUpSuccessTU()
		void Scanner_PowerUpSuccessTU()
		{
			try
			{
				this.clickWrapper.Connect();
				this.Control.InitSuccessfull();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region Scanner_PowerUpErrorTU()
		void Scanner_PowerUpErrorTU(Exception ex)
		{
			MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			UnLock();
		}
		#endregion

		#region StartLiveView_Click()
		private void StartLiveView_Click()
		{
			clickWrapper.StartLiveView();
		}
		#endregion

		#region StopLiveView_Click()
		private void StopLiveView_Click()
		{
			clickWrapper.StopLiveView();
		}
		#endregion

		#region Scanner_LiveViewCaptured()
		void Scanner_LiveViewCaptured(System.Drawing.Bitmap bitmap, ClickDLL.CameraScanPage scanPage)
		{
			if (this.CanInvoke)
			{
				try { this.Dispatcher.BeginInvoke((Action)delegate() { LiveViewImage_CapturedTU(bitmap, scanPage); }); }
				catch { }
			}
		}
		#endregion

		#region LiveViewImage_CapturedTU()
		void LiveViewImage_CapturedTU(System.Drawing.Bitmap bitmap, ClickDLL.CameraScanPage scanPage)
		{
			/*if (scanPage == ClickDLL.CameraScanPage.Right)
				this.dlg.ShowImageR(bitmap);
			else
				this.dlg.ShowImageL(bitmap);*/

			bitmap.Dispose();
		}
		#endregion

		#region TestScan_Click()
		private void TestScan_Click(bool distortion, bool whiteBalance, bool crop, bool bookfold, string saveTo)
		{
			try
			{
				this.Status = "Scanning...";
				Lock();

				this.adjustmentType = AdjustmentType.Scan;
				
				this.saveTestsDir = new DirectoryInfo(saveTo);
				this.saveTestsDir.Create();

				ClickDLL.ScanParameters scanParams = new ClickDLL.ScanParameters(whiteBalance, bookfold, distortion, crop, 100, false);
				this.clickWrapper.Scan(global::ClickDLL.ScanPage.FlatBoth, false, scanParams);
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex.Message);
				UnLock();
			}
		}
		#endregion

		#region Scanner_ImagesComplete()
		void Scanner_ImagesComplete(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR)
		{
			try
			{	
				switch (this.adjustmentType)
				{
					case AdjustmentType.Scan: ShowAndSaveImages(bitmapL, bitmapR); break;
					case AdjustmentType.FindingRegistrationPoints: FindRegistrationPoints(bitmapL, bitmapR); break;
					case AdjustmentType.FindingFocus: FindingFocus(bitmapL, bitmapR); break;
					case AdjustmentType.WhiteBalance: PerformWhiteBalance(bitmapL, bitmapR); break;
					case AdjustmentType.FixingLightDistribution: FindLightDistribution(bitmapL, bitmapR); break;
					case AdjustmentType.FindingLaser: FindLaser(bitmapL, bitmapR); break;
				}

				this.Status = "Scan Successfull.";
			}
			catch (Exception ex)
			{
				ShowErrorMessage("Scanner_ImagesComplete(): " + ex.Message);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region ShowAndSaveImages()
		void ShowAndSaveImages(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR)
		{
			try
			{
				this.Control.ShowImages(BscanILL.UI.Misc.GetBitmapSource(bitmapL), BscanILL.UI.Misc.GetBitmapSource(bitmapR));

				string filePrefix = "KIC Click Test Scan " + saveCounter.ToString("0000");
				saveCounter++;
				SaveImages(bitmapL, bitmapR, filePrefix);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("Can't show images! " + ex.Message);
			}
			finally
			{
				bitmapL.Dispose();
				bitmapR.Dispose();
			}
		}
		#endregion

		#region SaveImages()
		void SaveImages(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR, string filePrefix)
		{
			try
			{
				bitmapL.SetResolution(300.0F, 300.0F);
				bitmapL.Save(this.saveTestsDir.FullName + @"\" + filePrefix + " L.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
				bitmapR.SetResolution(300.0F, 300.0F);
				bitmapR.Save(this.saveTestsDir.FullName + @"\" + filePrefix + " R.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("Can't save images! " + ex.Message);
			}
		}
		#endregion

		#region BrowseFolders_Click()
		private void BrowseFolders_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

			dlg.ShowNewFolderButton = true;

			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.Yes || dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				//textBoxSaveDir.Text = dlg.SelectedPath;
			}
		}
		#endregion

		#region SaveImage()
		private void SaveImage(System.Drawing.Bitmap bitmap, string fileName)
		{
			/*string directory = @"C:\delete\";
			string file = directory + @"\" + fileName + ".jpg";
			
			try
			{
				if (bitmap == null)
					ShowErrorMessage("SaveImage(): Image is null!");

				Directory.CreateDirectory(directory);

				bitmap.SetResolution(300.0F, 300.0F);
				bitmap.Save(@"C:\delete\" + fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("Can't save image to " + file + "! " + ex.Message);
			}*/
		}
		#endregion

		#region FindFocus_Click()
		void FindFocus_Click()
		{
			try
			{
				Lock();
				this.Status = "Focusing...";

				this.adjustmentType = AdjustmentType.FindingFocus;

				ClickDLL.ScanParameters scanParams = new ClickDLL.ScanParameters(true, 100, false);
				this.clickWrapper.Scan(global::ClickDLL.ScanPage.FlatBoth, true, scanParams);
				/*this.clickWrapper.SetFocus();

				System.Drawing.Bitmap bitmapL = this.clickWrapper.LeftCamera.ScanWait(10000, false);
				System.Drawing.Bitmap bitmapR = this.clickWrapper.LeftCamera.ScanWait(10000, false);
				this.Control.FocusSuccessfull(bitmapL, bitmapR);*/
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex.Message);
				UnLock();
			}
			/*finally
			{
				UnLock();
			}*/
		}
		#endregion

		#region FindingFocus()
		void FindingFocus(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR)
		{
			this.Control.FocusSuccessfull(bitmapL, bitmapR);

			if (this.Control.SaveAllScansOnDisk)
			{
				string filePrefix = "KIC Click Focus Scan";
				SaveImages(bitmapL, bitmapR, filePrefix);
			}

			bitmapL.Dispose();
			bitmapR.Dispose();
		}
		#endregion

		#region SeekRegPoints_Click()
		void SeekRegPoints_Click()
		{
			try
			{
				Lock();
				this.Status = "Scanning...";

				this.adjustmentType = AdjustmentType.FindingRegistrationPoints;

				ClickDLL.ScanParameters scanParams = new ClickDLL.ScanParameters(true, 100, false);
				this.clickWrapper.Scan(global::ClickDLL.ScanPage.FlatBoth, false, scanParams);
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex.Message);
				UnLock();
			}
		}
		#endregion

		#region FindRegistrationPoints()
		void FindRegistrationPoints(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR)
		{
			ClickDLL.IT.RegistrationPoints regPointsL = ClickDLL.IP.RegistrationPointsSeeker.FindRegistrationPoints(bitmapL, true);
			ClickDLL.IT.RegistrationPoints regPointsR = ClickDLL.IP.RegistrationPointsSeeker.FindRegistrationPoints(bitmapR, false);

			regPointsL.MarkRegistrationPoints(bitmapL);
			regPointsR.MarkRegistrationPoints(bitmapR);

			SaveImage(bitmapL, "Reg Points L.jpg");
			SaveImage(bitmapR, "Reg Points R.jpg");

			Point pUL = new Point(regPointsL.UpperPoint.X * bitmapL.Width, regPointsL.UpperPoint.Y * bitmapL.Height);
			Point pLL = new Point(regPointsL.LowerPoint.X * bitmapL.Width, regPointsL.LowerPoint.Y * bitmapL.Height);
			Point pUR = new Point(regPointsR.UpperPoint.X * bitmapL.Width, regPointsR.UpperPoint.Y * bitmapL.Height);
			Point pLR = new Point(regPointsR.LowerPoint.X * bitmapL.Width, regPointsR.LowerPoint.Y * bitmapL.Height);

			BitmapSource ul = GetClip(bitmapL, new System.Drawing.Rectangle((int)pUL.X - 90, (int)pUL.Y - 70, 180, 140));
			BitmapSource ll = GetClip(bitmapL, new System.Drawing.Rectangle((int)pLL.X - 90, (int)pLL.Y - 70, 180, 140));
			BitmapSource ur = GetClip(bitmapR, new System.Drawing.Rectangle((int)pUR.X - 90, (int)pUR.Y - 70, 180, 140));
			BitmapSource lr = GetClip(bitmapR, new System.Drawing.Rectangle((int)pLR.X - 90, (int)pLR.Y - 70, 180, 140));

			double skewL = 0, skewR = 0;

			if (pUL.X < pLL.X - 1 || pUL.X > pLL.X + 1)
				skewL = Math.Atan2(pLL.X - pUL.X, pLL.Y - pUL.Y);
			if (pUR.X < pLR.X - 1 || pUR.X > pLR.X + 1)
				skewR = Math.Atan2(pLR.X - pUR.X, pLR.Y - pUR.Y);

			this.Control.FindingRegPointsSuccessfull(ul, ll, ur, lr, skewL * 180 / Math.PI, skewR * 180 / Math.PI);

			if (this.Control.SaveAllScansOnDisk)
			{
				string filePrefix = "KIC Click Reg Points Scan";
				SaveImages(bitmapL, bitmapR, filePrefix);
			}

			bitmapL.Dispose();
			bitmapR.Dispose();

			settings.Scanner.ClickScanner.IT.ItPageL.RegistrationPoints = regPointsL;
			settings.Scanner.ClickScanner.IT.ItPageR.RegistrationPoints = regPointsR;

			//settings.Scanner.ClickScanner.RaisePropertyChanged("RegistrationPoints");
		}
		#endregion

		#region FindLaser_Click()
		void FindLaser_Click()
		{
			try
			{
				this.Status = "Scanning...";
				Lock();

				this.adjustmentType = AdjustmentType.FindingLaser;

				ClickDLL.ScanParameters scanParams = new ClickDLL.ScanParameters(true, 0, true);
				this.clickWrapper.Scan(global::ClickDLL.ScanPage.FlatBoth, false, scanParams);
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex.Message);
				UnLock();
			}
		}
		#endregion

		#region FindLaser()
		void FindLaser(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR)
		{
			ClickDLL.IT.LaserLine laserLineL = ClickDLL.IP.LaserSeeker.Find(bitmapL);
			ClickDLL.IT.LaserLine laserLineR = ClickDLL.IP.LaserSeeker.Find(bitmapR);

			if (laserLineL != null && laserLineR != null)
			{
				laserLineL.MarkLaserLine(bitmapL);
				SaveImage(bitmapL, "Laser Line L.jpg");
				//this.dlg.ShowImageL(bitmapL);
				settings.Scanner.ClickScanner.IT.ItPageL.LaserLine = laserLineL;
				
				laserLineR.MarkLaserLine(bitmapR);
				SaveImage(bitmapL, "Laser Line R.jpg");
				//this.dlg.ShowImageR(bitmapR);
				settings.Scanner.ClickScanner.IT.ItPageR.LaserLine = laserLineR;

				BitmapSource ll = GetClip(bitmapL, new System.Drawing.Rectangle(0, (int)(laserLineL.LeftPoint.Y * bitmapL.Height) - 70, 140, 140));
				BitmapSource lm = GetClip(bitmapL, new System.Drawing.Rectangle(bitmapL.Width / 2 - 70, (int)(laserLineL.MiddlePoint.Y * bitmapL.Height) - 70, 140, 140));
				BitmapSource lr = GetClip(bitmapL, new System.Drawing.Rectangle(bitmapL.Width - 140, (int)(laserLineL.RightPoint.Y * bitmapL.Height) - 70, 140, 140));
				BitmapSource rl = GetClip(bitmapR, new System.Drawing.Rectangle(0, (int)(laserLineR.LeftPoint.Y * bitmapR.Height) - 70, 140, 140));
				BitmapSource rm = GetClip(bitmapR, new System.Drawing.Rectangle(bitmapR.Width / 2 - 70, (int)(laserLineR.MiddlePoint.Y * bitmapR.Height) - 70, 140, 140));
				BitmapSource rr = GetClip(bitmapR, new System.Drawing.Rectangle(bitmapR.Width - 140, (int)(laserLineR.RightPoint.Y * bitmapR.Height) - 70, 140, 140));

				this.Control.FindingLaserSuccessfull(ll, lm, lr, rl, rm, rr);
			}
			else if(laserLineL == null && laserLineR != null)
				MessageBox.Show("Can't find laser on the left side of the board!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
			else if (laserLineL != null && laserLineR == null)
				MessageBox.Show("Can't find laser on the right side of the board!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
			else
				MessageBox.Show("Can't find laser on either side of the board!", "", MessageBoxButton.OK, MessageBoxImage.Warning);

			if (this.Control.SaveAllScansOnDisk)
			{
				string filePrefix = "KIC Click Laser Lines Scan";
				SaveImages(bitmapL, bitmapR, filePrefix);
			}

			bitmapL.Dispose();
			bitmapR.Dispose();
		}
		#endregion

		#region WhiteBalance_Click()
		void WhiteBalance_Click(CanonCamera.CameraProperties.Av av)
		{
			try
			{
				this.Status = "Scanning...";
				Lock();

				whiteBalance = new ClickDLL.IP.WhiteBalance(settings.Scanner.ClickScanner.Defaults.DefaultTv);

				whiteBalance.OperationSuccessfull += delegate(CanonCamera.CameraProperties.Tv speed, double shade) { this.Dispatcher.Invoke((Action)delegate() { WhiteBalance_OperationSuccessfullTU(speed, shade); }); };
				whiteBalance.Progress2Changed += delegate(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR, CanonCamera.CameraProperties.Tv tv, double shade) { this.Dispatcher.Invoke((Action)delegate() { WhiteBalance_Progress2ChangedTU(bitmapL, bitmapR, tv, shade); }); };
				whiteBalance.OperationError += delegate(Exception ex) { this.Dispatcher.Invoke((Action)delegate() { WhiteBalance_OperationErrorTU(ex); }); };

				this.adjustmentType = AdjustmentType.WhiteBalance;
				settings.Scanner.ClickScanner.Defaults.DefaultAv = av;
				this.clickWrapper.ClickScanner.Av = av;
				whiteBalance.Go(this.clickWrapper.ClickScanner);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("WhiteBalance_Click(): " + ex.Message);
				UnLock();
			}
		}
		#endregion

		#region PerformWhiteBalance()
		void PerformWhiteBalance(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR)
		{
			this.whiteBalance.BitmapsComplete(bitmapL, bitmapR);
		}
		#endregion

		#region WhiteBalance_OperationSuccessfullTU()
		void WhiteBalance_OperationSuccessfullTU(CanonCamera.CameraProperties.Tv tv, double shade)
		{
			//MessageBox.Show("White Balance finished. Shade: " + shade.ToString() + ", Speed: " + BscanILL.UI.Settings.ClickWizard.MainPanel.GetSpeedDescription(tv), "", MessageBoxButton.OK, MessageBoxImage.Information);
			//this.settings.Scanner.ClickScanner.General.DefaultTv = tv;
			settings.Scanner.ClickScanner.Defaults.DefaultTv = tv;
			this.Control.WhiteBalanceSuccessfull(tv, shade);
			UnLock();
		}
		#endregion

		#region WhiteBalance_OperationErrorTU()
		void WhiteBalance_OperationErrorTU(Exception ex)
		{
			MessageBox.Show("White Balance error! " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			this.Control.WhiteBalanceError();
			UnLock();
		}
		#endregion

		#region WhiteBalance_Progress2ChangedTU()
		void WhiteBalance_Progress2ChangedTU(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR, CanonCamera.CameraProperties.Tv tv, double shade)
		{
			BitmapSource bL = GetClip(bitmapL, new System.Drawing.Rectangle(bitmapL.Width / 2 - 50, bitmapL.Height / 2 - 50, 100, 100));
			BitmapSource bR = GetClip(bitmapR, new System.Drawing.Rectangle(bitmapR.Width / 2 - 50, bitmapR.Height / 2 - 50, 100, 100));
			this.Control.ShowWhiteBalanceProgress(bL, bR, tv, shade);

			bitmapL.Dispose();
			bitmapR.Dispose();
		}
		#endregion
	
		#region LightDistribution_Click()
		void LightDistribution_Click()
		{
			try
			{
				this.Status = "Scanning...";
				Lock();

				this.adjustmentType = AdjustmentType.FixingLightDistribution;

				ClickDLL.ScanParameters scanParams = new ClickDLL.ScanParameters(true, 100, false);
				this.clickWrapper.Scan(global::ClickDLL.ScanPage.FlatBoth, false, scanParams);
			}
			catch (Exception ex)
			{
				ShowErrorMessage("Scan_Click(): " + ex.Message);
				UnLock();
			}
		}
		#endregion

		#region FindLightDistribution()
		void FindLightDistribution(System.Drawing.Bitmap bitmapL, System.Drawing.Bitmap bitmapR)
		{
			ClickDLL.IT.LightDistributionMap mapL, mapR;

			/*ClickDLL.IP.LightDistributionMaster.Compute(bitmapL, bitmapR, out mapL, out mapR,
				settings.Scanner.ClickScanner.IT.ItPageL.DistortionMap, settings.Scanner.ClickScanner.IT.ItPageR.DistortionMap,
				settings.Scanner.ClickScanner.IT.ItPageL.RegistrationPoints, settings.Scanner.ClickScanner.IT.ItPageR.RegistrationPoints
				);*/
			ClickDLL.IP.LightDistributionMaster.Compute(bitmapL, bitmapR, out mapL, out mapR);

			SaveImage(bitmapL, "Light Distribution L.jpg");
			SaveImage(bitmapR, "Light Distribution R.jpg");

			this.Control.LightDistributionSuccessfull();

			if (this.Control.SaveAllScansOnDisk)
			{
				string filePrefix = "KIC Click Light Distribution Scan";
				SaveImages(bitmapL, bitmapR, filePrefix);
			}

			bitmapL.Dispose();
			bitmapR.Dispose();

			settings.Scanner.ClickScanner.IT.ItPageL.LightDistributionMap = mapL;
			settings.Scanner.ClickScanner.IT.ItPageR.LightDistributionMap = mapR;
		}
		#endregion

		#region ResetLightsDistribution_Click()
		void ResetLightsDistribution_Click()
		{
			try
			{
				settings.Scanner.ClickScanner.IT.ItPageL.LightDistributionMap.Reset();
				settings.Scanner.ClickScanner.IT.ItPageR.LightDistributionMap.Reset();
				this.Control.LightDistributionSuccessfull();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex.Message);
			}
		}
		#endregion

		#region LightsOn_Click()
		void LightsOn_Click()
		{
			this.clickWrapper.LightLevel = 100;
		}
		#endregion

		#region LightsOff_Click()
		void LightsOff_Click()
		{
			this.clickWrapper.LightLevel = 0;
		}
		#endregion

		#region LaserOn_Click()
		void LaserOn_Click()
		{
			this.clickWrapper.LaserOn = true;
		}
		#endregion

		#region LaserOff_Click()
		void LaserOff_Click()
		{
			this.clickWrapper.LaserOn = false;
		}
		#endregion

		#region CamerasSettings_Click()
		void CamerasSettings_Click()
		{
			ClickDLL.UI.Dialogs.ClickSettingsDlg dlg = new ClickDLL.UI.Dialogs.ClickSettingsDlg(this.clickWrapper.ClickScanner);
			dlg.ShowDialog();
		}
		#endregion

		#region CamerasDefaultSettings_Click()
		void CamerasDefaultSettings_Click()
		{
			ClickDLL.UI.Dialogs.DefaultSettingsDlg dlg = new ClickDLL.UI.Dialogs.DefaultSettingsDlg(this.clickWrapper.ClickScanner);
			dlg.ShowDialog();
		}
		#endregion

		#region SwapCameras_Click()
		void SwapCameras_Click()
		{
			if (clickWrapper.CapturingLiveView)
				clickWrapper.StopLiveView();

			this.clickWrapper.SwapCameras();
			//this.settings.Save();
		}
		#endregion

		#region SaveAndClose_Click()
		void SaveAndClose_Click()
		{
			this.DialogResult = true;
		}
		#endregion

		#region Settings_PropertyChanged()
		void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IsLightDistributionMapDefined":
				case "IsLaserLineDefined":
				case "IsRegistrationPointsDefined":
					//SetBrushes();
					break;
			}
		}
		#endregion

		#region ShowErrorMessage()
		void ShowErrorMessage(string message)
		{
			MessageBox.Show(message, "KIC Click Calibration", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		#endregion

		#region GetClip()
		System.Windows.Media.Imaging.BitmapSource GetClip(System.Drawing.Bitmap bitmap, System.Drawing.Rectangle clip)
		{
			clip.Intersect(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));

			System.Drawing.Bitmap crop = ImageProcessing.ImageCopier.Copy(bitmap, clip);

			System.Windows.Media.Imaging.BitmapSource bmpSource = BscanILL.UI.Misc.GetBitmapSource(crop);
			crop.Dispose();

			return bmpSource;
		}
		#endregion

		#endregion
	
	}
}
