using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.S2N.Settings
{
	[XmlType(TypeName = "Scanners.S2N.Settings.EnumSetting")]
	public class EnumSetting : Scanners.S2N.Settings.ISetting
	{
		bool			defined = false;
		string			description = "";
		string			value = "";
		List<string>	values = new List<string>();

		public event SettingChangedHnd Changed;


		#region constructor
		public EnumSetting()
		{
		}

		public EnumSetting(string setting)
		{
			Load(setting);
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region IsDefined
		public bool IsDefined
		{
			get { return this.defined; }
		}
		#endregion

		#region Selected
		protected string Selected
		{
			get { return this.value; }
			set
			{
				//if (values.Contains(value) == false)
				//	throw new ArgumentException(string.Format("Value '{0}' is not part of '{1}' emun!", value, this.description));

				if (/*values.Contains(value) &&*/ (this.value != value))
				{
					this.value = value;

					if (Changed != null)
						Changed();
				}
			}
		}
		#endregion

		#region Items
		protected List<string> Items
		{
			get { return this.values; }
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Load()
		internal virtual void Load(string setting)
		{
			string[] a = setting.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (a.Length > 0)
			{
				this.description = a[0];

				if (a.Length >= 2)
				{
					this.value = a[1];

					for (int i = 3; i < a.Length; i++ )
						this.values.Add(a[i]);

					this.defined = true;
				}
			}
		}
		#endregion

		#region CopyFrom()
		public virtual void CopyFrom(EnumSetting source)
		{
			this.defined = source.defined;
			this.description = source.description;

			this.values.Clear();
			
			foreach(string v in source.values)
				this.values.Add(v);

			this.Selected = source.value;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			if (this.defined)
				return string.Format("+{0}{1}", this.description, this.value);
			else
				return "";
		}
		#endregion

		#endregion

	}
}
