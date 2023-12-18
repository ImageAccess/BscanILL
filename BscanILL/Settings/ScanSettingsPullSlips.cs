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
    [XmlRoot("ScanSettingsPullSlips", Namespace = "BscanILL.SETTINGS.ScanSettingsPullSlips")]
    [Serializable]
    public class ScanSettingsPullSlips : Scanners.SETTINGS.ScanSettings
    {
        static ScanSettingsPullSlips instance = null;        
		static string			settingsFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\DLSG\BscanILL\Settings\BscanILL.temporary_pullslips.settings"; 


		#region constructor
        private ScanSettingsPullSlips()
		{
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties		

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
        static public ScanSettingsPullSlips Instance
        {
            get
            {
                if (ScanSettingsPullSlips.instance == null)
                {
                    if (File.Exists(settingsFile))
                    {
                        try
                        {
                            using (FileStream stream = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(BscanILL.SETTINGS.ScanSettingsPullSlips));

                                ScanSettingsPullSlips.instance = (BscanILL.SETTINGS.ScanSettingsPullSlips)serializer.Deserialize(stream);

                                //BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.CopyFrom(ScanSettingsPullSlips.instance);
                            }
                        }
                        catch (Exception)
                        {
                            //throw new Exception(string.Format("Can't read settings from '{0}'!", settingsFile) + Environment.NewLine + Environment.NewLine + BscanILL.Misc.Misc.GetErrorMessage(ex));
                            ScanSettingsPullSlips.instance = new ScanSettingsPullSlips();
                            //BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.CopyFrom(ScanSettingsPullSlips.instance);
                        }
                    }
                    else
                    {
                        ScanSettingsPullSlips.instance = new ScanSettingsPullSlips();
                        //default to middle brightness/contrast when creating new setting file
                        ScanSettingsPullSlips.instance.ClickMini.Brightness.Value = 0;
                        ScanSettingsPullSlips.instance.ClickMini.Contrast.Value = 0;
                        ScanSettingsPullSlips.instance.Click.Brightness.Value = 0;
                        ScanSettingsPullSlips.instance.Click.Contrast.Value = 0;
                        ScanSettingsPullSlips.instance.S2N.Brightness.Value = 127;
                        ScanSettingsPullSlips.instance.S2N.Contrast.Value = 127;
                        ScanSettingsPullSlips.instance.BookEdge.Brightness.Value = 0;
                        ScanSettingsPullSlips.instance.BookEdge.Contrast.Value = 0;
                        ScanSettingsPullSlips.instance.Adf.Brightness.Value = 0;
                        ScanSettingsPullSlips.instance.Adf.Contrast.Value = 0;
                        
                        //BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.CopyFrom(ScanSettingsPullSlips.instance);
                    }
                }

                return ScanSettingsPullSlips.instance;
            }
        }
        #endregion

        #endregion


        //PUBLIC METHODS
        #region public methods

        #region Load()
        public static void Load()
        {
            ScanSettingsPullSlips currentSettings = ScanSettingsPullSlips.Instance;
        }
        #endregion

        #region Save()
        public void Save()
        {
            new FileInfo(settingsFile).Directory.Create();

            string tmpFile = settingsFile + ".tmp";

            using (FileStream stream = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BscanILL.SETTINGS.ScanSettingsPullSlips));

                serializer.Serialize(stream, this);
            }

            if (File.Exists(settingsFile))
                File.Delete(settingsFile);

            File.Move(tmpFile, settingsFile);
        }
        #endregion

        #region ApplySettings()
        public void ApplySettings(BscanILL.SETTINGS.ScanSettingsPullSlips source, bool copyToGlobal)
        {
            this.CopyFrom(source);

            if (copyToGlobal)
            {
                BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.CopyFrom(source);
            }
        }
        #endregion

        #endregion

    }
}
