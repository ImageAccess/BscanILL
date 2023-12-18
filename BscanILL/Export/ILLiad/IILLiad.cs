using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace BscanILL.Export.ILLiad
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public interface IILLiad
	{
		event ProgressChangedHandle ProgressChanged;
		event ProgressCommentHandle ProgressComment;

		//PUBLIC PROPERTIES
		bool SqlEnabled { get; } 

		//PUBLIC METHODS
		void											UpdateInfo(ExportUnit exportUnit, bool setAsRequestFinished);
		void											ExportArticle(ExportUnit exportUnit);
		BscanILL.Export.ILL.TransactionPair				GetSqlTransaction(int transactionId);
		BscanILL.Export.ILL.TransactionPair				GetSqlTransactionFromIllNumber(string illNumber);
		void											CheckArticleInDb(ExportUnit exportUnit);
		BscanILL.Export.ILL.PullslipReader.IPullslip	GetPullslip(int transactionNumber);
	}
}
