using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using BscanILLData.Models;

namespace BscanILLData.FluentMappings.SQLite
{
	public class DbExportFileFullMap : ClassMap<BscanILLData.Models.DbExportFileFull>
	{
		public DbExportFileFullMap()
		{
			Table("DbExportFiles");
			Id(x => x.Id)
				.Column("pExportFileId");
			Map(x => x.fExportId);
			Map(x => x.PreviousId);
			Map(x => x.NextId);
			Map(x => x.FileName);
			Map(x => x.ColorMode)
				.CustomSqlType("tinyint")
				.CustomType<BscanILLData.Models.ColorMode>();
			Map(x => x.FileFormat)
				.CustomSqlType("tinyint")
				.CustomType<BscanILLData.Models.ScanFileFormat>();
			Map(x => x.Dpi);
			Map(x => x.NumOfImages);
			Map(x => x.Status)
				.CustomSqlType("tinyint")
				.CustomType<BscanILLData.Models.ExportFileStatus>();

			References(x => x.Export)
				.ReadOnly()
				.Column("fExportId")
				.Not.LazyLoad();

			Polymorphism.Explicit();
		}
	}
}
