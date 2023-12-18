#define TransNumber_LONG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using BscanILLData.Models;

namespace BscanILLData.FluentMappings.SQLite
{
	public class DbArticleFullMap : ClassMap<BscanILLData.Models.DbArticleFull>
	{
		public DbArticleFullMap()
		{
			Table("DbArticles");
			Id(x => x.Id)
				.Column("pArticleId");

            Map(x => x.TransactionNumber);
#if TransNumber_LONG
            Map(x => x.TransactionNumberBig);
#endif

			Map(x => x.IllNumber);
			Map(x => x.Patron);
			Map(x => x.Address);
			Map(x => x.ExportType)
				.CustomSqlType("tinyint")
				.CustomType<BscanILLData.Models.ExportType>();
			Map(x => x.CreationDate);
			Map(x => x.LastModifiedDate);
			Map(x => x.FolderName);
			Map(x => x.ExportScans);
			Map(x => x.Status)
				.CustomSqlType("tinyint")
				.CustomType<BscanILLData.Models.ArticleStatus>();
			
			HasMany(x => x.Scans)
				.KeyColumn("fArticleId")
				.Inverse()
				//.Cascade.AllDeleteOrphan()
				.Cascade.All();

			HasMany(x => x.Exports)
				.KeyColumn("fArticleId")
				.Inverse()
				//.Cascade.AllDeleteOrphan()
				.Cascade.All();
			
			Polymorphism.Explicit();
		}
	}
}
