using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Printing;

namespace BscanILL.Export.Printing
{
	public class IllPrintableArea
	{
		double x;
		double y;
		double width;
		double height;


		#region constructor
		public IllPrintableArea(System.Printing.PageImageableArea pageImageableArea)
		{
			this.x = pageImageableArea.OriginWidth / 96.0;
			this.y = pageImageableArea.OriginHeight / 96.0;
			this.width = pageImageableArea.ExtentWidth / 96.0;
			this.height = pageImageableArea.ExtentHeight / 96.0;
		}

		/// <summary>
		/// values in inches
		/// </summary>
		/// <param name="x">in inches</param>
		/// <param name="y">in inches</param>
		/// <param name="width">in inches</param>
		/// <param name="height">in inches</param>
		public IllPrintableArea(double x, double y, double width, double height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public double Xmm { get { return (Xinches * 25.4); } }
		public double Ymm { get { return (Yinches * 25.4); } }
		public double Xinches { get { return (this.y / 96); } }
		public double Yinches { get { return (this.x / 96); } }

		public double WidthInMM			{ get { return (WidthInInches * 25.4); } }
		public double HeightInMM		{ get { return (HeightInInches * 25.4); } }
		public double WidthInInches		{ get { return (this.width) ; } }
		public double HeightInInches	{ get { return (this.height) ; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region ToString()
		public override string ToString()
		{
			return string.Format("X: {0}, Y: {1}, W: {2}, H: {3}", x, y, width, height);
		}
		#endregion

		#endregion
	
	}
}
