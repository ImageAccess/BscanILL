using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Click.Settings
{
	[XmlType(TypeName = "Scanners.Click.Settings.DpiSetting")]
	public class DpiSetting : Scanners.Click.Settings.ClickSettingBase
	{
		int value = 300;
		int max = 300;
		int min = 150;


		#region constructor
		public DpiSetting()
		{
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public int Minimum { get { return this.min; } }
		public int Maximum { get { return this.max; } }

		#region Value
		public int Value
		{
			get { return this.value; }
			set
			{
				if (this.value != value)
				{
					this.value = Math.Max(this.Minimum, Math.Min(this.Maximum, value));

					RaiseChanged();
				}
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(DpiSetting setting)
		{
			this.Value = Math.Max(this.min, Math.Min(this.max, setting.Value));
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#endregion
	
	}

}
