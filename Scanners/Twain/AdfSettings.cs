using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.Twain
{
	[Serializable]
	public class AdfSettings : Scanners.Twain.TwainSettings
	{
		public Scanners.Twain.Settings.Adf.DuplexSetting Duplex { get; set; }


		#region constructor
		public AdfSettings()
		{
			this.Duplex = new Scanners.Twain.Settings.Adf.DuplexSetting();
			
			this.settingsList.Add(this.Duplex);

			this.Duplex.Changed += delegate() { this.settingsChanged = true; };
		}

		public AdfSettings(TwainApp.TwainScanner scanner)
			: base(scanner)
		{
			this.Duplex = new Scanners.Twain.Settings.Adf.DuplexSetting();

			this.settingsList.Add(this.Duplex);

			this.Duplex.Load(scanner);
			this.Duplex.Changed += delegate() { this.settingsChanged = true; };
		}
		#endregion


		#region CopyFrom()
		public void CopyFrom(AdfSettings settings)
		{
			base.CopyFrom(settings);

			if (this.Duplex.IsDefined) this.Duplex.CopyFrom(settings.Duplex);
		}
		#endregion

		#region CopyValuesFrom()
		public void CopyValuesFrom(AdfSettings settings)
		{
			base.CopyValuesFrom(settings);

			this.Duplex.Value = settings.Duplex.Value;
		}
		#endregion

	}
}
