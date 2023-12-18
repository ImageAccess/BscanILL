using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Packaging;

namespace BscanILL.Misc
{
	class Zipping
	{
		// PUBLIC METHODS
		#region public methods

		#region Zip()
		internal static void Zip(FileInfo[] files, FileInfo packagePath)
		{

			using (ZipPackage package = (ZipPackage)ZipPackage.Open(packagePath.FullName, FileMode.Create))
			{
				foreach (FileInfo file in files)
				{
					Uri partUriDocument = PackUriHelper.CreatePartUri(new Uri(file.Name, UriKind.Relative));

					// Add the Document part to the Package
					PackagePart packagePartDocument = package.CreatePart(partUriDocument, System.Net.Mime.MediaTypeNames.Application.Zip, CompressionOption.Normal);

					// Copy the data to the Document Part
					using (FileStream fileStream = file.OpenRead())
					{
						CopyStream(fileStream, packagePartDocument.GetStream());
					}
				}
			}
		}
		#endregion

		#region UnZip()
		internal static void UnZip(FileInfo packagePath, DirectoryInfo dir)
		{
			dir.Create();

			using (ZipPackage package = (ZipPackage)ZipPackage.Open(packagePath.FullName, FileMode.Open, FileAccess.Read))
			{
				foreach (PackagePart part in package.GetParts())
				{
					FileInfo file = new FileInfo(dir.FullName + @"\" + CreateFilenameFromUri(part.Uri));

					using (Stream stmSource = part.GetStream(FileMode.Open, FileAccess.Read))
					{
						using (Stream stmDestination = file.OpenWrite())
						{
							CopyStream(stmSource, stmDestination);
						}
					}

					Uri partUriDocument = PackUriHelper.CreatePartUri(new Uri(file.Name, UriKind.Relative));
				}
			}
		}
		#endregion


		#endregion

		// PRIVATE METHODS
		#region private methods

		#region CreateFilenameFromUri()
		private static string CreateFilenameFromUri(Uri uri)
		{
			return uri.ToString().Replace("/", "");
		}
		#endregion

		#region CopyStream()
		private static void CopyStream(Stream source, Stream target)
		{
			const int bufSize = 0x1000;
			byte[] buf = new byte[bufSize];
			int bytesRead = 0;

			while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
				target.Write(buf, 0, bytesRead);
		}
		#endregion

		#endregion
	
	}
}
