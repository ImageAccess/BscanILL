using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.Twain.Settings
{
	public delegate void SettingChangedHnd();
	
	public interface ISetting
	{
		bool IsDefined		{ get; set;  }
		bool IsReadOnly		{ get; set; }

		void Load(TwainApp.TwainScanner scanner);

		event SettingChangedHnd Changed;
	}

}
