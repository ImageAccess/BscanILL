using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using System.IO;
using System.Drawing.Printing;
using System.Windows.Input;
using System.Windows;
using System.Runtime.InteropServices;
using BscanILL.Misc;


namespace BscanILL.UI.Frames.Edit
{
	public class FrameEdit : FrameBase
	{
		#region variables

		private BscanILL.UI.Frames.Edit.FrameEditUi	frameEditUi;

		public event KeyEventHandler			KeyDown;
        BscanILL.IP.IllImageOperations illImageOperations = null;

		#endregion


		#region constructor
		public FrameEdit(BscanILL.MainWindow mainWindow)
			: base(mainWindow)
		{
			this.frameEditUi = this.MainWindow.FrameEditUi;
			
			//this.frameEditUi.ScanClick += new VoidHnd(Scan_Click);
            illImageOperations = new BscanILL.IP.IllImageOperations(this.MainWindow);

			frameEditUi.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(Preview_KeyDown);
            frameEditUi.RotateSmallAngle_Click += new BscanILL.IP.IllImageOperations.RotateImageHnd(RotateSmallAngleImage_Requested);			
			frameEditUi.Rotate90_Click += new BscanILL.IP.IllImageOperations.RotateImageHnd(RotateImage_Requested);			
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Hierarchy.IllImage			SelectedImage { get { return (BscanILL.Hierarchy.IllImage)this.frameEditUi.SelectedImage; } }
		public System.Windows.Controls.UserControl	UserControl { get { return (System.Windows.Controls.UserControl)this.frameEditUi; } }
		public BscanILL.Hierarchy.Article			Article { get { return this.MainWindow.Article; } }
        public BscanILL.Hierarchy.SessionBatch      Batch { get { return this.MainWindow.Batch; } }

		#endregion


		//PUBLIC METHODS
		#region public methods
	
		#region Activate()
		public void Activate()
		{
            this.frameEditUi.Open(this.Batch);
			
			this.IsActivated = true;
		}
		#endregion

		#region Deactivate()
		public void Deactivate()
		{
			try
			{
				this.frameEditUi.Reset(this.IsActivated);

				ReleaseMemory();

				this.frameEditUi.Visibility = Visibility.Hidden;
				this.IsActivated = false;
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
			}
			finally
			{
			}
		}
		#endregion

        #region InitiatePageDown()
        public void InitiatePageDown()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameEditUi.viewPanel.SelectNextImage();
            }
        }
        #endregion

        #region InitiateArticleDown()
        public void InitiateArticleDown()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameEditUi.viewPanel.SelectPreviousArticle();
            }
        }
        #endregion

        #region InitiateArticleUp()
        public void InitiateArticleUp()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameEditUi.viewPanel.SelectNextArticle();
            }
        }
        #endregion

        #region InitiatePageUp()
        public void InitiatePageUp()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameEditUi.viewPanel.SelectPreviousImage();
            }
        }
        #endregion

        #region InitiateGoToHome()
        public void InitiateGoToHome()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameEditUi.viewPanel.SelectFirstImage();
            }
        }
        #endregion

        #region InitiateGoToEnd()
        public void InitiateGoToEnd()
        {
            if (this.IsActivated && this.IsEnabled)
            {
                this.frameEditUi.viewPanel.SelectLastImage();
            }
        }
		#endregion

		#region InitiateRotate90CV()
		public void InitiateRotate90CV()
		{
			if (this.IsActivated && this.IsEnabled)
			{
				this.frameEditUi.ControlPanel_Rotate90CVClick();
			}
		}
		#endregion

		#region InitiateRotate90CCV()
		public void InitiateRotate90CCV()
		{
			if (this.IsActivated && this.IsEnabled)
			{
				this.frameEditUi.ControlPanel_Rotate90CCVClick();
			}
		}
		#endregion

		#region AddPages()
		/*public void AddPages(string fileL, string fileR, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, ushort dpi, double brightness, double contrast, int pixelsOverlapping)
		{
			BscanILL.Hierarchy.IllImage illImage1 = this.Article.Scans.Add(this.Article, new FileInfo(fileL), colorMode, fileFormat, dpi, brightness, contrast);
			BscanILL.Hierarchy.IllImage illImage2 = this.Article.Scans.Add(this.Article, new FileInfo(fileR), colorMode, fileFormat, dpi, brightness, contrast);
		}

		public void AddPages(Bitmap bitmapL, Bitmap bitmapR, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast, int pixelsOverlapping)
		{
			Scanners.FileFormat fileFormat = (colorMode == Scanners.ColorMode.Bitonal) ? Scanners.FileFormat.Png : Scanners.FileFormat.Jpeg;
			string filePathL = this.Article.GetIdenticalScanPath(fileFormat);

			if (fileFormat == Scanners.FileFormat.Png)
				bitmapL.Save(filePathL, System.Drawing.Imaging.ImageFormat.Png);
			else
				bitmapL.Save(filePathL, System.Drawing.Imaging.ImageFormat.Jpeg);

			bitmapL.Dispose();
			BscanILL.Hierarchy.IllImage illImageL = this.Article.Scans.Add(this.Article, new FileInfo(filePathL), colorMode, fileFormat, dpi, brightness, contrast);

			string filePathR = this.Article.GetIdenticalScanPath(fileFormat);
			if (fileFormat == Scanners.FileFormat.Png)
				bitmapR.Save(filePathR, System.Drawing.Imaging.ImageFormat.Png);
			else
				bitmapR.Save(filePathR, System.Drawing.Imaging.ImageFormat.Jpeg);

			bitmapR.Dispose();
			BscanILL.Hierarchy.IllImage illImage2 = this.Article.Scans.Add(this.Article, new FileInfo(filePathR), colorMode, fileFormat, dpi, brightness, contrast);
		}*/
		#endregion

		#region InsertImage()
		/*public void InsertImage(int index, string file, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, ushort dpi, double brightness, double contrast)
		{
			this.Article.Scans.Insert(index, this.Article, new FileInfo(file), colorMode, fileFormat, dpi, brightness, contrast);
		}

		public void InsertImage(int index, Bitmap bitmap, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast)
		{
			Scanners.FileFormat fileFormat = (colorMode == Scanners.ColorMode.Bitonal) ? Scanners.FileFormat.Png : Scanners.FileFormat.Jpeg;
			string filePath = this.Article.GetIdenticalScanPath(fileFormat);

			if (fileFormat == Scanners.FileFormat.Png)
				bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
			else
				bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);

			bitmap.Dispose();
			this.Article.Scans.Insert(index, this.Article, new FileInfo(filePath), colorMode, fileFormat, dpi, brightness, contrast);
		}*/
		#endregion

		#region InsertPages()
		/*public void InsertPages(int index, string fileL, string fileR, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, ushort dpi, double brightness, double contrast, int pixelsOverlapping)
		{
			BscanILL.Hierarchy.IllImage illImage1 = this.Article.Scans.Insert(index, this.Article, new FileInfo(fileL), colorMode, fileFormat, dpi, brightness, contrast);
			BscanILL.Hierarchy.IllImage illImage2 = this.Article.Scans.Insert(index + 1, this.Article, new FileInfo(fileR), colorMode, fileFormat, dpi, brightness, contrast);
		}*/
		#endregion

		#region ReleaseMemory()
		private void ReleaseMemory()
		{
			BscanILL.Misc.MemoryManagement.ReleaseUnusedMemory();
		}
		#endregion

		#region Dispose()
		public override void Dispose()
		{
			base.Dispose();
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region ShowDefaultImage()
		void ShowDefaultImage()
		{
			if (frameEditUi != null)
				frameEditUi.ShowDefaultImage();
		}
		#endregion

		#region ShowIllImage()
		void ShowIllImage(BscanILL.Hierarchy.IllImage illImage)
		{
			this.frameEditUi.ShowImage(illImage);
		}
		#endregion

		#region FrameWpf_ImageSelected()
		void FrameWpf_ImageSelected(BscanILL.UI.Misc.ImageEventArgs args)
		{
			/*BscanILL.Hierarchy.IllImage illImage = args.Image as BscanILL.Hierarchy.IllImage;
			bool formLocked = this.locked;

			try
			{
				Lock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "FrameScan, FrameWpf_ImageSelected: " + ex.Message, ex);
				ShowWarningMessage(this.MainWindow, BscanILL.Languages.BscanILLStrings.Frames_ErrorChangingImageSelection_STR, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				if (formLocked == false)
					UnLock();
			}*/
		}
		#endregion

		#region Crop_Requested()
		/*void Crop_Requested(BscanILL.IP.IllImageOperations.CropEventArgs args)
		{
			try
			{
				Lock();

				messageWindow.Show(this.MainWindow, "Please Wait...");
				this.illImageOperations.Crop(args);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
				UnLock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				ShowErrorMessage(this.MainWindow, "Can't crop image!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
				UnLock();
			}
		}*/
		#endregion

		#region SplitImage_Requested()
		/*void SplitImage_Requested(BscanILL.Hierarchy.IllImage image, int imageIndex)
		{
			try
			{
				Lock();

				messageWindow.Show(this.MainWindow, "Please Wait...");
				this.illImageOperations.SplitImage(image, imageIndex);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
				UnLock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				ShowErrorMessage(this.MainWindow, "Can't split image!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
				UnLock();
			}
		}*/
		#endregion

		#region Unsplit_Requested()
		/*void Unsplit_Requested(BscanILL.Hierarchy.IllImage image)
		{
			try
			{
				Lock();

				messageWindow.Show(this.MainWindow, "Please Wait...");
				this.illImageOperations.Unsplit(image);
			}
			catch (IllException ex)
			{
				ShowWarningMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
				UnLock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "FrameScan, Unsplit_Requested(): " + ex.Message, ex);
				ShowErrorMessage(this.MainWindow, "Can't split image!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
				UnLock();
			}
		}*/
		#endregion

		#region RotateImage_Requested()
		void RotateImage_Requested(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle)
		{
			try
			{
				Lock();

				this.illImageOperations.OperationSuccessfull += new BscanILL.IP.IllImageOperations.OperationSuccessfullHnd(Rotate_Successfull);

				//messageWindow.Show(this.MainWindow, "Please Wait...");
				this.illImageOperations.RotateImage(image, imageIndex, angle);
				//UnLock();
			}
			catch (IllException ex)
			{
				//ShowWarningMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);

				this.illImageOperations.OperationSuccessfull -= new BscanILL.IP.IllImageOperations.OperationSuccessfullHnd(Rotate_Successfull);
				ShowWarningMessage(ex.Message);
				UnLock();
			}
			catch (Exception ex)
			{
				this.illImageOperations.OperationSuccessfull -= new BscanILL.IP.IllImageOperations.OperationSuccessfullHnd(Rotate_Successfull);
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				//ShowErrorMessage(this.MainWindow, "Can't rotate image!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
                ShowErrorMessage("Can't rotate image!");
				UnLock();
			}
		}
		#endregion


        
		#region RotateSmallAngleImage_Requested()
        void RotateSmallAngleImage_Requested(BscanILL.Hierarchy.IllImage image, int imageIndex, int angle)
		{
			try
			{
				Lock();

				//messageWindow.Show(this.MainWindow, "Please Wait...");
                this.illImageOperations.RotateSmallAngleImage(image, imageIndex, angle);
                //UnLock();
			}
			catch (IllException ex)
			{
				//ShowWarningMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
                ShowWarningMessage(ex.Message);
				UnLock();
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				//ShowErrorMessage(this.MainWindow, "Can't rotate image!", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
                ShowErrorMessage("Can't small angle rotate image!");
				UnLock();
			}
		}
		#endregion

		#region DeleteCurrent_Request()
		/*void DeleteCurrent_Request(object sender, EventArgs e)
		{
			if (this.Article != null && this.SelectedImage != null)
				this.Article.Scans.Remove(this.SelectedImage);
		}*/
		#endregion

		#region DeleteAll_Request()
		/*void DeleteAll_Request(object sender, EventArgs e)
		{
			Lock();

			try
			{
				if ((this.Article != null) && (this.Article.Scans.Count <= 1 || BscanILL.UI.Dialogs.AlertDlg.Show(this.MainWindow, "Are you sure you want to delete all the images?", BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Question)))
				{
					ShowDefaultImage();

					this.Article.Scans.Clear();
				}
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, System.Reflection.MethodBase.GetCurrentMethod().Name, ex.Message, ex);
				ShowErrorMessage(ex.Message);
			}
			finally
			{
				UnLock();
			}
		}*/
		#endregion

		#region FrameWpf_PrintClick()
		/*void FrameWpf_PrintClick(BscanILL.ExportSettings exportSettings)
		{
			try
			{
				Lock();
				Kic.Hierarchy.IllImages illImages = (exportSettings.Images == BscanILL.ImagesSelection.All) ? this.IllImages : this.SelectedImages;
				//ShowDefaultImage();

				if (illImages != null && illImages.Count > 0)
				{
					BscanILL.Export.PrintingOptions printingOptions;

					if (exportSettings.Images == BscanILL.ImagesSelection.All || illImages.Count > 1)
						printingOptions = Kic.Export.Printing.PrintOptions.OpenSimple(illImages, this.MainWindow);
					else
						printingOptions = Kic.Export.Printing.PrintOptions.OpenSimpleArea(illImages[0], this.MainWindow, this.imageWindow);

					if (printingOptions != null)
					{
						Kic.Export.ExportDlg.WorkUnit workUnit = new Kic.Export.ExportDlg.WorkUnit(illImages, exportSettings);
						workUnit.Tag = printingOptions;

						RunExport(workUnit);
					}
				}
				else
					ShowWarningMessage(this.MainWindow, BscanILL.Languages.BscanILLStrings.FrameScan_ScanAndSelectSomeImagesFirst_STR, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Warning);
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "FrameScan, FrameWpf_PrintClick: " + ex.Message, ex);
				ShowErrorMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				UnLock();
			}
		}*/
		#endregion

		#region Rotate_Successfull()
		void Rotate_Successfull( BscanILL.Hierarchy.IllImage modifiedImage )
		{
			ViewPane.Hierarchy.VpImage vpImageModified = null;

			try
			{				
				this.illImageOperations.OperationSuccessfull -= new BscanILL.IP.IllImageOperations.OperationSuccessfullHnd(Rotate_Successfull);

				//this.frameEditUi.viewPanel.SelectedImage
				//this.frameEditUi.SelectedImage

				vpImageModified = this.frameEditUi.LookUpVpImage(modifiedImage);
				if (vpImageModified != null)
				{
					//ViewPane.Thumbnails.Thumbnail. ThumbnailCreatedEvent += new ViewPane.Thumbnails.Thumbnail.VoidEventHnd(UnlockAfterSuccessfullRotate);

					vpImageModified.UpdateAfterRotate += new ViewPane.Hierarchy.VpImage.VpImageEventHnd(UnlockAfterSuccessfullRotate);

					vpImageModified.RecreateImageFiles();
					//vpImageModified.IsIndependent = true;

					//UnLock();
				}
				else
                {
					UnLock();
				}
			}

			catch (Exception ex)
			{
				if (vpImageModified != null)
				{
					vpImageModified.UpdateAfterRotate -= new ViewPane.Hierarchy.VpImage.VpImageEventHnd(UnlockAfterSuccessfullRotate);
				}

				UnLock();
				throw ex;
			}			
		}
		#endregion

		#region UnlockAfterSuccessfullRotate()
		void UnlockAfterSuccessfullRotate(ViewPane.Hierarchy.VpImage vpImage)
		{
			vpImage.UpdateAfterRotate -= new ViewPane.Hierarchy.VpImage.VpImageEventHnd(UnlockAfterSuccessfullRotate);

			if( vpImage.ItImage != null)
            {
				vpImage.ItImage.Dispose();
				vpImage.ItImage = null;
			}
			vpImage.IsFixed = false;
			vpImage.IsIndependent = true;

			//check if itImage already update at this time
			UnLock();
		}
		#endregion

		#region Operation_Successfull()
		/*void Operation_Successfull()
		{
			UnLock();
		}*/
		#endregion

		#region Operation_Error()
		/*void Operation_Error(IllException ex)
		{
			ShowErrorMessage(ex.Message);
			UnLock();
		}*/
		#endregion

		#region DeleteImage()
		void DeleteImage(BscanILL.Hierarchy.IllImage illImage)
		{
			//this.Article.Scans.Remove(illImage);
            this.Batch.CurrentArticle.Scans.Remove(illImage);
		}
		#endregion

		#region UserInteracted()
		void UserInteracted()
		{
		}
		#endregion

		#region Preview_KeyDown()
		void Preview_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			bool shift = ((e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Shift) > 0);

			if (KeyDown != null)
				KeyDown(this, e);
		}
		#endregion

		#region Collate_Request()
		/*void Collate_Request(object sender, EventArgs e)
		{
			Lock();

			try
			{
				Kic.Dialogs.CollateDlg dlg = new Kic.Dialogs.CollateDlg(this.imageWindow);

				dlg.Open(this.MainWindow, this.session, this.frameEditUi.SelectedImage);
			}
			catch (Exception ex)
			{
				Notify(this, Notifications.Type.Error, "FrameScan, Collate_Request(): " + ex.Message, ex);
				ShowErrorMessage(this.MainWindow, ex.Message, BscanILL.UI.Dialogs.AlertDlg.AlertDlgType.Error);
			}
			finally
			{
				UnLock();
			}
		}*/
		#endregion

		#endregion

	}
}
