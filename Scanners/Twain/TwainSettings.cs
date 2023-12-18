using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.Twain
{
	[Serializable]
	public class TwainSettings
	{
		public Scanners.Twain.Settings.FileFormatSetting	FileFormat	{ get; set; }
		public Scanners.Twain.Settings.ColorModeSetting		ColorMode	{ get; set; }
		public Scanners.Twain.Settings.BrightnessSetting	Brightness	{ get; set; }
		public Scanners.Twain.Settings.ContrastSetting		Contrast	{ get; set; }
		public Scanners.Twain.Settings.DpiSetting			Dpi			{ get; set; }
		public Scanners.Twain.Settings.DocSizeSetting		DocSize		{ get; set; }
		public Scanners.Twain.Settings.TransferCountSetting TransferCount { get; set; }

		protected List<Scanners.Twain.Settings.ISetting> settingsList ;
		protected bool settingsChanged = false;


		#region constructor
		public TwainSettings()
		{
			this.FileFormat = new Scanners.Twain.Settings.FileFormatSetting();
			this.ColorMode = new Scanners.Twain.Settings.ColorModeSetting();
			this.Brightness = new Scanners.Twain.Settings.BrightnessSetting();
			this.Contrast = new Scanners.Twain.Settings.ContrastSetting();
			this.Dpi = new Scanners.Twain.Settings.DpiSetting();
			this.DocSize = new Scanners.Twain.Settings.DocSizeSetting();
			this.TransferCount = new Settings.TransferCountSetting();

			settingsList = new List<Settings.ISetting>() { this.FileFormat, this.ColorMode, this.Brightness, this.Contrast, this.Dpi, this.DocSize, this.TransferCount };

			foreach (Scanners.Twain.Settings.ISetting setting in settingsList)
				setting.Changed += delegate() { this.settingsChanged = true; };
		}

		public TwainSettings(TwainApp.TwainScanner scanner)
		{
			this.FileFormat = new Scanners.Twain.Settings.FileFormatSetting();
			this.ColorMode = new Scanners.Twain.Settings.ColorModeSetting();
			this.Brightness = new Scanners.Twain.Settings.BrightnessSetting();
			this.Contrast = new Scanners.Twain.Settings.ContrastSetting();
			this.Dpi = new Scanners.Twain.Settings.DpiSetting();
			this.DocSize = new Scanners.Twain.Settings.DocSizeSetting();
			this.TransferCount = new Settings.TransferCountSetting();

			settingsList = new List<Settings.ISetting>() { this.FileFormat, this.ColorMode, this.Brightness, this.Contrast, this.Dpi, this.DocSize, this.TransferCount };

			foreach (Scanners.Twain.Settings.ISetting setting in settingsList)
				setting.Load(scanner);
					
			foreach (Scanners.Twain.Settings.ISetting setting in settingsList)
				setting.Changed += delegate() { this.settingsChanged = true; };
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region SettingsChanged
		public bool SettingsChanged
		{
			get { return this.settingsChanged; }
			set { this.settingsChanged = value; }
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(TwainSettings twainSettings)
		{
			this.FileFormat.CopyFrom(twainSettings.FileFormat);
			this.ColorMode.CopyFrom(twainSettings.ColorMode);
			this.Brightness.CopyFrom(twainSettings.Brightness);
			this.Contrast.CopyFrom(twainSettings.Contrast);
			this.Dpi.CopyFrom(twainSettings.Dpi);
			this.DocSize.CopyFrom(twainSettings.DocSize);
			this.TransferCount.CopyFrom(twainSettings.TransferCount);
		}
		#endregion

		#region CopyValuesFrom()
		public void CopyValuesFrom(TwainSettings twainSettings)
		{
			//if (this.FileFormat.IsDefined) 
				this.FileFormat.Value = twainSettings.FileFormat.Value;
			//if (this.ColorMode.IsDefined) 
				this.ColorMode.Value = twainSettings.ColorMode.Value;
			//if (this.Brightness.IsDefined) 
				this.Brightness.Value = twainSettings.Brightness.Value;
			//if (this.Contrast.IsDefined) 
				this.Contrast.Value = twainSettings.Contrast.Value;
			//if (this.Dpi.IsDefined) 
				this.Dpi.Value = twainSettings.Dpi.Value;
			//if (this.DocSize.IsDefined) 
				this.DocSize.Value = twainSettings.DocSize.Value;

				this.TransferCount.Value = twainSettings.TransferCount.Value;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			string settings = "";

			foreach (Scanners.Twain.Settings.ISetting setting in settingsList)
				if (setting.IsDefined)
					settings += setting.ToString();

			return settings;
		}
		#endregion

		#endregion

	}
}
