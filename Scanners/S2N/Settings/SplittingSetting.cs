using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.SplittingSetting")]
	public class SplittingSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.Splitting Value
		{
			get
			{
				switch (base.Selected)
				{
					case "left": return Scanners.S2N.Splitting.Left;
					case "right": return Scanners.S2N.Splitting.Right;
                    case "auto": return Scanners.S2N.Splitting.Auto;				
					default: return Scanners.S2N.Splitting.Off;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.Splitting.Left: base.Selected = "left"; break;
					case Scanners.S2N.Splitting.Right: base.Selected = "right"; break;
                    case Scanners.S2N.Splitting.Auto: base.Selected = "auto"; break;	
					default: base.Selected = "off"; break;
				}
			}
		}
		#endregion

		#endregion
	
	}
}
