using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using ViewPane.ItResults;
using System.Windows.Media.Imaging;

namespace ViewPane.IP
{
	public class ItResultsCreator
	{
		static ItResultsCreator instance = null;
		static object			instanceLocker = new object();

		FileInfo			lastFile = null;
		Bitmap				lastBitmap = null;

		object				threadLocker = new object ();

		System.Threading.AutoResetEvent waitEvent = new System.Threading.AutoResetEvent (true);
		System.Threading.AutoResetEvent waitForWorkerThreadToDieEvent = new System.Threading.AutoResetEvent (true);
		bool				isDisposed = false;
		bool				isRunning = true;
		object				runVariableLocker = new object ();
		object				queuesLocker = new object ();

		Queue<WorkUnit>		priorityQueue = new Queue<WorkUnit> ();
		Queue<WorkUnit>		normalQueue = new Queue<WorkUnit> ();



		#region constructor
		private ItResultsCreator()
		{
			System.Threading.Thread thread = new System.Threading.Thread (new System.Threading.ThreadStart (CreateImageThread));
			thread.Name = "ItResultsCreator, ItResultsCreator()";
			thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			thread.SetApartmentState (System.Threading.ApartmentState.STA);
			thread.Start ();
		}
		#endregion constructor


		#region class WorkUnit
		internal class WorkUnit
		{
			public readonly ViewPane.IP.IitResultsCaller		Caller;
			public readonly int									RenderingId;
			public readonly ItResultsImage						ItResultsImage;
			public readonly bool								LeftPage;
			public readonly System.Windows.Rect					ImageRect;
			public readonly double								Zoom;
			public readonly ImageProcessing.Resizing.ResizeMode Quality;
			public bool											IsCanceled = false;

			public WorkUnit(ViewPane.IP.IitResultsCaller caller, int renderingId, ItResultsImage itResultsImage, bool leftPage, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
			{
				this.Caller = caller;
				this.RenderingId = renderingId;
				this.ItResultsImage = itResultsImage;
				this.LeftPage = leftPage;
				this.ImageRect = imageRect;
				this.Zoom = zoom;
				this.Quality = quality;
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public static bool IsDisposed { get { return (instance == null); } }

		public static ItResultsCreator Instance 
		{ 
			get 
			{
				lock (instanceLocker)
				{
					if (instance == null)
						instance = new ItResultsCreator();

					return instance;
				}
			} 
		}


		#endregion public properties


		//PRIVATE PROPERTIES
		#region private properties

		#region IsRunning
		internal bool IsRunning
		{
			get
			{
				lock (runVariableLocker)
				{
					return this.isRunning;
				}
			}
			set
			{
				lock (runVariableLocker)
				{
					this.isRunning = value;
				}
			}
		}
		#endregion IsRunning

		#endregion private properties


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public static void Dispose ()
		{
			lock (instanceLocker)
			{
				if (instance != null)
				{
					if (instance.isDisposed == false)
					{
						lock (instance.runVariableLocker)
						{
							instance.isRunning = false;
						}

						lock (instance.queuesLocker)
						{
							instance.priorityQueue.Clear();
							instance.normalQueue.Clear();
							instance.waitEvent.Set();
						}

						instance.waitForWorkerThreadToDieEvent.WaitOne();
						instance.isDisposed = true;
					}
				}

				instance = null;
			}
		}
		#endregion Dispose()

		#region GetPreview()
		public Bitmap GetPreview (FileInfo file, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			return GetPreview(file, new BIP.Geometry.RatioRect(0, 0, 1, 1), zoom, quality);
		}

		public Bitmap GetPreview (FileInfo file, BIP.Geometry.RatioRect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			if (isDisposed == false)
			{
				lock (threadLocker)
				{
					try
					{
						if (file != null && this.lastFile != null && file.FullName.ToLower () != this.lastFile.FullName.ToLower ())
						{
							if (this.lastBitmap != null)
							{
								this.lastBitmap.Dispose ();
								this.lastBitmap = null;
							}

							this.lastFile = file;

							ImageProcessing.ImageFile.ImageInfo imageInfo = new ImageProcessing.ImageFile.ImageInfo (this.lastFile);

							if ((imageInfo.BitsPerPixel / 8) * imageInfo.Width * imageInfo.Height < 100 * 1024 * 1024)
							{
								try
								{
									this.lastBitmap = ImageProcessing.ImageCopier.LoadFileIndependentImage (file.FullName);
								}
								catch { this.lastBitmap = null; }
							}
						}

						if (this.lastBitmap != null)
						{
							switch (this.lastBitmap.PixelFormat)
							{
								case System.Drawing.Imaging.PixelFormat.Format24bppRgb: return GetPreview24bpp (this.lastBitmap, imageRect, zoom, quality);
								case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: return GetPreview8bpp (this.lastBitmap, imageRect, zoom, quality);
								case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
									return GetPreview1bpp (this.lastBitmap, imageRect, zoom, quality);
								default:
									//Notifications.Instance.Notify(this, Notifications.Type.Error, "ItResultsCreator, GetPreview(): Unsupported format", null);
									throw new VpException ("Can't create preview image!");
							}
						}
						else
						{
							using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder (file.FullName))
							{
								switch (itDecoder.PixelsFormat)
								{
									case ImageProcessing.PixelsFormat.Format24bppRgb: return GetPreview24bpp (itDecoder, imageRect, zoom, quality);
									case ImageProcessing.PixelsFormat.Format8bppGray:
									case ImageProcessing.PixelsFormat.Format8bppIndexed:
										return GetPreview8bpp (itDecoder, imageRect, zoom, quality);
									case ImageProcessing.PixelsFormat.FormatBlackWhite:
										return GetPreview1bpp (itDecoder, imageRect, zoom, quality);
									default:
										//Notifications.Instance.Notify(this, Notifications.Type.Error, "ItResultsCreator, GetPreview(): Unsupported format", null);
										throw new VpException ("Can't create preview image!");
								}
							}
						}
					}
					catch (VpException ex)
					{
						throw ex;
					}
					catch (Exception)// ex)
					{
						//Notifications.Instance.Notify(this, Notifications.Type.Error, "ItResultsCreator, GetPreview(): " + ex.Message, ex);
						throw new VpException ("Can't create preview image!");
					}
				}
			}
			else
				return null;
		}
		#endregion GetPreview()

		#region GetPreviewAsync()
		internal void GetPreviewAsync (ViewPane.IP.IitResultsCaller caller, int renderingId, ItResultsImage itResultsImage, bool leftPage, bool highPriority, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			System.Windows.Rect rect;

			if (leftPage == false && itResultsImage.FullImageSizeR != null)
				rect = new System.Windows.Rect(0,0,itResultsImage.FullImageSizeR.Width, itResultsImage.FullImageSizeR.Height);
			else
				rect = new System.Windows.Rect(0, 0, itResultsImage.FullImageSizeL.Width, itResultsImage.FullImageSizeL.Height);
			
			GetPreviewAsync(caller, renderingId, itResultsImage, leftPage, highPriority, rect, zoom, quality);
		}

		internal void GetPreviewAsync(ViewPane.IP.IitResultsCaller caller, int renderingId, ItResultsImage itResultsImage, bool leftPage, bool highPriority, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			if (isDisposed == false)
			{
				lock (queuesLocker)
				{
					WorkUnit workUnit = new WorkUnit(caller, renderingId, itResultsImage, leftPage, imageRect, zoom, quality);

					foreach (WorkUnit existingWorkUnit in priorityQueue)
						if (existingWorkUnit.Caller == caller)
							existingWorkUnit.IsCanceled = true;

					foreach (WorkUnit existingWorkUnit in normalQueue)
						if (existingWorkUnit.Caller == caller)
							existingWorkUnit.IsCanceled = true;

					if (highPriority)
						priorityQueue.Enqueue (workUnit);
					else
						normalQueue.Enqueue (workUnit);

					waitEvent.Set ();
				}
			}
		}
		#endregion GetPreviewAsync()

		#region Reset()
		public void Reset ()
		{
			if (isDisposed == false)
			{
				lock (threadLocker)
				{
					if (this.lastBitmap != null)
					{
						this.lastBitmap.Dispose ();
						this.lastBitmap = null;
					}

					this.lastFile = null;
				}

				lock (queuesLocker)
				{
					priorityQueue.Clear ();
					normalQueue.Clear ();
				}
			}
		}
		#endregion Reset()

		#endregion public methods


		//PRIVATE METHODS
		#region private methods

		#region CreateImageThread()
		private void CreateImageThread ()
		{
			try
			{
				waitForWorkerThreadToDieEvent.Reset ();

				while (IsRunning)
				{
					try
					{
						waitEvent.WaitOne ();
						WorkUnit workUnit = null;

						lock (queuesLocker)
						{
							if (priorityQueue.Count > 0)
								workUnit = priorityQueue.Dequeue ();
							else if (normalQueue.Count > 0)
								workUnit = normalQueue.Dequeue ();

							waitEvent.Reset ();
						}

						while (IsRunning && (workUnit != null))
						{
							try
							{
								if (workUnit.IsCanceled || workUnit.Caller == null/* || workUnit.Caller.IsDisplayed == false*/)
								{
									workUnit.Caller.ThumbnailCanceledDelegate ();
								}
								else
								{
									Bitmap bitmap = workUnit.ItResultsImage.GetClip(workUnit.LeftPage, workUnit.ImageRect, workUnit.Zoom);
									
									workUnit.Caller.ThumbnailCreatedDelegate(bitmap, workUnit.RenderingId);
								}
							}
							catch (Exception ex)
							{
								workUnit.Caller.ThumbnailErrorDelegate (ex);
							}

							lock (queuesLocker)
							{
								if (priorityQueue.Count > 0)
									workUnit = priorityQueue.Dequeue ();
								else if (normalQueue.Count > 0)
									workUnit = normalQueue.Dequeue ();
								else
									workUnit = null;
							}
						}
					}
#if DEBUG
					catch (Exception)
					{
					}
#else
				catch {}
#endif
				}
			}
			finally
			{
				waitForWorkerThreadToDieEvent.Set ();
			}
		}
		#endregion CreateImageThread()

		#region GetPreview24bpp()
		private Bitmap GetPreview24bpp (ImageProcessing.BigImages.ItDecoder itDecoder, BIP.Geometry.RatioRect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle	clipRect = new Rectangle (Convert.ToInt32 (imageRect.X * itDecoder.Width), Convert.ToInt32 (imageRect.Y * itDecoder.Height), Convert.ToInt32 (imageRect.Width * itDecoder.Width), Convert.ToInt32 (imageRect.Height * itDecoder.Height));

			if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
			{
				ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing ();
				return resizing.ResizeToBitmap (itDecoder, clipRect, zoom);
			}
			else
			{
				return ViewPane.IP.Resizing.GetBluryPreview (itDecoder, clipRect);
			}
		}
		#endregion GetPreview24bpp()

		#region GetPreview8bpp()
		private Bitmap GetPreview8bpp (ImageProcessing.BigImages.ItDecoder itDecoder, BIP.Geometry.RatioRect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle (Convert.ToInt32 (imageRect.X * itDecoder.Width), Convert.ToInt32 (imageRect.Y * itDecoder.Height), Convert.ToInt32 (imageRect.Width * itDecoder.Width), Convert.ToInt32 (imageRect.Height * itDecoder.Height));

			if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
			{
				ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing ();
				return resizing.ResizeToBitmap (itDecoder, clipRect, zoom);
			}
			else
				return ViewPane.IP.Resizing.GetBluryPreview (itDecoder, clipRect);
		}
		#endregion GetPreview8bpp()

		#region GetPreview1bpp()
		private Bitmap GetPreview1bpp (ImageProcessing.BigImages.ItDecoder itDecoder, BIP.Geometry.RatioRect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle (Convert.ToInt32 (imageRect.X * itDecoder.Width), Convert.ToInt32 (imageRect.Y * itDecoder.Height), Convert.ToInt32 (imageRect.Width * itDecoder.Width), Convert.ToInt32 (imageRect.Height * itDecoder.Height));

			if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
			{
				if (zoom != 1)
				{
					Size desiredSize = new Size (Convert.ToInt32 (imageRect.Width * itDecoder.Width * zoom), Convert.ToInt32 (imageRect.Height * itDecoder.Height * zoom));

					using (Bitmap source = itDecoder.GetClip (clipRect))
					{
						return ImageProcessing.ThumbnailCreator.Get (source, desiredSize);
					}
				}
				else
				{
					return itDecoder.GetClip (clipRect);
				}
			}
			else
				return ViewPane.IP.Resizing.GetBluryPreview (itDecoder, clipRect);
		}
		#endregion GetPreview1bpp()

		#region GetPreview24bpp()
		private Bitmap GetPreview24bpp (Bitmap bitmap, BIP.Geometry.RatioRect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle (Convert.ToInt32 (imageRect.X * bitmap.Width), Convert.ToInt32 (imageRect.Y * bitmap.Height), Convert.ToInt32 (imageRect.Width * bitmap.Width), Convert.ToInt32 (imageRect.Height * bitmap.Height));

			if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
				return ImageProcessing.Resizing.Resize (bitmap, clipRect, zoom, ImageProcessing.Resizing.ResizeMode.Quality);
			else
				return ViewPane.IP.Resizing.GetBluryPreview (bitmap, clipRect);
		}
		#endregion GetPreview24bpp()

		#region GetPreview8bpp()
		private Bitmap GetPreview8bpp (Bitmap bitmap, BIP.Geometry.RatioRect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle (Convert.ToInt32 (imageRect.X * bitmap.Width), Convert.ToInt32 (imageRect.Y * bitmap.Height), Convert.ToInt32 (imageRect.Width * bitmap.Width), Convert.ToInt32 (imageRect.Height * bitmap.Height));

			if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
				return ImageProcessing.Resizing.Resize (bitmap, clipRect, zoom, ImageProcessing.Resizing.ResizeMode.Quality);
			else
				return ViewPane.IP.Resizing.GetBluryPreview (bitmap, clipRect);
		}
		#endregion GetPreview8bpp()

		#region GetPreview1bpp()
		private Bitmap GetPreview1bpp (Bitmap source, BIP.Geometry.RatioRect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle (Convert.ToInt32 (imageRect.X * source.Width), Convert.ToInt32 (imageRect.Y * source.Height), Convert.ToInt32 (imageRect.Width * source.Width), Convert.ToInt32 (imageRect.Height * source.Height));

			if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
			{
				/*if (zoom == 1 && )
				{
					return source;
				}
				else*/
				{
					Size desiredSize = new Size (Convert.ToInt32 (source.Width * zoom), Convert.ToInt32 (source.Height * zoom));
					return ImageProcessing.Resizing.GetThumbnail (source, clipRect, desiredSize);
				}
			}
			else
				return ViewPane.IP.Resizing.GetBluryPreview (source, clipRect);
		}
		#endregion GetPreview1bpp()

		#endregion private methods
	}
}