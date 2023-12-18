using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Click.Settings
{
	[XmlType(TypeName = "Scanners.Click.Settings.FocusSetting")]
	public class FocusSetting : Scanners.Click.Settings.ClickSettingBase
	{
		bool focus = false;


		#region constructor
		public FocusSetting()
		{
		}
		#endregion
	
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public bool Value
		{
			get { return this.focus; }
			set
			{
				if (this.focus != value)
				{
					this.focus = value;

					RaiseChanged();
				}
			}
		}
		#endregion

		#endregion
	

		//PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(FocusSetting setting)
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
