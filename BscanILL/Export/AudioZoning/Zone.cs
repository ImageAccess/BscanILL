using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.Export.AudioZoning
{
	public class Zone
	{
		public double x = 0;
		public double y = 0;
		public double right = 1;
		public double bottom = 1;

		public delegate void ZoneChangedHnd(Zone zone);
		public event ZoneChangedHnd ZoneChanged;


		/// <summary>
		/// <0, 1>
		/// </summary>
		public Zone(double x, double y, double width, double height)
		{
			this.x = Math.Max(0, Math.Min(1, x));
			this.y = Math.Max(0, Math.Min(1, y));
			this.right = Math.Max(0, Math.Min(1, this.x + width));
			this.bottom = Math.Max(0, Math.Min(1, this.y + height));
		}

		//PUBLIC PROPERTIES
		#region public properties

		/// <summary>
		/// <0, 1>
		/// </summary>
		public double X
		{
			get { return this.x; }
			set
			{
				double tmp = this.x;
				this.x = Math.Max(0, Math.Min(right, value));

				if (this.x != tmp && ZoneChanged != null)
					ZoneChanged(this);
			}
		}

		/// <summary>
		/// <0, 1>
		/// </summary>
		public double Y
		{
			get { return this.y; }
			set
			{
				double tmp = this.y;
				this.y = Math.Max(0, Math.Min(bottom, value));

				if (this.y != tmp && ZoneChanged != null)
					ZoneChanged(this);
			}
		}

		/// <summary>
		/// <0, 1>
		/// </summary>
		public double Right
		{
			get { return this.right; }
			set
			{
				double tmp = this.right;
				this.right = Math.Max(x, Math.Min(1, value));

				if (this.right != tmp && ZoneChanged != null)
					ZoneChanged(this);
			}
		}

		/// <summary>
		/// <0, 1>
		/// </summary>
		public double Bottom
		{
			get { return this.bottom; }
			set
			{
				double tmp = this.bottom;
				this.bottom = Math.Max(y, Math.Min(1, value));

				if (this.bottom != tmp && ZoneChanged != null)
					ZoneChanged(this);
			}
		}

		/// <summary>
		/// <0, 1>
		/// </summary>
		public double Width
		{
			get { return this.right - this.x; }
		}

		/// <summary>
		/// <0, 1>
		/// </summary>
		public double Height
		{
			get { return this.bottom - this.y; }
		}

		#endregion

		//PUBLIC METHODS
		#region public methods

		#endregion

	}

}
