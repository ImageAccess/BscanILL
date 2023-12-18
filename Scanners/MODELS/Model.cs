using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.MODELS
{
	public class Model
	{
		ScanerModel			scannerModel;
		ScannerGroup?		scannerGroup = null;
		ScannerSubGroup?	scannerSubGroup = null;


		public Model(ScanerModel scannerModel)
		{
			this.scannerModel = scannerModel;
		}


		public Model(SettingsScannerType scannerType)
		{
			switch (scannerType)
			{
				case SettingsScannerType.iVinaFB6080E: this.scannerModel = MODELS.ScanerModel.iVinaFB6080E; break;
				case SettingsScannerType.iVinaFB6280E: this.scannerModel = MODELS.ScanerModel.iVinaFB6280E; break;
				case SettingsScannerType.KodakI1120: this.scannerModel = MODELS.ScanerModel.KodakI1120; break;
				case SettingsScannerType.KodakI1150: this.scannerModel = MODELS.ScanerModel.KodakI1150; break;
				case SettingsScannerType.KodakI1150New: this.scannerModel = MODELS.ScanerModel.KodakI1150New; break;
				case SettingsScannerType.KodakI1405: this.scannerModel = MODELS.ScanerModel.KodakI1405; break;
				case SettingsScannerType.KodakE1035: this.scannerModel = MODELS.ScanerModel.KodakE1035; break;
				case SettingsScannerType.KodakE1040: this.scannerModel = MODELS.ScanerModel.KodakE1040; break;
				case SettingsScannerType.Click: this.scannerModel = MODELS.ScanerModel.ClickV1; break;
				case SettingsScannerType.ClickMini: this.scannerModel = MODELS.ScanerModel.ClickMiniV1; break;
				default: throw new Exception("Can't determine scanner model from settings!");
			}
		}

		// PUBLIC PROPERTIES
		#region public properties

		public ScanerModel ScanerModel { get { return scannerModel; } }

		public ScannerGroup ScannerGroup
		{
			get
			{
				if (this.scannerGroup == null)
				{
					if ((int)scannerModel < 1024)
						this.scannerGroup = MODELS.ScannerGroup.S2N;
					else if ((int)scannerModel < 2048)
						this.scannerGroup = MODELS.ScannerGroup.Click;
					else
						this.scannerGroup = MODELS.ScannerGroup.Twain;
				}

				return this.scannerGroup.Value;
			}
		}

		public ScannerSubGroup ScannerSubGroup
		{
			get
			{
				if (this.scannerSubGroup == null)
				{
					if (scannerModel.ToString().StartsWith("BE2"))
					{
						if (scannerModel.ToString().Contains("_N3"))
							this.scannerSubGroup = MODELS.ScannerSubGroup.BE2_N3;
						else
							this.scannerSubGroup = MODELS.ScannerSubGroup.BE2_N2;
					}
					else if (scannerModel.ToString().StartsWith("BE3"))
						this.scannerSubGroup = MODELS.ScannerSubGroup.BE3;
					else if (scannerModel.ToString().StartsWith("BE4"))
						this.scannerSubGroup = MODELS.ScannerSubGroup.BE4;
                    else if (scannerModel.ToString().StartsWith("BE5"))
                        this.scannerSubGroup = MODELS.ScannerSubGroup.BE5;
					else if (scannerModel.ToString().StartsWith("FBA2_") || scannerModel.ToString().StartsWith("FBA2_") || scannerModel.ToString().StartsWith("WTA3_") || scannerModel.ToString().StartsWith("WT25_"))
						this.scannerSubGroup = MODELS.ScannerSubGroup.WideTekFlat;
					else if (scannerModel == MODELS.ScanerModel.WT36_200 || scannerModel == MODELS.ScanerModel.WT42_200 || scannerModel == MODELS.ScanerModel.WT48)
						this.scannerSubGroup = MODELS.ScannerSubGroup.WideTekThru;
					else if (scannerModel == MODELS.ScanerModel.ClickV1)
						this.scannerSubGroup = MODELS.ScannerSubGroup.Click;
					else if (scannerModel == MODELS.ScanerModel.ClickMiniV1)
						this.scannerSubGroup = MODELS.ScannerSubGroup.ClickMini;
					else if ((int)scannerModel >= 2048 && (int)scannerModel < 3072)
						this.scannerSubGroup = MODELS.ScannerSubGroup.TwainAdf;
					else if ((int)scannerModel >= 3072 && (int)scannerModel < 4096)
						this.scannerSubGroup = MODELS.ScannerSubGroup.BookEdge;
					else if ((int)scannerModel >= 4096 && (int)scannerModel < 5120)
						this.scannerSubGroup = MODELS.ScannerSubGroup.BookEdge;
					else
						throw new Exception("Unexpected scanner submodel!");
				}

				return this.scannerSubGroup.Value;
			}
		}

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region IsTheSameSettingsType()
		public bool IsTheSameSettingsType(Scanners.SettingsScannerType settingsScannerType)
		{
			switch (settingsScannerType)
			{
				case SettingsScannerType.Click: return (this.ScannerSubGroup == MODELS.ScannerSubGroup.Click);
				case SettingsScannerType.ClickMini: return (this.ScannerSubGroup == MODELS.ScannerSubGroup.ClickMini);
				case SettingsScannerType.FolderScanner: return false;
				case SettingsScannerType.iVinaFB6080E: return this.scannerModel == MODELS.ScanerModel.iVinaFB6080E;
				case SettingsScannerType.iVinaFB6280E: return this.scannerModel == MODELS.ScanerModel.iVinaFB6280E;
				case SettingsScannerType.KodakI1120: return this.scannerModel == MODELS.ScanerModel.KodakI1120;
				case SettingsScannerType.KodakI1150: return this.scannerModel == MODELS.ScanerModel.KodakI1150;
				case SettingsScannerType.KodakI1150New: return this.scannerModel == MODELS.ScanerModel.KodakI1150New;
				case SettingsScannerType.KodakI1405: return this.scannerModel == MODELS.ScanerModel.KodakI1405;
				case SettingsScannerType.KodakE1035: return this.scannerModel == MODELS.ScanerModel.KodakE1035;
				case SettingsScannerType.KodakE1040: return this.scannerModel == MODELS.ScanerModel.KodakE1040;
				case SettingsScannerType.S2N: return (this.ScannerGroup == MODELS.ScannerGroup.S2N);
				default: return false;
			}
		}
		#endregion

		#endregion

	}
}
