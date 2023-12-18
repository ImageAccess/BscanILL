using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.DocumentSizeSetting")]
	public class DocumentSizeSetting : Scanners.S2N.Settings.EnumSetting
	{
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.DocumentSize Value
		{
			get
			{
				switch (base.Selected)
				{
					case "maxl": return Scanners.S2N.DocumentSize.MaximumLandscape;
					case "maxp": return Scanners.S2N.DocumentSize.MaximumPortraitLeft;
					case "maxpr": return Scanners.S2N.DocumentSize.MaximumPortraitRight;
					case "legl": return Scanners.S2N.DocumentSize.LegalLandscape;
					case "letl": return Scanners.S2N.DocumentSize.LetterLandscape;
					case "letp": return Scanners.S2N.DocumentSize.LetterPortraitLeft;
					case "letpr": return Scanners.S2N.DocumentSize.LetterPortraitRight;
					case "hletp": return Scanners.S2N.DocumentSize.HalfLetterProtraitLeft;
					case "hletpr": return Scanners.S2N.DocumentSize.HalfLetterProtraitRight;
					case "usbl": return Scanners.S2N.DocumentSize.X11x17Landscape;
					case "usbp": return Scanners.S2N.DocumentSize.X11x17PortraitLeft;
					case "usbpr": return Scanners.S2N.DocumentSize.X11x17PortraitRight;
					case "user": return Scanners.S2N.DocumentSize.User;
					default: return Scanners.S2N.DocumentSize.Auto;
				}
			}
			set
			{
				switch (value)
				{
					case Scanners.S2N.DocumentSize.MaximumLandscape: base.Selected = "maxl"; break;
					case Scanners.S2N.DocumentSize.MaximumPortraitLeft: base.Selected = "maxp"; break;
					case Scanners.S2N.DocumentSize.MaximumPortraitRight: base.Selected = "maxpr"; break;
					case Scanners.S2N.DocumentSize.LetterLandscape: base.Selected = "letl"; break;
					case Scanners.S2N.DocumentSize.LetterPortraitLeft: base.Selected = "letp"; break;
					case Scanners.S2N.DocumentSize.LetterPortraitRight: base.Selected = "letpr"; break;
					case Scanners.S2N.DocumentSize.LegalLandscape: base.Selected = "legl"; break;
					case Scanners.S2N.DocumentSize.X11x17Landscape: base.Selected = "usbl"; break;
					case Scanners.S2N.DocumentSize.X11x17PortraitLeft: base.Selected = "usbp"; break;
					case Scanners.S2N.DocumentSize.X11x17PortraitRight: base.Selected = "usbpr"; break;
					case Scanners.S2N.DocumentSize.HalfLetterProtraitLeft: base.Selected = "hletp"; break;
					case Scanners.S2N.DocumentSize.HalfLetterProtraitRight: base.Selected = "hletpr"; break;
					case Scanners.S2N.DocumentSize.User: base.Selected = "user"; break;
					case Scanners.S2N.DocumentSize.Auto: base.Selected = "auto"; break;
				}
			}
		}
		#endregion	
		
		#region Load()
		internal override void Load(string setting)
		{
			base.Load(setting);

			/*	if (this.Items.Contains("legl") == false)
				this.Items.Add("legl");
			if (this.Items.Contains("letl") == false)
				this.Items.Add("letl");
			if (this.Items.Contains("letp") == false)
				this.Items.Add("letp");
			if (this.Items.Contains("letpr") == false)
				this.Items.Add("letpr");*/
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			return base.ToString();
		}
		#endregion

		#endregion

	}
}
