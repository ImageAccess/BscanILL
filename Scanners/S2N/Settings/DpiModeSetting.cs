using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.DpiModeSetting")]
	public class DpiModeSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.DpiMode Value
		{
			get
			{
				switch (base.Selected)
				{
					case "flexible": return Scanners.S2N.DpiMode.Flexible;
					case "fixed": return Scanners.S2N.DpiMode.Fixed;
					default: return Scanners.S2N.DpiMode.Flexible;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.DpiMode.Fixed: base.Selected = "fixed"; break;
					default: base.Selected = "flexible"; break;
				}
			}
		}
		#endregion

		#endregion

	}
}
