using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Click.Settings
{
	[XmlType(TypeName = "Scanners.Click.Settings.ColorModeSetting")]
	public class ColorModeSetting : Scanners.Click.Settings.ClickSettingBase
	{
		Scanners.Click.ClickColorMode colorMode = ClickColorMode.Color;

	
		public ColorModeSetting()
			: base()
		{
		}


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public ClickColorMode Value
		{
			get { return this.colorMode; }
			set
			{
				if (this.colorMode != value)
				{
					this.colorMode = value;
					RaiseChanged();
				}
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(ColorModeSetting setting)
		{
			this.Value = setting.Value;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#endregion

	}
}
