using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.BitonalThresholdSetting")]
	public class BitonalThresholdSetting : Scanners.S2N.Settings.EnumSetting
	{


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.BitonalThreshold Value
		{
			get
			{
				switch (base.Selected)
				{
					case "on": return Scanners.S2N.BitonalThreshold.Dynamic;
					case "off": return Scanners.S2N.BitonalThreshold.Fixed;
					default: return Scanners.S2N.BitonalThreshold.Fixed;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.BitonalThreshold.Dynamic: base.Selected = "on"; break;
					default: base.Selected = "off"; break;
				}
			}
		}
		#endregion

		#endregion

	}
}
