using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Twain.Settings.Adf
{
	[XmlType(TypeName = "Scanners.Twain.Settings.Adf.DuplexSetting")]
	public class DuplexSetting : Scanners.Twain.Settings.TwainSettingBase
	{
		bool						duplex = true;
		TwainApp.ICapability		cap = null;

		#region constructor
		public DuplexSetting()
		{
		}

		public DuplexSetting(TwainApp.TwainScanner scanner)
		{
			Load(scanner);
		}
		#endregion
	
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public bool Value
		{
			get { return this.duplex; }
			set
			{
				if (this.duplex != value)
				{
					this.duplex = value;
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
			CopyFrom(scanner.GetCapability((ushort)TwainApp.SharedCap.Cap_DUPLEXENABLED));
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(DuplexSetting setting)
		{
			CopyFrom(setting.cap);
			this.Value = setting.Value;
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
				if (cap is TwainApp.ValueCapabilityBool)
					this.duplex = ((TwainApp.ValueCapabilityBool)cap).Value;
				else if (cap is TwainApp.EnumCapabilityBool)
					this.duplex = ((TwainApp.EnumCapabilityBool)cap).Value;
			}
		}
		#endregion

		#endregion

	}
}
