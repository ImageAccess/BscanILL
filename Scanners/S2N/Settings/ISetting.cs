using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.S2N.Settings
{
	public delegate void SettingChangedHnd();
	
	interface ISetting
	{
		bool IsDefined { get; }

		event SettingChangedHnd Changed;
	}

}
