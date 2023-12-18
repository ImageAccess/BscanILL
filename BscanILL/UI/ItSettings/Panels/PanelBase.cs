using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;

namespace BscanILL.UI.ItSettings.Panels
{
	public class PanelBase : UserControl, INotifyPropertyChanged
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

		#region ApplySettings()
		public virtual void ApplySettings()
		{
		}
		#endregion

	}
}
