using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace BscanILL.Export.ILLiad
{


	partial class DsIlliad
	{

		//PUBLIC METHODS
		#region public methods

		#region GetPullslips()
		public Collection<BscanILL.Export.ILL.PullslipReader.IPullslip> GetPullslips()
		{
			Collection<BscanILL.Export.ILL.PullslipReader.IPullslip> pullslips = new Collection<BscanILL.Export.ILL.PullslipReader.IPullslip>();

			foreach (DsIlliad.TransactionsRow transactionsRow in this.Transactions.Rows)
				pullslips.Add(GetPullslip(transactionsRow));

			return pullslips;
		}
		#endregion

		#region GetPullslip()
		public BscanILL.Export.ILL.PullslipReader.IPullslip GetPullslip(DsIlliad.TransactionsRow transactionsRow)
		{
			BscanILL.Export.ILL.PullslipReader.IPullslip pullslip = new BscanILL.Export.ILL.PullslipReader.Pullslip();

			pullslip.TransactionId = transactionsRow.TransactionNumber.ToString();
			pullslip.IllId = (transactionsRow.IsILLNumberNull() == false && transactionsRow.ILLNumber.Length > 0) ? transactionsRow.ILLNumber : transactionsRow.TransactionNumber.ToString();

			if (transactionsRow.IsRequestTypeNull() == false && transactionsRow.RequestType.ToLower() == "loan")
			{
				pullslip.Author = (transactionsRow.IsLoanAuthorNull() == false) ? transactionsRow.LoanAuthor : "";
				pullslip.Article = (transactionsRow.IsLoanTitleNull() == false) ? transactionsRow.LoanTitle : "";
				pullslip.Journal = "";
				pullslip.Volume = (transactionsRow.IsLoanEditionNull() == false) ? transactionsRow.LoanEdition : "";
				pullslip.Issue = "";
				pullslip.Year = "";
				pullslip.Pages = "";
			}
			else
			{
				pullslip.Author = (transactionsRow.IsPhotoArticleAuthorNull() == false) ? transactionsRow.PhotoArticleAuthor : "";
				pullslip.Article = (transactionsRow.IsPhotoArticleTitleNull() == false) ? transactionsRow.PhotoArticleTitle : "";
				pullslip.Journal = (transactionsRow.IsPhotoJournalTitleNull() == false) ? transactionsRow.PhotoJournalTitle : "";
				pullslip.Volume = (transactionsRow.IsPhotoJournalVolumeNull() == false) ? transactionsRow.PhotoJournalVolume : "";
				pullslip.Issue = (transactionsRow.IsPhotoJournalIssueNull() == false) ? transactionsRow.PhotoJournalIssue : "";
				pullslip.Year = (transactionsRow.IsPhotoJournalYearNull() == false) ? transactionsRow.PhotoJournalYear : "";
				pullslip.Pages = (transactionsRow.IsPhotoJournalInclusivePagesNull() == false) ? transactionsRow.PhotoJournalInclusivePages : "";
			}

			pullslip.Patron = (transactionsRow.IsPatronNull() == false) ? transactionsRow.Patron : "";
			pullslip.Borrower = transactionsRow.BorrowerNVTGC;
			pullslip.CatalogNumber = (transactionsRow.IsCallNumberNull() == false) ? transactionsRow.CallNumber : "";
			pullslip.Location = (transactionsRow.IsLocationNull() == false) ? transactionsRow.Location : "";
			pullslip.ColorMode = BscanILL.Export.ILL.PullslipReader.ColorMode.BW;
			pullslip.ExportType = GetExportType(transactionsRow);
			pullslip.Address = GetAddress(transactionsRow, pullslip.ExportType);

			return pullslip;
		}
		#endregion

		#region GetLenderAdresses()
		public DsIlliad.LenderAddressesRow GetLenderAdresses(DsIlliad.TransactionsRow transactionsRow)
		{
			var rows = from l in this.LenderAddresses
					   where (l.LenderString == transactionsRow.LendingLibrary) &&
					   (l.AddressNumber == transactionsRow.LenderAddressNumber)
					   select l;

			List<DsIlliad.LenderAddressesRow> lendersRows = new List<LenderAddressesRow>();
			lendersRows.AddRange(rows);

			if (lendersRows.Count == 0)
				return null;
			else if (lendersRows.Count == 1)
				return lendersRows[0];
			else
			{
				if (transactionsRow.IsBorrowerNVTGCNull() == false)
				{
					foreach (DsIlliad.LenderAddressesRow r in lendersRows)
						if (r.NVTGC != null && r.NVTGC.ToLower() == transactionsRow.BorrowerNVTGC)
							return r;
				}

				foreach (DsIlliad.LenderAddressesRow r in lendersRows)
					if (r.NVTGC != null && r.NVTGC.ToLower() == "Lending")
						return r;

				return lendersRows[0];
			}
		}
		#endregion
	
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region GetExportType()
		private ExportType GetExportType(DsIlliad.TransactionsRow transactionsRow)
		{
			if (transactionsRow.IsProcessTypeNull() == false && transactionsRow.ProcessType.ToLower() == "lending")
			{
				if (transactionsRow.IsSystemIDNull() == false && transactionsRow.SystemID.ToLower() == "rapid")
					return ExportType.Ariel;

				if (transactionsRow.IsShippingOptionsNull() == false)
				{
					if (transactionsRow.ShippingOptions.ToLower().Contains("ariel"))
						return ExportType.Ariel;
					else if (transactionsRow.ShippingOptions.ToLower().Contains("odyssey"))
						return ExportType.Odyssey;
					else if (transactionsRow.ShippingOptions.ToLower().Contains("email") || transactionsRow.ShippingOptions.ToLower().Contains("e-mail"))
						return ExportType.Email;
					else if (transactionsRow.ShippingOptions.ToLower().Contains("ftp"))
						return ExportType.Ftp;
					else
						return ExportType.SaveOnDisk;
				}
				else if (transactionsRow.IsArielNull() == false && transactionsRow.Ariel.ToLower() == "yes")
					return ExportType.Ariel;
				else
				{
					return ExportType.Odyssey;
				}
			}
			else if (transactionsRow.IsProcessTypeNull() == false && transactionsRow.ProcessType.ToLower() == "doc del")
			{
				return ExportType.ILLiad;
			}
			else
				return ExportType.SaveOnDisk;
		}
		#endregion	

		#region GetAddress()
		private string GetAddress(DsIlliad.TransactionsRow transactionsRow, ExportType exportType)
		{
			switch (exportType)
			{
				case ExportType.Ariel:
					{
						DsIlliad.LenderAddressesRow lendersRow = this.LenderAddresses.FindByLenderStringAddressNumberNVTGC(transactionsRow.LendingLibrary, transactionsRow.LenderAddressNumber, transactionsRow.BorrowerNVTGC);
						
						if (lendersRow != null)
						{
							if (lendersRow.IsArielAddressNull() == false)
								return lendersRow.ArielAddress;
							else
								throw new Exception("Ariel address of lender '" + lendersRow.LenderString + "', ID: '" + lendersRow.AddressNumber + "', NVTGC: '" + lendersRow .NVTGC + "' is not specified!");
						}
						else
							throw new Exception("There is no lender associated with article TN# " + transactionsRow.TransactionNumber + " in the ILLiad SQL database!");
					}
				case ExportType.Email:
					{
						DsIlliad.LenderAddressesRow lendersRow = this.LenderAddresses.FindByLenderStringAddressNumberNVTGC(transactionsRow.LendingLibrary, transactionsRow.LenderAddressNumber, transactionsRow.BorrowerNVTGC);

						if (lendersRow != null)
						{
							if (lendersRow.IsEMailAddressNull() == false)
								return lendersRow.EMailAddress;
							else
								throw new Exception("E-mail address of lender '" + lendersRow.LenderString + "', ID: '" + lendersRow.AddressNumber + "', NVTGC: '" + lendersRow.NVTGC + "' is not specified!");
						}
						else
							throw new Exception("There is no lender associated with article TN# " + transactionsRow.TransactionNumber + " in the ILLiad SQL database!");
					}
				case ExportType.Odyssey:
					{
						/*DsIlliad.LenderAddressesRow lendersRow = this.LenderAddresses.FindByLenderStringAddressNumberNVTGC(transactionsRow.LendingLibrary, transactionsRow.LenderAddressNumber, transactionsRow.BorrowerNVTGC);

						if (lendersRow != null)
						{
							if (lendersRow.IsOdysseyIPNull() == false)
								return lendersRow.OdysseyIP;
							else
								throw new Exception("Odyssey address of lender '" + lendersRow.LenderString + "', ID: '" + lendersRow.AddressNumber + "', NVTGC: '" + lendersRow.NVTGC + "' is not specified!");
						}
						else
							throw new Exception("There is no lender associated with article TN# " + transactionsRow.TransactionNumber + " in the ILLiad SQL database!");
						*/
						return "N/A";
					}
				case ExportType.Ftp:
					{
						return "N/A";
					}
				case ExportType.FtpDir:
					{
						return "N/A";
					}
				case ExportType.ILLiad:
					{
						return "N/A";
					} 
				case ExportType.SaveOnDisk:
					{
						return "";
					}
				default:
					{
						return "";
					}
			}
		}
		#endregion	
	
		#endregion


	}
}

