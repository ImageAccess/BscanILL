using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;

namespace BscanILL.UI.Frames.Resend
{
	#region enum CompareBy
	public enum CompareBy
	{
		Id,
		Tn,
		IllNumber,
		Created,
		LastSent,
		Patron,
        Address,
		DeliveryMethod
	}
	#endregion


	#region class ArticlesLocalComparer
	public class ArticlesLocalComparer : IComparer
	{
		CompareBy compareBy;
		ListSortDirection direction;

		public ArticlesLocalComparer(CompareBy compareBy, ListSortDirection direction)
		{
			this.compareBy = compareBy;
			this.direction = direction;
		}

		public int Compare(object o1, object o2)
		{
			ArticleLocal x = o1 as ArticleLocal;
			ArticleLocal y = o2 as ArticleLocal;
			
			switch (this.compareBy)
			{
				case CompareBy.Id:
					{
						if (direction == ListSortDirection.Ascending)
							return x.Id.CompareTo(y.Id);
						else
							return y.Id.CompareTo(x.Id);
					}
				case CompareBy.Tn:
					{
						if (direction == ListSortDirection.Ascending)
						{
							if (x.Tn == null && y.Tn == null)
								return 0;
							else if (x.Tn == null)
								return -1;
							else if (y.Tn == null)
								return 1;
							else
								return x.Tn.Value.CompareTo(y.Tn.Value);
						}
						else
						{
							if (x.Tn == null && y.Tn == null)
								return 0;
							else if (x.Tn == null)
								return 1;
							else if (y.Tn == null)
								return -1;
							else
								return y.Tn.Value.CompareTo(x.Tn.Value);
						}
					}
				case CompareBy.IllNumber:
					{
						if (x.IllNumber != null && y.IllNumber != null)
						{
							int xNum, yNum;
							bool isXnum = int.TryParse(x.IllNumber, out xNum);
							bool isYnum = int.TryParse(y.IllNumber, out yNum);

							if (isXnum && isYnum)
							{
								if (direction == ListSortDirection.Ascending)
									return xNum.CompareTo(yNum);
								else
									return yNum.CompareTo(xNum);
							}
							else if (isXnum)
							{
								return -1;
							}
							else if (isYnum)
							{
								return 1;
							}
							else
							{
								if (direction == ListSortDirection.Ascending)
									return x.IllNumber.CompareTo(y.IllNumber);
								else
									return y.IllNumber.CompareTo(x.IllNumber);
							}
						}
						else if (x.IllNumber != null)
							return 1;
						else
							return -1;
					}
				case CompareBy.Created:
					{
						if (direction == ListSortDirection.Ascending)
							return x.Created.CompareTo(y.Created);
						else
							return y.Created.CompareTo(x.Created);
					}
				case CompareBy.LastSent:
					{
						if (direction == ListSortDirection.Ascending)
							return x.LastSent.CompareTo(y.LastSent);
						else
							return y.LastSent.CompareTo(x.LastSent);
					}
				case CompareBy.Patron:
					{
						if (direction == ListSortDirection.Ascending)
							return x.Patron.CompareTo(y.Patron);
						else
							return y.Patron.CompareTo(x.Patron);
					}
				case CompareBy.DeliveryMethod:
					{
						if (direction == ListSortDirection.Ascending)
							return x.ExportType.CompareTo(y.ExportType);
						else
							return y.ExportType.CompareTo(x.ExportType);
					}
				default:
					{
						if (direction == ListSortDirection.Ascending)
							return x.Id.CompareTo(y.Id);
						else
							return y.Id.CompareTo(x.Id);
					}
			}
		}
	}
	#endregion
}
