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
using System.Windows.Media.Animation;

namespace ViewPane.Thumbnails
{
	/// <summary>
	/// Interaction logic for StripPane.xaml
	/// </summary>
	public partial class StripPane : UserControl
	{
		VpImages vpImages = new VpImages();
		
		public static DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(StripPane), new PropertyMetadata(true, new PropertyChangedCallback(OnExpanded_Changed)));
		public static DependencyProperty IsPinnedProperty = DependencyProperty.Register("IsPinned", typeof(bool), typeof(StripPane), new PropertyMetadata(true, new PropertyChangedCallback(OnPinned_Changed)));
		public static DependencyProperty ClientRectSizeProperty = DependencyProperty.Register("ClientRectSize", typeof(int), typeof(StripPane), new PropertyMetadata(133, new PropertyChangedCallback(OnClientRectSize_Changed)));

		ViewPane.Licensing licensing = new Licensing();

		bool dragStarted = false;

		public static RoutedEvent OrientationChangedEvent = EventManager.RegisterRoutedEvent("OrientationChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StripPane));

		public delegate void ImageSelectedHnd(VpImage vpImage);
		public event ImageSelectedHnd ImageSelected;


		#region constructor
		public StripPane()
		{
			InitializeComponent();

			this.vpImages.ImageAdded += new ImageAddingEventHnd(Image_Added);
			this.vpImages.ImageInserted += new ImageInsertingEventHnd(Image_Inserted);
			this.vpImages.ImageRemoving += new ImageRemovingEventHnd(Image_Removing);
			this.vpImages.Clearing += new ClearingEventHnd(Images_Clearing);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public VpImages		Images { get { return this.vpImages; } }

		#region Parent
		new public DockPanel Parent
		{
			get { return (DockPanel)base.Parent; }
		}
		#endregion

		#region Dock
		public Dock Dock
		{
			get 
			{
				return DockPanel.GetDock(this);
			}
			set 
			{ 
				DockPanel.SetDock(this, value);

				switch (DockPanel.GetDock(this))
				{
					case Dock.Left:
						{
							DockPanel.SetDock(this.stripFence, Dock.Right);
							DockPanel.SetDock(this.stripControl, Dock.Top);

							this.Width = 140;
							this.BeginAnimation(StripPane.HeightProperty, null);
							this.Height = double.NaN;
							
							this.stripFence.Width = 7;
							this.stripFence.Height = double.NaN;
							this.fenceRotation.Angle = 90;

							this.stripControl.Width = double.NaN;
							this.stripControl.Height = 17;

							buttonPin.HorizontalAlignment = HorizontalAlignment.Right;
							buttonPin.VerticalAlignment = VerticalAlignment.Center;

							stripControlFence.HorizontalAlignment = HorizontalAlignment.Left;
							stripControlFence.VerticalAlignment = VerticalAlignment.Center;
							stripControlFenceRotation.Angle = 90;
							this.Orientation = Orientation.Vertical;
						} break;
					case Dock.Top:
						{
							DockPanel.SetDock(this.stripFence, Dock.Bottom);
							DockPanel.SetDock(this.stripControl, Dock.Right);

							this.BeginAnimation(StripPane.WidthProperty, null);
							this.Width = double.NaN;
							this.Height = 140;

							this.stripFence.Width = double.NaN;
							this.stripFence.Height = 7;
							this.fenceRotation.Angle = 0;

							this.stripControl.Width = 17;
							this.stripControl.Height = double.NaN;

							buttonPin.HorizontalAlignment = HorizontalAlignment.Center;
							buttonPin.VerticalAlignment = VerticalAlignment.Bottom;

							stripControlFence.HorizontalAlignment = HorizontalAlignment.Center;
							stripControlFence.VerticalAlignment = VerticalAlignment.Top;
							stripControlFenceRotation.Angle = 0;
							this.Orientation = Orientation.Horizontal;
						} break;
					case Dock.Right:
						{
							DockPanel.SetDock(this.stripFence, Dock.Left);
							DockPanel.SetDock(this.stripControl, Dock.Top);

							this.Width = 140;
							this.BeginAnimation(StripPane.HeightProperty, null);
							this.Height = double.NaN;
							
							this.stripFence.Width = 7;
							this.stripFence.Height = double.NaN;
							this.fenceRotation.Angle = 90;

							this.stripControl.Width = double.NaN;
							this.stripControl.Height = 17;

							buttonPin.HorizontalAlignment = HorizontalAlignment.Right;
							buttonPin.VerticalAlignment = VerticalAlignment.Center;

							stripControlFence.HorizontalAlignment = HorizontalAlignment.Left;
							stripControlFence.VerticalAlignment = VerticalAlignment.Center;
							stripControlFenceRotation.Angle = 90;
							this.Orientation = Orientation.Vertical;
						} break;
					case Dock.Bottom:
						{
							DockPanel.SetDock(this.stripFence, Dock.Top);
							DockPanel.SetDock(this.stripControl, Dock.Right);

							this.BeginAnimation(StripPane.WidthProperty, null);
							this.Width = double.NaN;
							this.Height = 140;

							this.stripFence.Width = double.NaN;
							this.stripFence.Height = 7;
							this.fenceRotation.Angle = 0;

							this.stripControl.Width = 17;
							this.stripControl.Height = double.NaN;

							buttonPin.HorizontalAlignment = HorizontalAlignment.Center;
							buttonPin.VerticalAlignment = VerticalAlignment.Bottom;

							stripControlFence.HorizontalAlignment = HorizontalAlignment.Center;
							stripControlFence.VerticalAlignment = VerticalAlignment.Top;
							stripControlFenceRotation.Angle = 0;
							this.Orientation = Orientation.Horizontal;
						} break;
				}
			}
		}
		#endregion

		#region IsExpanded
		public bool IsExpanded
		{
			get { return (bool)GetValue(IsExpandedProperty); }
			set { SetValue(IsExpandedProperty, value); }
		}
		#endregion

		#region IsPinned
		public bool IsPinned
		{
			get { return (bool)GetValue(IsPinnedProperty); }
			set { SetValue(IsPinnedProperty, value); }
		}
		#endregion

		#region ClientRectSize
		public int ClientRectSize
		{
			get { return (int)GetValue(ClientRectSizeProperty); }
			set { SetValue(ClientRectSizeProperty, value); }
		}
		#endregion

		#region Orientation
		public Orientation Orientation
		{
			get { return this.stackPanelThumbnails.Orientation; }
			set
			{
				this.stackPanelThumbnails.Orientation = value;
				this.scrollViewer.HorizontalScrollBarVisibility = (value == Orientation.Horizontal) ? ScrollBarVisibility.Visible : ScrollBarVisibility.Disabled;
				this.scrollViewer.VerticalScrollBarVisibility = (value == Orientation.Vertical) ? ScrollBarVisibility.Visible : ScrollBarVisibility.Disabled;
				RaiseEvent(new RoutedEventArgs(StripPane.OrientationChangedEvent, this));
			}
		}
		#endregion

		#region event OrientationChanged
		public event RoutedEventHandler OrientationChanged
		{
			add { AddHandler(StripPane.OrientationChangedEvent, value); }
			remove { RemoveHandler(StripPane.OrientationChangedEvent, value); }
		}
		#endregion

		#region Thumbnails
		public List<ViewPane.Thumbnails.Thumbnail> Thumbnails
		{
			get
			{
				List<ViewPane.Thumbnails.Thumbnail> thumbnails = new List<ViewPane.Thumbnails.Thumbnail>();

				foreach (UIElement element in this.stackPanelThumbnails.Children)
					if (element is Thumbnail)
						thumbnails.Add((Thumbnail)element);

				return thumbnails;
			}
		}
		#endregion

		#endregion


		//INTERNAL PROPERTIES
		#region internal properties

		#region Licensing
		public ViewPane.Licensing Licensing 
		{
			get { return licensing; }
			set { this.licensing = value; }
		}
		#endregion

		#region SelectedImage
		internal VpImage SelectedImage
		{
			get
			{
				foreach (Thumbnail thumbnail in this.stackPanelThumbnails.Children)
					if (thumbnail.IsSelected)
						return thumbnail.VpImage;

				return null;
			}
		}
		#endregion

		#region SelectedImageIndex
		/// <summary>
		/// returns -1 if no thumbnail is selected
		/// </summary>
		internal int SelectedImageIndex
		{
			get
			{
				for (int i = 0; i < this.stackPanelThumbnails.Children.Count; i++)
					if (((Thumbnail)this.stackPanelThumbnails.Children[i]).IsSelected)
						return i;

				return -1;
			}
		}
		#endregion
		
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		#region SelectedThumbnail
		private Thumbnail SelectedThumbnail
		{
			get
			{
				foreach (Thumbnail thumbnail in this.stackPanelThumbnails.Children)
					if (thumbnail.IsSelected)
						return thumbnail;

				return null;
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region LoadWrapper()
		/*public void LoadWrapper(OpusObjectManagerNS.OpusObjectWrapper wrapper)
		{
			Thumbnail thumbnail = null;

			foreach (OpusObjectManagerNS.FullScanData fullScanData in wrapper.GetAllImageDataSnapShot())
			{
				thumbnail = this.AddThumbnail(new VpImage(fullScanData));
			}

			//if (thumbnail != null)
				//thumbnail.IsSelected = true;
		}*/
		#endregion

		#region UpdateThumbnail()
		/*public Thumbnail UpdateThumbnail(OpusObjectManagerNS.FullScanData scanData)
		{
			Thumbnail thumbnail = GetThumbnail(scanData);

			if (thumbnail != null)
			{
				thumbnail.Update(scanData);
				thumbnail.BringIntoView();
			}
			else
			{
				int? index = null;

				for (int i = this.stackPanelThumbnails.Children.Count - 1; i>=0; i--)
				{
					Thumbnail t = (Thumbnail)this.stackPanelThumbnails.Children[i];
					
					if (t.ItImageData != null)
					{
						int scanId = t.ItImageData.GetScanID();

						if(scanId == scanData.TheScanData.ScanID)
						{
							index = this.stackPanelThumbnails.Children.IndexOf(t);
							RemoveThumbnail(t.ItImageData);
						}
					}
				}

				if (index.HasValue)
					InsertThumbnail(index.Value, new VpImage(scanData));
				else
					AddThumbnail(new VpImage(scanData));
			}

			return thumbnail;
		}

		public Thumbnail UpdateThumbnail(OpusObjectManagerNS.ItImageData itImageData)
		{
			Thumbnail thumbnail = GetThumbnail(itImageData);

			if (thumbnail != null)
			{
				thumbnail.Update(itImageData);
				thumbnail.BringIntoView();
			}

			return thumbnail;
		}*/
		#endregion

		#region UncheckAll()
		public void UncheckAll()
		{
			foreach (Thumbnail thumbnail in this.stackPanelThumbnails.Children)
				thumbnail.IsSelected = false;
		}
		#endregion

		#region SelectThumbnail()
		public Thumbnail SelectThumbnail(VpImage vpImage)
		{
			Thumbnail thumbnail = GetThumbnail(vpImage);

			if (thumbnail != null)
			{
				thumbnail.IsSelected = true;
				thumbnail.BringIntoView();
			}

			return thumbnail;
		}
		#endregion

		#region SelectFirstThumbnail()
		public Thumbnail SelectFirstThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				Thumbnail thumbnail = (Thumbnail)this.stackPanelThumbnails.Children[0];

				if (thumbnail != null)
				{
					thumbnail.IsSelected = true;
					thumbnail.BringIntoView();
				}

				return thumbnail;
			}
			else
				return null;
		}
		#endregion

		#region SelectPreviousThumbnail()
		public Thumbnail SelectPreviousThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				Thumbnail thumbnail = SelectedThumbnail;

				if (thumbnail != null)
				{
					int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

					if (index > 0)
					{
						Thumbnail t = (Thumbnail) this.stackPanelThumbnails.Children[index - 1];
						
						t.IsSelected = true;
						t.BringIntoView();
						return t;
					}
				}
			}

			return null;
		}
		#endregion

		#region SelectNextThumbnail()
		public Thumbnail SelectNextThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				Thumbnail thumbnail = SelectedThumbnail;

				if (thumbnail != null)
				{
					int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

					if (index < this.stackPanelThumbnails.Children.Count - 1)
					{
						Thumbnail t = (Thumbnail)this.stackPanelThumbnails.Children[index + 1];

						t.IsSelected = true;
						t.BringIntoView();
						return t;
					}
				}
			}

			return null;
		}
		#endregion


        #region SelectNextArticleThumbnail()
        public Thumbnail SelectNextArticleThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				Thumbnail thumbnail = SelectedThumbnail;

				if (thumbnail != null)
				{
					int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

                    if (index >= 0)
                    {                        
                        while (++index <= this.stackPanelThumbnails.Children.Count - 1)
                        {
                            Thumbnail t = (Thumbnail)this.stackPanelThumbnails.Children[index];
                            if (t.VpImage.IsPullSlip)
                            {
                                t.IsSelected = true;
                                t.BringIntoView();
                                break;
                            }
                        }
                    }
				}
			}

			return null;
		}
		#endregion

        #region SelectPreviousArticleThumbnail()
        public Thumbnail SelectPreviousArticleThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				Thumbnail thumbnail = SelectedThumbnail;

				if (thumbnail != null)
				{
					int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

                    if ( (index > 0) && (index < this.stackPanelThumbnails.Children.Count ) )
                    {                        
                        while (--index >= 0 )
                        {
                            Thumbnail t = (Thumbnail)this.stackPanelThumbnails.Children[index];
                            if (t.VpImage.IsPullSlip)
                            {
                                t.IsSelected = true;
                                t.BringIntoView();
                                break;
                            }
                        }
                    }
				}
			}

			return null;
		}
		#endregion

		#region SelectLastThumbnail()
		public Thumbnail SelectLastThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				Thumbnail thumbnail = (Thumbnail)this.stackPanelThumbnails.Children[this.stackPanelThumbnails.Children.Count - 1];

				if (thumbnail != null)
				{
					thumbnail.IsSelected = true;
					thumbnail.BringIntoView();
				}

				return thumbnail;
			}
			else
				return null;
		}
		#endregion

		#endregion

	
		//PRIVATE METHODS
		#region private methods

		#region Image_Added()
		void Image_Added(VpImage vpImage)
		{
			Thumbnail thumbnail = new Thumbnail(vpImage, this);
			thumbnail.Selected += new Thumbnail.ThumbanilEventHandler(Thumbnail_Checked);
			int index = this.stackPanelThumbnails.Children.Add(thumbnail);

			thumbnail.BringIntoView();
			thumbnail.Header = string.Format("{0}", index + 1);
		}
		#endregion

		#region Image_Inserted()
		void Image_Inserted(int index, VpImage vpImage)
		{
			Thumbnail thumbnail = new Thumbnail(vpImage, this);
			thumbnail.Selected += new Thumbnail.ThumbanilEventHandler(Thumbnail_Checked);

			if (index >= 0 && index < this.stackPanelThumbnails.Children.Count)
				this.stackPanelThumbnails.Children.Insert(index, thumbnail);
			else
				index = this.stackPanelThumbnails.Children.Add(thumbnail);

			thumbnail.BringIntoView();
			FixThumbnailsHeaders();
		}
		#endregion

		#region Image_Removing()
		void Image_Removing(VpImage vpImage)
		{
			Thumbnail thumbnail = GetThumbnail(vpImage);
			Thumbnail thumbToSelect = null;

			if (thumbnail != null)
			{			
				int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

				if (index >= 0 && index < this.stackPanelThumbnails.Children.Count - 1)
					thumbToSelect = (Thumbnail)this.stackPanelThumbnails.Children[index + 1];
				else if (index > 0)
					thumbToSelect = (Thumbnail)this.stackPanelThumbnails.Children[index - 1];

				thumbnail.Selected -= new Thumbnail.ThumbanilEventHandler(Thumbnail_Checked);
				thumbnail.IsSelected = false;
				this.stackPanelThumbnails.Children.Remove(thumbnail);
			}

			FixThumbnailsHeaders();
		}
		#endregion

		#region Images_Clearing()
		void Images_Clearing()
		{
			foreach (Thumbnail thumbnail in this.stackPanelThumbnails.Children)
			{
				thumbnail.IsSelected = false;
				thumbnail.Selected -= new Thumbnail.ThumbanilEventHandler(Thumbnail_Checked);
			}

			this.stackPanelThumbnails.Children.Clear();
			if (ImageSelected != null)
				ImageSelected(null);
		}
		#endregion

		#region OnExpanded_Changed()
		private static void OnExpanded_Changed(object sender, DependencyPropertyChangedEventArgs args)
		{
			StripPane instance = sender as StripPane;

			DoubleAnimation animation = new DoubleAnimation();
			animation.Duration = new Duration(TimeSpan.Parse("00:00:00.3"));
			animation.AutoReverse = false;
			animation.FillBehavior = FillBehavior.HoldEnd;

			if (instance.IsExpanded)
				animation.To = instance.ClientRectSize + 7;
			else
				animation.To = 7;

			switch (instance.Dock)
			{
				case Dock.Left:
				case Dock.Right:
					{
						instance.BeginAnimation(StripPane.WidthProperty, animation);
					} break;
				case Dock.Top:
				case Dock.Bottom:
					{
						instance.BeginAnimation(StripPane.HeightProperty, animation);
					} break;
			}
		}
		#endregion

		#region OnPinned_Changed()
		private static void OnPinned_Changed(object sender, DependencyPropertyChangedEventArgs args)
		{
			StripPane instance = sender as StripPane;

			if(instance.IsPinned)
			{
				instance.IsExpanded = true;
				instance.pinRotation.Angle = 0;
			}
			else
			{
				instance.IsExpanded = instance.IsMouseOver;
				instance.pinRotation.Angle = 90;
			}

		}
		#endregion

		#region OnClientRectSize_Changed()
		private static void OnClientRectSize_Changed(object sender, DependencyPropertyChangedEventArgs args)
		{
			StripPane instance = sender as StripPane;

			if (instance.IsExpanded)
			{
				if (instance.Dock == Dock.Top || instance.Dock == Dock.Bottom)
					instance.Height = instance.ClientRectSize + 7;
				else
					instance.Width = instance.ClientRectSize + 7;
			}
		}
		#endregion

		#region Pin_Click()
		private void Pin_Click(object sender, RoutedEventArgs e)
		{
			this.IsPinned = !this.IsPinned;
		}
		#endregion

		#region Form_PreviewMouseMove()
		private void Form_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			this.IsExpanded = true;
		}
		#endregion

		#region Form_MouseLeave()
		private void Form_MouseLeave(object sender, MouseEventArgs e)
		{
			this.IsExpanded = this.IsPinned;
		}
		#endregion

		#region Fence_PreviewMouseMove()
		private void Fence_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			dragStarted = true;
			//base.OnPreviewMouseDown(e);
			e.Handled = true;
		}
		#endregion

		#region Fence_PreviewMouseMove()
		private void Fence_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (this.dragStarted)
			{
				DataObject data = new DataObject(this);
				//Mouse.Capture(sender as UIElement);
				System.Windows.DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
				//Mouse.Capture(null);

				dragStarted = false;
				//base.OnPreviewMouseMove(e);
				e.Handled = true;
			}
		}
		#endregion

		#region Thumbnail_Checked()
		void Thumbnail_Checked(Thumbnail thumbnail)
		{
			if (thumbnail != null)
			{
				foreach (Thumbnail t in this.Thumbnails)
					if (t != thumbnail)
						t.IsSelected = false;

				VpImage vpImage = thumbnail.VpImage;

				if(ImageSelected != null)
					ImageSelected(vpImage);
			}
		}
		#endregion

		#region FixThumbnailsHeaders()
		public void FixThumbnailsHeaders()
		{
			for (int i = 0; i < this.stackPanelThumbnails.Children.Count; i++)
				((Thumbnail)this.stackPanelThumbnails.Children[i]).Header = string.Format("{0}", i + 1);
		}
		#endregion
	
		#region GetThumbnail()
		private Thumbnail GetThumbnail(VpImage vpImage)
		{
			foreach (Thumbnail thumbnail in this.stackPanelThumbnails.Children)
				if (thumbnail.VpImage == vpImage)
					return thumbnail;

			return null;
		}
		#endregion

		#region OnRender()
		protected override void OnRender(DrawingContext dc)
		{
			if (this.IsVisible)
				RedrawThumbnails();

			base.OnRender(dc);
		}
		#endregion

		#region ScrollViewer_ScrollChanged()
		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			RedrawThumbnails();
		}
		#endregion

		#region RedrawThumbnails()
		private void RedrawThumbnails()
		{
			if (this.IsVisible)
			{
				foreach (UIElement element in this.stackPanelThumbnails.Children)
					if (element is Thumbnail)
					{
						Thumbnail thumbnail = (Thumbnail)element;

						GeneralTransform childTransform = thumbnail.TransformToAncestor(this.scrollViewer);
						Rect rectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), thumbnail.RenderSize));

						//Check if the elements Rect intersects with that of the scrollviewer's
						Rect result = Rect.Intersect(new Rect(new Point(0, 0), this.scrollViewer.RenderSize), rectangle);

						//if result is Empty then the element is not in view
						thumbnail.IsVisibleInScrollViewer = (result != Rect.Empty);
					}
			}
		}
		#endregion
	
		#endregion
	}
}
