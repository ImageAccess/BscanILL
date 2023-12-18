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
using System.IO;

namespace ViewPane.Thumbnails
{
	/// <summary>
	/// Interaction logic for Thumbnail.xaml
	/// </summary>
	public partial class Thumbnail : UserControl, ViewPane.IP.IPreviewCaller
	{
		#region variables
		StripPane					stripPane;
		ViewPane.Hierarchy.VpImage	vpImage;
		DrawingStatus				drawingStatus = DrawingStatus.Idle;

		//ToolTip toolTip;

		public static DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapSource), typeof(Thumbnail), new PropertyMetadata(null));
		public static DependencyProperty ItGeometryProperty = DependencyProperty.Register("ItGeometry", typeof(Geometry), typeof(Thumbnail), new PropertyMetadata(null));

		public static DependencyProperty BackgroundBrushProperty = DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(Thumbnail));
		static DependencyProperty independenceBrushProperty = DependencyProperty.Register("IndependenceBrush", typeof(Brush), typeof(Thumbnail), new PropertyMetadata(new SolidColorBrush(Colors.Black)));
		static DependencyProperty isFixedProperty = DependencyProperty.Register("IsFixed", typeof(bool), typeof(Thumbnail), new PropertyMetadata(true, new PropertyChangedCallback(OnIsFixedChanged)));
		static DependencyProperty isIndependentProperty = DependencyProperty.Register("IsIndependent", typeof(bool), typeof(Thumbnail), new PropertyMetadata(true, new PropertyChangedCallback(OnIsIndependentChanged)));

        public static DependencyProperty InnerRectBrushProperty = DependencyProperty.Register("InnerRectBrush", typeof(Brush), typeof(Thumbnail), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));        

		public static DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(Thumbnail), new PropertyMetadata(""));

		public static Size ThumbnailSize = new Size(135, 120);

		delegate void VoidEventHnd();
		VoidEventHnd dlgItImageChanged;
		VoidEventHnd dlgItImageSettingsChanged;
		VoidEventHnd dlgUpdated;
		VoidEventHnd dlgSelectRequested;
		VoidEventHnd dlgBringIntoViewRequest;
		
		public delegate void ThumbanilEventHandler(Thumbnail thumbnail);
		public event ThumbanilEventHandler Selected;
		public event ThumbanilEventHandler Unselected;

		static DependencyProperty isSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(Thumbnail), new PropertyMetadata(false, new PropertyChangedCallback(OnIsSelectedChanged)));

		System.Windows.Media.Animation.Storyboard computingStoryboard;
		static DependencyProperty visibleInScrollViewerProperty = DependencyProperty.Register("IsVisibleInScrollViewer", typeof(bool), typeof(Thumbnail), new PropertyMetadata(false, new PropertyChangedCallback(IsVisibleInScrollViewer_Changed)));
		int			renderingCounter = 0;
		int			lastRenderedId = -1;

		#endregion


		#region constructor
		private Thumbnail()
		{
			InitializeComponent();

			this.computingStoryboard = this.Resources["lockStoryboardKey"] as System.Windows.Media.Animation.Storyboard;

			this.dlgItImageChanged = delegate() { ItImageChangedTU(); };
			this.dlgItImageSettingsChanged = delegate() { ItImageSettingsChangedTU(); };
			this.dlgUpdated = delegate() { UpdatedTU(); };
			this.dlgSelectRequested = delegate() { this.IsSelected = true; };
			this.dlgBringIntoViewRequest = delegate() { this.BringIntoView(); };

			ContextMenu contextMenu = new ContextMenu();
			this.ContextMenu = contextMenu;
			MenuItem itemRecreateThumb = new MenuItem();
			itemRecreateThumb.Header = "Recreate Thumbnail";
			itemRecreateThumb.Click += new RoutedEventHandler(RecreateThumbail_Click);
			contextMenu.Items.Add(itemRecreateThumb);
			this.ContextMenu = contextMenu;

			/*this.SizeChanged += new SizeChangedEventHandler(Thumbnail_SizeChanged);
			this.Checked += new RoutedEventHandler(Thumbnail_Checked);
			this.Unchecked += new RoutedEventHandler(Thumbnail_Unchecked);*/

			/*this.checkBox.Checked += delegate(object sender, RoutedEventArgs args)
			{
				this.IsSelected = true;
			};*/
			/*this.checkBox.Unchecked += delegate(object sender, RoutedEventArgs e)
			{
				this.IsSelected = false;
			};*/

//            this.toggleButton.ApplyTemplate();
//            _innerRect = (Rectangle)this.toggleButton.Template.FindName("innerRect", toggleButton);
//            _innerRect.Fill = new SolidColorBrush(Colors.LightBlue); 

			this.DataContext = this;
		}

		internal Thumbnail(VpImage vpImage, StripPane stripPane)
			: this()
		{
			this.vpImage = vpImage;
			this.stripPane = stripPane;

            if (this.vpImage.IsPullSlip)
            {
                this.InnerRectBrush = new SolidColorBrush(Colors.LightBlue);
            }

			//this.Image = CreateBitmapImage();
                                   
			this.Background = new SolidColorBrush(Colors.Snow);            

			if (this.stripPane.Orientation == Orientation.Horizontal)
				this.Width = Thumbnail.ThumbnailSize.Width;
			else
				this.Height = Thumbnail.ThumbnailSize.Height;

			stripPane.OrientationChanged += new RoutedEventHandler(ThumbsControl_OrientationChanged);

			if (this.vpImage.ToolTip != null && this.vpImage.ToolTip.Length > 0)
			{
				//this.ToolTip = new ToolTip();
				//this.ToolTip.Opened += new RoutedEventHandler(ToolTip_Opened);
				this.ToolTip = this.vpImage.ToolTip;
			}

			this.vpImage.ItImageChanged += delegate() { this.Dispatcher.BeginInvoke(this.dlgItImageChanged); };
			this.vpImage.ItImageSettingsChanged += delegate() { this.Dispatcher.BeginInvoke(this.dlgItImageSettingsChanged); };
			this.vpImage.Updated += delegate() { this.Dispatcher.Invoke(this.dlgUpdated); };
			this.vpImage.SelectRequested += delegate() { this.Dispatcher.Invoke(this.dlgSelectRequested); };
			this.vpImage.BringIntoViewRequested += delegate() { this.Dispatcher.Invoke(this.dlgBringIntoViewRequest); };

			this.IsFixed = vpImage.IsFixed;
			this.IsIndependent = vpImage.IsIndependent;
            
			CreateThumbnailImage();
			//AdjustItClips();
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

		public VpImage VpImage { get { return this.vpImage; } }
		public bool AllowTransforms { get { return this.vpImage.IsItActive; } }
		internal ViewPane.Hierarchy.VpImage.VpImageType ImageType { get { return this.vpImage.ImageType; } }

        #region InnerRectBrush
        public Brush InnerRectBrush
        {
            get { return (Brush)GetValue(InnerRectBrushProperty); }
            set { SetValue(InnerRectBrushProperty, value); }
        }
        #endregion

		#region Image
		public BitmapSource Image
		{
			get { return (BitmapSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}
		#endregion

		#region ItGeometry
		public Geometry ItGeometry
		{
			get { return (Geometry)GetValue(ItGeometryProperty); }
			set { SetValue(ItGeometryProperty, value); }
		}
		#endregion

		#region BackgroundBrush
		public Brush BackgroundBrush
		{
			get { return (Brush)GetValue(BackgroundBrushProperty); }
			set { SetValue(BackgroundBrushProperty, value); }
		}
		#endregion

		#region IndependenceBrush
		public Brush IndependenceBrush
		{
			get { return (Brush)GetValue(independenceBrushProperty); }
			set { SetValue(independenceBrushProperty, value); }
		}
		#endregion

		#region IsFixed
		public bool IsFixed
		{
			get { return (bool)GetValue(isFixedProperty); }
			set { SetValue(isFixedProperty, value); }
		}
		#endregion

		#region IsIndependent
		public bool IsIndependent
		{
			get { return (bool)GetValue(isIndependentProperty); }
			set { SetValue(isIndependentProperty, value); }
		}
		#endregion

		#region Header
		public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}
		#endregion

		#region ItImage
		public ImageProcessing.IpSettings.ItImage ItImage
		{
			get
			{
				if (this.vpImage.IsItActive && this.vpImage.IsFixed == false)
					return this.vpImage.ItImage;
				else
					return null;
			}
		}
		#endregion

		#region IsSelected
		public bool IsSelected
		{
			get { return (bool) GetValue(isSelectedProperty); }
			set { SetValue(isSelectedProperty, value); }
		}
		#endregion

		#region IsVisibleInScrollViewer
		public bool IsVisibleInScrollViewer
		{
			get { return (bool)GetValue(visibleInScrollViewerProperty); }
			set { SetValue(visibleInScrollViewerProperty, value); }
		}
		#endregion

		#region IsDisplayed
		public bool IsDisplayed
		{
			get { return (bool)this.Dispatcher.Invoke((Func<bool>)delegate() { return (this.IsVisibleInScrollViewer && this.IsVisible); }); }
		}
		#endregion

		#endregion


		// PRIVATE PROPERTIES
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
		public void ThumbnailCreatedDelegate(System.Drawing.Bitmap bitmap, int renderingId, TimeSpan timeSpan)
		{
			this.Dispatcher.BeginInvoke((Action)delegate() { ThumbnailCreated(bitmap, renderingId, timeSpan); });
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

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region ThumbsControl_OrientationChanged()
		void ThumbsControl_OrientationChanged(object sender, RoutedEventArgs e)
		{
			if (this.stripPane.Orientation == Orientation.Horizontal)
			{
				this.Width = Thumbnail.ThumbnailSize.Width;
				this.Height = double.NaN;
			}
			else
			{
				this.Width = double.NaN;
				this.Height = Thumbnail.ThumbnailSize.Height;
			}

			AdjustItClips();
		}
		#endregion


		#region CreateThumbnailImage()
		private void CreateThumbnailImage()
		{
			if (this.IsVisible && this.ActualWidth > 12 && this.ActualHeight > 12 && this.IsVisibleInScrollViewer)
			{
				if (this.renderingCounter > this.lastRenderedId)
				{
					this.vpImage.GetBitmapAsync(this, this.renderingCounter, false, (int)this.ActualWidth - 12, (int)this.ActualHeight - 12);
					
					this.Status = DrawingStatus.Computing;
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
				bool releaseMemory = (this.Image != null);
				BitmapSource bitmapSource = ViewPane.Misc.Misc.GetBitmapSource(bitmap);

				bitmap.Dispose();
				this.Image = bitmapSource;
				//this.imagePanel.Width = bitmapSource.Width;
				//this.imagePanel.Height = bitmapSource.Height;
				this.Status = DrawingStatus.Idle;
				this.lastRenderedId = renderingId;

				AdjustItClips();

				if(releaseMemory)
					ViewPane.Misc.MemoryManagement.ReleaseUnusedMemory();
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
	
		
		#region ToolTip_Opened()
		void ToolTip_Opened(object sender, RoutedEventArgs e)
		{
			/*if (this.vpImage.ImageType == VpImage.VpImageType.ScanImage && this.ScanData != null)
			{
				toolTip.Content = ViewPane2Strings.ScanID_STR + this.ScanData.TheScanData.ScanID + Environment.NewLine +
					ViewPane2Strings.BaseName_STR + System.IO.Path.GetFileNameWithoutExtension(this.ScanData.TheScanImageData.FullImagePath) + Environment.NewLine +
					ViewPane2Strings.Created_STR + this.ScanData.TheScanImageData.ImageDate + Environment.NewLine +
					ViewPane2Strings.BookPart_STR + Toolbar.ToolbarBookInfo.GetCaption(this.ScanData.TheScanData.BookPartType) + Environment.NewLine +
					ViewPane2Strings.Brightness_STR + this.ScanData.TheScanImageData.Brightness + Environment.NewLine +
					ViewPane2Strings.Contrast_STR + this.ScanData.TheScanImageData.Contrast + Environment.NewLine +
					ViewPane2Strings.Gamma_STR + this.ScanData.TheScanImageData.Gamma;
			}
			else if (this.vpImage.ImageType == VpImage.VpImageType.ItImage && this.ItImageData != null)
			{
				toolTip.Content = ViewPane2Strings.PageID_STR + this.ItImageData.fItPageID + Environment.NewLine +
					ViewPane2Strings.BaseName_STR + System.IO.Path.GetFileNameWithoutExtension(this.ItImageData.FullImagePath) + Environment.NewLine +
					ViewPane2Strings.Created_STR + this.ItImageData.CreationDate.ToString() + Environment.NewLine 
					;
			}*/

			//toolTip.Content = this.vpImage.ToolTip;
		}
		#endregion

		#region CreateBitmapImage()
		BitmapImage CreateBitmapImage()
		{
			FileStream reader = new FileStream(this.vpImage.ThumbnailPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			BitmapImage bitmap = new BitmapImage();

			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.StreamSource = reader;
			bitmap.EndInit();

			reader.Close();
			return bitmap;
		}
		#endregion

		#region ItImageChangedTU()
		void ItImageChangedTU()
		{
			try
			{
				if (this.vpImage.IsItActive && this.vpImage.ItImage != null)
				{
					IsFixed = this.vpImage.IsFixed;
					IsIndependent = this.vpImage.IsIndependent;

					AdjustItClips();
				}
			}
			catch { }
		}
		#endregion

		#region ItImageSettingsChangedTU()
		void ItImageSettingsChangedTU()
		{
			try
			{
				if (this.vpImage.IsItActive && this.vpImage.ItImage != null)
				{
					IsFixed = this.vpImage.IsFixed;
					IsIndependent = this.vpImage.IsIndependent;

					AdjustItClips();
				}
			}
			catch { }
		}
		#endregion

		#region OnIsFixedChanged()
		private static void OnIsFixedChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
		}
		#endregion

		#region OnIsIndependentChanged()
		private static void OnIsIndependentChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			Thumbnail instance = sender as Thumbnail;

			if (instance.AllowTransforms)
			{
				instance.BackgroundBrush = (instance.IsIndependent) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGreen);
				instance.IndependenceBrush = (instance.IsIndependent) ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.LightGreen);
			}
		}
		#endregion

		#region RecreateThumbail_Click()
		private void RecreateThumbail_Click(object sender, RoutedEventArgs e)
		{
			//this.Image = CreateBitmapImage();
			this.renderingCounter++;
			CreateThumbnailImage();
		}
		#endregion

		#region AdjustItClips()
		private void AdjustItClips()
		{
			if (this.IsFixed == false && this.AllowTransforms && this.ItImage != null && this.Image != null)
			{
				double width = this.Image.PixelWidth;
				double height = this.Image.PixelHeight;
				//double devWidth = this.IsSelected.Value ? this.ActualWidth - 10 : this.ActualWidth - 12;
				//double devHeight = this.IsSelected.Value ? this.ActualHeight - 10 : this.ActualHeight - 14;
				double devWidth = this.ActualWidth - 8;
				double devHeight = this.ActualHeight - 8;

				double zoom = Math.Min(devWidth / width, devHeight / height);

				if (zoom > 0)
				{
					width *= zoom;
					height *= zoom;

					if (this.ItImage.TwoPages)
					{
						try
						{
							ImageProcessing.IpSettings.ItPage pageL = this.ItImage.PageL;
							ImageProcessing.IpSettings.ItPage pageR = this.ItImage.PageR;

							GeometryGroup gg = new GeometryGroup();
							Geometry geometry = new RectangleGeometry(new Rect(0, 0, width, height));
							Geometry geometryL = new RectangleGeometry(new Rect(pageL.ClipRect.X * width, pageL.ClipRect.Y * height,
								pageL.ClipRect.Width * width, pageL.ClipRect.Height * height));
							Geometry geometryR = new RectangleGeometry(new Rect(pageR.ClipRect.X * width, pageR.ClipRect.Y * height,
								pageR.ClipRect.Width * width, pageR.ClipRect.Height * height));

							geometryL.Transform = new RotateTransform(pageL.Skew * 180 / Math.PI, geometryL.Bounds.Left + geometryL.Bounds.Width / 2, geometryL.Bounds.Top + geometryL.Bounds.Height / 2);
							geometryR.Transform = new RotateTransform(pageR.Skew * 180 / Math.PI, geometryR.Bounds.Left + geometryR.Bounds.Width / 2, geometryR.Bounds.Top + geometryR.Bounds.Height / 2);

							gg.Children.Add(geometry);
							gg.Children.Add(geometryL);
							gg.Children.Add(geometryR);

							this.ItGeometry = gg;
						}
						catch (Exception ex)
						{
							throw ex;
						}
					}
					else
					{
						ImageProcessing.IpSettings.ItPage page = this.ItImage.PageL;

						if (page.ClipSpecified)
						{
							try
							{
								GeometryGroup gg = new GeometryGroup();
								Geometry geometry = new RectangleGeometry(new Rect(0, 0, width, height));
								Geometry geometryL = new RectangleGeometry(new Rect(page.ClipRect.X * width, page.ClipRect.Y * height,
									page.ClipRect.Width * width, page.ClipRect.Height * height));

								geometryL.Transform = new RotateTransform(page.Skew * 180 / Math.PI, geometryL.Bounds.Left + geometryL.Bounds.Width / 2, geometryL.Bounds.Top + geometryL.Bounds.Height / 2);

								gg.Children.Add(geometry);
								gg.Children.Add(geometryL);

								this.ItGeometry = gg;
							}
							catch (Exception ex)
							{
								throw ex;
							}
						}
						else
							this.ItGeometry = new GeometryGroup();
					}
				}
				else
					this.ItGeometry = new GeometryGroup();
			}
			else
			{
				this.ItGeometry = new GeometryGroup();
			}
		}
		#endregion

		#region Thumbnail_SizeChanged()
		void Thumbnail_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.renderingCounter++;
			CreateThumbnailImage();
		}
		#endregion

		#region Thumbnail_Checked()
		void Thumbnail_Checked(object sender, RoutedEventArgs e)
		{
			AdjustItClips();
		}
		#endregion

		#region Thumbnail_Unchecked()
		void Thumbnail_Unchecked(object sender, RoutedEventArgs e)
		{
			AdjustItClips();
		}
		#endregion

		#region UpdatedTU()
		void UpdatedTU()
		{
			try
			{
				this.Image = CreateBitmapImage();

				if (this.stripPane.Orientation == Orientation.Horizontal)
					this.Width = Thumbnail.ThumbnailSize.Width;
				else
					this.Height = Thumbnail.ThumbnailSize.Height;

				this.renderingCounter++;
				CreateThumbnailImage();
			}
			catch { }
		}
		#endregion

		#region IsEnabled_Changed()
		private void IsEnabled_Changed(object sender, DependencyPropertyChangedEventArgs e)
		{
			/*if(this.IsEnabled)
				this.innerRect.Fill = new SolidColorBrush(Colors.Gray);
			else
				this.innerRect.Fill = new SolidColorBrush(Colors.White);*/
		}
		#endregion

		#region OnIsSelectedChanged()
		static void OnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			Thumbnail instance = (Thumbnail)sender;

			if ((bool)args.NewValue)
			{
				//instance.outerRect.Fill = new SolidColorBrush(Colors.Red);
				instance.toggleButton.IsChecked = true;
				if (instance.Selected != null)
					instance.Selected(instance);
				//instance.RaiseEvent(new RoutedEventArgs(Thumbnail.checkedEvent));
			}
			else
			{
				//instance.outerRect.Fill = new SolidColorBrush(Colors.White);
				instance.toggleButton.IsChecked = false;
				if (instance.Unselected != null)
					instance.Unselected(instance);
				//instance.RaiseEvent(new RoutedEventArgs(Thumbnail.uncheckedEvent));
			}
		}
		#endregion

		#region Preview_MouseDown()
		private void Preview_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (this.IsSelected)
			{
				/*if (stripPane != null)
				{
					if (stripPane.Status == ThumbnailsPanel.ControlStatus.Default)
						stripPane.FullScreen_Request(this);
					else
						this.IsSelected = false;
				}*/
			}
			else
				this.IsSelected = true;

			e.Handled = true;
		}
		#endregion

		#region Preview_MouseUp()
		private void Preview_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (this.IsSelected)
			{
				/*if (stripPane != null)
				{
					if (stripPane.Status == ThumbnailsPanel.ControlStatus.Default)
						stripPane.FullScreen_Request(this);
					else
						this.IsSelected = false;
				}*/
			}
			else
				this.IsSelected = true;
		}
		#endregion

		#region IsVisibleInScrollViewer_Changed()
		static void IsVisibleInScrollViewer_Changed(object sender, DependencyPropertyChangedEventArgs args)
		{
			Thumbnail thumbnail = (Thumbnail)sender;

			if ((bool)args.NewValue)
			{
				thumbnail.CreateThumbnailImage();
			}
			else
			{
			}
		}
		#endregion

		#region BringIntoView_Request()
		private void BringIntoView_Request(object sender, RequestBringIntoViewEventArgs e)
		{
			this.IsVisibleInScrollViewer = true;
		}
		#endregion

		#endregion

	}
}
