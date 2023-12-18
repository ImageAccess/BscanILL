using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using BscanILL.Misc;
using BscanILL.Hierarchy;

namespace BscanILL.Export.Ariel
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Ariel3_X : ArielBasics, IAriel
	{
		#region constructor
		public Ariel3_X()
			: base()
		{
		}
		#endregion

		#region destructor
		~Ariel3_X()
		{
			Dispose();
		}
		#endregion


		//PUBLIC METHODS
		#region public methods

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region Export()
		protected override void Export(ExportUnit exportUnit)
		{
			Article article = exportUnit.Article;
			
			if ((article.IllNumber == null || article.IllNumber.Trim().Length == 0) && (article.TransactionId != null))
				article.IllNumber = article.TransactionId.ToString();

			ProcessImportWindow(exportUnit);
			ProcessSaveWindow(exportUnit);
			SelectFirstListViewItem(exportUnit);
			Progress_Changed(exportUnit, 65, "");
			ProcessSendWindow(exportUnit);
			SelectFirstListViewItem(exportUnit);
			DeleteItemFromArchive(exportUnit);
		}
		#endregion

		#region GetMenuHandles()
		protected override void GetMenuHandles()
		{
			try
			{
				IntPtr hWndMenu = GetMenu(this.hWnd);
				IntPtr hWndSubMenuDoc = GetSubMenu(hWndMenu, 0);
				this.hWndMenuSend = GetMenuItemID(hWndSubMenuDoc, 3);
				this.hWndMenuDelete = GetMenuItemID(hWndSubMenuDoc, 7);
				this.hWndMenuArchive = GetMenuItemID(hWndSubMenuDoc, 21);
				this.hWndMenuImport = GetMenuItemID(hWndSubMenuDoc, 23);
				this.hWndMenuImportPatrons = 0;
			}
			catch (IllException ex)
			{
				throw new IllException(ErrorCode.StartMenuHandlesIncorrect, "Menu handles are not correct! Unexpected version of Ariel.\n" + ex.Message);
			}
		}
		#endregion

		#region CloseUnexpectedWindows()
		protected override void CloseUnexpectedWindows()
		{
			IntPtr hWndDialog;
			IntPtr hWndControl;
			int result;

			hWndDialog = FindWindow("#32770", "Ariel");
			if (hWndDialog != IntPtr.Zero)
			{
				if ((hWndControl = FindWindowEx(hWndDialog, IntPtr.Zero, "Button", "OK")) != IntPtr.Zero)
				{
					result = SendMessage(hWndDialog, WM_SETFOCUS, hWndControl.ToInt32(), 0);
					result = SendMessage(hWndControl, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
					result += SendMessage(hWndControl, WM_LBUTTONUP, 0, 65536 * 14 + 19);
					//SendMessage(hWndControl, WM_COMMAND, 1, 0);
					if (result > 0)
						throw new IllException(ErrorCode.StartCanNotCloseErrorMessage);
				}
				else
					throw new IllException(ErrorCode.StartCanNotCloseErrorMessage);

				Thread.Sleep(200);
			}

			hWndDialog = FindWindow("#32770", "Import");
			if (hWndDialog != IntPtr.Zero)
			{
				if ((hWndControl = FindWindowEx(hWndDialog, IntPtr.Zero, "Button", "Cancel")) != IntPtr.Zero)
				{
					result = SendMessage(hWndControl, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
					result += SendMessage(hWndControl, WM_LBUTTONUP, 0, 65536 * 14 + 19);
					if (result > 0)
						throw new IllException(ErrorCode.StartCanNotCloseImportWnd);
				}
				else
					throw new IllException(ErrorCode.StartCanNotCloseImportWnd);

				Thread.Sleep(200);
			}

			hWndDialog = FindWindow("#32770", "Save");
			if (hWndDialog != IntPtr.Zero)
			{
				if ((hWndControl = FindWindowEx(hWndDialog, IntPtr.Zero, "Button", "Cancel")) != IntPtr.Zero)
				{
					result = SendMessage(hWndControl, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
					result += SendMessage(hWndControl, WM_LBUTTONUP, 0, 65536 * 14 + 19);
					if(result > 0)
						throw new IllException(ErrorCode.StartCanNotCloseSaveWnd);
				}
				else
					throw new IllException(ErrorCode.StartCanNotCloseSaveWnd);

				Thread.Sleep(200);
			}

			hWndDialog = FindWindow("#32770", "Send");
			if (hWndDialog != IntPtr.Zero)
			{
				if ((hWndControl = FindWindowEx(hWndDialog, IntPtr.Zero, "Button", "Cancel")) != IntPtr.Zero)
				{
					result = SendMessage(hWndControl, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
					result += SendMessage(hWndControl, WM_LBUTTONUP, 0, 65536 * 14 + 19);
					if (result > 0)
						throw new IllException(ErrorCode.StartCanNotCloseSendWnd);
				}
				else
					throw new IllException(ErrorCode.StartCanNotCloseSendWnd);

				Thread.Sleep(200);
			}
		}
		#endregion

		#region InvalidDocumentFormatDialogPoppedUp()
		protected override bool InvalidDocumentFormatDialogPoppedUp()
		{
			bool dialogPoppedUp = false;
			IntPtr hWndDialog;
			IntPtr hWndOk;

			hWndDialog = FindWindow("#32770", "Ariel");

			if (hWndDialog != IntPtr.Zero)
			{
				dialogPoppedUp = true;
				hWndOk = FindWindowEx(hWndDialog, IntPtr.Zero, "Button", "OK");

				if (hWndOk != IntPtr.Zero)
				{
					int result;

					Thread.Sleep(1000);
					result = SendMessage(hWndDialog, WM_SETFOCUS, hWndOk.ToInt32(), 0);
					result += SendMessage(hWndOk, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
					result += SendMessage(hWndOk, WM_LBUTTONUP, 0, 65536 * 14 + 19);

					if (result > 0)
						throw new IllException(ErrorCode.ImportCanNotCloseErrorMessage);
				}
			}

			Thread.Sleep(1000);
			return dialogPoppedUp;
		}
		#endregion

		#region ProcessSaveWindow()
		private void ProcessSaveWindow(ExportUnit exportUnit)
		{
			Article article = exportUnit.Article;

			long	result1 = 0;
			long	result2 = 0;
			long	result = 0;
			IntPtr	hWndSaveDialog;
			IntPtr	hWndDocumentId;
			IntPtr	hWndPatron;
			IntPtr	hWndNote;
			IntPtr	hWndFileName;
			IntPtr	hWndDescription;
			IntPtr	hWndOkButton;
			int		attempts = 0;

			Progress_Comment(exportUnit, "Waiting while Save window is open...");
			
			//wait while save window is opened
			while ((hWndSaveDialog = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"#32770", "Save")) == IntPtr.Zero)
			{
				if (attempts > 20)
					throw new IllException(ErrorCode.SaveCanNotLocateWnd);

				attempts++;
				Thread.Sleep(1000);
			}
			
			Progress_Changed(exportUnit, 30, "Save window localized.");
			Progress_Comment(exportUnit, "Localizing Save window controls...");
			Thread.Sleep(1000);
			//get controls handles
			if ((hWndDocumentId = FindWindowEx(hWndSaveDialog, IntPtr.Zero, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SaveCanNotLocateDocIdBox);
			if ((hWndPatron = FindWindowEx(hWndSaveDialog, hWndDocumentId, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SaveCanNotLocatePatronBox);
			if ((hWndNote = FindWindowEx(hWndSaveDialog, hWndPatron, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SaveCanNotLocateNoteBox);
			if ((hWndFileName = FindWindowEx(hWndSaveDialog, hWndNote, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SaveCanNotLocateFileBox);
			if ((hWndDescription = FindWindowEx(hWndSaveDialog, hWndFileName, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SaveCanNotLocateDescriptionBox);
			if ((hWndOkButton = FindWindowEx(hWndSaveDialog, IntPtr.Zero, "Button", "OK")) == IntPtr.Zero)
				throw new IllException(ErrorCode.SaveCanNotLocateOkButton);
			Progress_Changed(exportUnit, 35, "Save window controls localized.");

			Progress_Comment(exportUnit, "Entering Article details...");
			Thread.Sleep(200);
			result1 = SendMessage(hWndDocumentId, WM_SETTEXT, (int)0, article.IllNumber.ToString());
			result2 = SendMessage(hWndDocumentId, WM_CHAR, VkKeyScan('c'), 0);
			result2 += SendMessage(hWndDocumentId, WM_KEYDOWN, 0x0025, 0);
			result2 += SendMessage(hWndDocumentId, WM_KEYDOWN, 0x002E, 0);
			if (result1 != 0)// && result2 == 0)
			{
				Progress_Changed(exportUnit, 40, "Document ID entered successfully.");
			}
			else
			{
				throw new IllException(ErrorCode.SaveCanNotEnterDocId);
			}

			Thread.Sleep(200);
			result1 = SendMessage(hWndPatron, WM_SETTEXT, (int)0, article.Patron);
			result2 = SendMessage(hWndPatron, WM_CHAR, VkKeyScan('c'), 0);
			result2 += SendMessage(hWndPatron, WM_KEYDOWN, 0x0025, 0);
			result2 += SendMessage(hWndPatron, WM_KEYDOWN, 0x002E, 0);
			if (result1 != 0)// && result2 == 0)
			{
				Progress_Changed(exportUnit, 44, "Patron entered successfully.");
			}
			else
			{
				throw new IllException(ErrorCode.SaveCanNotEnterPatron);
			}

			Thread.Sleep(200);
			result1 = SendMessage(hWndNote, WM_SETTEXT, (int)0, exportUnit.Files[0].FullName);
			result2 = SendMessage(hWndNote, WM_CHAR, VkKeyScan('c'), 0);
			result2 += SendMessage(hWndNote, WM_KEYDOWN, 0x0025, 0);
			result2 += SendMessage(hWndNote, WM_KEYDOWN, 0x002E, 0);
			if (result1 != 0)// && result2 == 0)
			{
				Progress_Changed(exportUnit, 48, "Note entered successfully.");
			}
			else
			{
				throw new IllException(ErrorCode.SaveCanNotEnterNote);
			}

			Thread.Sleep(200);
			result1 = SendMessage(hWndDescription, WM_SETTEXT, (int)0, article.IllNumber.ToString());
			result2 = SendMessage(hWndDescription, WM_CHAR, VkKeyScan('c'), 0);
			result2 += SendMessage(hWndDescription, WM_KEYDOWN, 0x0025, 0);
			result2 += SendMessage(hWndDescription, WM_KEYDOWN, 0x002E, 0);
			if (result1 != 0)// && result2 == 0)
			{
				Progress_Changed(exportUnit, 52, "Description entered successfully.");
			}
			else
			{
				throw new IllException(ErrorCode.SaveCanNotEnterDescription);
			}

			Thread.Sleep(300);
			//press OK button
			result = SendMessage(hWndOkButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
			result = SendMessage(hWndOkButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
			Progress_Comment(exportUnit, "Saving...");
			//result = SendMessage(hWndSaveDialog, WM_COMMAND, 1, hWndOkButton.ToInt32());
			//result = SendMessage(hWndSaveDialog, WM_COMMAND, 1, 0);

			if(result == 0)
			{
				Progress_Changed(exportUnit, 56, "OK button pressed.");
			}
			else
			{
				throw new IllException(ErrorCode.SaveCanNotPressOKButton);
			}

			//wait while window is open
			Progress_Comment(exportUnit, "Waiting for Save window to be closed...");
			while (IsWindowVisible(hWndSaveDialog))
			{
				if (attempts > 30)
					throw new IllException(ErrorCode.SaveWndStandsOpen);
				
				attempts++;
				Thread.Sleep(500);
			}

			Progress_Changed(exportUnit, 60, "Save window closed successfully.");
			Thread.Sleep(500);		
		}
		#endregion

		#region SelectFirstListViewItem()
		private void SelectFirstListViewItem(ExportUnit exportUnit)
		{		
			IntPtr		localBuffer = IntPtr.Zero;
			IntPtr		remoteBuffer = IntPtr.Zero;
			LVITEM		lvItem = new LVITEM();
			int			itemCount ;
			uint		MEM_COMMIT = 0x1000;
			uint		PAGE_READWRITE = 4;
			uint		MEM_RELEASE = 0x8000;
			int			bytesWritten = 0;
			int			lvItemSize = Marshal.SizeOf(lvItem);
			int			attempts;

			//Article article

			Progress_Comment(exportUnit, "Activating Archive window...");
			lvItem.mask = LVIF_STATE;
			lvItem.stateMask = LVIS_SELECTED | LVIS_FOCUSED;
			lvItem.state = 0;

			if(SendMessage(hWndArchive, WM_CHILDACTIVATE, 0, 0) != 0)
				throw new IllException(ErrorCode.ListItemCantActivateArchive);
			Progress_Changed(exportUnit, -1, "Archive window activated.");

			attempts = 0;
			Progress_Comment(exportUnit, "Waiting until article shows up in Archive window...");
			while ((itemCount = SendMessage(hWndArchiveListView, LVM_GETITEMCOUNT, 0, 0)) < 1)
			{
				if (attempts > 20)
					throw new IllException(ErrorCode.ListItemNoItem);

				attempts++;
				Thread.Sleep(1);
			}
			Progress_Changed(exportUnit, -1, "Article localized in Archive window.");

			try
			{
				Progress_Comment(exportUnit, "Selecting article...");
				remoteBuffer = VirtualAllocEx(arielProcess.Handle, IntPtr.Zero, 
					lvItemSize, MEM_COMMIT, PAGE_READWRITE);

				if(remoteBuffer == IntPtr.Zero)
					throw new IllException(ErrorCode.ListItemRemoteBufferNotAlloc, "Remote buffer not allocated: " + GetLastError());

				localBuffer = Marshal.AllocHGlobal(lvItemSize);
				Marshal.StructureToPtr(lvItem, localBuffer, false);

				if( WriteProcessMemory(arielProcess.Handle, remoteBuffer, localBuffer, lvItemSize, ref bytesWritten) )
				{
					unchecked 
					{
						if (SendMessage(hWndArchiveListView, LVM_SETITEMSTATE, -1, remoteBuffer.ToInt32()) != 0)
						{
							lvItem.state = LVIS_SELECTED | LVIS_FOCUSED;
							Marshal.StructureToPtr(lvItem, localBuffer, false);

							if (WriteProcessMemory(arielProcess.Handle, remoteBuffer, localBuffer, lvItemSize, ref bytesWritten))
							{
								if (SendMessage(hWndArchiveListView, LVM_SETITEMSTATE, 0, remoteBuffer.ToInt32()) == 0)
									throw new IllException(ErrorCode.ListItemCantSelectItem, "Can't select item in Archive: " + GetLastError());
							}
							else
							{
								throw new IllException(ErrorCode.ListItemCantWriteToArielProcess, "Can't write to Ariel process memory 2: " + GetLastError());
							}
						}
						else
						{
							throw new IllException(ErrorCode.ListItemCantUnselectItems, "Can't unselect items in Archive: " + GetLastError());
						}
					}
				}
				else
				{
					throw new IllException(ErrorCode.ListItemCantWriteToArielProcess, "Can't write to Ariel process memory" + GetLastError());
				}
				Progress_Changed(exportUnit, -1, "Article in Archive window selected.");
			}
			finally
			{
				if(localBuffer != IntPtr.Zero)
					Marshal.FreeHGlobal(localBuffer);

				if(remoteBuffer != IntPtr.Zero)
					VirtualFreeEx(arielProcess.Handle, remoteBuffer, lvItemSize, MEM_RELEASE);
			}
		}
		#endregion

		#region ProcessSendWindow()
		private void ProcessSendWindow(ExportUnit exportUnit)
		{
			long result = 0;
			IntPtr hWndSendDialog;
			IntPtr hWndDocumentId;
			IntPtr hWndPatron;
			IntPtr hWndNote;
			IntPtr hWndTo;
			IntPtr hWndOkButton;
			IntPtr hWndCancelButton;
			int attempts = 0;
			
			
			//Open Send dialog
			Progress_Comment(exportUnit, "Opening Save window...");
			hWndSendDialog = OpenSendWindow();
			Progress_Changed(exportUnit, 70, "Save window open successfully.");

			Thread.Sleep(300);
			Progress_Comment(exportUnit, "Localizing Save window controls...");
			//get controls handles
			if ((hWndDocumentId = FindWindowEx(hWndSendDialog, IntPtr.Zero, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateDocIdBox);
			if ((hWndPatron = FindWindowEx(hWndSendDialog, hWndDocumentId, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocatePatronBox);
			if ((hWndNote = FindWindowEx(hWndSendDialog, hWndPatron, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateNoteBox);
			if ((hWndTo = FindWindowEx(hWndSendDialog, hWndNote, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateToBox);
			if ((hWndOkButton = FindWindowEx(hWndSendDialog, IntPtr.Zero, "Button", "OK")) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateOkButton);
			if ((hWndCancelButton = FindWindowEx(hWndSendDialog, IntPtr.Zero, "Button", "Cancel")) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateCancelButton);
			Progress_Changed(exportUnit, 75, "Save window controls localized.");

			Progress_Comment(exportUnit, "Entering data to Send window...");
			result = SendMessage(hWndPatron, WM_SETTEXT, (int)0, exportUnit.Article.Patron);
			result += SendMessage(hWndTo, WM_SETTEXT, (int)0, exportUnit.Article.Address);

			if (result != 0)
			{
				Progress_Changed(exportUnit, 80, "Send window data entered successfully.");
			}
			else
			{
				throw new IllException(ErrorCode.SendCanNotEnterData);
			}

			Thread.Sleep(300);
			//press OK button
			Progress_Comment(exportUnit, "Sending...");
			result = PostMessage(hWndOkButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
			result += PostMessage(hWndOkButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);

			Progress_Changed(exportUnit, 85, "OK button pressed.");

			Thread.Sleep(1000);
			attempts = 0;

			Progress_Comment(exportUnit, "Waiting for Send window to be closed...");
			while (FindWindow("#32770", "Send") != IntPtr.Zero)
			{
				if (attempts > 10)
				{
					Progress_Comment(exportUnit, "Checking recipient...");
					if (InvalidAddressDialogPoppedUp())
					{
						SendMessage(hWndCancelButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
						SendMessage(hWndCancelButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);

						throw new IllException(ErrorCode.SendBadRecipientAddress);
					}
					else
					{
						throw new IllException(ErrorCode.SendWndStandsOpen);
					}

				}
				else
				{
					Thread.Sleep(1000);
					attempts++;
				}
			}
			Progress_Changed(exportUnit, 90, "Send window closed successfully.");
		}
		#endregion

		#region OpenSendWindow()
		private IntPtr OpenSendWindow()
		{
			IntPtr	hWndSendDialog = IntPtr.Zero;
			int		attempts = 0;
			Thread.Sleep(2000);

			for (int i = 0; i < 7; i++)
			{
				PostMessage(hWnd, WM_COMMAND, (int)this.hWndMenuSend, 0);
				attempts = 0;

				Thread.Sleep(1000);
				while ((attempts < 30) && (hWndSendDialog = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"#32770", "Send")) == IntPtr.Zero)
				{
					attempts++;
					Thread.Sleep(1000);
				}

				if (hWndSendDialog != IntPtr.Zero)
					return hWndSendDialog;
			}

			throw new IllException(ErrorCode.SendCantFindWindow);
		}
		#endregion

		#region InvalidAddressDialogPoppedUp()
		bool InvalidAddressDialogPoppedUp()
		{
			IntPtr hWndDialog = FindWindow("#32770", "Ariel");

			if (hWndDialog != IntPtr.Zero)
			{
				IntPtr hWndOk = FindWindowEx(hWndDialog, IntPtr.Zero, "Button", "OK");

				if (hWndOk != IntPtr.Zero)
				{
					int result;

					Thread.Sleep(1000);
					result = SendMessage(hWndDialog, WM_SETFOCUS, hWndOk.ToInt32(), 0);
					result += SendMessage(hWndOk, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
					result += SendMessage(hWndOk, WM_LBUTTONUP, 0, 65536 * 14 + 19);

					if(result != 0)
						throw new IllException(ErrorCode.SendCantCloseErrorMessage);
				}

				Thread.Sleep(1000);
				return true;
			}
			else
				return false;
		}
		#endregion

		#region DeleteItemFromArchive()
		void DeleteItemFromArchive(ExportUnit exportUnit)
		{
			int attempts = 0;
			int	result = 0;
			IntPtr hWndOkButton;
			IntPtr hWndDeleteDialog = IntPtr.Zero;

			//clear Import list view 
			//result = PostMessage(hWndArchiveListView, WM_KEYDOWN, 0x002E, 0)

			Thread.Sleep(1000);
			attempts = 0;

			Progress_Comment(exportUnit, "Waiting for Delete dialog to pop up...");
			for (int i = 0; i < 3 && hWndDeleteDialog == IntPtr.Zero; i++)
			{
				PostMessage(hWnd, WM_COMMAND, (int)this.hWndMenuDelete, 0);
				Thread.Sleep(300);
				while ((hWndDeleteDialog = FindWindow("#32770", "Ariel")) == IntPtr.Zero)
				{
					if (attempts > 10)
						break;

					attempts++;
					Thread.Sleep(500);
				}
			}

			if (hWndDeleteDialog == IntPtr.Zero)
				throw new IllException(ErrorCode.DeleteCantLocateWnd);

			Progress_Changed(exportUnit, -1, "Delete dialog showed up...");

			Progress_Comment(exportUnit, "Pressing Yes button in Delete dialog...");
			hWndOkButton = FindWindowEx(hWndDeleteDialog, IntPtr.Zero, "Button", "&Yes");

			Thread.Sleep(500);
			result = SendMessage(hWndOkButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
			result += SendMessage(hWndOkButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
			if (result == 0)
			{
				Progress_Changed(exportUnit, -1, "Delete dialog showed up...");
			}
			else
			{
				throw new IllException(ErrorCode.DeleteCantConfirm);
			}
		}
		#endregion

		#region ExportToPatron()
		protected override void ExportToPatron(ExportUnit exportUnit)
		{
			throw new Exception("Sending to Patrons is not supported with Ariel 3.X version!");
		}
		#endregion
		
		#endregion
	}
}
