using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections;


namespace BscanILL.Export.ILL
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Odyssey : ExportBasics
	{

		#region constructor
		public Odyssey()
		{
		}
		#endregion

		#region destructor
		public void Dispose()
		{
		}
		#endregion

		//PUBLIC METHODS
		#region public methods

        #region ExportArticle()
		public void ExportArticle(ExportUnit exportUnit)
		{
			string id = (exportUnit.Article.TransactionId.HasValue ? exportUnit.Article.TransactionId.Value.ToString() : exportUnit.Article.IllNumber);
			FileInfo source = exportUnit.Files[0];
			BscanILL.Export.AdditionalInfo.AdditionalOdyssey additional = (BscanILL.Export.AdditionalInfo.AdditionalOdyssey)exportUnit.AdditionalInfo;
			//FileInfo dest = new FileInfo(_settings.Export.Odyssey.ExportDir.FullName + @"\" + additional.FileName);
            FileInfo dest = new FileInfo(_settings.Export.Odyssey.ExportDir.FullName + @"\" + exportUnit.Files[0].Name);            
			
            Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via Odyssey---");
            Progress_Changed(exportUnit, 5, "Checking...");

			if (dest.Exists)
				dest.Delete();			

            Progress_Changed(exportUnit, 25, "Copying file " + dest.Name + " to Odyssey directory...");
			dest.Directory.Create();
			source.CopyTo(dest.FullName);

			Progress_Changed(exportUnit, 100, "File copied successfully.");
		}
        #endregion

        #endregion

		//PRIVATE METHODS
		#region private methods

		#endregion
	}
}
