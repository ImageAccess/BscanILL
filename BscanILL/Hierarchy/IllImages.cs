using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using ImageProcessing;
using BscanILL.Export.ILL;
using BscanILL.Misc;

namespace BscanILL.Hierarchy
{
	public class IllImages : List<BscanILL.Hierarchy.IllImage>
	{
		ReaderWriterLock			rwLock = new ReaderWriterLock() ;
		object						deletingLocker = new object();

		public event EventHandler					Changed;
		public event BscanILL.Hierarchy.IllImageHnd	ImageSelected;

		public event ImageAddingEventHnd	ImageAdding;
		public event ImageAddingEventHnd	ImageAdded;
		public event ImageInsertingEventHnd ImageInserting;
		public event ImageInsertingEventHnd ImageInserted;
		public event ImageRemovingEventHnd	ImageRemoving;
		public event ImageRemovingEventHnd	ImageRemoved;
		public event ClearingEventHnd		Clearing;
		public event ClearingEventHnd		Cleared;
		//public event DisposingEventHnd		DisposedSuccessfully;
		//public event DisposingEventHnd		DisposedWithErrors;


		#region constructor
		public IllImages()
		{
		}
		#endregion


		//	PUBLIC PROPERTIES
		#region public properties

		#region index
		new public BscanILL.Hierarchy.IllImage this[int index]
		{
			get
			{
				if(index >= 0 && index < this.Count)
					return (BscanILL.Hierarchy.IllImage)base[index];
				else 
					return null ;
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Add()
		public BscanILL.Hierarchy.IllImage Add(Article article, FileInfo filePath, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, ushort dpi, double brightness, double contrast)
		{
			lock (this)
			{				
				IllImage previousIllImage = (article.Scans.Count > 0) ? article.Scans[article.Scans.Count - 1] : null;

				BscanILLData.Models.Helpers.NewDbScan newDbScan = new BscanILLData.Models.Helpers.NewDbScan()
				{
					fArticleId = (int)article.Id,
					PreviousId = (previousIllImage != null) ? (int?)previousIllImage.DbScan.Id : null,
					NextId = null,
					FileName = filePath.Name,
					ColorMode = (BscanILLData.Models.ColorMode)colorMode,
					FileFormat = (BscanILLData.Models.ScanFileFormat)fileFormat,
					Dpi = (short)dpi,
					Status = BscanILLData.Models.ScanStatus.Creating
				};

				BscanILLData.Models.DbScan dbScan = BscanILL.DB.Database.Instance.InsertScan(newDbScan);

				if (previousIllImage != null)
				{
					previousIllImage.DbScan.NextId = dbScan.Id;
					BscanILL.DB.Database.Instance.SaveObject(previousIllImage.DbScan);
				}

				BscanILL.Hierarchy.IllImage illImage = new BscanILL.Hierarchy.IllImage(article, dbScan);
				illImage.ImageSelected += new BscanILL.Hierarchy.IllImageHnd(Image_Selected);

				if (ImageAdding != null)
					ImageAdding(illImage);

				base.Add(illImage);

				if (ImageAdded != null)
					ImageAdded(illImage);

				illImage.Status = ScanStatus.Active;
				return illImage;
			}
		}

		new public void Add(BscanILL.Hierarchy.IllImage illImage)
		{
			lock (this)
			{
				if (ImageAdding != null)
					ImageAdding(illImage);

				base.Add(illImage);
				illImage.ImageSelected += new BscanILL.Hierarchy.IllImageHnd(Image_Selected);

				if (ImageAdded != null)
					ImageAdded(illImage);
			}
		}
		#endregion

		#region Insert()
		public BscanILL.Hierarchy.IllImage Insert(int index, Article article, FileInfo filePath, Scanners.ColorMode colorMode, Scanners.FileFormat fileFormat, ushort dpi, double brightness, double contrast)
		{
			lock (this)
			{
				IllImage previousIllImage = (index > 0 && article.Scans.Count > index) ? article.Scans[index - 1] : null;
				IllImage nextIllImage = (article.Scans.Count > index) ? article.Scans[index] : null;
				index = Math.Max(0, Math.Min(this.Count, index));

				BscanILLData.Models.Helpers.NewDbScan newDbScan = new BscanILLData.Models.Helpers.NewDbScan()
				{
					fArticleId = (int)article.Id,
					PreviousId = (previousIllImage != null) ? (int?)previousIllImage.DbScan.Id : null,
					NextId = (nextIllImage != null) ? (int?)nextIllImage.DbScan.Id : null,
					FileName = filePath.Name,
					ColorMode = (BscanILLData.Models.ColorMode)colorMode,
					FileFormat = (BscanILLData.Models.ScanFileFormat)fileFormat,
					Dpi = (short)dpi,
					Status = BscanILLData.Models.ScanStatus.Creating
				};

				BscanILLData.Models.DbScan dbScan = BscanILL.DB.Database.Instance.InsertScan(newDbScan);

				if (previousIllImage != null)
				{
					previousIllImage.DbScan.NextId = dbScan.Id;
					BscanILL.DB.Database.Instance.SaveObject(previousIllImage.DbScan);
				}

				if (nextIllImage != null)
				{
					nextIllImage.DbScan.PreviousId = dbScan.Id;
					BscanILL.DB.Database.Instance.SaveObject(nextIllImage.DbScan);
				}

				BscanILL.Hierarchy.IllImage globalImage = new BscanILL.Hierarchy.IllImage(article, dbScan);
				globalImage.ImageSelected += new BscanILL.Hierarchy.IllImageHnd(Image_Selected);
				
				if (ImageInserting != null)
					ImageInserting(index, globalImage);

				base.Insert(index, globalImage);

				if (ImageInserted != null)
					ImageInserted(index, globalImage);

				globalImage.Status = ScanStatus.Active;
				return globalImage;
			}
		}

		new public void Insert(int index, BscanILL.Hierarchy.IllImage globalImage)
		{
			lock (this)
			{
				index = Math.Max(0, Math.Min(this.Count, index));

				if (ImageInserting != null)
					ImageInserting(index, globalImage);
				
				base.Insert(index, globalImage);
				globalImage.ImageSelected += new BscanILL.Hierarchy.IllImageHnd(Image_Selected);

				if (ImageInserted != null)
					ImageInserted(index, globalImage);
			}
		}
		#endregion
	
		#region Remove()
		new public void Remove(BscanILL.Hierarchy.IllImage illImage)
		{
			if (illImage != null)
			{
				rwLock.AcquireWriterLock(Timeout.Infinite);

				try
				{
					if (this.Contains(illImage))
					{
						int index = this.IndexOf(illImage);

						if (index >= 0)
						{
							IllImage previousIllImage = (index > 0 && this.Count > index) ? this[index - 1] : null;
							IllImage nextIllImage = (this.Count > (index + 1)) ? this[index + 1] : null;

							if( (previousIllImage != null) && (nextIllImage != null))
                            {
								previousIllImage.DbScan.NextId = nextIllImage.DbScan.Id;
								nextIllImage.DbScan.PreviousId = previousIllImage.DbScan.Id;
								BscanILL.DB.Database.Instance.SaveObject(previousIllImage.DbScan);
								BscanILL.DB.Database.Instance.SaveObject(nextIllImage.DbScan);
							}
							else if (previousIllImage != null)
							{
								previousIllImage.DbScan.NextId = null;
								BscanILL.DB.Database.Instance.SaveObject(previousIllImage.DbScan);
							}
							else if (nextIllImage != null)
							{
								nextIllImage.DbScan.PreviousId = null;
								BscanILL.DB.Database.Instance.SaveObject(nextIllImage.DbScan);
							}

							if (ImageRemoving != null)
								ImageRemoving(illImage);

							IllImages illImages = new IllImages();
							illImage.ImageSelected -= new BscanILL.Hierarchy.IllImageHnd(Image_Selected);
							illImages.Add(illImage);
							base.Remove(illImage);

							if (ImageRemoved != null)
								ImageRemoved(illImage);

							Thread t = new Thread(new ParameterizedThreadStart(ClearTU));
							t.Name = "ThreadIllImages_Clear1";
							t.SetApartmentState(ApartmentState.STA);
							t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
							t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
							t.Start(illImages);
						}
					}
				}
				finally
				{
					rwLock.ReleaseWriterLock();
				}

				if (Changed != null)
					Changed(null, null);
			}
		}
		#endregion

		#region Clear()
		new public void Clear()
		{
			lock (this)
			{
				if (this.Clearing != null)
					Clearing();

				IllImages illImages = new IllImages();
				foreach (BscanILL.Hierarchy.IllImage illImage in this)
				{
					illImage.ImageSelected -= new BscanILL.Hierarchy.IllImageHnd(Image_Selected);
					illImages.Add(illImage);
				}

				base.Clear();

				if (this.Cleared != null)
					Cleared();

				Thread t = new Thread(new ParameterizedThreadStart(ClearTU));
				t.Name = "ThreadIllImages_Clear2";
				t.SetApartmentState(ApartmentState.STA);
				t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				t.Start(illImages);
			}

			if (Changed != null)
				Changed(null, null);
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			/*lock (this)
			{
				if (this.Clearing != null)
					Clearing();

				IllImages illImages = new IllImages();
				foreach (IllImage illImage in this)
				{
					illImage.ImageSelected -= new BscanILL.Hierarchy.IllImageHnd(Image_Selected);
					illImages.Add(illImage);
				}

				base.Clear();

				if (this.Cleared != null)
					Cleared();
				
				Thread t = new Thread(new ParameterizedThreadStart(DisposeTU));
				t.Name = "ThreadIllImages_Dispose";
				t.SetApartmentState(ApartmentState.STA);
				t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				t.Start(illImages);
			}

			if (Changed != null)
				Changed(null, null);*/
		}
		#endregion

		#region ShuffleIllImage()
		public void ShuffleIllImage(BscanILL.Hierarchy.IllImage illImage, int index)
		{
			if (this.IndexOf(illImage) < index)
				index--;
			
			if (ImageRemoving != null)
				ImageRemoving(illImage);

			base.Remove(illImage);
			
			if (ImageRemoved != null)
				ImageRemoved(illImage);

			if (ImageInserting != null)
				ImageInserting(index, illImage);

			base.Insert(index, illImage);

			if (ImageInserted != null)
				ImageInserted(index, illImage);
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region Image_Selected()
		private void Image_Selected(BscanILL.Hierarchy.IllImage illImage)
		{
			if(ImageSelected != null)
				ImageSelected(illImage);
		}
		#endregion

		#region ClearTU()
		public void ClearTU(object obj)
		{
			lock (deletingLocker)
			{
				try
				{
					foreach (BscanILL.Hierarchy.IllImage image in (IllImages)obj)
					{
						image.Status = ScanStatus.Deleted;
						image.FilesStatus = IllImage.IllImageFilesStatus.Deleting;
					}

					foreach (BscanILL.Hierarchy.IllImage image in (IllImages)obj)
					{
						image.Dispose();
						image.FilesStatus = IllImage.IllImageFilesStatus.Deleted;
					}
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "IllImages, ClearTU(): " + ex.Message, ex);
				}
			}
		}
		#endregion
	
		#region DisposeTU()
		/*public void DisposeTU(object obj)
		{
			lock (deletingLocker)
			{
				try
				{
					foreach (BscanILL.Hierarchy.IllImage image in (IllImages)obj)
						image.FilesStatus = IllImage.IllImageFilesStatus.Deleting;

					foreach (BscanILL.Hierarchy.IllImage image in (IllImages)obj)
						image.Dispose();

					if (DisposedSuccessfully != null)
						DisposedSuccessfully();
				}
				catch (IllException ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "IllImages, DisposeTU(): " + ex.Message, ex);

					if (DisposedWithErrors != null)
						DisposedWithErrors();
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "IllImages, DisposeTU(): " + ex.Message, ex);

					if (DisposedWithErrors != null)
						DisposedWithErrors();
				}
			}
		}*/
		#endregion

		#endregion
	
	}
}
