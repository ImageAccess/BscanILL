using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.ObjectModel;
using BscanILL.Misc;
using BscanILL.Hierarchy;

namespace BscanILL.Export.ILLiad
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ILLiadBasics : ExportBasics
	{
		#region variables
		protected bool				sqlEnabled = false;
		
		//protected bool		illiadAlreadyAllocated = false;
		//protected IntPtr	hWnd = IntPtr.Zero;

		//protected static ILLiadVersion illiadVersion = ILLiadVersion.Unknown;
		static IntPtr	hWndStatic = IntPtr.Zero;

		protected const int WM_SETTEXT = 0x000C;
		protected const int WM_GETTEXT = 0x000D;
		protected const int WM_CHILDACTIVATE = 0x0022;
		protected const int WM_KEYDOWN = 0x0100;
		protected const int WM_KEYUP = 0x0101;
		protected const int WM_CHAR = 0x0102;
		protected const int WM_COMMAND = 0x0111;
		protected const int WM_LBUTTONDOWN = 0x0201;
		protected const int WM_LBUTTONUP = 0x0202;
		protected const int WM_SETFOCUS = 0x0007;
		protected const int WM_CLOSE = 0x0010;
		protected const int WM_PARENTNOTIFY = 0x0210;
		protected const int WM_MOUSEACTIVATE = 0x0021;
		protected const int WM_NCACTIVATE = 0x0086;

		protected const int SW_SHOWNORMAL = 0x01;
		protected const int SW_MINIMIZE   = 0x06;
		protected const int SW_SHOWMINIMIZED = 0x02;

		protected const int LVM_GETITEMCOUNT = 0x1000 + 4;
		protected const int LVM_DELETEALLITEMS = 0x1000 + 9;
		protected const int LVM_SETITEMSTATE = 0x1000 + 43;
		protected const int LVM_GETITEMSTATE = 0x1000 + 44;

		protected const int LVIS_FOCUSED = 0x0001;
		protected const int LVIS_SELECTED = 0x0002;
		protected const int LVIS_ACTIVATING = 0x0020;

		protected const int LVIF_COLUMNS = 0x0200;
		protected const int LVIF_GROUPID = 0x0100;
		protected const int LVIF_IMAGE = 0x0002;
		protected const int LVIF_TEXT = 0x0001;
		protected const int LVIF_PARAM = 0x0004;
		protected const int LVIF_STATE = 0x0008;
		protected const int LVIF_INDENT = 0x0010;
		protected const int LVIF_NORECOMPUTE = 0x0800;

		protected const int LVIS_STATEIMAGEMASK = 0xF000;

		protected const int BM_GETCHECK     = 0x00F0;
		protected const int BM_SETCHECK		= 0x00F1;
		protected const int BST_UNCHECKED	= 0x0000;
		protected const int BM_CLICK		= 0x00F5;

		//To use with keybd_event()
		protected const int KEYEVENTF_KEYDOWN = 0x0;
		protected const int  KEYEVENTF_EXTENDEDKEY = 0x1;
		protected const int  KEYEVENTF_KEYUP = 0x2;

		protected const int  HTCLIENT = 1;

		protected delegate Int32 EnumChildProc(IntPtr hWnd, IntPtr lParam);
		static protected EnumChildProc findMainWindowStatic = new EnumChildProc(FindMainWindowStatic);
		//protected EnumChildProc findMainWindow;
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
		public static extern bool EnumWindows(Delegate lpEnumFunc, IntPtr lParam);

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
		public static extern bool ShowWindowAsync(IntPtr hWnd, int flag);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool DestroyWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool CloseWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SetFocus(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool IsHungAppWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool GetWindowPlacement(IntPtr hWnd, IntPtr pPlacement);

		#endregion

		#region constructor
		protected ILLiadBasics()
		{
		}
		#endregion

		#region Dispose()
		public virtual void Dispose()
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

		#region struct WINDOWPLACEMENT
		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPLACEMENT
		{
			public uint length;
			public uint flags;
			public uint showCmd;
			public POINT ptMinPosition;
			public POINT ptMaxPosition;
			public RECT rcNormalPosition;

		}
		#endregion

		#endregion

		//PUBLIC PROPERTIES
		#region public properties
		
		public bool							SqlEnabled { get { return sqlEnabled; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region UpdateInfo()
		public void UpdateInfo(ExportUnit exportUnit, bool setAsRequestFinished)
		{
			Article article = exportUnit.Article;

			if (article.TransactionId != null)
			{
				//if (article.TransactionId.Value > 0)
					Update(exportUnit, setAsRequestFinished);
				//else
			    //		Progress_Changed(exportUnit, 100, "ILLiad update is disabled for negative TN.");
			}
			else
				Progress_Changed(exportUnit, 100, "Can't update ILLiad, Transaction Number is null!!!");
		}
		#endregion

		#region ExportArticle()
		public virtual void ExportArticle(ExportUnit exportUnit)
		{
			Export(exportUnit);
		}
        #endregion

		#region GetIlliadInstance()
		public static IILLiad GetIlliadInstance()
		{
			try
			{				
				switch (BscanILL.SETTINGS.Settings.Instance.Export.ILLiad.Version)
				{
					case BscanILL.Export.ILLiad.ILLiadVersion.Version7_1_8_0: return new ILLiad7_1_8_0();
					case ILLiadVersion.Version7_2_0_0: return new ILLiad7_2_0_0();
					case ILLiadVersion.Version7_3_0_0: return new ILLiad7_3_0_0();
					case ILLiadVersion.Version7_4_0_0: return new ILLiad7_4_0_0();
					case ILLiadVersion.Version8_0_0_0: return new ILLiad8_0_0_0();
					case ILLiadVersion.Version8_1_0_0: return new ILLiad8_1_0_0();
					case ILLiadVersion.Version8_1_4_0: return new ILLiad8_1_4_0();
					default: throw new IllException(ErrorCode.ILLiadUnsupportedVersion);
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				throw new IllException(ErrorCode.IlliadNotRunning, ex.Message);
			}
		}
		#endregion

		#region GetPullslip()
		public BscanILL.Export.ILL.PullslipReader.IPullslip GetPullslip(int transactionNumber)
		{
			if (SqlEnabled)
			{
				BscanILL.Export.ILL.TransactionPair pair = GetSqlTransaction(transactionNumber);

				if (pair != null)
				{
					BscanILL.Export.ILL.PullslipReader.IPullslip pullslip = pair.GetPullslip();

					if (pullslip == null)
						throw new Exception("Can't find article " + transactionNumber.ToString() + " in the SQL database!");

					return pullslip;
				}
				else
					throw new Exception("Can't find article " + transactionNumber.ToString() + " in the SQL database!");
			}

			return null;
		}
		#endregion

		#region CheckArticleInDb()
		public void CheckArticleInDb(ExportUnit exportUnit)
		{
			if (SqlEnabled)
			{
				Article article = exportUnit.Article;

				if (article.TransactionId != null && article.TransactionId.Value > 0)
				{
					Progress_Changed(exportUnit, -1, "SQL checking is enabled.");

					BscanILL.Export.ILL.TransactionPair pair = null;

					Progress_Changed(exportUnit, -1, "Checking article settings in the SQL database");

					//getting transaction
					if (article.TransactionId != null)
						pair = GetSqlTransaction(Convert.ToInt32(article.TransactionId.Value));

					if (pair == null && article.TransactionId != null)
						pair = GetSqlTransactionFromIllNumber(article.TransactionId.Value.ToString());

					if (pair == null && article.IllNumber != null && article.IllNumber.Trim().Length > 0)
						pair = GetSqlTransactionFromIllNumber(article.IllNumber);


					if (pair == null)
						throw new Exception("Can't find article " + (article.TransactionId.HasValue ? article.TransactionId.Value.ToString() : article.IllNumber) + " in the SQL database!");

					if (article.TransactionId.HasValue == false)
					{
						article.TransactionId = pair.TransactionRow.TransactionNumber;
						Progress_Changed(exportUnit, -1, "Transaction number " + article.TransactionId + " was pulled from SQL database.");
					}

					switch (article.ExportType)
					{
						case ExportType.Ariel:
						case ExportType.Email:
						case ExportType.Ftp:
						case ExportType.FtpDir:
						case ExportType.SaveOnDisk:
							{
								if (pair.TransactionRow.IsILLNumberNull() == false && article.IllNumber != pair.TransactionRow.ILLNumber)
								{
									article.IllNumber = pair.TransactionRow.ILLNumber;
									Progress_Changed(exportUnit, -1, "ILL number " + article.IllNumber + " was pulled and fixed from SQL database.");
								}

								if (article.ExportType == ExportType.Ariel)
								{
									if (pair.TransactionRow.IsPatronNull() == false && article.Patron.ToLower() != pair.TransactionRow.Patron.ToLower())
									{
										string message = "The provided Patron for transaction '" + article.TransactionId + "' is different than the Patron in the database!" + Environment.NewLine + Environment.NewLine;

										message += "Entered Patron: " + article.Patron + Environment.NewLine;
										message += "Database Patron: " + pair.TransactionRow.Patron + Environment.NewLine + Environment.NewLine;

										message += "Do you want to change Patron to the one in the database?";

										if (MessageBox.Show(message, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
										{
											article.Patron = pair.TransactionRow.Patron;
											Progress_Changed(exportUnit, -1, "Patron " + article.Patron + " was pulled and fixed from SQL database.");
										}
									}

									if (pair.LenderAddressRow != null)
									{
										if (pair.LenderAddressRow.IsArielAddressNull() == false && article.Address.ToLower() != pair.LenderAddressRow.ArielAddress.ToLower())
										{
											string message = "The provided Ariel address for transaction '" + article.TransactionId + "' is different than the Ariel address in the database!" + Environment.NewLine + Environment.NewLine;

											message += "Entered Ariel Address: " + article.Address + Environment.NewLine;
											message += "Database Ariel Address: " + pair.LenderAddressRow.ArielAddress + Environment.NewLine + Environment.NewLine;

											message += "Do you want to change Ariel address to the one in the database?";

											if (MessageBox.Show(message, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
											{
												article.Address = pair.LenderAddressRow.ArielAddress;
												Progress_Changed(exportUnit, -1, "Ariel address " + article.Address + " was pulled and fixed from SQL database.");
											}
										}
									}
								}

								if (article.ExportType == ExportType.Email)
								{
									if (pair.LenderAddressRow != null)
									{
										if (pair.LenderAddressRow.IsEMailAddressNull() == false && article.Address.ToLower() != pair.LenderAddressRow.EMailAddress.ToLower())
										{
											string message = "The provided Email address for transaction '" + article.TransactionId + "' is different than the Email address in the database!" + Environment.NewLine + Environment.NewLine;

											message += "Entered Email: " + article.Address + Environment.NewLine;
											message += "Database Email: " + pair.LenderAddressRow.EMailAddress + Environment.NewLine + Environment.NewLine;

											message += "Do you want to change Email address to the one in the database?";

											if (MessageBox.Show(message, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
											{
												article.Address = pair.LenderAddressRow.EMailAddress;
												Progress_Changed(exportUnit, -1, "E-mail address " + article.Address + " was pulled and fixed from SQL database.");
											}
										}
									}
								}
							} break;
						case ExportType.ILLiad:
							{
							} break;
						case ExportType.Odyssey:
							{
								string requestStatus = pair.TransactionRow.TransactionStatus;

								if (pair.TransactionRow.TransactionStatus.ToLower().Contains("in stacks searching") == false)
									MessageBox.Show("Request " + pair.TransactionRow.TransactionNumber + " is not in 'In Stacks Searching' and " +
										"Odyssey Helper will not pick it up! Please open up ILLiad and change request status to 'In Stacks Searching'.",
										"", MessageBoxButtons.OK, MessageBoxIcon.Warning);
							} break;
					}
					Progress_Changed(exportUnit, -1, "SQL Checking finished.");
				}
				else
					Progress_Changed(exportUnit, -1, "SQL Checking disabled for negative TN articles.");
			}
			else
				Progress_Changed(exportUnit, -1, "SQL checking is disabled.");
		}
		#endregion

		#region GetSqlTransaction()
		public virtual BscanILL.Export.ILL.TransactionPair GetSqlTransaction(int transactionId)
		{
			throw new Exception("Unsupported function GetTransactionsRow()!");
		}
		#endregion

		#region GetSqlTransactionFromIllNumber()
		public virtual BscanILL.Export.ILL.TransactionPair GetSqlTransactionFromIllNumber(string illNumber)
		{
			throw new Exception("Unsupported function GetTransactionsRowFromIllNumber()!");
		}
		#endregion

		#region GetLenderAddress()
		/*public virtual DsIlliad.LenderAddressesRow GetLenderAddress(DsIlliad.TransactionsRow transactionsRow)
		{
			throw new Exception("Unsupported function GetLenderAddress()!");
		}*/
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region SetILLiadToMainPosition()
		private void SetILLiadToMainPosition(ExportUnit exportUnit)
		{
			try
			{
				Progress_Comment(exportUnit, "Checking user interface...");
				CloseUnexpectedWindows();
				Progress_Changed(exportUnit, -1, "Pop ups checked.");

				Progress_Changed(exportUnit, -1, "Initialization was succesfull.");
			}
			catch (IllException ex)
			{
				throw new IllException(ex.ErrorCode, ex.Message);
			}
			catch
			{
				throw new IllException(ErrorCode.IlliadNotRunning);
			}
		}
		#endregion

		#region GetILLiadWindowHandle()
		private static IntPtr GetILLiadWindowHandle()
		{
			Process[] processes = Process.GetProcessesByName("ILLiadClient");
			Process illiadProcess = null;

			if (processes.Length > 0)
				illiadProcess = processes[0];
			else
				throw new IllException(ErrorCode.IlliadNotRunning);

			hWndStatic = IntPtr.Zero;
			EnumWindows(findMainWindowStatic, IntPtr.Zero);

			if (hWndStatic == IntPtr.Zero)
				throw new IllException(ErrorCode.IlliadNoWindowHndl);

			return hWndStatic;
		}
		#endregion

		#region FindMainWindowStatic()
		private static Int32 FindMainWindowStatic(IntPtr hWindow, IntPtr lParam)
		{
			StringBuilder controlText = new StringBuilder(99);
			int length;

			length = GetWindowText(hWindow, controlText, 100);
			if (length == 0)
				return 1;

			if (controlText.ToString().ToLower().StartsWith("illiad version 7.1"))
			{
				hWndStatic = hWindow;
				//illiadVersion = ILLiadVersion.Version7_1_8_0;
				return 0;
			}
			else if (controlText.ToString().ToLower().StartsWith("illiad version 7.2"))
			{
				hWndStatic = hWindow;
				//illiadVersion = ILLiadVersion.Version7_2_0_0;
				return 0;
			}
			else if (controlText.ToString().ToLower().StartsWith("illiad version 7.3"))
			{
				hWndStatic = hWindow;
				//illiadVersion = ILLiadVersion.Version7_3_0_0;
				return 0;
			}
			else if (controlText.ToString().ToLower().StartsWith("illiad version 7.4"))
			{
				hWndStatic = hWindow;
				//illiadVersion = ILLiadVersion.Version7_4_0_0;
				return 0;
			}
			else if (controlText.ToString().ToLower().StartsWith("illiad client 8.0.0"))
			{
				hWndStatic = hWindow;
				//illiadVersion = ILLiadVersion.Version8_0_0_0;
				return 0;
			}
			return 1;
		}
		#endregion	

		#region SwitchToDocDelivery()
		private void SwitchToDocDelivery(ExportUnit exportUnit, IntPtr hWnd)
		{
			Progress_Comment(exportUnit, "Switching to documet delivery mode...");

			ShowWindowAsync(hWnd, SW_SHOWNORMAL);
			WindowToTop(hWnd);

			keybd_event(0x79, 0, 0, 0); //Alt key down
			keybd_event(0x79, 0, KEYEVENTF_KEYUP, 0); //Alt Key up

			Thread.Sleep(500);
				
			for(int i = 0; i < 2; i++)
			{
				keybd_event(0x28, 0, 0, 0); //Down arrow key down
				keybd_event(0x28, 0, KEYEVENTF_KEYUP, 0); //Down arrow Key up

				Thread.Sleep(300);
			}
				
			keybd_event(0xD, 0, 0, 0); //Enter key down
			keybd_event(0xD, 0, KEYEVENTF_KEYUP, 0); //Enter Key up
			Thread.Sleep(500);
			
			Progress_Changed(exportUnit, -1, "ILLiad was switched to documet delivery mode.");
			Thread.Sleep(1000);
		}
		#endregion
		
		#region SwitchToLending()
		private void SwitchToLending(ExportUnit exportUnit, IntPtr hWnd)
		{
			Progress_Comment(exportUnit, "Switching to lending mode...");

			ShowWindowAsync(hWnd, SW_SHOWNORMAL);
			WindowToTop(hWnd);

			keybd_event(0x79, 0, 0, 0); //Alt key down
			keybd_event(0x79, 0, KEYEVENTF_KEYUP, 0); //Alt Key up
			Thread.Sleep(500);
				
			for(int i = 0; i < 3; i++)
			{
				keybd_event(0x28, 0, 0, 0); //Down arrow key down
				keybd_event(0x28, 0, KEYEVENTF_KEYUP, 0); //Down arrow Key up

				Thread.Sleep(300);
			}

			keybd_event(0xD, 0, 0, 0); //Enter key down
			keybd_event(0xD, 0, KEYEVENTF_KEYUP, 0); //Enter Key up
			Thread.Sleep(500);
			
			Progress_Changed(exportUnit, -1, "ILLiad was switched to lending mode.");
			Thread.Sleep(1000);
		}
		#endregion

		#region CloseUnexpectedWindows()
		private void CloseUnexpectedWindows()
		{
			IntPtr hWndDialog;
			IntPtr hWndControl;
			int result;

			hWndDialog = FindWindow("#32770", "Select Image To Import");
			if (hWndDialog != IntPtr.Zero)
			{
				if ((hWndControl = FindWindowEx(hWndDialog, IntPtr.Zero, "Button", "Cancel")) != IntPtr.Zero)
				{
					result = SendMessage(hWndDialog, WM_SETFOCUS, hWndControl.ToInt32(), 0);
					result = SendMessage(hWndControl, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
					result += SendMessage(hWndControl, WM_LBUTTONUP, 0, 65536 * 14 + 19);
					if (result > 0)
						throw new IllException(ErrorCode.IlliadCantCloseSelectWindow);
				}
				else
					throw new IllException(ErrorCode.IlliadCantCloseSelectWindow);

				Thread.Sleep(200);
			}

			hWndDialog = FindWindow("TLendingUSSForm", "Update Stacks Search Form");
			if (hWndDialog != IntPtr.Zero)
			{
				throw new IllException(ErrorCode.IlliadUpdateWindowOpen);
			}

			hWndDialog = FindWindow("TScanForm", "Lending Scanning");
			if (hWndDialog != IntPtr.Zero)
			{
				throw new IllException(ErrorCode.IlliadScanWindowOpen);
			}
		}
		#endregion

		
		#region Update()
		protected virtual void Update(ExportUnit exportUnit, bool setAsRequestFinished)
		{
			IntPtr hWnd = GetILLiadWindowHandle();

			SetILLiadToMainPosition(exportUnit);
			SwitchToLending(exportUnit, hWnd);
			SelectUpdateStacksFromMenu(exportUnit, hWnd);
			ProcessUpdateStacksWindow(exportUnit);

			if (setAsRequestFinished)
				UpdateToRequestFinished(exportUnit, hWnd);

			Progress_Changed(exportUnit, 100, "");
		}
		#endregion

		#region SelectUpdateStacksFromMenu()
		protected virtual void SelectUpdateStacksFromMenu(ExportUnit exportUnit, IntPtr hWnd)
		{
			Progress_Comment(exportUnit, "Selecting Update Stacks Search Results from menu...");

			WindowToTop(hWnd);

			keybd_event(0x79, 0, 0, 0); //Alt key down
			keybd_event(0x79, 0, KEYEVENTF_KEYUP, 0); //Alt Key up

			Thread.Sleep(500);

			for(int i = 0; i < 4; i++)
			{
				keybd_event(0x27, 0, 0, 0); //Right arrow key down
				keybd_event(0x27, 0, KEYEVENTF_KEYUP, 0); //Right arrow Key up

				Thread.Sleep(200);
			}

			keybd_event(0x28, 0, 0, 0); //Down arrow key down
			keybd_event(0x28, 0, KEYEVENTF_KEYUP, 0); //Down arrow Key up

			Thread.Sleep(200);
				
			keybd_event(0x28, 0, 0, 0); //Down arrow key down
			keybd_event(0x28, 0, KEYEVENTF_KEYUP, 0); //Down arrow Key up

			Thread.Sleep(500);

			keybd_event(0xD, 0, 0, 0); //Enter key down
			keybd_event(0xD, 0, KEYEVENTF_KEYUP, 0); //Enter Key up
			Thread.Sleep(500);

			Progress_Changed(exportUnit, 54, "Menu item selected successfully.");
		}
		#endregion

		#region ProcessUpdateStacksWindow()
		protected virtual void ProcessUpdateStacksWindow(ExportUnit exportUnit)
		{
			throw new Exception("Method ProcessUpdateStacksWindow() must be overriden!");
		}
		#endregion

		#region ProcessAddBillingChargesWindow()
		protected void ProcessAddBillingChargesWindow(ExportUnit exportUnit)
		{
			IntPtr	hWndBilling = IntPtr.Zero;
			IntPtr	hWndChargeButton;
			int		attempts = 0;
			int		result;

			try
			{
				//locating window
				Progress_Comment(exportUnit, "Waiting for Add Billing Charges Form window to be opened...");

				while ((hWndBilling = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TAddBillingForm", "Add Billing Charges Form")) == IntPtr.Zero)
				{
					if (attempts++ > 8)
					{
						return;
						//throw new IllException(ErrorCode.ILLiadCantLocateBillingFormWnd);
					}

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, 67, "Add Billing Charges Form window located successfully.");

				Thread.Sleep(500);

				//locating controls
				Progress_Comment(exportUnit, "Locating Add Billing Charges Form window controls...");

				hWndChargeButton = FindWindowEx(hWndBilling, IntPtr.Zero, "TBitBtn", "Charge");
				if (hWndChargeButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateBillingFormControls, "Can't locate button Charge in \"Add Billing Charges Form\" window! Please open up ILLiad and check status.");

				//pressing charge button
				Progress_Comment(exportUnit, "Pressing Charge button in Add Billing Charges Form window...");
				result = PostMessage(hWndChargeButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndChargeButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
				
				Progress_Changed(exportUnit, 69, "Charge button pressed.");
				Thread.Sleep(1000);

				//processing confirmation window, if any
				ProcessBillingConfirmationWindow(exportUnit);

				//waiting for window to be closed
				Progress_Comment(exportUnit, "Waiting for Add Billing Charges Form window to close...");
				attempts = 0;
				while (FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TAddBillingForm", "Add Billing Charges Form") != IntPtr.Zero)
				{
					if (attempts++ > 10)
						throw new IllException(ErrorCode.ILLiadBillingFormNotClosing);

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, 73, "Add Billing Charges Form window closed successfully.");
			}
			catch (IllException ex)
			{
				if (hWndBilling != IntPtr.Zero)
					SendMessage(hWndBilling, WM_CLOSE, 0, 0);

				throw ex;
			}
		}
		#endregion

		#region ProcessBillingConfirmationWindow()
		private void ProcessBillingConfirmationWindow(ExportUnit exportUnit)
		{
			IntPtr hWndInfo = IntPtr.Zero;
			IntPtr hWndYesButton;
			int attempts = 0;
			int result;

			try
			{
				//locating window
				Progress_Comment(exportUnit, "Waiting for billing confirmation window to be opened...");

				while ((hWndInfo = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TMessageForm", "Information")) == IntPtr.Zero)
				{
					if (attempts++ > 10)
					{
						return;
						//throw new IllException(ErrorCode.ILLiadCantLocateBillingFormWnd);
					}

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, 70, "Billing confirmation window located successfully.");
				Thread.Sleep(500);

				//locating controls
				Progress_Comment(exportUnit, "Locating Add Billing Charges Form window controls...");

				hWndYesButton = FindWindowEx(hWndInfo, IntPtr.Zero, "TButton", "&Yes");
				if (hWndYesButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateBillingConfirmationControls, "Can't locate button Yes in billing confirmation window! Please open up ILLiad and check status.");

				//pressing yes button
				Progress_Comment(exportUnit, "Pressing Yes button in billing confirmation window...");
				result = PostMessage(hWndYesButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndYesButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
				
				Progress_Changed(exportUnit, 71, "Yes button pressed.");
				Thread.Sleep(1000);

				//waiting for window to be closed
				Progress_Comment(exportUnit, "Waiting for billing confirmation window to close...");
				attempts = 0;
				while (FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TMessageForm", "Information") != IntPtr.Zero)
				{
					if (attempts++ > 10)
						throw new IllException(ErrorCode.ILLiadBillingConfirmationNotClosing);

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, 72, "billing confirmation window closed successfully.");
			}
			catch (IllException ex)
			{
				if (hWndInfo != IntPtr.Zero)
					SendMessage(hWndInfo, WM_CLOSE, 0, 0);

				throw ex;
			}
		}
		#endregion
		

		#region UpdateToRequestFinished()
		private void UpdateToRequestFinished(ExportUnit exportUnit, IntPtr hWnd)
		{
			SetILLiadToMainPosition(exportUnit);
			SelectEditRequestFromMenu(exportUnit, hWnd);
			ProcessGeneralSearchWindow(exportUnit);
		}
		#endregion

		#region SelectEditRequestFromMenu()
		private void SelectEditRequestFromMenu(ExportUnit exportUnit, IntPtr hWnd)
		{
			Progress_Comment(exportUnit, "Selecting Edit Request from menu...");

			WindowToTop(hWnd);

			keybd_event(0x79, 0, 0, 0); //Alt key down
			keybd_event(0x79, 0, KEYEVENTF_KEYUP, 0); //Alt Key up

			Thread.Sleep(500);

			keybd_event(0x45, 0, 0, 0); //E key down
			keybd_event(0x45, 0, KEYEVENTF_KEYUP, 0); //E Key up

			Thread.Sleep(200);
				
			keybd_event(0xD, 0, 0, 0); //Enter key down
			keybd_event(0xD, 0, KEYEVENTF_KEYUP, 0); //Enter Key up

			Thread.Sleep(500);

			Progress_Changed(exportUnit, 77,"Menu item selected successfully.");
		}
		#endregion

		#region ProcessGeneralSearchWindow()
		protected virtual void ProcessGeneralSearchWindow(ExportUnit exportUnit)
		{
			throw new Exception("Method ProcessGeneralSearchWindow() must be overriden!");
		}
		#endregion

		#region ProcessGeneralUpdateWindow()
		protected void ProcessGeneralUpdateWindow(ExportUnit exportUnit)
		{
			IntPtr hWndGeneralUpdate = IntPtr.Zero;
			int attempts = 0;

			try
			{
				//locating window
				Progress_Comment(exportUnit, "Waiting for General Update Form window to be opened...");

				while ((hWndGeneralUpdate = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TGenUpdate", "General Update Form")) == IntPtr.Zero)
				{
					if (attempts > 10)
						throw new IllException(ErrorCode.ILLiadCantLocateGeneralUpdateWnd);

					Thread.Sleep(1000);
					attempts++;
				}

				Progress_Changed(exportUnit, 87, "General Update Form window located successfully.");

				Thread.Sleep(1500);

				//selecting Request Finished from menu
				Progress_Comment(exportUnit, "Selecting Request Finished from menu...");

				WindowToTop(hWndGeneralUpdate);

				keybd_event(0x79, 0, 0, 0); //Alt key down
				keybd_event(0x79, 0, KEYEVENTF_KEYUP, 0); //Alt Key up

				Thread.Sleep(200);

				for (int i = 0; i < 5; i++)
				{
					keybd_event(0x27, 0, 0, 0); //Right arrow key down
					keybd_event(0x27, 0, KEYEVENTF_KEYUP, 0); //Right arrow Key up

					Thread.Sleep(200);
				}

				for (int i = 0; i < 17; i++)
				{
					keybd_event(0x28, 0, 0, 0); //Down arrow key down
					keybd_event(0x28, 0, KEYEVENTF_KEYUP, 0); //Down arrow Key up

					Thread.Sleep(200);
				}

				Thread.Sleep(500);

				keybd_event(0xD, 0, 0, 0); //Enter key down
				keybd_event(0xD, 0, KEYEVENTF_KEYUP, 0); //Enter Key up

				Thread.Sleep(500);

				Progress_Changed(exportUnit, 89, "Menu item selected successfully.");

				//processing General Update Form
				ProcessGeneralUpdateInfoWindow(exportUnit);

				//closing this window
				Progress_Comment(exportUnit, "Closing General Update Form window...");
				CloseIllWindow(hWndGeneralUpdate);

				//waiting window to be closed
				Progress_Comment(exportUnit, "Waiting for General Update Form window to close...");
				attempts = 0;

				while (FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TGenUpdate", "General Update Form") != IntPtr.Zero)
				{
					if (attempts++ > 30)
						throw new IllException(ErrorCode.ILLiadGeneralUpdateNotClosing);

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, 94, "General Update Form window closed successfully.");
			}
			catch (IllException ex)
			{
				if (hWndGeneralUpdate != IntPtr.Zero)
					SendMessage(hWndGeneralUpdate, WM_CLOSE, 0, 0);

				throw ex;
			}
		}
		#endregion

		#region ProcessGeneralUpdateInfoWindow()
		protected virtual void ProcessGeneralUpdateInfoWindow(ExportUnit exportUnit)
		{
			throw new Exception("Method ProcessGeneralUpdateInfoWindow() must be overriden!");
		}
		#endregion


		#region Export()
		protected virtual void Export(ExportUnit exportUnit)
		{
			IntPtr hWnd = GetILLiadWindowHandle();

			SetILLiadToMainPosition(exportUnit);
			SwitchToDocDelivery(exportUnit, hWnd);
			SelectUpdateDocDeliveryMenu(exportUnit, hWnd);
			ProcessUpdateStackSearchWindow(exportUnit);

			Progress_Changed(exportUnit, 100, "");
		}
		#endregion
		
		#region SelectUpdateDocDeliveryMenu()
		private void SelectUpdateDocDeliveryMenu(ExportUnit exportUnit, IntPtr hWnd)
		{
			Progress_Comment(exportUnit, "Selecting Update Document Delivery Search Results from menu...");

			unchecked
			{
				WindowToTop(hWnd);

				keybd_event(0x79, 0, 0, 0); //Alt key down
				keybd_event(0x79, 0, KEYEVENTF_KEYUP, 0); //Alt Key up

				Thread.Sleep(500);

				keybd_event(0x27, 0, 0, 0); //Right arrow key down
				keybd_event(0x27, 0, KEYEVENTF_KEYUP, 0); //Right arrow Key up

				Thread.Sleep(200);

				keybd_event(0x27, 0, 0, 0); //Right arrow key down
				keybd_event(0x27, 0, KEYEVENTF_KEYUP, 0); //Right arrow Key up

				Thread.Sleep(500);

				keybd_event(0x28, 0, 0, 0); //Down arrow key down
				keybd_event(0x28, 0, KEYEVENTF_KEYUP, 0); //Down arrow Key up

				Thread.Sleep(500);
				
				keybd_event(0x28, 0, 0, 0); //Down arrow key down
				keybd_event(0x28, 0, KEYEVENTF_KEYUP, 0); //Down arrow Key up

				Thread.Sleep(500);
				//BringWindowToTop(intILLiad_hWnd)

				keybd_event(0xD, 0, 0, 0); //Enter key down
				keybd_event(0xD, 0, KEYEVENTF_KEYUP, 0); //Enter Key up
				Thread.Sleep(500);
			}

			Thread.Sleep(500);

			Progress_Changed(exportUnit, 20, "Menu item selected successfully.");
		}
		#endregion

		#region ProcessUpdateStackSearchWindow()		
		private void ProcessUpdateStackSearchWindow(ExportUnit exportUnit)
		{
			IntPtr hWndStackSearch = IntPtr.Zero;
			IntPtr hWndRadioButtonNo;
			IntPtr hWndSearchButton;
			IntPtr hWndSearchBox;
			IntPtr hWndUpdateBox;
			IntPtr hWndScanButton;
			IntPtr hWndGroup;
			IntPtr hWndTemp;
			int attempts = 0;

			try
			{
				//locating window
				Progress_Comment(exportUnit, "Waiting for Update Stacks Search window to be opened...");

				while ((hWndStackSearch = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TDDUSSForm", "Document Delivery - Update Stacks Search")) == IntPtr.Zero)
				{
					if (attempts > 10)
						throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchWnd);

					Thread.Sleep(1000);
					attempts++;
				}
				Progress_Changed(exportUnit, 10, "Update Stacks Search window located successfully.");

				Thread.Sleep(500);

				//locating controls
				Progress_Comment(exportUnit, "Locating Update Stacks Search window controls...");

				hWndScanButton = FindWindowEx(hWndStackSearch, IntPtr.Zero, "TcxButton", "Scan &Now");
				if (hWndScanButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate Scan Now button in \"Update Stacks Search\" window! Please open up ILLiad and check status.");

				hWndTemp = FindWindowEx(hWndStackSearch, IntPtr.Zero, "TcxGroupBox", "Select Record");

				hWndSearchButton = FindWindowEx(hWndTemp, IntPtr.Zero, "TcxButton", "&Search");
				if (hWndSearchButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate Search button in \"Update Stacks Search\" window! Please open up ILLiad and check status.");

				IntPtr hWndTemp2 = FindWindowEx(hWndTemp, IntPtr.Zero, null, "Select Only \"In DD Stacks Searching\" Status");
				hWndRadioButtonNo = FindWindowEx(hWndTemp2, IntPtr.Zero, "TcxCustomRadioGroupButton", "No");
				if (hWndRadioButtonNo == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate radio button 'No' in \"Update Stacks Search\" window! Please open up ILLiad and check status.");

				hWndSearchBox = FindWindowEx(hWndTemp, IntPtr.Zero, "TcxTextEdit", null);
				if (hWndSearchBox == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate Transaction Number search text box in \"Update Stacks Search\" window! Please open up ILLiad and check status.");

				hWndGroup = FindWindowEx(hWndStackSearch, IntPtr.Zero, "TcxGroupBox", "Update Stacks Search");

				hWndTemp = FindWindowEx(hWndGroup, IntPtr.Zero, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);

				hWndUpdateBox = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				if (hWndUpdateBox == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate Transaction Number update text box in \"Update Stacks Search\" window! Please open up ILLiad and check status.");

				Thread.Sleep(200);
				Progress_Changed(exportUnit, 15, "Update Stacks Search window controls located successfully.");

				//entering transaction number
				Progress_Comment(exportUnit, "Entering Transaction Number into Update Stacks Search window controls...");

				if(SetTextFieldText(hWndSearchBox, exportUnit.Article.TransactionId.ToString()))
					Progress_Changed(exportUnit, 20, "Transaction Number entered successfully.");
				else
					throw new IllException(ErrorCode.ILLiadCantEnterDataToUpdateStacksSearchWnd);
				//int result = SendMessage(hWndSearchBox, WM_SETTEXT, (int)0, article.Id.ToString());
				//if (result != 0)
					//Progress_Changed(exportUnit, 20, "Transaction Number entered successfully.");
				//else
					//throw new IllException(ErrorCode.ILLiadCantEnterDataToUpdateStacksSearchWnd);

				Thread.Sleep(200);

				//selecting radio button 'No'
				Progress_Comment(exportUnit, "Selecting radio button 'No' in Update Stacks Search window controls...");

				int result = PostMessage(hWndRadioButtonNo, WM_LBUTTONDOWN, 1, 65536 * 5 + 5);
				result = PostMessage(hWndRadioButtonNo, WM_LBUTTONDOWN, 1, 65536 * 5 + 5);
				result += PostMessage(hWndRadioButtonNo, WM_LBUTTONUP, 0, 65536 * 5 + 5);

				if (result == 0)
					throw new IllException(ErrorCode.ILLiadCantCheckRadioButtonInUpdateStacksSearchWnd);

				Progress_Changed(exportUnit, 25, "Radio button checked successfully.");
				Thread.Sleep(1500);
				
				//pressing search button
				Progress_Comment(exportUnit, "Pressing Search button in Update Stacks Search window...");
				result = PostMessage(hWndSearchButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndSearchButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);

				result = PostMessage(hWndSearchButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndSearchButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
				Progress_Changed(exportUnit, 30, "Search button pressed.");

				Thread.Sleep(1000);

				//checking if request found - checking if bottom transaction number field not empty
				Progress_Comment(exportUnit, "Checking if article found...");

				attempts = 0;
				while (GetTextFieldText(hWndUpdateBox).Length == 0)
				{
					if (attempts++ > 10)
						throw new IllException(ErrorCode.ILLiadUpdateStacksSearchArticleNotFound);

					Thread.Sleep(500);
				}
				
				Progress_Changed(exportUnit, 35, "Article found.");

				//pressing Scan Now button
				Progress_Comment(exportUnit, "Pressing Scan Now button in Update Stacks Search window...");
				result = PostMessage(hWndScanButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndScanButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
				Progress_Changed(exportUnit, 40, "Scan Now button pressed.");

				//dealing with charges
				Progress_Comment(exportUnit, "Checking the billing...");
				ProcessAddBillingChargesWindow(exportUnit);
				Progress_Changed(exportUnit, 71, "Billing checking finished.");

				//processing doc delivery scanning window
				Thread.Sleep(200);
				Progress_Changed(exportUnit, 41, "Lending Scanning window controls located successfully.");

				ProcessDocDeliveryScanning(exportUnit);

				//closing this window
				Progress_Comment(exportUnit, "Closing Update Stacks Search window...");
				CloseIllWindow(hWndStackSearch);

				Thread.Sleep(500);
				//wait while window open
				Progress_Comment(exportUnit, "Update Stacks Search window to close...");
				attempts = 0;
				while (FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TDDUSSForm", "Document Delivery - Update Stacks Search") != IntPtr.Zero)
				{
					Thread.Sleep(500);

					if (attempts++ > 20)
						throw new IllException(ErrorCode.ILLiadUpdateStacksSearchNotClosing);
				}

				Progress_Changed(exportUnit, 95, "Update Stacks Search window closed successfully.");
			}
			catch (IllException ex)
			{
				if (hWndStackSearch != IntPtr.Zero)
					SendMessage(hWndStackSearch, WM_CLOSE, 0, 0);

				throw ex;
			}
		}
		#endregion

		#region ProcessDocDeliveryScanning()
		/// <summary>
		/// Progress value is between 50-90
		/// </summary>
		/// <param name="article"></param>
		private void ProcessDocDeliveryScanning(ExportUnit exportUnit)
		{
			IntPtr	hWndScanning = IntPtr.Zero;
			IntPtr	hWndTemp;
			IntPtr	hWndDeliver;
			int		attempts = 0;
			int		result;

			try
			{
				//locating window
				Progress_Comment(exportUnit, "Waiting for Document Delivery Scanning window to be opened...");

				while ((hWndScanning = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TScanForm", "Document Delivery Scanning")) == IntPtr.Zero)
				{
					if (attempts > 10)
						throw new IllException(ErrorCode.ILLiadCantLocateDocDelineryScanningWnd);

					Thread.Sleep(1000);
					attempts++;
				}
				Progress_Changed(exportUnit, 50, "Document Delivery Scanning window located successfully.");

				Thread.Sleep(6000);
				CheckForTwainSharingViolationError(exportUnit);
				Thread.Sleep(1000);

				//locating controls
				hWndTemp = FindWindowEx(hWndScanning, IntPtr.Zero, "TPanel", "");
				hWndTemp = FindWindowEx(hWndScanning, hWndTemp, "TPanel", "");
				hWndTemp = FindWindowEx(hWndTemp, IntPtr.Zero, "TcxGroupBox", "Information");

				hWndDeliver = FindWindowEx(hWndTemp, IntPtr.Zero, "TcxButton", "Deliver");
				if (hWndDeliver == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateDocDeliveryScanningControls, "Can't locate Deliver button in \"Document Delivery Scanning\" window! Please open up ILLiad and check status.");

				//pressing import button
				Progress_Comment(exportUnit, "Pressing Import Image button...");

				WindowToTop(hWndScanning);

				keybd_event(0x11, 0, 0, 0); //Enter key ctrl down
				keybd_event(0x49, 0, 0, 0); //Enter key I down
				keybd_event(0x49, 0, KEYEVENTF_KEYUP, 0); //Enter key I up
				keybd_event(0x11, 0, KEYEVENTF_KEYUP, 0); //Enter Key ctrl up

				Thread.Sleep(500);
				Progress_Changed(exportUnit, 52, "Import Image button pressed successfully.");

				Thread.Sleep(1500);
				ProcessSelectImageWindow(exportUnit);

				Thread.Sleep(3000);

				//pressing Deliver button
				Progress_Comment(exportUnit, "Pressing Deliver button in Document Delivery Scanning window...");
				result = PostMessage(hWndDeliver, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndDeliver, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndDeliver, WM_LBUTTONUP, 0, 65536 * 14 + 19);
				Progress_Changed(exportUnit, -1, "Deliver button pressed.");

				//wait while window open
				Progress_Comment(exportUnit, "Document Delivery Scanning window to close...");
				Thread.Sleep(1000);
				attempts = 0;
				while (FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TScanForm", "Document Delivery Scanning") != IntPtr.Zero)
				{
					Thread.Sleep(1000);

					if (attempts++ > 200)
						throw new IllException(ErrorCode.ILLiadDocDeliveryScanningNotClosing);
				}

				Progress_Changed(exportUnit, 80, "Document Delivery Scanning window closed successfully.");
			}
			catch (IllException ex)
			{
				if (hWndScanning != IntPtr.Zero)
					SendMessage(hWndScanning, WM_CLOSE, 0, 0);

				throw ex;
			}
		}
		#endregion

		#region CheckForTwainSharingViolationError()
		private void CheckForTwainSharingViolationError(ExportUnit exportUnit)
		{
			IntPtr hWndTwainViolation;
			IntPtr hWndYesButton;
			int result;

			//locating window
			Progress_Comment(exportUnit, "Checking if TWAIN sharring violation has been detected...");

			if ((hWndTwainViolation = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "TWAIN driver")) != IntPtr.Zero)
			{
				Thread.Sleep(1000);

				Progress_Changed(exportUnit, -1, "TWAIN sharring violation detected.");

				//locating OK button
				Progress_Comment(exportUnit, "Locating OK button...");

				hWndYesButton = FindWindowEx(hWndTwainViolation, IntPtr.Zero, null, "OK");
				if (hWndYesButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadDocDelScanningTWAINViolationCantLocateControl, "Can't locate OK button in \"TWAIN driver\" message form! Please open up ILLiad and check status.");

				Progress_Changed(exportUnit, -1, "OK button located successfully.");

				//pressing OK button
				Progress_Comment(exportUnit, "Pressing OK button in TWAIN driver message form...");
				result = PostMessage(hWndYesButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndYesButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
				Progress_Changed(exportUnit, -1, "OK button pressed.");

				//waiting window to be closed
				Progress_Comment(exportUnit, "Waiting for TWAIN driver form to close...");

				int attempts = 0;
				while (FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, "TWAIN driver") != IntPtr.Zero)
				{
					if (attempts++ > 30)
						throw new IllException(ErrorCode.ILLiadInfoWndNotClosing);

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, -1, "TWAIN driver form closed.");
				Thread.Sleep(500);
			}
		}
		#endregion
	
		#region ProcessSelectImageWindow()
		private void ProcessSelectImageWindow(ExportUnit exportUnit)
		{
			int attempts = 0;
			IntPtr selectWindow = IntPtr.Zero;

			try
			{
				Progress_Comment(exportUnit, "Seeking for Select Image To Import window...");

				while ((selectWindow = FindWindow("#32770", "Select Image To Import")) == IntPtr.Zero)
				{
					if (attempts++ > 20)
						throw new IllException(ErrorCode.ILLiadCantLocateSelectWindow);

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, -1, "Select Image To Import window localized.");
				Thread.Sleep(1500);
				Progress_Comment(exportUnit, "Seeking for Select Image To Import window controls...");

				IntPtr openButton = FindWindowEx(selectWindow, IntPtr.Zero, "Button", @"&Open");
				if (openButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateSelectControls, "Can't locate Open button in \"Select Image To Import\" window! Please open up ILLiad and check status.");

				IntPtr hWndTmp = FindWindowEx(selectWindow, IntPtr.Zero, "ComboBoxEx32", null);
				if (hWndTmp == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateSelectControls, "Can't locate combo box in \"Select Image To Import\" window! Please open up ILLiad and check status.");
				hWndTmp = FindWindowEx(hWndTmp, IntPtr.Zero, "ComboBox", null);
				if (hWndTmp == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateSelectControls, "Can't locate combo box child in \"Select Image To Import\" window! Please open up ILLiad and check status.");
				hWndTmp = FindWindowEx(hWndTmp, IntPtr.Zero, "Edit", null);
				if (hWndTmp == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateSelectControls, "Can't locate combo box edit in \"Select Image To Import\" window! Please open up ILLiad and check status.");

				Progress_Changed(exportUnit, 60, "Select window controls located successfully.");
				Progress_Comment(exportUnit, "Entering data to Select Image To Import window...");

				Thread.Sleep(500);

				//entering file path
				if(SetTextFieldText(hWndTmp, exportUnit.Files[0].FullName) == false)
					throw new IllException(ErrorCode.ILLiadCantEnterDataToSelectWnd, "Can't enter image path to \"Select Image To Import\" window! Please open up ILLiad and check status.");

				Progress_Changed(exportUnit, -1, "Select window data entered successfully.");

				Thread.Sleep(300);
				Progress_Comment(exportUnit, "Pressing Open button in Select Image To Import window...");
				int result = PostMessage(openButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result = PostMessage(openButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(openButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);

				Progress_Changed(exportUnit, -1, "Open button in Select Image To Import window pressed.");

				attempts = 0;
				while (FindWindow("#32770", "Select Image To Import") != IntPtr.Zero)
				{
					if (attempts++ > 60)
						throw new IllException(ErrorCode.ILLiadSelectWndNotClosing);

					Thread.Sleep(1000);
				}

				Progress_Changed(exportUnit, 70, "");
			}
			catch (IllException ex)
			{
				if (selectWindow != IntPtr.Zero)
					SendMessage(selectWindow, WM_CLOSE, 0, 0);

				throw ex;
			}
		}
		#endregion	

		#region GetTextFieldText()
		internal static string GetTextFieldText(IntPtr hWndTextBox)
		{
			StringBuilder		textBoxText = new StringBuilder(100);
			SendMessage(hWndTextBox, WM_GETTEXT, 99, textBoxText);

			return  textBoxText.ToString().Trim();
		}
		#endregion

		#region SetTextFieldText()
		internal static bool SetTextFieldText(IntPtr hWndTextBox, string text)
		{
			Thread.Sleep(300);

			for (int i = 0; i < 50; i++)
			{
				if (SendMessage(hWndTextBox, WM_SETTEXT, (int)0, text) != 0)
					break;
				else
					Thread.Sleep(200);
			}

			Thread.Sleep(200);

			int attempts = 0;
			string textFieldText = GetTextFieldText(hWndTextBox);

			while (textFieldText != text)
			{
				Thread.Sleep(1000);

				if (attempts++ > 30)
					return false;

				textFieldText = GetTextFieldText(hWndTextBox);
			}

			return true;
		}
		#endregion

		#region CloseIllWindow()
		protected void CloseIllWindow(IntPtr hWindow)
		{
			WindowToTop(hWindow);
			
			Thread.Sleep(500);

			keybd_event(0x12, 0, 0, 0); //Enter key ctrl down
			keybd_event(0x73, 0, 0, 0); //Enter key I down
			keybd_event(0x73, 0, KEYEVENTF_KEYUP, 0); //Enter key I up
			keybd_event(0x12, 0, KEYEVENTF_KEYUP, 0); //Enter Key ctrl up
		}
		#endregion

		#region WindowToTop()
		protected void WindowToTop(IntPtr wndHandle)
		{
			Thread.Sleep(300);
			SetForegroundWindow(wndHandle);
			Thread.Sleep(300);
			BringWindowToTop(wndHandle);
			BringWindowToTop(wndHandle);

			Thread.Sleep(500);
		}
		#endregion

		#region IsMinimized()
		/*protected unsafe bool IsMinimized(IntPtr hWindow)
		{
			WINDOWPLACEMENT wp;

			if (GetWindowPlacement(hWindow, new IntPtr(&wp)))
				return ((wp.showCmd & SW_SHOWMINIMIZED) > 0);

			return false;
		}*/
		#endregion

		#endregion
	}
}
