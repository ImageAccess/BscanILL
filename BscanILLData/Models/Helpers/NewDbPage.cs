using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models.Helpers
{
	public class NewDbPage
	{
		public NewDbPage()
		{

		}
		
		//PUBLIC PROPERTIES
		#region public properties

		public virtual int				fScanId { get; set; }
		public virtual int?				PreviousId { get; set; }
		public virtual int?				NextId { get; set; }
		public virtual string			FileName { get; set; }
		public virtual ColorMode		ColorMode { get; set; }
		public virtual ScanFileFormat	FileFormat { get; set; }
		public virtual short			Dpi { get; set; }
		public virtual PageStatus		Status { get; set; }

		#endregion

	}
}
