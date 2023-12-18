using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Twain.Settings
{
	[XmlType(TypeName = "Scanners.Twain.Settings.ColorModeSetting")]
	public class ColorModeSetting : Scanners.Twain.Settings.TwainSettingBase
	{
		Scanners.Twain.ColorMode	colorMode = ColorMode.Color;
		TwainApp.ICapability		cap = null;

	
		public ColorModeSetting()
			: base()
		{
		}

		public ColorModeSetting(TwainApp.TwainScanner scanner)
			: base()
		{
			Load(scanner);
		}


		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.Twain.ColorMode Value
		{
			get { return this.colorMode; }
			set
			{
				if (this.colorMode != value)
				{
					this.colorMode = value;
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
			CopyFrom(scanner.GetCapability((ushort)TwainApp.SharedCap.Icap_PIXELTYPE));		
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(ColorModeSetting setting)
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
				if (cap is TwainApp.ValueCapabilityUInt32)
				{
					TwainApp.ValueCapabilityUInt32 c = (TwainApp.ValueCapabilityUInt32)cap;

					switch ((TwainApp.PixelType)c.Value)
					{
						case TwainApp.PixelType.BW: this.colorMode = ColorMode.Bitonal; break;
						case TwainApp.PixelType.GRAY: this.colorMode = ColorMode.Grayscale; break;
						default: this.colorMode = ColorMode.Color; break;
					}
				}
			}
		}
		#endregion

		#endregion

	}
}
