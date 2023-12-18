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



namespace ViewPane.ImageTreatmentUi
{
	/// <summary>
	/// Interaction logic for ItUi.xaml
	/// </summary>
	internal partial class ImageUi : UserControl
	{
		ImagePane	imagePane = null;
		Image		imageBox;
		bool		skewVisible = true;

		ImageProcessing.IpSettings.ItImage		itImage = null;


		#region ImageUi()
		public ImageUi()
		{
			InitializeComponent();
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties
		
		internal ImagePane	ImagePane { get { return this.imagePane; } }
		internal IViewImage	IImage { get { return imagePane.IImage; } }
		internal bool		TwoPages { get { return (imagePane.IImage != null) ? imagePane.IImage.TwoPages : false; } }
		internal bool		SkewVisible { get { return this.skewVisible; } set { this.skewVisible = value; } }

		#region CurrentWidth
		/// <summary>
		/// if this.Width != NaN, returns this.Width, this.ActualWidth otherwise
		/// </summary>
		internal double CurrentWidth { get { return double.IsNaN(this.Width) ? this.ActualWidth : this.Width; } }
		#endregion

		#region CurrentHeight
		/// <summary>
		/// if this.Height != NaN, returns this.Height, this.ActualHeight otherwise
		/// </summary>
		internal double CurrentHeight { get { return double.IsNaN(this.Height) ? this.ActualHeight : this.Height; } }
		#endregion
	
		#endregion

		//PRIVATE PROPERTIES
		#region private properties

		private ViewPane.ToolbarSelection ToolbarSelection { get { return this.imagePane.ToolbarSelection; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Init()
		public void Init(ImagePane imagePane)
		{
			this.imagePane = imagePane;

			this.imageBox = this.imagePane.imageBox;

			this.pageUiL.Init(this);
			this.pageUiR.Init(this);
			this.opticsUi.Init(this);
		}
		#endregion

		#region Synchronize()
		internal void Synchronize(ImageProcessing.IpSettings.ItImage itImage)
		{
			if (this.itImage != itImage)
			{
				if (this.itImage != null)
				{
					this.itImage.Changed -= new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);
				}

				this.itImage = itImage;

				if (this.itImage != null)
				{
					this.itImage.Changed += new ImageProcessing.IpSettings.ItPropertiesChangedHnd(ItImage_Changed);
				}

				if (this.imagePane.AllowTransforms && itImage != null && itImage.IsFixed == false)
				{
					if (this.itImage.TwoPages)
					{
						ImageProcessing.IpSettings.ItPage pageL = this.itImage.PageL;
						ImageProcessing.IpSettings.ItPage pageR = this.itImage.PageR;

						if (GetPageUi(pageL) == null)
							pageUiL.Synchronize(pageL);
						if (GetPageUi(pageR) == null)
							pageUiR.Synchronize(pageR);
					}
					else
					{
						ImageProcessing.IpSettings.ItPage page = this.itImage.PageL;

						if (GetPageUi(page) == null)
							pageUiL.Synchronize(page);

						pageUiR.Synchronize(null);
					}

					opticsUi.Synchronize(this.itImage);
				}
				else
				{
					pageUiL.Synchronize(null);
					pageUiR.Synchronize(null);
					opticsUi.Synchronize(null);
				}
			}

			RefreshUi();
		}
		#endregion

		#region GetMousePoint()
		/*internal Point GetMousePoint(MouseEventArgs e)
		{
			return e.GetPosition(this.imageBox);
		}*/
		#endregion

		#region ToSystemDrawingPoint()
		/*internal static System.Drawing.Point ToSystemDrawingPoint(Point p)
		{
			return new System.Drawing.Point(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
		}*/
		#endregion

		#region KeyDown_Occured()
		public void KeyDown_Occured(KeyEventArgs e)
		{
			this.pageUiL.KeyDown_Occured(e);
			this.pageUiR.KeyDown_Occured(e);
		}
		#endregion

		#region KeyUp_Occured()
		public void KeyUp_Occured(KeyEventArgs e)
		{
			this.pageUiL.KeyUp_Occured(e);
			this.pageUiR.KeyUp_Occured(e);
		}
		#endregion

		#region PanelToImage()
		internal RatioPoint PanelToImage(MouseEventArgs e)
		{
			return this.imagePane.PanelToImage(e);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region GetPageUi()
		private PageUi GetPageUi(ImageProcessing.IpSettings.ItPage page)
		{
			if (IImage.TwoPages)
			{
				if (pageUiL.Page == page)
					return pageUiL;
				if (pageUiR.Page == page)
					return pageUiR;
			}
			else
			{
				if (pageUiL.Page == page)
					return pageUiL;
			}

			return null;
		}
		#endregion

		#region LeftMouse_Down()
		private void LeftMouse_Down(object sender, MouseButtonEventArgs e)
		{
			if (itImage != null && itImage.IsFixed == false)
			{
				if (this.imagePane.ToolbarSelection == ViewPane.ToolbarSelection.Pages)
				{
					if (itImage.TwoPages == false)
					{
						RatioPoint p = this.imagePane.PanelToImage(e);

						if (p.X >= 0 && p.X <= 1 && p.Y >= 0 && p.Y <= 1)
						{
							ImageProcessing.IpSettings.ItPage pageL = this.itImage.PageL;
							ImageProcessing.IpSettings.ItPage pageR = this.itImage.PageR;

							if (pageL.ClipSpecified)
							{
								if (p.X >= pageL.ClipRect.X)
								{
									if (itImage.IsIndependent)
									{
										itImage.PageR.Lock();
										itImage.PageR.Activate(new RatioRect(p.X, p.Y, 0, 0), true);

										pageUiR.Synchronize(pageR);

										this.visibleGrid.Visibility = Visibility.Visible;
										this.pageUiR.DragStarted(p);
									}
									else
									{
										double x = Math.Max(0, Math.Min(p.X, 1 - pageL.ClipRect.Width));
										double y = Math.Max(0, Math.Min(p.Y, 1 - pageL.ClipRect.Height));
										
										itImage.PageR.Activate(new RatioRect(x, y, pageL.ClipRect.Width, pageL.ClipRect.Height), true);

										pageUiR.Synchronize(pageR);
										RefreshUi();

										this.visibleGrid.Visibility = Visibility.Visible;
									}
								}
								else
								{
									if (itImage.IsIndependent)
									{
										itImage.PageL.Lock();
										//itImage.SetTo2Clips();
										itImage.PageR.ImportSettings(pageL);
										itImage.PageL.Reset(new RatioRect(p.X, p.Y, 0, 0));

										pageUiL.Synchronize(itImage.PageL);
										pageUiR.Synchronize(itImage.PageR);
										RefreshUi();

										this.visibleGrid.Visibility = Visibility.Visible;
										this.pageUiL.DragStarted(p);
									}
									else
									{
										double x = Math.Max(0, Math.Min(p.X, 1 - pageL.ClipRect.Width));
										double y = Math.Max(0, Math.Min(p.Y, 1 - pageL.ClipRect.Height));

										//itImage.SetTo2Clips();
										itImage.PageR.ImportSettings(pageL);
										itImage.PageL.Reset(new RatioRect(x, y, pageL.ClipRect.Width, pageL.ClipRect.Height));

										pageUiL.Synchronize(itImage.PageL);
										pageUiR.Synchronize(itImage.PageR);
										RefreshUi();

										this.visibleGrid.Visibility = Visibility.Visible;
									}
								}
							}
							else
							{
								if (itImage.IsIndependent)
								{
									itImage.PageL.Lock();
									itImage.PageL.SetClip(new RatioRect(p.X, p.Y, 0, 0), true);

									pageUiL.Synchronize(itImage.PageL);

									this.visibleGrid.Visibility = Visibility.Visible;
									this.pageUiL.DragStarted(p);
								}
								else
								{
									InchSize? size = null;

									if (this.imagePane.ItImages != null)
										size = this.imagePane.ItImages.GetDependantClipsSize();

									if (size.HasValue)
									{
										RatioRect clip = new RatioRect(p.X, p.Y, size.Value.Width / itImage.InchSize.Width, size.Value.Height / itImage.InchSize.Height);
										itImage.PageL.SetClip(clip, true);
										pageUiL.Synchronize(itImage.PageL);

										this.visibleGrid.Visibility = Visibility.Visible;
									}
									else
									{
										itImage.PageL.Lock();
										itImage.PageL.SetClip(new RatioRect(p.X, p.Y, 0, 0), true);

										pageUiL.Synchronize(itImage.PageL);

										this.visibleGrid.Visibility = Visibility.Visible;
										this.pageUiL.DragStarted(p);
									}
								}
							}

							imagePane.AdjustToolbar();
							imagePane.InvalidateVisual();
						}
					}

					e.Handled = true;
				}
			}
		}
		#endregion

		#region RemoveLeftPage_Request()
		void RemoveLeftPage_Request(object sender, EventArgs e)
		{
			if (this.pageUiL.Exists && IImage != null)
			{
				if (IImage.TwoPages)
					IImage.ItImage.RemovePage(this.pageUiL.Page);
				else
					IImage.ItImage.ResetClip(this.pageUiL.Page);

				pageUiL.Synchronize(IImage.ItImage.PageL);
				pageUiR.Synchronize(IImage.ItImage.PageR);
				
				imagePane.AdjustToolbar();
				imagePane.InvalidateVisual();
			}
		}
		#endregion

		#region RemoveRightPage_Request()
		void RemoveRightPage_Request(object sender, EventArgs e)
		{
			if (this.pageUiR.Exists && IImage != null)
			{
				if (IImage.TwoPages)
					IImage.ItImage.RemovePage(this.pageUiR.Page);
				else
					IImage.ItImage.ResetClip(this.pageUiL.Page);

				pageUiL.Synchronize(IImage.ItImage.PageL);
				pageUiR.Synchronize(IImage.ItImage.PageR);
				
				imagePane.AdjustToolbar();
				imagePane.InvalidateVisual();
			}
		}
		#endregion

		#region RefreshUi()
		internal void RefreshUi()
		{
			if (this.imagePane.AllowTransforms && itImage != null && itImage.IsFixed == false && this.imagePane.IImage != null)
			{
				System.Drawing.Size fullImageSize = this.imagePane.IImage.FullImageInfo.Size;
				
				this.Visibility = Visibility.Visible;
				this.Margin = new Thickness(-this.imagePane.ImageRect.X, -this.imagePane.ImageRect.Y, 0, 0);
				this.Width = fullImageSize.Width * this.imagePane.Zoom;
				this.Height = fullImageSize.Height * this.imagePane.Zoom;

				if (itImage.TwoPages)
				{
					if (pageUiL.Page != null && pageUiR.Page != null)
					{		
						GeometryGroup gg = new GeometryGroup();
						Geometry geometry = new RectangleGeometry(new Rect(0, 0, this.Width, this.Height));
						Geometry geometryL = new RectangleGeometry(new Rect(itImage.PageL.ClipRect.X * this.Width, itImage.PageL.ClipRect.Y * this.Height,
							itImage.PageL.ClipRect.Width * this.Width, itImage.PageL.ClipRect.Height * this.Height));
						Geometry geometryR = new RectangleGeometry(new Rect(itImage.PageR.ClipRect.X * this.Width, itImage.PageR.ClipRect.Y * this.Height,
							itImage.PageR.ClipRect.Width * this.Width, itImage.PageR.ClipRect.Height * this.Height));

						geometryL.Transform = new RotateTransform(pageUiL.Page.Skew * 180 / Math.PI, geometryL.Bounds.Left + geometryL.Bounds.Width / 2, geometryL.Bounds.Top + geometryL.Bounds.Height / 2);
						geometryR.Transform = new RotateTransform(pageUiR.Page.Skew * 180 / Math.PI, geometryR.Bounds.Left + geometryR.Bounds.Width / 2, geometryR.Bounds.Top + geometryR.Bounds.Height / 2);

						gg.Children.Add(geometry);
						gg.Children.Add(geometryL);
						gg.Children.Add(geometryR);
						this.path.Data = gg;
						this.visibleGrid.Visibility = Visibility.Visible;


						pageUiL.Synchronize(itImage.PageL);						
						pageUiR.Synchronize(itImage.PageR);
					}
				}
				else
				{
					if (pageUiL.Page != null)
					{
						if (itImage.PageL.ClipSpecified)
						{
							GeometryGroup gg = new GeometryGroup();
							Geometry geometry = new RectangleGeometry(new Rect(0, 0, this.Width, this.Height));
							Geometry geometryL = new RectangleGeometry(new Rect(itImage.PageL.ClipRect.X * this.Width, itImage.PageL.ClipRect.Y * this.Height,
								itImage.PageL.ClipRect.Width * this.Width, itImage.PageL.ClipRect.Height * this.Height));

							geometryL.Transform = new RotateTransform(pageUiL.Page.Skew * 180 / Math.PI, geometryL.Bounds.Left + geometryL.Bounds.Width / 2, geometryL.Bounds.Top + geometryL.Bounds.Height / 2);

							gg.Children.Add(geometry);
							gg.Children.Add(geometryL);
							this.path.Data = gg;
							this.visibleGrid.Visibility = Visibility.Visible;

							pageUiL.Synchronize(itImage.PageL);
						}
						else
							this.visibleGrid.Visibility = Visibility.Hidden;
					}
				}

				opticsUi.Synchronize(this.itImage);

				if (this.ToolbarSelection == ViewPane.ToolbarSelection.Pages)
					mainGrid.Cursor = Cursors.Cross;
				else
					mainGrid.Cursor = null;
			}
			else
			{
				this.Visibility = Visibility.Hidden;
			}
		}
		#endregion

		#region Mouse_Move()
		private void Mouse_Move(object sender, MouseEventArgs e)
		{
			if (itImage != null && itImage.IsFixed == false)
			{
				if (this.imagePane.ToolbarSelection == ViewPane.ToolbarSelection.Pages)
				{
					if (itImage.TwoPages)
					{
						if (this.pageUiL != null && this.pageUiR != null)
						{
							UIElement uiElement = null;

							if ((uiElement = this.pageUiL.IsMouseOverLeadingPoints(e)) != null)
							{
								this.pageUiR.SetValue(Panel.ZIndexProperty, 0);
								this.pageUiL.SetValue(Panel.ZIndexProperty, 1);
							}
							else if ((uiElement = this.pageUiR.IsMouseOverLeadingPoints(e)) != null)
							{
								this.pageUiL.SetValue(Panel.ZIndexProperty, 0);
								this.pageUiR.SetValue(Panel.ZIndexProperty, 1);
							}
						}
					}
				}
				else
					mainGrid.Cursor = null;
			}
			else
				mainGrid.Cursor = null;
		}
		#endregion

		#region ItImage_Changed()
		void ItImage_Changed(ImageProcessing.IpSettings.ItImage itImage, ImageProcessing.IpSettings.ItProperty type)
		{
			if (this.Dispatcher.CheckAccess())
				ItImage_ChangedTU(itImage, type);
			else
				this.Dispatcher.Invoke((Action)delegate() { ItImage_ChangedTU(itImage, type); });
		}
		#endregion

		#region ItImage_ChangedTU()
		void ItImage_ChangedTU(ImageProcessing.IpSettings.ItImage itImage, ImageProcessing.IpSettings.ItProperty type)
		{
			try
			{
				if ((type & ImageProcessing.IpSettings.ItProperty.Clip) > 0)
				{
					if (this.itImage != null && this.itImage.PageR.IsActive && this.pageUiR.Page == null)
						this.pageUiR.Synchronize(this.itImage.PageR);

					RefreshUi();
				}

				if ((type & ImageProcessing.IpSettings.ItProperty.ImageSettings) > 0)
				{
					if (this.itImage == itImage)
						Synchronize(itImage);
				}
			}
#if DEBUG
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show("ImageUi, ItImage_ChangedTU() error: " + ex.Message);
			}
#else
			catch { }
#endif
		}
		#endregion

		#endregion
	}
}
