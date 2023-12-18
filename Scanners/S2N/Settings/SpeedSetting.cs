using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.SpeedSetting")]
	public class SpeedSetting : Scanners.S2N.Settings.EnumSetting
	{
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.Speed Value
		{
			get
			{
				switch (base.Selected)
				{
					case "1": return Scanners.S2N.Speed.Quality;
					default: return Scanners.S2N.Speed.Fast;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.Speed.Quality: base.Selected = "1"; break;
					default: base.Selected = "2"; break;
				}
			}
		}
		#endregion

		#endregion
	
	}
}
