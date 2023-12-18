using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;

namespace BscanILL.UI.Dialogs
{
	public class DialogBase : Window, INotifyPropertyChanged
	{
		protected BscanILL.SETTINGS.Settings	_settings { get { return BscanILL.SETTINGS.Settings.Instance; } }
		
		public event PropertyChangedEventHandler PropertyChanged;


		#region RaisePropertyChanged
		/// <summary>
		/// with get_
		/// </summary>
		/// <param name="propertyName"></param>
		protected void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName.Substring(4)));
		}
		#endregion

		#region class ComboItemFileFormat
		public class ComboItemFileFormat
		{
			BscanILL.Scan.FileFormat value;

			public ComboItemFileFormat(BscanILL.Scan.FileFormat value)
			{
				this.value = value;
			}

			public BscanILL.Scan.FileFormat Value { get { return value; } }

			public override string ToString()
			{
				switch (value)
				{
					case Scan.FileFormat.Tiff: return "TIFF";
					case Scan.FileFormat.Pdf: return "PDF";
					case Scan.FileFormat.SPdf: return "Searchable PDF";
					case Scan.FileFormat.Jpeg: return "JPEG";
					case Scan.FileFormat.Png: return "PNG";
					case Scan.FileFormat.Text: return "Rich Text";
					case Scan.FileFormat.Audio: return "Audio";
				}

				return value.ToString();
			}
		}
		#endregion

		#region class ComboItemScannersFileFormat
		public class ComboItemScannersFileFormat
		{
			Scanners.FileFormat value;

			public ComboItemScannersFileFormat(Scanners.FileFormat value)
			{
				this.value = value;
			}

			public Scanners.FileFormat Value { get { return value; } }

			public override string ToString()
			{
				switch (value)
				{
					case Scanners.FileFormat.Tiff: return "TIFF";
					case Scanners.FileFormat.Jpeg: return "JPEG";
					case Scanners.FileFormat.Png: return "PNG";
				}

				return value.ToString();
			}
		}
		#endregion

	}
}
