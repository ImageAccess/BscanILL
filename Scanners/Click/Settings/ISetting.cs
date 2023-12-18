using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.Click.Settings
{
	public delegate void SettingChangedHnd();
	
	public interface ISetting
	{
		bool IsDefined		{ get; set;  }
		bool IsReadOnly		{ get; set; }

		event SettingChangedHnd Changed;
	}

}
