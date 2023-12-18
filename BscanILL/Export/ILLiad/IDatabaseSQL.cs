using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient ;
using System.Windows.Forms;
using System.Globalization;
//using System.Management;
using System.Text;



namespace BscanILL.Export.ILLiad
{
	public interface IDatabaseSQL
	{
		//EVENTS
		event ProgressChangedHandle ProgressChanged;
		event ProgressCommentHandle ProgressComment;

		//PUBLIC METHODS
		void		Login();
		void		Logout();
		BscanILL.Export.ILL.TransactionPair GetRequest(int transactionId);
		BscanILL.Export.ILL.TransactionPair GetRequestFromIllNumber(string illNumber);
		List<BscanILL.Export.ILL.TransactionPair> GetRequests(string[] transactionStatuses, bool loans, bool articles);

	}
}
