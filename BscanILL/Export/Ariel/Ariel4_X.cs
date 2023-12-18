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
	public class Ariel4_X : ArielBasics, IAriel
	{
		ManualResetEvent waitForEmailBoxToChange = new ManualResetEvent(false);
	
		#region constructor
		public Ariel4_X()
			: base()
		{
		}
		#endregion

		#region destructor
		~Ariel4_X()
		{
			Dispose();
		}
		#endregion

		#region class SendToPatronNativeWindow
		public class SendToPatronNativeWindow : NativeWindow
		{
			private IntPtr windowHandle;
			public IntPtr TextBoxHandle;

			public event EventHandler TextBoxChanged;

			public SendToPatronNativeWindow(IntPtr hWnd, IntPtr hWndTextBox)
			{
				this.windowHandle = hWnd;
				this.TextBoxHandle = hWndTextBox;

				this.AssignHandle(hWnd);
			}

			// Listen to when the handle changes to keep the variable in sync
			[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
			protected override void OnHandleChange()
			{
				this.windowHandle = this.Handle;
			}

			[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
			protected override void WndProc(ref Message m)
			{
				// Listen for messages that are sent to the button window. Some messages are sent
				// to the parent window instead of the button's window.

				switch (m.Msg)
				{
					case WM_SETTEXT:
						if (m.HWnd == this.TextBoxHandle && TextBoxChanged != null)
							TextBoxChanged(null, null);
						break;
				}
				base.WndProc(ref m);
			}

		}
		#endregion


		//PUBLIC METHODS
		#region public methods

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region exporting article

		#region Export()
		protected override void Export(ExportUnit exportUnit)
		{
			ProcessImportWindow(exportUnit);
			ProcessSendWindow(exportUnit);
		}
		#endregion

		#region GetMenuHandles()
		protected override void GetMenuHandles()
		{
			try
			{
				IntPtr hWndMenu = GetMenu(this.hWnd);
				IntPtr hWndSubMenuDoc = GetSubMenu(hWndMenu, 0);
				this.hWndMenuSend = GetMenuItemID(hWndSubMenuDoc, 2);
				this.hWndMenuArchive = GetMenuItemID(hWndSubMenuDoc, 21);
                IntPtr hWndSubMenuImport = GetSubMenu(hWndSubMenuDoc, 23);
                hWndMenuImport = GetMenuItemID(hWndSubMenuImport, 1);
				hWndMenuImportPatrons = GetMenuItemID(hWndSubMenuImport, 2);
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

			hWndDialog = FindWindow("#32770", "Send");
			if (hWndDialog != IntPtr.Zero)
			{
				if ((hWndControl = FindWindowEx(hWndDialog, IntPtr.Zero, "Button", "&Cancel")) != IntPtr.Zero)
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

		#region ProcessSendWindow()
		private void ProcessSendWindow(ExportUnit exportUnit)
		{
			Article article = exportUnit.Article; 
			
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
			Progress_Comment(exportUnit, "Opening Send window...");
			hWndSendDialog = OpenSendWindow();
			Progress_Changed(exportUnit, 30, "Send window opened successfully.");

			Thread.Sleep(300);
			Progress_Comment(exportUnit, "Localizing Send window controls...");
			//get controls handles
			if ((hWndDocumentId = FindWindowEx(hWndSendDialog, IntPtr.Zero, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateDocIdBox);
			if ((hWndPatron = FindWindowEx(hWndSendDialog, hWndDocumentId, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocatePatronBox);
			if ((hWndNote = FindWindowEx(hWndSendDialog, hWndPatron, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateNoteBox);
			if ((hWndTo = FindWindowEx(hWndSendDialog, hWndNote, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateToBox);
			if ((hWndOkButton = FindWindowEx(hWndSendDialog, IntPtr.Zero, "Button", "&OK")) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateOkButton);
			if ((hWndCancelButton = FindWindowEx(hWndSendDialog, IntPtr.Zero, "Button", "&Cancel")) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendCanNotLocateCancelButton);
			Progress_Changed(exportUnit, 35, "Send window controls localized.");

			Progress_Comment(exportUnit, "Entering data to Send window...");

			Thread.Sleep(200);
			result = SendMessage(hWndDocumentId, WM_SETTEXT, (int)0, article.IllNumber);
			if (result != 0)// && result2 == 0)
			{
				Progress_Changed(exportUnit, 45, "Document ID entered successfully.");
			}
			else
			{
				throw new IllException(ErrorCode.SendCanNotEnterData);
			}

			result = SendMessage(hWndPatron, WM_SETTEXT, (int)0, article.Patron);
			if (result != 0)// && result2 == 0)
			{
				Progress_Changed(exportUnit, 45, "Patron entered successfully.");
			}
			else
			{
				throw new IllException(ErrorCode.SendCanNotEnterData);
			}
			
			result += SendMessage(hWndTo, WM_SETTEXT, (int)0, article.Address);
			if (result != 0)
			{
				Progress_Changed(exportUnit, 55, "Send window data entered successfully.");
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

			Progress_Changed(exportUnit, 65, "OK button pressed.");

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
			IntPtr hWndSendDialog = IntPtr.Zero;
			int attempts = 0;
			Thread.Sleep(2000);

			while ((attempts < 30) && (hWndSendDialog = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"#32770", "Send")) == IntPtr.Zero)
			{
				attempts++;
				Thread.Sleep(2000);
			}

			if (hWndSendDialog != IntPtr.Zero)
				return hWndSendDialog;

			if (InvalidDocumentFormatDialogPoppedUp())
			{
				throw new IllException(ErrorCode.ImportBadFileFormat);
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

		#endregion

		#region exporting article to patron

		#region ExportToPatron()
		protected override void ExportToPatron(ExportUnit exportUnit)
		{
			ExportToPatronFake(exportUnit);
		}
		#endregion

		#region ProcessSendToPatronWindow()
		private void ProcessSendToPatronWindow(ExportUnit exportUnit)
		{
			long result = 0;
			IntPtr hWndSendDialog;
			IntPtr hWndListViewPatrons;
			IntPtr hWndTextBoxEmail;
			IntPtr hWndButtonSend;
			IntPtr hWndButtonCancel;
			int attempts = 0;
			
			//Open Send to Patron dialog
			Progress_Comment(exportUnit, "Opening Send to Patron window...");
			hWndSendDialog = OpenSendToPatronWindow();
			Progress_Changed(exportUnit, 30, "Send to Patron window opened successfully.");

			Thread.Sleep(300);
			Progress_Comment(exportUnit, "Localizing Send to Patron window controls...");
			//get controls handles
			if ((hWndListViewPatrons = FindWindowEx(hWndSendDialog, IntPtr.Zero, "ListBox", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendToPatronCantLocateListBoxPatrons);

			if ((hWndTextBoxEmail = FindWindowEx(hWndSendDialog, IntPtr.Zero, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendToPatronCantLocateBoxEmail);
			for(int i = 0; i < 8; i++)
				if ((hWndTextBoxEmail = FindWindowEx(hWndSendDialog, hWndTextBoxEmail, "Edit", null)) == IntPtr.Zero)
					throw new IllException(ErrorCode.SendToPatronCantLocateBoxEmail);

			if ((hWndButtonSend = FindWindowEx(hWndSendDialog, IntPtr.Zero, "Button", "&Send")) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendToPatronCantLocateButtonSend);
			if ((hWndButtonCancel = FindWindowEx(hWndSendDialog, IntPtr.Zero, "Button", "&Cancel")) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendToPatronCantLocateButtonCancel);
			Progress_Changed(exportUnit, 35, "Send to Patron window controls localized.");

			//localizing Send to Patron Window message queue;
			Progress_Comment(exportUnit, "Localizing Send to Patron message queue...");
			SendToPatronNativeWindow stpWnd = new SendToPatronNativeWindow(hWndSendDialog, hWndTextBoxEmail);
			stpWnd.TextBoxChanged += new EventHandler(SendToPatron_EmailBoxChanged);
			Progress_Changed(exportUnit, 40, "Send to Patron message queue localized.");
		
			//selecting appropriate record in list view
			Progress_Comment(exportUnit, "Finding patron in the patron list view...");
			Thread.Sleep(200);
			SelectPatronInListBox(exportUnit, hWndSendDialog, hWndListViewPatrons, hWndTextBoxEmail, stpWnd);
			Progress_Changed(exportUnit, 45, "Patron selected successfully.");

			Thread.Sleep(300);
			//press Send button
			Progress_Comment(exportUnit, "Sending...");
			result = PostMessage(hWndButtonSend, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
			result += PostMessage(hWndButtonSend, WM_LBUTTONUP, 0, 65536 * 14 + 19);

			Progress_Changed(exportUnit, 65, "Send button pressed.");

			Thread.Sleep(1000);
			attempts = 0;

			Progress_Comment(exportUnit, "Waiting for Send to Patrons window to be closed...");
			while (FindWindow("#32770", "Send to Partons") != IntPtr.Zero)
			{
				if (attempts > 30)
				{
					throw new IllException(ErrorCode.SendToPatronWndStandsOpen);
				}
				else
				{
					Thread.Sleep(1000);
					attempts++;
				}
			}
			Progress_Changed(exportUnit, 90, "Send to Patron window closed successfully.");
		}
		#endregion

		#region SendToPatron_EmailBoxChanged
		void SendToPatron_EmailBoxChanged(object sender, EventArgs e)
		{
			waitForEmailBoxToChange.Set();
		}
		#endregion

		#region OpenSendToPatronWindow()
		private IntPtr OpenSendToPatronWindow()
		{
			IntPtr hWndSendDialog = IntPtr.Zero;
			int attempts = 0;
			Thread.Sleep(2000);

			while ((attempts < 30) && (hWndSendDialog = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"#32770", "Send to Patron")) == IntPtr.Zero)
			{
				attempts++;
				Thread.Sleep(2000);
			}

			if (hWndSendDialog != IntPtr.Zero)
				return hWndSendDialog;

			if (InvalidDocumentFormatDialogPoppedUp())
				throw new IllException(ErrorCode.ImportBadFileFormat);

			throw new IllException(ErrorCode.SendToPatronCantFindWindow);
		}
		#endregion

		#region SelectPatronInListBox()
		private bool SelectPatronInListBox(ExportUnit exportUnit, IntPtr hWndDialog, IntPtr hWndListViewPatrons, IntPtr hWndEmail, SendToPatronNativeWindow stpWnf)
		{
			IntPtr localBuffer = IntPtr.Zero;
			IntPtr remoteBuffer = IntPtr.Zero;
			LVITEM lvItem = new LVITEM();
			int itemCount;
			uint MEM_COMMIT = 0x1000;
			uint PAGE_READWRITE = 4;
			uint MEM_RELEASE = 0x8000;
			int bytesWritten = 0;
			int lvItemSize = Marshal.SizeOf(lvItem);

			Progress_Comment(exportUnit, "Activating Send to Patron window...");
			lvItem.mask = LVIF_STATE;
			lvItem.stateMask = LVIS_SELECTED | LVIS_FOCUSED;
			lvItem.state = 0;

			if (SendMessage(hWndDialog, WM_CHILDACTIVATE, 0, 0) != 0)
				throw new IllException(ErrorCode.SendToPatronCantActivateWnd);
			Progress_Changed(exportUnit, -1, "Send to Patron window activated.");

			Progress_Comment(exportUnit, "Getting patrons count...");
			itemCount = SendMessage(hWndListViewPatrons, LVM_GETITEMCOUNT, 0, 0);
			if (itemCount < 1)
				throw new IllException(ErrorCode.SendToPatronNoPatrons);
			Progress_Changed(exportUnit, -1, "Patrons count is " + itemCount.ToString() + ".");

			try
			{
				Progress_Comment(exportUnit, "Selecting article...");
				remoteBuffer = VirtualAllocEx(arielProcess.Handle, IntPtr.Zero, lvItemSize, MEM_COMMIT, PAGE_READWRITE);

				if (remoteBuffer == IntPtr.Zero)
					throw new IllException(ErrorCode.ListItemRemoteBufferNotAlloc, "Remote buffer not allocated: " + GetLastError());

				localBuffer = Marshal.AllocHGlobal(lvItemSize);
				Marshal.StructureToPtr(lvItem, localBuffer, false);

				if (WriteProcessMemory(arielProcess.Handle, remoteBuffer, localBuffer, lvItemSize, ref bytesWritten))
				{
					unchecked
					{
						for (int i = 0; i < itemCount; i++)
						{
							if (SendMessage(hWndListViewPatrons, LVM_SETITEMSTATE, -1, remoteBuffer.ToInt32()) != 0)
							{
								lvItem.state = LVIS_SELECTED | LVIS_FOCUSED;
								Marshal.StructureToPtr(lvItem, localBuffer, false);

								if (WriteProcessMemory(arielProcess.Handle, remoteBuffer, localBuffer, lvItemSize, ref bytesWritten))
								{
									waitForEmailBoxToChange.Reset();

									if (SendMessage(hWndListViewPatrons, LVM_SETITEMSTATE, 0, remoteBuffer.ToInt32()) == 0)
										throw new IllException(ErrorCode.ListItemCantSelectItem, "Can't select item in Send to Patron: " + GetLastError());

									if (waitForEmailBoxToChange.WaitOne(10000, true) == false)
										throw new IllException(ErrorCode.SendToPatronEmailBoxNotChanging);

									string email = GetControlText(hWndEmail);

									if (email.ToLower() == exportUnit.Article.Address.ToLower())
										return true;
								}
								else
								{
									throw new IllException(ErrorCode.ListItemCantWriteToArielProcess, "Can't write to Ariel process memory 2: " + GetLastError());
								}
							}
							else
							{
								throw new IllException(ErrorCode.SendToPatronCantUnselectItems, "Can't unselect items in Archive: " + GetLastError());
							}
						}
					}
				}
				else
				{
					throw new IllException(ErrorCode.SendToPatronCantWriteToArielProcess, "Can't write to Ariel process memory" + GetLastError());
				}
				Progress_Changed(exportUnit, -1, "Article in Archive window selected.");
			}
			finally
			{
				if (localBuffer != IntPtr.Zero)
					Marshal.FreeHGlobal(localBuffer);

				if (remoteBuffer != IntPtr.Zero)
					VirtualFreeEx(arielProcess.Handle, remoteBuffer, lvItemSize, MEM_RELEASE);
			}

			return false;
		}
		#endregion		

		#endregion

		#region GetControlText()
		public static string GetControlText(IntPtr hWndControl)
		{
			StringBuilder text = new StringBuilder(100);
			SendMessage(hWndControl, WM_GETTEXT, 99, text);

			return text.ToString().Trim();
		}
		#endregion



		#region ExportToPatronFake()
		protected void ExportToPatronFake(ExportUnit exportUnit)
		{
			ProcessImportWindowFake(exportUnit);
			ProcessSendToPatronWindowFake(exportUnit);
		}
		#endregion

		#region ProcessSendToPatronWindowFake()
		private void ProcessSendToPatronWindowFake(ExportUnit exportUnit)
		{
			Article article = exportUnit.Article;
			
			long result = 0;
			IntPtr hWndSendDialog;
			IntPtr hWndListViewPatrons;
			IntPtr hWndTextBoxEmail;
			IntPtr hWndButtonSend;
			IntPtr hWndButtonCancel;
			int attempts = 0;

			//Open Send to Patron dialog
			Progress_Comment(exportUnit, "Opening Send to Patron window...");
			hWndSendDialog = OpenSendToPatronWindowFake();
			Progress_Changed(exportUnit, 30, "Send to Patron window opened successfully.");

			Thread.Sleep(300);
			Progress_Comment(exportUnit, "Localizing Send to Patron window controls...");
			//get controls handles
			if ((hWndListViewPatrons = FindWindowEx(hWndSendDialog, IntPtr.Zero, "WindowsForms10.LISTBOX.app.0.378734a", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendToPatronCantLocateListBoxPatrons);

			if ((hWndTextBoxEmail = FindWindowEx(hWndSendDialog, IntPtr.Zero, "WindowsForms10.STATIC.app.0.378734a", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendToPatronCantLocateBoxEmail);
			for (int i = 0; i < 7; i++)
				if ((hWndTextBoxEmail = FindWindowEx(hWndSendDialog, hWndTextBoxEmail, "WindowsForms10.STATIC.app.0.378734a", null)) == IntPtr.Zero)
					throw new IllException(ErrorCode.SendToPatronCantLocateBoxEmail);

			if ((hWndButtonSend = FindWindowEx(hWndSendDialog, IntPtr.Zero, null, "&Send")) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendToPatronCantLocateButtonSend);
			if ((hWndButtonCancel = FindWindowEx(hWndSendDialog, IntPtr.Zero, null, "&Cancel")) == IntPtr.Zero)
				throw new IllException(ErrorCode.SendToPatronCantLocateButtonCancel);
			Progress_Changed(exportUnit, 35, "Send to Patron window controls localized.");

			//localizing Send to Patron Window message queue;
			Progress_Comment(exportUnit, "Localizing Send to Patron message queue...");
			SendToPatronNativeWindow stpWnd = new SendToPatronNativeWindow(hWndSendDialog, hWndTextBoxEmail);
			stpWnd.TextBoxChanged += new EventHandler(SendToPatron_EmailBoxChanged);
			Progress_Changed(exportUnit, 40, "Send to Patron message queue localized.");

			waitForEmailBoxToChange.Reset();
			waitForEmailBoxToChange.WaitOne();
			
			//selecting appropriate record in list view
			Progress_Comment(exportUnit, "Finding patron in the patron list view...");
			Thread.Sleep(200);
			SelectPatronInListBoxFake(exportUnit, hWndSendDialog, hWndListViewPatrons, hWndTextBoxEmail, stpWnd);
			Progress_Changed(exportUnit, 45, "Patron selected successfully.");

			Thread.Sleep(300);
			//press Send button
			Progress_Comment(exportUnit, "Sending...");
			result = PostMessage(hWndButtonSend, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
			result += PostMessage(hWndButtonSend, WM_LBUTTONUP, 0, 65536 * 14 + 19);

			Progress_Changed(exportUnit, 65, "Send button pressed.");

			Thread.Sleep(1000);
			attempts = 0;

			Progress_Comment(exportUnit, "Waiting for Send to Patrons window to be closed...");
			while (FindWindow("#32770", "Send to Partons") != IntPtr.Zero)
			{
				if (attempts > 30)
				{
					throw new IllException(ErrorCode.SendToPatronWndStandsOpen);
				}
				else
				{
					Thread.Sleep(1000);
					attempts++;
				}
			}
			Progress_Changed(exportUnit, 90, "Send to Patron window closed successfully.");
		}
		#endregion

		#region OpenSendToPatronWindowFake()
		private IntPtr OpenSendToPatronWindowFake()
		{
			IntPtr hWndSendDialog = IntPtr.Zero;
			int attempts = 0;
			Thread.Sleep(2000);

			while ((attempts < 30) && (hWndSendDialog = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Send to Patron")) == IntPtr.Zero)
			{
				attempts++;
				Thread.Sleep(2000);
			}

			if (hWndSendDialog != IntPtr.Zero)
				return hWndSendDialog;

			if (InvalidDocumentFormatDialogPoppedUp())
				throw new IllException(ErrorCode.ImportBadFileFormat);

			throw new IllException(ErrorCode.SendToPatronCantFindWindow);
		}
		#endregion

		#region SelectPatronInListBoxFake()
		private bool SelectPatronInListBoxFake(ExportUnit exportUnit, IntPtr hWndDialog, IntPtr hWndListViewPatrons, IntPtr hWndEmail, SendToPatronNativeWindow stpWnf)
		{
			IntPtr localBuffer = IntPtr.Zero;
			IntPtr remoteBuffer = IntPtr.Zero;
			LVITEM lvItem = new LVITEM();
			int itemCount;
			uint MEM_COMMIT = 0x1000;
			uint PAGE_READWRITE = 4;
			uint MEM_RELEASE = 0x8000;
			int bytesWritten = 0;
			int lvItemSize = Marshal.SizeOf(lvItem);

			Progress_Comment(exportUnit, "Activating Send to Patron window...");
			lvItem.mask = LVIF_STATE;
			lvItem.stateMask = LVIS_SELECTED | LVIS_FOCUSED;
			lvItem.state = 0;

			if (SendMessage(hWndDialog, WM_CHILDACTIVATE, 0, 0) != 0)
				throw new IllException(ErrorCode.SendToPatronCantActivateWnd);
			Progress_Changed(exportUnit, -1, "Send to Patron window activated.");

			Progress_Comment(exportUnit, "Getting patrons count...");
			itemCount = SendMessage(hWndListViewPatrons, LVM_GETITEMCOUNT, 0, 0);
			if (itemCount < 1)
				throw new IllException(ErrorCode.SendToPatronNoPatrons);
			Progress_Changed(exportUnit, -1, "Patrons count is " + itemCount.ToString() + ".");

			try
			{
				Progress_Comment(exportUnit, "Selecting article...");
				remoteBuffer = VirtualAllocEx(arielProcess.Handle, IntPtr.Zero, lvItemSize, MEM_COMMIT, PAGE_READWRITE);

				if (remoteBuffer == IntPtr.Zero)
					throw new IllException(ErrorCode.ListItemRemoteBufferNotAlloc, "Remote buffer not allocated: " + GetLastError());

				localBuffer = Marshal.AllocHGlobal(lvItemSize);
				Marshal.StructureToPtr(lvItem, localBuffer, false);

				if (WriteProcessMemory(arielProcess.Handle, remoteBuffer, localBuffer, lvItemSize, ref bytesWritten))
				{
					unchecked
					{
						for (int i = 0; i < itemCount; i++)
						{
							if (SendMessage(hWndListViewPatrons, LVM_SETITEMSTATE, -1, remoteBuffer.ToInt32()) != 0)
							{
								lvItem.state = LVIS_SELECTED | LVIS_FOCUSED;
								Marshal.StructureToPtr(lvItem, localBuffer, false);

								if (WriteProcessMemory(arielProcess.Handle, remoteBuffer, localBuffer, lvItemSize, ref bytesWritten))
								{
									waitForEmailBoxToChange.Reset();

									if (SendMessage(hWndListViewPatrons, LVM_SETITEMSTATE, 0, remoteBuffer.ToInt32()) == 0)
										throw new IllException(ErrorCode.ListItemCantSelectItem, "Can't select item in Send to Patron: " + GetLastError());

									if (waitForEmailBoxToChange.WaitOne(10000, true) == false)
										throw new IllException(ErrorCode.SendToPatronEmailBoxNotChanging);

									string email = GetControlText(hWndEmail);

									if (email.ToLower() == exportUnit.Article.Address.ToLower())
										return true;
								}
								else
								{
									throw new IllException(ErrorCode.ListItemCantWriteToArielProcess, "Can't write to Ariel process memory 2: " + GetLastError());
								}
							}
							else
							{
								throw new IllException(ErrorCode.SendToPatronCantUnselectItems, "Can't unselect items in Archive: " + GetLastError());
							}
						}
					}
				}
				else
				{
					throw new IllException(ErrorCode.SendToPatronCantWriteToArielProcess, "Can't write to Ariel process memory" + GetLastError());
				}
				Progress_Changed(exportUnit, -1, "Article in Archive window selected.");
			}
			finally
			{
				if (localBuffer != IntPtr.Zero)
					Marshal.FreeHGlobal(localBuffer);

				if (remoteBuffer != IntPtr.Zero)
					VirtualFreeEx(arielProcess.Handle, remoteBuffer, lvItemSize, MEM_RELEASE);
			}

			return false;
		}
		#endregion	


		#endregion
	}
}
