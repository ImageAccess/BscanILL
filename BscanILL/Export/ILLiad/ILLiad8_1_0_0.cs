using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using BscanILL.Hierarchy;

namespace BscanILL.Export.ILLiad
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ILLiad8_1_0_0 : ILLiadBasics, IILLiad
	{
		IDatabaseSQL db = null;

		#region constructor
		internal ILLiad8_1_0_0()
			: base()
		{
			if (_settings.Export.ILLiad.SqlEnabled)
			{
				try
				{
					db = new DatabaseSQL8_1();
					db.ProgressChanged += new ProgressChangedHandle(this.Progress_Changed);
					db.ProgressComment += new ProgressCommentHandle(this.Progress_Comment);

					db.Login();
					this.sqlEnabled = true;

					Progress_Changed(null, -1, "Login to the SQL database was successfull.");
				}
				catch (Exception ex)
				{
					Progress_Changed(null, -1, "Login to the SQL database was unsuccessfull! " + ex.Message);
					throw new Exception("Can't login to the ILLiad SQL Server! " + ex.Message);
				}
			}
			else
			{
				Progress_Changed(null, -1, "SQL database checking is disabled.");
			}
		}
		#endregion

		#region destructor
		public override void Dispose()
		{
			if (db != null)
			{
				db.Logout();
				db = null;
			}

			base.Dispose();
		}
		#endregion

		//PUBLIC METHODS
		#region public methods

		#region GetSqlTransaction()
		public override BscanILL.Export.ILL.TransactionPair GetSqlTransaction(int transactionId)
		{
			if (SqlEnabled)
			{
				return db.GetRequest(transactionId);
			}
			else
			{
				throw new Exception("SQL Database checking is not set well in settings!");
			}
		}
		#endregion

		#region GetSqlTransactionFromIllNumber()
		public override BscanILL.Export.ILL.TransactionPair GetSqlTransactionFromIllNumber(string illNumber)
		{
			if (SqlEnabled)
			{
				return db.GetRequestFromIllNumber(illNumber);
			}
			else
			{
				throw new Exception("SQL Database checking is not set well in settings!");
			}
		}
		#endregion

		#region ExportArticle()
		public override void ExportArticle(ExportUnit exportUnit)
		{
			Article article = exportUnit.Article;
			FileInfo source = exportUnit.Files[0];
			BscanILL.Export.AdditionalInfo.AdditionalIlliad additional = (BscanILL.Export.AdditionalInfo.AdditionalIlliad)exportUnit.AdditionalInfo;
			//FileInfo dest = new FileInfo(_settings.Export.ILLiad.OdysseyHelperDir + @"\" + additional.FileName);
            FileInfo dest = new FileInfo(_settings.Export.ILLiad.OdysseyHelperDir + @"\" + exportUnit.Files[0].Name);            

			//Progress_Changed(exportUnit, 10, "");
            Progress_Changed(exportUnit, 10, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via ILLiad---");

			if (dest.Exists)
				dest.Delete();

			Progress_Changed(exportUnit, 25, "");

			Progress_Comment(exportUnit, "Copying file " + dest.Name + " to Odyssey directory...");
			dest.Directory.Create();
			source.CopyTo(dest.FullName);

			Progress_Changed(exportUnit, 100, "File copied successfully.");
		}
		#endregion

		#region Update()
		protected override void Update(ExportUnit exportUnit, bool setAsRequestFinished)
		{
			Article article = exportUnit.Article;
			
			if (article.TransactionId == null && SqlEnabled)
			{
				BscanILL.Export.ILL.TransactionPair pair = GetSqlTransactionFromIllNumber(article.IllNumber);

				if (pair != null)
					article.TransactionId = pair.TransactionRow.TransactionNumber;
			}

			if (article.TransactionId == null)
			{
				BscanILL.UI.Dialogs.TransactionNumberDlg dlg = new BscanILL.UI.Dialogs.TransactionNumberDlg();
				article.TransactionId = dlg.Open(article);
			}

			if (article.TransactionId != null)
			{
				FileInfo dest = new FileInfo(_settings.Export.Odyssey.ExportDir + @"\" + article.TransactionId.Value + ".sent"); ;

				Progress_Changed(exportUnit, 10, "");

				if (dest.Exists)
					dest.Delete();

				Progress_Changed(exportUnit, 25, "");

				Progress_Comment(exportUnit, "Copying file " + dest.Name + " to Odyssey directory...");
				dest.Directory.Create();

				using (FileStream stream = new FileStream(dest.FullName, FileMode.Create))
				{
					using (StreamWriter streamWriter = new StreamWriter(stream))
					{
						streamWriter.WriteLine("Created by DLSG at Image Access");
						streamWriter.WriteLine("");
						streamWriter.WriteLine("Creation Date:      " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
						streamWriter.WriteLine("Transaction Number: " + article.TransactionId.Value);						
                        streamWriter.WriteLine("Export Type:        " + exportUnit.ExportType.ToString());
					}
				}

				Progress_Changed(exportUnit, 100, "File copied successfully.");
			}
			else
			{
				throw new Exception("Can't update ILLiad, Transaction Number was not provided!");
			}
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#endregion
	}
}
