using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.DocumentModeSetting")]
	public class DocumentModeSetting : Scanners.S2N.Settings.EnumSetting
	{

		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.S2N.DocumentMode Value
		{
			get
			{
				try { return (Scanners.S2N.DocumentMode)int.Parse(base.Selected); }
				catch { return S2N.DocumentMode.Flat; }
			}
			set 
			{
				//if (base.IsDefined && base.Items.Contains(((int)value).ToString()))
			    // 		base.Selected = string.Format("{0}", (int)value);

                base.Selected = string.Format("{0}", (int)value);       //when initializing this setting from settings xml file, the Items and IsDefined is not set yet so this setting was not initialized properly
			}
		}
		#endregion

		#region AvailableValues
		public List<Scanners.S2N.DocumentMode> AvailableValues
		{	
			get
			{
				List<Scanners.S2N.DocumentMode> list = new List<S2N.DocumentMode>();

				if (this.IsDefined)
				{
					if (base.Items.Contains("0")) list.Add(S2N.DocumentMode.Flat);
					if (base.Items.Contains("1")) list.Add(S2N.DocumentMode.Book);
					if (base.Items.Contains("2")) list.Add(S2N.DocumentMode.Folder);
					if (base.Items.Contains("3")) list.Add(S2N.DocumentMode.FixedFocus);
					if (base.Items.Contains("4")) list.Add(S2N.DocumentMode.GlassPlate);
					if (base.Items.Contains("5")) list.Add(S2N.DocumentMode.Auto);
					if (base.Items.Contains("6")) list.Add(S2N.DocumentMode.V);
					if (base.Items.Contains("7")) list.Add(S2N.DocumentMode.BookGlassPlate);
				}

				return list;
			}
		}
		#endregion

		public bool IsFolderAvailable			{ get { return base.Items.Contains("2"); } }
		public bool IsFixedFocusAvailable		{ get { return base.Items.Contains("3"); } }
		public bool IsGlassPlateModeAvailable	{ get { return base.Items.Contains("4"); } }
		public bool IsVModeAvailable			{ get { return base.Items.Contains("5"); } }

		#endregion
	
	}
}
