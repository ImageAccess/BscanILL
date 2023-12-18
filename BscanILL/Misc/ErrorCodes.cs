using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Misc
{
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

		FileOverSizeLimit = 550,

		IrisGeneral = 600,

		Unknown = int.MaxValue
	}
}
