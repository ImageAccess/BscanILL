using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace BscanILL.Export.ILLiad
{
	public partial class DatabaseSQL8_1 : DatabaseSQLBase, IDatabaseSQL
	{
		#region variables

		private SqlDataAdapter daTransactions;
		private SqlDataAdapter daLenderAddressesALL;

		#endregion

		#region constructor
		public DatabaseSQL8_1(string sqlServerUri, string databaseName, bool windowsCredentials, string username, string password)
			: base(sqlServerUri, databaseName, windowsCredentials, username, password)
		{
			InitializeComponent();
		}

		public DatabaseSQL8_1()
			: base()
		{
			InitializeComponent();
		}
		#endregion

		#region InitializeComponent()
		private void InitializeComponent()
		{
			this.daLenderAddressesALL = new System.Data.SqlClient.SqlDataAdapter();
			this.daTransactions = new System.Data.SqlClient.SqlDataAdapter();

			// 
			// daLenderAddressesAll
			// 
			this.daLenderAddressesALL.TableMappings.AddRange(new System.Data.Common.DataTableMapping[] {
            new System.Data.Common.DataTableMapping("Table", "LenderAddressesALL", new System.Data.Common.DataColumnMapping[] {
                        new System.Data.Common.DataColumnMapping("LenderString", "LenderString"),
                        new System.Data.Common.DataColumnMapping("AddressNumber", "AddressNumber"),
                        new System.Data.Common.DataColumnMapping("NVTGC", "NVTGC"),
                        new System.Data.Common.DataColumnMapping("LibraryName", "LibraryName"),
                        new System.Data.Common.DataColumnMapping("Address1", "Address1"),
                        new System.Data.Common.DataColumnMapping("Address2", "Address2"),
                        new System.Data.Common.DataColumnMapping("Address3", "Address3"),
                        new System.Data.Common.DataColumnMapping("Address4", "Address4"),
                        new System.Data.Common.DataColumnMapping("BAddress1", "BAddress1"),
                        new System.Data.Common.DataColumnMapping("BAddress2", "BAddress2"),
                        new System.Data.Common.DataColumnMapping("BAddress3", "BAddress3"),
                        new System.Data.Common.DataColumnMapping("BAddress4", "BAddress4"),
                        new System.Data.Common.DataColumnMapping("Fax", "Fax"),
                        new System.Data.Common.DataColumnMapping("ArielAddress", "ArielAddress"),
                        new System.Data.Common.DataColumnMapping("Phone", "Phone"),
                        new System.Data.Common.DataColumnMapping("PriorityShipping", "PriorityShipping"),
                        new System.Data.Common.DataColumnMapping("CopyrightPayer", "CopyrightPayer"),
                        new System.Data.Common.DataColumnMapping("BillingExempt", "BillingExempt"),
                        new System.Data.Common.DataColumnMapping("BillingCategory", "BillingCategory"),
                        new System.Data.Common.DataColumnMapping("LibCopyCharge", "LibCopyCharge"),
                        new System.Data.Common.DataColumnMapping("LibLoanCharge", "LibLoanCharge"),
                        new System.Data.Common.DataColumnMapping("LibBillingMethod", "LibBillingMethod"),
                        new System.Data.Common.DataColumnMapping("EFTS", "EFTS"),
                        new System.Data.Common.DataColumnMapping("EmailAddress", "EmailAddress"),
                        new System.Data.Common.DataColumnMapping("OdysseyIP", "OdysseyIP"),
                        new System.Data.Common.DataColumnMapping("OverrideIFM", "OverrideIFM"),
                        new System.Data.Common.DataColumnMapping("ISO", "ISO"),
                        new System.Data.Common.DataColumnMapping("ISOIPAddress", "ISOIPAddress"),
                        new System.Data.Common.DataColumnMapping("ISORequesterId", "ISORequesterId"),
                        new System.Data.Common.DataColumnMapping("ISOUserName", "ISOUserName"),
                        new System.Data.Common.DataColumnMapping("ISOPassword", "ISOPassword"),
                        new System.Data.Common.DataColumnMapping("WebPassword", "WebPassword"),
                        new System.Data.Common.DataColumnMapping("Blocked", "Blocked"),
                        new System.Data.Common.DataColumnMapping("TrustedSender", "TrustedSender"),
                        new System.Data.Common.DataColumnMapping("ISOPaymentMethod", "ISOPaymentMethod"),
                        new System.Data.Common.DataColumnMapping("DefaultShippingMethodLoan", "DefaultShippingMethodLoan"),
                        new System.Data.Common.DataColumnMapping("ISOTransport", "ISOTransport"),
                        new System.Data.Common.DataColumnMapping("ISOEMailAddress", "ISOEMailAddress"),
                        new System.Data.Common.DataColumnMapping("ISOAutoUpdateAddress", "ISOAutoUpdateAddress"),
                        new System.Data.Common.DataColumnMapping("ESPShipTo", "ESPShipTo"),
                        new System.Data.Common.DataColumnMapping("ESPBillTo", "ESPBillTo"),
                        new System.Data.Common.DataColumnMapping("RSSID", "RSSID"),
                        new System.Data.Common.DataColumnMapping("OverrideOdyssey", "OverrideOdyssey"),
                        new System.Data.Common.DataColumnMapping("TrustedSenderOverride", "TrustedSenderOverride"),
                        new System.Data.Common.DataColumnMapping("OdysseyVersion", "OdysseyVersion"),
                        new System.Data.Common.DataColumnMapping("DefaultShippingMethodArticle", "DefaultShippingMethodArticle"),
                        new System.Data.Common.DataColumnMapping("LendingDeptEmail", "LendingDeptEmail")})});

			// 
			// daTransactions
			// 
			this.daTransactions.TableMappings.AddRange(new System.Data.Common.DataTableMapping[] {
            new System.Data.Common.DataTableMapping("Table", "Transactions", new System.Data.Common.DataColumnMapping[] {
                        new System.Data.Common.DataColumnMapping("TransactionNumber", "TransactionNumber"),
                        new System.Data.Common.DataColumnMapping("Username", "Username"),
                        new System.Data.Common.DataColumnMapping("RequestType", "RequestType"),
                        new System.Data.Common.DataColumnMapping("LoanAuthor", "LoanAuthor"),
                        new System.Data.Common.DataColumnMapping("LoanTitle", "LoanTitle"),
                        new System.Data.Common.DataColumnMapping("LoanPublisher", "LoanPublisher"),
                        new System.Data.Common.DataColumnMapping("LoanPlace", "LoanPlace"),
                        new System.Data.Common.DataColumnMapping("LoanDate", "LoanDate"),
                        new System.Data.Common.DataColumnMapping("LoanEdition", "LoanEdition"),
                        new System.Data.Common.DataColumnMapping("PhotoJournalTitle", "PhotoJournalTitle"),
                        new System.Data.Common.DataColumnMapping("PhotoJournalVolume", "PhotoJournalVolume"),
                        new System.Data.Common.DataColumnMapping("PhotoJournalIssue", "PhotoJournalIssue"),
                        new System.Data.Common.DataColumnMapping("PhotoJournalMonth", "PhotoJournalMonth"),
                        new System.Data.Common.DataColumnMapping("PhotoJournalYear", "PhotoJournalYear"),
                        new System.Data.Common.DataColumnMapping("PhotoJournalInclusivePages", "PhotoJournalInclusivePages"),
                        new System.Data.Common.DataColumnMapping("PhotoArticleAuthor", "PhotoArticleAuthor"),
                        new System.Data.Common.DataColumnMapping("PhotoArticleTitle", "PhotoArticleTitle"),
                        new System.Data.Common.DataColumnMapping("CitedIn", "CitedIn"),
                        new System.Data.Common.DataColumnMapping("CitedTitle", "CitedTitle"),
                        new System.Data.Common.DataColumnMapping("CitedDate", "CitedDate"),
                        new System.Data.Common.DataColumnMapping("CitedVolume", "CitedVolume"),
                        new System.Data.Common.DataColumnMapping("CitedPages", "CitedPages"),
                        new System.Data.Common.DataColumnMapping("NotWantedAfter", "NotWantedAfter"),
                        new System.Data.Common.DataColumnMapping("AcceptNonEnglish", "AcceptNonEnglish"),
                        new System.Data.Common.DataColumnMapping("AcceptAlternateEdition", "AcceptAlternateEdition"),
                        new System.Data.Common.DataColumnMapping("TransactionStatus", "TransactionStatus"),
                        new System.Data.Common.DataColumnMapping("TransactionDate", "TransactionDate"),
                        new System.Data.Common.DataColumnMapping("ISSN", "ISSN"),
                        new System.Data.Common.DataColumnMapping("ILLNumber", "ILLNumber"),
                        new System.Data.Common.DataColumnMapping("ESPNumber", "ESPNumber"),
                        new System.Data.Common.DataColumnMapping("LendingString", "LendingString"),
                        new System.Data.Common.DataColumnMapping("BaseFee", "BaseFee"),
                        new System.Data.Common.DataColumnMapping("PerPage", "PerPage"),
                        new System.Data.Common.DataColumnMapping("Pages", "Pages"),
                        new System.Data.Common.DataColumnMapping("DueDate", "DueDate"),
                        new System.Data.Common.DataColumnMapping("RenewalsAllowed", "RenewalsAllowed"),
                        new System.Data.Common.DataColumnMapping("SpecIns", "SpecIns"),
                        new System.Data.Common.DataColumnMapping("Pieces", "Pieces"),
                        new System.Data.Common.DataColumnMapping("LibraryUseOnly", "LibraryUseOnly"),
                        new System.Data.Common.DataColumnMapping("AllowPhotocopies", "AllowPhotocopies"),
                        new System.Data.Common.DataColumnMapping("LendingLibrary", "LendingLibrary"),
                        new System.Data.Common.DataColumnMapping("ReasonForCancellation", "ReasonForCancellation"),
                        new System.Data.Common.DataColumnMapping("CallNumber", "CallNumber"),
                        new System.Data.Common.DataColumnMapping("Location", "Location"),
                        new System.Data.Common.DataColumnMapping("Maxcost", "Maxcost"),
                        new System.Data.Common.DataColumnMapping("ProcessType", "ProcessType"),
                        new System.Data.Common.DataColumnMapping("ItemNumber", "ItemNumber"),
                        new System.Data.Common.DataColumnMapping("LenderAddressNumber", "LenderAddressNumber"),
                        new System.Data.Common.DataColumnMapping("Ariel", "Ariel"),
                        new System.Data.Common.DataColumnMapping("Patron", "Patron"),
                        new System.Data.Common.DataColumnMapping("PhotoItemAuthor", "PhotoItemAuthor"),
                        new System.Data.Common.DataColumnMapping("PhotoItemPlace", "PhotoItemPlace"),
                        new System.Data.Common.DataColumnMapping("PhotoItemPublisher", "PhotoItemPublisher"),
                        new System.Data.Common.DataColumnMapping("PhotoItemEdition", "PhotoItemEdition"),
                        new System.Data.Common.DataColumnMapping("DocumentType", "DocumentType"),
                        new System.Data.Common.DataColumnMapping("InternalAcctNo", "InternalAcctNo"),
                        new System.Data.Common.DataColumnMapping("PriorityShipping", "PriorityShipping"),
                        new System.Data.Common.DataColumnMapping("Rush", "Rush"),
                        new System.Data.Common.DataColumnMapping("CopyrightAlreadyPaid", "CopyrightAlreadyPaid"),
                        new System.Data.Common.DataColumnMapping("WantedBy", "WantedBy"),
                        new System.Data.Common.DataColumnMapping("SystemID", "SystemID"),
                        new System.Data.Common.DataColumnMapping("ReplacementPages", "ReplacementPages"),
                        new System.Data.Common.DataColumnMapping("IFMCost", "IFMCost"),
                        new System.Data.Common.DataColumnMapping("CopyrightPaymentMethod", "CopyrightPaymentMethod"),
                        new System.Data.Common.DataColumnMapping("ShippingOptions", "ShippingOptions"),
                        new System.Data.Common.DataColumnMapping("CCCNumber", "CCCNumber"),
                        new System.Data.Common.DataColumnMapping("IntlShippingOptions", "IntlShippingOptions"),
                        new System.Data.Common.DataColumnMapping("ShippingAcctNo", "ShippingAcctNo"),
                        new System.Data.Common.DataColumnMapping("ReferenceNumber", "ReferenceNumber"),
                        new System.Data.Common.DataColumnMapping("CopyrightComp", "CopyrightComp"),
                        new System.Data.Common.DataColumnMapping("TAddress", "TAddress"),
                        new System.Data.Common.DataColumnMapping("TAddress2", "TAddress2"),
                        new System.Data.Common.DataColumnMapping("TCity", "TCity"),
                        new System.Data.Common.DataColumnMapping("TState", "TState"),
                        new System.Data.Common.DataColumnMapping("TZip", "TZip"),
                        new System.Data.Common.DataColumnMapping("TCountry", "TCountry"),
                        new System.Data.Common.DataColumnMapping("TFax", "TFax"),
                        new System.Data.Common.DataColumnMapping("TEMailAddress", "TEMailAddress"),
                        new System.Data.Common.DataColumnMapping("TNumber", "TNumber"),
                        new System.Data.Common.DataColumnMapping("HandleWithCare", "HandleWithCare"),
                        new System.Data.Common.DataColumnMapping("CopyWithCare", "CopyWithCare"),
                        new System.Data.Common.DataColumnMapping("RestrictedUse", "RestrictedUse"),
                        new System.Data.Common.DataColumnMapping("ReceivedVia", "ReceivedVia"),
                        new System.Data.Common.DataColumnMapping("CancellationCode", "CancellationCode"),
                        new System.Data.Common.DataColumnMapping("BillingCategory", "BillingCategory"),
                        new System.Data.Common.DataColumnMapping("CCSelected", "CCSelected"),
                        new System.Data.Common.DataColumnMapping("OriginalTN", "OriginalTN"),
                        new System.Data.Common.DataColumnMapping("OriginalNVTGC", "OriginalNVTGC"),
                        new System.Data.Common.DataColumnMapping("InProcessDate", "InProcessDate"),
                        new System.Data.Common.DataColumnMapping("InvoiceNumber", "InvoiceNumber"),
                        new System.Data.Common.DataColumnMapping("BorrowerTN", "BorrowerTN"),
                        new System.Data.Common.DataColumnMapping("WebRequestForm", "WebRequestForm"),
                        new System.Data.Common.DataColumnMapping("TName", "TName"),
                        new System.Data.Common.DataColumnMapping("TAddress3", "TAddress3"),
                        new System.Data.Common.DataColumnMapping("IFMPaid", "IFMPaid"),
                        new System.Data.Common.DataColumnMapping("BorrowerNVTGC", "BorrowerNVTGC"),
                        new System.Data.Common.DataColumnMapping("TISOPaymentMethod", "TISOPaymentMethod"),
                        new System.Data.Common.DataColumnMapping("ISOStatus", "ISOStatus"),
                        new System.Data.Common.DataColumnMapping("ShippingDetail", "ShippingDetail"),
                        new System.Data.Common.DataColumnMapping("OdysseyErrorStatus", "OdysseyErrorStatus"),
                        new System.Data.Common.DataColumnMapping("WorldCatLCNumber", "WorldCatLCNumber"),
                        new System.Data.Common.DataColumnMapping("Locations", "Locations"),
                        new System.Data.Common.DataColumnMapping("FlagType", "FlagType"),
                        new System.Data.Common.DataColumnMapping("FlagNote", "FlagNote")})});
		}
		#endregion

		//PUBLIC METHODS
		#region public methods

		#region Login
		public override void Login()
		{
			if (sqlConnection.State != ConnectionState.Open && sqlConnection.State != ConnectionState.Connecting)
			{
				base.Login();

				this.cmdTransactionsSelectText = "SELECT        TransactionNumber, Username, RequestType, LoanAuthor, LoanTitle, LoanPublisher, LoanPlace, LoanDate, LoanEdition, PhotoJournalTitle, PhotoJournalVolume, " +
                         "PhotoJournalIssue, PhotoJournalMonth, PhotoJournalYear, PhotoJournalInclusivePages, PhotoArticleAuthor, PhotoArticleTitle, CitedIn, CitedTitle, CitedDate, " +
                         "CitedVolume, CitedPages, NotWantedAfter, AcceptNonEnglish, AcceptAlternateEdition, TransactionStatus, TransactionDate, ISSN, ILLNumber, ESPNumber, " +
                         "LendingString, BaseFee, PerPage, Pages, DueDate, RenewalsAllowed, SpecIns, Pieces, LibraryUseOnly, AllowPhotocopies, LendingLibrary, ReasonForCancellation, " +
                         "CallNumber, Location, Maxcost, ProcessType, ItemNumber, LenderAddressNumber, Ariel, Patron, PhotoItemAuthor, PhotoItemPlace, PhotoItemPublisher, " +
                         "PhotoItemEdition, DocumentType, InternalAcctNo, PriorityShipping, Rush, CopyrightAlreadyPaid, WantedBy, SystemID, ReplacementPages, IFMCost, " +
                         "CopyrightPaymentMethod, ShippingOptions, CCCNumber, IntlShippingOptions, ShippingAcctNo, ReferenceNumber, CopyrightComp, TAddress, TAddress2, TCity, " +
                         "TState, TZip, TCountry, TFax, TEMailAddress, TNumber, HandleWithCare, CopyWithCare, RestrictedUse, ReceivedVia, CancellationCode, BillingCategory, " +
                         "CCSelected, OriginalTN, OriginalNVTGC, InProcessDate, InvoiceNumber, BorrowerTN, WebRequestForm, TName, TAddress3, IFMPaid, BorrowerNVTGC, " +
                         "TISOPaymentMethod, ISOStatus, ShippingDetail, OdysseyErrorStatus, WorldCatLCNumber, Locations, FlagType, FlagNote " +
						 " FROM            Transactions ";

				if (this.singleStationSystem)
				{
					this.cmdLenderAddressesSelectText = "SELECT        LenderString, AddressNumber, NVTGC, LibraryName, Address1, Address2, Address3, Address4, BAddress1, BAddress2, BAddress3, BAddress4, Fax, ArielAddress, " +
                         "Phone, PriorityShipping, CopyrightPayer, BillingExempt, BillingCategory, LibCopyCharge, LibLoanCharge, LibBillingMethod, EFTS, " +
                         "BorrowingDeptEmail AS EmailAddress, OdysseyIP, OverrideIFM, ISO, ISOIPAddress, ISORequesterId, ISOUserName, ISOPassword, WebPassword, Blocked, " +
                         "TrustedSender, ISOPaymentMethod, DefaultShippingMethodLoan, ISOTransport, ISOEMailAddress, ISOAutoUpdateAddress, ESPShipTo, ESPBillTo, RSSID, " +
                         "OverrideOdyssey, TrustedSenderOverride, OdysseyVersion, DefaultShippingMethodArticle, LendingDeptEmail " +
						 " FROM            LenderAddresses ";

					this.cmdUserNvtgcString = "Select NVTGC from Users ";
				}
				else
				{
					this.cmdLenderAddressesSelectText = "SELECT        LenderString, AddressNumber, NVTGC, LibraryName, Address1, Address2, Address3, Address4, BAddress1, BAddress2, BAddress3, BAddress4, Fax, ArielAddress, " +
                         "Phone, PriorityShipping, CopyrightPayer, BillingExempt, BillingCategory, LibCopyCharge, LibLoanCharge, LibBillingMethod, EFTS, " +
                         "BorrowingDeptEmail AS EmailAddress, OdysseyIP, OverrideIFM, ISO, ISOIPAddress, ISORequesterId, ISOUserName, ISOPassword, WebPassword, Blocked, " +
                         "TrustedSender, ISOPaymentMethod, DefaultShippingMethodLoan, ISOTransport, ISOEMailAddress, ISOAutoUpdateAddress, ESPShipTo, ESPBillTo, RSSID, " +
                         "OverrideOdyssey, TrustedSenderOverride, OdysseyVersion, DefaultShippingMethodArticle, LendingDeptEmail " +
						 " FROM            LenderAddressesALL ";

					this.cmdUserNvtgcString = "Select NVTGC from UsersALL ";
				}

				Progress_Changed(null, -1, "SQL structure received.");
			}

		}
		#endregion		

		#region GetRequest()
		public override BscanILL.Export.ILL.TransactionPair GetRequest(int transactionId)
		{
			try
			{
				Progress_Changed(null, -1, "Getting request '" + transactionId + "'from ILLiad SQL server...");

				DsIlliad ds = new DsIlliad();
				Login();

				SqlCommand selectCmd = new SqlCommand(cmdTransactionsSelectText + " WHERE TransactionNumber = " + transactionId, sqlConnection);
				selectCmd.CommandTimeout = 20000;

				daTransactions.SelectCommand = selectCmd;

				daTransactions.Fill(ds.Transactions);
				Progress_Changed(null, -1, "Request from ILLiad SQL server was finished.");

				if (ds.Transactions.Rows.Count > 0)
				{
					Progress_Changed(null, -1, "Transaction row exists.");
					DsIlliad.TransactionsRow transactionsRow = (DsIlliad.TransactionsRow)ds.Transactions.Rows[0];

					Progress_Changed(null, -1, "Getting transaction's NVTGC...");
					transactionsRow.BorrowerNVTGC = GetNVTGC(transactionsRow.Username);

					Progress_Changed(null, -1, "Adding lender to the ds...");
					DsIlliad.LenderAddressesRow lenderAddressRow = GetLenderAddress(ds, transactionsRow);
					BscanILL.Export.ILL.TransactionPair	pair = new BscanILL.Export.ILL.TransactionPair(transactionsRow, lenderAddressRow);

					return pair;
				}
				else
				{
					Progress_Changed(null, -1, "!!! Can't get Transaction Row !!!");
				}
			}
			catch (Exception ex)
			{
				Progress_Changed(null, -1, "!!! " + ex.Message + " !!!");
				throw ex;
			}

			return null;
		}
		#endregion	
	
		#region GetRequests()
		public override List<BscanILL.Export.ILL.TransactionPair> GetRequests(string[] transactionStatuses, bool loans, bool articles)
		{
			DsIlliad ds = new DsIlliad();
			List<BscanILL.Export.ILL.TransactionPair> pairs = new List<BscanILL.Export.ILL.TransactionPair>();
			
			Login();

			string cmdWhereString = "";

			if (transactionStatuses.Length > 0)
			{
				cmdWhereString += " WHERE (TransactionStatus = '" + transactionStatuses[0] + "'";
				for (int i = 1; i < transactionStatuses.Length; i++)
					cmdWhereString += " OR TransactionStatus = '" + transactionStatuses[i] + "'";

				cmdWhereString += ") ";
			}

			if (loans != articles)
			{
				if (loans)
					cmdWhereString += " AND RequestType = 'Loan' ";
				if (articles)
					cmdWhereString += " AND RequestType = 'Article' ";
			}

			SqlCommand selectTransactionsCmd = new SqlCommand(cmdTransactionsSelectText + cmdWhereString, sqlConnection);
			selectTransactionsCmd.CommandTimeout = 20000;

			daTransactions.SelectCommand = selectTransactionsCmd;
			daTransactions.Fill(ds.Transactions);

			foreach (DsIlliad.TransactionsRow transactionsRow in ds.Transactions.Rows)
			{
				try
				{
					transactionsRow.BorrowerNVTGC = GetNVTGC(transactionsRow.Username);
					DsIlliad.LenderAddressesRow lenderAddressRow = GetLenderAddress(ds, transactionsRow);

					pairs.Add(new BscanILL.Export.ILL.TransactionPair(transactionsRow, lenderAddressRow));
				}
				catch (Exception ex)
				{
					MessageBox.Show("Transaction '" + transactionsRow.TransactionNumber + "': " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error); 
				}
			}

			return pairs;
		}
		#endregion	

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region GetLenderAddress()
		private DsIlliad.LenderAddressesRow GetLenderAddress(DsIlliad ds, DsIlliad.TransactionsRow transactionsRow)
		{
			DsIlliad.LenderAddressesRow lendersRow = null;
			
			Progress_Changed(null, -1, "Getting lender addresses from ILLiad SQL server...");
			double lenderAddressNumber = transactionsRow.IsLenderAddressNumberNull() ? 1 : transactionsRow.LenderAddressNumber;

			if (transactionsRow.IsLendingLibraryNull() == false && transactionsRow.IsBorrowerNVTGCNull() == false)
			{
				lendersRow = ds.LenderAddresses.FindByLenderStringAddressNumberNVTGC(transactionsRow.LendingLibrary, lenderAddressNumber, transactionsRow.BorrowerNVTGC);

				if (lendersRow == null)
				{
					SqlCommand selectCmd = new SqlCommand(cmdLenderAddressesSelectText + " WHERE LenderString = '" + transactionsRow.LendingLibrary +
						"' AND AddressNumber = " + lenderAddressNumber + " AND NVTGC = '" + transactionsRow.BorrowerNVTGC + "'", sqlConnection);
					selectCmd.CommandTimeout = 20000;

					this.daLenderAddressesALL.SelectCommand = selectCmd;
					int addedRows = this.daLenderAddressesALL.Fill(ds.LenderAddresses);

					if (addedRows > 0)
					{
						Progress_Changed(null, -1, "Lender addresses of '" + transactionsRow.TransactionNumber + "' were returned.");

						lendersRow = ds.LenderAddresses.FindByLenderStringAddressNumberNVTGC(transactionsRow.LendingLibrary, lenderAddressNumber, transactionsRow.BorrowerNVTGC);
					}
					else
					{
						lendersRow = ds.LenderAddresses.FindByLenderStringAddressNumberNVTGC(transactionsRow.LendingLibrary, lenderAddressNumber, "Lending");

						if (lendersRow == null)
						{
							selectCmd = new SqlCommand(cmdLenderAddressesSelectText + " WHERE LenderString = '" + transactionsRow.LendingLibrary +
									"' AND AddressNumber = " + lenderAddressNumber + " AND NVTGC = 'Lending'", sqlConnection);
							selectCmd.CommandTimeout = 20000;

							this.daLenderAddressesALL.SelectCommand = selectCmd;
							addedRows = this.daLenderAddressesALL.Fill(ds.LenderAddresses);

							if (addedRows == 0)
							{
								selectCmd = new SqlCommand(cmdLenderAddressesSelectText + " WHERE LenderString = '" + transactionsRow.LendingLibrary +
										"' AND AddressNumber = " + lenderAddressNumber, sqlConnection);
								selectCmd.CommandTimeout = 20000;

								this.daLenderAddressesALL.SelectCommand = selectCmd;
								addedRows = this.daLenderAddressesALL.Fill(ds.LenderAddresses);
							}

							if (addedRows > 0)
							{
								Progress_Changed(null, -1, "Lender addresses of '" + transactionsRow.TransactionNumber + "' were returned.");

								lendersRow = ds.LenderAddresses[0];//.FindByLenderStringAddressNumberNVTGC(transactionsRow.LendingLibrary, lenderAddressNumber, "Lending");
								lendersRow.NVTGC = transactionsRow.BorrowerNVTGC;
							}
						}
					}
				}
			}

			//Progress_Changed(null, -1, "Lender addresses of '" + transactionsRow.TransactionNumber + "' were not found.");
			return lendersRow;
		}
		#endregion	

		#region GetNVTGC()
		private string GetNVTGC(string username)
		{
			Progress_Changed(null, -1, this.cmdUserNvtgcString + " WHERE UserName = '" + username + "'");
			SqlCommand sqlCommand = new SqlCommand(this.cmdUserNvtgcString + " WHERE UserName = '" + username + "'", sqlConnection);

			string nvtgc = (string)sqlCommand.ExecuteScalar();
			return nvtgc;
		}
		#endregion

		#endregion
	}
}
