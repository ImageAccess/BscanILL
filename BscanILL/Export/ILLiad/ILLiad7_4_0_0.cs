using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace BscanILL.Export.ILLiad
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ILLiad7_4_0_0 : ILLiad7_3_0_0, IILLiad
	{
		#region constructor
		internal ILLiad7_4_0_0()
			: base()
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

		#region SelectUpdateStacksFromMenu()
		protected override void SelectUpdateStacksFromMenu(ExportUnit exportUnit, IntPtr hWnd)
		{
			Progress_Comment(exportUnit, "Selecting Update Stacks Search Results from menu...");

			WindowToTop(hWnd);

			keybd_event(0x79, 0, 0, 0); //Alt key down
			keybd_event(0x79, 0, KEYEVENTF_KEYUP, 0); //Alt Key up

			Thread.Sleep(500);

			for (int i = 0; i < 4; i++)
			{
				keybd_event(0x27, 0, 0, 0); //Right arrow key down
				keybd_event(0x27, 0, KEYEVENTF_KEYUP, 0); //Right arrow Key up

				Thread.Sleep(200);
			}

			for (int i = 0; i < 2 + _settings.Export.ILLiad.UpdateExtraMenuItems; i++)
			{
				keybd_event(0x28, 0, 0, 0); //Down arrow key down
				keybd_event(0x28, 0, KEYEVENTF_KEYUP, 0); //Down arrow Key up

				Thread.Sleep(200);
			}

			keybd_event(0xD, 0, 0, 0); //Enter key down
			keybd_event(0xD, 0, KEYEVENTF_KEYUP, 0); //Enter Key up
			Thread.Sleep(500);

			Progress_Changed(exportUnit, 54, "Menu item selected successfully.");
		}
		#endregion

		#endregion
	}
}
