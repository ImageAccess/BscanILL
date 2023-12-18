using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Twain.Settings.Bookedge
{
	[XmlType(TypeName = "Scanners.Twain.Settings.Bookedge.ScanPageSetting")]
	public class ScanPageSetting : Scanners.Twain.Settings.TwainSettingBase
	{
		BookedgePage				scanPage = BookedgePage.Automatic;
		TwainApp.ICapability		cap = null;

		#region constructor
		public ScanPageSetting()
		{
		}

		public ScanPageSetting(TwainApp.TwainScanner scanner)
		{
			Load(scanner);
		}
		#endregion
	
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public BookedgePage Value
		{
			get { return this.scanPage; }
			set
			{
				if (this.scanPage != value)
				{
					this.scanPage = value;

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
			CopyFrom(scanner.GetCapability((ushort)TwainApp.SharedCap.Icap_AUTOMATICROTATE));
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(ScanPageSetting setting)
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
					this.scanPage = (((TwainApp.ValueCapabilityBool)cap).Value) ? BookedgePage.Automatic : BookedgePage.FlatMode;
				else if (cap is TwainApp.EnumCapabilityBool)
					this.scanPage = (((TwainApp.EnumCapabilityBool)cap).Value) ? BookedgePage.Automatic : BookedgePage.FlatMode;
			}
		}
		#endregion

		#endregion

	}
}
