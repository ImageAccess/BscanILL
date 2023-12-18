using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using BscanILLData.Models;

namespace BscanILLData.FluentMappings.SQLite
{
	public class DbExportFullMap : ClassMap<BscanILLData.Models.DbExportFull>
	{
		public DbExportFullMap()
		{
			Table("DbExports");
			Id(x => x.Id)
				.Column("pExportId");
			Map(x => x.fArticleId);
			Map(x => x.ExportType)
				.CustomSqlType("tinyint")
				.CustomType<BscanILLData.Models.ExportType>();
			Map(x => x.ExportDate);
			Map(x => x.FolderName);
			Map(x => x.FileFormat)
				.CustomSqlType("tinyint")
				.CustomType<BscanILLData.Models.ExportFileFormat>();
			Map(x => x.FileNamePrefix);
			Map(x => x.PdfA);
			Map(x => x.MultiImage);
			Map(x => x.Status)
				.CustomSqlType("tinyint")
				.CustomType<BscanILLData.Models.ExportStatus>();
			
			References(x => x.Article)
				.ReadOnly()
				.Column("fArticleId")
				.Not.LazyLoad();

			HasMany(x => x.ExportFiles)
				.KeyColumn("fExportId")
				.Inverse()
				//.Cascade.AllDeleteOrphan()
				.Cascade.All();
			
			 Polymorphism.Explicit();
		}
	}
}
