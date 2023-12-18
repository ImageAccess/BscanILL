using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using BscanILL.Misc;

namespace BscanILL.IP
{
	#region interface IPreviewCaller
	public interface IPreviewCaller
	{
		bool		IsDisplayed { get; }

		void		ThumbnailCreatedDelegate(System.Drawing.Bitmap bitmap, int renderingId, TimeSpan timeSpan);
		void		ThumbnailCanceledDelegate();
		void		ThumbnailErrorDelegate(Exception ex);
	}
	#endregion
	
	
	class PreviewCreator
	{
		private static PreviewCreator instance = new PreviewCreator();

		FileInfo			lastFile = null;
		Bitmap				lastBitmap = null;

		object				threadLocker = new object();

		System.Threading.AutoResetEvent waitEvent = new System.Threading.AutoResetEvent(true);
		System.Threading.AutoResetEvent waitForWorkerThreadToDieEvent = new System.Threading.AutoResetEvent(true);
		volatile bool		isDisposed = false;
		volatile bool		run = true;
		object				runVariableLocker = new object();
		object				queuesLocker = new object();

		Queue<WorkUnit>		priorityQueue = new Queue<WorkUnit>();
		Queue<WorkUnit>		normalQueue = new Queue<WorkUnit>();



		#region constructor
		private PreviewCreator()
		{
			System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(CreateImageThread));
			thread.Name = "ThreadPreviewCreator_CreateImage";
			thread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			thread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			thread.SetApartmentState(System.Threading.ApartmentState.STA);
			thread.Start();
		}
		#endregion

		#region class WorkUnit
		internal class WorkUnit
		{
			public readonly BscanILL.IP.IPreviewCaller			Caller;
			public readonly int									RenderingId;
			public readonly System.Windows.Rect					ImageRect;
			public readonly FileInfo							File;
			public readonly Scanners.ColorMode					ColorMode;
			public readonly double								Zoom;
			public readonly ImageProcessing.Resizing.ResizeMode Quality;
			public bool											IsCanceled = false;

			public WorkUnit(BscanILL.IP.IPreviewCaller caller, int renderingId, FileInfo file, System.Windows.Rect imageRect, Scanners.ColorMode colorMode, double zoom, ImageProcessing.Resizing.ResizeMode quality)
			{
				this.Caller = caller;
				this.RenderingId = renderingId;
				this.File = file;
				this.ImageRect = imageRect;
				this.ColorMode = colorMode;
				this.Zoom = zoom;
				this.Quality = quality;
			}
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties

		public static PreviewCreator Instance { get { return instance; } }
		
		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		#region IsRunning
		internal bool IsRunning
		{
			get
			{
				lock (runVariableLocker)
				{
					return this.run;
				}
			}
			set
			{
				lock (runVariableLocker)
				{
					this.run = value;
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
			if (isDisposed == false)
			{
				lock (runVariableLocker)
				{
					this.run = false;
				}

				lock (this.queuesLocker)
				{
					priorityQueue.Clear();
					normalQueue.Clear();
					this.waitEvent.Set();
				}

				waitForWorkerThreadToDieEvent.WaitOne();
				this.isDisposed = true;
			}
		}
		#endregion

		#region GetPreview()
		public Bitmap GetPreview(FileInfo file, Scanners.ColorMode colorMode, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			return GetPreview(file, new System.Windows.Rect(0, 0, 1, 1), colorMode, zoom, quality);
		}

		public Bitmap GetPreview(FileInfo file, System.Windows.Rect imageRect, Scanners.ColorMode colorMode, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			if (isDisposed == false)
			{
				lock (threadLocker)
				{
					try
					{
						try
						{
							if (file != this.lastFile)
							{
								if (this.lastBitmap != null)
								{
									this.lastBitmap.Dispose();
									this.lastBitmap = null;
								}

								this.lastFile = file;

								ImageProcessing.ImageFile.ImageInfo imageInfo = new ImageProcessing.ImageFile.ImageInfo(this.lastFile);

								if ((imageInfo.BitsPerPixel / 8) * imageInfo.Width * imageInfo.Height < 60 * 1024 * 1024)
								{
									try
									{
										file.Refresh();
										this.lastBitmap = ImageProcessing.ImageCopier.LoadFileIndependentImage(file.FullName);
									}
									catch
									{
										this.lastBitmap = null;
									}
								}
							}
						}
						catch (Exception ex)
						{
							throw ex;
						}

						if (this.lastBitmap != null)
						{
							switch (colorMode)
							{
								case Scanners.ColorMode.Color:
									try
									{
										return GetPreview24bpp(this.lastBitmap, imageRect, zoom, quality);
									}
									catch (Exception ex)
									{
										try
										{
											this.lastBitmap.Dispose();
										}
										catch{}
										this.lastBitmap = null;
										
										throw ex;
									}
								case Scanners.ColorMode.Grayscale:
									try
									{
										return GetPreview8bpp(this.lastBitmap, imageRect, zoom, quality);
									}
									catch (Exception ex)
									{
										throw ex;
									}
								case Scanners.ColorMode.Bitonal:
									try
									{
										return GetPreview1bpp(this.lastBitmap, imageRect, zoom, quality);
									}
									catch (Exception ex)
									{
										throw ex;
									}
								default:
									Notifications.Instance.Notify(this, Notifications.Type.Error, "PreviewCreator, GetPreview(): Unsupported format", null);
									throw new IllException("Can't create preview image! Unsupported Color Mode");
							}
						}
						else
						{
							using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(file.FullName))
							{
								switch (colorMode)
								{
									case Scanners.ColorMode.Color:
										try
										{
											return GetPreview24bpp(itDecoder, imageRect, zoom, quality);
										}
										catch (Exception ex)
										{
											throw ex;
										}
									case Scanners.ColorMode.Grayscale:
										try
										{
											return GetPreview8bpp(itDecoder, imageRect, zoom, quality);
										}
										catch (Exception ex)
										{
											throw ex;
										}
									case Scanners.ColorMode.Bitonal:
										try
										{
											return GetPreview1bpp(itDecoder, imageRect, zoom, quality);
										}
										catch (Exception ex)
										{
											throw ex;
										}
									default:
										Notifications.Instance.Notify(this, Notifications.Type.Error, "PreviewCreator, GetPreview(): Unsupported format", null);
										throw new IllException("Can't create preview image! Unsupported Color Mode.");
								}
							}
						}
					}
					catch (IllException ex)
					{
						throw ex;
					}
					catch (Exception ex)
					{
						Notifications.Instance.Notify(this, Notifications.Type.Error, "PreviewCreator, GetPreview(): " + ex.Message, ex);
						throw new IllException("Can't create preview image!" + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
					}
				}
			}
			else
				return null;
		}
		#endregion

		#region GetPreviewAsync()
		internal void GetPreviewAsync(BscanILL.IP.IPreviewCaller caller, int renderingId, bool highPriority, FileInfo file, Scanners.ColorMode colorMode, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			GetPreviewAsync(caller, renderingId, highPriority, file, new System.Windows.Rect(0, 0, 1, 1), colorMode, zoom, quality);
		}

		internal void GetPreviewAsync(BscanILL.IP.IPreviewCaller caller, int renderingId, bool highPriority, FileInfo file, System.Windows.Rect imageRect, Scanners.ColorMode colorMode, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			if (isDisposed == false)
			{
				lock (queuesLocker)
				{
					WorkUnit workUnit = new WorkUnit(caller, renderingId, file, imageRect, colorMode, zoom, quality);

					foreach (WorkUnit existingWorkUnit in priorityQueue)
						if (existingWorkUnit.Caller == caller)
							existingWorkUnit.IsCanceled = true;

					foreach (WorkUnit existingWorkUnit in normalQueue)
						if (existingWorkUnit.Caller == caller)
							existingWorkUnit.IsCanceled = true;

					if (highPriority)
						priorityQueue.Enqueue(workUnit);
					else
						normalQueue.Enqueue(workUnit);

					waitEvent.Set();
				}
			}
		}
		#endregion

		#region Reset()
		public void Reset()
		{
			if (isDisposed == false)
			{
				lock (threadLocker)
				{
					if (this.lastBitmap != null)
					{
						this.lastBitmap.Dispose();
						this.lastBitmap = null;
					}

					this.lastFile = null;
				}

				lock (queuesLocker)
				{
					priorityQueue.Clear();
					normalQueue.Clear();
				}
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region CreateImageThread()
		private void CreateImageThread()
		{
			try
			{
				waitForWorkerThreadToDieEvent.Reset();

				while (IsRunning)
				{
					try
					{
						waitEvent.WaitOne();
						WorkUnit workUnit = null;

						lock (queuesLocker)
						{
							if (priorityQueue.Count > 0)
								workUnit = priorityQueue.Dequeue();
							else if (normalQueue.Count > 0)
								workUnit = normalQueue.Dequeue();

							waitEvent.Reset();
						}

						while (IsRunning && (workUnit != null))
						{
							try
							{
								if (workUnit.IsCanceled || workUnit.Caller == null || workUnit.Caller.IsDisplayed == false)
								{
									workUnit.Caller.ThumbnailCanceledDelegate();
								}
								else
								{
									Bitmap bitmap = GetPreview(workUnit.File, workUnit.ImageRect, workUnit.ColorMode, workUnit.Zoom, workUnit.Quality);

									TimeSpan timeSpan = TimeSpan.Zero;
									workUnit.Caller.ThumbnailCreatedDelegate(bitmap, workUnit.RenderingId, timeSpan);
								}
							}
							catch (Exception ex)
							{
								workUnit.Caller.ThumbnailErrorDelegate(ex);
							}

							lock (queuesLocker)
							{
								if (priorityQueue.Count > 0)
									workUnit = priorityQueue.Dequeue();
								else if (normalQueue.Count > 0)
									workUnit = normalQueue.Dequeue();
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
				waitForWorkerThreadToDieEvent.Set();
			}
		}
		#endregion

		#region GetPreview24bpp()
		private Bitmap GetPreview24bpp(ImageProcessing.BigImages.ItDecoder itDecoder, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{	
			Rectangle	clipRect = new Rectangle(Convert.ToInt32(imageRect.X * itDecoder.Width), Convert.ToInt32(imageRect.Y * itDecoder.Height), Convert.ToInt32(imageRect.Width * itDecoder.Width), Convert.ToInt32(imageRect.Height * itDecoder.Height));

			if (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format24bppRgb)
			{
				if (zoom != 1)
					return ImageProcessing.Resizing.Resize(itDecoder.GetImage(), clipRect, zoom, quality);
				else
					return ImageProcessing.ImageCopier.Copy(itDecoder.GetImage(), clipRect);
			}
			else
			{
				if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
				{
					ImageProcessing.BigImages.Resizing resizing = new ImageProcessing.BigImages.Resizing();
					Bitmap crop = resizing.ResizeToBitmap(itDecoder, clipRect, zoom);

					return crop;
				}
				else
				{
					return BscanILL.IP.Resizing.GetBluryPreview(itDecoder, clipRect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
				}
			}
		}
		#endregion

		#region GetPreview8bpp()
		private Bitmap GetPreview8bpp(ImageProcessing.BigImages.ItDecoder itDecoder, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle(Convert.ToInt32(imageRect.X * itDecoder.Width), Convert.ToInt32(imageRect.Y * itDecoder.Height), Convert.ToInt32(imageRect.Width * itDecoder.Width), Convert.ToInt32(imageRect.Height * itDecoder.Height));

			if (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format8bppGray || itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format8bppIndexed)
			{
				if (zoom != 1)
					return ImageProcessing.Resizing.Resize(itDecoder.GetImage(), clipRect, zoom, quality);
				else
					return ImageProcessing.ImageCopier.Copy(itDecoder.GetImage(), clipRect);
			}
			else
			{
				if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
				{
					ImageProcessing.BigImages.ResizingAndResampling resizing = new ImageProcessing.BigImages.ResizingAndResampling();
					Bitmap resized = resizing.ResizeAndResampleToBitmap(itDecoder, clipRect, ImageProcessing.PixelsFormat.Format8bppGray, zoom);

					return resized;
				}
				else
					return BscanILL.IP.Resizing.GetBluryPreview(itDecoder, clipRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			}
		}
		#endregion

		#region GetPreview1bpp()
		private Bitmap GetPreview1bpp(ImageProcessing.BigImages.ItDecoder itDecoder, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle(Convert.ToInt32(imageRect.X * itDecoder.Width), Convert.ToInt32(imageRect.Y * itDecoder.Height), Convert.ToInt32(imageRect.Width * itDecoder.Width), Convert.ToInt32(imageRect.Height * itDecoder.Height));

			if (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite)
			{			
				if (zoom != 1)
					return ImageProcessing.Resizing.Resize(itDecoder.GetImage(), clipRect, zoom, quality);
				else
					return ImageProcessing.ImageCopier.Copy(itDecoder.GetImage(), clipRect);
			}
			else
			{
				if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
				{
					ImageProcessing.Histogram histogram = new ImageProcessing.Histogram(itDecoder, clipRect);
					byte thresholdR = (byte)Math.Max(1, Math.Min(254, (int)(histogram.ThresholdR)));
					byte thresholdG = (byte)Math.Max(1, Math.Min(254, (int)(histogram.ThresholdG)));
					byte thresholdB = (byte)Math.Max(1, Math.Min(254, (int)(histogram.ThresholdG)));
					
					ImageProcessing.BigImages.Binarization.BinarizationParameters parameters = new ImageProcessing.BigImages.Binarization.BinarizationParameters(thresholdR, thresholdG, thresholdB);
					ImageProcessing.BigImages.Binarization binorization = new ImageProcessing.BigImages.Binarization();
					Bitmap bitmap = binorization.ThresholdToBitmap(itDecoder, clipRect, parameters);

					if (zoom != 1)
					{
						Size desiredSize = new Size(Convert.ToInt32(imageRect.Width * itDecoder.Width * zoom), Convert.ToInt32(imageRect.Height * itDecoder.Height * zoom));

						Bitmap preview = ImageProcessing.Resizing.GetThumbnail(bitmap, desiredSize);
						bitmap.Dispose();

						return preview;
					}
					else
					{
						return bitmap;
					}
				}
				else
					return BscanILL.IP.Resizing.GetBluryPreview(itDecoder, clipRect, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
			}
		}
		#endregion

		#region GetPreview24bpp()
		private Bitmap GetPreview24bpp(Bitmap bitmap, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle(Convert.ToInt32(imageRect.X * bitmap.Width), Convert.ToInt32(imageRect.Y * bitmap.Height), Convert.ToInt32(imageRect.Width * bitmap.Width), Convert.ToInt32(imageRect.Height * bitmap.Height));

			if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
			{
				if (zoom != 1)
					return ImageProcessing.Resizing.Resize(bitmap, clipRect, zoom, quality);
				else
					return ImageProcessing.ImageCopier.Copy(bitmap, clipRect);
			}
			else
			{
				if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
				{
					Bitmap crop = ImageProcessing.Resizing.Resize(bitmap, clipRect, zoom, ImageProcessing.Resizing.ResizeMode.Quality);

					return crop;
				}
				else
					return BscanILL.IP.Resizing.GetBluryPreview(bitmap, clipRect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			}
		}
		#endregion

		#region GetPreview8bpp()
		private Bitmap GetPreview8bpp(Bitmap bitmap, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle(Convert.ToInt32(imageRect.X * bitmap.Width), Convert.ToInt32(imageRect.Y * bitmap.Height), Convert.ToInt32(imageRect.Width * bitmap.Width), Convert.ToInt32(imageRect.Height * bitmap.Height));

			if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
			{
				if (zoom != 1)
					return ImageProcessing.Resizing.Resize(bitmap, clipRect, zoom, quality);
				else
					return ImageProcessing.ImageCopier.Copy(bitmap, clipRect);
			}
			else
			{
				if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
				{
					ImageProcessing.BigImages.ResizingAndResampling resizing = new ImageProcessing.BigImages.ResizingAndResampling();
					Bitmap resized = resizing.ResizeAndResampleToBitmap(bitmap, clipRect, ImageProcessing.PixelsFormat.Format8bppGray, zoom);

					return resized;
				}
				else
					return BscanILL.IP.Resizing.GetBluryPreview(bitmap, clipRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
			}
		}
		#endregion

		#region GetPreview1bpp()
		private Bitmap GetPreview1bpp(Bitmap source, System.Windows.Rect imageRect, double zoom, ImageProcessing.Resizing.ResizeMode quality)
		{
			Rectangle clipRect = new Rectangle(Convert.ToInt32(imageRect.X * source.Width), Convert.ToInt32(imageRect.Y * source.Height), Convert.ToInt32(imageRect.Width * source.Width), Convert.ToInt32(imageRect.Height * source.Height));

			if (source.PixelFormat == System.Drawing.Imaging.PixelFormat.Format1bppIndexed)
			{
				if (zoom != 1)
					return ImageProcessing.Resizing.Resize(source, clipRect, zoom, quality);
				else
					return ImageProcessing.ImageCopier.Copy(source, clipRect);
			}
			else
			{
				if (quality == ImageProcessing.Resizing.ResizeMode.Quality)
				{
					ImageProcessing.Histogram histogram = new ImageProcessing.Histogram(source, clipRect);

					byte thresholdR = (byte)Math.Max(1, Math.Min(254, (int)(histogram.ThresholdR)));
					byte thresholdG = (byte)Math.Max(1, Math.Min(254, (int)(histogram.ThresholdG)));
					byte thresholdB = (byte)Math.Max(1, Math.Min(254, (int)(histogram.ThresholdG)));
					
					ImageProcessing.BigImages.Binarization.BinarizationParameters parameters = new ImageProcessing.BigImages.Binarization.BinarizationParameters(thresholdR, thresholdG, thresholdB);
					ImageProcessing.BigImages.Binarization binorization = new ImageProcessing.BigImages.Binarization();
					Bitmap bitmap = binorization.ThresholdToBitmap(source, clipRect, parameters);

					if (zoom != 1)
					{
						Size desiredSize = new Size(Convert.ToInt32(bitmap.Width * zoom), Convert.ToInt32(bitmap.Height * zoom));

						Bitmap preview = ImageProcessing.Resizing.GetThumbnail(bitmap, desiredSize);
						bitmap.Dispose();

						return preview;
					}
					else
					{
						return bitmap;
					}
				}
				else
					return BscanILL.IP.Resizing.GetBluryPreview(source, clipRect, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
			}
		}
		#endregion

		#endregion
	
	}
}
