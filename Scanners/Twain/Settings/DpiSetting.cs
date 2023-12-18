using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Twain.Settings
{
	[XmlType(TypeName = "Scanners.Twain.Settings.DpiSetting")]
	public class DpiSetting : Scanners.Twain.Settings.TwainSettingBase
	{
		int value = 300;
		int max = 600;
		int min = 150;

		TwainApp.ICapability cap = null;


		#region constructor
		public DpiSetting()
		{
		}

		public DpiSetting(TwainApp.TwainScanner scanner)
		{
			Load(scanner);
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
				if (value < this.min)
				{
					//throw new ArgumentException("Range value is smaller than required minimum!");
					value = this.min;
				}
				if (value > this.max)
				{
					//throw new ArgumentException("Range value is bigger than required maximum!");
					value = this.max;
				}

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

		#region Load()
		public override void Load(TwainApp.TwainScanner scanner)
		{
			CopyFrom(scanner.GetCapability((ushort)TwainApp.SharedCap.Icap_XRESOLUTION));
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(DpiSetting setting)
		{
			CopyFrom(setting.cap);
			this.Value = Math.Max(this.min, Math.Min(this.max, setting.Value));
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region CopyFrom()
		void CopyFrom(TwainApp.ICapability cap)
		{
			this.cap = cap;
			this.IsDefined = (this.cap != null);
			this.IsReadOnly = (this.cap == null || this.cap.IsReadOnly);

			if (cap != null)
			{
				if (cap is TwainApp.EnumCapabilityFloat)
				{
					TwainApp.EnumCapabilityFloat capValue = (TwainApp.EnumCapabilityFloat)cap;

					this.min = Convert.ToInt32(capValue.Values[0]);
					this.max = Convert.ToInt32(capValue.Values[0]);

					foreach (int v in capValue.Values)
					{
						if (this.min > v)
							this.min = v;
						if (this.max < v)
							this.max = v;
					}

					this.Value = Convert.ToInt32(capValue.Value);
				}
				else if (cap is TwainApp.ValueCapabilityFloat)
				{
					TwainApp.ValueCapabilityFloat capValue = (TwainApp.ValueCapabilityFloat)cap;

					this.min = Convert.ToInt32(capValue.Value);
					this.max = Convert.ToInt32(capValue.Value);
					this.Value = Convert.ToInt32(capValue.Value);
				}
				else if (cap is TwainApp.RangeCapabilityFloat)
				{
					TwainApp.RangeCapabilityFloat capValue = (TwainApp.RangeCapabilityFloat)cap;

					this.min = Convert.ToInt32(capValue.Min);
					this.max = Convert.ToInt32(capValue.Max);
					this.Value = Convert.ToInt32(capValue.Value);
				}
			}
		}
		#endregion

		#endregion
	
	}

}
