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
	public class IllPages : List<BscanILL.Hierarchy.IllPage>
	{
		ReaderWriterLock			rwLock = new ReaderWriterLock() ;
		object						deletingLocker = new object();

		public event EventHandler					Changed;
		public event BscanILL.Hierarchy.IllPageHnd	ImageSelected;

		public event IllPageAddingEventHnd		ImageAdding;
		public event IllPageAddingEventHnd		ImageAdded;
		public event ClearingEventHnd			Clearing;
		public event ClearingEventHnd			Cleared;
		//public event DisposingEventHnd			DisposedSuccessfully;
		//public event DisposingEventHnd			DisposedWithErrors;


		#region constructor
		public IllPages()
		{
		}
		#endregion


		//	PUBLIC PROPERTIES
		#region public properties

		#region index
		new public BscanILL.Hierarchy.IllPage this[int index]
		{
			get
			{
				if(index >= 0 && index < this.Count)
					return (BscanILL.Hierarchy.IllPage)base[index];
				else 
					return null ;
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Add()
		public BscanILL.Hierarchy.IllPage Add(IllImage illImage, FileInfo filePath, Scanners.FileFormat fileFormat, Scanners.ColorMode colorMode, ushort dpi, double brightness, double contrast)
		{
			IllPage previousIllPage = (illImage.IllPages.Count > 0) ? illImage.IllPages[illImage.IllPages.Count - 1] : null;
		
			lock (this)
			{
				BscanILLData.Models.Helpers.NewDbPage newDbPage = new BscanILLData.Models.Helpers.NewDbPage()
				{
					fScanId = illImage.DbScan.Id,
 					PreviousId = (previousIllPage != null) ? (int?)previousIllPage.DbPage.Id : null,
					NextId = null,
					FileName = filePath.Name,
					ColorMode = (BscanILLData.Models.ColorMode)colorMode,
					FileFormat = (BscanILLData.Models.ScanFileFormat) fileFormat,
					Dpi = (short)dpi,
					Status = BscanILLData.Models.PageStatus.Creating
				};

				BscanILLData.Models.DbPage dbPage = BscanILL.DB.Database.Instance.InsertPage(newDbPage);

				if (previousIllPage != null)
				{
					previousIllPage.DbPage.NextId = dbPage.Id;
					BscanILL.DB.Database.Instance.SaveObject(previousIllPage.DbPage);
				}

				BscanILL.Hierarchy.IllPage illPage = new BscanILL.Hierarchy.IllPage(illImage, dbPage, true);  //set flag to run Ocr to create ocr data xml files
				illPage.ImageSelected += new BscanILL.Hierarchy.IllPageHnd(Image_Selected);

				illPage.Status = PageStatus.Active;
				
				if (ImageAdding != null)
					ImageAdding(illPage);

				base.Add(illPage);

				if (ImageAdded != null)
					ImageAdded(illPage);

				return illPage;
			}
		}

		new public void Add(BscanILL.Hierarchy.IllPage illPage)
		{
			lock (this)
			{
				if (ImageAdding != null)
					ImageAdding(illPage);

				base.Add(illPage);
				illPage.ImageSelected += new BscanILL.Hierarchy.IllPageHnd(Image_Selected);

				if (ImageAdded != null)
					ImageAdded(illPage);
			}
		}
		#endregion
	
		#region Clear()
		public void Clear(bool deleteOnBackground)
		{
			lock (this)
			{
				if (this.Clearing != null)
					Clearing();
				
				IllPages illPages = new IllPages();
				foreach (BscanILL.Hierarchy.IllPage illPage in this)
				{
					BscanILL.DB.Database.Instance.DeletePage(illPage.DbPage);
					
					illPage.ImageSelected -= new BscanILL.Hierarchy.IllPageHnd(Image_Selected);
					illPages.Add(illPage);
				}

				if (this.Cleared != null)
					Cleared();

				if (deleteOnBackground)
				{
					Thread t = new Thread(new ParameterizedThreadStart(ClearTU));
					t.Name = "ThreadIllPages_Clear2";
					t.SetApartmentState(ApartmentState.STA);
					t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
					t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
					t.Start(illPages);
				}
				else
					ClearTU(illPages);
			}

			if (Changed != null)
				Changed(null, null);
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Image_Selected()
		private void Image_Selected(BscanILL.Hierarchy.IllPage illPage)
		{
			if(ImageSelected != null)
				ImageSelected(illPage);
		}
		#endregion

		#region ClearTU()
		public void ClearTU(object obj)
		{
			lock (deletingLocker)
			{
				try
				{
					foreach (BscanILL.Hierarchy.IllPage image in (IllPages)obj)
					{
						image.Status = PageStatus.Deleted;
						image.FilesStatus = IllPage.IllImageFilesStatus.Deleting;
					}

					foreach (BscanILL.Hierarchy.IllPage image in (IllPages)obj)
						image.Delete();
				}
				catch (Exception ex)
				{
					Notifications.Instance.Notify(this, Notifications.Type.Error, "IllPages, ClearTU(): " + ex.Message, ex);
				}
			}
		}
		#endregion
	
		#endregion
	
	}
}
