using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace BscanILL.Export.ILL.PullslipReader
{
	public class MainClass
	{
		public static IPullslip[] ReadPullslips(FileInfo file)
		{
			file.Refresh();
			if (file.Exists == false)
				throw new Exception("File '" + file.Name + "' doesn't exist!");

			switch (GetPullslipType(file))
			{
				case PullslipType.ILLiad: return WordXmlParserUniversal.Read(file);
				case PullslipType.Rapid: return RapidParser.Read(file);
				case PullslipType.JournalArticle: return JournalPullslipParser.Read(file);
				case PullslipType.BookChapter: return BookPullslipParser.Read(file);
				default: throw new Exception("File '" + file.Name + "' is in unexpected format!");
			}
		}

		private static PullslipType GetPullslipType(FileInfo file)
		{
			if (file.Extension.ToLower() == ".xml")
				return PullslipType.ILLiad;
			
			using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string fileString = reader.ReadToEnd();

					if (fileString.Contains(" Rapid #: -"))
						return PullslipType.Rapid;
					if (fileString.IndexOf("Pullslip for Journal Article") >= 0)
						return PullslipType.JournalArticle;
					else if (fileString.IndexOf("Pullslip for Book") >= 0)
						return PullslipType.BookChapter;

					return PullslipType.Unknown;
				}
			}
		}

		/*public static IPullslip ReadPullslip(FileInfo file)
		{
			file.Refresh();
			if (file.Exists == false)
				throw new Exception("File '" + file.Name + "' doesn't exist!");

			switch (GetPullslipType(file))
			{
				case PullslipType.JournalArticle: return JournalPullslip.Read(file);
				case PullslipType.BookChapter: return BookPullslipParser.Read(file);
				default: throw new Exception("File '" + file.Name + "' is in unexpected format!");
			}
		}*/

		/*private static PullslipType GetPullslipType(FileInfo file)
		{
			using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string fileString = reader.ReadToEnd();

					if (fileString.IndexOf("Pullslip for Journal Article") >= 0)
						return PullslipType.JournalArticle;
					if (fileString.IndexOf("Pullslip for Book") >= 0)
						return PullslipType.BookChapter;

					return PullslipType.Unknown;
				}
			}
		}*/
	}

}
