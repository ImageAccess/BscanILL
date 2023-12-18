using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.OnOffSetting")]
	public class OnOffSetting : Scanners.S2N.Settings.ISetting
	{
		bool defined = false;
		string description = "";
		bool value = true;

		public event SettingChangedHnd Changed;

		#region constructor
		public OnOffSetting()
		{
		}

		public OnOffSetting(bool value)
		{
			this.value = value;
		}

		public OnOffSetting(string dpiSetting)
		{
			Load(dpiSetting);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region IsDefined
		public bool IsDefined
		{
			get { return this.defined; }
		}
		#endregion

		#region Value
		public bool Value
		{
			get { return this.value; }
			set
			{
				if (this.value != value)
				{
					this.value = value;

					if (Changed != null)
						Changed();
				}
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Load()
		public void Load(string dpiSetting)
		{
			string[] a = dpiSetting.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (a.Length > 0)
			{
				this.description = a[0];

				if (a.Length >= 5)
				{
					this.value = (a[1].ToLower() != "off");
					this.defined = true;
				}
			}
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(OnOffSetting source)
		{
			this.defined = source.defined;
			this.description = source.description;
			this.Value = source.Value;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			if (this.defined)
				return string.Format("+{0}{1}", this.description, this.value ? "on" : "off");
			else
				return "";
		}
		#endregion

		#endregion

	}

}
