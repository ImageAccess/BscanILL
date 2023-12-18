using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Scanners.Twain.Settings
{
	[XmlType(TypeName = "Scanners.Twain.Settings.FileFormatSetting")]
	public class FileFormatSetting : Scanners.Twain.Settings.TwainSettingBase
	{
		Scanners.Twain.FileFormat			fileFormat = Scanners.Twain.FileFormat.Tiff;
		List<Scanners.Twain.FileFormat>		availableFileFormats = new List<Scanners.Twain.FileFormat>();	
		TwainApp.ICapability				cap = null;


		#region constructor
		public FileFormatSetting()
		{
		}

		public FileFormatSetting(TwainApp.TwainScanner scanner)
		{
			Load(scanner);
		}
		#endregion
	
		
		//PUBLIC PROPERTIES
		#region public properties

		#region Value
		public Scanners.Twain.FileFormat Value
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

		#region Load()
		public override void Load(TwainApp.TwainScanner scanner)
		{
			CopyFrom(scanner.GetCapability((ushort)TwainApp.SharedCap.Icap_IMAGEFILEFORMAT));
		}
		#endregion

		#region CopyFrom()
		public void CopyFrom(FileFormatSetting setting)
		{
			CopyFrom(setting.cap);

			if (this.availableFileFormats.Contains(setting.Value))
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

					availableFileFormats.Clear();
					availableFileFormats.Add(Misc.TwainFileFormatToScannerFileFormat((TwainApp.FileFormat)c.Value));

					this.Value = Misc.TwainFileFormatToScannerFileFormat((TwainApp.FileFormat)c.Value);
				}
				else if (cap is TwainApp.EnumCapabilityUInt32)
				{
					TwainApp.EnumCapabilityUInt32 c = (TwainApp.EnumCapabilityUInt32)cap;

					availableFileFormats.Clear();
					foreach (TwainApp.FileFormat ff in c.Values)
						availableFileFormats.Add(Misc.TwainFileFormatToScannerFileFormat(ff));

					this.Value = Misc.TwainFileFormatToScannerFileFormat((TwainApp.FileFormat)c.Value);
				}
			}
		}
		#endregion

		#endregion

	}
}
