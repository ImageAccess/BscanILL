using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
    [XmlType(TypeName = "Scanners.S2N.Settings.SplittingStartPageSetting")]
	public class SplittingStartPageSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
        public Scanners.S2N.SplittingStartPage Value
		{
			get
			{
				switch (base.Selected)
				{
                    case "right": return Scanners.S2N.SplittingStartPage.Right;					
                    default: return Scanners.S2N.SplittingStartPage.Left;
				}
			}
			set
			{
				switch (value)
				{
                    case Scanners.S2N.SplittingStartPage.Right: base.Selected = "right"; break;
                    default: base.Selected = "left"; break;
				}
			}
		}
		#endregion

		#endregion
	
	}
}