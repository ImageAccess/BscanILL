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
using ViewPane.Hierarchy;

namespace ViewPane.ItResults
{
	/// <summary>
	/// ItResultsPanel doesn't catch Opus Manager and Opus Wrapper events. Use ViewPane to let pane deal with those events.
	/// </summary>
	public partial class ItResultsPanel : UserControl, IDisposable
	{
		DisplayModeType		displayMode = DisplayModeType.TwoPages;
		Thumbnail			thumbnail = null;
		
		//public event ResultsThumbnailHnd ImageSelected;


		#region constructor
		public ItResultsPanel()
		{
			InitializeComponent();

			this.imagePaneL.ViewPanel = this;
			this.imagePaneL.ImageChanged += new ImageSelectedHnd(ImagePane_ImageChanged);
			
			this.imagePaneR.ViewPanel = this;
			this.imagePaneR.ImageChanged += new ImageSelectedHnd(ImagePane_ImageChanged);

			this.stripPane.ThumbnailSelected += new Thumbnail.ThumbnailSelectedHnd(Thumbnail_Selected);
		}
		#endregion


		#region enum DisplayModeType
		enum DisplayModeType
		{
			Single,
			TwoPages
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		
		public ViewPane.ItResults.Thumbnail			SelectedThumbnail { get { return this.stripPane.SelectedThumbnail; } }
		public List<ViewPane.ItResults.Thumbnail>	Thumbnails	 { get { return this.stripPane.Thumbnails; } }

#if DEBUG
		public ItResultsImagePane ImagePanelL { get { return this.imagePaneL; } }
		public ItResultsImagePane ImagePanelR { get { return this.imagePaneR; } }
#endif

		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		#region DisplayMode
		DisplayModeType DisplayMode
		{
			get { return this.displayMode; }
			set
			{
				if (this.displayMode != value)
				{
					this.displayMode = value;

					if (this.displayMode == DisplayModeType.Single)
					{
						this.imagePaneR.Clear();
						this.columnR.Width = new GridLength(0);
					}
					else
					{
						this.columnR.Width = new GridLength(1, GridUnitType.Star);
					}
				}
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			ViewPane.IP.ItResultsCreator.Dispose();
			this.Clear();
		}
		#endregion
	
		#region Contains()
		public bool Contains(ViewPane.Hierarchy.VpImage vpImage)
		{
			foreach (ViewPane.ItResults.Thumbnail t in this.Thumbnails)
				if (t.VpImage == vpImage)
					return true;
			
			return false;
		}
		#endregion

		#region AddImage()
		public ViewPane.ItResults.Thumbnail AddImage(ViewPane.Hierarchy.VpImage vpImage)
		{
			if (this.Dispatcher.CheckAccess())
				return AddImageTU(vpImage);
			else
				return (ViewPane.ItResults.Thumbnail)this.Dispatcher.Invoke((Func<ViewPane.ItResults.Thumbnail>)delegate() { return AddImageTU(vpImage); });
		}
		#endregion

		#region InsertThumbnailBefore()
		public ViewPane.ItResults.Thumbnail InsertThumbnailBefore(ViewPane.Hierarchy.VpImage vpImage, ViewPane.Hierarchy.VpImage currentImage)
		{
			if (this.Dispatcher.CheckAccess())
				return InsertBeforeTU(vpImage, currentImage);
			else
				return (ViewPane.ItResults.Thumbnail)this.Dispatcher.Invoke((Func<ViewPane.ItResults.Thumbnail>)delegate() { return InsertBeforeTU(vpImage, currentImage); });
		}
		#endregion

		#region InsertThumbnailAfter()
		public ViewPane.ItResults.Thumbnail InsertThumbnailAfter(ViewPane.Hierarchy.VpImage vpImage, ViewPane.Hierarchy.VpImage currentImage)
		{
			if (this.Dispatcher.CheckAccess())
				return InsertAfterTU(vpImage, currentImage);
			else
				return (ViewPane.ItResults.Thumbnail)this.Dispatcher.Invoke((Func<ViewPane.ItResults.Thumbnail>)delegate() { return InsertAfterTU(vpImage, currentImage); });
		}
		#endregion

		#region RemoveThumbnail()
		/// <summary>
		/// returns thumbnail that was selected after removal
		/// </summary>
		/// <param name="thumbnail"></param>
		/// <returns></returns>
		public ViewPane.ItResults.Thumbnail RemoveThumbnail(ViewPane.Hierarchy.VpImage vpImage)
		{
			if (this.Dispatcher.CheckAccess())
				return RemoveThumbnailTU(vpImage);
			else
				return (ViewPane.ItResults.Thumbnail)this.Dispatcher.Invoke((Func<ViewPane.ItResults.Thumbnail>)delegate() { return RemoveThumbnailTU(vpImage); });
		}
		#endregion

		#region Lock()
		public void Lock()
		{
			if (this.Dispatcher.CheckAccess())
				LockTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { LockTU(); });
		}
		#endregion

		#region LockWithProgress()
		public void LockWithProgress()
		{
			if (this.Dispatcher.CheckAccess())
				LockWithProgressTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { LockWithProgressTU(); });
		}
		#endregion

		#region ProgressChanged()
		public void ProgressChanged(double progress)
		{
			if (this.Dispatcher.CheckAccess())
				ProgressChangedTU(progress);
			else
				this.Dispatcher.Invoke((Action)delegate() { ProgressChangedTU(progress); });
		}
		#endregion

		#region UnLock()
		public void UnLock()
		{
			if (this.Dispatcher.CheckAccess())
				UnLockTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { UnLockTU(); });
		}
		#endregion

		#region SelectImage()
		public void SelectImage(ViewPane.Hierarchy.VpImage vpImage)
		{
			if (this.Dispatcher.CheckAccess())
				SelectImageTU(vpImage);
			else
				this.Dispatcher.Invoke((Action)delegate() { SelectImageTU(vpImage); });
		}
		#endregion

		#region SelectFirstImage()
		public void SelectFirstImage()
		{
			if (this.Dispatcher.CheckAccess())
				SelectFirstImageTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { SelectFirstImageTU(); });
		}
		#endregion

		#region SelectPreviousImage()
		public void SelectPreviousImage()
		{
			if (this.Dispatcher.CheckAccess())
				SelectPreviousImageTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { SelectPreviousImageTU(); });
		}
		#endregion

		#region SelectNextImage()
		public void SelectNextImage()
		{
			if (this.Dispatcher.CheckAccess())
				SelectNextImageTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { SelectNextImageTU(); });
		}
		#endregion

		#region SelectLastImage()
		public void SelectLastImage()
		{
			if (this.Dispatcher.CheckAccess())
				SelectLastImageTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { SelectLastImageTU(); });
		}
		#endregion

		#region ShowDefaultImage()
		public void ShowDefaultImage()
		{
			if (this.Dispatcher.CheckAccess())
				ShowDefaultImageTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { ShowDefaultImageTU(); });
		}
		#endregion

		#region Clear()
		public void Clear()
		{
			if (this.Dispatcher.CheckAccess())
				ClearTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { ClearTU(); });
		}
		#endregion

		#region Refresh()
		/*public void Refresh()
		{
			if (this.Dispatcher.CheckAccess())
				RefreshTU();
			else
				this.Dispatcher.Invoke((Action)delegate() { RefreshTU(); });
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region LockTU()
		private void LockTU()
		{
			this.IsEnabled = false;
			this.imagePaneL.LockUi();
			this.imagePaneR.LockUi();
		}

		private void LockWithProgressTU()
		{
			this.IsEnabled = false;
			this.imagePaneL.LockUiWithProgreess();
			this.imagePaneR.LockUiWithProgreess();
		}
		#endregion

		#region ProgressChangedTU()
		private void ProgressChangedTU(double progress)
		{
			this.imagePaneL.ProgressChanged(progress);
			this.imagePaneR.ProgressChanged(progress);
		}
		#endregion

		#region UnLockTU()
		private void UnLockTU()
		{
			this.imagePaneL.UnLockUi();
			this.imagePaneR.UnLockUi();
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

		#region Thumbnail_Selected()
		void Thumbnail_Selected(ViewPane.ItResults.Thumbnail thumbnail)
		{
			ShowThumbnail(thumbnail);
		}
		#endregion

		#region AddImageTU()
		ViewPane.ItResults.Thumbnail AddImageTU(ViewPane.Hierarchy.VpImage vpImage)
		{
			try
			{
				ViewPane.ItResults.Thumbnail t = new ViewPane.ItResults.Thumbnail(vpImage, this.stripPane);
				
				this.stripPane.AddThumbnail(t);
				return t;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}
		#endregion

		#region InsertBeforeTU()
		ViewPane.ItResults.Thumbnail InsertBeforeTU(ViewPane.Hierarchy.VpImage vpImage, ViewPane.Hierarchy.VpImage currentImage)
		{
			try
			{
				ViewPane.ItResults.Thumbnail t = new ViewPane.ItResults.Thumbnail(vpImage, this.stripPane);

				this.stripPane.InsertThumbnailBefore(currentImage, t);
				return t;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}
		#endregion

		#region InsertAfterTU()
		ViewPane.ItResults.Thumbnail InsertAfterTU(ViewPane.Hierarchy.VpImage vpImage, ViewPane.Hierarchy.VpImage currentImage)
		{
			try
			{
				ViewPane.ItResults.Thumbnail t = new ViewPane.ItResults.Thumbnail(vpImage, this.stripPane);
				this.stripPane.InsertThumbnailAfter(currentImage, t);
				return t;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}
		#endregion

		#region RemoveThumbnailTU()
		ViewPane.ItResults.Thumbnail RemoveThumbnailTU(ViewPane.Hierarchy.VpImage vpImage)
		{
			try
			{
				ViewPane.ItResults.Thumbnail t = GetThumbnail(vpImage);
				bool showNewlySelectedThumbnail = false;

				if (this.imagePaneL.Thumbnail == t)
				{
					this.imagePaneL.Clear();
					this.imagePaneR.Clear();
					showNewlySelectedThumbnail = true;
				}

				ViewPane.ItResults.Thumbnail newlySelectedThumbnail = this.stripPane.RemoveThumbnail(t);

				if (showNewlySelectedThumbnail && newlySelectedThumbnail != null)
					newlySelectedThumbnail.Select();

				return newlySelectedThumbnail;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}
		#endregion

		#region ShowDefaultImageTU()
		void ShowDefaultImageTU()
		{
			if (this.thumbnail != null && this.thumbnail.ItImage != null)
				this.thumbnail.ItImage.Changed -= new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);

			this.thumbnail = null;

			this.stripPane.UncheckAll();
			this.imagePaneL.Clear();
			this.imagePaneR.Clear();
		}
		#endregion

		#region SelectImageTU()
		void SelectImageTU(ViewPane.Hierarchy.VpImage vpImage)
		{
			try
			{
				ViewPane.ItResults.Thumbnail t = GetThumbnail(vpImage);

				if (t != null)
					this.stripPane.SelectThumbnail(t);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region SelectFirstImageTU()
		void SelectFirstImageTU()
		{
			this.stripPane.SelectFirstThumbnail();
		}
		#endregion

		#region SelectPreviousImageTU()
		void SelectPreviousImageTU()
		{
			this.stripPane.SelectPreviousThumbnail();
		}
		#endregion

		#region SelectNextImageTU()
		void SelectNextImageTU()
		{
			this.stripPane.SelectNextThumbnail();
		}
		#endregion

		#region SelectLastImageTU()
		void SelectLastImageTU()
		{
			this.stripPane.SelectLastThumbnail();
		}
		#endregion

		#region ClearTU()
		void ClearTU()
		{
			ShowDefaultImage();
			this.stripPane.Clear();
			this.imagePaneL.Reset();
			this.imagePaneR.Reset();
		}
		#endregion

		#region ImagePane_ImageChanged()
		void ImagePane_ImageChanged(VpImage vpImage)
		{
			//if (ImageSelected != null)
			//	ImageSelected(this.imagePane.Thumbnail);
		}
		#endregion

		#region GetThumbnail()
		private ViewPane.ItResults.Thumbnail GetThumbnail(ViewPane.Hierarchy.VpImage vpImage)
		{
			return this.stripPane.GetThumbnail(vpImage);
		}
		#endregion

		#region ShowThumbnail()
		private void ShowThumbnail(ViewPane.ItResults.Thumbnail t)
		{
			try
			{
				if (this.thumbnail != null && this.thumbnail.ItImage != null)
					this.thumbnail.ItImage.Changed -= new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);

				this.thumbnail = t;

				if (this.thumbnail != null && this.thumbnail.ItImage != null)
					this.thumbnail.ItImage.Changed += new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);

				AdjustUi();
			}
			catch (Exception ex)
			{
				MessageBox.Show(Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region RefreshTU()
		/*private void RefreshTU()
		{
			try
			{

			}
			catch (Exception ex)
			{
				MessageBox.Show(Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}*/
		#endregion

		#region ItImage_ItPagesChanged()
		void ItImage_Changed(ImageProcessing.IpSettings.ItImage itImage, ImageProcessing.IpSettings.ItProperty type)
		{
			if ((type & ImageProcessing.IpSettings.ItProperty.Clip) > 0)
				AdjustUi();
		}
		#endregion

		#region AdjustUi()
		void AdjustUi()
		{
			if (this.thumbnail != null)
			{
				if (thumbnail.VpImage != null && thumbnail.VpImage.IsFixed == false && thumbnail.VpImage.TwoPages)
				{
					this.DisplayMode = DisplayModeType.TwoPages;
					this.imagePaneL.ShowImage(thumbnail, true);
					this.imagePaneR.ShowImage(thumbnail, false);
				}
				else
				{
					this.DisplayMode = DisplayModeType.Single;
					this.imagePaneL.ShowImage(thumbnail, true);
				}
			}
			else
			{
				this.DisplayMode = DisplayModeType.Single;
				this.imagePaneL.Clear();
			}
		}
		#endregion
	
		#endregion

	}
}
