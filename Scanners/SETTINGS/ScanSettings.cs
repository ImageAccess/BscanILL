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



namespace Scanners.SETTINGS
{
	[XmlRoot("ScanSettings", Namespace = "Scanners.ScanSettings")]
	[Serializable]
	public class ScanSettings : BaseClass
	{
		Scanners.S2N.S2NSettings			s2n = new Scanners.S2N.S2NSettings();
		Scanners.Twain.BookedgeSettings		bookedge = new Scanners.Twain.BookedgeSettings();
		Scanners.Twain.AdfSettings			adf = new Twain.AdfSettings();
		Scanners.Click.ClickSettings		click = new Click.ClickSettings();
		Scanners.Click.ClickMiniSettings	clickMini = new Click.ClickMiniSettings();

		#region constructor
		public ScanSettings()
		{
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public Scanners.S2N.S2NSettings			S2N { get { return s2n; } set { if (s2n != value) { s2n = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } } }
		public Scanners.Twain.BookedgeSettings	BookEdge { get { return bookedge; } set { if (bookedge != value) { bookedge = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } } }
		public Scanners.Twain.AdfSettings		Adf { get { return adf; } set { if (adf != value) { adf = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } } }
		public Scanners.Click.ClickSettings		Click { get { return click; } set { if (click != value) { click = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } } }
		public Scanners.Click.ClickMiniSettings ClickMini { get { return clickMini; } set { if (clickMini != value) { clickMini = value; RaisePropertyChanged(MethodBase.GetCurrentMethod().Name); } } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region CopyFrom()
		public void CopyFrom(Scanners.SETTINGS.ScanSettings source)
		{
			this.S2N.CopyFrom(source.S2N);
			this.BookEdge.CopyFrom(source.BookEdge);
			this.Adf.CopyFrom(source.Adf);
			this.click.CopyFrom(source.Click);
			this.clickMini.CopyFrom(source.ClickMini);
		}
		#endregion

		#endregion

	}

}
