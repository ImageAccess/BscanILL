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
using System.Windows.Media.Animation;
using System.IO;
using ViewPane.Hierarchy;

namespace ViewPane.ItResults
{
	/// <summary>
	/// Interaction logic for StripPane.xaml
	/// </summary>
	internal partial class StripPane : UserControl, ViewPane.Thumbnails.IStripPane
	{
		bool allowTransforms = false;

		public static DependencyProperty IsExpandedProperty;
		public static DependencyProperty IsPinnedProperty;
		public static DependencyProperty ClientRectSizeProperty;

		bool dragStarted = false;

		public static RoutedEvent OrientationChangedEvent = EventManager.RegisterRoutedEvent("OrientationChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StripPane));

		public event ViewPane.ItResults.Thumbnail.ThumbnailSelectedHnd ThumbnailSelected;


		#region constructor
		public StripPane()
		{
			InitializeComponent();

			if (IsExpandedProperty == null)
			{
				IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(StripPane), 
					new PropertyMetadata(true, new PropertyChangedCallback(OnExpanded_Changed)));
				IsPinnedProperty = DependencyProperty.Register("IsPinned", typeof(bool), typeof(StripPane),
					new PropertyMetadata(true, new PropertyChangedCallback(OnPinned_Changed)));
				ClientRectSizeProperty = DependencyProperty.Register("ClientRectSize", typeof(int), typeof(StripPane),
					new PropertyMetadata(133, new PropertyChangedCallback(OnClientRectSize_Changed)));
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

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

		#endregion


		//INTERNAL PROPERTIES
		#region internal properties
		//internal ImageProcessing.IpSettings.ItImages	ItImages { get { return this.thumbnailsControl.ItImages; } }
		//internal List<ViewPane.ItResults.Thumbnail>			Thumbnails  { get { return this.thumbnailsControl.Thumbnails; } }
		//internal List<System.IO.FileInfo>	ImageFiles { get { return this.thumbnailsControl.ImageFiles; } }
		//internal ViewPane.ItResults.Thumbnail					SelectedThumbnail { get { return this.thumbnailsControl.SelectedThumbnail; } }
		//internal bool						AllowTramsforms { get { return this.thumbnailsControl.AllowTransforms; } set { this.thumbnailsControl.AllowTransforms = value; } }
	
		internal bool AllowTransforms { get { return allowTransforms; } set { this.allowTransforms = value; } }
		
		internal List<ViewPane.ItResults.Thumbnail> Thumbnails
		{
			get
			{
				List<ViewPane.ItResults.Thumbnail> thumbnails = new List<ViewPane.ItResults.Thumbnail>();

				foreach (ViewPane.ItResults.Thumbnail thumbnail in this.stackPanelThumbnails.Children)
					thumbnails.Add(thumbnail);

				return thumbnails;
			}
		}

		internal ImageProcessing.IpSettings.ItImages ItImages
		{
			get
			{
				ImageProcessing.IpSettings.ItImages itImages = new ImageProcessing.IpSettings.ItImages();

				foreach (ViewPane.ItResults.Thumbnail thumbnail in this.stackPanelThumbnails.Children)
					itImages.Add(thumbnail.ItImage);

				return itImages;
			}
		}

		internal List<FileInfo> ImageFiles
		{
			get
			{
				List<FileInfo> files = new List<FileInfo>();

				foreach (ViewPane.ItResults.Thumbnail thumbnail in this.stackPanelThumbnails.Children)
					files.Add(new FileInfo(thumbnail.VpImage.FullPath));

				return files;
			}
		}

		internal ViewPane.ItResults.Thumbnail SelectedThumbnail
		{
			get
			{
				foreach (ViewPane.ItResults.Thumbnail thumbnail in this.stackPanelThumbnails.Children)
					if (thumbnail.IsChecked.Value)
						return thumbnail;

				return null;
			}
		}
		
		#endregion


		//PUBLIC METHODS
		#region public methods

		#region AddThumbnail()
		public void AddThumbnail(ViewPane.ItResults.Thumbnail thumbnail)
		{
			//thumbnail.Checked += new RoutedEventHandler(Thumbnail_Checked);
			thumbnail.Selected += new Thumbnail.ThumbnailSelectedHnd(Thumbnail_Selected);

			int index = this.stackPanelThumbnails.Children.Add(thumbnail);

			thumbnail.BringIntoView();
			thumbnail.Header = string.Format("{0}", index + 1);
		}
		#endregion

		#region InsertThumbnailBefore()
		public void InsertThumbnailBefore(VpImage currentImage, ViewPane.ItResults.Thumbnail thumbnail)
		{
			ViewPane.ItResults.Thumbnail currentThumbnail = GetThumbnail(currentImage);

			if (currentThumbnail != null)
				InsertThumbnail(this.stackPanelThumbnails.Children.IndexOf(currentThumbnail), thumbnail);
			else
				AddThumbnail(thumbnail);
		}
		#endregion

		#region InsertThumbnailAfter()
		public void InsertThumbnailAfter(VpImage currentImage, ViewPane.ItResults.Thumbnail thumbnail)
		{
			ViewPane.ItResults.Thumbnail currentThumbnail = GetThumbnail(currentImage);

			if (currentThumbnail != null)
				InsertThumbnail(this.stackPanelThumbnails.Children.IndexOf(currentThumbnail) + 1, thumbnail);
			else
				AddThumbnail(thumbnail);
		}

		#endregion

		#region InsertThumbnail()
		public void InsertThumbnail(int index, ViewPane.ItResults.Thumbnail thumbnail)
		{
			//thumbnail.Checked += new RoutedEventHandler(Thumbnail_Checked);
			thumbnail.Selected += new Thumbnail.ThumbnailSelectedHnd(Thumbnail_Selected);

			if (index >= 0 && index < this.stackPanelThumbnails.Children.Count)
				this.stackPanelThumbnails.Children.Insert(index, thumbnail);
			else
				index = this.stackPanelThumbnails.Children.Add(thumbnail);

			thumbnail.BringIntoView();
			thumbnail.Header = string.Format("{0}", index + 1);
		}
		#endregion

		#region RemoveThumbnail()
		/// <summary>
		/// Removes the thumbnail and returns a thumbnail that should be selected.
		/// </summary>
		/// <param name="scanData"></param>
		/// <returns></returns>
		public ViewPane.ItResults.Thumbnail RemoveThumbnail(ViewPane.ItResults.Thumbnail thumbnail)
		{
			ViewPane.ItResults.Thumbnail thumbToSelect = null;

			if (thumbnail != null)
			{
				int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

				if (index >= 0 && index < this.stackPanelThumbnails.Children.Count - 1)
					thumbToSelect = (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[index + 1];
				else if (index > 0)
					thumbToSelect = (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[index - 1];

				//thumbnail.Checked -= new RoutedEventHandler(Thumbnail_Checked);
				thumbnail.Selected -= new Thumbnail.ThumbnailSelectedHnd(Thumbnail_Selected);
				thumbnail.Dispose();
				this.stackPanelThumbnails.Children.Remove(thumbnail);

				for (int i = 0; i < this.stackPanelThumbnails.Children.Count; i++)
					((ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[i]).Header = string.Format("{0}", i + 1);
			}

			return thumbToSelect;
		}
		#endregion

		#region Clear()
		public void Clear()
		{
			foreach (ViewPane.ItResults.Thumbnail thumbnail in this.stackPanelThumbnails.Children)
			{
				//thumbnail.Checked -= new RoutedEventHandler(Thumbnail_Checked);
				thumbnail.Selected -= new Thumbnail.ThumbnailSelectedHnd(Thumbnail_Selected);
				thumbnail.Dispose();
			}

			this.stackPanelThumbnails.Children.Clear();
		}
		#endregion

		#region UncheckAll()
		public void UncheckAll()
		{
			foreach (ViewPane.ItResults.Thumbnail thumbnail in this.stackPanelThumbnails.Children)
				thumbnail.IsChecked = false;
		}
		#endregion

		#region SelectThumbnail()
		public void SelectThumbnail(ViewPane.ItResults.Thumbnail thumbnail)
		{
			if (thumbnail != null)
			{
				thumbnail.Select();
			}
		}
		#endregion

		#region SelectFirstThumbnail()
		public void SelectFirstThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				ViewPane.ItResults.Thumbnail thumbnail = (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[0];

				thumbnail.Select();
			}
		}
		#endregion

		#region SelectPreviousThumbnail()
		public void SelectPreviousThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				ViewPane.ItResults.Thumbnail thumbnail = SelectedThumbnail;

				if (thumbnail != null)
				{
					int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

					if (index > 0)
					{
						ViewPane.ItResults.Thumbnail t = (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[index - 1];

						t.Select();
					}
				}
			}
		}
		#endregion

		#region SelectNextThumbnail()
		public void SelectNextThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				ViewPane.ItResults.Thumbnail thumbnail = SelectedThumbnail;

				if (thumbnail != null)
				{
					int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

					if (index < this.stackPanelThumbnails.Children.Count - 1)
					{
						ViewPane.ItResults.Thumbnail t = (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[index + 1];

						t.Select();
					}
				}
			}
		}
		#endregion

		#region SelectLastThumbnail()
		public void SelectLastThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				ViewPane.ItResults.Thumbnail thumbnail = (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[this.stackPanelThumbnails.Children.Count - 1];

				thumbnail.Select();
			}
		}
		#endregion

		#region GetFirstThumbnail()
		public ViewPane.ItResults.Thumbnail GetFirstThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
				return (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[0];
			else
				return null;
		}
		#endregion

		#region GetPreviousThumbnail()
		public ViewPane.ItResults.Thumbnail GetPreviousThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				ViewPane.ItResults.Thumbnail thumbnail = SelectedThumbnail;

				if (thumbnail != null)
				{
					int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

					if (index > 0)
					{
						return (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[index - 1];
					}
				}
			}

			return null;
		}
		#endregion

		#region GetNextThumbnail()
		public ViewPane.ItResults.Thumbnail GetNextThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
			{
				ViewPane.ItResults.Thumbnail thumbnail = SelectedThumbnail;

				if (thumbnail != null)
				{
					int index = this.stackPanelThumbnails.Children.IndexOf(thumbnail);

					if (index < this.stackPanelThumbnails.Children.Count - 1)
					{
						return (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[index + 1];
					}
				}
			}

			return null;
		}
		#endregion

		#region GetLastThumbnail()
		public ViewPane.ItResults.Thumbnail GetLastThumbnail()
		{
			if (this.stackPanelThumbnails.Children.Count > 0)
				return (ViewPane.ItResults.Thumbnail)this.stackPanelThumbnails.Children[this.stackPanelThumbnails.Children.Count - 1];
			else
				return null;
		}
		#endregion

		#region GetThumbnail()
		public ViewPane.ItResults.Thumbnail GetThumbnail(VpImage vpImage)
		{
			foreach (ViewPane.ItResults.Thumbnail thumbnail in this.stackPanelThumbnails.Children)
				if (thumbnail.VpImage == vpImage)
					return thumbnail;

			return null;
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region OnExpanded_Changed()
		private void OnExpanded_Changed(object sender, DependencyPropertyChangedEventArgs args)
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
		private void OnPinned_Changed(object sender, DependencyPropertyChangedEventArgs args)
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
		private void OnClientRectSize_Changed(object sender, DependencyPropertyChangedEventArgs args)
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

		#region Thumbnail_Selected()
		void Thumbnail_Selected(Thumbnail thumbnail)
		{
			if (ThumbnailSelected != null)
				ThumbnailSelected(thumbnail);
		}
		#endregion

		#endregion

	}
}
