using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Click.Settings.ClickMini
{
	[XmlType(TypeName = "Scanners.Click.Settings.ClickMini.ScanModeSetting")]
	public class ScanModeSetting : Scanners.Click.Settings.ClickSettingBase
	{
		ClickMiniScanMode scanMode = ClickMiniScanMode.SplitImage;


		#region constructor
		public ScanModeSetting()
		{
		}
		#endregion
	
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public ClickMiniScanMode Value
		{
			get { return this.scanMode; }
			set
			{
				if (this.scanMode != value)
				{
					this.scanMode = value;

					RaiseChanged();
				}
			}
		}
		#endregion

		#endregion

	
		//PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(ScanModeSetting setting)
		{
			this.Value = setting.Value;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods
		#endregion

	}
}
