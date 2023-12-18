using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using BscanILL.Misc;

namespace BscanILL.Export.ILLiad
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ILLiad7_1_8_0 : ILLiadBasics, IILLiad
	{

		#region constructor
		internal ILLiad7_1_8_0()
			:base()
		{
		}
		#endregion

		#region destructor
		public override void Dispose()
		{
			base.Dispose();
		}
		#endregion

		//PUBLIC METHODS
		#region public methods
		
		#endregion

		//PRIVATE METHODS
		#region private methods

		#region ProcessUpdateStacksWindow()
		protected override void ProcessUpdateStacksWindow(ExportUnit exportUnit)
		{
			IntPtr hWndUpdateStacks = IntPtr.Zero;
			IntPtr hWndGroup;
			IntPtr hWndRadioButtonNo;
			IntPtr hWndSearchButton;
			IntPtr hWndSearchBox;
			IntPtr hWndUpdateBox;
			IntPtr hWndPagesBox;
			IntPtr hWndMarkItemButton;
			IntPtr hWndTemp;
			int attempts = 0;

			try
			{
				//locating window
				Progress_Comment(exportUnit, "Waiting for Update Stacks Search Form window to be opened...");

				while ((hWndUpdateStacks = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TLendingUSSForm", "Update Stacks Search Form")) == IntPtr.Zero)
				{
					if (attempts > 10)
						throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksFormWnd);

					Thread.Sleep(1000);
					attempts++;
				}

				Progress_Changed(exportUnit, 52, "Update Stacks Search Form window located successfully.");

				Thread.Sleep(500);

				//locating controls
				Progress_Comment(exportUnit, "Locating Update Stacks Search Form window controls...");

				hWndMarkItemButton = FindWindowEx(hWndUpdateStacks, IntPtr.Zero, "TcxButton", "&Mark Item as Found");
				if (hWndMarkItemButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksFormControls, "Can't locate Mark Item as Found button in \"Update Stacks Search Form\" window! Please open up ILLiad and check status.");

				hWndGroup = FindWindowEx(hWndUpdateStacks, IntPtr.Zero, "TcxGroupBox", "Select Record");

				hWndSearchButton = FindWindowEx(hWndGroup, IntPtr.Zero, "TcxButton", "&Search");
				if (hWndSearchButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate Search button in \"Update Stacks Search Form\" window! Please open up ILLiad and check status.");

				IntPtr hWndTemp2 = FindWindowEx(hWndGroup, IntPtr.Zero, "TcxRadioGroup", null);
				hWndRadioButtonNo = FindWindowEx(hWndTemp2, IntPtr.Zero, "TcxCustomRadioGroupButton", "No");
				if (hWndRadioButtonNo == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate radio button 'No' in \"Update Stacks Search\" window! Please open up ILLiad and check status.");

				hWndTemp = FindWindowEx(hWndGroup, IntPtr.Zero, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);

				hWndSearchBox = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				if (hWndSearchBox == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate ILL Number search text box in \"Update Stacks Search Form\" window! Please open up ILLiad and check status.");

				hWndGroup = FindWindowEx(hWndUpdateStacks, IntPtr.Zero, "TcxGroupBox", "Update Stacks Search");
				hWndTemp = FindWindowEx(hWndGroup, IntPtr.Zero, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndPagesBox = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				if (hWndPagesBox == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate Pages update text box in \"Update Stacks Search Form\" window! Please open up ILLiad and check status.");
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				hWndTemp = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);

				hWndUpdateBox = FindWindowEx(hWndGroup, hWndTemp, "TcxTextEdit", null);
				if (hWndUpdateBox == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateUpdateStacksSearchControls, "Can't locate Transaction Number update text box in \"Update Stacks Search Form\" window! Please open up ILLiad and check status.");

				Thread.Sleep(200);
				Progress_Changed(exportUnit, 55, "Update Stacks Search window controls located successfully.");

				//entering transaction number
				WindowToTop(hWndUpdateStacks);
				Progress_Comment(exportUnit, "Entering Transaction Number into Update Stacks Search window controls...");

				if ((SetTextFieldText(hWndSearchBox, exportUnit.Article.TransactionId.Value.ToString())) == false)
					throw new IllException(ErrorCode.ILLiadCantEnterDataToUpdateStacksSearchWnd);
				//int result = SendMessage(hWndSearchBox, WM_SETTEXT, (int)0, article.Id.ToString());
				//if (result == 0)
					//throw new IllException(ErrorCode.ILLiadCantEnterDataToUpdateStacksSearchWnd);
					
				Progress_Changed(exportUnit, 57, "Transaction Number entered successfully.");
				Thread.Sleep(1500);

				//selecting radio button 'No'
				Progress_Comment(exportUnit, "Selecting radio button 'No' in Update Stacks Search window controls...");

				int result = PostMessage(hWndRadioButtonNo, WM_LBUTTONDOWN, 1, 65536 * 5 + 5);
				result = PostMessage(hWndRadioButtonNo, WM_LBUTTONDOWN, 1, 65536 * 5 + 5);
				result += PostMessage(hWndRadioButtonNo, WM_LBUTTONUP, 0, 65536 * 5 + 5);

				if (result == 0)
					throw new IllException(ErrorCode.ILLiadCantCheckRadioButtonInUpdateStacksSearchWnd);
					
				Progress_Changed(exportUnit, 58, "Radio button checked successfully.");
				Thread.Sleep(1500);

				//pressing search button
				result = PostMessage(hWndSearchButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result = PostMessage(hWndSearchButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndSearchButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);

				//keybd_event(0xD, 0, 0, 0); //Enter key down
				//keybd_event(0xD, 0, KEYEVENTF_KEYUP, 0); //Enter Key up
				Thread.Sleep(500);

				Progress_Changed(exportUnit, 59, "");
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
				
				Progress_Changed(exportUnit, 62, "Article found.");

				//entering pages number
				Progress_Comment(exportUnit, "Entering page number...");

				int pages;
				try
				{
					Bitmap bitmap = new Bitmap(exportUnit.Files[0].Directory.FullName + @"\" + Path.GetFileNameWithoutExtension(exportUnit.Files[0].FullName) + ".tif");

					pages = bitmap.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page);
				}
				catch (Exception ex)
				{
					throw new IllException(ErrorCode.ILLiadCantGetFrameCountInUpdateStacksFormWnd, "Can't get page count in \"Update Stacks Search Form\" window! Exception: " + ex.Message);
				}

				if(SetTextFieldText(hWndPagesBox, pages.ToString()) == false)
					throw new IllException(ErrorCode.ILLiadCantEnterDataToUpdateStacksForm);
				//result = SendMessage(hWndPagesBox, WM_SETTEXT, (int)0, pages.ToString());
				//if (result == 0)
					//throw new IllException(ErrorCode.ILLiadCantEnterDataToUpdateStacksForm);
					
				Progress_Changed(exportUnit, 65, "Transaction Number entered successfully.");

				//unchecking check box Odyssey Enabled
				Progress_Comment(exportUnit, "Determining if Odyssey Enabled check box checked...");
				hWndGroup = FindWindowEx(hWndUpdateStacks, IntPtr.Zero, "TcxGroupBox", "");
				hWndTemp = FindWindowEx(hWndGroup, IntPtr.Zero, "TcxCheckBox", "Odyssey Enabled");

				if (hWndTemp != IntPtr.Zero)
				{
					Progress_Changed(exportUnit, -1, "Odyssey Enabled localized.");
					Progress_Comment(exportUnit, "Clicking on check box Odyssey Enabled...");
					WindowToTop(hWndUpdateStacks);
					Thread.Sleep(400);
					result = SendMessage(hWndUpdateStacks, WM_SETFOCUS, hWndTemp.ToInt32(), 0);
					Thread.Sleep(100);
					result = SendMessage(hWndTemp, WM_LBUTTONDOWN, 1, 65536 * 5 + 19);
					Thread.Sleep(200);
					result = SendMessage(hWndTemp, WM_LBUTTONDOWN, 1, 65536 * 5 + 19);
					Thread.Sleep(200);
					result += SendMessage(hWndTemp, WM_LBUTTONUP, 0, 65536 * 5 + 19);
					Progress_Changed(exportUnit, -1, "Clicking on Check box successfull.");
					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, 66, ""); ;
				Thread.Sleep(1500);

				//pressing Mark Item As Found button
				Progress_Comment(exportUnit, "Pressing Mark Item As Found button in Update Stacks Search Form window...");
				result = PostMessage(hWndMarkItemButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndMarkItemButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);

				result = PostMessage(hWndMarkItemButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndMarkItemButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
				Progress_Changed(exportUnit, -1, "Mark Item As Found button pressed.");

				ProcessAddBillingChargesWindow(exportUnit);

				//closing this window
				Progress_Comment(exportUnit, "Closing Update Stacks Search Form window...");
				Thread.Sleep(1500);
				CloseIllWindow(hWndUpdateStacks);

				Thread.Sleep(200);

				//waiting window to be closed
				Progress_Comment(exportUnit, "Waiting for Update Stacks Search Form window to close...");
				attempts = 0;
				while (FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TLendingUSSForm", "Update Stacks Search Form") != IntPtr.Zero)
				{
					if (attempts++ > 10)
						throw new IllException(ErrorCode.ILLiadUpdateStacksFormNotClosing);

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, 75, "Update Stacks Search Form window closed successfully.");
			}
			catch (IllException ex)
			{
				if (hWndUpdateStacks != IntPtr.Zero)
					SendMessage(hWndUpdateStacks, WM_CLOSE, 0, 0);

				throw ex;
			}
		}
		#endregion

		#region ProcessGeneralSearchWindow()
		protected override void ProcessGeneralSearchWindow(ExportUnit exportUnit)
		{
			IntPtr hWndGeneralSearch = IntPtr.Zero;
			IntPtr hWndSearchButton;
			IntPtr hWndILLNumberBox;
			IntPtr hWndTemp;
			IntPtr hWndTabSheet;
			int attempts = 0;

			try
			{
				//locating window
				Progress_Comment(exportUnit, "Waiting for General Search Form window to be opened...");

				while ((hWndGeneralSearch = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TGeneralSearch", "General Search Form - Lending")) == IntPtr.Zero)
				{
					if (attempts > 10)
						throw new IllException(ErrorCode.ILLiadCantLocateGeneralSearchWnd);

					Thread.Sleep(1000);
					attempts++;
				}

				Progress_Changed(exportUnit, 79, "General Search Form window located successfully.");

				Thread.Sleep(500);

				//locating controls
				Progress_Comment(exportUnit, "Locating General Search Form window controls...");

				hWndTemp = FindWindowEx(hWndGeneralSearch, IntPtr.Zero, "TPageControl", null);
				if (hWndTemp == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateGeneralSearchControls, "Can't locate page control in \"General Search Form\" window! Please open up ILLiad and check status.");

				hWndTabSheet = FindWindowEx(hWndTemp, IntPtr.Zero, "TTabSheet", "Default Search");
				if (hWndTabSheet == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateGeneralSearchControls, "Can't locate tab sheet in \"General Search Form\" window! Please open up ILLiad and check status.");

				hWndSearchButton = FindWindowEx(hWndTabSheet, IntPtr.Zero, "TBitBtn", "Search");
				if (hWndSearchButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateGeneralSearchControls, "Can't locate Search button in \"General Search Form\" window! Please open up ILLiad and check status.");

				hWndTemp = FindWindowEx(hWndTabSheet, IntPtr.Zero, "TEdit", null);
				hWndTemp = FindWindowEx(hWndTabSheet, hWndTemp, "TEdit", null);
				hWndTemp = FindWindowEx(hWndTabSheet, hWndTemp, "TEdit", null);
				hWndTemp = FindWindowEx(hWndTabSheet, hWndTemp, "TEdit", null);

				hWndILLNumberBox = FindWindowEx(hWndTabSheet, hWndTemp, "TEdit", null);
				if (hWndILLNumberBox == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateGeneralSearchControls, "Can't locate ILL Number search text box in \"General Search Form\" window! Please open up ILLiad and check status.");

				Thread.Sleep(200);
				Progress_Changed(exportUnit, 81, "General Search Form window controls located successfully.");

				//entering ill number number
				Progress_Comment(exportUnit, "Entering ILL Number into General Search Form window controls...");

				if (SetTextFieldText(hWndILLNumberBox, exportUnit.Article.IllNumber.ToString()) == false)
					throw new IllException(ErrorCode.ILLiadCantEnterDataToGeneralSearchForm);
				//int result = SendMessage(hWndILLNumberBox, WM_SETTEXT, (int)0, article.Id.ToString());
				//if (result == 0)
				//	throw new IllException(ErrorCode.ILLiadCantEnterDataToGeneralSearchForm);
					
				Progress_Changed(exportUnit, 83, "ILL Number entered successfully.");
				Thread.Sleep(200);

				//pressing search button
				Progress_Comment(exportUnit, "Pressing Search button in General Search Form window...");
				int result = PostMessage(hWndSearchButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndSearchButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
				
				Progress_Changed(exportUnit, 85, "Search button pressed.");
				Thread.Sleep(1000);

				//processing General Update Form
				ProcessGeneralUpdateWindow(exportUnit);

				//closing this window
				Progress_Comment(exportUnit, "Closing General Search Form window...");
				CloseIllWindow(hWndGeneralSearch);

				//waiting window to be closed
				Progress_Comment(exportUnit, "Waiting for General Search Form window to close...");
				attempts = 0;

				while (FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TGeneralSearch", "General Searh Form - Lending") != IntPtr.Zero)
				{
					if (attempts++ > 10)
						throw new IllException(ErrorCode.ILLiadGeneralSearchNotClosing);

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, 95, "General Search Form window closed successfully.");
			}
			catch (IllException ex)
			{
				if (hWndGeneralSearch != IntPtr.Zero)
					SendMessage(hWndGeneralSearch, WM_CLOSE, 0, 0);

				throw ex;
			}
		}
		#endregion

		#region ProcessGeneralUpdateInfoWindow()
		protected override void ProcessGeneralUpdateInfoWindow(ExportUnit exportUnit)
		{
			IntPtr hWndInfo = IntPtr.Zero;
			IntPtr hWndYesButton;
			int attempts = 0;
			int result;

			try
			{
				//locating window
				Progress_Comment(exportUnit, "Waiting for Information window to be opened...");

				while ((hWndInfo = FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TMessageForm", "Information")) == IntPtr.Zero)
				{
					if (attempts > 20)
						throw new IllException(ErrorCode.ILLiadCantLocateGeneralUpdateInfoWnd);

					Thread.Sleep(1000);
					attempts++;
				}

				Progress_Changed(exportUnit, 90, "Info window located successfully.");

				Thread.Sleep(500);

				//locating Yes button
				Progress_Comment(exportUnit, "Locating Yes button...");

				hWndYesButton = FindWindowEx(hWndInfo, IntPtr.Zero, "TButton", "&Yes");
				if (hWndYesButton == IntPtr.Zero)
					throw new IllException(ErrorCode.ILLiadCantLocateInfoYesButton, "Can't locate Yes button in \"Information\" message form! Please open up ILLiad and check status.");

				Progress_Changed(exportUnit, 92, "Yes button located successfully.");

				//pressing Yes button
				Progress_Comment(exportUnit, "Pressing Yes button in Information message form...");
				result = PostMessage(hWndYesButton, WM_LBUTTONDOWN, 1, 65536 * 14 + 19);
				result += PostMessage(hWndYesButton, WM_LBUTTONUP, 0, 65536 * 14 + 19);
				Progress_Changed(exportUnit, -1, "Yes button pressed.");

				//waiting window to be closed
				Progress_Comment(exportUnit, "Waiting for Information form to close...");
				attempts = 0;

				while (FindWindowEx(IntPtr.Zero, IntPtr.Zero, @"TMessageForm", "Information") != IntPtr.Zero)
				{
					if (attempts++ > 30)
						throw new IllException(ErrorCode.ILLiadInfoWndNotClosing);

					Thread.Sleep(500);
				}

				Progress_Changed(exportUnit, 93, "Information form closed.");
				Thread.Sleep(1000);
			}
			catch (IllException ex)
			{
				if (hWndInfo != IntPtr.Zero)
					SendMessage(hWndInfo, WM_CLOSE, 0, 0);

				throw ex;
			}
		}
		#endregion

		#endregion
	}
}
