using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.UserUnitSetting")]
	public class UserUnitSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.UserUnit Value
		{
			get { return /*(base.Selected == "mm") ? Scanners.S2N.UserUnit.mm :*/ Scanners.S2N.UserUnit.mil; }
			set
			{
				switch (value)
				{
					//case Scanners.S2N.UserUnit.mm: base.Selected = "mm"; break;
					default: base.Selected = "mil"; break;
				}
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Load()
		internal override void Load(string setting)
		{
			base.Load(setting);
			this.Value = Scanners.S2N.UserUnit.mil;
		}
		#endregion

		#endregion

	}
}
