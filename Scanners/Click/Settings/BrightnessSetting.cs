using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Click.Settings
{
	[XmlType(TypeName = "Scanners.Click.Settings.BrightnessSetting")]
	public class BrightnessSetting : Scanners.Click.Settings.ClickSettingBase
	{
		double value = 0;
		double max = 1;
		double min = -1;


		#region constructor
		public BrightnessSetting()
		{
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public double Minimum { get { return this.min; } }
		public double Maximum { get { return this.max; } }

		#region Value
		public double Value
		{
			get { return this.value; }
			set
			{
				if (this.value != value)
				{
					this.value = value;

					RaiseChanged();
				}
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(BrightnessSetting setting)
		{
			this.max = setting.Maximum;
			this.min = setting.Minimum;
			this.Value = setting.Value;
		}
		#endregion
	
		#endregion


		// PRIVATE METHODS
		#region private methods
		#endregion
	
	}

}
