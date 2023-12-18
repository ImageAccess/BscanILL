using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.DespeckleSetting")]
	public class DespeckleSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.Despeckle Value
		{
			get
			{
				switch (base.Selected)
				{
					case "4x4p": return Scanners.S2N.Despeckle.Despeckle4x4;
					default: return Scanners.S2N.Despeckle.Off;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.Despeckle.Despeckle4x4: base.Selected = "4x4p"; break;
					default: base.Selected = "off"; break;
				}
			}
		}
		#endregion

		#endregion

	}
}
