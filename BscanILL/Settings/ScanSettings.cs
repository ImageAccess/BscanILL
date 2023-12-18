using System;
using System.Collections;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Speech.Synthesis;

using BscanILL.Export;
using BscanILL.Export.ILLiad;



namespace BscanILL.SETTINGS
{
	[XmlRoot("ScanSettings", Namespace="BscanILL.SETTINGS.ScanSettings")]
	[Serializable]
	public class ScanSettings : Scanners.SETTINGS.ScanSettings
	{
		static ScanSettings		instance = null;        
		static string			settingsFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\Settings\BscanILL.temporary.settings"; 


		#region constructor
		private ScanSettings()
		{
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		//public Scanners.Settings	Scanner { get { return Scanners.Settings.Instance; } set { Scanners.Settings.Instance = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } }

		#endregion


		#region class BaseClass
		public abstract class BaseClass : INotifyPropertyChanged
		{
			protected volatile bool raisePropertyChanged = true;
			protected volatile bool propertyChanged = false;
			
			public event PropertyChangedEventHandler PropertyChanged;

			public abstract void CopyFrom(ScanSettings source);

			#region RaisePropertyChanged()
			public void RaisePropertyChanged(string propertyName)
			{
				if (this.raisePropertyChanged)
				{
					if (PropertyChanged != null)
					{
						if (propertyName != null && (propertyName.StartsWith("get_") || propertyName.StartsWith("set_")))
							propertyName = propertyName.Substring(4);

						PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
					}

					this.propertyChanged = false;
				}
				else
					this.propertyChanged = true;
			}
			#endregion

			#region RaisePropertyChanged()
			public void RaisePropertyChanged(object sender, string propertyName)
			{
				if (this.raisePropertyChanged)
				{
					if (PropertyChanged != null)
					{
						if (propertyName != null && (propertyName.StartsWith("get_") || propertyName.StartsWith("set_")))
							propertyName = propertyName.Substring(4);

						PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
					}

					this.propertyChanged = false;
				}
				else
					this.propertyChanged = true;
			}
			#endregion

		}
		#endregion



		//PUBLIC PROPERTIES
		#region public properties

		#region Instance
		static public ScanSettings Instance
		{
			get 
			{
				if (ScanSettings.instance == null)
				{
					if (File.Exists(settingsFile))
					{
						try
						{
							using (FileStream stream = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.Read))
							{
								XmlSerializer serializer = new XmlSerializer(typeof(BscanILL.SETTINGS.ScanSettings));

								ScanSettings.instance = (BscanILL.SETTINGS.ScanSettings)serializer.Deserialize(stream);

								//BscanILL.SETTINGS.ScanSettings.Instance.CopyFrom(ScanSettings.instance);
							}
						}
						catch (Exception)
						{
							//throw new Exception(string.Format("Can't read settings from '{0}'!", settingsFile) + Environment.NewLine + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
							ScanSettings.instance = new ScanSettings();
							//BscanILL.SETTINGS.ScanSettings.Instance.CopyFrom(ScanSettings.instance);
						}
					}
					else
					{
						ScanSettings.instance = new ScanSettings();
						//BscanILL.SETTINGS.ScanSettings.Instance.CopyFrom(ScanSettings.instance);
					}
				}
					
				return ScanSettings.instance;
			} 
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Load()
		public static void Load()
		{
			ScanSettings currentSettings = ScanSettings.Instance;
		}
		#endregion

		#region Save()
		public void Save()
		{
			new FileInfo(settingsFile).Directory.Create();
			
			string tmpFile = settingsFile + ".tmp";

			using (FileStream stream = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(BscanILL.SETTINGS.ScanSettings));

				serializer.Serialize(stream, this);
			}

			if (File.Exists(settingsFile))
				File.Delete(settingsFile);

			File.Move(tmpFile, settingsFile);
		}
		#endregion

		#region ApplySettings()
		public void ApplySettings(BscanILL.SETTINGS.ScanSettings source, bool copyToGlobal)
		{
			this.CopyFrom(source);

			if (copyToGlobal)
			{
				BscanILL.SETTINGS.ScanSettings.Instance.CopyFrom(source);
			}
		}
		#endregion

		#endregion

	}

}

