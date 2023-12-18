using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Twain.Settings
{
	[XmlType(TypeName = "Scanners.Twain.Settings.DocSizeSetting")]
	public class DocSizeSetting : Scanners.Twain.Settings.TwainSettingBase
	{
		DocSize						docSize = DocSize.Auto;
		TwainApp.ICapability		cap = null;

		#region constructor
		public DocSizeSetting()
		{
		}

		public DocSizeSetting(TwainApp.TwainScanner scanner)
		{
			Load(scanner);
		}
		#endregion
	
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public DocSize Value
		{
			get { return this.docSize; }
			set
			{
				if (this.docSize != value)
				{
					this.docSize = value;
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
			CopyFrom(scanner.GetCapability((ushort)TwainApp.SharedCap.Icap_AUTOMATICBORDERDETECTION));
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(DocSizeSetting setting)
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
					this.Value = (((TwainApp.ValueCapabilityBool)cap).Value) ? DocSize.Auto : DocSize.Max;
				else if (cap is TwainApp.EnumCapabilityBool)
					this.Value = (((TwainApp.EnumCapabilityBool)cap).Value) ? DocSize.Auto : DocSize.Max;
			}
		}
		#endregion

		#endregion

	}
}
