using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using System.IO;

namespace BscanILL.Export.ILL.PullslipReader
{
	class RapidParser
	{

		#region Read()
		public static Pullslip[] Read(FileInfo file)
		{
			ArrayList requestsPulledFromFile = new ArrayList();
			String line = "";
			ArrayList lines = new ArrayList();

			using (StreamReader streamReader = new StreamReader(file.FullName))
			{
				while ((line = streamReader.ReadLine()) != null && line.Contains(" Rapid #: -") == false)
				{
				}

				while (line != null)
				{
					lines.Add(line);

					while ((line = streamReader.ReadLine()) != null && line.Contains(" Rapid #: -") == false)
						lines.Add(line);

					Pullslip pullslip = ReadPullslip((string[])lines.ToArray(typeof(string)));

					if(pullslip != null)
						requestsPulledFromFile.Add(pullslip);

					lines.Clear();
				}
			}

			return (Pullslip[])requestsPulledFromFile.ToArray(typeof(Pullslip));
		}
		#endregion

		#region ReadPullslip()
		public static Pullslip ReadPullslip(string[] lines)
		{
			Pullslip pullslip = null;

			if (lines.Length > 0)
			{
				pullslip = new Pullslip();
				pullslip.TransactionId = Convert.ToInt32(GetItemValue(lines[0])).ToString();
				pullslip.IllId = pullslip.TransactionId;
				pullslip.ExportType = ExportType.Ariel;

				foreach (string linefromList in lines)
				{
					string line = linefromList.Trim();

					if (line.ToLower().StartsWith("ariel  ip:"))
						pullslip.Address = GetItemValue(line);
					else if (line.ToLower().StartsWith("journal title:"))
						pullslip.Journal = GetItemValue(line);
					else if (line.ToLower().StartsWith("article title:"))
						pullslip.Article = GetItemValue(line);
					else if (line.ToLower().StartsWith("article author:"))
						pullslip.Author = GetItemValue(line);
					else if (line.ToLower().StartsWith("year:"))
						pullslip.Year = GetItemValue(line);
					else if (line.ToLower().StartsWith("volume:"))
						pullslip.Volume = GetItemValue(line);
					else if (line.ToLower().StartsWith("issue:"))
						pullslip.Issue = GetItemValue(line);
					else if (line.ToLower().StartsWith("pages:"))
						pullslip.Pages = GetItemValue(line);
					else if (line.ToLower().StartsWith("location"))
						pullslip.Location = GetItemValue(line);
					else if (line.ToLower().StartsWith("borrower:"))
						pullslip.Borrower = GetItemValue(line);
					else if (line.ToLower().StartsWith("patron:"))
						pullslip.Patron = GetItemValue(line);
					else if (line.ToLower().StartsWith("call #:"))
						pullslip.CatalogNumber = GetItemValue(line);
				}
			}

			if (pullslip.TransactionId.Length > 0)
				return pullslip;
			else
				return null;
		}
		#endregion

		#region GetItemValue()
		public static string GetItemValue(string line)
		{
			line = line.Trim();
			int colonIndex = line.IndexOf(":");
			if (colonIndex >= 0 && line.Length > colonIndex + 1)
				line = line.Substring(colonIndex + 1);
			else
				line = "";

			return line.Trim();
		}
		#endregion

	}
}
