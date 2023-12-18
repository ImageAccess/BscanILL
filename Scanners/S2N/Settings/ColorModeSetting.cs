using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.ColorModeSetting")]
	public class ColorModeSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.ColorMode Value
		{
			get
			{
				switch (base.Selected)
				{
					case "grayscale": return Scanners.S2N.ColorMode.Grayscale;
					case "lineart": return Scanners.S2N.ColorMode.Lineart;
					case "photo": return Scanners.S2N.ColorMode.Photo;
					default: return Scanners.S2N.ColorMode.Color;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.ColorMode.Grayscale: base.Selected = "grayscale"; break;
					case Scanners.S2N.ColorMode.Lineart: base.Selected = "lineart"; break;
					case Scanners.S2N.ColorMode.Photo: base.Selected = "photo"; break;
					default: base.Selected = "truecolor"; break;
				}
			}
		}
		#endregion

		public bool IsColorOptionInstalled { get { return base.Items.Contains("truecolor"); } }

		#endregion

	}
}
