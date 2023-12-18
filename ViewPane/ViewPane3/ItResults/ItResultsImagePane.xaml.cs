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
using ViewPane.ImagePanel;
using ViewPane.Hierarchy;
using ViewPane.IP;



namespace ViewPane.ItResults
{
	/// <summary>
	/// Interaction logic for ItResultsImagePane.xaml
	/// </summary>
	public partial class ItResultsImagePane : UserControl, IImagePane, IitResultsCaller
	{
		#region variables
		ViewPane.ItResults.ItResultsPanel	viewPane = null;
		ViewPane.ItResults.Thumbnail		thumbnail = null;
		bool								leftPageVisible = true;
		DrawingStatus						drawingStatus = DrawingStatus.Idle;
	
		BitmapImage		defaultImage;
		object			locker = new object();
		bool			loaded = false;

		System.Timers.Timer		timerUiLocked = new System.Timers.Timer(100);
		EventHandler			dlgLockUiTick;
		
		public event ImagePane.ZoomModeChangedHnd	ZoomModeChanged;
		public event ImagePane.ZoomChangedHnd		ZoomChanged;

		public delegate void SelectClipHnd();

		double zoom = 1.0;
		float[] zoomSteps = { .0833F, .125F, .25F, .3333F, .50F, .6667F, .75F, 1F, 1.25F, 1.50F, 2F, 3F, 4F, 6F, 8F, 16F };
		Rect imageRect = Rect.Empty;

		RatioPoint? startPoint = null;
		RatioRect zoomRectangle = RatioRect.Empty;

		//events
		public event ImageSelectedHnd ImageChanged;

		public Cursor cursorHand = null;
		public Cursor cursorFist = null;
		public Cursor cursorZoomIn = null;
		public Cursor cursorZoomOut = null;
		public Cursor cursorZoomDynamic = null;

		delegate void ExceptionRaisedHnd(Exception ex);
		ExceptionRaisedHnd	dlgDrawingExceptionRaised;

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		volatile int renderingCounter = 0;
		volatile int lastRenderedId = -1;
		System.Windows.Media.Animation.Storyboard computingStoryboard;

#if DEBUG
		public double LastSkewBefore = 0;
		public double LastSkewAfter = 0;
#endif
		#endregion


		#region constructor
		public ItResultsImagePane()
		{
#if DEBUG
			try
			{
#endif
				InitializeComponent();

				this.computingStoryboard = this.Resources["lockStoryboardKey"] as System.Windows.Media.Animation.Storyboard;
				
				//toolbar
				this.toolbar.Init(this, true, true, false, false, false, false, false, false, false);

				this.toolbar.ToolbarZoomMode.ZoomModeChanged += new ViewPane.ImagePanel.ImagePane.ZoomModeChangedHnd(ZoomMode_Changed);
				
				this.toolbar.ToolbarZoomSize.ZoomTypeChanged += new ViewPane.Toolbar.ToolbarZoomSize.ZoomSizeEventHandler(ToolBar_ZoomTypeChanged);
				this.toolbar.ToolbarZoomSize.ZoomInRequest += new EventHandler(ZoomIn_Request);
				this.toolbar.ToolbarZoomSize.ZoomOutRequest += new EventHandler(ZoomOut_Request);

				this.toolbar.ZoomModeChanged += delegate(ToolbarSelection oldZoomMode, ToolbarSelection newZoomMode)
				{
					if (ZoomModeChanged != null)
						ZoomModeChanged(oldZoomMode, newZoomMode);
				};

				defaultImage = new BitmapImage(new Uri("../images/NoImage.jpg", UriKind.Relative));
				this.imageBox.Source = defaultImage;

				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
				string assemblyName = assembly.GetName().Name;
				cursorHand = new Cursor( assembly.GetManifestResourceStream("ViewPane.CursorHand.cur"));
				cursorFist = new Cursor(assembly.GetManifestResourceStream("ViewPane.CursorFist.cur"));
				cursorZoomIn = new Cursor(assembly.GetManifestResourceStream("ViewPane.CursorZoomIn.cur"));
				cursorZoomOut = new Cursor(assembly.GetManifestResourceStream("ViewPane.CursorZoomOut.cur"));
				cursorZoomDynamic = new Cursor(assembly.GetManifestResourceStream("ViewPane.CursorZoomDynamic.cur"));

				this.dlgDrawingExceptionRaised += delegate(Exception ex)
				{
					MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				};

				timerUiLocked.AutoReset = true;
				timerUiLocked.Elapsed += new System.Timers.ElapsedEventHandler(LockUiTimer_Elapsed);
				dlgLockUiTick = new EventHandler(LockUiTick);
#if DEBUG
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
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

		#region DrawingStatus
		enum DrawingStatus
		{
			Idle,
			Computing
		}
		#endregion
	

		//PUBLIC PROPERTIES
		#region public properties

		//public Cursor		PictBoxCursor { get { return this.pictBox.Cursor; } set { this.pictBox.Cursor = value; } }

		#region VisiblePage
		public Toolbar.VisiblePage VisiblePage
		{
			get { return this.toolbar.toolbarPages.Pages; }
			set { this.toolbar.toolbarPages.Pages = value; }
		}
		#endregion

		#endregion


		//INTERNAL PROPERTIES
		#region internal properties
		
		internal double										Zoom { get { return this.zoom; } }
		internal VpImage									SelectedImage { get { return (this.thumbnail != null) ? this.thumbnail.VpImage : null ; } }
		internal Rect										ImageRect { get { return imageRect; } }
		internal ViewPane.ItResults.ItResultsPanel			ViewPanel { get { return this.viewPane; } set { this.viewPane = value; } }
		internal ViewPane.ItResults.Thumbnail				Thumbnail { get { return this.thumbnail; } }
		
		#region ToolbarSelection
		internal ViewPane.ToolbarSelection ToolbarSelection
		{
			get { return this.toolbar.ToolbarSelection; }
			set { this.toolbar.ToolbarSelection = value; }
		}
		#endregion

		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		#region BitmapSize
		/// <summary>
		/// returns full image size in pixels
		/// </summary>
		private System.Drawing.Size BitmapSize 
		{ 
			get 
			{
				if (this.thumbnail != null)
				{ 
					if(this.leftPageVisible)
						return this.thumbnail.FullImageSizeL;
					else
						return this.thumbnail.FullImageSizeR;
				}
				else
					return System.Drawing.Size.Empty; 
			} 
		}
		#endregion

		#region ZoomType
		ViewPane.ZoomType ZoomType
		{
			get { return this.toolbar.ZoomType; }
			set { this.toolbar.ZoomType = value; }
		}
		#endregion

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
						this.lockUiGrid.Visibility = Visibility.Hidden;
						this.computingStoryboard.Stop();
					}
					else
					{
						this.lockUiGrid.Visibility = Visibility.Visible;
						this.computingStoryboard.Begin();
					}
				}
			}
		}
		#endregion
	
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region ThumbnailCreatedDelegate()
		public void ThumbnailCreatedDelegate(System.Drawing.Bitmap bitmap, int renderingId)
		{
			this.Dispatcher.BeginInvoke((Action)delegate() { ThumbnailCreated(bitmap, renderingId); });
		}
		#endregion

		#region ThumbnailCanceledDelegate()
		public void ThumbnailCanceledDelegate()
		{
			this.Dispatcher.Invoke((Action)delegate() { ThumbnailCreationCanceled(); });
		}
		#endregion

		#region ThumbnailErrorDelegate()
		public void ThumbnailErrorDelegate(Exception ex)
		{
			this.Dispatcher.Invoke((Action)delegate() { ThumbnailCreationError(ex); });
		}
		#endregion

		#region LockUi()
		internal void LockUi()
		{
			this.lockUiGrid.Visibility = Visibility.Visible;
			this.timerUiLocked.Start();

			this.progressGrid.Visibility = Visibility.Hidden;
		}
		
		internal void LockUiWithProgreess()
		{
			this.lockUiGrid.Visibility = Visibility.Visible;
			this.timerUiLocked.Start();

			this.progressBar.Value = this.progressBar.Minimum;
			this.progressLabel.Text = "0%";
			this.progressGrid.Visibility = Visibility.Visible;
		}
		#endregion

		#region UnLockUi()
		public void UnLockUi()
		{
			this.progressBar.Value = this.progressBar.Minimum;
			this.timerUiLocked.Stop();
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
			if (this.thumbnail != null)
			{
				if (this.thumbnail.ItImage != null)
				{
					//if (showLeftPage)
						this.thumbnail.ItImage.PageL.Changed -= new ImageProcessing.IpSettings.ItPage.PageChangedHnd(Page_Changed);
					//else if (showLeftPage == false && this.thumbnail.ItImage.TwoPages)
						this.thumbnail.ItImage.PageR.Changed -= new ImageProcessing.IpSettings.ItPage.PageChangedHnd(Page_Changed);
				}
			}
			
			this.thumbnail = null;
			this.leftPageVisible = true;
			GC.Collect();

			AdjustScrollBars();
			CreateThumbnailImage(true);

			if (ImageChanged != null)
				ImageChanged(null);
		}
		#endregion

		#region ShowImage()
		public void ShowImage(ViewPane.ItResults.Thumbnail thumbnail, bool showLeftPage)
		{
			try
			{
				if(thumbnail == null)
					Clear();
				else if (this.thumbnail != thumbnail || (leftPageVisible != showLeftPage))
				{
					Clear();

					this.thumbnail = thumbnail;

					if (this.thumbnail.ItImage != null)
					{
						if(showLeftPage)
							this.thumbnail.ItImage.PageL.Changed += new ImageProcessing.IpSettings.ItPage.PageChangedHnd(Page_Changed);
						else if (showLeftPage == false && this.thumbnail.ItImage.TwoPages)
							this.thumbnail.ItImage.PageR.Changed += new ImageProcessing.IpSettings.ItPage.PageChangedHnd(Page_Changed);
					}
					
					//this.thumbnail.ItUpdating += new Thumbnail.VoidEventHnd(Thumbnail_ItUpdating);
					//this.thumbnail.ItUpdated += new Thumbnail.VoidEventHnd(Thumbnail_ItUpdated);
					
					this.leftPageVisible = showLeftPage;

					if (ZoomType == ZoomType.FitImage)
						this.zoom = Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height);
					else if (this.toolbar.ToolbarZoomSize.ZoomType == ViewPane.ZoomType.FitWidth)
						this.zoom = pictBox.ActualWidth / this.BitmapSize.Width;

					this.VisiblePage = (thumbnail.ItImage != null && thumbnail.ItImage.TwoPages) ? (this.leftPageVisible ? Toolbar.VisiblePage.Left : Toolbar.VisiblePage.Right) : ViewPane.Toolbar.VisiblePage.None;
		
					AdjustToolbar();
					AdjustScrollBars();
					CreateThumbnailImage(true);

					if (ImageChanged != null)
						ImageChanged(thumbnail.VpImage);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				Clear();
				Cursor = null;
			}
		}
		#endregion

		#region SetZoom()
		public void SetZoom(double zoom)
		{
			double oldZoom = this.zoom;
			this.zoom = Math.Max(.01F, Math.Min(16F, zoom));

			if (this.zoom != oldZoom)
			{
				Size oldVisible = new Size(pictBox.ActualWidth / oldZoom, pictBox.ActualHeight / oldZoom);
				Size newVisible = new Size(pictBox.ActualWidth / this.zoom, pictBox.ActualHeight / this.zoom);

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

				this.toolbar.ToolbarZoomSize.AdjustUi(this.toolbar.ToolbarZoomSize.ZoomType, this.Zoom);
				CreateThumbnailImage(true);

				if (ZoomChanged != null)
					ZoomChanged(this.zoom);
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
			if (this.thumbnail == null)
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
		internal RatioPoint PanelToImage(MouseEventArgs e)
		{
			if (this.thumbnail != null)
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

		#region Reset()
		public void Reset()
		{
			Clear();
			this.toolbar.ZoomType = ViewPane.ZoomType.FitImage;
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region LockUiTimer_Elapsed()
		void LockUiTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			this.Dispatcher.BeginInvoke(this.dlgLockUiTick, new object[] { this, null });
		}
		#endregion

		#region LockUiTick()
		void LockUiTick(object sender, EventArgs args)
		{
			this.r1.Opacity = (this.r1.Opacity < 1.0) ? this.r1.Opacity + 0.1 : 0.3;
			this.r2.Opacity = (this.r2.Opacity < 1.0) ? this.r2.Opacity + 0.1 : 0.3;
			this.r3.Opacity = (this.r3.Opacity < 1.0) ? this.r3.Opacity + 0.1 : 0.3;
			this.r4.Opacity = (this.r4.Opacity < 1.0) ? this.r4.Opacity + 0.1 : 0.3;
			this.r5.Opacity = (this.r5.Opacity < 1.0) ? this.r5.Opacity + 0.1 : 0.3;
			this.r6.Opacity = (this.r6.Opacity < 1.0) ? this.r6.Opacity + 0.1 : 0.3;
			this.r7.Opacity = (this.r7.Opacity < 1.0) ? this.r7.Opacity + 0.1 : 0.3;
			this.r8.Opacity = (this.r8.Opacity < 1.0) ? this.r8.Opacity + 0.1 : 0.3;
		}
		#endregion

		#region AdjustScrollBars()
		private void AdjustScrollBars()
		{
			if (this.thumbnail != null)
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

		#region DrawImage()
		private void DrawImage()
		{
			lock (locker)
			{
				Cursor currentCursor = Cursor;

				try
				{
					if (currentCursor == null || currentCursor == Cursors.None)
						Mouse.OverrideCursor = Cursors.Wait;

					if (this.thumbnail == null)
					{
						this.imageRect = new Rect(0, 0, (pictBox.ActualWidth > 1) ? pictBox.ActualWidth : 1, (pictBox.ActualHeight > 1) ? pictBox.ActualHeight : 1);

						imageBox.Stretch = Stretch.Fill;
						imageBox.Source = this.defaultImage;
						imageBox.Margin = new Thickness(0);
					}
					else
					{
						if (pictBox.ActualWidth > 0 && pictBox.ActualHeight > 0)
						{
							if (this.renderingCounter > this.lastRenderedId)
							{
								if (this.imageRect.IsEmpty)
									this.imageRect = new Rect(0, 0, 1, 1);
								if (this.Zoom <= 0)
									this.zoom = 0.1;

								this.imageRect.Width = this.BitmapSize.Width * this.Zoom;
								this.imageRect.Height = this.BitmapSize.Height * this.Zoom;
								if (this.imageRect.Width < 1)
									this.imageRect.Width = 1;
								if (this.imageRect.Height < 1)
									this.imageRect.Height = 1;

								if (hScroll.IsEnabled)
									this.imageRect.X = hScroll.Value;
								else
									this.imageRect.X = -((pictBox.ActualWidth - this.BitmapSize.Width * this.Zoom) / 2.0F);

								if (vScroll.IsEnabled)
									this.imageRect.Y = vScroll.Value;
								else
									this.imageRect.Y = -((pictBox.ActualHeight - this.BitmapSize.Height * this.Zoom) / 2.0F);

								Rect imagePortion = new Rect(this.imageRect.X / this.Zoom, this.imageRect.Y / this.Zoom,
									(int)Math.Ceiling(pictBox.ActualWidth / this.Zoom), (int)Math.Ceiling(pictBox.ActualHeight / this.Zoom));

								if (imagePortion.X < 0)
									imagePortion.X = 0;
								if (imagePortion.Y < 0)
									imagePortion.Y = 0;
								if (imagePortion.Right > this.BitmapSize.Width)
									imagePortion.Width = this.BitmapSize.Width - imagePortion.X;
								if (imagePortion.Bottom > this.BitmapSize.Height)
									imagePortion.Height = this.BitmapSize.Height - imagePortion.Y;

#if DEBUG
								//Console.WriteLine("Skewww: " + this.Thumbnail.ItImage.PageR.Skew + ", renderingCounter: " + renderingCounter + ", lastRenderedId: " + lastRenderedId);
#endif
								this.Thumbnail.GetClipAsync(this, this.renderingCounter, this.leftPageVisible, false, imagePortion, this.zoom, ImageProcessing.Resizing.ResizeMode.Quality);
								this.Status = DrawingStatus.Computing;

								/*BitmapSource clip = this.thumbnail.GetClip(this.leftPageVisible, imagePortion, this.Zoom);

								double xMargin = clip.PixelWidth < pictBox.ActualWidth ? (pictBox.ActualWidth - clip.PixelWidth) / 2 : 0;
								double yMargin = clip.PixelHeight < pictBox.ActualHeight ? (pictBox.ActualHeight - clip.PixelHeight) / 2 : 0;

								imageBox.Source = clip;
								imageBox.Margin = new Thickness(xMargin, yMargin, xMargin, yMargin);

								clip = null;
								GC.Collect();*/
							}
						}
						else
							this.renderingCounter++;
					}
				}
				catch (Exception ex)
				{
					this.Dispatcher.BeginInvoke(this.dlgDrawingExceptionRaised, ex);

					Clear();
				}
				finally
				{
					if (currentCursor != Cursors.Wait)
						Mouse.OverrideCursor = currentCursor;
					else
						Mouse.OverrideCursor = null;
				}
			}
		}
		#endregion

		#region OnRender()
		/*protected override void OnRender(DrawingContext drawingContext)
		{
			try
			{
				base.OnRender(drawingContext);

				pictBox_Paint(null, null);
				
				if (this.Thumbnail != null && !zoomRectangle.IsEmpty)
					DrawDottedRectangle(zoomRectangle);
			}
			catch (Exception ex)
			{
				try
				{
					this.Dispatcher.BeginInvoke(this.dlgDrawingExceptionRaised, ex);
				}
				catch
				{
					//MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}*/
		#endregion

		#region PictBox_Resize()
		private void PictBox_Resize(object sender, SizeChangedEventArgs e)
		{
			try
			{
				if (loaded)
				{
					try
					{
						ViewPane.ZoomType type = this.toolbar.ToolbarZoomSize.ZoomType;

						if (type != ViewPane.ZoomType.Value)
						{
							this.SetZoom(GetZoomValue(type));
						}

						AdjustScrollBars();
					}
					finally
					{
						CreateThumbnailImage(true);
					}
				}
			}
#if DEBUG
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
#else
			catch
			{
			}
#endif
		}
		#endregion

		#region Scroll()
		private void Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
		{
			CreateThumbnailImage(true);
		}
		#endregion

		#region ToolBar_ZoomChanged()
		private void ToolBar_ZoomTypeChanged(object sender, Toolbar.ToolbarZoomSize.ZoomSizeEventArgs e)
		{
			double newZoom;

			if (e.ZoomType == ViewPane.ZoomType.Value)
				newZoom = e.ZoomValue;
			else
				newZoom = GetZoomValue(e.ZoomType);

			this.SetZoom(newZoom);
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
		private float GetNearestZoom(double zoom, bool bigger)
		{
			if (bigger)
			{
				for (int i = 0; i < zoomSteps.Length - 1; i++)
					if (zoom < zoomSteps[i])
						return zoomSteps[i];

				return zoomSteps[zoomSteps.Length - 1];
			}
			else
			{
				for (int i = zoomSteps.Length - 1; i > 0; i--)
					if (zoom > zoomSteps[i])
						return zoomSteps[i];

				return zoomSteps[0];
			}
		}
		#endregion

		#region Image_MouseDown()
		private void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			SetCursor(true);
			startPoint = PanelToImage(e);

			if (ToolbarSelection == ViewPane.ToolbarSelection.ZoomIn || ToolbarSelection == ViewPane.ToolbarSelection.ZoomDynamic || ToolbarSelection == ViewPane.ToolbarSelection.ZoomOut)
				this.toolbar.ToolbarZoomSize.AdjustUi(ViewPane.ZoomType.Value, this.Zoom);
		}
		#endregion

		#region Image_MouseMove()
		private void Image_MouseMove(object sender, MouseEventArgs e)
		{
			if (startPoint != null && (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
			{
				RatioPoint p = PanelToImage(e);

				switch (ToolbarSelection)
				{
					case ViewPane.ToolbarSelection.Move:
						{
							if (this.BitmapSize != null)
							{
								double dx = (p.X - startPoint.Value.X) * this.BitmapSize.Width;
								double dy = (p.Y - startPoint.Value.Y) * this.BitmapSize.Width;

								hScroll.Value = Math.Max(0, Math.Min(hScroll.Maximum, hScroll.Value - dx));
								vScroll.Value = Math.Max(0, Math.Min(vScroll.Maximum, vScroll.Value - dy));
								startPoint = p;
								CreateThumbnailImage(true);
							}
						} break;
					case ViewPane.ToolbarSelection.ZoomIn:
						{
							RatioRect zoomRectangleTemp = zoomRectangle;

							zoomRectangle = RatioRect.FromLTRB(startPoint.Value.X, startPoint.Value.Y, p.X, p.Y);
							zoomRectangle.X = Math.Min(zoomRectangle.X, zoomRectangle.X + zoomRectangle.Width);
							zoomRectangle.Y = Math.Min(zoomRectangle.Y, zoomRectangle.Y + zoomRectangle.Height);
							zoomRectangle.Width = Math.Abs(zoomRectangle.Width);
							zoomRectangle.Height = Math.Abs(zoomRectangle.Height);

							CreateThumbnailImage(true);
						} break;
					case ViewPane.ToolbarSelection.ZoomOut:
						{
							if ((p.X - startPoint.Value.X != 0) || (p.Y - startPoint.Value.Y != 0))
							{
								RatioRect zoomRectangleTemp = zoomRectangle;

								zoomRectangle = RatioRect.FromLTRB(startPoint.Value.X, startPoint.Value.Y, p.X, p.Y);
								zoomRectangle.X = Math.Min(zoomRectangle.X, zoomRectangle.X + zoomRectangle.Width);
								zoomRectangle.Y = Math.Min(zoomRectangle.Y, zoomRectangle.Y + zoomRectangle.Height);
								zoomRectangle.Width = Math.Abs(zoomRectangle.Width);
								zoomRectangle.Height = Math.Abs(zoomRectangle.Height);

								CreateThumbnailImage(true);
							}
							else
								zoomRectangle = RatioRect.Empty;
						} break;
					case ViewPane.ToolbarSelection.ZoomDynamic:
						{
							if ((p.X - startPoint.Value.X != 0) || (p.Y - startPoint.Value.Y != 0))
							{
								if ((p.Y - startPoint.Value.Y) != 0)
									SetZoom(this.Zoom - ((p.Y - startPoint.Value.Y) / (50F / this.Zoom)));

								startPoint = p;
							}
						} break;
					case ViewPane.ToolbarSelection.SelectRegion:
						{
							if ((p.X - startPoint.Value.X != 0) || (p.Y - startPoint.Value.Y != 0))
							{
								RatioRect zoomRectangleTemp = zoomRectangle;

								zoomRectangle = RatioRect.FromLTRB(startPoint.Value.X, startPoint.Value.Y, p.X, p.Y);
								zoomRectangle.X = Math.Min(zoomRectangle.X, zoomRectangle.X + zoomRectangle.Width);
								zoomRectangle.Y = Math.Min(zoomRectangle.Y, zoomRectangle.Y + zoomRectangle.Height);
								zoomRectangle.Width = Math.Abs(zoomRectangle.Width);
								zoomRectangle.Height = Math.Abs(zoomRectangle.Height);

								DrawDottedRectangle(zoomRectangle);
							}
							else
								zoomRectangle = RatioRect.Empty;
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
			if (this.thumbnail != null)
			{
				SetCursor(false);

				switch (ToolbarSelection)
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
						} break;
				}
			}

			zoomRectangle = RatioRect.Empty;
			this.mouseZoneRectangle.Visibility = Visibility.Hidden;
			this.startPoint = null;
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

				CreateThumbnailImage(true);
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

				CreateThumbnailImage(true);
			}
		}
		#endregion

		#region SetCursor()
		private void SetCursor(bool mouseButtonPressed)
		{
			if (this.thumbnail == null)
			{
				this.pictBox.Cursor = null;
			}
			else
			{
				switch (ToolbarSelection)
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
				}
			}
		}
		#endregion

		#region DrawDottedRectangle()
		private void DrawDottedRectangle(RatioRect rect)
		{
			if (this.BitmapSize != null)
			{
				Rect r = new Rect(rect.X * this.BitmapSize.Width, rect.Y * this.BitmapSize.Height, rect.Width * this.BitmapSize.Width, rect.Height * this.BitmapSize.Height);
				r.Offset(this.imageBox.Margin.Left, this.imageBox.Margin.Top);
				this.mouseZoneRectangle.Margin = new Thickness(r.Left, r.Top, 0, 0);
				this.mouseZoneRectangle.Width = r.Width;
				this.mouseZoneRectangle.Height = r.Height;
				this.mouseZoneRectangle.Visibility = Visibility.Visible;
			}
		}
		#endregion

		#region ZoomMode_Changed()
		private void ZoomMode_Changed(ViewPane.ToolbarSelection oldMode, ViewPane.ToolbarSelection newMode)
		{
			this.ToolbarSelection = newMode;

			if (this.thumbnail != null)
			{
				if (oldMode == ViewPane.ToolbarSelection.Pages || newMode == ViewPane.ToolbarSelection.Pages ||
					oldMode == ViewPane.ToolbarSelection.FingerRemoval || newMode == ViewPane.ToolbarSelection.FingerRemoval ||
					oldMode == ViewPane.ToolbarSelection.Bookfold || newMode == ViewPane.ToolbarSelection.Bookfold)
				{
					CreateThumbnailImage(true);
				}
			}
		}
		#endregion

		#region Form_Loaded()
		private void Form_Loaded(object sender, RoutedEventArgs e)
		{
			loaded = true;
			CreateThumbnailImage(false);
		}
		#endregion

		#region AdjustToolbar()
		public void AdjustToolbar()
		{
			this.toolbar.ToolbarZoomSize.AdjustUi(this.toolbar.ToolbarZoomSize.ZoomType, this.Zoom);
		}
		#endregion

		#region Thumbnail_ItUpdating()
		/*void Thumbnail_ItUpdating()
		{
		}*/
		#endregion

		#region Thumbnail_ItUpdated()
		/*void Thumbnail_ItUpdated()
		{
			if (this.thumbnail != null)
				ShowImage(this.thumbnail, true, this.leftPageVisible);
		}*/
		#endregion

		#region FixZoomIfNecessary()
		void FixZoomIfNecessary()
		{
			if (this.thumbnail != null && this.BitmapSize.IsEmpty == false && pictBox.ActualWidth > 0 && pictBox.ActualHeight > 0)
			{
				if (ZoomType == ViewPane.ZoomType.FitImage)
					SetZoom(Math.Min(pictBox.ActualWidth / this.BitmapSize.Width, pictBox.ActualHeight / this.BitmapSize.Height));
				else if (this.toolbar.ToolbarZoomSize.ZoomType == ViewPane.ZoomType.FitWidth)
					SetZoom(pictBox.ActualWidth / this.BitmapSize.Width);
			}
		}
		#endregion

		#region CreateThumbnailImage()
		private void CreateThumbnailImage(bool invalidate)
		{
			if (invalidate)
			{
				this.renderingCounter++;

#if DEBUG
				//Console.WriteLine("Skew: " + itPage.Skew + ", renderingCounter: " + renderingCounter + ", lastRenderedId: " + lastRenderedId);
				//Console.WriteLine("renderingCounter: " + renderingCounter + ", lastRenderedId: " + lastRenderedId);
				if (this.Thumbnail != null && this.Thumbnail.ItImage != null)
				{
					if (this.leftPageVisible)
						this.LastSkewBefore = this.Thumbnail.ItImage.PageL.Skew;
					else
						this.LastSkewBefore = this.Thumbnail.ItImage.PageR.Skew;
				}
#endif

			}

			if (this.Dispatcher.CheckAccess())
			{
				DrawImage();
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate() { DrawImage(); });
			}
		}
		#endregion

		#region ThumbnailCreated()
		private void ThumbnailCreated(System.Drawing.Bitmap bitmap, int renderingId)
		{
			if (bitmap != null)
			{
				bool releaseMemory = (this.imageBox.Source != null);

				BitmapSource bitmapSource = Misc.Misc.GetBitmapSource(bitmap);

				double xMargin = bitmapSource.PixelWidth < pictBox.ActualWidth ? (pictBox.ActualWidth - bitmapSource.PixelWidth) / 2 : 0;
				double yMargin = bitmapSource.PixelHeight < pictBox.ActualHeight ? (pictBox.ActualHeight - bitmapSource.PixelHeight) / 2 : 0;

				imageBox.Source = bitmapSource;
				imageBox.Margin = new Thickness(xMargin, yMargin, xMargin, yMargin);

				bitmapSource = null;
				GC.Collect();

				if (!zoomRectangle.IsEmpty)
					DrawDottedRectangle(zoomRectangle);

				if (releaseMemory)
					ViewPane.Misc.MemoryManagement.ReleaseUnusedMemory();

				if (this.ZoomType == ViewPane.ZoomType.FitImage || this.ZoomType == ViewPane.ZoomType.FitWidth)
				{
					double newZoom = GetZoomValue(this.ZoomType);

					if ((newZoom < this.Zoom * .999) || (newZoom > this.Zoom * 1.001))
						this.SetZoom(newZoom);
				}

				this.lastRenderedId = renderingId;

#if DEBUG
				//Console.WriteLine("renderingCounter: " + renderingCounter + ", lastRenderedId: " + lastRenderedId);
				if (this.Thumbnail != null && this.Thumbnail.ItImage != null)
				{
					if (this.leftPageVisible)
						this.LastSkewAfter = this.Thumbnail.ItImage.PageL.Skew;
					else
						this.LastSkewAfter = this.Thumbnail.ItImage.PageR.Skew;
				}
#endif

				if (this.renderingCounter > renderingId)
					DrawImage();
				else
				{
					this.Status = DrawingStatus.Idle;
				}
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
			this.Status = DrawingStatus.Idle;
		}
		#endregion

		#region Page_Changed()
		void Page_Changed(ImageProcessing.IpSettings.ItPage itPage, ImageProcessing.IpSettings.ItProperty type)
		{
			if (type == ImageProcessing.IpSettings.ItProperty.Clip)
			{
				if (this.Dispatcher.CheckAccess())
					Page_ChangedTU(itPage, type);
				else
					this.Dispatcher.Invoke((Action)delegate() { Page_ChangedTU(itPage, type); });
			}
		}
		#endregion

		#region Page_ChangedTU()
		void Page_ChangedTU(ImageProcessing.IpSettings.ItPage itPage, ImageProcessing.IpSettings.ItProperty type)
		{
			CreateThumbnailImage(true);
		}
		#endregion

		#endregion

	}
}
