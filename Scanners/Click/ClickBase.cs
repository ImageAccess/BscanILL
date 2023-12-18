using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickCommon.Settings;

namespace Scanners.Click
{
	public class ClickBase
	{
		protected volatile static List<object> registeredCallers = new List<object>();

		protected Scanners.MODELS.Model scannerModel = new Scanners.MODELS.Model(Scanners.MODELS.ScanerModel.ClickV1);
		protected DeviceInfoRebel deviceInfo = null;

		protected Notifications notifications = Notifications.Instance;

		protected bool cropImages = true;
		protected bool catchingEvents = false;
		

		public ClickBase()
		{

		}


		//PUBLIC PROPERTIES
		#region public properties

		public Scanners.DeviceInfo DeviceInfo { get { return this.deviceInfo; } }
		public Scanners.MODELS.Model Model { get { return this.scannerModel; } }

		public bool CropImages { get { return this.cropImages; } set { this.cropImages = value; } }

		public bool IsActive
		{
			get { return true; }
			set { }
		}

		#endregion
	}
}
