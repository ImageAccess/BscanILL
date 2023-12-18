using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.TiffCompressionSetting")]
	public class TiffCompressionSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.TiffCompression Value
		{
			get
			{
				switch (base.Selected)
				{
					case "g4": return Scanners.S2N.TiffCompression.G4;
					default: return Scanners.S2N.TiffCompression.None;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.TiffCompression.G4: base.Selected = "g4"; break;
					default: base.Selected = "none"; break;
				}
			}
		}
		#endregion

		#endregion
	
	}
}
