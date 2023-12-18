#define TransNumber_LONG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models.Helpers
{
	public class NewDbArticle
	{

		public NewDbArticle()
		{ }

		//PUBLIC PROPERTIES
		#region public properties

        public virtual int?				TransactionNumber { get; set; }
#if TransNumber_LONG
        public virtual long?            TransactionNumberBig { get; set; }        
#endif       
		public virtual string			IllNumber { get; set; }
		public virtual string			Patron { get; set; }
		public virtual string			Address { get; set; }
		public virtual ExportType		ExportType { get; set; }
		public virtual string			FolderName { get; set; }
		public virtual bool				ExportScans { get; set; }
		public virtual ArticleStatus	Status { get; set; }

		#endregion

	}
}
