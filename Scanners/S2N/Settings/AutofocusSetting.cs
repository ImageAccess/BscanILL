using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.AutofocusSetting")]
	public class AutofocusSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.Autofocus Value
		{
			get
			{
				switch (base.Selected)
				{
					case "off": return Scanners.S2N.Autofocus.Off;
					case "skip": return Scanners.S2N.Autofocus.Skip;
					default: return Scanners.S2N.Autofocus.On;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.Autofocus.Off: base.Selected = "off"; break;
					case Scanners.S2N.Autofocus.Skip: base.Selected = "skip"; break;
					default: base.Selected = "on"; break;
				}
			}
		}
		#endregion

		#endregion

	}
}
