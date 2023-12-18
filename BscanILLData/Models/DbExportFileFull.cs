using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models
{
	public class DbExportFileFull
	{
		public DbExportFileFull()
		{

		}
		
		//PUBLIC PROPERTIES
		#region public properties

		public virtual int					Id { get; set; }
		public virtual int					fExportId { get; set; }
		public virtual int?					PreviousId { get; set; }
		public virtual int?					NextId { get; set; }
		public virtual string				FileName { get; set; }
		public virtual ColorMode			ColorMode { get; set; }
		public virtual ExportFileFormat		FileFormat { get; set; }
		public virtual short				Dpi { get; set; }
		public virtual short				NumOfImages { get; set; }
		public virtual ExportFileStatus		Status { get; set; }

		public virtual DbExportFull			Export { get; set; }

		#endregion


		//PUBLIC METHODS
		#region public methods

		public override bool Equals(object obj)
		{
			if (this == obj)
				return true;

			if (obj == null)
				return false;

			DbExportFileFull exportFile = (DbExportFileFull)obj;

			if (this.Id != exportFile.Id)
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
