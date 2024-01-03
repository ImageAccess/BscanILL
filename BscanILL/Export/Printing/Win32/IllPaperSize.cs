using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Printing;

namespace BscanILL.Export.Printing.Win32
{
	class IllPaperSize : IPaperSize
	{
		PaperSize paperSize;

		double? width = null;
		double? height = null;

		//readonly string key;
		//readonly string displayName;


		#region constructor
		public IllPaperSize(PaperSize paperSize)
		{
			this.paperSize = paperSize;

			this.width = paperSize.Width * 96 / 100;
			this.height = paperSize.Height * 96 / 100;
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		//public double? Width			{ get { return (this.width != null) ? (this.width / 25.4) * 96 / 1000 : null; } }
		//public double? Height			{ get { return (this.height != null) ? (this.height / 25.4) * 96 / 1000 : null; } }
		public double? WidthInMM		{ get { return (this.WidthInInches.HasValue) ? (WidthInInches * 25.4) : (double?)null; } }
		public double? HeightInMM		{ get { return (this.HeightInInches.HasValue) ? (HeightInInches * 25.4) : (double?)null; } }
		public double? WidthInInches	{ get { return (this.paperSize.Width > 0) ? (this.paperSize.Width / 100) : (double?)null; } }
		public double? HeightInInches	{ get { return (this.paperSize.Height > 0) ? (this.paperSize.Height / 100) : (double?) null; } }

		public string			Key { get { return this.paperSize.Kind.ToString(); } }
		public string			DisplayName { get { return this.paperSize.PaperName; } }
		public PaperSize		PaperSize { get { return this.paperSize; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region GetPaperSizes()
		public static List<IPaperSize> GetPaperSizes(PrintDocument printDoc)
		{
			List<IPaperSize> paperSizes = new List<IPaperSize>();

			foreach (PaperSize paperSize in printDoc.PrinterSettings.PaperSizes)
				paperSizes.Add(new IllPaperSize(paperSize));

			return paperSizes;
		}
		#endregion

		#region ToString()
		public override string ToString()
		{
			return this.DisplayName;
		}
		#endregion

		#endregion
	
	}
}
