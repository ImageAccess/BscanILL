using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace BscanILL.Export.Printing
{
	public class PrinterProfiles : ObservableCollection<BscanILL.Export.Printing.PrinterProfile>
	{
		
		public PrinterProfiles()
		{
		}

		public void Validate()
		{
			for (int i = this.Count - 1; i >= 0; i--)
				if (this[i].IsConnected == false)
					this.RemoveAt(i);
		}

		public void CopyFrom(PrinterProfiles source)
		{
			this.Clear();

			foreach (PrinterProfile p in source)
			{
				PrinterProfile profile = new PrinterProfile(p.Description, p.PrinterName, p.PaperSize, p.PaperTray, p.Functionality);
				this.Add(profile);
			}
		}

	}
}
