using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.Twain
{
	[Serializable]
	public class BookedgeSettings : Scanners.Twain.TwainSettings
	{
		public Scanners.Twain.Settings.Bookedge.ScanPageSetting ScanPage { get; set; }


		#region constructor
		public BookedgeSettings()
		{
			this.ScanPage = new Scanners.Twain.Settings.Bookedge.ScanPageSetting();
		}

		public BookedgeSettings(TwainApp.TwainScanner scanner)
			: base(scanner)
		{
			this.ScanPage = new Scanners.Twain.Settings.Bookedge.ScanPageSetting();

			this.settingsList.Add(this.ScanPage);

			this.ScanPage.Load(scanner);
			this.ScanPage.Changed += delegate() { this.settingsChanged = true; };
		}
		#endregion


		#region CopyFrom()
		public void CopyFrom(BookedgeSettings settings)
		{
			base.CopyFrom(settings);
			
			this.ScanPage.CopyFrom(settings.ScanPage);
		}
		#endregion

		#region CopyValuesFrom()
		public void CopyValuesFrom(BookedgeSettings settings)
		{
			base.CopyValuesFrom(settings);

			//if (this.ScanPage.IsDefined) 
				this.ScanPage.Value = settings.ScanPage.Value;
		}
		#endregion

	}
}
