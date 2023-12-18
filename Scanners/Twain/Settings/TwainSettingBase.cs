using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanners.Twain.Settings
{
	public class TwainSettingBase : Scanners.Twain.Settings.ISetting
	{
		bool isDefined = false;
		bool isReadOnly = false;
		
		public event SettingChangedHnd Changed;


		// PUBLIC PROPERTIES
		#region public properties

		public bool IsDefined { get { return this.isDefined; } set { if (this.isDefined != value) { this.isDefined = value; RaiseChanged(); }; } }
		public bool IsReadOnly { get { return this.isReadOnly; } set { if (this.isReadOnly != value) { this.isReadOnly = value; RaiseChanged(); }; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		public virtual void Load(TwainApp.TwainScanner scanner)
		{
		}

		#region RaiseChanged()
		internal virtual void RaiseChanged()
		{
			if (Changed != null)
				Changed();
		}
		#endregion

		#endregion
	}
}
