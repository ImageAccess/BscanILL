using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.ScanModeSetting")]
	public class ScanModeSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.ScanMode Value
		{
			get
			{
				if (base.Selected == "wait")
					return Scanners.S2N.ScanMode.Wait;
				else
					return Scanners.S2N.ScanMode.Direct;
			}
			set
			{
				if (value == Scanners.S2N.ScanMode.Wait)
					base.Selected = "wait";
				else
					base.Selected = "direct";
			}
		}
		#endregion

		#endregion

	}
}
