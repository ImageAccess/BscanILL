using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BscanILL.Export.ILL.PullslipReader
{
	class BookPullslipParser
	{
		string course;
		string author;

		private BookPullslipParser(string course, string author)
		{
			this.course = course;
			this.author = author;
		}

		public string Course { get { return course; } }
		public string Author { get { return author; } }

		public static IPullslip[] Read(FileInfo file)
		{
			using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					Pullslip pullslip = new Pullslip();
					string fileString = reader.ReadToEnd();

					pullslip.Article = ReadField(fileString, "Course Number");
					pullslip.Author = ReadField(fileString, "Book Author");

					if (pullslip.Article.Length > 0 || pullslip.Author.Length > 0)
						return new IPullslip[] { pullslip };
					else
						return null;
				}
			}
		}

		public static string ReadField(string entireFile, string fieldName)
		{
			int indexField = entireFile.IndexOf(fieldName);

			int indexFieldValueStart = entireFile.IndexOf((char)0x07, indexField) + 1;
			int indexFieldValueStop = entireFile.IndexOf((char)0x07, indexFieldValueStart + 1);

			return entireFile.Substring(indexFieldValueStart, indexFieldValueStop - indexFieldValueStart);
		}

	}
}
