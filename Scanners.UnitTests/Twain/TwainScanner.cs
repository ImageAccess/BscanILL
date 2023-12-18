using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Scanners.UnitTests.Twain
{
	[TestFixture]
	public class TwainScanner
	{
		
		public TwainScanner()
		{
			
		}

		
		[SetUp]
		public void Init()
		{
		}


		[Test]
		public void TwainScanner_Ivina6280Brightness_NoException()
		{
			Scanners.Twain.TwainScanner scanner = null;

			try
			{
				scanner = Scanners.Twain.TwainScanner.GetInstance(this, new Scanners.MODELS.Model(MODELS.ScanerModel.iVinaFB6280E));
				scanner.Brightness = -0.4949;
			}
			finally
			{
				if (scanner != null)
					scanner.Dispose(this);
			}
		}
	}
}
