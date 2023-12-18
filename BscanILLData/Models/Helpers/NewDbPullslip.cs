using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models.Helpers
{
	public class NewDbPullslip
	{
		public NewDbPullslip()
		{

		}

		//PUBLIC PROPERTIES
		#region public properties

		public virtual int				fArticleId { get; set; }
		public virtual string			FileName { get; set; }
		public virtual ColorMode		ColorMode { get; set; }
		public virtual ScanFileFormat	FileFormat { get; set; }
		public virtual short			Dpi { get; set; }

		#endregion
	}
}
