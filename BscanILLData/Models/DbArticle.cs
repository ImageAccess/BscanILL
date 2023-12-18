#define TransNumber_LONG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BscanILLData.Models
{
	public class DbArticle
	{
		public DbArticle()
		{

		}
		
		//PUBLIC PROPERTIES
		#region public properties

		public virtual int				Id { get; set; }

        public virtual int?				TransactionNumber { get; set; }
#if TransNumber_LONG
        public virtual long?            TransactionNumberBig { get; set; }        
#endif       
		public virtual string			IllNumber { get; set; }
		public virtual string			Patron { get; set; }
		public virtual string			Address { get; set; }
		public virtual ExportType		ExportType { get; set; }
		public virtual DateTime			CreationDate { get; set; }
		public virtual DateTime			LastModifiedDate { get; set; }
		public virtual string			FolderName { get; set; }
		public virtual bool				ExportScans { get; set; }
		public virtual ArticleStatus	Status { get; set; }

		#endregion


		//PUBLIC METHODS
		#region public methods

		public override bool Equals(object obj)
		{
			if (this == obj)
				return true;

			if (obj == null)
				return false;

			DbArticle article = (DbArticle)obj;

			if (this.Id != article.Id)
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
