using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models
{
	public class DbPageFull
	{
		public DbPageFull()
		{

		}
		
		//PUBLIC PROPERTIES
		#region public properties

		public virtual int				Id { get; set; }
		public virtual int				fScanId { get; set; }
		public virtual int?				PreviousId { get; set; }
		public virtual int?				NextId { get; set; }
		public virtual string			FileName { get; set; }
		public virtual ColorMode		ColorMode { get; set; }
		public virtual ScanFileFormat	FileFormat { get; set; }
		public virtual short			Dpi { get; set; }
		public virtual PageStatus		Status { get; set; }

		public virtual DbScanFull		Scan { get; set; }

		#endregion


		//PUBLIC METHODS
		#region public methods

		public override bool Equals(object obj)
		{
			if (this == obj)
				return true;

			if (obj == null)
				return false;

			DbPageFull page = (DbPageFull)obj;

			if (this.Id != page.Id)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			return this.Id.GetHashCode() * 23;
		}

		#endregion
	
	}
}
