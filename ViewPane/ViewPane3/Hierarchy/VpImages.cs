using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewPane.Hierarchy
{
	public delegate void ImageAddingEventHnd(VpImage vpImage);
	public delegate void ImageInsertingEventHnd(int index, VpImage vpImage);
	public delegate void ImageRemovingEventHnd(VpImage vpImage);
	public delegate void ClearingEventHnd();
	public delegate void DisposingEventHnd();

	public class VpImages : List<VpImage>
	{
		object locker = new object();

		public event ImageAddingEventHnd	ImageAdding;
		public event ImageAddingEventHnd	ImageAdded;
		public event ImageInsertingEventHnd ImageInserting;
		public event ImageInsertingEventHnd ImageInserted;
		public event ImageRemovingEventHnd	ImageRemoving;
		public event ImageRemovingEventHnd	ImageRemoved;
		public event ClearingEventHnd		Clearing;
		public event ClearingEventHnd		Cleared;


		#region constructor
		public VpImages()
		{
		}
		#endregion

		//	PUBLIC PROPERTIES
		#region public properties

		#region index
		new public VpImage this[int index]
		{
			get
			{
				if(index >= 0 && index < this.Count)
					return (VpImage)base[index];
				else 
					return null ;
			}
		}
		#endregion

		#endregion

		//PUBLIC METHODS
		#region public methods

		#region Add()
		new public void Add(VpImage vpImage)
		{
			lock (this)
			{
				if (ImageAdding != null)
					ImageAdding(vpImage);

				base.Add(vpImage);

				if (ImageAdded != null)
					ImageAdded(vpImage);
			}
		}
		#endregion

		#region Insert()
		new public void Insert(int index, VpImage globalImage)
		{
			lock (this)
			{
				index = Math.Max(0, Math.Min(this.Count, index));

				if (ImageInserting != null)
					ImageInserting(index, globalImage);
				
				base.Insert(index, globalImage);

				if (ImageInserted != null)
					ImageInserted(index, globalImage);
			}
		}
		#endregion
	
		#region Remove()
		new public void Remove(VpImage vpImage)
		{
			if (vpImage != null)
			{
				lock (this.locker)
				{
					if (this.Contains(vpImage))
					{
						if (ImageRemoving != null)
							ImageRemoving(vpImage);

						base.Remove(vpImage);

						if (ImageRemoved != null)
							ImageRemoved(vpImage);

						vpImage.Dispose();
					}
				}
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

				foreach (VpImage image in this)
				{
					image.Dispose();
				}

				base.Clear();

				if (this.Cleared != null)
					Cleared();
			}
		}
		#endregion

		#region ShuffleImage()
		public void ShuffleImage(VpImage vpImage, int index)
		{
			if (this.IndexOf(vpImage) < index)
				index--;
			
			if (ImageRemoving != null)
				ImageRemoving(vpImage);

			base.Remove(vpImage);
			
			if (ImageRemoved != null)
				ImageRemoved(vpImage);

			if (ImageInserting != null)
				ImageInserting(index, vpImage);

			base.Insert(index, vpImage);

			if (ImageInserted != null)
				ImageInserted(index, vpImage);
		}
		#endregion

		#region GetVpImage()
		public VpImage GetVpImage(object tag)
		{
			foreach (VpImage vpImage in this)
				if (vpImage.Tag != null && vpImage.Tag == tag)
					return vpImage;

			return null;
		}
		#endregion

		#region GetVpImage()
		/*public List<VpImage> GetVpImages(int? scanId)
		{
			List<VpImage> vpImages = new List<VpImage>();

			if (scanId != null)
			{
				foreach (VpImage vpImage in this)
				{
					if (vpImage.ScanId == scanId)
						vpImages.Add(vpImage);
				}
			}

			return vpImages;
		}*/
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#endregion

	}
}
