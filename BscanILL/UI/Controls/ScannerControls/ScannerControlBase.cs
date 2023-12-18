using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace BscanILL.UI.Controls.ScannerControls
{
	public class ScannerControlBase : UserControl
	{


		#region constructor
		public ScannerControlBase()
		{
		}
		#endregion


		// PROTECTED METHODS
		#region protected methods

		protected void ScanSettings_ValueChanged()
		{
			if (this.Dispatcher.CheckAccess())
				ApplyFromSettings();
			else
				this.Dispatcher.BeginInvoke((Action)delegate() { ApplyFromSettings(); });
		}

		protected virtual void ApplyFromSettings()
		{
		}

		protected virtual void ApplyToSettings()
		{
		}

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Settings_PropertyChanged()
		void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (this.Dispatcher.CheckAccess())
				ApplyFromSettings();
			else
				this.Dispatcher.BeginInvoke((Action)delegate() { ApplyFromSettings(); });
		}
		#endregion

		#endregion

	}
}
