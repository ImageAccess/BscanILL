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
using BIP.Geometry;
using ViewPane;
using ViewPane.Toolbar;
using ViewPane.Hierarchy;


namespace ViewPane.ImagePanel
{
	/// <summary>
	/// Interaction logic for ImagePane.xaml
	/// </summary>
	public partial class ImagePane : UserControl, IImagePane, ViewPane.IP.IPreviewCaller
	{
		#region variables
		ViewPanel					viewPane = null;
		IViewImage					iImage = null;
		BitmapSource				defaultImage;
		object						locker = new object();

		DrawingStatus				drawingStatus = DrawingStatus.Idle;

		LastVpImageViewParams		lastVpImageViewParams = new LastVpImageViewParams();

		public delegate void		ZoomModeChangedHnd(ViewPane.ToolbarSelection oldZoomMode, ViewPane.ToolbarSelection newZoomMode);
		public delegate void		ZoomChangedHnd(double newZoom);
		
		public event ZoomModeChangedHnd			ZoomModeChanged;
		public event ZoomChangedHnd				ZoomChanged;
		
		double					zoom = 1.0;
		float[]					zoomSteps = { .0833F, .125F, .25F, .3333F, .50F, .6667F, .75F, 1F, 1.25F, 1.50F, 2F, 3F, 4F, 6F, 8F, 16F };
		BIP.Geometry.RatioRect	imageRect = BIP.Geometry.RatioRect.Empty;

		Point			startPoint = new Point();
		Rect			zoomRectangle = Rect.Empty;

		//events
		public delegate void RegionSelectedHandler(RatioRect rectangle);
		public event RegionSelectedHandler	RegionSelected;
		public event ImageSelectedHnd		ImageChanged;
		//public event ImageSelectedHnd		ScanDataChanged;

		public Cursor cursorHand = null;
		public Cursor cursorFist = null;
		public Cursor cursorZoomIn = null;
		public Cursor cursorZoomOut = null;
		public Cursor cursorZoomDynamic = null;

		delegate void ExceptionRaisedHnd(Exception ex);
		ExceptionRaisedHnd		dlgDrawingExceptionRaised;
		VpImage.VoidEventHnd	dlgImageChanged;
		VpImage.VoidEventHnd	dlgVpImageUpdated;

		ViewPane.Licensing licensing = new Licensing();

		delegate void ThumbnailCreatedHnd(System.Drawing.Bitmap bitmap, int renderingId, TimeSpan timeSpan);
		delegate void ThumbnailCreationErrorHnd(Exception ex);
		delegate void ThumbnailCreationCanceledHnd();
		delegate bool IsDisplayedHnd();

		ThumbnailCreatedHnd				dlgThumbnailCreated;
		ThumbnailCreationErrorHnd		dlgThumbnailCreationError;
		ThumbnailCreationCanceledHnd	dlgThumbnailCreationCanceled;
		IsDisplayedHnd					dlgIsDisplayed;

		System.Windows.Media.Animation.Storyboard lockUiStoryboard;
		int			renderingCounter = 0;
		int			lastRenderedId = -1;
		#endregion


		#region constructor
		public ImagePane()
		{
			InitializeComponent();

#if DEBUG
			try
			{
#endif
				this.lockUiStoryboard = this.Resources["lockStoryboardKey"] as System.Windows.Media.Animation.Storyboard;

				this.toolbar.Init(this, true, true, false, false, false, false, false, false, false);


				this.toolbar.ToolbarZoomMode.ZoomModeChanged += new ZoomModeChangedHnd(ZoomMode_Changed);

				this.toolbar.ToolbarZoomSize.ZoomTypeChanged += new ViewPane.Toolbar.ToolbarZoomSize.ZoomSizeEventHandler(ToolBar_ZoomTypeChanged);
				this.toolbar.ToolbarZoomSize.ZoomInRequest += new EventHandler(ZoomIn_Request);
				this.toolbar.ToolbarZoomSize.ZoomOutRequest += new EventHandler(ZoomOut_Request);

				this.toolbar.ToolbarItSettings.IndependentImageClick += new ViewPane.Toolbar.ToolbarItSettings.IndependentImageClickHnd(ToolbarItSettings_IndependentImageClick);
				this.toolbar.ToolbarItSettings.ClipsSameSizeClick += new EventHandler(ToolbarItSettings_ClipsSameSizeClick);

				this.toolbar.ToolbarItTransforms.ZoomModeChanged += new ZoomModeChangedHnd(ZoomMode_Changed);

				this.toolbar.ToolbarBookInfo.BookPartTypeChanged += new ViewPane.Toolbar.ToolbarBookInfo.BookPartTypeHnd(ToolbarBookInfo_BookPartTypeChanged);

				this.toolbar.ZoomModeChanged += delegate(ToolbarSelection oldZoomMode, ToolbarSelection newZoomMode)
				{
					if (ZoomModeChanged != null)
						ZoomModeChanged(oldZoomMode, newZoomMode);
				};

				defaultImage = new BitmapImage(new Uri("images/NoImage.jpg", UriKind.Relative));
				this.imageBox.Source = defaultImage;

				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

				//cursorHand = new Cursor(assembly.GetManifestResourceStream("ViewPane.CursorHand.cur"));
				//cursorFist = new Cursor(assembly.GetManifestResourceStream("ViewPane.CursorFist.cur"));
				//cursorZoomIn = new Cursor(assembly.GetManifestResourceStream("ViewPane.CursorZoomIn.cur"));
				//cursorZoomOut = new Cursor(assembly.GetManifestResourceStream("ViewPane.CursorZoomOut.cur"));
				//cursorZoomDynamic = new Cursor(assembly.GetManifestResourceStream("ViewPane.CursorZoomDynamic.cur"));

				cursorHand = ((FrameworkElement)this.Resources["cursorHand"]).Cursor;
				cursorFist = ((FrameworkElement)this.Resources["cursorFist"]).Cursor;
				cursorZoomIn = ((FrameworkElement)this.Resources["cursorZoomIn"]).Cursor;
				cursorZoomOut = ((FrameworkElement)this.Resources["cursorZoomOut"]).Cursor;
				cursorZoomDynamic = ((FrameworkElement)this.Resources["cursorZoomDynamic"]).Cursor;

				this.dlgDrawingExceptionRaised += delegate(Exception ex)
				{
					ShowErrorMessage(ex);
				};

				this.dlgImageChanged = new VpImage.VoidEventHnd(ItImageSettingsChangedTU);
				this.dlgVpImageUpdated = new VpImage.VoidEventHnd(VpImageUpdatedTU);

				this.dlgThumbnailCreated += new ThumbnailCreatedHnd(ThumbnailCreated);
				this.dlgThumbnailCreationError += new ThumbnailCreationErrorHnd(ThumbnailCreationError);
				this.dlgThumbnailCreationCanceled += new ThumbnailCreationCanceledHnd(ThumbnailCreationCanceled);
				this.dlgIsDisplayed += delegate() { return (this.IsVisible); };

				this.imageTreatmentUi.Init(this);
				//this.computingStoryboard.Begin();
#if DEBUG
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
#endif
		}
		#endregion

		#region class RegionSelectedArgs
		public class RegionSelectedArgs : EventArgs
		{
			public Rect Rect;
			public MouseButton MouseButton;

			public RegionSelectedArgs(Rect rectangle, MouseButton mouseButton)
			{
				this.Rect = rectangle;
				this.MouseButton = mouseButton;
			}
		}
		#endregion

		#region enum ToolbarType
		[Flags]
		public enum ToolbarType : long
		{
			ItScanImage = 1,
			ItTransforms = 2,
			Navigation = 4,
			Pages = 8,
			Treatment = 16,
			ZoomMode = 32,
			ZoomSize = 64,
			Scan = 128
		}
		#endregion

		#region DrawingStatus
		enum DrawingStatus
		{
			Idle,
			Computing
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		internal double					Zoom { get { return this.zoom; } }
		internal IViewImage				IImage { get { return iImage; } }
		internal BIP.Geometry.RatioRect ImageRect { get { return imageRect; } }
		//public Cursor		PictBoxCursor { get { return this.pictBox.Cursor; } set { this.pictBox.Cursor = value; } }


		#region ToolbarSelection
		internal ViewPane.ToolbarSelection ToolbarSelection
		{
			get { return this.toolbar.ToolbarSelection; }
			set { this.toolbar.ToolbarSelection = value; }
		}
		#endregion

		#region ZoomType
		ViewPane.ZoomType ZoomType
		{
			get { return this.toolbar.ZoomType; }
			set { this.toolbar.ZoomType = value; }
		}
		#endregion

		#region AllowTransforms
		internal bool AllowTransforms
		{
			get { return (this.Licensing.Ip == ImageProcessingMode.Basic || this.Licensing.Ip == ImageProcessingMode.Advanced); }
		}
		#endregion

		#region VisibleImagePortion
		public Rect VisibleImagePortion
		{
			get
			{
				return new Rect(imageRect.X / this.Zoom, imageRect.Y / this.Zoom, pictBox.ActualWidth / this.Zoom, pictBox.ActualHeight / this.Zoom);
			}
			set
			{
				if (this.iImage != null)
				{
					SetZoom(Math.Min(pictBox.ActualWidth / value.Width, pictBox.ActualHeight / value.Height));

					AdjustScrollBars();

					if (hScroll.IsEnabled)
						hScroll.Value = (int)Math.Max(0, Math.Min(hScroll.Maximum, value.X * this.Zoom));
					if (vScroll.IsEnabled)
						vScroll.Value = (int)Math.Max(0, Math.Min(vScroll.Maximum, value.Y * this.Zoom));

					AdjustToolbar();
					AdjustScrollBars();

					RedrawImage();
				}
			}
		}
		#endregion

		#region Licensing
		public ViewPane.Licensing Licensing
		{
			get { return this.licensing; }
			set 
			{ 
				this.licensing = value;
				
				this.toolbarPostProcessing.Set(this);
				AdjustToolbar();
			}
		}
		#endregion

		#region IsDisplayed()
		public bool IsDisplayed
		{
			get { return (bool)this.Dispatcher.Invoke(this.dlgIsDisplayed); }
		}
		#endregion

		#region DefaultImage
		public BitmapSource DefaultImage
		{
			set
			{
				if (this.Dispatcher.CheckAccess())
				{
					if (this.defaultImage != value)
					{
						this.defaultImage = value;
						RedrawImage();
					}
				}
				else
				{
					this.Dispatcher.Invoke((Action)delegate() 
					{
						if (this.defaultImage != value)
						{
							this.defaultImage = value;
							RedrawImage();
						}
					});
				}
			}
		}
		#endregion

		#endregion


		//INTERNAL PROPERTIES
		#region internal properties
		internal ViewPanel								ViewPanel { get { return this.viewPane; } set { this.viewPane = value; } }
		internal ImageProcessing.IpSettings.ItImages	ItImages { get { return this.viewPane.ItImages; } }
		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		#region Status
		DrawingStatus Status
		{
			get { return this.drawingStatus; }
			set
			{
				if (this.drawingStatus != value)
				{
					this.drawingStatus = value;

					if (this.drawingStatus == DrawingStatus.Idle)
					{
						this.computingGrid.Visibility = Visibility.Hidden;
						//this.computingStoryboard.Pause();
					}
					else
					{
						this.computingGrid.Visibility = Visibility.Visible;
						//this.computingStoryboard.Begin();
					}
				}
			}
		}
		#endregion

		#region BitmapSize
		/// <summary>
		/// returns full image size in pixels
		/// </summary>
		private Size BitmapSize 
		{ 
			get 
			{
				if (iImage != null)
					return this.iImage.FullImageSize;
				else
					return Size.Empty; 
			} 
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			ViewPane.IP.PreviewCreator creator = ViewPane.IP.PreviewCreator.Instance;

			if (creator.IsRunning)
				creator.Dispose();
		}
		#endregion

		#region LockUi()
		internal void LockUi()
		{
			this.lockUiGrid.Visibility = Visibility.Visible;
			this.lockUiStoryboard.Begin();

			this.progressGrid.Visibility = Visibility.Hidden;
		}
		
		internal void LockUiWithProgreess()
		{
			this.lockUiGrid.Visibility = Visibility.Visible;
			this.lockUiStoryboard.Begin();

			this.progressBar.Value = this.progressBar.Minimum;
			this.progressLabel.Text = "0%";
			this.progressGrid.Visibility = Visibility.Visible;
		}
		#endregion

		#region UnLockUi()
		public void UnLockUi()
		{
			this.progressBar.Value = this.progressBar.Minimum;
			this.lockUiStoryboard.Stop();
			this.lockUiGrid.Visibility = Visibility.Hidden;
		}
		#endregion

		#region ProgressChanged()
		public void ProgressChanged(double progress)
		{
			this.progressBar.Value = Math.Max(this.progressBar.Minimum, Math.Min(this.progressBar.Maximum, progress * 100.0));
			this.progressLabel.Text = string.Format("{0}%", Convert.ToInt32(progress * 100.0));
		}
		#endregion

		#region Clear()
		public void Clear()
		{
			if (iImage != null)
			{
				if (iImage is VpImage && ((VpImage)iImage).ImageType == VpImage.VpImageType.ScanImage)
					((VpImage)iImage).Updated -= new VpImage.VoidEventHnd(VpImage_Updated);


				this.iImage.Dispose();
			}

			this.iImage = null;
			GC.Collect();

			this.toolbar.ToolbarBookInfo.Visibility = System.Windows.Visibility.Collapsed;

			this.imageBox.Width = Double.NaN;// viewBox.ActualWidth;
			this.imageBox.Height = double.NaN;// viewBox.ActualHeight;

			AdjustScrollBars();
			RedrawImage();

			if (ImageChanged != null)
				ImageChanged(null);
		}
		#endregion

		#region ShowImage()
		public void ShowImage(VpImage vpImage)
		{
			try
			{
				if (vpImage != null && this.iImage != vpImage)
				{
					//Clear();
					if (this.iImage != null && this.iImage is VpImage)
						SaveLastVpImageViewParams();

					this.iImage = vpImage;
					if (vpImage.ImageType == VpImage.VpImageType.ScanImage)
					{
						vpImage.ItImageSettingsChanged += new VpImage.VoidEventHnd(ItImageSettings_Changed);
						vpImage.Updated += new VpImage.VoidEventHnd(VpImage_Updated);
						double newZoom = this.Zoom;

						if (ZoomType == ViewPane.ZoomType.FitImage)
							newZoom = Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height);
						else if (this.toolbar.ToolbarZoomSize.ZoomType == ViewPane.ZoomType.FitWidth)
							newZoom = pictBox.ActualWidth / this.iImage.FullImageSize.Width;

						double minimumZoom = 0.03;
						if (this.iImage != null && this.BitmapSize.Width > 0 && this.BitmapSize.Height > 0)
							minimumZoom = Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height);
						this.zoom = Math.Max(Math.Max(.03, minimumZoom), Math.Min(16, newZoom));

						//this.toolbar.ToolbarBookInfo.Visibility = System.Windows.Visibility.Visible;
						//this.toolbar.ToolbarBookInfo.BookPart = vpImage.BookPart;

						AdjustToolbar();
						AdjustScrollBars();
						LoadLastVpImageViewParams();
						RedrawImage();

						if (ImageChanged != null)
							ImageChanged(vpImage);
					}
					else if (vpImage.ImageType == VpImage.VpImageType.ItImage)
					{
						double newZoom = this.Zoom;
						
						if (ZoomType == ViewPane.ZoomType.FitImage)
							newZoom = Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height);
						else if (this.toolbar.ToolbarZoomSize.ZoomType == ViewPane.ZoomType.FitWidth)
							newZoom = pictBox.ActualWidth / this.iImage.FullImageSize.Width;

						double minimumZoom = 0.03;
						if (this.iImage != null && this.BitmapSize.Width > 0 && this.BitmapSize.Height > 0)
							minimumZoom = Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height);
						this.zoom = Math.Max(Math.Max(.03, minimumZoom), Math.Min(16, newZoom));

						//this.toolbar.ToolbarBookInfo.Visibility = System.Windows.Visibility.Collapsed;

						AdjustToolbar();
						AdjustScrollBars();
						LoadLastVpImageViewParams();
						RedrawImage();

						if (ImageChanged != null)
							ImageChanged(null);
					}
				}
				else if (vpImage == null)
					Clear();
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
				Clear();
				Cursor = null;
			}
		}

		public void ShowImage(System.Drawing.Bitmap bitmap)
		{
			try
			{
				if (this.iImage != null && this.iImage is VpImage)
					SaveLastVpImageViewParams();
				
				Clear();

				if (bitmap != null)
				{		
					this.iImage = new ViewBitmap(bitmap);

					ZoomType = ViewPane.ZoomType.FitImage;
					double newZoom = this.Zoom;

					if (ZoomType == ViewPane.ZoomType.FitImage)
						newZoom = Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height);
					else if (this.toolbar.ToolbarZoomSize.ZoomType == ViewPane.ZoomType.FitWidth)
						newZoom = pictBox.ActualWidth / bitmap.Width;

					double minimumZoom = 0.03;
					if (this.iImage != null && this.BitmapSize.Width > 0 && this.BitmapSize.Height > 0)
						minimumZoom = Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height);
					this.zoom = Math.Max(Math.Max(.03, minimumZoom), Math.Min(16, newZoom));

					//this.toolbar.ToolbarBookInfo.Visibility = System.Windows.Visibility.Collapsed;

					AdjustToolbar();
					AdjustScrollBars();
					RedrawImage();

					if (ImageChanged != null)
						ImageChanged(null);
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
				Clear();
			}
		}

		public void ShowImage(string filePath)
		{
			try
			{
				System.Drawing.Bitmap bitmap = ImageProcessing.ImageCopier.LoadFileIndependentImage(filePath);

				ShowImage(bitmap);
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
				Clear();
			}
		}
		#endregion

		#region ZoomIn()
		public void ZoomIn()
		{
			SetZoom(this.Zoom * 1.5F);
		}
		#endregion

		#region ZoomOut()
		public void ZoomOut()
		{
			SetZoom(this.Zoom / 1.5F);
		}
		#endregion

		#region GetZoomValue()
		public double GetZoomValue(ViewPane.ZoomType zoomType)
		{
			if (this.iImage == null)
				return 1.0F;

			switch (zoomType)
			{
				case ViewPane.ZoomType.ActualSize:
					return 1.0F;
				case ViewPane.ZoomType.FitImage:
					return Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height);
				case ViewPane.ZoomType.FitWidth:
					return pictBox.ActualWidth / this.BitmapSize.Width;
				default:
					return this.Zoom;
			}
		}
		#endregion

		#region PanelToImage()
		public RatioPoint PanelToImage(double x, double y)
		{		
			return new RatioPoint((imageRect.X + x) / Zoom, (imageRect.Y + y) / Zoom);
		}

		internal RatioPoint PanelToImage(MouseEventArgs e)
		{
			if (this.IImage != null)
			{
				System.Windows.Point visiblePoint = e.GetPosition(this.imageBox);
				double x, y;

				if (this.imageRect.X > 0)
					x = (this.imageRect.X + visiblePoint.X) / this.Zoom;
				else
					x = visiblePoint.X / this.Zoom;

				if (this.imageRect.Y >= 0)
					y = (this.imageRect.Y + visiblePoint.Y) / this.Zoom;
				else
					y = visiblePoint.Y / this.Zoom;

				RatioPoint p = new RatioPoint(x / this.BitmapSize.Width, y / this.BitmapSize.Height);

				p.X = (p.X < 0) ? 0 : ((p.X > 1) ? 1 : p.X);
				p.Y = (p.Y < 0) ? 0 : ((p.Y > 1) ? 1 : p.Y);

				return p;
			}
			else
				return new RatioPoint(0, 0);
		}		
		#endregion

		#region ImageToPanel()
		public Point ImageToPanel(RatioPoint imagePoint)
		{
			if (this.BitmapSize != null)
				return new Point(imagePoint.X * this.BitmapSize.Width, imagePoint.Y * this.BitmapSize.Height);
			else
				return new Point(0, 0);
		}
		#endregion

		#region AdjustToolbar()
		public void AdjustToolbar()
		{
			this.toolbar.ToolbarZoomSize.AdjustUi(this.toolbar.ToolbarZoomSize.ZoomType, this.Zoom);

			this.toolbar.ToolbarItSettings.Visibility = (this.AllowTransforms && this.IImage != null && this.IImage.IsFixed == false) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
			this.toolbar.ToolbarItTransforms.Visibility = (this.AllowTransforms && this.IImage != null && this.IImage.IsFixed == false) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
			this.toolbarPostProcessing.Set(this);
		}
		#endregion

		#region SetToolbarVisibilities()
		public void SetToolbarVisibilities(long visibilities)
		{
			this.toolbar.ToolbarItSettings.Visibility = ((visibilities & (long)ToolbarType.ItScanImage) > 0) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
			this.toolbar.ToolbarItTransforms.Visibility = ((visibilities & (long)ToolbarType.ItTransforms) > 0) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
			this.toolbar.ToolbarNavigation.Visibility = ((visibilities & (long)ToolbarType.Navigation) > 0) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
			this.toolbar.ToolbarPages.Visibility = ((visibilities & (long)ToolbarType.Pages) > 0) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
			this.toolbar.ToolbarScan.Visibility = ((visibilities & (long)ToolbarType.Scan) > 0) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
			this.toolbar.ToolbarTreatment.Visibility = ((visibilities & (long)ToolbarType.Treatment) > 0) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
		}
		#endregion

		#region KeyDown_Occured()
		public void KeyDown_Occured(KeyEventArgs e)
		{
			if (this.AllowTransforms && this.imageTreatmentUi.IsVisible)
				this.imageTreatmentUi.KeyDown_Occured(e);
		}
		#endregion

		#region KeyUp_Occured()
		public void KeyUp_Occured(KeyEventArgs e)
		{
			if (this.AllowTransforms && this.imageTreatmentUi.IsVisible)
				this.imageTreatmentUi.KeyUp_Occured(e);
		}
		#endregion

		#region ThumbnailCreatedDelegate()
		public void ThumbnailCreatedDelegate(System.Drawing.Bitmap bitmap, int renderingId, TimeSpan timeSpan)
		{
			this.Dispatcher.Invoke(this.dlgThumbnailCreated, bitmap, renderingId, timeSpan);
		}
		#endregion

		#region ThumbnailCanceledDelegate()
		public void ThumbnailCanceledDelegate()
		{
			this.Dispatcher.Invoke(this.dlgThumbnailCreationCanceled);
		}
		#endregion

		#region ThumbnailErrorDelegate()
		public void ThumbnailErrorDelegate(Exception ex)
		{
			this.Dispatcher.Invoke(this.dlgThumbnailCreationError, ex);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region SetZoom()
		private void SetZoom(double zoom)
		{
			double oldZoom = this.zoom;
			double minimumZoom = 0.03;

			if (this.iImage != null && this.BitmapSize.Width > 0 && this.BitmapSize.Height > 0)
				minimumZoom = Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height);

			this.zoom = Math.Max(Math.Max(.03, minimumZoom), Math.Min(16, zoom));

			if (this.zoom != oldZoom)
			{
				Point? center = null;

				if (hScroll.IsEnabled || vScroll.IsEnabled)
				{
					center = new Point((hScroll.Value + (pictBox.ActualWidth / 2)) / oldZoom, (vScroll.Value + (pictBox.ActualHeight / 2)) / oldZoom);
					center = new Point(center.Value.X * this.zoom, center.Value.Y * this.zoom);
				}

				AdjustScrollBars();

				if (hScroll.IsEnabled && center.HasValue)
					hScroll.Value = (int)Math.Max(0, Math.Min(hScroll.Maximum, center.Value.X - (pictBox.ActualWidth / 2)));
				if (vScroll.IsEnabled && center.HasValue)
					vScroll.Value = (int)Math.Max(0, Math.Min(vScroll.Maximum, center.Value.Y - (pictBox.ActualHeight / 2)));

				this.toolbar.ToolbarZoomSize.AdjustUi(this.toolbar.ZoomType, this.Zoom);
				RedrawImage();

				if (ZoomChanged != null)
					ZoomChanged(this.zoom);
			}
		}
		#endregion

		#region AdjustScrollBars()
		private void AdjustScrollBars()
		{
			if (this.iImage != null)
			{
				bool hEnabled = (((this.BitmapSize.Width * Zoom) - pictBox.ActualWidth) >= 1);
				bool vEnabled = (((this.BitmapSize.Height * Zoom) - pictBox.ActualHeight) >= 1);

				hScroll.IsEnabled = hEnabled;
				vScroll.IsEnabled = vEnabled;

				if (hEnabled)
				{
					double hRatio = (hScroll.Maximum > 0) ? ((double)hScroll.Value / hScroll.Maximum) : 0;

					hScroll.Value = 0;
					hScroll.Maximum = (this.BitmapSize.Width * Zoom) - pictBox.ActualWidth;
					hScroll.Value = hScroll.Maximum * hRatio;
					hScroll.LargeChange = (pictBox.ActualWidth);
					hScroll.ViewportSize = (pictBox.ActualWidth);
				}
				else
				{
					hScroll.Value = 0;
					hScroll.Maximum = 0;
					hScroll.LargeChange = hScroll.Maximum;
					hScroll.ViewportSize = hScroll.Maximum;
				}

				if (vEnabled)
				{
					double vRatio = (vScroll.Maximum > 0) ? ((double)vScroll.Value / vScroll.Maximum) : 0;

					vScroll.Value = 0;
					vScroll.Maximum = (this.BitmapSize.Height * Zoom) - pictBox.ActualHeight;
					vScroll.Value = vScroll.Maximum * vRatio;
					vScroll.LargeChange = (pictBox.ActualHeight);
					vScroll.ViewportSize = (pictBox.ActualHeight);
				}
				else
				{
					vScroll.Value = 0;
					vScroll.Maximum = 0;
					vScroll.LargeChange = vScroll.Maximum;
					vScroll.ViewportSize = vScroll.Maximum;
				}
			}
			else
			{
				hScroll.IsEnabled = false;
				hScroll.Value = 0;
				vScroll.IsEnabled = false;
				vScroll.Value = 0;
			}
		}
		#endregion

		#region RedrawImage()
		void RedrawImage()
		{
			try
			{
				this.renderingCounter++;
				CreateBitmap();
			}
			catch (Exception ex)
			{
				try
				{
					this.Dispatcher.BeginInvoke(this.dlgDrawingExceptionRaised, ex);
				}
				catch { }
			}
		}
		#endregion

		#region DrawTransforms()
		private void DrawTransforms()
		{
			if(this.IImage != null)
				imageTreatmentUi.Synchronize(this.IImage.ItImage);
			else
				imageTreatmentUi.Synchronize(null);
		}
		#endregion

		#region PictBox_Resize()
		private void PictBox_Resize(object sender, SizeChangedEventArgs e)
		{
			if (this.Visibility == Visibility.Visible && this.ActualWidth > 0 && this.ActualHeight > 0)
			{
				try
				{
					if (this.ZoomType != ViewPane.ZoomType.Value)
						this.SetZoom(GetZoomValue(this.ZoomType));

					AdjustScrollBars();
				}
				finally
				{
					RedrawImage();
				}
			}
		}
		#endregion

		#region Scroll()
		private void Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
		{
			RedrawImage();
		}
		#endregion

		#region ToolBar_ZoomChanged()
		private void ToolBar_ZoomTypeChanged(object sender, Toolbar.ToolbarZoomSize.ZoomSizeEventArgs e)
		{
			if (e.ZoomType == ViewPane.ZoomType.Value)
				this.SetZoom(e.ZoomValue);
			else
				this.SetZoom(GetZoomValue(e.ZoomType));
		}
		#endregion

		#region ZoomIn_Request()
		private void ZoomIn_Request(object sender, EventArgs e)
		{
			this.SetZoom(GetNearestZoom(this.Zoom, true));
		}
		#endregion

		#region ZoomOut_Request()
		private void ZoomOut_Request(object sender, EventArgs e)
		{
			this.SetZoom(GetNearestZoom(this.Zoom, false));
		}
		#endregion

		#region GetNearestZoom()
		private float GetNearestZoom(double aZoom, bool bigger)
		{
			if (bigger)
			{
				for (int i = 0; i < zoomSteps.Length - 1; i++)
					if (aZoom < zoomSteps[i])
						return zoomSteps[i];

				return zoomSteps[zoomSteps.Length - 1];
			}
			else
			{
				for (int i = zoomSteps.Length - 1; i > 0; i--)
					if (aZoom > zoomSteps[i])
						return zoomSteps[i];

				return zoomSteps[0];
			}
		}
		#endregion

		#region Image_MouseDown()
		private void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			SetCursor(true);

			if (this.ToolbarSelection == ViewPane.ToolbarSelection.ZoomDynamic)
				startPoint = e.GetPosition(pictBox);
			else
				startPoint = e.GetPosition(imageBox);

			if (this.ToolbarSelection == ViewPane.ToolbarSelection.ZoomIn || this.ToolbarSelection == ViewPane.ToolbarSelection.ZoomDynamic || this.ToolbarSelection == ViewPane.ToolbarSelection.ZoomOut)
				this.toolbar.ToolbarZoomSize.AdjustUi(ViewPane.ZoomType.Value, this.Zoom);
		}
		#endregion

		#region Image_MouseMove()
		private void Image_MouseMove(object sender, MouseEventArgs e)
		{
			if (startPoint != null && (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
			{
				Point p = e.GetPosition(imageBox);

				switch (this.ToolbarSelection)
				{
					case ViewPane.ToolbarSelection.Move:
						{
							if (this.BitmapSize != null)
							{
								double dx = p.X - startPoint.X;
								double dy = p.Y - startPoint.Y;

								hScroll.Value = Math.Max(0, Math.Min(hScroll.Maximum, hScroll.Value - dx));
								vScroll.Value = Math.Max(0, Math.Min(vScroll.Maximum, vScroll.Value - dy));
								startPoint = p;
								RedrawImage();
							}
						} break;
					case ViewPane.ToolbarSelection.ZoomIn:
						{
							if (Math.Abs(p.X - startPoint.X) > 5 || Math.Abs(p.Y - startPoint.Y) > 5)
							{
								Rect rect = new Rect(startPoint, p);
								rect.X = Math.Min(rect.X, rect.X + rect.Width);
								rect.Y = Math.Min(rect.Y, rect.Y + rect.Height);
								rect.Width = Math.Abs(rect.Width);
								rect.Height = Math.Abs(rect.Height);

								DrawDottedRectangle(rect);
							}
							else
								DrawDottedRectangle(Rect.Empty);
						} break;
					case ViewPane.ToolbarSelection.ZoomOut:
						{
							if (Math.Abs(p.X - startPoint.X) > 5 || Math.Abs(p.Y - startPoint.Y) > 5)
							{
								Rect rect = new Rect(startPoint, p);
								rect.X = Math.Min(rect.X, rect.X + rect.Width);
								rect.Y = Math.Min(rect.Y, rect.Y + rect.Height);
								rect.Width = Math.Abs(rect.Width);
								rect.Height = Math.Abs(rect.Height);

								DrawDottedRectangle(rect);
							}
							else
								DrawDottedRectangle(Rect.Empty);
						} break;
					case ViewPane.ToolbarSelection.ZoomDynamic:
						{
							p = e.GetPosition(pictBox);

							if (Math.Abs(p.X - startPoint.X) > 0 || Math.Abs(p.Y - startPoint.Y) > 0)
							{
								if ((p.Y - startPoint.Y) != 0)
								{
									SetZoom(this.Zoom - ((p.Y - startPoint.Y) / (50F / this.Zoom)));
								}

								startPoint = p;
							}
						} break;
					case ViewPane.ToolbarSelection.SelectRegion:
						{
							if (Math.Abs(p.X - startPoint.X) >= 0 || Math.Abs(p.Y - startPoint.Y) >= 0)
							{
								Rect rect = new Rect(startPoint, p);
								rect.X = Math.Min(rect.X, rect.X + rect.Width);
								rect.Y = Math.Min(rect.Y, rect.Y + rect.Height);
								rect.Width = Math.Abs(rect.Width);
								rect.Height = Math.Abs(rect.Height);

								DrawDottedRectangle(rect);
							}
							else
								DrawDottedRectangle(Rect.Empty);
						} break;
					case ViewPane.ToolbarSelection.FingerRemoval:
						{

						} break;
				}
			}
		}
		#endregion

		#region Image_MouseUp()
		private void Image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (this.iImage != null)
			{
				SetCursor(false);

				switch (this.ToolbarSelection)
				{
					case ViewPane.ToolbarSelection.ZoomIn:
						{
							if (zoomRectangle.IsEmpty)
							{
								Point p = e.GetPosition(imageBox);
								Point center = new Point((((hScroll.IsEnabled) ? hScroll.Value : 0) + p.X) / Zoom, (((vScroll.IsEnabled) ? vScroll.Value : 0) + p.Y) / Zoom);

								ZoomIn();
								MoveToImagePoint(center);
							}
							else
							{
								// imageRect is cumputed from UL corner of image panel, while zoomRectangle is computerd from UL corner of image. This is why Math.Max()
								Point center = new Point((zoomRectangle.X + (int)Math.Max(0, imageRect.X) + zoomRectangle.Width / 2) / Zoom, (zoomRectangle.Y + (int)Math.Max(0, imageRect.Y) + zoomRectangle.Height / 2) / Zoom);

								SetZoom(this.Zoom * Math.Min(pictBox.ActualWidth / zoomRectangle.Width, pictBox.ActualHeight / zoomRectangle.Height));
								MoveToImagePoint(center);
							}
						} break;
					case ViewPane.ToolbarSelection.ZoomOut:
						{
							if (zoomRectangle.IsEmpty)
							{
								MoveTo(e.GetPosition(imageBox));
								ZoomOut();
							}
							else
							{
								MoveTo(new Point(zoomRectangle.X + zoomRectangle.Width / 2, zoomRectangle.Y + zoomRectangle.Height / 2));
								ZoomOut();
							}
						} break;
					case ViewPane.ToolbarSelection.SelectRegion:
						{
							if (this.BitmapSize != Size.Empty && startPoint != null && RegionSelected != null)
							{
								if (!zoomRectangle.IsEmpty)
								{
									double x, y, width, height;

									x = Math.Min(zoomRectangle.X, zoomRectangle.Right) / this.BitmapSize.Width;
									y = Math.Min(zoomRectangle.Y, zoomRectangle.Bottom) / this.BitmapSize.Height;
									width = Math.Abs(zoomRectangle.X - zoomRectangle.Right) / this.BitmapSize.Width;
									height = Math.Abs(zoomRectangle.Y - zoomRectangle.Bottom) / this.BitmapSize.Height;

									if (x < 0.001)
									{
										width = (width + x);
										x = 0;
									}

									if (y < 0.001)
									{
										height = (height + y);
										y = 0;
									}

									if (width + x > 0.999)
										width = 1 - x;
									if (height + y > 0.999)
										height = 1 - y;

									RegionSelected(new RatioRect(x, y, width, height));
								}
								else
								{
									double x = startPoint.X / this.BitmapSize.Width;
									double y = startPoint.X / this.BitmapSize.Height;

									RegionSelected(new RatioRect(x, y, 0, 0));
								}
							}
						} break;
				}
			}

			zoomRectangle = Rect.Empty;
			this.mouseZoneRectangle.Visibility = Visibility.Hidden;
			this.startPoint = new Point();
		}
		#endregion

		#region Image_MouseLeave()
		private void Image_MouseLeave(object sender, MouseEventArgs e)
		{
			//this.mouseZoneRectangle.Visibility = Visibility.Hidden;
			//this.Cursor = null;
		}
		#endregion

		#region Image_MouseEnter()
		private void Image_MouseEnter(object sender, MouseEventArgs e)
		{
			SetCursor(false);
		}
		#endregion

		#region MoveTo()
		private void MoveTo(Point mouseP)
		{
			Point centerP = new Point(this.pictBox.ActualWidth / 2, this.pictBox.ActualHeight / 2);

			if (mouseP != centerP)
			{
				if (hScroll.IsEnabled)
					hScroll.Value = (int)Math.Max(0, Math.Min(hScroll.Maximum, hScroll.Value + mouseP.X - centerP.X));
				if (vScroll.IsEnabled)
					vScroll.Value = (int)Math.Max(0, Math.Min(vScroll.Maximum, vScroll.Value + mouseP.Y - centerP.Y));

				RedrawImage();
			}
		}
		#endregion

		#region MoveToImagePoint()
		private void MoveToImagePoint(Point imageP)
		{
			Point centerP = new Point(this.pictBox.ActualWidth / 2, this.pictBox.ActualHeight / 2);

			imageP.X *= Zoom;
			imageP.Y *= Zoom;

			if (imageP != centerP)
			{
				if (hScroll.IsEnabled)
					hScroll.Value = (int)Math.Max(0, Math.Min(hScroll.Maximum, imageP.X - centerP.X));
				if (vScroll.IsEnabled)
					vScroll.Value = (int)Math.Max(0, Math.Min(vScroll.Maximum, imageP.Y - centerP.Y));

				RedrawImage();
			}
		}
		#endregion

		#region SetCursor()
		private void SetCursor(bool mouseButtonPressed)
		{
			if (this.iImage == null)
			{
				this.pictBox.Cursor = null;
			}
			else
			{
				switch (this.ToolbarSelection)
				{
					case ViewPane.ToolbarSelection.Move:
						{
							if (mouseButtonPressed)
								this.pictBox.Cursor = cursorFist != null ? cursorFist : null;
							else
								this.pictBox.Cursor = cursorHand != null ? cursorHand : null;
						} break;
					case ViewPane.ToolbarSelection.ZoomIn:
						{
							this.pictBox.Cursor = cursorZoomIn != null ? cursorZoomIn : null;
						} break;
					case ViewPane.ToolbarSelection.ZoomOut:
						{
							this.pictBox.Cursor = cursorZoomOut != null ? cursorZoomOut : null;
						} break;
					case ViewPane.ToolbarSelection.ZoomDynamic:
						{
							this.pictBox.Cursor = cursorZoomDynamic != null ? cursorZoomDynamic : null;
						} break;
					case ViewPane.ToolbarSelection.SelectRegion:
						{
							this.pictBox.Cursor = Cursors.Cross;
						} break;
					case ViewPane.ToolbarSelection.Pages:
					case ViewPane.ToolbarSelection.Bookfold:
					case ViewPane.ToolbarSelection.FingerRemoval:
					case ViewPane.ToolbarSelection.Transforms:
					case ViewPane.ToolbarSelection.None:
						{
							this.pictBox.Cursor = null;
						} break;
				}
			}
		}
		#endregion

		#region DrawDottedRectangle()
		private void DrawDottedRectangle(Rect rect)
		{
			this.zoomRectangle = rect;
			
			if (rect.IsEmpty)
			{
				this.mouseZoneRectangle.Visibility = Visibility.Hidden;
			}
			else
			{
				//rect.Offset(this.imageBox.Margin.Left, this.imageBox.Margin.Top);
				Point p = viewBox.TransformToAncestor(this.pictBox).Transform(new Point(0, 0));
				rect.Offset(p.X, p.Y);
				this.mouseZoneRectangle.Margin = new Thickness(rect.Left, rect.Top, 0, 0);
				this.mouseZoneRectangle.Width = rect.Width;
				this.mouseZoneRectangle.Height = rect.Height;
				this.mouseZoneRectangle.Visibility = Visibility.Visible;
			}
		}
		#endregion

		#region ZoomMode_Changed()
		private void ZoomMode_Changed(ViewPane.ToolbarSelection oldMode, ViewPane.ToolbarSelection newMode)
		{
			this.ToolbarSelection = newMode;
			
			if (iImage != null)
			{
				if (oldMode == ViewPane.ToolbarSelection.Pages || newMode == ViewPane.ToolbarSelection.Pages ||
					oldMode == ViewPane.ToolbarSelection.FingerRemoval || newMode == ViewPane.ToolbarSelection.FingerRemoval ||
					oldMode == ViewPane.ToolbarSelection.Bookfold || newMode == ViewPane.ToolbarSelection.Bookfold)
				{
					RedrawImage();
				}
			}
		}
		#endregion

		#region ToolbarItSettings_ClipsSameSizeClick()
		void ToolbarItSettings_ClipsSameSizeClick(object sender, EventArgs e)
		{
			if (IImage != null && IImage.IsFixed == false && IImage.TwoPages)
			{
				InchSize size = new InchSize(Math.Max(IImage.ItImage.PageL.ClipRectInch.Width, IImage.ItImage.PageR.ClipRectInch.Width),
					Math.Max(IImage.ItImage.PageL.ClipRectInch.Height, IImage.ItImage.PageR.ClipRectInch.Height));

				IImage.ItImage.SetClipsSize(size);
			}
		}
		#endregion

		#region ToolbarItSettings_IndependentImageClick()
		void ToolbarItSettings_IndependentImageClick(bool isIndependent)
		{
			if (IImage != null && IImage.IsFixed == false)
			{
				IImage.ItImage.IsIndependent = isIndependent;

				if (isIndependent == false)
				{
					//this.ItImages. ResetItImage(IImage.ItImage);
					this.ItImages.MakeItImageDependant(IImage.ItImage);
					RedrawImage();
				}
			}
		}
		#endregion

		#region ImagePane_ItImageSettingsChanged()
		void ImagePane_ItImageSettingsChanged(object sender, EventArgs e)
		{
		}
		#endregion

		#region ItImageSettings_Changed()
		void ItImageSettings_Changed()
		{
			this.Dispatcher.BeginInvoke(this.dlgImageChanged);
		}
		#endregion

		#region ItImageSettingsChangedTU()
		void ItImageSettingsChangedTU()
		{
		}
		#endregion

		#region VpImage_Updated()
		void VpImage_Updated()
		{
			this.Dispatcher.BeginInvoke(this.dlgVpImageUpdated);
		}
		#endregion

		#region VpImageUpdatedTU()
		void VpImageUpdatedTU()
		{
			try
			{
				IViewImage iImage = this.IImage;

				if (iImage != null && iImage is VpImage)
				{
					this.Clear();
					this.ShowImage((VpImage)iImage);
				}
			}
			catch (Exception ex)
			{
				ShowErrorMessage(ex);
			}
		}
		#endregion

		#region ToolbarBookInfo_BookPartTypeChanged()
		void ToolbarBookInfo_BookPartTypeChanged(ViewPane.IT.BookPartType bookPart)
		{
			if (this.iImage != null && (this.iImage is VpImage))
			{
				VpImage vpImage = this.iImage as VpImage;

				vpImage.BookPart = bookPart;
				
				if (ImageChanged != null)
					ImageChanged(vpImage);
			}
		}
		#endregion

		#region Form_Loaded()
		private void Form_Loaded(object sender, RoutedEventArgs e)
		{
		}
		#endregion

		#region CreateBitmap()
		private void CreateBitmap()
		{
			if (this.IsVisible && this.ActualWidth > 0 && this.ActualHeight > 0)
			{
				if (this.renderingCounter > this.lastRenderedId)
				{
					if(this.iImage == null)
					{
						imageRect = new BIP.Geometry.RatioRect(0, 0, (pictBox.ActualWidth > 1) ? pictBox.ActualWidth : 1, (pictBox.ActualHeight > 1) ? pictBox.ActualHeight : 1);

						//imageBox.Stretch = Stretch.Fill;
						imageBox.Source = this.defaultImage;
						//imageBox.Margin = new Thickness(0);

						this.imageTreatmentUi.Visibility = Visibility.Hidden;

						DrawTransforms();
						this.Status = DrawingStatus.Idle;
					}
					else
					{
						if (this.imageRect.IsEmpty)
							imageRect = new BIP.Geometry.RatioRect(0, 0, 1, 1);

						imageRect.Width = (this.BitmapSize.Width * Zoom > 1) ? this.BitmapSize.Width * Zoom : 1 ;
						imageRect.Height = (this.BitmapSize.Height * Zoom > 1) ? this.BitmapSize.Height * Zoom : 1;

						if (hScroll.IsEnabled)
							imageRect.X = hScroll.Value;
						else
							imageRect.X = -((pictBox.ActualWidth - this.BitmapSize.Width * Zoom) / 2.0F);

						if (vScroll.IsEnabled)
							imageRect.Y = vScroll.Value;
						else
							imageRect.Y = -((pictBox.ActualHeight - this.BitmapSize.Height * Zoom) / 2.0F);

						Rect imagePortion = new Rect(imageRect.X / Zoom, imageRect.Y / Zoom,
							(int)Math.Ceiling(pictBox.ActualWidth / Zoom), (int)Math.Ceiling(pictBox.ActualHeight / Zoom));

						if (imagePortion.X < 0)
							imagePortion.X = 0;
						if (imagePortion.Y < 0)
							imagePortion.Y = 0;
						if (imagePortion.Right > this.BitmapSize.Width)
							imagePortion.Width = this.BitmapSize.Width - imagePortion.X;
						if (imagePortion.Bottom > this.BitmapSize.Height)
							imagePortion.Height = this.BitmapSize.Height - imagePortion.Y;

						this.Status = DrawingStatus.Computing;
						if (imagePortion.Left == 0 && imagePortion.Top == 0 && imagePortion.Right == this.BitmapSize.Width && imagePortion.Bottom == this.BitmapSize.Height)
							this.iImage.GetBitmapAsync(this, this.renderingCounter, false, (int)this.pictBox.ActualWidth, (int)this.pictBox.ActualHeight);
						else
						{
							BIP.Geometry.RatioRect ratioRect = new BIP.Geometry.RatioRect(imagePortion.Left / this.BitmapSize.Width, imagePortion.Top / this.BitmapSize.Height, imagePortion.Width / this.BitmapSize.Width, imagePortion.Height / this.BitmapSize.Height);
							this.iImage.GetBitmapAsync(this, this.renderingCounter, false, ratioRect, (int)this.pictBox.ActualWidth, (int)this.pictBox.ActualHeight);
						}
					}
				}
			}
			else
				this.renderingCounter++;
		}
		#endregion

		#region ThumbnailCreated()
		private void ThumbnailCreated(System.Drawing.Bitmap bitmap, int renderingId, TimeSpan timeSpan)
		{
			if (bitmap != null)
			{
				BitmapSource bitmapSource = ViewPane.Misc.Misc.GetBitmapSource(bitmap);

				bitmap.Dispose();
				this.imageBox.Source = bitmapSource;
				this.imageBox.Width = bitmapSource.Width;
				this.imageBox.Height = bitmapSource.Height;
				this.Status = DrawingStatus.Idle;
				this.lastRenderedId = renderingId;

				DrawTransforms();
			}
		}
		#endregion

		#region ThumbnailCreationError()
		void ThumbnailCreationError(Exception ex)
		{
			this.Status = DrawingStatus.Idle;
		}
		#endregion

		#region ThumbnailCreationCanceled()
		void ThumbnailCreationCanceled()
		{
		}
		#endregion

		#region Form_IsVisibleChanged()
		private void Form_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.IsVisible)
				CreateBitmap();
		}
		#endregion

		#region ShowErrorMessage()
		void ShowErrorMessage(Exception ex)
		{
			MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		#endregion

		#region class LastVpImageViewParams
		class LastVpImageViewParams
		{
			public ZoomType		ZoomType { get; set; }
			public double		Zoom { get; set; }
			public double		ScrollBarH { get; set; }
			public double		ScrollBarV { get; set; }

			public LastVpImageViewParams()
			{
				this.ZoomType = ZoomType.FitImage;
				this.Zoom = 1.0;
				this.ScrollBarH = 0;
				this.ScrollBarV = 0;
			}
		}
		#endregion

		#region SaveLastVpImageViewParams()
		void SaveLastVpImageViewParams()
		{
			this.lastVpImageViewParams.ZoomType = this.ZoomType;
			this.lastVpImageViewParams.Zoom = this.Zoom;
			this.lastVpImageViewParams.ScrollBarH = (hScroll.IsEnabled) ? hScroll.Value : 0;
			this.lastVpImageViewParams.ScrollBarV = (vScroll.IsEnabled) ? vScroll.Value : 0;
		}
		#endregion

		#region LoadLastVpImageViewParams()
		void LoadLastVpImageViewParams()
		{
			this.ZoomType = this.lastVpImageViewParams.ZoomType;
			
			if(this.ZoomType == ViewPane.ZoomType.Value)
				this.SetZoom(this.lastVpImageViewParams.Zoom);
			else
				this.SetZoom(GetZoomValue(this.ZoomType)); 

			if (hScroll.IsEnabled)
				hScroll.Value = Math.Max(hScroll.Minimum, Math.Min(hScroll.Maximum, this.lastVpImageViewParams.ScrollBarH));
			if (vScroll.IsEnabled)
				vScroll.Value = Math.Max(vScroll.Minimum, Math.Min(vScroll.Maximum, this.lastVpImageViewParams.ScrollBarV));
		}
		#endregion

		#endregion
	}
}
 