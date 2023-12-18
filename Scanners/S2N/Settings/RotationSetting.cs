using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.RotationSetting")]
	public class RotationSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.Rotation Value
		{
			get
			{
				switch (base.Selected)
				{
					case "90": return Scanners.S2N.Rotation.CV90;
					case "180": return Scanners.S2N.Rotation.CV180;
					case "270": return Scanners.S2N.Rotation.CV270;
					default: return Scanners.S2N.Rotation.None;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.Rotation.CV90: base.Selected = "90"; break;
					case Scanners.S2N.Rotation.CV180: base.Selected = "180"; break;
					case Scanners.S2N.Rotation.CV270: base.Selected = "270"; break;
					default: base.Selected = "0"; break;
				}
			}
		}
		#endregion

		#endregion

	}
}
