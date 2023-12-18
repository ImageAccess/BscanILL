using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.LightSetting")]
	public class LightSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.LightSwitch Value
		{
			get
			{
				switch (base.Selected)
				{
					case "off": return Scanners.S2N.LightSwitch.Off;
					default: return Scanners.S2N.LightSwitch.On;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.LightSwitch.Off: base.Selected = "off"; break;
					default: base.Selected = "on"; break;
				}
			}
		}
		#endregion

		#endregion

	}
}
