using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models
{
	public class DbExport
	{
		public DbExport()
		{

		}
		
		//PUBLIC PROPERTIES
		#region public properties

		public virtual int				Id { get; set; }
		public virtual int				fArticleId { get; set; }
		public virtual ExportType		ExportType { get; set; }
		public virtual DateTime			ExportDate { get; set; }
		public virtual string			FolderName { get; set; }
		public virtual ExportFileFormat FileFormat { get; set; }
		public virtual string			FileNamePrefix { get; set; }
		public virtual bool				PdfA { get; set; }
		public virtual bool				MultiImage { get; set; }
		public virtual ExportStatus		Status { get; set; }

		#endregion


		//PUBLIC METHODS
		#region public methods

		public override bool Equals(object obj)
		{
			if (this == obj)
				return true;

			if (obj == null)
				return false;

			DbExport export = (DbExport)obj;

			if (this.Id != export.Id)
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
