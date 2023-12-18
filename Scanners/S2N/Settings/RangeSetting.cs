using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.RangeSetting")]
	public class RangeSetting : Scanners.S2N.Settings.ISetting
	{
		bool defined = false;
		string description = "";
		int value = 0;
		int max = 0;
		int min = 0;

		public event SettingChangedHnd Changed;

		#region constructor
		public RangeSetting()
		{
			this.min = 0;
			this.max = 0;
			this.value = 0;
		}

		public RangeSetting(int minimum, int maximum, int value)
		{
			this.min = minimum;
			this.max = maximum;
			this.value = value;
		}

		public RangeSetting(string dpiSetting)
		{
			Load(dpiSetting);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public int Minimum { get { return this.min; } set { this.min = value; } }
		public int Maximum { get { return this.max; } set { this.max = value; } }

		#region IsDefined
		public bool IsDefined
		{
			get { return this.defined; }
		}
		#endregion

		#region Value
		public int Value
		{
			get { return this.value; }
			set
			{
				/*if (value < this.min)
				{
					//throw new ArgumentException("Range value is smaller than required minimum!");
					value = this.min;
				}
				if (value > this.max)
				{
					//throw new ArgumentException("Range value is bigger than required maximum!");
					value = this.max;
				}*/

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
					this.value = int.Parse(a[1]);
					this.max = int.Parse(a[4]);
					this.min = int.Parse(a[3]);
					this.defined = true;
				}
			}
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(RangeSetting source)
		{
			this.defined = source.defined;
			this.description = source.description;
			this.min = source.min;
			this.max = source.max;
			this.Value = source.Value;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			if (this.defined)
				return string.Format("+{0}{1}", this.description, this.value);
			else
				return "";
		}
		#endregion

		#endregion

	}

}
