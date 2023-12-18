using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Click.Settings.Click
{
	[XmlType(TypeName = "Scanners.Click.Settings.Click.ScanPageSetting")]
	public class ScanPageSetting : Scanners.Click.Settings.ClickSettingBase
	{
		ClickScanPage scanPage = ClickScanPage.Both;


		#region constructor
		public ScanPageSetting()
		{
		}
		#endregion
	
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public ClickScanPage Value
		{
			get { return this.scanPage; }
			set
			{
				if (this.scanPage != value)
				{
					this.scanPage = value;

					RaiseChanged();
				}
			}
		}
		#endregion

		#endregion

	
		//PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(ScanPageSetting setting)
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
