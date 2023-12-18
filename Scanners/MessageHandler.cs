using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Scanners
{
	public class MessageHandler : NativeWindow
	{
		public delegate void MessageReceivedHnd(ref Message msg);
		public event MessageReceivedHnd MessageReceived;

		public MessageHandler()
		{
			CreateHandle(new CreateParams());
		}

		protected override void WndProc(ref Message msg)
		{
			if (MessageReceived != null)
				MessageReceived(ref msg);

			base.WndProc(ref msg);
		}
	}
}
