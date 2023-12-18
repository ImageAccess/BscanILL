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
using ImageProcessing;
using BIP.Geometry;
using ViewPane.ImagePanel;


namespace ViewPane.ImageTreatmentUi
{
	/// <summary>
	/// Interaction logic for PageUi.xaml
	/// </summary>
	internal partial class PageUi : UserControl
	{
		ImageUi									imageUi;
		ImageProcessing.IpSettings.ItPage		itPage = null;
		object									locker = new object();

		RatioPoint? startPoint = null;
		UIElement	selectedControl = null;
		double		skewOriginalY = 0;

		List<ImageProcessing.IpSettings.ItPage> affectedPages = new List<ImageProcessing.IpSettings.ItPage>();


		#region constructor
		public PageUi()
		{
			InitializeComponent();
		}
		#endregion


		#region enum MouseOperation
		internal enum MouseOperation
		{
			None = 0,
			Move = 1,
			Resize = 2,
			Skew = 3
		}
		#endregion


		//PUBLIC PROPERTIES
		#region internal properties

		internal ImageUi							ImageUi { get { return this.imageUi; } }
		internal ImageProcessing.IpSettings.ItPage	Page { get { return this.itPage; } }
		internal bool								Exists { get { return this.itPage != null; } }
		internal ImagePane							ImagePane { get { return this.imageUi.ImagePane; } }

		internal bool SkewVisible { get { return this.imageUi.SkewVisible; } set { this.gridSkew.Visibility = (value) ? Visibility.Visible : Visibility.Hidden; } }

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
		private ViewPane.ToolbarSelection ToolbarSelection { get { return this.ImagePane.ToolbarSelection; } }
		#endregion


		//PUBLIC METHODS
		#region internal methods

		#region Init()
		public void Init(ImageUi imageUi)
		{
			this.imageUi = imageUi;
			this.imageUi.ImagePane.ZoomModeChanged += new ImagePane.ZoomModeChangedHnd(ImagePane_ZoomModeChanged);
			//this.imageUi.ImagePane.ZoomChanged += new ImagePane.ZoomChangedHnd(ImagePane_ZoomChanged);

			this.fingersUi.Init(this);
			this.bookfoldUi.Init(this);
		}
		#endregion

		#region Synchronize()
		internal void Synchronize(ImageProcessing.IpSettings.ItPage itPage)
		{	
			if (this.itPage != itPage)
			{
				if (this.itPage != null)
					this.itPage.Changed -= new ImageProcessing.IpSettings.ItPage.PageChangedHnd(Page_Changed);

				this.itPage = itPage;

				if (this.itPage != null)
					this.itPage.Changed += new ImageProcessing.IpSettings.ItPage.PageChangedHnd(Page_Changed);
			}

			this.fingersUi.Synchronize(itPage);	
			this.bookfoldUi.Synchronize();
			
			RefreshUi();
		}
		#endregion

		#region DragStarted()
		internal void DragStarted(RatioPoint imagePoint)
		{
			if (this.itPage != null)
			{
				this.Visibility = Visibility.Visible;
				this.selectedGrid.Visibility = Visibility.Visible;

				SelectAffectedPages(MouseOperation.Resize);
				this.itPage.Lock();
				this.itPage.SetClip(new RatioRect(imagePoint.X, imagePoint.Y, 0, 0), false);
				foreach (ImageProcessing.IpSettings.ItPage pg in affectedPages)
					pg.SetClip(itPage.ClipRect, true);

				this.startPoint = imagePoint;
				//Console.WriteLine("DragStarted: Start Point set to " + startPoint.Value.X.ToString());
				this.selectedControl = this.pSE;
				this.pSE.CaptureMouse();
			}
		}
		#endregion

		#region PanelToImage()
		internal RatioPoint PanelToImage(MouseEventArgs e)
		{
			return imageUi.ImagePane.PanelToImage(e);
		}
		#endregion

		#region PanelToPage()
		internal RatioPoint PanelToPage(MouseEventArgs e)
		{
			RatioPoint imagePoint = imageUi.ImagePane.PanelToImage(e);

			return itPage.GetPagePoint(imagePoint);
		}
		#endregion

		#region KeyDown_Occured()
		public void KeyDown_Occured(KeyEventArgs e)
		{
			SetToolTips();
		}
		#endregion

		#region KeyUp_Occured()
		public void KeyUp_Occured(KeyEventArgs e)
		{
			SetToolTips();
		}
		#endregion

		#region PanelToPage()
		internal RatioPoint PanelToPage(Point p)
		{
			return new RatioPoint(p.X / this.ImagePane.Zoom, p.Y / this.ImagePane.Zoom);
		}
		#endregion

		#region IsMouseOverLeadingPoints()
		internal UIElement IsMouseOverLeadingPoints(MouseEventArgs e)
		{
			foreach (UIElement uiElement in selectedGrid.Children)
			{
				if (uiElement is Button)
				{
					Button	b = uiElement as Button;
					Point	p = e.GetPosition(b);

					if (p.X >= 0 && p.Y >= 0 && p.X <= b.ActualWidth && p.Y <= b.ActualHeight)
						return b;
				}
			}

			foreach (UIElement uiElement in gridSkew.Children)
			{
				if (uiElement is Button)
				{
					Button	b = uiElement as Button;
					Point	p = e.GetPosition(b);

					if (p.X >= 0 && p.Y >= 0 && p.X <= b.ActualWidth && p.Y <= b.ActualHeight)
						return b;
				}
			}
	
			return null;
		}
		#endregion

		#region MouseOverLeadingPoint()
		public void MouseOverLeadingPoint(UIElement sender, MouseButtonEventArgs e)
		{
			if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.Pages && sender != null)
			{
				if(sender != null)

				e.Handled = true;
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region ImagePane_ZoomModeChanged()
		void ImagePane_ZoomModeChanged(ViewPane.ToolbarSelection oldZoomMode, ViewPane.ToolbarSelection newZoomMode)
		{
			RefreshUi();
		}
		#endregion

		#region MoveRegion()
		private bool MoveRegion(RatioPoint imagePoint, RatioPoint startPoint, ImageProcessing.IpSettings.Clip clip, ref double dl, ref double dt, ref double dr, ref double db)
		{
			RatioRect origRect = clip.RectangleNotSkewed;
			RatioRect newRect = clip.RectangleNotSkewed;
			double dx = imagePoint.X - startPoint.X;
			double dy = imagePoint.Y - startPoint.Y;

			if (dx != 0 || dy != 0)
			{
				newRect.X = (newRect.X + dx > 0) ? newRect.X + dx : 0;
				newRect.Y = (newRect.Y + dy > 0) ? newRect.Y + dy : 0;

				if (newRect.Right > 1)
					newRect.X = 1 - newRect.Width;
				if (newRect.Bottom > 1)
					newRect.Y = 1 - newRect.Height;
			}

			if (origRect != newRect)
			{
				dl = newRect.X - origRect.X;
				dt = newRect.Y - origRect.Y;
				dr = newRect.Right - origRect.Right;
				db = newRect.Bottom - origRect.Bottom;

				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion

		#region ResizeRegion()
		private bool ResizeRegion(ref object sender, RatioPoint imagePoint, RatioPoint startPoint,
			ImageProcessing.IpSettings.Clip clip, ref double dl, ref double dt, ref double dr, ref double db)
		{
			RatioRect rect = clip.RectangleNotSkewed;
			double dx = imagePoint.X - startPoint.X;
			double dy = imagePoint.Y - startPoint.Y;

			if (dx != 0 || dy != 0)
			{
				if (sender == this.pW)
				{
					if (dx != 0)
					{
						if (imagePoint.X <= rect.Right)
						{
							dl = dx;
						}
						else
						{
							dl = rect.Right - rect.X;
							dr = rect.X + dx - rect.Right;
							sender = this.pE;
						}
					}
				}
				else if (sender == this.pN)
				{
					if (dy != 0)
					{
						if (imagePoint.Y <= rect.Bottom)
						{
							dt = dy;
						}
						else
						{
							dt = rect.Bottom - rect.Y;
							db = rect.Y + dy - rect.Bottom;
							sender = this.pS;
						}
					}
				}
				else if (sender == this.pE)
				{
					if (dx != 0)
					{
						if (imagePoint.X >= rect.Left)
							dr = dx;
						else
						{
							dl = imagePoint.X - rect.X;
							dr = rect.X - rect.Right;
							sender = this.pW;
						}
					}
				}
				else if (sender == this.pS)
				{
					if (dy != 0)
					{
						if (imagePoint.Y >= rect.Top)
							db = dy;
						else
						{
							dt = imagePoint.Y - rect.Y;
							db = rect.Y - rect.Bottom;
							sender = this.pN;
						}
					}
				}
				else if (sender == this.pNW)
				{
					if ((imagePoint.X <= rect.Right) && (imagePoint.Y <= rect.Bottom))
					{
						dl = dx;
						dt = dy;
					}
					else if (imagePoint.X <= rect.Right)
					{
						dl = dx;
						dt = rect.Bottom - rect.Y;
						db = rect.Y + dy - rect.Bottom;
						sender = this.pSW;
					}
					else if (imagePoint.Y <= rect.Bottom)
					{
						dl = rect.Right - rect.X;
						dr = rect.X + dx - rect.Right;
						dt = dy;
						sender = this.pNE;
					}
					else 
					{
						dl = rect.Right - rect.X;
						dr = rect.X + dx - rect.Right;
						dt = rect.Bottom - rect.Y;
						db = rect.Y + dy - rect.Bottom;
						sender = this.pSE;
					}
				}
				else if (sender == this.pNE)
				{
					if ((imagePoint.X >= rect.Left) && (imagePoint.Y <= rect.Bottom))
					{
						dr = dx;
						dt = dy;
					}
					else if (imagePoint.X >= rect.Left)
					{
						dr = dx;
						dt = rect.Bottom - rect.Y;
						db = rect.Y + dy - rect.Bottom;
						sender = this.pSE;
					}
					else if (imagePoint.Y <= rect.Bottom)
					{
						dl = imagePoint.X - rect.X;
						dr = rect.X - rect.Right;
						dt = dy;
						sender = this.pNW;
					}
					else
					{
						dl = imagePoint.X - rect.X;
						dr = rect.X - rect.Right;
						dt = rect.Bottom - rect.Y;
						db = rect.Y + dy - rect.Bottom;
						sender = this.pSW;
					}
				}
				else if (sender == this.pSW)
				{
					if ((imagePoint.X <= rect.Right) && (imagePoint.Y >= rect.Top))
					{
						dl = dx;
						db = dy;
					}
					else if (imagePoint.X <= rect.Right)
					{
						dl = dx;
						dt = imagePoint.Y - rect.Y;
						db = rect.Y - rect.Bottom;
						sender = this.pNW;
					}
					else if (imagePoint.Y >= rect.Top)
					{
						dl = rect.Right - rect.X;
						dr = rect.X + dx - rect.Right;
						db = dy;
						sender = this.pSE;
					}
					else
					{
						dl = rect.Right - rect.X;
						dr = rect.X + dx - rect.Right;
						dt = imagePoint.Y - rect.Y;
						db = rect.Y - rect.Bottom;
						sender = this.pNE;
					}
				}
				else if (sender == this.pSE)
				{
					if ((imagePoint.X >= rect.Left) && (imagePoint.Y >= rect.Top))
					{
						dr = dx;
						db = dy;
					}
					else if (imagePoint.X >= rect.Left)
					{
						dr = dx;
						dt = imagePoint.Y - rect.Y;
						db = rect.Y - rect.Bottom;
						sender = this.pNE;
					}
					else if (imagePoint.Y >= rect.Top)
					{
						dl = imagePoint.X - rect.X;
						dr = rect.X - rect.Right;
						db = dy;
						sender = this.pSW;
					}
					else
					{
						dl = imagePoint.X - rect.X;
						dr = rect.X - rect.Right;
						dt = imagePoint.Y - rect.Y;
						db = rect.Y - rect.Bottom;
						sender = this.pNW;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion

		#region MoveRegion()
		/*private bool MoveRegion(ref object sender, RatioPoint imagePoint, RatioPoint startPoint,
			ImageProcessing.IpSettings.Clip clip, out double dl, out double dt, out double dr, out double db)
		{
			RatioRect origRect = clip.RectangleNotSkewed;
			RatioRect newRect = clip.RectangleNotSkewed;
			double dx = imagePoint.X - startPoint.X;
			double dy = imagePoint.Y - startPoint.Y;

			if (dx != 0 || dy != 0)
			{
				if (sender == this.contentRect)
				{
					newRect.X += dx;
					newRect.Y += dy;

					newRect.X = (newRect.X > 0) ? newRect.X : 0;
					newRect.Y = (newRect.Y > 0) ? newRect.Y : 0;

					if (newRect.Right > 1)
						newRect.X = 1 - newRect.Width;
					if (newRect.Bottom > 1)
						newRect.Y = 1 - newRect.Height;
				}
				else if (sender == this.pW)
				{
					if (dx != 0)
					{
						if (newRect.Width > dx)
						{
							newRect.X = imagePoint.X;
							newRect.Width -= dx;
						}
						else
						{
							newRect.X = newRect.X + newRect.Width;
							newRect.Width = dx - newRect.Width;
							sender = this.pE;
						}
					}
				}
				else if (sender == this.pN)
				{
					if (dy != 0)
					{
						if (newRect.Height > dy)
						{
							newRect.Y = imagePoint.Y;
							newRect.Height -= dy;
						}
						else
						{
							newRect.Y = newRect.Y + newRect.Height;
							newRect.Height = dy - newRect.Height;
							sender = this.pS;
						}
					}
				}
				else if (sender == this.pE)
				{
					if (dx != 0)
					{
						if (newRect.Width + dx > 0)
							newRect.Width += dx;
						else
						{
							newRect.Width = newRect.X - imagePoint.X;
							newRect.X = imagePoint.X;
							sender = this.pW;
						}
					}
				}
				else if (sender == this.pS)
				{
					if (dy != 0)
					{
						if (newRect.Height + dy > 0)
							newRect.Height += dy;
						else
						{
							newRect.Height = newRect.Y - imagePoint.Y;
							newRect.Y = imagePoint.Y;
							sender = this.pN;
						}
					}
				}
				else if (sender == this.pNW)
				{
					if ((newRect.Width - dx > 0) && (newRect.Height - dy > 0))
					{
						newRect.X += dx;
						newRect.Width -= dx;
						newRect.Y += dy;
						newRect.Height -= dy;
					}
					else if ((newRect.Width - dx > 0) && (newRect.Height - dy < 0))
					{
						newRect.X += dx;
						newRect.Width -= dx;
						newRect.Y = newRect.Y + newRect.Height;
						newRect.Height = dy - newRect.Height;
						sender = this.pSW;
					}
					else if ((newRect.Width - dx < 0) && (newRect.Height - dy > 0))
					{
						newRect.X = newRect.X + newRect.Width;
						newRect.Width = dx - newRect.Width;
						newRect.Y += dy;
						newRect.Height -= dy;
						sender = this.pNE;
					}
					else if ((newRect.Width - dx < 0) && (newRect.Height - dy < 0))
					{
						newRect.X = newRect.X + newRect.Width;
						newRect.Width = dx - newRect.Width;
						newRect.Y = newRect.Y + newRect.Height;
						newRect.Height = dy - newRect.Height;
						sender = this.pSE;
					}
				}
				else if (sender == this.pNE)
				{
					if ((newRect.Width + dx > 0) && (newRect.Height - dy > 0))
					{
						newRect.Width += dx;
						newRect.Y += dy;
						newRect.Height -= dy;
					}
					else if ((newRect.Width + dx < 0) && (newRect.Height - dy > 0))
					{
						newRect.Width = newRect.X - imagePoint.X;
						newRect.X = imagePoint.X;
						newRect.Y += dy;
						newRect.Height -= dy;
						sender = this.pNW;
					}
					else if ((newRect.Width + dx > 0) && (newRect.Height - dy < 0))
					{
						newRect.Width += dx;
						newRect.Y = newRect.Y + newRect.Height;
						newRect.Height = dy - newRect.Height;
						sender = this.pSE;
					}
					else if ((newRect.Width + dx < 0) && (newRect.Height - dy < 0))
					{
						newRect.Width = newRect.X - imagePoint.X;
						newRect.X = imagePoint.X;
						newRect.Y = newRect.Y + newRect.Height;
						newRect.Height = dy - newRect.Height;
						sender = this.pSW;
					}
				}
				else if (sender == this.pSW)
				{
					if ((newRect.Width - dx > 0) && (newRect.Height + dy > 0))
					{
						newRect.X += dx;
						newRect.Width -= dx;
						newRect.Height += dy;
					}
					else if ((newRect.Width - dx < 0) && (newRect.Height + dy > 0))
					{
						newRect.X = newRect.X + newRect.Width;
						newRect.Width = dx - newRect.Width;
						newRect.Height += dy;
						sender = this.pSE;
					}
					else if ((newRect.Width - dx > 0) && (newRect.Height + dy < 0))
					{
						newRect.X += dx;
						newRect.Width -= dx;
						newRect.Height = newRect.Y - imagePoint.Y;
						newRect.Y = imagePoint.Y;
						sender = this.pNW;
					}
					else if ((newRect.Width - dx < 0) && (newRect.Height + dy < 0))
					{
						newRect.X = newRect.X + newRect.Width;
						newRect.Width = dx - newRect.Width;
						newRect.Height = newRect.Y - imagePoint.Y;
						newRect.Y = imagePoint.Y;
						sender = this.pNE;
					}
				}
				else if (sender == this.pSE)
				{
					if ((newRect.Width + dx > 0) && (newRect.Height + dy > 0))
					{
						newRect.Width += dx;
						newRect.Height += dy;
					}
					else if ((newRect.Width + dx < 0) && (newRect.Height + dy > 0))
					{
						newRect.Width = newRect.X - imagePoint.X;
						newRect.X = imagePoint.X;
						newRect.Height += dy;
						sender = this.pSW;
					}
					else if ((newRect.Width + dx > 0) && (newRect.Height + dy < 0))
					{
						newRect.Width += dx;
						newRect.Height = newRect.Y - imagePoint.Y;
						newRect.Y = imagePoint.Y;
						sender = this.pNE;
					}
					else if ((newRect.Width + dx < 0) && (newRect.Height + dy < 0))
					{
						newRect.Width = newRect.X - imagePoint.X;
						newRect.X = imagePoint.X;
						newRect.Height = newRect.Y - imagePoint.Y;
						newRect.Y = imagePoint.Y;
						sender = this.pNW;
					}
				}
			}

			if (origRect != newRect)
			{
				dl = newRect.X - origRect.X;
				dt = newRect.Y - origRect.Y;
				dr = newRect.Right - origRect.Right;
				db = newRect.Bottom - origRect.Bottom;

				return true;
			}
			else
			{
				dl = 0;
				dt = 0;
				dr = 0;
				db = 0;

				return false;
			}
		}*/
		#endregion

		#region SelectAffectedPages()
		void SelectAffectedPages(MouseOperation mouseOperation)
		{
			if (this.ImagePane.ItImages != null && this.Page != null && this.Page.ItImage.IsFixed == false)
			{
				ReleaseAffectedPages();

				switch (mouseOperation)
				{
					case MouseOperation.Move:
					case MouseOperation.Skew:
						{
							this.affectedPages.Add(this.Page);

							if (this.Page.ItImage.IsFixed == false && this.Page.ItImage.IsIndependent == false && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
							{
								ImageProcessing.IpSettings.ItImages itImages = this.ImagePane.ItImages;
								int imageIndex = itImages.IndexOf(this.Page.ItImage);
								bool leftPage = !(this.Page == this.Page.ItImage.GetRightPage());

								for (int i = imageIndex + 1; i < itImages.Count; i++)
								{
									if (itImages[i].IsFixed == false && itImages[i].IsIndependent == false)
									{
										ImageProcessing.IpSettings.ItPage itPage = (leftPage) ? itImages[i].GetLeftPage() : itImages[i].GetRightPage();

										if (itPage != null && itPage.ClipSpecified && itPage.ClipRect.IsEmpty == false)
											affectedPages.Add(itPage);
									}
								}
							}
						} break;
					case MouseOperation.Resize:
						{
							this.affectedPages.Add(this.Page);
							
							if (this.Page.ItImage.IsFixed == false && this.Page.ItImage.IsIndependent == false)
							{
								ImageProcessing.IpSettings.ItImages itImages = this.ImagePane.ItImages;

								for (int i = 0; i < itImages.Count; i++)
								{
									if (itImages[i].IsFixed == false && itImages[i].IsIndependent == false)
									{
										if (itImages[i].TwoPages)
										{
											affectedPages.Add(itImages[i].PageL);
											affectedPages.Add(itImages[i].PageR);
										}
										else if (itImages[i].Page.ClipSpecified && itPage.ClipRect.IsEmpty == false)
										{
											affectedPages.Add(itImages[i].Page);
										}
									}
								}
							}
						} break;
				}

				foreach (ImageProcessing.IpSettings.ItPage itPage in affectedPages)
					itPage.Lock();
			}
		}
		#endregion

		#region ReleaseAffectedPages()
		void ReleaseAffectedPages()
		{
			foreach (ImageProcessing.IpSettings.ItPage itPage in affectedPages)
				itPage.Unlock();
			
			affectedPages.Clear();
		}
		#endregion
	
		#region Mouse_LeftButtonDown()
		private void Mouse_LeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			lock (locker)
			{
				if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.Pages)
				{
					this.startPoint = this.ImagePane.PanelToImage(e);
					//Console.WriteLine("Mouse_LeftButtonDown: Start Point set to " + startPoint.Value.X.ToString());

					if (sender == this.contentRect)
						SelectAffectedPages(MouseOperation.Move);
					else if (sender == this.pSkewL1 || sender == this.pSkewL2 || sender == this.pSkewL3 || sender == this.pSkewL4 ||
						sender == this.pSkewR1 || sender == this.pSkewR2 || sender == this.pSkewR3 || sender == this.pSkewR4)
					{
						SelectAffectedPages(MouseOperation.Skew);
						this.skewOriginalY = Math.Tan(itPage.Skew) * (itPage.Clip.RectangleNotSkewed.Width / 2);
					}
					else
						SelectAffectedPages(MouseOperation.Resize);

					((UIElement)sender).CaptureMouse();
					this.selectedControl = (UIElement)sender;
					e.Handled = true;
				}
			}
		}
		#endregion

		#region Skew_MouseMove()
		private void Skew_MouseMove(object sender, MouseEventArgs e)
		{
			lock (locker)
			{
				if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.Pages)
				{
					if (e.LeftButton == MouseButtonState.Pressed && startPoint.HasValue)
					{
						//Point mousePoint = this.imageUi.GetMousePoint(e);
						RatioPoint p = PanelToImage(e);
						double skew;

						if (sender == this.pSkewL1 || sender == this.pSkewL2 || sender == this.pSkewL3 || sender == this.pSkewL4)
							skew = Math.Atan2(skewOriginalY - (p.Y - this.startPoint.Value.Y), this.itPage.Clip.RectangleNotSkewed.Width / 2);
						else
							skew = Math.Atan2(skewOriginalY + (p.Y - this.startPoint.Value.Y), this.itPage.Clip.RectangleNotSkewed.Width / 2);

						foreach (ImageProcessing.IpSettings.ItPage pg in affectedPages)
							pg.SetSkew(skew, 1.0F);

						this.ImagePane.InvalidateVisual();
					}

					e.Handled = true;
				}
			}
		}
		#endregion

		#region Move_MouseMove()
		private void Move_MouseMove(object sender, MouseEventArgs e)
		{
			lock (locker)
			{
				if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.Pages && e.LeftButton == MouseButtonState.Pressed && startPoint.HasValue &&
					this.selectedControl == sender && this.selectedControl == this.contentRect)
				{
					//Point mousePoint = this.imageUi.GetMousePoint(e);
					RatioPoint p = this.ImagePane.PanelToImage(e);

					double dl = 0, dt = 0, dr = 0, db = 0;

					if (MoveRegion(p, startPoint.Value, itPage.Clip, ref dl, ref dt, ref dr, ref db))
					{
						ImageProcessing.IpSettings.ResizeDirection direction = GetResizeDirection(sender);

						if (this.ImagePane.IImage.IsIndependent == false && this.ImagePane.ViewPanel.ItImages != null)
						{
							InchSize imageInchSize = itPage.ItImage.InchSize;

							InchPoint newLocation = new InchPoint((itPage.Clip.RectangleNotSkewed.X + dl) * imageInchSize.Width, (itPage.Clip.RectangleNotSkewed.Y + dt) * imageInchSize.Height);

							foreach (ImageProcessing.IpSettings.ItPage pg in affectedPages)
								pg.MoveClip(newLocation, true);
						}
						else
						{
							RatioRect clip = this.itPage.Clip.RectangleNotSkewed;

							clip.X += dl;
							clip.Y += dt;

							this.itPage.SetClip(clip, true);
						}

						this.startPoint = p;
						//Console.WriteLine("Move_MouseMove: Start Point set to " + startPoint.Value.X.ToString());
						this.ImagePane.InvalidateVisual();
					}

					e.Handled = true;
				}
			}
		}
		#endregion

		#region Resize_MouseMove()
		private void Resize_MouseMove(object sender, MouseEventArgs e)
		{
			lock (locker)
			{
				//Console.WriteLine("In.");
				if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.Pages && e.LeftButton == MouseButtonState.Pressed && startPoint.HasValue &&
								this.selectedControl == sender && sender != this.contentRect)
				{
					//Point mousePoint = this.imageUi.GetMousePoint(e);
					RatioPoint p = this.ImagePane.PanelToImage(e);
					//Console.WriteLine(p.X.ToString() + "..." + this.startPoint.Value.X.ToString());

					double dl = 0, dt = 0, dr = 0, db = 0;
					object s = sender;

					if (ResizeRegion(ref s, p, startPoint.Value, itPage.Clip, ref dl, ref dt, ref dr, ref db))
					{
						ImageProcessing.IpSettings.ResizeDirection direction = GetResizeDirection(s);

						if (this.ImagePane.IImage.IsIndependent == false && this.ImagePane.ViewPanel.ItImages != null)
						{
							InchSize imageInchSize = itPage.ItImage.InchSize;

							InchSize newInchSize = new InchSize(imageInchSize.Width * (itPage.ClipRect.Width - dl + dr), imageInchSize.Height * (itPage.ClipRect.Height - dt + db));

							ImageProcessing.IpSettings.ResizeDirection directionL, directionR;

							switch (this.Page.Layout)
							{
								case ImageProcessing.IpSettings.ItPage.PageLayout.Left:
									{
										directionL = direction;

										if (Keyboard.Modifiers == ModifierKeys.Control)
											directionR = direction;
										else
											directionR = GetOppositeDirection(direction);
									} break;
								case ImageProcessing.IpSettings.ItPage.PageLayout.Right:
									{
										if (Keyboard.Modifiers == ModifierKeys.Control)
											directionL = direction;
										else
											directionL = GetOppositeDirection(direction);
										
										directionR = direction;
									} break;
								default:
									{
										directionL = direction;
										directionR = direction;
									} break;
							}

							this.ImagePane.ViewPanel.ItImages.ChangeClipsSize(newInchSize, directionL, directionR, true);
						}
						else
						{
							this.itPage.SetClip(dl, dt, dr, db, false);
						}

						this.startPoint = p;
						//Console.WriteLine("Resize_MouseMove: Start Point set to " + startPoint.Value.X.ToString());
						this.ImagePane.InvalidateVisual();
					}

					if (s != sender)
					{
						((UIElement)s).CaptureMouse();
						this.selectedControl = (UIElement)s;
					}

					e.Handled = true;
				}
				//Console.WriteLine("Out.");
			}
		}
		#endregion

		#region GetOppositeDirection()
		ImageProcessing.IpSettings.ResizeDirection GetOppositeDirection(ImageProcessing.IpSettings.ResizeDirection direction)
		{
			switch (direction)
			{
				case ImageProcessing.IpSettings.ResizeDirection.NE: return ImageProcessing.IpSettings.ResizeDirection.NW;
				case ImageProcessing.IpSettings.ResizeDirection.E: return ImageProcessing.IpSettings.ResizeDirection.W;
				case ImageProcessing.IpSettings.ResizeDirection.SE: return ImageProcessing.IpSettings.ResizeDirection.SW;
				case ImageProcessing.IpSettings.ResizeDirection.NW: return ImageProcessing.IpSettings.ResizeDirection.NE;
				case ImageProcessing.IpSettings.ResizeDirection.W: return ImageProcessing.IpSettings.ResizeDirection.E;
				case ImageProcessing.IpSettings.ResizeDirection.SW: return ImageProcessing.IpSettings.ResizeDirection.SE;
				default: return direction;
			}
		}
		#endregion

		#region Mouse_LeftButtonUp()
		private void Mouse_LeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			lock (locker)
			{
				if (this.ImagePane.ToolbarSelection == ViewPane.ToolbarSelection.Pages)
				{
					startPoint = null;
					//Console.WriteLine("Start Point set to null");
					this.selectedControl = null;
					ReleaseAffectedPages();
					Mouse.Capture(null);
					e.Handled = true;
				}
			}
		}
		#endregion

		#region Mouse_RightButtonDown()
		private void Mouse_RightButtonDown(object sender, MouseButtonEventArgs e)
		{
			//if (RemovePageRequest != null)
			//	RemovePageRequest(this, null);

			if (itPage != null)
				itPage.RaiseRemoveRequest();
		}
		#endregion

		#region Page_Changed()
		private void Page_Changed(ImageProcessing.IpSettings.ItPage itPage, ImageProcessing.IpSettings.ItProperty type)
		{
			if (type == ImageProcessing.IpSettings.ItProperty.Clip)
			{
#if DEBUG
				//System.Windows.Forms.MessageBox.Show("PageUi, Page_Changed(): 01");
#endif

				if (this.Dispatcher.CheckAccess())
					Page_ChangedTU(itPage);
				else
					this.Dispatcher.Invoke((Action)delegate() { Page_ChangedTU(itPage); });

#if DEBUG
				//System.Windows.Forms.MessageBox.Show("PageUi, Page_Changed(): 02");
#endif
			}
		}
		#endregion

		#region Page_ChangedTU()
		private void Page_ChangedTU(ImageProcessing.IpSettings.ItPage itPage)
		{
			RefreshUi();
		}
		#endregion

		#region RefreshUi()
		private void RefreshUi()
		{
			if (this.itPage != null && this.itPage.IsActive && this.itPage.ItImage.IsFixed == false && itPage.ClipSpecified && itPage.ClipRect.IsEmpty == false &&
				double.IsNaN(this.ImageUi.Width) == false && double.IsNaN(this.ImageUi.Height) == false)
			{
				this.Visibility = Visibility.Visible;
				double width = (double.IsNaN(this.ImageUi.Width)) ? this.ImageUi.ActualWidth : this.ImageUi.Width;
				double height = (double.IsNaN(this.ImageUi.Height)) ? this.ImageUi.ActualHeight : this.ImageUi.Height;
				this.Margin = new Thickness(itPage.ClipRect.X * width, itPage.ClipRect.Y * height, 0, 0);
				this.Width = itPage.ClipRect.Width * width;
				this.Height = itPage.ClipRect.Height * height;

				if (this.ToolbarSelection == ViewPane.ToolbarSelection.Pages)
					selectedGrid.Visibility = Visibility.Visible;
				else
					selectedGrid.Visibility = Visibility.Hidden;

				if (itPage.Skew != 0)
				{
					RotateTransform rotateTransform = new RotateTransform(itPage.Skew * 180 / Math.PI, this.Width / 2, this.Height / 2);
					this.mainGrid.RenderTransform = rotateTransform;
				}
				else
					this.mainGrid.RenderTransform = null;
			}
			else
			{
				this.Visibility = Visibility.Hidden;
			}

			SetToolTips();

			this.bookfoldUi.Synchronize();
			this.fingersUi.Synchronize(this.itPage);

			if (this.ToolbarSelection == ViewPane.ToolbarSelection.Pages)
				this.contentRect.Cursor = Cursors.SizeAll;
			else
				this.contentRect.Cursor = null;
		}
		#endregion

		#region MainGrid_MouseLeftDown()
		private void MainGrid_MouseLeftDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				switch (this.ToolbarSelection)
				{
					case ViewPane.ToolbarSelection.Pages:
						{
						} break;
					case ViewPane.ToolbarSelection.Bookfold:
						{
						} break;
					case ViewPane.ToolbarSelection.FingerRemoval:
						{
							this.fingersUi.Mouse_LeftButtonDown(sender, e);
						} break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region MainGrid_MouseLeftUp()
		private void MainGrid_MouseLeftUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				switch (this.ToolbarSelection)
				{
					case ViewPane.ToolbarSelection.Pages:
						{
						} break;
					case ViewPane.ToolbarSelection.Bookfold:
						{
						} break;
					case ViewPane.ToolbarSelection.FingerRemoval:
						{
						} break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region MainGrid_MouseMove()
		private void MainGrid_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				switch (this.ToolbarSelection)
				{
					case ViewPane.ToolbarSelection.Pages:
						{
						} break;
					case ViewPane.ToolbarSelection.Bookfold:
						{
							this.bookfoldUi.MainGrid_MouseMove(sender, e);
						} break;
					case ViewPane.ToolbarSelection.FingerRemoval:
						{
						} break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region MainGrid_MouseRightDown()
		private void MainGrid_MouseRightDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				switch (this.ToolbarSelection)
				{
					case ViewPane.ToolbarSelection.Pages:
						{
						} break;
					case ViewPane.ToolbarSelection.Bookfold:
						{
						} break;
					case ViewPane.ToolbarSelection.FingerRemoval:
						{
						} break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region MainGrid_MouseRightUp()
		private void MainGrid_MouseRightUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				switch (this.ToolbarSelection)
				{
					case ViewPane.ToolbarSelection.Pages:
						{
						} break;
					case ViewPane.ToolbarSelection.Bookfold:
						{
						} break;
					case ViewPane.ToolbarSelection.FingerRemoval:
						{
						} break;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region SetToolTips()
		private void SetToolTips()
		{
			if (this.itPage != null && this.itPage.ItImage.IsFixed == false && itPage.ClipSpecified && itPage.ClipRect.IsEmpty == false)
			{
				//setting up tool tips
				if (this.itPage.ItImage.IsIndependent == false && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
					selectedGrid.ToolTip = "Content relocation applies to all subsequent pages.";
				else
					selectedGrid.ToolTip = "Content relocation applies to current page only.";

				foreach (UIElement control in this.selectedGrid.Children)
				{
					if (control is Button)
					{
						if (this.itPage.ItImage.IsIndependent)
							((Button)control).ToolTip = "Page size applies to current page only.";
						else
							((Button)control).ToolTip = "Page size applies to all pages.";
					}
				}

				foreach (UIElement control in this.gridSkew.Children)
				{
					if (control is Button)
					{
						if (this.itPage.ItImage.IsIndependent == false && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
							((Button)control).ToolTip = "Skew applies to all subsequent pages.";
						else
							((Button)control).ToolTip = "Skew applies to current page only.";
					}
				}
			}
			else
			{
			}
		}
		#endregion

		#region GetResizeDirection()
		private ImageProcessing.IpSettings.ResizeDirection GetResizeDirection(object sender)
		{
			if (sender == this.pW)
				return ImageProcessing.IpSettings.ResizeDirection.W;
			else if(sender == this.pN)
				return ImageProcessing.IpSettings.ResizeDirection.N;
			else if (sender == this.pE)
				return ImageProcessing.IpSettings.ResizeDirection.E;
			else if (sender == this.pS)
				return ImageProcessing.IpSettings.ResizeDirection.S;
			else if (sender == this.pNW)
				return ImageProcessing.IpSettings.ResizeDirection.NW;
			else if (sender == this.pNE)
				return ImageProcessing.IpSettings.ResizeDirection.NE;
			else if (sender == this.pSE)
				return ImageProcessing.IpSettings.ResizeDirection.SE;
			else if (sender == this.pSW)
				return ImageProcessing.IpSettings.ResizeDirection.SW;
			else
				return ImageProcessing.IpSettings.ResizeDirection.All;
			
		}
		#endregion

		#region Close_Click()
		private void Close_Click(object sender, RoutedEventArgs e)
		{
			if (itPage != null)
				itPage.RaiseRemoveRequest();
		}
		#endregion

		#endregion

	}
}
