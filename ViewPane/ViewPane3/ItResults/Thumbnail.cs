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
using System.IO;
using ViewPane.IP;

namespace ViewPane.ItResults
{
	[SmartAssembly.Attributes.DoNotPrune]
	[SmartAssembly.Attributes.DoNotPruneType]
	[SmartAssembly.Attributes.DoNotObfuscate]
	[SmartAssembly.Attributes.DoNotObfuscateType]
	public partial class Thumbnail : RadioButton, IDisposable
	{
		#region variables

		ViewPane.Hierarchy.VpImage			vpImage;
		ViewPane.ItResults.StripPane		stripPane;
		ItResultsImage						itResultsImage = null;

		ToolTip toolTip;

		public static DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapSource), typeof(Thumbnail), new PropertyMetadata(null));
		public static DependencyProperty ItGeometryProperty = DependencyProperty.Register("ItGeometry", typeof(Geometry), typeof(Thumbnail), new PropertyMetadata(null));
		public static DependencyProperty BackgroundBrushProperty = DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(Thumbnail));
		public static DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(Thumbnail), new PropertyMetadata(""));        

		//Size thumbnailSize = new Size(135, 120);

		public delegate void ThumbnailSelectedHnd(ViewPane.ItResults.Thumbnail thumbnail);

		public delegate void VoidEventHnd();

		//public event VoidEventHnd ItUpdating;
		//public event VoidEventHnd ItUpdated;
		public event ThumbnailSelectedHnd Selected;

		static object	threadLocker = new object();

		#endregion


		#region constructor
		static Thumbnail()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ViewPane.ItResults.Thumbnail), new FrameworkPropertyMetadata(typeof(ViewPane.ItResults.Thumbnail)));
		}

		private Thumbnail()
		{
			ContextMenu contextMenu = new ContextMenu();
			this.ContextMenu = contextMenu;
			MenuItem itemRecreateThumb = new MenuItem();
			itemRecreateThumb.Header = "Recreate Thumbnail";
			itemRecreateThumb.Click += new RoutedEventHandler(RecreateThumbail_Click);
			contextMenu.Items.Add(itemRecreateThumb);
			this.ContextMenu = contextMenu;

			this.SizeChanged += new SizeChangedEventHandler(Thumbnail_SizeChanged);
			this.Checked += new RoutedEventHandler(Thumbnail_Checked);
			this.Unchecked += new RoutedEventHandler(Thumbnail_Unchecked);
			this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Preview_MouseLeftButtonDown);

			this.BackgroundBrush = new SolidColorBrush(Colors.White);            
		}

		internal Thumbnail(ViewPane.Hierarchy.VpImage vpImage, ViewPane.ItResults.StripPane stripPane)// OpusObjectManagerNS.FullScanData scanData, ImageProcessing.IpSettings.ItImage itImage, ThumbnailsControl thumbsControl)
			: this()
		{
			this.vpImage = vpImage;
			this.stripPane = stripPane;

			this.Image = CreateBitmapImage(this.vpImage.ThumbnailPath);
			//this.Background = new SolidColorBrush(Colors.Snow);
            if (this.vpImage.IsPullSlip)
            {
                this.Background = new SolidColorBrush(Colors.LightBlue);
            }
            else
            {
                this.Background = new SolidColorBrush(Colors.Snow);
            }            

			if (this.stripPane.Orientation == Orientation.Horizontal)
				this.Width = ViewPane.Thumbnails.Thumbnail.ThumbnailSize.Width;
			else
				this.Height = ViewPane.Thumbnails.Thumbnail.ThumbnailSize.Height;

			stripPane.OrientationChanged += new RoutedEventHandler(ThumbsControl_OrientationChanged);

			toolTip = new ToolTip();
			toolTip.Opened += new RoutedEventHandler(ToolTip_Opened);
			this.ToolTip = toolTip;

			AdjustItClips();

			this.itResultsImage = new ItResultsImage(vpImage);

			if (this.ItImage != null)
				this.ItImage.Changed += new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties
		public ViewPane.Hierarchy.VpImage			VpImage			{ get { return this.vpImage; } }
		public ImageProcessing.IpSettings.ItImage	ItImage			{ get { return (this.vpImage != null) ? this.vpImage.ItImage : null; } }
		//internal ItResultsImage						ItResultsImage { get { return this.itResultsImage; } }
		public System.Drawing.Size					FullImageSizeL	{ get { return this.itResultsImage.FullImageSizeL; } }
		public System.Drawing.Size					FullImageSizeR	{ get { return this.itResultsImage.FullImageSizeR; } }

		public BitmapSource Image
		{
			get { return (BitmapSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public Geometry ItGeometry
		{
			get { return (Geometry)GetValue(ItGeometryProperty); }
			set { SetValue(ItGeometryProperty, value); }
		}

		public Brush BackgroundBrush
		{
			get { return (Brush)GetValue(BackgroundBrushProperty); }
			set { SetValue(BackgroundBrushProperty, value); }
		}

		public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		#endregion        

        //PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			if (this.ItImage != null)
				this.ItImage.Changed -= new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);
			
			this.itResultsImage.Dispose();
		}
		#endregion

		#region Update()
		/*public void Update()
		{
			if (ItUpdating != null)
				ItUpdating();
			
			this.Image = CreateBitmapImage(vpImage.ThumbnailPath);

			if (this.stripPane.Orientation == Orientation.Horizontal)
				this.Width = ViewPane.Thumbnails.Thumbnail.ThumbnailSize.Width;
			else
				this.Height = ViewPane.Thumbnails.Thumbnail.ThumbnailSize.Height;

			AdjustItClips();

			lock (threadLocker)
			{
				itResultsImage.Dispose();
				itResultsImage = new ItResultsImage(this.vpImage);
			}

			if (ItUpdated != null)
				ItUpdated();
		}*/
		#endregion

		#region Select()
		public void Select()
		{
			this.IsChecked = true;

			this.BringIntoView();

			if (Selected != null)
				Selected(this);
		}
		#endregion

		#region GetClip()
		public System.Drawing.Bitmap GetClip(bool leftPage, Rect clip, double zoom)
		{
			lock (threadLocker)
			{
				return this.itResultsImage.GetClip(leftPage, clip, zoom);
			}
		}
		#endregion

		#region GetClipAsync()
		internal void GetClipAsync(ViewPane.IP.IitResultsCaller caller, int renderingId, bool leftPage, bool highPriority, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			this.itResultsImage.GetClipAsync(caller, renderingId, leftPage, highPriority, imageRect, zoom, quality);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region CreateBitmap()
		/*private BitmapImage CreateBitmap(Uri source, int preferredHeight)
		{
			BitmapImage thumbnail = new BitmapImage();

			thumbnail.BeginInit();
			thumbnail.DecodePixelHeight = preferredHeight;
			thumbnail.CacheOption = BitmapCacheOption.OnLoad;
			thumbnail.UriSource = source;
			thumbnail.EndInit();

			return thumbnail;
		}*/
		#endregion

		#region ThumbsControl_OrientationChanged()
		void ThumbsControl_OrientationChanged(object sender, RoutedEventArgs e)
		{
			if (this.stripPane.Orientation == Orientation.Horizontal)
			{
				this.Width = ViewPane.Thumbnails.Thumbnail.ThumbnailSize.Width;
				this.Height = double.NaN;
			}
			else
			{
				this.Width = double.NaN;
				this.Height = ViewPane.Thumbnails.Thumbnail.ThumbnailSize.Height;
			}
		}
		#endregion

		#region IsFileAvailable()
		private bool IsFileAvailable(string path)
		{
			// try and read the file...
			int count = 0;

			while (count <= 400)
			{
				count = count + 10;
				try
				{
					using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
					{
						fs.Close();
					}

					return true;
				}
				catch (IOException)
				{
					System.Threading.Thread.Sleep(count);
				}
				catch (UnauthorizedAccessException)
				{
					System.Threading.Thread.Sleep(count);
				}
			}

			return false;
		}
		#endregion

		#region ToolTip_Opened()
		void ToolTip_Opened(object sender, RoutedEventArgs e)
		{
			/*if (this.scanData != null)
			{
				toolTip.Content = ViewPane.Languages.UiStrings.ScanID_STR + scanData.TheScanData.ScanID + Environment.NewLine +
					ViewPane.Languages.UiStrings.BaseName_STR + System.IO.Path.GetFileNameWithoutExtension(scanData.TheScanImageData.FullImagePath) + Environment.NewLine +
					ViewPane.Languages.UiStrings.Created_STR + scanData.TheScanImageData.ImageDate + Environment.NewLine +
					ViewPane.Languages.UiStrings.BookPart_STR + Toolbar.ToolbarBookInfo.GetCaption(scanData.TheScanData.BookPartType) + Environment.NewLine +
					ViewPane.Languages.UiStrings.Brightness_STR + scanData.TheScanImageData.Brightness + Environment.NewLine +
					ViewPane.Languages.UiStrings.Contrast_STR + scanData.TheScanImageData.Contrast + Environment.NewLine +
					ViewPane.Languages.UiStrings.Gamma_STR + scanData.TheScanImageData.Gamma;
			}*/
			if (this.vpImage.ToolTip != null)
				toolTip.Content = this.vpImage.ToolTip;
		}
		#endregion

		#region CreateBitmapImage()
		BitmapImage CreateBitmapImage(string file)
		{
			FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
			BitmapImage bitmap = new BitmapImage();

			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.StreamSource = reader;
			bitmap.EndInit();

			reader.Close();
			return bitmap;
		}
		#endregion

		#region RecreateThumbail_Click()
		private void RecreateThumbail_Click(object sender, RoutedEventArgs e)
		{
			this.Image = CreateBitmapImage(vpImage.ThumbnailPath);
			AdjustItClips();
		}
		#endregion

		#region AdjustItClips()
		private void AdjustItClips()
		{
			if (this.ActualWidth > 0 && this.ActualHeight > 0)
			{
				if (this.ItImage != null && this.ItImage.IsFixed == false && (this.ItImage.TwoPages || this.ItImage.PageL.ClipSpecified))
				{
					double width = this.Image.PixelWidth;
					double height = this.Image.PixelHeight;
					double devWidth = this.IsChecked.Value ? this.ActualWidth - 10 : this.ActualWidth - 12;
					double devHeight = this.IsChecked.Value ? this.ActualHeight - 10 : this.ActualHeight - 14;

					double zoom = Math.Min(devWidth / width, devHeight / height);

					if (zoom > 0)
					{
						width *= zoom;
						height *= zoom;

						if (this.ItImage.TwoPages)
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
						else
						{
							ImageProcessing.IpSettings.ItPage page = this.ItImage.PageL;

							if (page.ClipSpecified)
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
		}
		#endregion

		#region Thumbnail_SizeChanged()
		void Thumbnail_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			AdjustItClips();
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

		#region Preview_MouseLeftButtonDown()
		void Preview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (this.IsChecked == false)
			{
				this.IsChecked = true;

				if (Selected != null)
					Selected(this);
			}

			e.Handled = true;
		}
		#endregion

		#region ItImage_Changed()
		void ItImage_Changed(ImageProcessing.IpSettings.ItImage itImage, ImageProcessing.IpSettings.ItProperty type)
		{
			if (this.Dispatcher.CheckAccess())
				ItImageChangedTU(itImage, type);
			else
				this.Dispatcher.Invoke((Action)delegate() { ItImageChangedTU(itImage, type); });
		}
		#endregion

		#region ItImageChangedTU()
		void ItImageChangedTU(ImageProcessing.IpSettings.ItImage itImage, ImageProcessing.IpSettings.ItProperty type)
		{
			try
			{
				if ((this.ItImage == itImage) && ((type & ImageProcessing.IpSettings.ItProperty.Clip) != 0))
				{
					AdjustItClips();

					//if (ItUpdated != null)
					//	ItUpdated();
				}
			}
			catch (Exception)
			{
			}
		}
		#endregion

		#endregion

	}
}
