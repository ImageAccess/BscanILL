using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using BscanILL.Misc;
using BscanILL.Hierarchy;
using BscanILL.Export.AdditionalInfo;

namespace BscanILL.Export.Ariel
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ArielBasics : ExportBasics
	{
		#region variables

		protected FileInfo	appPath;
		protected Process	arielProcess = null;
		protected bool		arielAlreadyAllocated = false;
		protected IntPtr	hWnd = IntPtr.Zero;

		protected IntPtr hWndArchive = IntPtr.Zero;
		protected IntPtr hWndArchiveListView = IntPtr.Zero;
		protected IntPtr hWndSend = IntPtr.Zero;
		protected IntPtr hWndReceive = IntPtr.Zero;
		protected IntPtr hWndDelivery = IntPtr.Zero;
		protected uint hWndMenuSend = 0;
		protected uint hWndMenuDelete = 0;
		protected uint hWndMenuArchive = 0;
		protected uint hWndMenuImport = 0;
		protected uint hWndMenuImportPatrons = 0;

		protected const int WM_GETTEXT = 0x000D;
		protected const int WM_SETTEXT = 0x000C;
		protected const int WM_CHILDACTIVATE = 0x0022;
		protected const int WM_KEYDOWN = 0x0100;
		protected const int WM_KEYUP = 0x0101;
		protected const int WM_CHAR = 0x0102;
		protected const int WM_COMMAND = 0x0111;
		protected const int WM_LBUTTONDOWN = 0x0201;
		protected const int WM_LBUTTONUP = 0x0202;
		protected const int WM_SETFOCUS = 0x0007;

		protected const int SW_SHOWNORMAL = 0x01;

		protected const int LVM_GETITEMCOUNT = 0x1000 + 4;
		protected const int LVM_DELETEALLITEMS = 0x1000 + 9;
		protected const int LVM_SETITEMSTATE = 0x1000 + 43;
		protected const int LVM_GETITEMSTATE = 0x1000 + 44;

		protected const int LVIS_FOCUSED = 0x0001;
		protected const int LVIS_SELECTED = 0x0002;
		protected const int LVIS_ACTIVATING = 0x0020;
		protected const int LVIS_STATEIMAGEMASK = 0xF000;

		protected const int LVIF_COLUMNS = 0x0200;
		protected const int LVIF_GROUPID = 0x0100;
		protected const int LVIF_IMAGE = 0x0002;
		protected const int LVIF_TEXT = 0x0001;
		protected const int LVIF_PARAM = 0x0004;
		protected const int LVIF_STATE = 0x0008;
		protected const int LVIF_INDENT = 0x0010;
		protected const int LVIF_NORECOMPUTE = 0x0800;

		protected delegate Int32 EnumChildProc(IntPtr hWnd, IntPtr lParam);
		protected EnumChildProc findArchiveProc;
		#endregion

		
		//DLL IMPORTS
		#region DllImport
		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern int GetLastError();

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr address,
			int size, uint freeType, uint protection);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr address,
			int size, uint freeType);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr address,
			IntPtr lvItemPtr, int size, ref int numOfBytesWritten);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr address,
			ref LVITEM lvItem, int size, ref int numOfBytesRead);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

		[DllImport("user32.DLL", SetLastError = true)]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wparam, StringBuilder sb);

		[DllImport("user32.DLL", SetLastError = true)]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wparam, string text);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr FindWindow(string controlClass, string controlTitle);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChild,
			string controlClass, string controlTitle);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SetWindowText(IntPtr hWnd, String text);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool EnumChildWindows(IntPtr hWnd, Delegate lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder text, int maxCount);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr ChildWindowFromPointEx(IntPtr hWnd, POINT point, uint flags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern short VkKeyScan(char ch);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetMenu(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetSubMenu(IntPtr hMenu, int position);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern uint GetMenuItemID(IntPtr hMenu, int position);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetMenuItemCount(IntPtr hMenu);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool ShowWindow(IntPtr hWnd, int flag);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool DestroyWindow(IntPtr hWnd);

		/*ListView_SetItemState(
    HWND hwnd,
    int i,
    UINT state,
    UINT mask
);*/

		
		#endregion

		#region constructor
		public ArielBasics()
		{
			try { this.appPath = new FileInfo(_settings.Export.Ariel.Executable); }
			catch { throw new IllException(ErrorCode.ArielPathIncorrect); }
			
			if (appPath.Exists == false)
                throw new IllException(ErrorCode.ArielPathIncorrect, "Ariel path is incorrect!, The path is '" + appPath + "'.");

			this.findArchiveProc = new EnumChildProc(FindWindows);

			/*if (_settings.Export.Ariel.UpdateILLiad)
			{
				illiad = BscanILL.Export.ILLiad.ILLiadBasics.GetIlliadInstance();

				illiad.ProgressChanged += new ProgressChangedHandle(this.Progress_Changed);
				illiad.ProgressComment += new ProgressCommentHandle(this.Progress_Comment);
			}*/
		}
		#endregion

		#region destructor
		public void Dispose()
		{
		}
		#endregion


		//STRUCTURES
		#region structs

		#region struct POINT
		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}

			public void Set(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}
		}
		#endregion

		#region struct RECT
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
		#endregion

		#region struct LVITEM
		[StructLayout(LayoutKind.Sequential)]
		public struct LVITEM
		{
			public uint mask;
			public int item;
			public int subItem;
			public uint state;
			public uint stateMask;
			public IntPtr text;
			public int textMax;
			public int image;
			public IntPtr lParam;
			public int indent;
		}
		#endregion

		#region MENUINFO
		[StructLayout(LayoutKind.Sequential)]
		public struct MENUINFO
		{
			int cbSize;
			int fMask;
			int dwStyle;
			uint cyMax;
			IntPtr hbrBack;
			int dwContextHelpID;
			IntPtr dwMenuData;
		}
		#endregion

		#endregion


		// PRIVATE PROPERTIES
		#region private properties
		#endregion


		//PUBLIC METHODS
		#region public methods

        #region ExportArticle()
		public void ExportArticle(ExportUnit exportUnit)
		{
			Article			article = exportUnit.Article;
			AdditionalAriel additionalAriel = (AdditionalAriel)exportUnit.AdditionalInfo;
			
			if ((article.IllNumber == null || article.IllNumber.Trim().Length == 0) && (article.TransactionId != null))
				article.IllNumber = article.TransactionId.ToString();

            Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via Ariel---");
            Progress_Changed(exportUnit, 5, "Checking...");

			StartAriel(exportUnit);

			if (article.IllNumber.StartsWith("-") == false && this.CheckingArticleInDb)
				this.Illiad.CheckArticleInDb(exportUnit);

			Export(exportUnit);

			//if (additionalAriel.UpdateILLiad && (additionalAriel.UpdateArticlesWithNegativeId || article.IllNumber.StartsWith("-") == false) && additionalAriel.AvoidIp.Contains(article.Address) == false)
            if (additionalAriel.UpdateILLiad && additionalAriel.AvoidIp.Contains(article.Address) == false)
			{
                Progress_Changed(exportUnit, 95, "Updating ILLiad...");
				this.Illiad.UpdateInfo(exportUnit, additionalAriel.ChangeStatusToRequestFinished);
                Progress_Changed(exportUnit, 100, "ILLiad updated successfully.");
			}
		}
        #endregion

		#region ExportArticleToPatron()
		public void ExportArticleToPatron(ExportUnit exportUnit)
		{
			Article article = exportUnit.Article;
			AdditionalAriel additional = (AdditionalAriel)exportUnit.AdditionalInfo;
			
			if ((article.IllNumber == null || article.IllNumber.Trim().Length == 0) && (article.TransactionId != null))
				article.IllNumber = article.TransactionId.ToString();

            Progress_Comment(exportUnit, "---Exporting Article (ILL#:" + exportUnit.Article.IllNumber + ", TN:" + exportUnit.Article.TransactionId + ") via Ariel to Patron---");
            Progress_Changed(exportUnit, 5, "Checking...");

			StartAriel(exportUnit);
			ExportToPatron(exportUnit);

			//if (additional.UpdateILLiad && (additional.UpdateArticlesWithNegativeId || article.IllNumber.StartsWith("-") == false) && additional.AvoidIp.Contains(article.Address) == false)
            if (additional.UpdateILLiad && additional.AvoidIp.Contains(article.Address) == false)
			{
                Progress_Changed(exportUnit, 95, "Updating ILLiad...");
				this.Illiad.UpdateInfo(exportUnit, additional.ChangeStatusToRequestFinished);
                Progress_Changed(exportUnit, 100, "ILLiad updated successfully.");
			}
		}
		#endregion
		
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region ArielProcess_Exited()
		protected void ArielProcess_Exited(object sender, EventArgs e)
		{
			try
			{
				arielAlreadyAllocated = false;
				//StartAriel();
			}
			finally
			{
			}
		}
		#endregion

		#region IsArielRunning()
		protected bool IsArielRunning()
		{
			return (FindArielProcess() != null);
		}
		#endregion

		#region FindArielProcess()
		protected Process FindArielProcess()
		{
			Process[] processes = Process.GetProcessesByName("WinAriel");

			if (processes.Length > 0)
				return processes[0];

			return null;
		}
		#endregion

		#region LaunchAriel()
		public Process LaunchAriel()
		{
			try
			{
				Process		process = new Process();
				process.StartInfo.FileName = this.appPath.FullName;
				process.StartInfo.WorkingDirectory = this.appPath.Directory.FullName;
				process.Start();

				Thread.Sleep(6000);

				if (process.WaitForInputIdle(20000))
				{
					process.Exited += new EventHandler(ArielProcess_Exited);
					return process;
				}
				else
					throw new IllException(ErrorCode.ArielLaunchTimeout);
			}
			catch(Exception ex)
			{
				if (ex.Message != null && ex.Message.Length > 0)
					throw new IllException(ErrorCode.ArielCantLaunch, "Can't launch Ariel! " + ex.Message);
				else
					throw new IllException(ErrorCode.ArielCantLaunch);
			}
		}
		#endregion

		#region GetArielWindowHandle()
		public IntPtr GetArielWindowHandle(Process process)
		{
			IntPtr windowHandle = process.MainWindowHandle;

			if (windowHandle == IntPtr.Zero)
				throw new Exception("Ariel is running, but Ariel window can't be localized!");
			return windowHandle;
		}
		#endregion

        #region StartAriel()
		private bool StartAriel(ExportUnit exportUnit)
		{
			if (IsArielRunning() == false || !arielAlreadyAllocated)
			{
				Progress_Comment(exportUnit, "Locating Ariel process...");
				this.arielProcess = FindArielProcess();
				Progress_Changed(exportUnit, 2, "");

				if (this.arielProcess == null)
				{
					Progress_Comment(exportUnit, "Launching Ariel...");
					this.arielProcess = LaunchAriel();
					Progress_Changed(exportUnit, 3, "Ariel launched successfully.");
				}
				else
				{
					Progress_Changed(exportUnit, 3, "Ariel process located.");
				}
				
				Progress_Comment(exportUnit, "Obtaining Ariel window handle...");
				this.hWnd = GetArielWindowHandle(this.arielProcess);
				Progress_Changed(exportUnit, 4, "Ariel window located.");

				Progress_Comment(exportUnit, "Obtaining Ariel window menu handles...");
				GetMenuHandles();
				Progress_Changed(exportUnit, 5, "Ariel menu handles located.");

				Progress_Comment(exportUnit, "Getting Ariel Archive window handle...");
				GetArchiveWindowHandle();
				Progress_Changed(exportUnit, 6, "Ariel Archive window located.");

				Progress_Comment(exportUnit, "Checking user interface...");
				CloseUnexpectedWindows();
				Progress_Changed(exportUnit, 7, "Pop ups checked.");

				Progress_Comment(exportUnit, "Preparing windows...");
				SetWindowsNormal();
				Progress_Changed(exportUnit, 8, "Windows checked.");

				int archiveItemsCount;
				if (this is Ariel3_X)
				{
					do
					{
						Progress_Comment(exportUnit, "Checking Archive list...");
						archiveItemsCount = GetArchiveDocumentsListCount();
						Progress_Changed(exportUnit, -1, "Archive documents list received.");

						if (archiveItemsCount > 0)
						{
							if (MessageBox.Show("There are some articles in Archive window. Delete or finish all documents " +
								"in Archive window first, then, press OK button.\n\nTo cancel sending articles to Ariel, press Cancel.",
								"", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
								throw new IllException(ErrorCode.StartArchiveNotEmpty);
						}
					} while (archiveItemsCount > 0);
				}

				Progress_Changed(exportUnit, 10, "Initialization was succesfull.");
				arielAlreadyAllocated = true;
				return true;
			}
			else
			{
				Progress_Changed(exportUnit, 10, "");
				return true;
			}
		}
        #endregion

		#region SetWindowsNormal()
		private void SetWindowsNormal()
		{
			ShowWindow(this.hWndArchive, SW_SHOWNORMAL);

			if (this.hWndSend != IntPtr.Zero)
				ShowWindow(this.hWndSend, SW_SHOWNORMAL);

			if (this.hWndReceive != IntPtr.Zero)
				ShowWindow(this.hWndReceive, SW_SHOWNORMAL);

			if (this.hWndDelivery != IntPtr.Zero)
				ShowWindow(this.hWndDelivery, SW_SHOWNORMAL);
		}
		#endregion


		#region Export()
		protected virtual void Export(ExportUnit exportUnit)
        {
			throw new Exception("Function Export() must be inherited!");
		}
        #endregion

		#region GetMenuHandles()
        protected virtual void GetMenuHandles()
        {
            throw new Exception("This function must be inherited!");
        }
        #endregion

		#region GetArchiveWindowHandle()
		private void GetArchiveWindowHandle()
		{
			this.hWndArchive = IntPtr.Zero;

			EnumChildWindows(hWnd, findArchiveProc, hWnd);

			if (hWndArchive == IntPtr.Zero)
				OpenArchiveWindow();

			if (this.hWndArchive == IntPtr.Zero)
				throw new IllException(ErrorCode.StartCantGetArchiveWindowHandle);
		}
		#endregion

		#region OpenArchiveWindow()
		public void OpenArchiveWindow()
		{
			SendMessage(hWnd, WM_COMMAND, (int)this.hWndMenuArchive, 0);
			Thread.Sleep(1000);
			EnumChildWindows(hWnd, findArchiveProc, hWnd);
		}
		#endregion

		#region FindWindows()
		private Int32 FindWindows(IntPtr hWindow, IntPtr lParam)
		{
			StringBuilder controlText = new StringBuilder(99);
			int length;

			length = GetWindowText(hWindow, controlText, 100);
			if (length == 0)
				return 1;

			if (controlText.ToString().StartsWith("Archive"))
			{
				this.hWndArchive = hWindow;
				this.hWndArchiveListView = FindWindowEx(this.hWndArchive, IntPtr.Zero, "SysListView32", null);
				return 1;
			}
			if (controlText.ToString().StartsWith("Send Queue"))
			{
				this.hWndSend = hWindow;
				return 1;
			}
			if (controlText.ToString().StartsWith("Received Queue"))
			{
				this.hWndReceive = hWindow;
				return 1;
			}

			if (controlText.ToString().StartsWith("Delivery Queue"))
			{
				this.hWndDelivery = hWindow;
				return 1;
			}

			return 1;
		}
		#endregion	

		#region CloseUnexpectedWindows()
		protected virtual void CloseUnexpectedWindows()
		{
			throw new Exception("This function must be inherited!");
		}
		#endregion

		#region GetArchiveDocumentsListCount()
		protected int GetArchiveDocumentsListCount()
		{
			int itemCount = SendMessage(this.hWndArchiveListView, LVM_GETITEMCOUNT, 0, 0);
			return itemCount;
		}
		#endregion

		#region ProcessImportWindow()
		protected void ProcessImportWindow(ExportUnit exportUnit)
		{
			Article article = exportUnit.Article;
			
			long result;
			IntPtr hWndImport = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"#32770", "Import");
			IntPtr hWndEditBox;
			IntPtr hWndOkButton;
			IntPtr hWndCancelButton;
			int attempts = 0;

			Progress_Comment(exportUnit, "Opening Import window...");
			//open Import window
			if (hWndImport == IntPtr.Zero)
			{
				if (article.ExportType == ExportType.ArielPatron)
					PostMessage(hWnd, WM_COMMAND, (int)this.hWndMenuImportPatrons, 0);
				else
					PostMessage(hWnd, WM_COMMAND, (int)this.hWndMenuImport, 0);

				Thread.Sleep(500);

				while ((hWndImport = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"#32770", "Import")) == IntPtr.Zero)
				{
					if (attempts > 10)
						throw new IllException(ErrorCode.ImportCanNotOpenWnd);

					Thread.Sleep(1000);
					attempts++;
				}
			}

			Progress_Changed(exportUnit, 5, "Import window opened.");

			Thread.Sleep(200);
			Progress_Comment(exportUnit, "Localizing Import window controls...");
			//get controls handles	
			if ((hWndEditBox = FindWindowEx(hWndImport, IntPtr.Zero, "Edit", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.ImportCanNotLocateEditBox);
			if ((hWndOkButton = FindWindowEx(hWndImport, IntPtr.Zero, "Button", "&Open")) == IntPtr.Zero)
				throw new IllException(ErrorCode.ImportCanNotLocateOkButton);
			if ((hWndCancelButton = FindWindowEx(hWndImport, IntPtr.Zero, "Button", "Cancel")) == IntPtr.Zero)
				throw new IllException(ErrorCode.ImportCanNotLocateCancelButton);

			Progress_Changed(exportUnit, 10, "Import window controls localized.");

			//fill in tiff file path
			Thread.Sleep(1000);
			Progress_Comment(exportUnit, "Entering image path...");
			result = SendMessage(hWndEditBox, WM_SETTEXT, (int)0, exportUnit.Files[0].FullName);
			Thread.Sleep(100);
			result = SendMessage(hWndEditBox, WM_SETTEXT, (int)0, exportUnit.Files[0].FullName);
			if (result == 0)
				throw new IllException(ErrorCode.ImportCanNotEnterImagePath);

			string textBoxText;
			attempts = 0;
			while ((textBoxText = BscanILL.Export.ILLiad.ILLiad7_1_8_0.GetTextFieldText(hWndEditBox)) != exportUnit.Files[0].FullName)
			{
				Progress_Changed(exportUnit, -1, "Reentering image path.");
				result = SendMessage(hWndEditBox, WM_SETTEXT, (int)0, exportUnit.Files[0].FullName);

				if(attempts > 20)
					throw new IllException(ErrorCode.ImportCanNotEnterImagePath, "Can't enter image path to Ariel insert window!");

				Thread.Sleep(500);
			}

			Progress_Changed(exportUnit, 15, "Image Path entered successfully.");

			//press Open button
			Thread.Sleep(1000);
			Progress_Comment(exportUnit, "Pressing OK button...");
			result = PostMessage(hWndOkButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
			result += PostMessage(hWndOkButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
			//if (result != 0)
			//	throw new IllException(ErrorCode.ImportCanNotPressOKButton);
			Progress_Changed(exportUnit, 20, "OK button pressed.");

			Thread.Sleep(1000);
			attempts = 0;

			Progress_Comment(exportUnit, "Waiting for Import window to be closed...");
			while (FindWindow("#32770", "Import") != IntPtr.Zero)
			{
				if (attempts > 45)
				{
					Progress_Comment(exportUnit, "Checking article image format...");
					if (InvalidDocumentFormatDialogPoppedUp())
					{
						SendMessage(hWndCancelButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
						SendMessage(hWndCancelButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);

						exportUnit.Files[0].Refresh();
						if (exportUnit.Files[0].Exists)
							throw new IllException(ErrorCode.ImportBadFileFormat);
						else
							throw new IllException(ErrorCode.ImportFileNotFound);
					}
					else
					{
						SendMessage(hWndCancelButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
						SendMessage(hWndCancelButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
						throw new IllException(ErrorCode.ImportWndStandsOpen);
					}
				}
				else
				{
					Thread.Sleep(1000);
					attempts++;
				}
			}
			Progress_Changed(exportUnit, 25, "Import window closed successfully.");
		}
		#endregion

		#region InvalidDocumentFormatDialogPoppedUp()
		protected virtual bool InvalidDocumentFormatDialogPoppedUp()
		{
			throw new Exception("This function must be inherited!");
		}
		#endregion

		
		#region ExportToPatron()
		protected virtual void ExportToPatron(ExportUnit exportUnit)
		{
			throw new Exception("Function ExportToPatron() must be inherited!");
		}
		#endregion

		#region ProcessImportWindow()
		protected void ProcessImportWindowFake(ExportUnit exportUnit)
		{
			Article article = exportUnit.Article;

			long result;
			IntPtr hWndImport = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Import");
			IntPtr hWndEditBox;
			IntPtr hWndOkButton;
			IntPtr hWndCancelButton;
			int attempts = 0;

			Progress_Comment(exportUnit, "Opening Import window...");
			//open Import window
			if (hWndImport == IntPtr.Zero)
			{
				if (article.ExportType == ExportType.ArielPatron)
					PostMessage(hWnd, WM_COMMAND, (int)this.hWndMenuImportPatrons, 0);
				else
					PostMessage(hWnd, WM_COMMAND, (int)this.hWndMenuImport, 0);

				Thread.Sleep(500);

				while ((hWndImport = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "Import")) == IntPtr.Zero)
				{
					if (attempts > 10)
						throw new IllException(ErrorCode.ImportCanNotOpenWnd);

					Thread.Sleep(1000);
					attempts++;
				}
			}
			Progress_Changed(exportUnit, 5, "Import window opened.");

			Thread.Sleep(200);
			Progress_Comment(exportUnit, "Localizing Import window controls...");
			//get controls handles	
			if ((hWndEditBox = FindWindowEx(hWndImport, IntPtr.Zero, "ComboBoxEx32", null)) == IntPtr.Zero)
				throw new IllException(ErrorCode.ImportCanNotLocateEditBox);
			if ((hWndOkButton = FindWindowEx(hWndImport, IntPtr.Zero, "Button", "&Open")) == IntPtr.Zero)
				throw new IllException(ErrorCode.ImportCanNotLocateOkButton);
			if ((hWndCancelButton = FindWindowEx(hWndImport, IntPtr.Zero, "Button", "Cancel")) == IntPtr.Zero)
				throw new IllException(ErrorCode.ImportCanNotLocateCancelButton);
			Progress_Changed(exportUnit, 10, "Import window controls localized.");

			//fill in tiff file path
			Thread.Sleep(1000);
			Progress_Comment(exportUnit, "Entering image path...");
			result = SendMessage(hWndEditBox, WM_SETTEXT, (int)0, exportUnit.Files[0].FullName);
			Thread.Sleep(100);
			result = SendMessage(hWndEditBox, WM_SETTEXT, (int)0, exportUnit.Files[0].FullName);
			if (result == 0)
				throw new IllException(ErrorCode.ImportCanNotEnterImagePath);

			string textBoxText;
			attempts = 0;
			while ((textBoxText = BscanILL.Export.ILLiad.ILLiad7_1_8_0.GetTextFieldText(hWndEditBox)) != exportUnit.Files[0].FullName)
			{
				Progress_Changed(exportUnit, -1, "Reentering image path.");
				result = SendMessage(hWndEditBox, WM_SETTEXT, (int)0, exportUnit.Files[0].FullName);

				if (attempts > 20)
					throw new IllException(ErrorCode.ImportCanNotEnterImagePath, "Can't enter image path to Ariel insert window!");

				Thread.Sleep(500);
			}

			Progress_Changed(exportUnit, 15, "Image Path entered successfully.");

			//press Open button
			Thread.Sleep(1000);
			Progress_Comment(exportUnit, "Pressing OK button...");
			result = PostMessage(hWndOkButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
			result += PostMessage(hWndOkButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
			//if (result != 0)
			//	throw new IllException(ErrorCode.ImportCanNotPressOKButton);
			Progress_Changed(exportUnit, 20, "OK button pressed.");

			Thread.Sleep(1000);
			attempts = 0;

			Progress_Comment(exportUnit, "Waiting for Import window to be closed...");
			while (FindWindow("#32770", "Import") != IntPtr.Zero)
			{
				if (attempts > 45)
				{
					Progress_Comment(exportUnit, "Checking article image format...");
					if (InvalidDocumentFormatDialogPoppedUp())
					{
						SendMessage(hWndCancelButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
						SendMessage(hWndCancelButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);

						exportUnit.Files[0].Refresh();
						if (exportUnit.Files[0].Exists)
							throw new IllException(ErrorCode.ImportBadFileFormat);
						else
							throw new IllException(ErrorCode.ImportFileNotFound);
					}
					else
					{
						SendMessage(hWndCancelButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
						SendMessage(hWndCancelButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
						throw new IllException(ErrorCode.ImportWndStandsOpen);
					}
				}
				else
				{
					Thread.Sleep(1000);
					attempts++;
				}
			}
			Progress_Changed(exportUnit, 25, "Import window closed successfully.");
		}
		#endregion

		#endregion
	}
}
