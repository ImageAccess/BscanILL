using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models.Helpers
{
	public class NewDbExport
	{
		public NewDbExport()
		{

		}
		
		//PUBLIC PROPERTIES
		#region public properties

		public virtual int				fArticleId { get; set; }
		public virtual ExportType		ExportType { get; set; }
		public virtual string			FolderName { get; set; }
		public virtual ExportFileFormat FileFormat { get; set; }
		public virtual string			FileNamePrefix { get; set; }
		public virtual bool				PdfA { get; set; }
		public virtual bool				MultiImage { get; set; }
		public virtual ExportStatus		Status { get; set; }

		#endregion
	
	}
}
