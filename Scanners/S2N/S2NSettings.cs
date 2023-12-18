using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N
{
	[Serializable]
	public class S2NSettings
	{
		public Scanners.S2N.Settings.AutofocusSetting		Autofocus { get; set; }
		public Scanners.S2N.Settings.RangeSetting			Gamma { get; set; }
		public Scanners.S2N.Settings.SpeedSetting			Speed { get; set; }
		public Scanners.S2N.Settings.ScanModeSetting		ScanMode { get; set; }
		public Scanners.S2N.Settings.DocumentModeSetting	DocMode { get; set; }
		public Scanners.S2N.Settings.ColorModeSetting		ColorMode { get; set; }
		public Scanners.S2N.Settings.RangeSetting			Brightness { get; set; }
		public Scanners.S2N.Settings.RangeSetting			Contrast { get; set; }
		public Scanners.S2N.Settings.RangeSetting			Dpi { get; set; }
		public Scanners.S2N.Settings.DpiModeSetting			DpiMode { get; set; }
		public Scanners.S2N.Settings.LightSetting			Light { get; set; }
		public Scanners.S2N.Settings.RotationSetting		Rotation { get; set; }
		public Scanners.S2N.Settings.DespeckleSetting		Despeckle { get; set; }
		public Scanners.S2N.Settings.RangeSetting			Sharpening { get; set; }
		public Scanners.S2N.Settings.RangeSetting			UserX { get; set; }
		public Scanners.S2N.Settings.RangeSetting			UserY { get; set; }
		public Scanners.S2N.Settings.RangeSetting			UserW { get; set; }
		public Scanners.S2N.Settings.RangeSetting			UserH { get; set; }
		public Scanners.S2N.Settings.UserUnitSetting		UserUnit { get; set; }
		public Scanners.S2N.Settings.DocumentSizeSetting	DocSize { get; set; }
		public Scanners.S2N.Settings.SplittingSetting		Splitting { get; set; }
        public Scanners.S2N.Settings.SplittingStartPageSetting       Splitting_StartPage { get; set; }
		public Scanners.S2N.Settings.RangeSetting			JpegQuality { get; set; }
		public Scanners.S2N.Settings.TiffCompressionSetting TiffCompression { get; set; }
		public Scanners.S2N.Settings.FileFormatSetting		FileFormat { get; set; }
		public Scanners.S2N.Settings.BitonalThresholdSetting BitonalThreshold { get; set; }
		public Scanners.S2N.Settings.OnOffSetting			Bidirectional { get; set; }

		List<Scanners.S2N.Settings.ISetting> settingsList ;

		bool settingsChanged = false;


		#region constructor
		public S2NSettings()
		{
			this.Autofocus = new Scanners.S2N.Settings.AutofocusSetting();
			this.Gamma = new Scanners.S2N.Settings.RangeSetting(10, 25, 10);
			this.Speed = new Scanners.S2N.Settings.SpeedSetting();
			this.ScanMode = new Scanners.S2N.Settings.ScanModeSetting();
			this.DocMode = new Scanners.S2N.Settings.DocumentModeSetting();
			this.ColorMode = new Scanners.S2N.Settings.ColorModeSetting();
			this.Brightness = new Scanners.S2N.Settings.RangeSetting(0, 255, 127);
			this.Contrast = new Scanners.S2N.Settings.RangeSetting(0, 255, 127);
			this.Dpi = new Scanners.S2N.Settings.RangeSetting(150, 300, 200);
			this.DpiMode = new Scanners.S2N.Settings.DpiModeSetting();
			this.Light = new Scanners.S2N.Settings.LightSetting();
			this.Rotation = new Scanners.S2N.Settings.RotationSetting();
			this.Despeckle = new Scanners.S2N.Settings.DespeckleSetting();
			this.Sharpening = new Scanners.S2N.Settings.RangeSetting(-7, 7, 0);
			this.UserX = new Scanners.S2N.Settings.RangeSetting(-12007, 12007, 0);
			this.UserY = new Scanners.S2N.Settings.RangeSetting(-787, 17716, 0);
			this.UserW = new Scanners.S2N.Settings.RangeSetting(0, 24014, 0);
			this.UserH = new Scanners.S2N.Settings.RangeSetting(0, 18503, 0);
			this.UserUnit = new Scanners.S2N.Settings.UserUnitSetting();
			this.DocSize = new Scanners.S2N.Settings.DocumentSizeSetting();
			this.Splitting = new Scanners.S2N.Settings.SplittingSetting();
            this.Splitting_StartPage = new Scanners.S2N.Settings.SplittingStartPageSetting();
			this.JpegQuality = new Scanners.S2N.Settings.RangeSetting(25, 100, 85);
			this.TiffCompression = new Scanners.S2N.Settings.TiffCompressionSetting();
			this.FileFormat = new Scanners.S2N.Settings.FileFormatSetting();
			this.BitonalThreshold = new Scanners.S2N.Settings.BitonalThresholdSetting();
			this.Bidirectional = new Settings.OnOffSetting(true);

			this.settingsList = new List<Scanners.S2N.Settings.ISetting>() {this.Autofocus, this.Gamma, this.Speed, this.ScanMode, 
				this.DocMode, this.ColorMode, this.Brightness, this.Contrast, this.Dpi, this.DpiMode, this.Rotation,
				this.Despeckle, this.Light, this.Sharpening, this.UserX, this.UserY, this.UserW, this.UserH, this.UserUnit, 
				this.DocSize, this.Splitting, this.Splitting_StartPage, this.JpegQuality, this.TiffCompression, this.FileFormat, this.BitonalThreshold, this.Bidirectional};
		}

		public S2NSettings(string settings)
			: this()
		{
			string[] settingsArray = settings.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
	
			foreach (string setting in settingsArray)
			{
				if (setting.StartsWith("autofocus:"))
					this.Autofocus.Load(setting);
				else if (setting.StartsWith("gamma:"))
					this.Gamma.Load(setting);
				else if (setting.StartsWith("speed:"))
					this.Speed.Load(setting);
				else if (setting.StartsWith("scanmode:"))
					this.ScanMode.Load(setting);
				else if (setting.StartsWith("docmode:"))
					this.DocMode.Load(setting);
				else if (setting.StartsWith("colormode:"))
					this.ColorMode.Load(setting);
				else if (setting.StartsWith("bright:"))
					this.Brightness.Load(setting);
				else if (setting.StartsWith("contr:"))
					this.Contrast.Load(setting);
				else if (setting.StartsWith("dpi:"))
					this.Dpi.Load(setting);
				else if (setting.StartsWith("dpi_mode:"))
					this.DpiMode.Load(setting);
				else if (setting.StartsWith("rotation:"))
					this.Rotation.Load(setting);
				else if (setting.StartsWith("despeckle:"))
					this.Despeckle.Load(setting);
				else if (setting.StartsWith("light:"))
					this.Light.Load(setting);
				else if (setting.StartsWith("sharpen:"))
					this.Sharpening.Load(setting);
				else if (setting.StartsWith("user_x:"))
					this.UserX.Load(setting);
				else if (setting.StartsWith("user_y:"))
					this.UserY.Load(setting);
				else if (setting.StartsWith("user_w:"))
					this.UserW.Load(setting);
				else if (setting.StartsWith("user_h:"))
					this.UserH.Load(setting);
				else if (setting.StartsWith("user_unit:"))
					this.UserUnit.Load(setting);
				else if (setting.StartsWith("docsize:"))
					this.DocSize.Load(setting);
                else if (setting.StartsWith("splitting_startpage:"))
                    this.Splitting_StartPage.Load(setting);
				else if (setting.StartsWith("splitting:"))
					this.Splitting.Load(setting);
				else if (setting.StartsWith("jpeg_quality:"))
					this.JpegQuality.Load(setting);
				else if (setting.StartsWith("tiff_compr:"))
					this.TiffCompression.Load(setting);
				else if (setting.StartsWith("fileformat:"))
					this.FileFormat.Load(setting);
				else if (setting.StartsWith("dyn_bright:"))
					this.BitonalThreshold.Load(setting);
				else if (setting.StartsWith("bidirectional:"))
					this.Bidirectional.Load(setting);
			}
	
			//SetFromSettings();
	
			foreach (Scanners.S2N.Settings.ISetting setting in settingsList)
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
		public void CopyFrom(Scanners.S2N.S2NSettings source)
		{
			this.Gamma.CopyFrom(source.Gamma);
			this.Speed.CopyFrom(source.Speed);
			this.ScanMode.CopyFrom(source.ScanMode);
			this.DocMode.CopyFrom(source.DocMode);

			/*if (this.ColorMode.IsDefined)
			{
				if (this.ColorMode.IsColorOptionInstalled == false && (source.ColorMode.Value == Scanners.S2N.ColorMode.Color))
					this.ColorMode.Value = Scanners.S2N.ColorMode.Grayscale;
				else
					this.ColorMode.Value = source.ColorMode.Value;
			}*/
			this.ColorMode.CopyFrom(source.ColorMode);
			
			this.Brightness.CopyFrom(source.Brightness);
			this.Contrast.CopyFrom(source.Contrast);
			this.Dpi.CopyFrom(source.Dpi);
			this.DpiMode.CopyFrom(source.DpiMode);
			this.Rotation.CopyFrom(source.Rotation);
			this.Despeckle.CopyFrom(source.Despeckle);
			this.Sharpening.CopyFrom(source.Sharpening);

			this.UserX.CopyFrom(source.UserX);
			this.UserY.CopyFrom(source.UserY);
			this.UserW.CopyFrom(source.UserW);
			this.UserH.CopyFrom(source.UserH);

			this.UserUnit.CopyFrom(source.UserUnit);
			this.DocSize.CopyFrom(source.DocSize);
			this.Splitting.CopyFrom(source.Splitting);
            this.Splitting_StartPage.CopyFrom(source.Splitting_StartPage);

			this.JpegQuality.CopyFrom(source.JpegQuality);
			this.TiffCompression.CopyFrom(source.TiffCompression);
			this.FileFormat.CopyFrom(source.FileFormat);
			this.BitonalThreshold.CopyFrom(source.BitonalThreshold);
			this.Bidirectional.CopyFrom(source.Bidirectional);
		}
		#endregion

        #region CopyValuesFrom()
        public void CopyValuesFrom(Scanners.S2N.S2NSettings source)
		{
			this.Gamma.Value = source.Gamma.Value;
			this.Speed.Value = source.Speed.Value;
			this.ScanMode.Value = source.ScanMode.Value;
			this.DocMode.Value = source.DocMode.Value;

			/*if (this.ColorMode.IsDefined.Value
			{
				if (this.ColorMode.IsColorOptionInstalled == false && (source.ColorMode.Value == Scanners.S2N.ColorMode.Color.Value.Value
					this.ColorMode.Value = Scanners.S2N.ColorMode.Grayscale;
				else
					this.ColorMode.Value = source.ColorMode.Value;
			}*/
			this.ColorMode.Value = source.ColorMode.Value;

			this.Brightness.Value = source.Brightness.Value;
			this.Contrast.Value = source.Contrast.Value;
			this.Dpi.Value = source.Dpi.Value;
			this.DpiMode.Value = source.DpiMode.Value;
			this.Rotation.Value = source.Rotation.Value;
			this.Despeckle.Value = source.Despeckle.Value;
			this.Sharpening.Value = source.Sharpening.Value;

			this.UserX.Value = source.UserX.Value;
			this.UserY.Value = source.UserY.Value;
			this.UserW.Value = source.UserW.Value;
			this.UserH.Value = source.UserH.Value;

			this.UserUnit.Value = source.UserUnit.Value;
			this.DocSize.Value = source.DocSize.Value;
			this.Splitting.Value = source.Splitting.Value;
            this.Splitting_StartPage.Value = source.Splitting_StartPage.Value;

			this.JpegQuality.Value = source.JpegQuality.Value;
			this.TiffCompression.Value = source.TiffCompression.Value;
			this.FileFormat.Value = source.FileFormat.Value;
			this.BitonalThreshold.Value = source.BitonalThreshold.Value;
			this.Bidirectional.Value = source.Bidirectional.Value;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			string settings = "";

			foreach (Scanners.S2N.Settings.ISetting setting in settingsList)
				if (setting.IsDefined)
					settings += setting.ToString();

			return settings;
		}
		#endregion

		#endregion

	}
}
