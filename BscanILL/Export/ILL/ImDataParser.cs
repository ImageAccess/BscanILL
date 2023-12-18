using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BscanILL.Hierarchy;

namespace BscanILL.Export.ILL
{
	/// <summary>
	/// Summary description for ImDataParser.
	/// </summary>
	public class ImDataParser
	{
		#region constructor
		private ImDataParser()
		{
		}
		#endregion

		#region Parse()
		public static List<Article> Parse(FileInfo imData)
		{
			List<Article> articles = new List<Article>();
			
			/*using(StreamReader reader = new StreamReader(imData.FullName))
			{
				string	line;
				
				while( (line = reader.ReadLine()) != null)
				{
					if(line.Length > 0)
					{
						articles.Add(new Article(line));
					}
				}
			}*/

			return articles;
		}
		#endregion

		#region GetExportType()
		public static ExportType GetExportType(string exportStr)
		{
			switch(exportStr.ToLower())
			{
				case "ariel" : return ExportType.Ariel;
				case "ariel to patron" : return ExportType.ArielPatron;
				case "odyssey" : return ExportType.Odyssey;
				case "email": return ExportType.Email;
				case "e-mail": return ExportType.Email;
				case "email+ftp": return ExportType.Ftp;
				case "save on disk": return ExportType.SaveOnDisk;
				case "illiad": return ExportType.ILLiad;
				default: return ExportType.Ariel;
				//default: throw new IllException(ErrorCode.UnexpectedExportType, "Unexpected export type string: " + exportStr);
			}
		}
		#endregion

		#region GetExportTypeString()
		/*public static string GetExportTypeString(ExportType exportType)
		{
			switch(exportType)
			{
				case ExportType.Ariel : return "Ariel";
				case ExportType.ArielPatron : return "Ariel to Patron";
				case ExportType.Odyssey : return "Odyssey";
				case ExportType.Email : return "Email";
				case ExportType.Ftp: return "Email+Ftp";
				case ExportType.SaveOnDisk : return "Save on Disk";
				case ExportType.ILLiad : return "Illiad";
				default: throw new IllException(ErrorCode.UnexpectedExportType, "Unexpected export type string: " + exportType.ToString());
			}
		}*/
		#endregion

	}
}
