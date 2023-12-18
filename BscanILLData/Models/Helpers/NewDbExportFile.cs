using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models.Helpers
{
	public class NewDbExportFile
	{
		public NewDbExportFile()
		{

		}
		
		//PUBLIC PROPERTIES
		#region public properties

		public virtual int					fExportId { get; set; }
		public virtual int?					PreviousId { get; set; }
		public virtual int?					NextId { get; set; }
		public virtual string				FileName { get; set; }
		public virtual ColorMode			ColorMode { get; set; }
		public virtual ExportFileFormat		FileFormat { get; set; }
		public virtual short				Dpi { get; set; }
		public virtual short				NumOfImages { get; set; }
		public virtual ExportFileStatus		Status { get; set; }

		#endregion	
	
	}
}
