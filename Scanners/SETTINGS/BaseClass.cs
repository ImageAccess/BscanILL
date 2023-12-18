using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scanners.SETTINGS
{
	public class BaseClass : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// with or without get_ and set_
		/// </summary>
		/// <param name="propertyName"></param>
		public void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				if (propertyName != null && (propertyName.StartsWith("get_") || propertyName.StartsWith("set_")))
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName.Substring(4)));
				else
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public void RaisePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, e);
		}
	}

}
