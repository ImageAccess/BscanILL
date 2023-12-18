using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Scan
{
	class ScannersManager
	{
		static ScannersManager			instance = null;
		
		Scanners.IScanner				primaryScanner = null;
		Scanners.IScanner				secondaryScanner = null;
		bool							isPrimaryScannerSelected = true;
		
		public delegate void ScannersChangedHnd();
		public event ScannersChangedHnd			PrimaryScannerChanged;
		public event ScannersChangedHnd			SecondaryScannerChanged;

		public delegate void SelectedScannerChangedHnd(bool isPrimaryScannerSelected);
		public event SelectedScannerChangedHnd	SelectedScannerChanged;


		#region constructor
		public ScannersManager()
		{
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#region Instance
		public static ScannersManager Instance
		{
			get
			{
				if (instance == null)
					instance = new ScannersManager();

				return instance;
			}
		}
		#endregion

		public bool		MultiScannerMode { get { return primaryScanner != null && secondaryScanner != null; } }

		public BscanILL.SETTINGS.ScanSettings ScanSettings { get { return BscanILL.SETTINGS.ScanSettings.Instance; } }
	
		#region IsPrimaryScannerSelected
		public bool IsPrimaryScannerSelected
		{
			get { return isPrimaryScannerSelected; }
			set
			{
				if (isPrimaryScannerSelected != value && (value == true || this.MultiScannerMode))
				{
					isPrimaryScannerSelected = value;

					if (SelectedScannerChanged != null)
						SelectedScannerChanged(isPrimaryScannerSelected);
				}
			}
		}
		#endregion

		#region PrimaryScanner
		public Scanners.IScanner PrimaryScanner
		{
			get { return this.primaryScanner; }
			set
			{
				if (this.primaryScanner != value)
				{
					this.primaryScanner = value;

					if (this.primaryScanner is Scanners.Twain.AdfScanner)
					{
						Scanners.Twain.AdfSettings adfSettings = ((Scanners.Twain.AdfScanner)this.primaryScanner).GetScannerSettings();
						Scanners.Twain.AdfSettings tmp = new Scanners.Twain.AdfSettings();
						tmp.CopyValuesFrom(this.ScanSettings.Adf);
						this.ScanSettings.Adf.CopyFrom(adfSettings);
						this.ScanSettings.Adf.CopyValuesFrom(tmp);
					}
					else if (this.primaryScanner is Scanners.Twain.TwainScanner)
					{
						Scanners.Twain.BookedgeSettings bookedgeSettings = ((Scanners.Twain.TwainScanner)this.primaryScanner).GetScannerSettings();
						Scanners.Twain.BookedgeSettings tmp = new Scanners.Twain.BookedgeSettings();
						tmp.CopyValuesFrom(this.ScanSettings.BookEdge);
						this.ScanSettings.BookEdge.CopyFrom(bookedgeSettings);
						this.ScanSettings.BookEdge.CopyValuesFrom(tmp);
					}
					else if (this.primaryScanner is Scanners.Click.ClickWrapper)
					{
						Scanners.Click.ClickSettings clickSettings = ((Scanners.Click.ClickWrapper)this.primaryScanner).GetScannerSettings();
						Scanners.Click.ClickSettings tmp = new Scanners.Click.ClickSettings();
						tmp.CopyValuesFrom(this.ScanSettings.Click);
						this.ScanSettings.Click.CopyFrom(clickSettings);
						this.ScanSettings.Click.CopyValuesFrom(tmp);
					}
					else if (this.primaryScanner is Scanners.Click.ClickMiniWrapper)
					{
						Scanners.Click.ClickMiniSettings clickSettings = ((Scanners.Click.ClickMiniWrapper)this.primaryScanner).GetScannerSettings();
						Scanners.Click.ClickMiniSettings tmp = new Scanners.Click.ClickMiniSettings();
						tmp.CopyValuesFrom(this.ScanSettings.ClickMini);
						this.ScanSettings.ClickMini.CopyFrom(clickSettings);
						this.ScanSettings.ClickMini.CopyValuesFrom(tmp);
					}
					else if (this.primaryScanner is Scanners.S2N.ScannerS2NWrapper)
					{
						Scanners.S2N.S2NSettings settings = ((Scanners.S2N.ScannerS2NWrapper)this.primaryScanner).ScannerSettings;
						Scanners.S2N.S2NSettings tmp = new Scanners.S2N.S2NSettings();
						tmp.CopyValuesFrom(this.ScanSettings.S2N);
						this.ScanSettings.S2N.CopyFrom(settings);
						this.ScanSettings.S2N.CopyValuesFrom(tmp);
					}
					else if (this.primaryScanner is Scanners.S2N.Bookeye4Wrapper)
					{
						Scanners.S2N.S2NSettings settings = ((Scanners.S2N.Bookeye4Wrapper)this.primaryScanner).ScannerSettings;
						Scanners.S2N.S2NSettings tmp = new Scanners.S2N.S2NSettings();
						tmp.CopyValuesFrom(this.ScanSettings.S2N);
						this.ScanSettings.S2N.CopyFrom(settings);
						this.ScanSettings.S2N.CopyValuesFrom(tmp);
					}

					if (PrimaryScannerChanged != null)
						PrimaryScannerChanged();
				}
			}
		}
		#endregion

		#region SecondaryScanner
		public Scanners.IScanner SecondaryScanner
		{
			get { return this.secondaryScanner; }
			set
			{
				if (this.secondaryScanner != value)
				{
					this.secondaryScanner = value;

					if (this.secondaryScanner is Scanners.Twain.AdfScanner)
					{
						Scanners.Twain.AdfSettings adfSettings = ((Scanners.Twain.AdfScanner)this.secondaryScanner).GetScannerSettings();
						Scanners.Twain.AdfSettings tmp = new Scanners.Twain.AdfSettings();
						tmp.CopyValuesFrom(this.ScanSettings.Adf);
						this.ScanSettings.Adf.CopyFrom(adfSettings);
						this.ScanSettings.Adf.CopyValuesFrom(tmp);
					}

					if (SecondaryScannerChanged != null)
						SecondaryScannerChanged();
				}
			}
		}
		#endregion

		#region SelectedScanner
		public Scanners.IScanner SelectedScanner
		{
			get { return (this.MultiScannerMode && this.IsPrimaryScannerSelected == false) ? secondaryScanner : primaryScanner; }
		}
		#endregion

		#endregion

	}
}
