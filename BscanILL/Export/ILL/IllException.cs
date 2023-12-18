using System;
using System.Collections;
using System.Text;

namespace BscanILL.Export.ILL
{
	/*public class IllException : Exception
	{
		ErrorCode errorCode;
		//String message;
		
		public IllException(ErrorCode errorCode)
			: this(errorCode, GetErrorCodeMessage(errorCode))
		{
			this.errorCode = errorCode;
		}

		public IllException(ErrorCode errorCode, string message)
			:base(message)
		{
			this.errorCode = errorCode;
		}

		public ErrorCode ErrorCode { get { return this.errorCode; } }

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
				case ErrorCode.EmailAttachToBig: return "The attachment is bigger than maximum allowed size!";

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

				default: return "Unexpected exception raised!";
			}
		}
	}

	public enum ErrorCode : int
	{
		Ok = 0,
		ArielPathIncorrect = 1,
		UnexpectedExportType = 2,
		ArielCantLaunch = 3,
		ArielLaunchTimeout = 4,

		StartArchiveNotEmpty = 16,
		StartMenuHandlesIncorrect = 17,
		StartCantGetArchiveWindowHandle = 18,
		StartCanNotCloseErrorMessage = 19,
		StartCanNotCloseImportWnd = 20,
		StartCanNotCloseSaveWnd = 21,
		StartCanNotCloseSendWnd = 22,

		ImportCanNotOpenWnd = 32,
		ImportCanNotLocateEditBox = 33,
		ImportCanNotLocateOkButton = 34,
		ImportCanNotLocateCancelButton = 35,
		ImportCanNotEnterImagePath = 36,
		ImportCanNotPressOKButton = 37,
		ImportBadFileFormat = 38,
		ImportFileNotFound = 39,
		ImportCanNotCloseErrorMessage = 40,
		ImportWndStandsOpen = 41,

		SaveCanNotLocateWnd = 48,
		SaveCanNotLocateDocIdBox = 49,
		SaveCanNotLocatePatronBox = 50,
		SaveCanNotLocateNoteBox = 51,
		SaveCanNotLocateFileBox = 52,
		SaveCanNotLocateDescriptionBox = 53,
		SaveCanNotLocateOkButton = 54,
		SaveCanNotEnterDocId = 55,
		SaveCanNotEnterPatron = 56,
		SaveCanNotEnterNote = 57,
		SaveCanNotEnterFile = 58,
		SaveCanNotEnterDescription = 59,
		SaveCanNotPressOKButton = 60,
		SaveWndStandsOpen = 61,

		ListItemCantActivateArchive = 72,
		ListItemNoItem = 73,
		ListItemRemoteBufferNotAlloc = 74,
		ListItemCantSelectItem = 75,
		ListItemCantUnselectItems = 76,
		ListItemCantWriteToArielProcess = 77,

		SendCantFindWindow = 88,
		SendCanNotLocateDocIdBox = 89,
		SendCanNotLocatePatronBox = 90,
		SendCanNotLocateNoteBox = 91,
		SendCanNotLocateToBox = 92,
		SendCanNotLocateOkButton = 93,
		SendCanNotLocateCancelButton = 94,
		SendCanNotEnterData = 95,
		SendCantPressOkButton = 96,
		SendBadRecipientAddress = 97,
		SendWndStandsOpen = 98,
		SendCantCloseErrorMessage = 99,

		DeleteCantLocateWnd = 104,
		DeleteCantConfirm = 105,

		UnexpectedException = 127,

		IlliadNotRunning = 256,
		IlliadNoWindowHndl = 257,
		ILLiadUnsupportedVersion = 269,
		IlliadMenuHndlsIncorrect = 258,
		IlliadCantSwitchToLending = 259,
		IlliadCantCloseSelectWindow = 264,
		IlliadUpdateWindowOpen = 265,
		IlliadScanWindowOpen = 266,
		IlliadCantBringWindowToFront = 267,
		ILLiadWindowIsNotResponding = 268,

		IlliadCantSelectMenuUpdate = 272,
		ILLiadCantOpenLandingScanning = 298,
		ILLiadCantLocateLendingScanningControls = 299,
		ILLiadCantEnterTransactionNumToLending = 300,
		ILLiadInvalidTransactionNumber = 301,
		ILLiadCantLocateSelectWindow = 302,
		ILLiadCantLocateSelectControls = 303,
		ILLiadCantEnterDataToSelectWnd = 304,
		ILLiadCantPressDeliverButton = 305,
		ILLiadSelectWndNotClosing = 306,

		ILLiadCantLocateUpdateStacksSearchWnd = 320,
		ILLiadCantLocateUpdateStacksSearchControls = 321,
		ILLiadCantEnterDataToUpdateStacksSearchWnd = 322,
		ILLiadCantPressSearchButtonInUpdateStacksSearchWnd = 323,
		ILLiadUpdateStacksSearchArticleNotFound = 324,
		ILLiadCantPressScanButtonInUpdateStacksSearchWnd = 325,
		ILLiadUpdateStacksSearchNotClosing = 326,
		ILLiadCantCheckRadioButtonInUpdateStacksSearchWnd = 327,

		ILLiadCantLocateDocDelineryScanningWnd = 330,
		ILLiadCantLocateDocDeliveryScanningControls = 331,
		ILLiadDocDeliveryScanningNotClosing = 332,
		ILLiadDocDelScanningTWAINViolationCantLocateControl = 333,

		ILLiadCantLocateUpdateStacksFormWnd = 340,
		ILLiadCantLocateUpdateStacksFormControls = 341,
		ILLiadCantGetFrameCountInUpdateStacksFormWnd = 342,
		ILLiadCantEnterDataToUpdateStacksForm = 343,
		ILLiadUpdateStacksFormNotClosing = 344,

		ILLiadCantLocateBillingFormWnd = 380,
		ILLiadCantLocateBillingFormControls = 381,
		ILLiadBillingFormNotClosing = 382,

		ILLiadCantLocateBillingConfirmationControls = 390,
		ILLiadBillingConfirmationNotClosing = 391,
	
		ILLiadCantLocateGeneralSearchWnd = 350,
		ILLiadCantLocateGeneralSearchControls = 351,
		ILLiadCantEnterDataToGeneralSearchForm = 352,
		ILLiadGeneralSearchNotClosing = 353,

		ILLiadCantLocateGeneralUpdateWnd = 360,
		ILLiadCantLocateGeneralUpdateInfoWnd = 361,
		ILLiadGeneralUpdateNotClosing = 362,

		ILLiadCantLocateInfoYesButton = 370,
		ILLiadInfoWndNotClosing = 371,

		EmailCantCreateMessage = 400,
		EmailCantSendMessage = 401,
		EmailAttachDoesntExist = 402,
		EmailAttachToBig = 403,

		FtpCantConnectToExportDir = 410,

		SaveOnDiskNoDirectory = 420,

		SendToPatronCantFindWindow = 450,
		SendToPatronCantLocateListBoxPatrons = 451,
		SendToPatronCantLocateBoxEmail = 452,
		SendToPatronCantLocateButtonSend = 453,
		SendToPatronCantLocateButtonCancel = 454,
		SendToPatronNoRecords = 455,
		SendToPatronNoAppropriateRecord = 456,
		SendToPatronWndStandsOpen = 457,
		SendToPatronCantCloseErrorMessage = 458,
		SendToPatronCantActivateWnd = 459,
		SendToPatronNoPatrons = 460,
		SendToPatronCantSelectItem = 461,
		SendToPatronEmailBoxNotChanging = 462,
		SendToPatronCantWriteToArielProcess = 463,
		SendToPatronCantUnselectItems = 464,

		FtpCanceledByUser = 500,
		FtpCantLogin = 501,
		FtpNotConnectedToServer = 502,
		FtpCantCreateRemoteDir = 503,
		FtpCantgetRemoreDirList = 504,
		FtpCantCopyFile = 505,
	}*/

}
