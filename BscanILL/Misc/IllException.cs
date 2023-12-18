using System;
using BscanILL.UI.Dialogs;

namespace BscanILL.Misc
{
	/// <summary>
	/// Summary description for IllException.
	/// </summary>
	public class IllException : Exception
	{
		public readonly AlertDlg.AlertDlgType	AlertType = AlertDlg.AlertDlgType.Error;
		public readonly ErrorCode				ErrorCode = ErrorCode.Unknown;


		#region constructor
		public IllException(string message)
			: base(message)
		{
		}

		public IllException(Exception ex)
			: base(ex.Message, ex)
		{
		}

		public IllException(string message, AlertDlg.AlertDlgType alertType)
			: base(message)
		{
			this.AlertType = alertType;
		}

		public IllException(ErrorCode errorCode)
			: this(errorCode, GetErrorCodeMessage(errorCode))
		{
			this.ErrorCode = errorCode;
		}

		public IllException(ErrorCode errorCode, string message)
			: base(message)
		{
			this.ErrorCode = errorCode;
		}
		#endregion


		#region GetErrorCodeMessage()
		public static string GetErrorCodeMessage(ErrorCode errorCode)
		{
			switch (errorCode)
			{
				case ErrorCode.Ok: return "OK.";
				case ErrorCode.UnexpectedExportType: return "Unexpected export type!";
				case ErrorCode.ArielCantLaunch: return "Can't launch Ariel!";
				case ErrorCode.ArielLaunchTimeout: return "Timeout occured while launching Ariel!";

				case ErrorCode.ArielPathIncorrect: return "Ariel path is incorrect!";
				case ErrorCode.StartArchiveNotEmpty: return "Archive window is not empty!";
				case ErrorCode.StartMenuHandlesIncorrect: return "Menu handles are not correct!";
				case ErrorCode.StartCantGetArchiveWindowHandle: return "Can't open Archive window!";
				case ErrorCode.StartCanNotCloseErrorMessage: return "Can't close Ariel error message window!";
				case ErrorCode.StartCanNotCloseImportWnd: return "Can't close Ariel 'Import' window!";
				case ErrorCode.StartCanNotCloseSaveWnd: return "Can't close Ariel 'Save' window!";
				case ErrorCode.StartCanNotCloseSendWnd: return "Can't close Ariel 'Send' window!";

				case ErrorCode.ImportCanNotOpenWnd: return "Can't open Import window!";
				case ErrorCode.ImportCanNotLocateEditBox: return "Can't locate edit box in Import window!";
				case ErrorCode.ImportCanNotLocateOkButton: return "Can't locate OK button in Import window!";
				case ErrorCode.ImportCanNotLocateCancelButton: return "Can't locate Cancel button in Import window!";

				case ErrorCode.ImportCanNotEnterImagePath: return "Can't enter image path to Import window!";
				case ErrorCode.ImportCanNotPressOKButton: return "Can't press OK button in Import window!";
				case ErrorCode.ImportBadFileFormat: return "Ariel didn't accept article image file. It might be caused by sending image(s) bigger than 11\"x17\". Crop images first!";
				case ErrorCode.ImportFileNotFound: return "Article image file doesn't exist!";
				case ErrorCode.ImportCanNotCloseErrorMessage: return "Can't close Import Error message!";
				case ErrorCode.ImportWndStandsOpen: return "Import window stands open! The source image file might not exist!";

				case ErrorCode.SaveCanNotLocateWnd: return "Can't localize Save window!";
				case ErrorCode.SaveCanNotLocateDocIdBox: return "Can't locate Document ID text box in Save window!";
				case ErrorCode.SaveCanNotLocatePatronBox: return "Can't locate text box Patron in Save window!";
				case ErrorCode.SaveCanNotLocateNoteBox: return "Can't locate text box Note in Save window!";
				case ErrorCode.SaveCanNotLocateFileBox: return "Can't locate text box File Name in Save window!";
				case ErrorCode.SaveCanNotLocateDescriptionBox: return "Can't locate text box Description in Save window!";
				case ErrorCode.SaveCanNotLocateOkButton: return "Can't locate OK button in Save window!";
				case ErrorCode.SaveCanNotEnterDocId: return "Can't enter Document ID into Save window!";
				case ErrorCode.SaveCanNotEnterPatron: return "Can't enter Patron into Save window!";
				case ErrorCode.SaveCanNotEnterNote: return "Can't enter Note into Save window!";
				case ErrorCode.SaveCanNotEnterFile: return "Can't enter file path into Save window!";
				case ErrorCode.SaveCanNotEnterDescription: return "Can't enter description into Save window!";
				case ErrorCode.SaveCanNotPressOKButton: return "Can't press OK button in Save window!";
				case ErrorCode.SaveWndStandsOpen: return "Save window stands open!";

				case ErrorCode.ListItemCantActivateArchive: return "Can't activate Archive window!";
				case ErrorCode.ListItemNoItem: return "No item displayed in Archive list view!";

				case ErrorCode.SendCanNotLocateDocIdBox: return "Can't locate text box Document ID in Send window!";
				case ErrorCode.SendCanNotLocatePatronBox: return "Can't locate text box Patron in Send window!";
				case ErrorCode.SendCanNotLocateNoteBox: return "Can't locate text box Note in Send window!";
				case ErrorCode.SendCanNotLocateToBox: return "Can't locate text box To in Send window!";
				case ErrorCode.SendCanNotLocateOkButton: return "Can't locate OK button in Send window!";
				case ErrorCode.SendCanNotLocateCancelButton: return "Can't locate Cancel button in Send window!";
				case ErrorCode.SendCanNotEnterData: return "Can't enter data to Send window!";
				case ErrorCode.SendCantPressOkButton: return "Can't press OK button!";
				case ErrorCode.SendBadRecipientAddress: return "The recipint's address is not stored in the Ariel address list!";
				case ErrorCode.SendWndStandsOpen: return "Send window stands open!";

				case ErrorCode.SendCantFindWindow: return "Can't find Send window!";
				case ErrorCode.SendCantCloseErrorMessage: return "Can't close 'Invalid Address' dialog window!";

				case ErrorCode.DeleteCantLocateWnd: return "Can't find Delete dialog!";
				case ErrorCode.DeleteCantConfirm: return "Can't confirm Delete dialog!";
				//case ErrorCode.: ;
				case ErrorCode.IlliadNotRunning: return "ILLiad is not running!";
				case ErrorCode.IlliadNoWindowHndl: return "ILLiad is running, but ILLiad window can't be localized!";
				case ErrorCode.ILLiadUnsupportedVersion: return "Unsupported version of illiad is running!";
				case ErrorCode.IlliadMenuHndlsIncorrect: return "Menu handles are not correct! Probably unexpected version of ILLiad.";
				case ErrorCode.IlliadCantSwitchToLending: return "Can't switch to lending mode!";
				case ErrorCode.IlliadCantCloseSelectWindow: return "Can't close Select window!";
				case ErrorCode.IlliadUpdateWindowOpen: return "Update Stacks window in ILLIad is open!";
				case ErrorCode.IlliadScanWindowOpen: return "Odyssey scanning window in ILLIad is open!";
				case ErrorCode.IlliadCantBringWindowToFront: return "Can't bring ILLiad window to the front!";
				case ErrorCode.ILLiadWindowIsNotResponding: return "ILLiad window is not responding!";

				case ErrorCode.IlliadCantSelectMenuUpdate: return "Can't select menu item Update Stacks Search Results from menu!";
				case ErrorCode.ILLiadCantOpenLandingScanning: return "Can't locate \"Lending Scanning\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantLocateLendingScanningControls: return "Can't locate controls in Lending Scanning Window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantEnterTransactionNumToLending: return "Can't enter Transaction # to Lending Scanning window!";
				case ErrorCode.ILLiadInvalidTransactionNumber: return "The transaction specified could not be found or is not at the correct status.";
				case ErrorCode.ILLiadCantLocateSelectWindow: return "Can't find \"Select Image To Import\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantLocateSelectControls: return "Can't locate controls in \"Select Image To Import\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantEnterDataToSelectWnd: return "Can't enter image path to \"Select Image To Import\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantPressDeliverButton: return "Can't press Deliver Button in \"Lending Scanning\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadSelectWndNotClosing: return "\"Document Delivery Scanning\" window is not closing! Please open up ILLiad and check status.";

				case ErrorCode.ILLiadCantLocateUpdateStacksSearchWnd: return "Can't locate \"Document Delivery - Update Stacks Search\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantLocateUpdateStacksSearchControls: return "Can't locate controls in \"Update Stacks Search\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantEnterDataToUpdateStacksSearchWnd: return "Can't enter Transaction Number to \"Update Stacks Search\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantCheckRadioButtonInUpdateStacksSearchWnd: return "Can't check radio button 'No' in \"Update Stacks Search\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantPressSearchButtonInUpdateStacksSearchWnd: return "Can't press Search button in \"Update Stacks Search\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadUpdateStacksSearchArticleNotFound: return "Article not found in \"Update Stacks Search Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantPressScanButtonInUpdateStacksSearchWnd: return "Can't press Scan Now button in \"Update Stacks Search\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadUpdateStacksSearchNotClosing: return "\"Document Delivery - Update Stacks Search\" window is not closing! Please open up ILLiad and check status.";

				case ErrorCode.ILLiadCantLocateDocDelineryScanningWnd: return "Can't locate \"Document Delivery Scanning\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantLocateDocDeliveryScanningControls: return "Can't locate controls in \"Document Delivery Scanning\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadDocDeliveryScanningNotClosing: return "\"Document Delivery Scanning\" window is not closing! Please open up ILLiad and check status.";

				case ErrorCode.ILLiadCantLocateUpdateStacksFormWnd: return "Can't locate \"Update Stacks Search Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantLocateUpdateStacksFormControls: return "Can't locate controls in \"Update Stacks Search Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantGetFrameCountInUpdateStacksFormWnd: return "Can't get page count in \"Update Stacks Search Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantEnterDataToUpdateStacksForm: return "Can't enter Transaction Number to \"Update Stacks Search Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadUpdateStacksFormNotClosing: return "\"Update Stacks Search Form\" is not closing! Please open up ILLiad and check status.";

				case ErrorCode.ILLiadCantLocateBillingFormWnd: return "Can't locate \"Add Billing Charges Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantLocateBillingFormControls: return "Can't locate \"Add Billing Charges Form\" window controls! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadBillingFormNotClosing: return "\"Add Billing Charges Form\" is not closing! Please open up ILLiad and check status.";

				case ErrorCode.ILLiadCantLocateBillingConfirmationControls: return "Can't locate \"Information\" window controls! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadBillingConfirmationNotClosing: return "\"Information\" is not closing! Please open up ILLiad and check status.";

				case ErrorCode.ILLiadCantLocateGeneralSearchWnd: return "Can't locate \"General Search Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantLocateGeneralSearchControls: return "Can't locate controls in \"General Search Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantEnterDataToGeneralSearchForm: return "Can't enter ILL Number to \"General Search Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadGeneralSearchNotClosing: return "\"General Search Form\" window is not closing! Please open up ILLiad and check status.";

				case ErrorCode.ILLiadCantLocateGeneralUpdateWnd: return "Can't locate \"General Update Form\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadGeneralUpdateNotClosing: return "\"General Update Form\" window is not closing! Please open up ILLiad and check status.";

				case ErrorCode.ILLiadCantLocateGeneralUpdateInfoWnd: return "Can't locate \"Information\" window! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadCantLocateInfoYesButton: return "Can't locate Yes button in \"Information\" message form! Please open up ILLiad and check status.";
				case ErrorCode.ILLiadInfoWndNotClosing: return "\"Information\" message form is not closing! Please open up ILLiad and check status.";

				case ErrorCode.EmailCantCreateMessage: return "Can't create email!";
				case ErrorCode.EmailCantSendMessage: return "Can't send email!";
				case ErrorCode.EmailAttachDoesntExist: return "Can't find attachment file!";
				case ErrorCode.EmailAttachToBig: return "The attachment is bigger than SMTP maximum allowed size!";

				case ErrorCode.FtpCantConnectToExportDir: return "Can't connect to FTP remote directory!";

				case ErrorCode.SaveOnDiskNoDirectory: return "No export directory specified!";

				case ErrorCode.SendToPatronCantFindWindow: return "Can't locate Send to Patron window!";
				case ErrorCode.SendToPatronCantLocateListBoxPatrons: return "Can't locate list box with patrons in Send to Patrons window!";
				case ErrorCode.SendToPatronCantLocateBoxEmail: return "Can't locate text box E-Mail in Send to Patron window!";
				case ErrorCode.SendToPatronCantLocateButtonSend: return "Can't locate button Send in Send to Patron window!";
				case ErrorCode.SendToPatronCantLocateButtonCancel: return "Can't locate button Cancel in Send to Patron window!";
				case ErrorCode.SendToPatronNoRecords: return "There are no Patrons in patrons list!";
				case ErrorCode.SendToPatronNoAppropriateRecord: return "Email address was not found!";

				case ErrorCode.SendToPatronWndStandsOpen: return "Send to Patrons window stands open!";
				case ErrorCode.SendToPatronCantCloseErrorMessage: return "Can't close error message window!";
				case ErrorCode.SendToPatronCantActivateWnd: return "Can't activate Send to Patron window!";
				case ErrorCode.SendToPatronNoPatrons: return "There are no patrons in patron's list!";
				case ErrorCode.SendToPatronEmailBoxNotChanging: return "Email field is not changign in Send to Patron window!";

				case ErrorCode.FtpCanceledByUser: return "FTP export was canceled by user!";
				case ErrorCode.FtpNotConnectedToServer: return "There is no connection to the FTP server!";

				case ErrorCode.FileOverSizeLimit: return "File size is over size limit!";

				case ErrorCode.IrisGeneral: return "OCR general error!";

				default: return "Unexpected exception raised!";
			}
		}
		#endregion

	}
}
