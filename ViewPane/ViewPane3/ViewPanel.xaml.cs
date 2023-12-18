using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewPane.ImagePanel;
using ViewPane.Thumbnails;
using ViewPane.Hierarchy;

namespace ViewPane
{
	/// <summary>
	/// ViewPanel doesn't catch Opus Manager and Opus Wrapper events. Use ViewPane to let pane deal with those events.
	/// </summary>
	public partial class ViewPanel : UserControl
	{
		public event ImageSelectedHnd ImageSelected;
		public event ZoneSelectedHnd ZoneSelected;

		ViewPanel.Status status = ViewPanel.Status.Visible;

		delegate void BitmapHnd(System.Drawing.Bitmap bitmap);
		delegate void FileHnd(string filePath);
		delegate void VoidHnd();
		delegate void ProgressHnd(double progress);

		delegate void		AddImageHnd(VpImage vpImage);
		delegate void		InsertImageHnd(VpImage vpImage, VpImage lastActiveImage);
		delegate VpImage	RemoveImageHnd(VpImage vpImage);
		delegate void		UpdateImageHnd(VpImage vpImage);

		delegate void		SelectImageHnd(VpImage vpImage);


		VoidHnd				dlgDispose;
		AddImageHnd			dlgAddImage;
		InsertImageHnd		dlgInsertImageBefore;
		InsertImageHnd		dlgInsertImageAfter;
		RemoveImageHnd		dlgRemoveImage;

		BitmapHnd		dlgShowBitmap;
		FileHnd			dlgShowFile;
		VoidHnd			dlgLock;
		VoidHnd			dlgLockWithProgress;
		ProgressHnd		dlgProgressChanged;
		VoidHnd			dlgUnlock;
		VoidHnd			dlgShowDefaultImage;
		SelectImageHnd	dlgSelectImage;
		VoidHnd			dlgSelectFirstImage;
		VoidHnd			dlgSelectPreviousImage;
		VoidHnd			dlgSelectNextImage;
        VoidHnd         dlgSelectNextArticle;
        VoidHnd         dlgSelectPreviousArticle;
		VoidHnd			dlgSelectLastImage;
		VoidHnd			dlgClear;

		delegate VpImages							GetImagesHnd();
		delegate VpImage							GetSelectedImageHnd();
		delegate ViewPanel.Status					GetPaneStatusHnd();
		delegate void								SetPaneStatusHnd(ViewPanel.Status status);
		delegate void								SetToolbarSelectionHnd(ViewPane.ToolbarSelection toolbarSelection);
		delegate ViewPane.Licensing			GetImageTreatmentHnd();
		delegate void								SetImageTreatmentHnd(ViewPane.Licensing licensing);
		delegate int								GetSelectedImageIndexHnd();

		GetImagesHnd				dlgGetImages;
		GetSelectedImageHnd			dlgGetSelectedImage;
		GetPaneStatusHnd			dlgGetPaneStatus;
		SetPaneStatusHnd			dlgSetPaneStatus;
		SetToolbarSelectionHnd		dlgSetToolbarSelection;
		GetImageTreatmentHnd		dlgGetImageTreatment;
		SetImageTreatmentHnd		dlgSetImageTreatment;
		GetSelectedImageIndexHnd	dlgGetSelectedImageIndex;



		#region constructor
		public ViewPanel()
		{
			InitializeComponent();

			this.dlgDispose = new VoidHnd(DisposeTU);
			this.dlgAddImage = new AddImageHnd(AddImageTU);
			this.dlgInsertImageBefore = new InsertImageHnd(InsertImageBeforeTU);
			this.dlgInsertImageAfter = new InsertImageHnd(InsertImageAfterTU);
			this.dlgRemoveImage = new RemoveImageHnd(RemoveImageTU);

			this.imagePane.ViewPanel = this;
			this.imagePane.ImageChanged += new ImageSelectedHnd(ImagePane_ImageChanged);

			dlgLock = new VoidHnd(LockTU);
			dlgLockWithProgress = new VoidHnd(LockWithProgressTU);
			dlgProgressChanged = new ProgressHnd(ProgressChangedTU);
			dlgUnlock = new VoidHnd(UnLockTU);
			dlgShowBitmap = new BitmapHnd(ShowImageTU);
			dlgShowFile = new FileHnd(ShowImageTU);
			dlgShowDefaultImage = new VoidHnd(ShowDefaultImageTU);
			dlgSelectImage = new SelectImageHnd(SelectImageTU);
			dlgSelectFirstImage = new VoidHnd(SelectFirstImageTU);
			dlgSelectPreviousImage = new VoidHnd(SelectPreviousImageTU);
			dlgSelectNextImage = new VoidHnd(SelectNextImageTU);
            dlgSelectNextArticle = new VoidHnd(SelectNextArticleTU);
            dlgSelectPreviousArticle = new VoidHnd(SelectPreviousArticleTU);            
			dlgSelectLastImage = new VoidHnd(SelectLastImageTU);

			dlgClear = new VoidHnd(ClearTU);

			//this.stripPane.ThumbnailChecked += new RoutedEventHandler(Thumbnail_Checked);
			this.stripPane.ImageSelected += new StripPane.ImageSelectedHnd(Strip_ImageSelected);
			this.imagePane.RegionSelected += new ImagePane.RegionSelectedHandler(ImagePane_RegionSelected);

			this.dlgGetImages = delegate() { return this.ImagesTU; };
			this.dlgGetSelectedImage = delegate() { return this.SelectedImageTU; };
			this.dlgGetPaneStatus = delegate() { return this.PaneStatusTU; };
			this.dlgSetPaneStatus = delegate(ViewPanel.Status status) { this.PaneStatusTU = status; };
			this.dlgSetToolbarSelection = delegate(ViewPane.ToolbarSelection toolbarSelection) { this.ToolbarSelectionTU = toolbarSelection; };
			this.dlgGetImageTreatment = delegate() { return LicensingTU; };
			this.dlgSetImageTreatment = delegate(ViewPane.Licensing licensing) { this.LicensingTU = licensing; };
			this.dlgGetSelectedImageIndex = delegate() { return this.SelectedImageIndexTU; };

			ViewPane.Misc.MemoryManagement.Create(this);
		}
		#endregion


		#region enum Status
		public enum Status
		{
			Visible,
			Hidden
		}
		#endregion

	
		//PUBLIC PROPERTIES
		#region public properties

		#region SelectedImage
		public VpImage SelectedImage	
		{
			get
			{
				if (this.Dispatcher.CheckAccess())
					return SelectedImageTU;
				else
					return (VpImage)this.Dispatcher.Invoke(dlgGetSelectedImage);
			} 
		}
		#endregion

		#region Images
		public VpImages Images
		{ 
			get 
			{
				if (this.Dispatcher.CheckAccess())
					return ImagesTU;
				else
					return (VpImages)this.Dispatcher.Invoke(this.dlgGetImages);
			} 
		}
		#endregion

		#region SelectedItImage
		public ImageProcessing.IpSettings.ItImage SelectedItImage 
		{ 
			get
			{
				VpImage selectedImage = this.SelectedImage;

				if (selectedImage != null)
					return selectedImage.ItImage;

				return null;
				//return this.imagePane.IImage != null ? this.imagePane.IImage.ItImage : null; 
			} 
		}
		#endregion

		#region PaneStatus
		public ViewPanel.Status PaneStatus 
		{
			get
			{
				if (this.Dispatcher.CheckAccess())
					return PaneStatusTU;
				else
					return (ViewPanel.Status)this.Dispatcher.Invoke(dlgGetPaneStatus); 
			}
			set
			{
				if (this.Dispatcher.CheckAccess())
					PaneStatusTU = value;
				else
					this.Dispatcher.Invoke(dlgSetPaneStatus, value); 
			}
		}
		#endregion

		#region AllowTransforms
		public bool AllowTransforms
		{
			get { return this.Licensing.PostProcessing != PostProcessingMode.Disabled; }
		}
		#endregion

		#region ToolbarSelection
		public ViewPane.ToolbarSelection ToolbarSelection
		{
			set 
			{
				if (this.Dispatcher.CheckAccess())
					ToolbarSelectionTU = value;
				else
					this.Dispatcher.Invoke(this.dlgSetToolbarSelection, value);
			}
		}
		#endregion

		#region Licensing
		public ViewPane.Licensing Licensing
		{
			get
			{
				if (this.Dispatcher.CheckAccess())
					return LicensingTU;
				else
					return (ViewPane.Licensing)this.Dispatcher.Invoke(this.dlgGetImageTreatment); 
			}
			set 
			{
				if (this.Dispatcher.CheckAccess())
					LicensingTU = value;
				else
					this.Dispatcher.Invoke(this.dlgSetImageTreatment, value);
			}
		}
		#endregion

		#region SelectedImageIndex
		/// <summary>
		/// returns -1 if no thumbnail is selected
		/// </summary>
		public int SelectedImageIndex
		{ 
			get
			{
				if (this.Dispatcher.CheckAccess())
					return SelectedImageIndexTU;
				else
					return (int) this.Dispatcher.Invoke(this.dlgGetSelectedImageIndex);
			} 
		}
		#endregion

		#region ItImages
		public ImageProcessing.IpSettings.ItImages ItImages
		{
			get
			{
				ImageProcessing.IpSettings.ItImages itImages = new ImageProcessing.IpSettings.ItImages();

				foreach (VpImage vpImage in this.Images)
					if (vpImage.ItImage != null && vpImage.ImageType == VpImage.VpImageType.ScanImage)
						itImages.Add(vpImage.ItImage);

				return itImages;
			}
		}
		#endregion
		
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		VpImage		SelectedImageTU { get { return this.stripPane.SelectedImage; } }
		VpImages	ImagesTU { get { return this.stripPane.Images; } }

		#region PaneStatusTU
		ViewPanel.Status PaneStatusTU
		{
			get { return this.status; }
			set
			{
				this.status = value;

				if (this.status == ViewPanel.Status.Hidden)
				{
					this.stripPane.UncheckAll();
					this.imagePane.Clear();
				}
			}
		}
		#endregion

		#region ToolbarSelectionTU
		ViewPane.ToolbarSelection ToolbarSelectionTU
		{
			set { this.imagePane.ToolbarSelection = value; }
		}
		#endregion

		#region LicensingTU
		ViewPane.Licensing LicensingTU
		{
			get { return this.imagePane.Licensing; }
			set
			{
				this.imagePane.Licensing = value;
				this.stripPane.Licensing = value;
			}
		}
		#endregion

		#region SelectedImageIndexTU
		/// <summary>
		/// returns -1 if no thumbnail is selected
		/// </summary>
		int SelectedImageIndexTU { get { return this.stripPane.SelectedImageIndex; } }
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			if (this.Dispatcher.CheckAccess())
				DisposeTU();
			else
				this.Dispatcher.Invoke(this.dlgDispose);

			ViewPane.Misc.MemoryManagement.Dispose(this);
		}
		#endregion

		#region Close()
		public void Close()
		{
		}
		#endregion		

		#region AddImage()
		public void AddImage(VpImage vpImage)
		{
			if (this.Dispatcher.CheckAccess())
				AddImageTU(vpImage);
			else
				this.Dispatcher.Invoke(dlgAddImage, vpImage);
		}
		#endregion

		#region InsertImageBefore()
		public void InsertImageBefore(VpImage vpImage, VpImage lastActiveImage)
		{
			if (this.Dispatcher.CheckAccess())
				InsertImageBeforeTU(vpImage, lastActiveImage);
			else
				this.Dispatcher.Invoke(dlgInsertImageBefore, vpImage, lastActiveImage);
		}
		#endregion

		#region InsertImageAfter()
		public void InsertImageAfter(VpImage vpImage, VpImage lastActiveImage)
		{
			if (this.Dispatcher.CheckAccess())
				InsertImageAfterTU(vpImage, lastActiveImage);
			else
				this.Dispatcher.Invoke(dlgInsertImageAfter, vpImage, lastActiveImage);
		}
		#endregion

		#region RemoveImage()
		public VpImage RemoveImage(VpImage vpImage)
		{
			if (this.Dispatcher.CheckAccess())
				return RemoveImageTU(vpImage);
			else
				return (VpImage)this.Dispatcher.Invoke(dlgRemoveImage, vpImage);
		}
		#endregion

		#region Lock()
		public void Lock()
		{
			if (this.Dispatcher.CheckAccess())
				LockTU();
			else
				this.Dispatcher.Invoke(dlgLock);
		}
		#endregion

		#region LockWithProgress()
		public void LockWithProgress()
		{
			if (this.Dispatcher.CheckAccess())
				LockWithProgressTU();
			else
				this.Dispatcher.Invoke(dlgLockWithProgress);
		}
		#endregion

		#region ProgressChanged()
		public void ProgressChanged(double progress)
		{
			if (this.Dispatcher.CheckAccess())
				ProgressChangedTU(progress);
			else
				this.Dispatcher.Invoke(dlgProgressChanged, progress);
		}
		#endregion

		#region UnLock()
		public void UnLock()
		{
			if (this.Dispatcher.CheckAccess())
				UnLockTU();
			else
				this.Dispatcher.Invoke(dlgUnlock);
		}
		#endregion

		#region ShowImage()
		public void ShowImage(System.Drawing.Bitmap bitmap)
		{
			if (this.Dispatcher.CheckAccess())
				ShowImageTU(bitmap);
			else
				this.Dispatcher.Invoke(dlgShowBitmap, bitmap);
		}

		public void ShowImage(string filePath)
		{
			if (this.Dispatcher.CheckAccess())
				ShowImageTU(filePath);
			else
				this.Dispatcher.Invoke(dlgShowFile, filePath);
		}
		#endregion

		#region SelectImage()
		public void SelectImage(VpImage vpImage)
		{
			if (this.Dispatcher.CheckAccess())
				SelectImageTU(vpImage);
			else
				this.Dispatcher.Invoke(dlgSelectImage, vpImage);
		}
		#endregion

		#region SelectFirstImage()
		public void SelectFirstImage()
		{
			if (this.Dispatcher.CheckAccess())
				SelectFirstImageTU();
			else
				this.Dispatcher.Invoke(dlgSelectFirstImage);
		}
		#endregion

		#region SelectPreviousImage()
		public void SelectPreviousImage()
		{
			if (this.Dispatcher.CheckAccess())
				SelectPreviousImageTU();
			else
				this.Dispatcher.Invoke(dlgSelectPreviousImage); 
		}
		#endregion

		#region SelectNextImage()
		public void SelectNextImage()
		{
			if (this.Dispatcher.CheckAccess())
				SelectNextImageTU();
			else
				this.Dispatcher.Invoke(dlgSelectNextImage);
		}
		#endregion

        #region SelectNextArticle()
        public void SelectNextArticle()
		{
			if (this.Dispatcher.CheckAccess())
				SelectNextArticleTU();
			else
				this.Dispatcher.Invoke(dlgSelectNextArticle);
		}
		#endregion

        #region SelectPreviousArticle()
        public void SelectPreviousArticle()
		{
			if (this.Dispatcher.CheckAccess())
				SelectPreviousArticleTU();
			else
				this.Dispatcher.Invoke(dlgSelectPreviousArticle);
		}
		#endregion

		#region SelectLastImage()
		public void SelectLastImage()
		{
			if (this.Dispatcher.CheckAccess())
				SelectLastImageTU();
			else
				this.Dispatcher.Invoke(dlgSelectLastImage); 
		}
		#endregion

		#region ShowDefaultImage()
		public void ShowDefaultImage()
		{
			if (this.Dispatcher.CheckAccess())
				ShowDefaultImageTU();
			else
				this.Dispatcher.Invoke(dlgShowDefaultImage);
		}
		#endregion

		#region Clear()
		public void Clear()
		{
			if (this.Dispatcher.CheckAccess())
				ClearTU();
			else
				this.Dispatcher.Invoke(dlgClear);
		}
		#endregion

		#region KeyDown_Occured()
		public void KeyDown_Occured(KeyEventArgs e)
		{
			this.imagePane.KeyDown_Occured(e);
		}
		#endregion

		#region KeyUp_Occured()
		public void KeyUp_Occured(KeyEventArgs e)
		{
			this.imagePane.KeyUp_Occured(e);
		}
		#endregion

		#region SetPaneStatus()
		/*public void SetPaneStatus(ViewPanel.Status status, int? selectedScanId)
		{
			this.status = status;

			if (this.status == ViewPanel.Status.Hidden)
			{
				this.stripPane.UncheckAll();
				this.imagePane.Clear();
			}
			else
			{
				if (selectedScanId != null && this.wrapper != null)
				{
					OpusObjectManagerNS.FullScanData scanData = this.wrapper.GetOneImageDataSnapShotByScanID(selectedScanId.Value);

					if (scanData != null)
						this.SelectImage(scanData);
				}
			}
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region DisposeTU()
		private void DisposeTU()
		{
			Clear();
			this.imagePane.Dispose();
		}
		#endregion

		#region LockTU()
		private void LockTU()
		{
			this.IsEnabled = false;
			this.imagePane.LockUi();
		}

		private void LockWithProgressTU()
		{
			this.IsEnabled = false;
			this.imagePane.LockUiWithProgreess();
		}
		#endregion

		#region ProgressChangedTU()
		private void ProgressChangedTU(double progress)
		{
			this.imagePane.ProgressChanged(progress);
		}
		#endregion

		#region UnLockTU()
		private void UnLockTU()
		{
			this.imagePane.UnLockUi();
			this.IsEnabled = true;
		}
		#endregion

		#region Drag_Drop()
		private void Drag_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(StripPane)))
			{
				Dock? dock = GetDockStyle(e.GetPosition(this));

				if (dock.HasValue && dock.Value != stripPane.Dock)
					stripPane.Dock = dock.Value;
			}

			e.Handled = true;
		}
		#endregion

		#region Drag_Over()
		private void Drag_Over(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(StripPane)))
			{
				if (GetDockStyle(e.GetPosition(this)).HasValue)
					e.Effects = DragDropEffects.Move;
				else
					e.Effects = DragDropEffects.None;
			}
			else
				e.Effects = DragDropEffects.None;

			e.Handled = true;
		}
		#endregion

		#region GetDockStyle()
		private Dock? GetDockStyle(Point p)
		{
			if (p.X > 0 && p.X < 30)
				return Dock.Left;

			if (p.X > this.ActualWidth - 30 && p.X < this.ActualWidth)
				return Dock.Right;

			if (p.Y > 0 && p.Y < 30)
				return Dock.Top;

			if (p.Y > this.ActualHeight - 30 && p.Y < this.ActualHeight)
				return Dock.Bottom;

			return null;
		}
		#endregion

		#region Thumbnail_Checked()
		/*void Thumbnail_Checked(object sender, RoutedEventArgs e)
		{
			try
			{
				Thumbnail thumbnail = (Thumbnail)e.OriginalSource;

				if (thumbnail.ScanData != null)
					this.imagePane.ShowImage(this.wrapper, thumbnail.ScanData, thumbnail.ItImage);
				else if (thumbnail.ItImageData != null)
					this.imagePane.ShowImage(this.wrapper, thumbnail.ItImageData);
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}*/
		#endregion

		#region Strip_ImageSelected
		void Strip_ImageSelected(VpImage vpImage)
		{
			try
			{
				this.imagePane.ShowImage(vpImage);
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region AddImageTU()
		void AddImageTU(VpImage vpImage)
		{
			try
			{
				this.Images.Add(vpImage);
				vpImage.BringIntoView();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region InsertImageBeforeTU()
		void InsertImageBeforeTU(VpImage vpImage, VpImage selectedImage)
		{
			try
			{
				if (this.Images.IndexOf(selectedImage) >= 0)
					this.Images.Insert(this.Images.IndexOf(selectedImage), vpImage);
				else
					this.Images.Add(vpImage);

				vpImage.BringIntoView();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region InsertImageAfterTU()
		void InsertImageAfterTU(VpImage vpImage, VpImage selectedImage)
		{
			try
			{
				int index = this.Images.IndexOf(selectedImage);

				if (index >= 0 && index < this.Images.Count - 1)
					this.Images.Insert(index + 1, vpImage);
				else
					this.Images.Add(vpImage);

				vpImage.BringIntoView();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region RemoveImageTU()
		VpImage RemoveImageTU(VpImage vpImage)
		{
			try
			{
				VpImage vpImageToSelect = null;

				int index = this.Images.IndexOf(vpImage);

				if (index >= 0 && index < this.Images.Count - 1)
					vpImageToSelect = this.Images[index + 1];
				else if (index > 0)
					vpImageToSelect = this.Images[index - 1];

				this.Images.Remove(vpImage);

				if (vpImageToSelect == null)
					this.imagePane.Clear();

				return vpImageToSelect;
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
				return null;
			}
		}
		#endregion

		#region ShowImageTU()
		void ShowImageTU(System.Drawing.Bitmap bitmap)
		{
			this.stripPane.UncheckAll();
			this.imagePane.ShowImage(bitmap);
		}

		void ShowImageTU(string filePath)
		{
			this.stripPane.UncheckAll();
			this.imagePane.ShowImage(filePath);
		}
		#endregion

		#region ShowDefaultImageTU()
		void ShowDefaultImageTU()
		{
			this.stripPane.UncheckAll();
			this.imagePane.Clear();
		}
		#endregion

		#region SelectImageTU()
		void SelectImageTU(VpImage vpImage)
		{
			try
			{
				this.stripPane.SelectThumbnail(vpImage);
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region SelectFirstImageTU()
		void SelectFirstImageTU()
		{
			try
			{
				this.stripPane.SelectFirstThumbnail();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region SelectPreviousImageTU()
		void SelectPreviousImageTU()
		{
			try
			{
				this.stripPane.SelectPreviousThumbnail();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region SelectNextImageTU()
		void SelectNextImageTU()
		{
			try
			{
				this.stripPane.SelectNextThumbnail();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

        #region SelectNextArticleTU()
        void SelectNextArticleTU()
		{
			try
			{
				this.stripPane.SelectNextArticleThumbnail();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

        #region SelectPreviousArticleTU()
        void SelectPreviousArticleTU()
		{
			try
			{
				this.stripPane.SelectPreviousArticleThumbnail();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region SelectLastImageTU()
		void SelectLastImageTU()
		{
			try
			{
				this.stripPane.SelectLastThumbnail();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region ClearTU()
		void ClearTU()
		{
			this.Images.Clear();
		}
		#endregion

		#region ImagePane_RegionSelected()
		void ImagePane_RegionSelected(BIP.Geometry.RatioRect rectangle)
		{
			if (ZoneSelected != null)
				ZoneSelected(rectangle);
		}
		#endregion

		#region ImagePane_ImageChanged()
		void ImagePane_ImageChanged(VpImage vpImage)
		{
			//this.stripPane.SaveIT();
			
			if (ImageSelected != null)
				ImageSelected(vpImage);
		}
		#endregion

		#region ShowErrorMessage()
		void ShowErrorMessage(Exception ex)
		{
			MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		#endregion

		#endregion

	}
}
