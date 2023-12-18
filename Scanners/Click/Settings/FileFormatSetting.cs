using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Click.Settings
{
	[XmlType(TypeName = "Scanners.Click.Settings.FileFormatSetting")]
	public class FileFormatSetting : Scanners.Click.Settings.ClickSettingBase
	{
		Scanners.FileFormat			fileFormat = Scanners.FileFormat.Tiff;


		#region constructor
		public FileFormatSetting()
		{
		}
		#endregion
	
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.FileFormat Value
		{
			get { return this.fileFormat; }
			set
			{
				if (this.fileFormat != value)
				{
					this.fileFormat = value;

					RaiseChanged();
				}
			}
		}
		#endregion

		#endregion
	

		//PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(FileFormatSetting setting)
		{
			this.Value = setting.Value;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#endregion

	}
}
