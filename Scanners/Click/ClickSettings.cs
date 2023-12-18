using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanners.Click
{
	[Serializable]
	public class ClickSettings
	{
		public Scanners.Click.Settings.FileFormatSetting		FileFormat	{ get; set; }
		public Scanners.Click.Settings.ColorModeSetting			ColorMode	{ get; set; }
		public Scanners.Click.Settings.BrightnessSetting		Brightness	{ get; set; }
		public Scanners.Click.Settings.ContrastSetting			Contrast	{ get; set; }
		public Scanners.Click.Settings.DpiSetting				Dpi			{ get; set; }
		public Scanners.Click.Settings.Click.ScanPageSetting	ScanPage	{ get; set; }
		public Scanners.Click.Settings.Click.ScanModeSetting	ScanMode	{ get; set; }
		public Scanners.Click.Settings.FocusSetting				Focus { get; set; }

		protected List<Scanners.Click.Settings.ISetting> settingsList ;
		protected bool settingsChanged = false;


		#region constructor
		public ClickSettings()
		{
			this.FileFormat = new Scanners.Click.Settings.FileFormatSetting();
			this.ColorMode = new Scanners.Click.Settings.ColorModeSetting();
			this.Brightness = new Scanners.Click.Settings.BrightnessSetting();
			this.Contrast = new Scanners.Click.Settings.ContrastSetting();
			this.Dpi = new Scanners.Click.Settings.DpiSetting();
			this.ScanPage = new Settings.Click.ScanPageSetting();
			this.ScanMode = new Settings.Click.ScanModeSetting();
			this.Focus = new Settings.FocusSetting();

			settingsList = new List<Settings.ISetting>() { this.FileFormat, this.ColorMode, this.Brightness, this.Contrast, this.Dpi, this.ScanPage, this.ScanMode, this.Focus };

			foreach (Scanners.Click.Settings.ISetting setting in settingsList)
				setting.Changed += delegate() { this.settingsChanged = true; };
		}

		public ClickSettings(ClickWrapper scanner)
		{
			this.FileFormat = new Scanners.Click.Settings.FileFormatSetting();
			this.ColorMode = new Scanners.Click.Settings.ColorModeSetting();
			this.Brightness = new Scanners.Click.Settings.BrightnessSetting();
			this.Contrast = new Scanners.Click.Settings.ContrastSetting();
			this.Dpi = new Scanners.Click.Settings.DpiSetting();
			this.ScanPage = new Settings.Click.ScanPageSetting();
			this.ScanMode = new Settings.Click.ScanModeSetting();
			this.Focus = new Settings.FocusSetting();

			settingsList = new List<Settings.ISetting>() { this.FileFormat, this.ColorMode, this.Brightness, this.Contrast, this.Dpi, this.ScanPage, this.ScanMode, this.Focus };

			/*foreach (Scanners.Click.Settings.ISetting setting in settingsList)
				setting.Load(scanner);*/
					
			foreach (Scanners.Click.Settings.ISetting setting in settingsList)
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
		public void CopyFrom(ClickSettings clickSettings)
		{
			this.FileFormat.CopyFrom(clickSettings.FileFormat);
			this.ColorMode.CopyFrom(clickSettings.ColorMode);
			this.Brightness.CopyFrom(clickSettings.Brightness);
			this.Contrast.CopyFrom(clickSettings.Contrast);
			this.Dpi.CopyFrom(clickSettings.Dpi);
			this.ScanPage.CopyFrom(clickSettings.ScanPage);
			this.ScanMode.CopyFrom(clickSettings.ScanMode);
			this.Focus.CopyFrom(clickSettings.Focus);
		}
		#endregion

		#region CopyValuesFrom()
		public void CopyValuesFrom(ClickSettings clickSettings)
		{
			//if (this.FileFormat.IsDefined) 
				this.FileFormat.Value = clickSettings.FileFormat.Value;
			//if (this.ColorMode.IsDefined) 
				this.ColorMode.Value = clickSettings.ColorMode.Value;
			//if (this.Brightness.IsDefined) 
				this.Brightness.Value = clickSettings.Brightness.Value;
			//if (this.Contrast.IsDefined) 
				this.Contrast.Value = clickSettings.Contrast.Value;
			//if (this.Dpi.IsDefined) 
				this.Dpi.Value = clickSettings.Dpi.Value;
			//if (this.DocSize.IsDefined) 
				this.ScanPage.Value = clickSettings.ScanPage.Value;

				this.ScanMode.Value = clickSettings.ScanMode.Value;
				this.Focus.Value = clickSettings.Focus.Value;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			string settings = "";

			foreach (Scanners.Click.Settings.ISetting setting in settingsList)
				if (setting.IsDefined)
					settings += setting.ToString();

			return settings;
		}
		#endregion

		#endregion
	
	}
}
