using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using BscanILLData.Models;

namespace BscanILLData.FluentMappings.SQLite
{
	public class DbScanMap : ClassMap<BscanILLData.Models.DbScan>
	{
		public DbScanMap()
		{
			Table("DbScans");
			Id(x => x.Id)
				.Column("pScanId");
			Map(x => x.fArticleId);
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
			Map(x => x.Status)
				.CustomSqlType("tinyint")
				.CustomType<BscanILLData.Models.ScanStatus>();
		}
	}
}
