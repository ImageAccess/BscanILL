using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Twain.Settings
{
	[XmlType(TypeName = "Scanners.Twain.Settings.BrightnessSetting")]
	public class BrightnessSetting : Scanners.Twain.Settings.TwainSettingBase
	{
		double value = 0;
		double max = 0;
		double min = 0;

		TwainApp.ICapability cap = null;


		#region constructor
		public BrightnessSetting()
		{
		}

		public BrightnessSetting(TwainApp.TwainScanner scanner)
		{
			Load(scanner);
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
			CopyFrom(scanner.GetCapability((ushort)TwainApp.SharedCap.Icap_BRIGHTNESS));			
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(BrightnessSetting setting)
		{
			CopyFrom(setting.cap);

			if (this.cap == null)
				this.Value = setting.Value;
			else
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

					this.min = capValue.Values[0] / 1000.0;
					this.max = capValue.Values[0] / 1000.0;

					foreach (float v in capValue.Values)
					{
						if (this.min > v / 1000.0)
							this.min = v / 1000.0;
						if (this.max < v / 1000.0)
							this.max = v / 1000.0;
					}

					this.value = capValue.Value;
				}
				else if (cap is TwainApp.ValueCapabilityFloat)
				{
					TwainApp.ValueCapabilityFloat capValue = (TwainApp.ValueCapabilityFloat)cap;

					this.min = capValue.Value / 1000.0;
					this.max = capValue.Value / 1000.0;
					this.value = capValue.Value / 1000.0;
				}
				else if (cap is TwainApp.RangeCapabilityFloat)
				{
					TwainApp.RangeCapabilityFloat capValue = (TwainApp.RangeCapabilityFloat)cap;

					this.min = capValue.Min / 1000.0;
					this.max = capValue.Max / 1000.0;
					this.value = capValue.Value / 1000.0;
				}
			}
		}
		#endregion

		#endregion
	
	}

}
