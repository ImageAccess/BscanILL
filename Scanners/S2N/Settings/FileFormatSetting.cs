using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.FileFormatSetting")]
	public class FileFormatSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.FileFormat Value
		{
			get
			{
				switch (base.Selected)
				{
					case "tiff": return Scanners.S2N.FileFormat.Tiff;
					case "jpeg": return Scanners.S2N.FileFormat.Jpeg;
					case "pnm": return Scanners.S2N.FileFormat.Pnm;
					case "pdf": return Scanners.S2N.FileFormat.Pdf;
					default: return Scanners.S2N.FileFormat.Unknown;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.FileFormat.Tiff: base.Selected = "tiff"; break;
					default: base.Selected = "jpeg"; break;
				}
			}
		}
		#endregion

		#endregion
	
	}
}
