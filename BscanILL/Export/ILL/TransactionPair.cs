using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.ILL
{
	public class TransactionPair
	{
		public readonly BscanILL.Export.ILLiad.DsIlliad.TransactionsRow TransactionRow;
		public readonly BscanILL.Export.ILLiad.DsIlliad.LenderAddressesRow LenderAddressRow;

		#region constructor
		public TransactionPair(BscanILL.Export.ILLiad.DsIlliad.TransactionsRow transactionRow, BscanILL.Export.ILLiad.DsIlliad.LenderAddressesRow lenderAddressRow)
		{
			this.TransactionRow = transactionRow;
			this.LenderAddressRow = lenderAddressRow;
		}
		#endregion



		#region GetPullslip()
		public PullslipReader.IPullslip GetPullslip()
		{
			PullslipReader.IPullslip pullslip = new PullslipReader.Pullslip();

			pullslip.TransactionId = TransactionRow.TransactionNumber.ToString();
			pullslip.IllId = (TransactionRow.IsILLNumberNull() == false && TransactionRow.ILLNumber.Length > 0) ? TransactionRow.ILLNumber : TransactionRow.TransactionNumber.ToString();

			if (TransactionRow.IsRequestTypeNull() == false && TransactionRow.RequestType.ToLower() == "loan")
			{
				pullslip.Author = (TransactionRow.IsLoanAuthorNull() == false) ? TransactionRow.LoanAuthor : "";
				pullslip.Article = (TransactionRow.IsLoanTitleNull() == false) ? TransactionRow.LoanTitle : "";
				pullslip.Journal = "";
				pullslip.Volume = (TransactionRow.IsLoanEditionNull() == false) ? TransactionRow.LoanEdition : "";
				pullslip.Issue = "";
				pullslip.Year = "";
				pullslip.Pages = "";
			}
			else
			{
				pullslip.Author = (TransactionRow.IsPhotoArticleAuthorNull() == false) ? TransactionRow.PhotoArticleAuthor : "";
				pullslip.Article = (TransactionRow.IsPhotoArticleTitleNull() == false) ? TransactionRow.PhotoArticleTitle : "";
				pullslip.Journal = (TransactionRow.IsPhotoJournalTitleNull() == false) ? TransactionRow.PhotoJournalTitle : "";
				pullslip.Volume = (TransactionRow.IsPhotoJournalVolumeNull() == false) ? TransactionRow.PhotoJournalVolume : "";
				pullslip.Issue = (TransactionRow.IsPhotoJournalIssueNull() == false) ? TransactionRow.PhotoJournalIssue : "";
				pullslip.Year = (TransactionRow.IsPhotoJournalYearNull() == false) ? TransactionRow.PhotoJournalYear : "";
				pullslip.Pages = (TransactionRow.IsPhotoJournalInclusivePagesNull() == false) ? TransactionRow.PhotoJournalInclusivePages : "";
			}

			pullslip.Patron = (TransactionRow.IsPatronNull() == false) ? TransactionRow.Patron : "";
			pullslip.Borrower = TransactionRow.BorrowerNVTGC;
			pullslip.CatalogNumber = (TransactionRow.IsCallNumberNull() == false) ? TransactionRow.CallNumber : "";
			pullslip.Location = (TransactionRow.IsLocationNull() == false) ? TransactionRow.Location : "";
			pullslip.ColorMode = PullslipReader.ColorMode.BW;
			pullslip.ExportType = GetExportType(TransactionRow);
			pullslip.Address = GetAddress(TransactionRow, pullslip.ExportType);

			if (LenderAddressRow != null)
				ApplyPreferredExports( pullslip, LenderAddressRow);
			
			return pullslip;
		}
		#endregion

		//PRIVATE METHODS
		#region private methods

		#region GetExportType()
		private ExportType GetExportType(BscanILL.Export.ILLiad.DsIlliad.TransactionsRow transactionsRow)
		{
			if (transactionsRow.IsProcessTypeNull() == false && transactionsRow.ProcessType.ToLower() == "lending")
			{
				if (transactionsRow.IsSystemIDNull() == false && transactionsRow.SystemID.ToLower() == "rapid")
				{
					if (LenderAddressRow.IsOdysseyIPNull() == false)
						return ExportType.Odyssey;
					else if (LenderAddressRow.IsArielAddressNull() == false)
						return ExportType.Ariel;
					else
						return ExportType.SaveOnDisk;
				}

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
		private string GetAddress(BscanILL.Export.ILLiad.DsIlliad.TransactionsRow transactionsRow, ExportType exportType)
		{
			switch (exportType)
			{
				case ExportType.Ariel:
					{
						if (LenderAddressRow != null)
						{
							if (LenderAddressRow.IsArielAddressNull() == false)
								return LenderAddressRow.ArielAddress;
							else
								throw new Exception("Ariel address of lender '" + LenderAddressRow.LenderString + "', ID: '" + LenderAddressRow.AddressNumber + "', NVTGC: '" + LenderAddressRow.NVTGC + "' is not specified!");
						}
						else
							throw new Exception("There is no lender associated with article TN# " + transactionsRow.TransactionNumber + " in the ILLiad SQL database!");
					}
				case ExportType.Email:
					{
						if (LenderAddressRow != null)
						{
							if (LenderAddressRow.IsEMailAddressNull() == false)
								return LenderAddressRow.EMailAddress;
							else
								throw new Exception("E-mail address of lender '" + LenderAddressRow.LenderString + "', ID: '" + LenderAddressRow.AddressNumber + "', NVTGC: '" + LenderAddressRow.NVTGC + "' is not specified!");
						}
						else
							throw new Exception("There is no lender associated with article TN# " + transactionsRow.TransactionNumber + " in the ILLiad SQL database!");
					}
				case ExportType.Odyssey:
					{
						/*BscanILL.Export.ILLiad.DsIlliad.LenderAddressesRow lendersRow = this.LenderAddresses.FindByLenderStringAddressNumberNVTGC(transactionsRow.LendingLibrary, transactionsRow.LenderAddressNumber, transactionsRow.BorrowerNVTGC);

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

		#region ApplyPreferredExports()
		private void ApplyPreferredExports(PullslipReader.IPullslip pullslip, BscanILL.Export.ILLiad.DsIlliad.LenderAddressesRow r)
		{
			BscanILL.SETTINGS.Settings settings = BscanILL.SETTINGS.Settings.Instance;
			
			if (pullslip.ExportType == ExportType.Ariel || pullslip.ExportType == ExportType.ArielPatron || pullslip.ExportType == ExportType.Email ||
				pullslip.ExportType == ExportType.Ftp || pullslip.ExportType == ExportType.FtpDir || pullslip.ExportType == ExportType.Odyssey || pullslip.ExportType == ExportType.SaveOnDisk)
			{
				int tn = 0;
				int ill = 0;

				try
				{
					tn = int.Parse(pullslip.TransactionId);
					ill = int.Parse(pullslip.IllId);

					if (tn >= 0 && ill < 0)
						ApplyPreferredExport(pullslip, r, settings.Export.ILLiad.PreferredExportPN1, settings.Export.ILLiad.PreferredExportPN2, settings.Export.ILLiad.PreferredExportPN3);
					else if (tn < 0 && ill >= 0)
						ApplyPreferredExport(pullslip, r, settings.Export.ILLiad.PreferredExportNP1, settings.Export.ILLiad.PreferredExportNP2, settings.Export.ILLiad.PreferredExportNP3);
					else if (tn < 0 && ill < 0)
						ApplyPreferredExport(pullslip, r, settings.Export.ILLiad.PreferredExportNN1, settings.Export.ILLiad.PreferredExportNN2, settings.Export.ILLiad.PreferredExportNN3);
					else
						ApplyPreferredExport(pullslip, r, settings.Export.ILLiad.PreferredExportPP1, settings.Export.ILLiad.PreferredExportPP2, settings.Export.ILLiad.PreferredExportPP3);
				}
				catch (Exception)
				{
				}
			}
		}
		#endregion
	
		#region ApplyPreferredExports()
		/*private void ApplyPreferredExports(ExportSettings settings, PullslipReader.IPullslip pullslip, BscanILL.Export.ILLiad.DsIlliad.LenderAddressesRow r)
		{
			if (pullslip.ExportType == ExportType.Ariel || pullslip.ExportType == ExportType.ArielPatron || pullslip.ExportType == ExportType.Email ||
				pullslip.ExportType == ExportType.Ftp || pullslip.ExportType == ExportType.Odyssey || pullslip.ExportType == ExportType.SaveOnDisk)
			{
				if(settings.Export.ILLiad.PreferredExportPP1 == ExportType.None)
				{
				}
				else if ((settings.Export.ILLiad.PreferredExportPP1 == ExportType.Odyssey) && (r.IsOdysseyIPNull() == false && r.OdysseyIP.Length > 0))
				{
					pullslip.ExportType = ExportType.Odyssey;
					pullslip.Address = r.OdysseyIP;
				}
				else if ((settings.Export.ILLiad.PreferredExportPP1 == ExportType.Ariel) && (r.IsArielAddressNull() == false && r.ArielAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Ariel;
					pullslip.Address = r.ArielAddress;
				}
				else if ((settings.Export.ILLiad.PreferredExportPP1 == ExportType.Email) && (r.IsEMailAddressNull() == false && r.EMailAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Email;
					pullslip.Address = r.EMailAddress;
				}
				else if (settings.Export.ILLiad.PreferredExportPP1 == ExportType.Ftp)
				{
					pullslip.ExportType = ExportType.Ftp;
					pullslip.Address = "";
				}
				else if (settings.Export.ILLiad.PreferredExportPP1 == ExportType.SaveOnDisk)
				{
					pullslip.ExportType = ExportType.SaveOnDisk;
					pullslip.Address = "";
				}

				else if (settings.Export.ILLiad.PreferredExportPP2 == ExportType.None)
				{
				}
				else if ((settings.Export.ILLiad.PreferredExportPP2 == ExportType.Odyssey) && (r.IsOdysseyIPNull() == false && r.OdysseyIP.Length > 0))
				{
					pullslip.ExportType = ExportType.Odyssey;
					pullslip.Address = r.OdysseyIP;
				}
				else if ((settings.Export.ILLiad.PreferredExportPP2 == ExportType.Ariel) && (r.IsArielAddressNull() == false && r.ArielAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Ariel;
					pullslip.Address = r.ArielAddress;
				}
				else if ((settings.Export.ILLiad.PreferredExportPP2 == ExportType.Email) && (r.IsEMailAddressNull() == false && r.EMailAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Email;
					pullslip.Address = r.EMailAddress;
				}
				else if (settings.Export.ILLiad.PreferredExportPP2 == ExportType.Ftp)
				{
					pullslip.ExportType = ExportType.Ftp;
					pullslip.Address = "";
				}
				else if (settings.Export.ILLiad.PreferredExportPP2 == ExportType.SaveOnDisk)
				{
					pullslip.ExportType = ExportType.SaveOnDisk;
					pullslip.Address = "";
				}

				else if (settings.Export.ILLiad.PreferredExportPP3 == ExportType.None)
				{
				}
				else if ((settings.Export.ILLiad.PreferredExportPP3 == ExportType.Odyssey) && (r.IsOdysseyIPNull() == false && r.OdysseyIP.Length > 0))
				{
					pullslip.ExportType = ExportType.Odyssey;
					pullslip.Address = r.OdysseyIP;
				}
				else if ((settings.Export.ILLiad.PreferredExportPP3 == ExportType.Ariel) && (r.IsArielAddressNull() == false && r.ArielAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Ariel;
					pullslip.Address = r.ArielAddress;
				}
				else if ((settings.Export.ILLiad.PreferredExportPP3 == ExportType.Email) && (r.IsEMailAddressNull() == false && r.EMailAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Email;
					pullslip.Address = r.EMailAddress;
				}
				else if (settings.Export.ILLiad.PreferredExportPP3 == ExportType.Ftp)
				{
					pullslip.ExportType = ExportType.Ftp;
					pullslip.Address = "";
				}
				else if (settings.Export.ILLiad.PreferredExportPP3 == ExportType.SaveOnDisk)
				{
					pullslip.ExportType = ExportType.SaveOnDisk;
					pullslip.Address = "";
				}
			}
		}*/
		#endregion

		#region ApplyPreferredExport()
		private void ApplyPreferredExport(PullslipReader.IPullslip pullslip, BscanILL.Export.ILLiad.DsIlliad.LenderAddressesRow r, ExportType e1, ExportType e2, ExportType e3)
		{
			if (pullslip.ExportType == ExportType.Ariel || pullslip.ExportType == ExportType.ArielPatron || pullslip.ExportType == ExportType.Email ||
				pullslip.ExportType == ExportType.Ftp || pullslip.ExportType == ExportType.FtpDir || pullslip.ExportType == ExportType.Odyssey || pullslip.ExportType == ExportType.SaveOnDisk)
			{
				if (e1 == ExportType.None)
				{
				}
				else if ((e1 == ExportType.Odyssey) && (r.IsOdysseyIPNull() == false && r.OdysseyIP.Length > 0))
				{
					pullslip.ExportType = ExportType.Odyssey;
					pullslip.Address = r.OdysseyIP;
				}
				else if ((e1 == ExportType.Ariel) && (r.IsArielAddressNull() == false && r.ArielAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Ariel;
					pullslip.Address = r.ArielAddress;
				}
				else if ((e1 == ExportType.Email) && (r.IsEMailAddressNull() == false && r.EMailAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Email;
					pullslip.Address = r.EMailAddress;
				}
				else if (e1 == ExportType.Ftp)
				{
					pullslip.ExportType = ExportType.Ftp;
					pullslip.Address = "";
				}
				else if (e1 == ExportType.FtpDir)
				{
					pullslip.ExportType = ExportType.FtpDir;
					pullslip.Address = "";
				}
				else if (e1 == ExportType.SaveOnDisk)
				{
					pullslip.ExportType = ExportType.SaveOnDisk;
					pullslip.Address = "";
				}

				else if (e2 == ExportType.None)
				{
				}
				else if ((e2 == ExportType.Odyssey) && (r.IsOdysseyIPNull() == false && r.OdysseyIP.Length > 0))
				{
					pullslip.ExportType = ExportType.Odyssey;
					pullslip.Address = r.OdysseyIP;
				}
				else if ((e2 == ExportType.Ariel) && (r.IsArielAddressNull() == false && r.ArielAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Ariel;
					pullslip.Address = r.ArielAddress;
				}
				else if ((e2 == ExportType.Email) && (r.IsEMailAddressNull() == false && r.EMailAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Email;
					pullslip.Address = r.EMailAddress;
				}
				else if (e2 == ExportType.Ftp)
				{
					pullslip.ExportType = ExportType.Ftp;
					pullslip.Address = "";
				}
				else if (e2 == ExportType.FtpDir)
				{
					pullslip.ExportType = ExportType.FtpDir;
					pullslip.Address = "";
				}
				else if (e2 == ExportType.SaveOnDisk)
				{
					pullslip.ExportType = ExportType.SaveOnDisk;
					pullslip.Address = "";
				}

				else if (e3 == ExportType.None)
				{
				}
				else if ((e3 == ExportType.Odyssey) && (r.IsOdysseyIPNull() == false && r.OdysseyIP.Length > 0))
				{
					pullslip.ExportType = ExportType.Odyssey;
					pullslip.Address = r.OdysseyIP;
				}
				else if ((e3 == ExportType.Ariel) && (r.IsArielAddressNull() == false && r.ArielAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Ariel;
					pullslip.Address = r.ArielAddress;
				}
				else if ((e3 == ExportType.Email) && (r.IsEMailAddressNull() == false && r.EMailAddress.Length > 0))
				{
					pullslip.ExportType = ExportType.Email;
					pullslip.Address = r.EMailAddress;
				}
				else if (e3 == ExportType.Ftp)
				{
					pullslip.ExportType = ExportType.Ftp;
					pullslip.Address = "";
				}
				else if (e3 == ExportType.FtpDir)
				{
					pullslip.ExportType = ExportType.FtpDir;
					pullslip.Address = "";
				}
				else if (e3 == ExportType.SaveOnDisk)
				{
					pullslip.ExportType = ExportType.SaveOnDisk;
					pullslip.Address = "";
				}
			}
		}
		#endregion
	
		#endregion

	}
}
