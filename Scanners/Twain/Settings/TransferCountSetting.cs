using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Twain.Settings
{
	[XmlType(TypeName = "Scanners.Twain.Settings.TransferCountSetting")]
	public class TransferCountSetting : Scanners.Twain.Settings.TwainSettingBase
	{
		int value = -1;

		TwainApp.ICapability cap = null;


		#region constructor
		public TransferCountSetting()
		{
		}

		public TransferCountSetting(TwainApp.TwainScanner scanner)
		{
			Load(scanner);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public int Value
		{
			get { return this.value; }
			set
			{
				if (value < 0)
					value = -1;

				if (value != 0 && this.value != value)
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
			CopyFrom(scanner.GetCapability((ushort)TwainApp.SharedCap.Cap_XFERCOUNT));
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(TransferCountSetting setting)
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
				TwainApp.ValueCapabilityInt32 capValue = (TwainApp.ValueCapabilityInt32)cap;

				this.Value = capValue.Value;
			}
		}
		#endregion

		#endregion
	
	}

}
