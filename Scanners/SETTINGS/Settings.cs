using System;
using System.Collections;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using Scanners.SETTINGS;



namespace Scanners
{
	[XmlRoot("ScannerSettings", Namespace = "Scanners.Settings")]
	[Serializable]
	public class Settings : BaseClass
	{
		static Scanners.Settings instance = null;

		GeneralClass					general ;
		S2NScannerClass					s2nScanner ;
		FolderScannerClass				folderScanner ;
		ClickScannerClass				clickScanner;
		ClickMiniClass					clickMini;
		TwainScannerClass				twainScanner;
		AdfAddOnScannerClass			adfAddOnScanner ;


		#region constructor
		public Settings()
		{
			this.general = new GeneralClass();
			this.s2nScanner = new S2NScannerClass();
			this.folderScanner = new FolderScannerClass();
			this.clickScanner = new ClickScannerClass();// ClickDLL.Settings.Settings.Instance;
			this.clickMini = new ClickMiniClass();
			this.twainScanner = new TwainScannerClass();
			this.adfAddOnScanner = new AdfAddOnScannerClass();

			//this.general.PropertyChanged += new PropertyChangedEventHandler(RaisePropertyChanged);

			//Scanners.Settings.instance = this;
		}
		#endregion


		#region class GeneralClass
		[Serializable]
		public class GeneralClass : BaseClass
		{
			Scanners.SettingsScannerType	scannerType = Scanners.SettingsScannerType.S2N;
			int								saveModeTimeoutInMins = 30;
			string							tempImagesDir;


			public GeneralClass()
			{
				tempImagesDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\Shared\TempImages";

				new DirectoryInfo(tempImagesDir).Create();
			}

			public Scanners.SettingsScannerType ScannerType { get { return scannerType; } set { scannerType = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public int					SaveModeTimeoutInMins { get { return saveModeTimeoutInMins; } set { saveModeTimeoutInMins = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public string				TempImagesDir { get { return tempImagesDir; } set { tempImagesDir = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }


			#region CopyFrom()
			public void CopyFrom(Settings source)
			{
				this.ScannerType = source.General.ScannerType;
				this.SaveModeTimeoutInMins = source.General.SaveModeTimeoutInMins;
				this.TempImagesDir = source.General.TempImagesDir;
			}
			#endregion
		}
		#endregion

		#region class S2NScannerClass
		[Serializable]
		public class S2NScannerClass : BaseClass
		{
			string	ip = "192.168.1.50";
			bool	footPedal = true;
			bool	wakeOnLAN = false;
			string	wakeOnLanMacAddress = "";

			public S2NScannerClass()
			{
			}

			public string	Ip { get { return ip; } set { ip = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public bool		FootPedal { get { return footPedal; } set { footPedal = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public bool		WakeOnLAN { get { return wakeOnLAN; } set { wakeOnLAN = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public string	WakeOnLanMacAddress { get { return wakeOnLanMacAddress; } set { wakeOnLanMacAddress = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

			#region CopyFrom()
			public void CopyFrom(Settings source)
			{
				this.Ip = source.S2NScanner.Ip;
				this.FootPedal = source.S2NScanner.FootPedal;
				this.WakeOnLAN = source.S2NScanner.WakeOnLAN;
				this.WakeOnLanMacAddress = source.S2NScanner.WakeOnLanMacAddress;
			}
			#endregion
		}
		#endregion

		#region class FolderScannerClass
		[Serializable]
		public class FolderScannerClass : BaseClass
		{
			string path = @"C:\BscanILL\FolderScanner";

			public FolderScannerClass()
			{
			}

			public string Path { get { return path; } set { path = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

			#region CopyFrom()
			public void CopyFrom(Settings source)
			{
				this.Path = source.FolderScanner.Path;
			}
			#endregion
		}
		#endregion

		#region class TwainScannerClass
		[Serializable]
		public class TwainScannerClass : BaseClass
		{
			bool isEnergyStarOn = true;
			int energyStarTimeout = 30;
			double brightnessDelta = 0;
			double contrastDelta = 0;

			public TwainScannerClass()
			{
			}

			public bool IsEnergyStarOn { get { return isEnergyStarOn; } set { isEnergyStarOn = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public int EnergyStarTimeout { get { return energyStarTimeout; } set { energyStarTimeout = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			/// <summary>
			/// <-1,1>
			/// </summary>
			public double BrightnessDelta { get { return brightnessDelta; } set { brightnessDelta = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			/// <summary>
			/// <-1,1>
			/// </summary>
			public double ContrastDelta { get { return contrastDelta; } set { contrastDelta = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

			#region CopyFrom()
			public void CopyFrom(Settings source)
			{
				this.IsEnergyStarOn = source.TwainScanner.IsEnergyStarOn;
				this.EnergyStarTimeout = source.TwainScanner.EnergyStarTimeout;
				this.BrightnessDelta = source.TwainScanner.BrightnessDelta;
				this.ContrastDelta = source.TwainScanner.ContrastDelta;
			}
			#endregion
		}
		#endregion

		#region class AdfAddOnScannerClass
		[Serializable]
		public class AdfAddOnScannerClass : BaseClass
		{
			bool isEnabled = false;
			Scanners.SettingsScannerType scannerType = Scanners.SettingsScannerType.KodakI1120;
			Scanners.ScanMode defaultScanMode = Scanners.ScanMode.AdfDuplexMulti;
			bool isEnergyStarOn = true;
			int energyStarTimeout = 30;
			double brightnessDelta = 0;
			double contrastDelta = 0;


			#region constructor
			public AdfAddOnScannerClass()
			{
			}
			#endregion


			public bool				IsEnabled { get { return this.isEnabled; } set { isEnabled = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public Scanners.SettingsScannerType ScannerType { get { return scannerType; } set { scannerType = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public Scanners.ScanMode	DefaultScanMode { get { return defaultScanMode; } set { defaultScanMode = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public bool				IsEnergyStarOn { get { return isEnergyStarOn; } set { isEnergyStarOn = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
			public int				EnergyStarTimeout { get { return this.energyStarTimeout; } set { energyStarTimeout = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

			#region BrightnessDelta
			/// <summary>
			/// <-1,1>
			/// </summary>
			public double BrightnessDelta
			{
				get { return this.brightnessDelta; }
				set { brightnessDelta = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
			}
			#endregion

			#region ContrastDelta
			/// <summary>
			/// <-1,1>
			/// </summary>
			public double ContrastDelta
			{
				get { return this.contrastDelta; }
				set { contrastDelta = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); }
			}
			#endregion

			#region CopyFrom()
			public void CopyFrom(Settings source)
			{
				this.IsEnabled = source.AdfAddOnScanner.IsEnabled;
				this.ScannerType = source.AdfAddOnScanner.ScannerType;
				this.DefaultScanMode = source.AdfAddOnScanner.DefaultScanMode;
				this.IsEnergyStarOn = source.AdfAddOnScanner.IsEnergyStarOn;
				this.EnergyStarTimeout = source.AdfAddOnScanner.EnergyStarTimeout;
				this.BrightnessDelta = source.AdfAddOnScanner.BrightnessDelta;
				this.ContrastDelta = source.AdfAddOnScanner.ContrastDelta;
			}
			#endregion
		}
		#endregion

		#region class ClickScannerClass
		[Serializable]
		public class ClickScannerClass : BaseClass
		{
			bool catchScanButtonEvents = true;

			public ClickScannerClass()
			{
				this.Settings = ClickCommon.Settings.SettingsClick.Instance;
			}

			public ClickCommon.Settings.SettingsClick Settings
			{
				get { return ClickCommon.Settings.SettingsClick.Instance; }
				set
				{
					//ClickDLL.Settings.SettingsClick.Instance = value;
          ClickCommon.Settings.SettingsClick.Instance.CopyFrom(value);
          //ClickCommon.Settings.SettingsClick.Instance.Save();
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}

			[XmlIgnore]
			public string ComPort { get { return this.Settings.General.ComPort; } set { this.Settings.General.ComPort = value; } }

			/// <summary>
			/// turn to false if magic box gets crazy and the button is always on
			/// </summary>
			public bool CatchScanButtonEvents { get { return this.catchScanButtonEvents; } set { this.catchScanButtonEvents = value; } }


			#region CopyFrom()
			public void CopyFrom(Settings source)
			{
				this.CatchScanButtonEvents = source.ClickScanner.CatchScanButtonEvents;
				this.ComPort = source.ClickScanner.ComPort;
				this.Settings = source.ClickScanner.Settings;
			}
			#endregion
		}
		#endregion

		#region class ClickMiniClass
		[Serializable]
		public class ClickMiniClass : BaseClass
		{
			bool catchScanButtonEvents = true;


			public ClickMiniClass()
			{
				this.Settings = ClickCommon.Settings.SettingsMini.Instance;
			}

			public ClickCommon.Settings.SettingsMini Settings
			{
				get { return ClickCommon.Settings.SettingsMini.Instance; }
				set
				{
					//ClickDLL.Settings.SettingsMini.Instance = value;
          ClickCommon.Settings.SettingsMini.Instance.CopyFrom(value);
          //ClickCommon.Settings.SettingsMini.Instance.Save();
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}

			[XmlIgnore]
			public string ComPort { get { return this.Settings.General.ComPort; } set { this.Settings.General.ComPort = value; } }

			/// <summary>
			/// turn to false if magic box gets crazy and the button is always on
			/// </summary>
			public bool CatchScanButtonEvents { get { return this.catchScanButtonEvents; } set { this.catchScanButtonEvents = value; } }


			#region CopyFrom()
			public void CopyFrom(Settings source)
			{
				this.CatchScanButtonEvents = source.ClickMini.CatchScanButtonEvents;
				this.ComPort = source.ClickMini.ComPort;
				this.Settings = source.ClickMini.Settings;
			}
			#endregion

			#region Save()
			public void Save()
            {
				ClickCommon.Settings.SettingsMini.Instance.Save();
			}
			#endregion
		}
        #endregion


        //PUBLIC PROPERTIES
        #region public properties

        public static Scanners.Settings Instance
		{ 
			get { return Settings.instance ?? (Settings.instance = new Scanners.Settings()); } 
			set { Settings.instance = value; }
		}

		public GeneralClass					General { get { return general; } set { general = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
		public S2NScannerClass				S2NScanner { get { return s2nScanner; } set { s2nScanner = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
		public FolderScannerClass			FolderScanner { get { return folderScanner; } set { folderScanner = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
		public ClickScannerClass			ClickScanner { get { return clickScanner; } set { if (clickScanner != value) { clickScanner = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } } }
		public ClickMiniClass				ClickMini { get { return clickMini; } set { if (clickMini != value) { clickMini = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } } }
		public TwainScannerClass			TwainScanner { get { return twainScanner; } set { twainScanner = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }
		public AdfAddOnScannerClass			AdfAddOnScanner { get { return adfAddOnScanner; } set { adfAddOnScanner = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(Scanners.Settings source)
		{
			this.General.CopyFrom(source);
			this.S2NScanner.CopyFrom(source);
			this.FolderScanner.CopyFrom(source);
			this.ClickScanner.CopyFrom(source);
			this.ClickMini.CopyFrom(source);
			this.TwainScanner.CopyFrom(source);
			this.AdfAddOnScanner.CopyFrom(source);
		}
		#endregion

		#endregion

	}

}
